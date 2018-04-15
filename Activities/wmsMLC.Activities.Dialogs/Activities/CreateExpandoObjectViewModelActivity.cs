using System;
using System.Activities;
using System.Activities.Presentation.PropertyEditing;
using System.Collections.ObjectModel;
using System.ComponentModel;
using wmsMLC.Activities.Dialogs.Views.Editors;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.General;
using wmsMLC.General.PL.Model;

namespace wmsMLC.Activities.Dialogs.Activities
{
    public class CreateExpandoObjectViewModelActivity : NativeActivity<ExpandoObjectViewModelBase>
    {
        private Collection<ValueDataField> _fields;

        public CreateExpandoObjectViewModelActivity()
        {
            DisplayName = @"Создать модель объекта";

            // Всегда проставляем новый Guid. Это позволит уникально определять каждую форму отображения
            LayoutSettingsFileSuffix = Guid.NewGuid().ToString();
        }

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

        [DisplayName(@"Текст настройки (Layout)")]
        public OutArgument<string> Layout { get; set; }

        [DisplayName(@"Показывать меню настройки вида")]
        [Description("Если данный флаг установлен, то пользователю будет выведено меню с возможностью сохранить вид")]
        public bool ShowCustomizeMenu { get; set; }

//        [DisplayName(@"Суффикс файла настроек")]
//        [Description("Уникальный суффикс, который будет добавлен к имени файла настроек при их чтении/записи")]
//        public string LayoutSettingsFileSuffix { get; set; }
//        [ReadOnly(true)]

        [DisplayName(@"Суффикс файла настроек")]
        [Description("Уникальный суффикс, который будет добавлен к имени файла настроек при их чтении/записи")]
        public string LayoutSettingsFileSuffix { get; set; }

        [Browsable(false)]
        public string LayoutValue { get; set; }

        #endregion

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, Layout, type.ExtractPropertyName(() => Layout));
            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var dataContext = context.DataContext;
            var propertyDescriptorCollection = dataContext.GetProperties();
            foreach (var field in Fields)
            {
                field.FieldName = field.Name;
                field.SourceName = field.Name;

                if (field.Value != null)
                {
                    //MEGA-HACK: получаем переменные из WF и выбираем значение нужной переменной
                    foreach (PropertyDescriptor propertyDesc in propertyDescriptorCollection)
                    {
                        if (propertyDesc.Name.Equals(field.Value))
                        {
                            field.Value = propertyDesc.GetValue(dataContext);
                            break;
                        }
                        if (propertyDesc.Name.Equals(field.LookupFilterExt))
                        {
                            var value = propertyDesc.GetValue(dataContext);
                            if (value != null)
                                field.LookupFilterExt = value.ToString();
                            break;
                        }
                    }
                }
            }

            Layout.Set(context, LayoutValue);

            var result = new ExpandoObjectViewModelBase {Fields = new ObservableCollection<ValueDataField>(Fields)};

            if (!string.IsNullOrEmpty(LayoutSettingsFileSuffix))
                result.SetSuffix(LayoutSettingsFileSuffix);

            if (ShowCustomizeMenu)
                result.IsCustomizeBarEnabled = true;

            result.SuspendDispose = true;
            context.SetValue(Result, result);
        }
    }
}
