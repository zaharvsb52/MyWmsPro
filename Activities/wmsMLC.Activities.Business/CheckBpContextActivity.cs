using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using wmsMLC.Activities.General;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.Activities.Business
{
    public class CheckBpContextActivity<T> : BaseCRUDActivity<T, T[]>
    {
        #region .  Properties  .
        [RequiredArgument]
        [DisplayName(@"Контекст")]
        public InArgument<BpContext> Context { get; set; }

        [DisplayName(@"Коллекция объектов")]
        public bool IsMultipleItems { get; set; }

        [DisplayName(@"Признак того, что коллекция объектов может быть унаследована")]
        public bool IsAssignableFrom { get; set; }

        private readonly TerminateWorkflow _terminateWorkflow = new TerminateWorkflow
        {
            Reason = "Выбрано более одного объекта!"
        };
        #endregion .  Properties  .

        public CheckBpContextActivity()
        {
            DisplayName = "Проверка параметров контекста";
        }

        #region .  Methods  .
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, Context, type.ExtractPropertyName(() => Context));

            metadata.SetArgumentsCollection(collection);
            metadata.AddImplementationChild(_terminateWorkflow);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var bpCtx = Context.Get(context);
            if (bpCtx == null)
                throw new NullReferenceException("BpContext");

            if (bpCtx.Items == null || bpCtx.Items.Length < 1)
            {
                var vs = IoC.Instance.Resolve<IViewService>();
                vs.ShowDialog(bpCtx.Name, "Входной список пуст", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error,
                    System.Windows.MessageBoxResult.OK);
                context.ScheduleActivity(_terminateWorkflow);
                return;
            }

            // выбор из коллекции только заданного типа Т
            var typedItems = bpCtx.Items.Where(i => (i.GetType() == typeof(T)) || (IsAssignableFrom && i is T)).ToArray();

            if (typedItems.Length > 1 & !IsMultipleItems)
            {
                var vs = IoC.Instance.Resolve<IViewService>();
                vs.ShowDialog(bpCtx.Name, "Выбрано более одного объекта", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error,
                    System.Windows.MessageBoxResult.OK);
                context.ScheduleActivity(_terminateWorkflow);
                return;
            }

            if (typedItems.Length == 0)
            {
                Result.Set(context, new T[]{});
                return;
            }

            // если не надо обновлять список
            if (bpCtx.DoNotRefresh)
            {
                Result.Set(context, typedItems.Cast<T>().ToArray());
                return;
            }

            var manager = GetBaseManager();
            var uow = BeginTransactionActivity.GetUnitOfWork(context);
            if (uow != null)
                manager.SetUnitOfWork(uow);
            manager.ClearCache();
            var filter = FilterHelper.GetFilterIn(typeof(T), typedItems);
            var result = manager.GetFiltered(filter).ToArray();
            Result.Set(context, result);
        }
        #endregion
    }
}
