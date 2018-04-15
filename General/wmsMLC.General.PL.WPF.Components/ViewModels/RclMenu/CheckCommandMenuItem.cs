namespace wmsMLC.General.PL.WPF.Components.ViewModels.RclMenu
{
    public class CheckCommandMenuItem : CommandMenuItem
    {
        #region Fields&Consts
        public const string IsCheckedPropertyName = "IsChecked";

        private bool? _isChecked;
        #endregion

        #region Properties
        public bool? IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked == value)
                    return;

                _isChecked = value;
                OnPropertyChanged(IsCheckedPropertyName);
            }
        }
        #endregion
    }
}
