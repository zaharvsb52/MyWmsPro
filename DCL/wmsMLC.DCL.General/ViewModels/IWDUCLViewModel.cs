using System;

namespace wmsMLC.DCL.General.ViewModels
{
    public interface IWDUCLViewModel
    {
        event EventHandler CanClose;
        void SetSource(object source);
        bool GetIsReadOnly();
        void SetIsReadOnly(bool isReadOnly);
    }
}
