using System;

namespace wmsMLC.General.PL.WPF.ViewModels
{
    public interface ICustomDisposable : IDisposable
    {
        bool SuspendDispose { get; set; }
    }
}
