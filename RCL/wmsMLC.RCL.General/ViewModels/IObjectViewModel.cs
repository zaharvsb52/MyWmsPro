using System.Windows.Input;

namespace wmsMLC.RCL.General.ViewModels
{
    public interface IObjectViewModel
    {
        bool DoAction();
        ICommand DoActionCommand { get; }
    }
    public interface IObjectViewModel<TModel> : IObjectViewModel
    {
    }
}
