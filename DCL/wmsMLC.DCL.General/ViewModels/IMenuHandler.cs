using wmsMLC.DCL.General.ViewModels.Menu;

namespace wmsMLC.DCL.General.ViewModels
{
    public interface IMenuHandler
    {
        MenuViewModel Menu { get; }
        MenuItemCollection ContextMenu { get; }
    }
}