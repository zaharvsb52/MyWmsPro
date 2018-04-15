using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using wmsMLC.Activities.General;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Activities.RclViewInteraction
{
    public class RclArtSubstituteActivity<T> : NativeActivity
    {
        private const string EntitiesDisplayName = @"Объект";
        private const string ArtCodeDisplayName = @"Наименование кода артикула";
        private const string OperationCodeDisplayName = @"Код операции";

        public RclArtSubstituteActivity()
        {
            DisplayName = "ТСД: Заполнение значения по ART";
        }

        #region .  Properties  .
        
        [DisplayName(EntitiesDisplayName)]
        public InOutArgument<T> Entity { get; set; }

        [DisplayName(ArtCodeDisplayName)]
        public InArgument<string> ArtCode { get; set; }

        [DisplayName(OperationCodeDisplayName)]
        public InArgument<string> OperationCode { get; set; }
        
        #endregion .  Properties  .

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, ArtCode, type.ExtractPropertyName(() => ArtCode));
            ActivityHelpers.AddCacheMetadata(collection, metadata, OperationCode, type.ExtractPropertyName(() => OperationCode));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Entity, type.ExtractPropertyName(() => Entity));

            metadata.SetArgumentsCollection(collection);

            var typeT = typeof(T);
            if (!typeof(WMSBusinessObject).IsAssignableFrom(typeT))
                metadata.AddValidationError(new System.Activities.Validation.ValidationError(string.Format("Тип '{0}' не поддерживается.", typeT)));
        }

        #region .  Methods  .

        protected override void Execute(NativeActivityContext context)
        {
            const string isnullerrorformat = "Свойство '{0}' должно быть задано.";

            var entity = Entity.Get(context);
            var obj = entity as WMSBusinessObject;
            if (obj == null)
                throw new NotImplementedException(string.Format("Тип '{0}' не поддерживается", entity.GetType()));

            var artcode = ArtCode.Get(context);
            if (string.IsNullOrEmpty(artcode))
                throw new DeveloperException(isnullerrorformat, ArtCodeDisplayName);

            var operationCode = OperationCode.Get(context);
            if (string.IsNullOrEmpty(operationCode))
                throw new DeveloperException(isnullerrorformat, OperationCodeDisplayName);
            
            Art art;
            using (var mgr = GetManager<Art>(context))
                art = mgr.Get(artcode);
            
            if (art == null)
                throw new DeveloperException(string.Format("Артикул с кодом '{0}' не существует", artcode));

            List<PMConfig> mustList;
            using (var pmConfigMgr = IoC.Instance.Resolve<IPMConfigManager>())
            {
                SetUnitOfWork(context, pmConfigMgr);
                mustList = pmConfigMgr.GetPMConfigByParamListByArtCode(art.ArtCode, operationCode, "ART_SUBSTITUTE");
            }

            var typeName = obj.GetType().Name.ToUpper();
            if (typeName == "IWBPOSINPUT")
                typeName = "IWBPOS";
            var tArt = typeof(Art);
            var props = TypeDescriptor.GetProperties(tArt);

            foreach (var item in mustList)
            {
                var prop = props.Find(item.ObjectName_r.ToUpper().Replace(typeName, "ART"), false) ??
                           props.Find(item.ObjectName_r.ToUpper(), false);
                if (prop == null) 
                    continue;
                
                obj.SetProperty(item.ObjectName_r, prop.GetValue(art));
            }

            Entity.Set(context, obj);
        }
       
        private IBaseManager<TM> GetManager<TM>(NativeActivityContext context)
        {
            var mgr = IoC.Instance.Resolve<IBaseManager<TM>>();
            SetUnitOfWork(context, mgr);
            return mgr;
        }

        private void SetUnitOfWork(NativeActivityContext context, IBaseManager mgr)
        {
            var uw = BeginTransactionActivity.GetUnitOfWork(context);
            if (uw != null)
                mgr.SetUnitOfWork(uw);
        }
    
        #endregion .  Methods  .
    }
}