using System;
using System.ComponentModel;
using wmsMLC.General.PL.Properties;

namespace wmsMLC.General.PL.WPF.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged, IViewModel, ICustomDisposable
    {
        #region .  INotifyPropertyChanged  .
        public event PropertyChangedEventHandler PropertyChanged;
        private bool _disposed;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        } 
        #endregion

        #region .  IDisposable  .
        public void Dispose()
        {
            if (!SuspendDispose)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }
        #endregion

        public bool SuspendDispose { get; set; }
    }
}