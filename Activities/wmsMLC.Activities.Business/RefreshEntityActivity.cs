using System;
using System.Activities;
using System.Activities.Presentation.PropertyEditing;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using wmsMLC.Activities.Business.Views.Editors;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.Business.General;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.Activities.Business
{
    public class RefreshEntityActivity : NativeActivity
    {
        private Collection<ValueDataField> _fields;

        public RefreshEntityActivity()
        {
            DisplayName = "Обновление менеджеров сущностей";
        }

        [DisplayName(@"Использовать")]
        [DefaultValue(true)]
        public InArgument<bool> IsEnabled { get; set; }

        [DisplayName(@"Типы сущностей")]
        [Editor(typeof(EntityTypeCollectionEditor), typeof(DialogPropertyValueEditor))]
        public Collection<ValueDataField> EntityTypes
        {
            get
            {
                return _fields ?? (_fields = new Collection<ValueDataField>());
            }
            set
            {
                _fields = value;
            }
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, IsEnabled, type.ExtractPropertyName(() => IsEnabled));

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (!IsEnabled.Get(context))
                return;

            var datacontext = context.DataContext;
            var properties = datacontext.GetProperties();

            Action<IBaseManager, NotifyCollectionChangedAction, IList> ivokeHandler = (mngInstance, chngAction, data) =>
            {
                DispatcherHelper.BeginInvoke(new Action(() =>
                {
                    mngInstance.RiseManagerChanged(chngAction, data);
                }));
            };
            
            var mto = IoC.Instance.Resolve<IManagerForObject>();
            foreach (var entity in EntityTypes)
            {
                if (string.IsNullOrEmpty(entity.Name))
                    continue;

                var type = mto.GetTypeByName(entity.Name);
                if (type == null)
                    throw new DeveloperException("Unknown source type '{0}'.", entity.Name);

                var mgrType = mto.GetManagerByTypeName(type.Name);
                if (mgrType == null)
                    throw new DeveloperException(string.Format("Unknown source type '{0}'.", type.Name));

                using (var managerInstance = IoC.Instance.Resolve(mgrType, null) as IBaseManager)
                {
                    if (managerInstance == null)
                        throw new DeveloperException(string.Format("Can't resolve IBaseManager by '{0}'.", mgrType.Name));

                    if (!string.IsNullOrEmpty(entity.Caption) && entity.Value != null)
                    {
                        var action = (RefreshAction)Enum.Parse(typeof(RefreshAction), entity.Value.ToString());
                        if (action != RefreshAction.Changed)
                        {
                            var prop = properties.Find(entity.Caption, true);
                            if (prop != null)
                            {
                                var ld = prop.GetValue(datacontext);
                                if (ld != null)
                                {
                                    var listData = ld as IList;
                                    switch (action)
                                    {
                                        case RefreshAction.InsertOrUpdate:
                                            //managerInstance.RiseManagerChanged(NotifyCollectionChangedAction.Add, listData);
                                            ivokeHandler(managerInstance, NotifyCollectionChangedAction.Add, listData);
                                            break;
                                        case RefreshAction.Remove:
                                            //managerInstance.RiseManagerChanged(NotifyCollectionChangedAction.Remove, listData);
                                            ivokeHandler(managerInstance, NotifyCollectionChangedAction.Remove, listData);
                                            break;
                                    }
                                    continue;
                                }
                            }
                        }
                    }
                    managerInstance.RiseManagerChanged();
                }
            }
        }
    }

    public enum RefreshAction
    {
        InsertOrUpdate,
        Remove,
        Changed
    }

}
