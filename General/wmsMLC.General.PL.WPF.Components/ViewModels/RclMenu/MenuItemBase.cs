namespace wmsMLC.General.PL.WPF.Components.ViewModels.RclMenu
{
    public abstract class MenuItemBase : MenuViewModelBase
    {
        #region Fields&Consts
        public const string CaptionPropertyName = "Caption";
        public const string IsVisiblePropertyName = "IsVisible";
        public const string IsEnablePropertyName = "IsEnable";

        private bool _isVisible;
        private bool _isEnable;
        private string _caption;
        #endregion

        protected MenuItemBase()
        {
            IsVisible = true;
            IsEnable = true;
        }

        #region Properties
        public string Caption
        {
            get { return _caption; }
            set
            {
                if (_caption == value)
                    return;

                _caption = value;
                OnPropertyChanged(CaptionPropertyName);
            }
        }

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible == value)
                    return;

                _isVisible = value;
                OnPropertyChanged(IsVisiblePropertyName);
            }
        }

        public bool IsEnable
        {
            get { return _isEnable; }
            set
            {
                if (_isEnable == value)
                    return;

                _isEnable = value;
                OnPropertyChanged(IsEnablePropertyName);
            }
        }
        #endregion
    }
}