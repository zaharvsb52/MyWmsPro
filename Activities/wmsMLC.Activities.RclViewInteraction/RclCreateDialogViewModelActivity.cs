using System.Activities;
using System.Activities.Presentation.PropertyEditing;
using System.Collections.ObjectModel;
using System.ComponentModel;
using wmsMLC.Activities.Business.Designer;
using wmsMLC.Activities.Dialogs.Models;
using wmsMLC.Activities.Dialogs.Views.Editors;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.General;
using wmsMLC.General.PL.Model;

namespace wmsMLC.Activities.RclViewInteraction
{
    [Designer(typeof(RclShowDialogActivityDesigner))]
    public class RclCreateDialogViewModelActivity : NativeActivity<DialogModel>
    {
        private Collection<ValueDataField> _fields;

        [DisplayName(@"Заголовок диалога")]
        public InArgument<string> Header { get; set; }

        [DisplayName(@"Описание диалога")]
        public InArgument<string> Description { get; set; }

        [DisplayName(@"Размер шрифта")]
        [DefaultValue(14)]
        public InArgument<double> FontSize { get; set; }

        [DisplayName(@"Поля для отображения")]
        [Editor(typeof(CollectionPropertyEditor), typeof(DialogPropertyValueEditor))]
        public Collection<ValueDataField> Fields
        {
            get { return _fields ?? (_fields = new Collection<ValueDataField>()); }
            set { _fields = value; }
        }

        [DisplayName(@"Настройка (Layout)")]
        public OutArgument<string> Layout { get; set; }

        [Browsable(false)]
        public string LayoutValue { get; set; }

        public RclCreateDialogViewModelActivity()
        {
            DisplayName = "ТСД: Создать модель диалога";
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, Header, type.ExtractPropertyName(() => Header));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Description, type.ExtractPropertyName(() => Description));
            ActivityHelpers.AddCacheMetadata(collection, metadata, FontSize, type.ExtractPropertyName(() => FontSize));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Layout, type.ExtractPropertyName(() => Layout));

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            //var test = WorkflowInspectionServices.GetActivities(this);
            var result = new DialogModel
                {
                    Header = Header.Get(context),
                    Description = Description.Get(context),
                    FontSize = FontSize.Get(context)
                };
            foreach (var field in Fields)
            {
                field.FieldName = field.Name;
                field.SourceName = field.Name;
            }

            ActivityHelpers.SetWorkFlowPropertyValue(context, Fields);

            result.Fields.AddRange(Fields);
            context.SetValue(Result, result);
            Layout.Set(context, LayoutValue);
        }
    }
}
