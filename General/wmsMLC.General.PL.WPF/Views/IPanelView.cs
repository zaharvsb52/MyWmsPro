namespace wmsMLC.General.PL.WPF.Views
{
    public interface IPanelView : IView, IClosable
    {
        string PanelCaption { get; set; }
        //void SetStartLoadTime();
    }
}