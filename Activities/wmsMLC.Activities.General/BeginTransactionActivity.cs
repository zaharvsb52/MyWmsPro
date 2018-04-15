//TODO: уйти от использования BpContext для передачи UoW
using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.DAL;

namespace wmsMLC.Activities.General
{
    public class BeginTransactionActivity : NativeActivity
    {
        private const string UnitOfWorkInternalPropertyName = "UnitOfWorkInternal";
        private const string UseTransPropertyName = "BeginTransactionActivity";

        private static IDictionary<string, object> _properties;

        public BeginTransactionActivity()
        {
            _properties = new Dictionary<string, object>();
            IsolationLevel = IsolationLevel.Unspecified;
        }

        [DisplayName(@"Изоляционный уровень транзакции")]
        [DefaultValue(IsolationLevel.Unspecified)]
        public IsolationLevel IsolationLevel { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            var properties = ActivityHelpers.GetBpContextProperties(context, _properties);
            if (properties == null)
                throw new DeveloperException("Ошибка получения свойств для создания транзакции.");

            properties[UseTransPropertyName] = true;

            IUnitOfWork uw;
            if (properties.ContainsKey(UnitOfWorkInternalPropertyName))
            {
                uw = (IUnitOfWork)properties[UnitOfWorkInternalPropertyName];
                if (uw != null)
                    throw new DeveloperException("Невозможно открыть новую транзакцию. Предыдущая транзакция не завершена.");
            }

            var uowFactory = IoC.Instance.Resolve<IUnitOfWorkFactory>();
            uw = uowFactory.Create(false);
            
            if (IsolationLevel == IsolationLevel.Unspecified)
                uw.BeginChanges();
            else
                uw.BeginChanges(IsolationLevel);

            properties[UnitOfWorkInternalPropertyName] = uw;
        }

        public static IUnitOfWork GetUnitOfWork(NativeActivityContext context)
        {
            var properties = ActivityHelpers.GetBpContextProperties(context, _properties);
            //if (properties == null)
            //    throw new DeveloperException("Ошибка получения свойств для создания транзакции (GetUnitOfWork).");
            if (properties == null) //Убрал exception для случая отсутсвия BpContext
                return null;

            if (properties.ContainsKey(UnitOfWorkInternalPropertyName))
                return (IUnitOfWork) properties[UnitOfWorkInternalPropertyName];

            if (properties.ContainsKey(UseTransPropertyName))
                throw new DeveloperException("Не найдено свойство типа IUnitOfWork в аргументе {0}.", BpContext.BpContextArgumentName);
            return null;
        }

        public static void CommitChanges(NativeActivityContext context)
        {
            IUnitOfWork uw = null;
            try
            {
                uw = GetUnitOfWork(context);
                if (uw == null)
                    throw new DeveloperException("UnitOfWork is null.");
                uw.CommitChanges();
            }
            catch (Exception)
            {
                if (uw != null) 
                    uw.RollbackChanges();
                throw;
            }
            finally
            {
                Clear(context);
            }
        }

        public static void RollbackChanges(NativeActivityContext context)
        {
            try
            {
                var uw = GetUnitOfWork(context);
                if (uw == null)
                    return;

                uw.RollbackChanges();
            }
            finally
            {
                Clear(context);
            }
        }

        private static void Clear(NativeActivityContext context)
        {
            var properties = ActivityHelpers.GetBpContextProperties(context, _properties);

            if (properties.ContainsKey(UnitOfWorkInternalPropertyName))
            {
                var uw = (IUnitOfWork) properties[UnitOfWorkInternalPropertyName];
                if (uw != null)
                    uw.Dispose();
                properties[UnitOfWorkInternalPropertyName] = null;
            }

            if (properties.ContainsKey(UseTransPropertyName))
                properties.Remove(UseTransPropertyName);

            //throw new DeveloperException("Не найдено свойство типа IUnitOfWork в аргументе {0}.", BpContextArgumentName);
        }
    }
}
