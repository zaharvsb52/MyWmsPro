using System;

namespace wmsMLC.General.PL.WPF.Components.Views
{
    public interface IRclUi
    {
        event EventHandler ControlKeyDown;
        bool IsMovedFocusOnNextControl { get; set; }
        void RaiseControlKeyDownEvent();
    }
}
