using System.ComponentModel;

namespace wmsMLC.General.PL.WPF.Views
{
    public interface IShell : IView
    {
        event CancelEventHandler Closing;
    }
}