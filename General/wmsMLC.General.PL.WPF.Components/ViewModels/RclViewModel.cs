using wmsMLC.General.PL.WPF.ViewModels;

namespace wmsMLC.General.PL.WPF.Components.ViewModels
{
    public class RclViewModel : ViewModelBase
    {
        private bool _waitIndicatorVisible;
        public bool WaitIndicatorVisible
        {
            get { return _waitIndicatorVisible; }
            set
            {
                if (_waitIndicatorVisible == value)
                    return;

                _waitIndicatorVisible = value;
                OnPropertyChanged("WaitIndicatorVisible");
            }
        }
    }
}
