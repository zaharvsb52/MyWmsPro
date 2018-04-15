using System.Windows;
using System.Windows.Controls;
using wmsMLC.DCL.General.ViewModels.Menu;

namespace wmsMLC.DCL.Main.Helpers
{
    public class MenuItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CommandTemplate { get; set; }
        public DataTemplate SeparatorTemplate { get; set; }
        public DataTemplate ListTemplate { get; set; }
        public DataTemplate SubListMenuTemplate { get; set; }
        public DataTemplate CheckCommandTemplate { get; set; }
        public DataTemplate EditCommandTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            //Должно быть выше, чем  if (item is CommandMenuItem) return CommandTemplate;!!!!!!!!!!
            if (item is CheckCommandMenuItem) 
                return CheckCommandTemplate; 
            if (item is EditMenuItem) 
                return EditCommandTemplate;
            if (item is SeparatorMenuItem) 
                return SeparatorTemplate;
            if (item is SubListMenuItem) 
                return SubListMenuTemplate;
            if (item is ListMenuItem) 
                return ListTemplate;
            if (item is CommandMenuItem) 
                return CommandTemplate;
            return base.SelectTemplate(item, container);
        }
    }
}