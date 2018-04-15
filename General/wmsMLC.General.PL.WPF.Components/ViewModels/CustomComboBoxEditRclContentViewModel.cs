using System.Windows.Input;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Components.Controls.Rcl;
using wmsMLC.General.PL.WPF.Components.Views;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.General.PL.WPF.Components.ViewModels
{
    [View(typeof(CustomComboBoxEditRclContent))]
    public class CustomComboBoxEditRclContentViewModel : RclListViewModelBase<CustomSelectControlBase.SelectListItem>
    {
        public CustomComboBoxEditRclContentViewModel()
        {
            SelectCommand = new DelegateCustomCommand(this, OnSelectCommand, CanSelectCommand);
        }

        private int _maxRowsOnPage;
        public int MaxRowsOnPage
        {
            get { return _maxRowsOnPage; }
            set
            {
                if (_maxRowsOnPage == value)
                    return;
                _maxRowsOnPage = value;
                OnPropertyChanged("MaxRowsOnPage");
            }
        }

        private bool _useFunctionKeys;
        public bool UseFunctionKeys
        {
            get { return _useFunctionKeys; }
            set
            {
                if (_useFunctionKeys == value)
                    return;
                _useFunctionKeys = value;
                OnPropertyChanged("UseFunctionKeys");
            }
        }

        private bool _parentKeyPreview;
        public bool ParentKeyPreview
        {
            get { return _parentKeyPreview; }
            set
            {
                if (_parentKeyPreview == value)
                    return;
                _parentKeyPreview = value;
                OnPropertyChanged("ParentKeyPreview");
            }
        }

        public ICommand SelectCommand { get; private set; }

        private bool CanSelectCommand()
        {
            return !WaitIndicatorVisible && SelectedItem != null;
        }

        private void OnSelectCommand()
        {
            if(!CanSelectCommand())
                return;

            if (MenuAction != null)
                MenuAction(ValueDataFieldConstants.CreateDefaultParameter(string.Empty));
        }
    }
}
