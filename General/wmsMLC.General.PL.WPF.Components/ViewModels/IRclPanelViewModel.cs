using System;
using wmsMLC.General.PL.WPF.ViewModels;

namespace wmsMLC.General.PL.WPF.Components.ViewModels
{
    public interface IRclPanelViewModel : IPanelViewModel
    {
        double FontSize { get; set; }
        string LayoutValue { get; set; }
        bool IsWfDesignMode { get; set; }
        void SetItemsSource(object source);

        event EventHandler LayoutViewSaved;
        void GetLayoutFromView();
    }
}
