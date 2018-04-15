using System.Activities;
using System.Activities.Expressions;
using System.Collections.Generic;
using System.Windows;
using wmsMLC.General;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.ViewModels;

namespace wmsMLC.Activities.Business.Designer
{
    public partial class RclShowDialogActivityDesigner
    {
        public RclShowDialogActivityDesigner()
        {
            InitializeComponent();
        }

        private void OnDesignButtonClick(object sender, RoutedEventArgs e)
        {
            const string fontSizePropertyName = "FontSize";
            const string layoutValuePropertyName = "LayoutValue";

            var fontSizeArg = ModelItem.Properties[fontSizePropertyName].ComputedValue as InArgument<double>;
            var fontSizeLiteral = fontSizeArg.Expression as Literal<double>;

            var dlgmodel = new RclDialogLayoutViewModel
            {
                Title = "Настройка диалога",
                DialogHeder = "Диалог",
                LayoutHeder = "Свойства",
                FontSize = fontSizeLiteral == null ? ModelItem.Properties[fontSizePropertyName].DefaultValue.To(12d) : fontSizeLiteral.Value,
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
