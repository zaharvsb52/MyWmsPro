namespace wmsMLC.DCL.General.ViewModels.Menu
{
    public abstract class MenuItemBase : MenuViewModelBase
    {
        #region Fields&Consts
        public const string CaptionPropertyName = "Caption";
        public const string HintPropertyName = "Hint";
        public const string IsVisiblePropertyName = "IsVisible";        

        private bool _isVisible;
        private string _caption;
        private string _hint;
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

        public string Hint
        {
            get { return _hint; }
            set
            {
                if (_hint == value)
                    return;

                _hint = value;
                OnPropertyChanged(HintPropertyName);
            }
        }
        #endregion
    }
}