using System;
using wmsMLC.General.PL.Model;

namespace wmsMLC.General.PL.WPF.Components.ViewModels
{
    public interface IRclMenuHandler
    {
        //MenuViewModel Menu { get; set; }
        //ObservableCollection<MenuItemBase> ContextMenu { get; set; }
        Action<ValueDataField> MenuAction { get; set; }
    }
}
