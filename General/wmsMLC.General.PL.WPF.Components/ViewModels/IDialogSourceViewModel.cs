namespace wmsMLC.General.PL.WPF.Components.ViewModels
{
    public interface IDialogSourceViewModel
    {
        string PanelCaption { get; set; }
        double FontSize { get; set; }
    }
    public interface IDialogSourceViewModel<T> : IDialogSourceViewModel
    {
        void SetEntityFilter(string filter);
    }
}
