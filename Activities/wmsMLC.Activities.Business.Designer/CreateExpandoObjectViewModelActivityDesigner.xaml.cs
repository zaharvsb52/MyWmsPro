using System.Activities.Presentation.Metadata;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using wmsMLC.Activities.Dialogs.Activities;
using wmsMLC.General;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.ViewModels;

namespace wmsMLC.Activities.Business.Designer
{
    public partial class CreateExpandoObjectViewModelActivityDesigner
    {
        public CreateExpandoObjectViewModelActivityDesigner()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Регистрация атрибутов компонента.
        /// </summary>
        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            builder.AddCustomAttributes(typeof(CreateExpandoObjectViewModelActivity), new DesignerAttribute(typeof(CreateExpandoObjectViewModelActivityDesigner)));
            //builder.AddCustomAttributes(typeof(ShowCustomObjectViewModelActivity), new ToolboxBitmapAttribute(typeof(ShowCustomObjectViewModelActivity<>), "DCLDefault16.png"));
            //builder.AddCustomAttributes(typeof(ShowCustomObjectViewModelActivity), new CategoryAttribute(@"General"));
            builder.AddCustomAttributes(typeof(CreateExpandoObjectViewModelActivity), new DisplayNameAttribute(@"Показываем модель"));
            builder.AddCustomAttributes(typeof(CreateExpandoObjectViewModelActivity), new DescriptionAttribute(@"Показываем модель"));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            const string layoutValuePropertyName = "LayoutValue";

            var dlgmodel = new DialogLayoutViewModel
            {
                Title = "Настройка диалога",
                DialogHeder = "Диалог",
                LayoutHeder = "Свойства",
                LayoutValue = ModelItem.Properties[layoutValuePropertyName].ComputedValue.To<string>()
            };

            var collection = ModelItem.Properties["Fields"].Collection;
            var actualCollection = collection.GetCurrentValue() as IEnumerable<ValueDataField>;
            dlgmodel.Fields.AddRange(actualCollection);
            dlgmodel.UpdateSource();

            var viewService = IoC.Instance.Resolve<IViewService>();
            if (viewService.ShowDialogWindow(viewModel: dlgmodel, isRestoredLayout: false) == true) //Не менять isRestoredLayout на true. Проблемы с отображением формы в WorkflowDesigner'е.
            {
                using (var scope = collection.BeginEdit())
                {
                    ModelItem.Properties[layoutValuePropertyName].SetValue(dlgmodel.LayoutValue);
                    scope.Complete();
                }
            }
        }
    }
}
