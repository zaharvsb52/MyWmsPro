using System.Windows;
using System.Windows.Controls;
using wmsMLC.General.PL.WPF.Components.ViewModels.RclMenu;

namespace wmsMLC.General.PL.WPF.Components.Helpers
{
    public class MenuItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CommandTemplate { get; set; }
        public DataTemplate SeparatorTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is SeparatorMenuItem) 
                return SeparatorTemplate;
            if (item is CommandMenuItem) 
                return CommandTemplate;

            return base.SelectTemplate(item, container);
        }
    }
}