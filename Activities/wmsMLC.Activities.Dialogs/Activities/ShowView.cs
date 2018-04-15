using System.Activities;
using System.Activities.Presentation.PropertyEditing;
using System.Collections.ObjectModel;
using System.ComponentModel;
using wmsMLC.Activities.Dialogs.Views.Editors;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.General;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.Activities.Dialogs.Activities
{
    public class ShowView : NativeActivity<bool>
    {
        private Collection<ValueDataField> _fields;

        #region .  Properties  .
        [Editor(typeof(CustomCollectionPropertyEditor), typeof(DialogPropertyValueEditor))]
        [DisplayName(@"Поля для отображения")]
        public Collection<ValueDataField> Fields
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

        [DisplayName(@"Заголовок")]
        public InArgument<string> Title { get; set; }

        [DisplayName(@"Настройка отображения диалога")]
        public InArgument<string> LayoutSettings { get; set; }
        #endregion

        protected override void Execute(NativeActivityContext context)
        {
            var dataContext = context.DataContext;
            var dataContextProperties = dataContext.GetProperties();

            //MEGA-HACK: получаем переменные из WF и выбираем значение нужной переменной
            foreach (var field in Fields)
            {
                field.FieldName = field.Name;
                field.SourceName = field.Name;

                if (field.Value == null)
                    continue;

                foreach (PropertyDescriptor propertyDesc in dataContextProperties)
                {
                    if (propertyDesc.Name.Equals(field.Value))
                    {
                        field.Value = propertyDesc.GetValue(dataContext);
                        break;
                    }
                }
            }

            var model = new ExpandoObjectViewModelBase();
            model.Fields = new ObservableCollection<ValueDataField>(Fields);
            model.PanelCaption = Title.Get(context);
            var viewService = IoC.Instance.Resolve<IViewService>();
            var dr = viewService.ShowDialogWindow(model, true) == true;
            context.SetValue(Result, dr);
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, Title, type.ExtractPropertyName(() => Title));

            metadata.SetArgumentsCollection(collection);
        }
    }
}