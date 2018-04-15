using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using wmsMLC.Activities.General;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Activities.Business
{
    public class ValidateCpvActivity : NativeActivity<bool>
    {
        public ValidateCpvActivity()
        {
            DisplayName = "Валидация пользовательских параметров сущности";
        }

        [DisplayName(@"Объект валидации")]
        [RequiredArgument, DefaultValue(null)]
        public InArgument<WMSBusinessObject> Entity { get; set; }

        [DisplayName(@"Ошибки валидации")]
        public OutArgument<string> ErrorMessage { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, Entity, type.ExtractPropertyName(() => Entity));
            ActivityHelpers.AddCacheMetadata(collection, metadata, ErrorMessage, type.ExtractPropertyName(() => ErrorMessage));

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            Result.Set(context, false);

            var entity = Entity.Get(context);
            if (entity == null)
                throw new DeveloperException("Неопределен объект валидации.");

            var entitykey = entity.GetKey();
            if (entitykey == null)
                throw new DeveloperException("Неопределен PK у сущности '{0}'.", entity);

            var entitySource = SourceNameHelper.Instance.GetSourceName(entity.GetType());
            CustomParam[] customParams;

            using (var managerCustomParam = IoC.Instance.Resolve<IBaseManager<CustomParam>>())
            {
                var uow = BeginTransactionActivity.GetUnitOfWork(context);
                if (uow != null)
                    managerCustomParam.SetUnitOfWork(uow);

                customParams = ((ICustomParamManager)managerCustomParam).GetCPByInstance(entitySource.ToUpper(), entitykey.To<string>())
                    .Where(p => p.CustomParamMustSet)
                    .ToArray();
            }

            if (customParams.Length == 0)
            {
                Result.Set(context, true);
                return;
            }

            var typeCustomParamValue = typeof(CustomParamValue);
            var errors = new List<string>();
            using (var managerCustomParamValue = IoC.Instance.Resolve<IBaseManager<CustomParamValue>>())
            {
                var uow = BeginTransactionActivity.GetUnitOfWork(context);
                if (uow != null)
                    managerCustomParamValue.SetUnitOfWork(uow);

                foreach (var customParam in customParams)
                {
                    var customParamValues = managerCustomParamValue.GetFiltered(
                        string.Format("{0} = '{1}' AND {2} = '{3}' AND {4} = '{5}'",
                            SourceNameHelper.Instance.GetPropertySourceName(typeCustomParamValue, CustomParamValue.CustomParamCodePropertyName),
                            customParam.GetKey(),
                            SourceNameHelper.Instance.GetPropertySourceName(typeCustomParamValue, CustomParamValue.CPV2EntityPropertyName),
                            entitySource,
                            SourceNameHelper.Instance.GetPropertySourceName(typeCustomParamValue, CustomParamValue.CPVKeyPropertyName),
                            entitykey),
                        GetModeEnum.Partial).ToArray();

                    if (customParamValues.Length == 0)
                    {
                        errors.Add(string.Format("У сущности '{0}' отсутствует обязательный пользовательский параметр '{1}'.",
                            entity, customParam.CustomParamName));
                        continue;
                    }

                    errors.AddRange(customParamValues.Where(p => string.IsNullOrEmpty(p.CPVValue))
                        .Select(
                            customParamValue =>
                                string.Format(
                                    "У сущности '{0}' не заполнено значение обязательного пользовательского параметра '{1}'.",
                                    entity, customParam.CustomParamName)));
                }
            }

            if (errors.Count == 0)
            {
                Result.Set(context, true);
                return;
            }

            if (ErrorMessage != null)
                ErrorMessage.Set(context, string.Join(Environment.NewLine, errors.Distinct()));
        }
    }
}