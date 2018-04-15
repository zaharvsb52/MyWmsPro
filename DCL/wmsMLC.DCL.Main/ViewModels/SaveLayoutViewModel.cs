using System.Windows.Input;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Main.ViewModels
{
    [View(typeof(SaveLayoutView))]
    public class SaveLayoutViewModel : PanelViewModelBase, IObjectViewModel
    {
        public SaveLayoutViewModel()
        {
            HasViewWindow = true;
            IsSaveFormComponents = true;
            DoActionCommand = new DelegateCustomCommand(this, () => { }, OnCanOkClick);
        }

        #region . Propertieds .
        private bool _isSaveMenu;
        public bool IsSaveMenu
        {
            get { return _isSaveMenu; }
            set
            {
                if (_isSaveMenu == value)
                    return;
                _isSaveMenu = value;
                OnPropertyChanged("IsSaveMenu");
            }
        }

        private bool _isSaveFormComponents;
        public bool IsSaveFormComponents
        {
            get { return _isSaveFormComponents; }
            set
            {
                if (_isSaveFormComponents == value)
                    return;
                _isSaveFormComponents = value;
                OnPropertyChanged("IsSaveFormComponents");
            }
        }

        private bool _isSaveFormSize;
        public bool IsSaveFormSize
        {
            get { return _isSaveFormSize; }
            set
            {
                if (_isSaveFormSize == value)
                    return;
                _isSaveFormSize = value;
                OnPropertyChanged("IsSaveFormSize");
            }
        }

        private bool _isSaveFormPosition;
        public bool IsSaveFormPosition
        {
            get { return _isSaveFormPosition; }
            set
            {
                if (_isSaveFormPosition == value)
                    return;
                _isSaveFormPosition = value;
                OnPropertyChanged("IsSaveFormPosition");
            }
        }

        private bool _hasViewWindow;
        public bool HasViewWindow
        {
            get { return _hasViewWindow; }
            set
            {
                if (_hasViewWindow == value)
                    return;
                _hasViewWindow = value;
                OnPropertyChanged("HasViewWindow");
                if (!HasViewWindow)
                {
                    IsSaveFormSize = false;
                    IsSaveFormPosition = false;
                }
            }
        }

        private bool _isReadOnly;
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set
            {
                if (_isReadOnly == value)
                    return;
                _isReadOnly = value;
                OnPropertyChanged("IsReadOnly");
            }
        }

        public FormComponents Components
        {
            get
            {
                var result = FormComponents.None;
                if (IsSaveMenu)
                    result |= FormComponents.Menu;
                if (IsSaveFormComponents)
                    result |= FormComponents.Components;
                if (IsSaveFormSize)
                    result |= FormComponents.FormSize;
                if (IsSaveFormPosition)
                    result |= FormComponents.FormPosition;
                return result;
            }
        }
        #endregion . Propertieds .

        #region . Methods .

        private bool OnCanOkClick()
        {
            return !IsReadOnly && (IsSaveMenu || IsSaveFormComponents || IsSaveFormSize || IsSaveFormPosition);
        }

        #endregion . Methods .

        #region IObjectViewModel

        bool IActionHandler.DoAction()
        {
            return OnCanOkClick();
        }
        public ICommand DoActionCommand { get; private set; }

        ObjectViewModelMode IObjectViewModel.Mode
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        System.Collections.ObjectModel.ObservableCollection<wmsMLC.General.PL.Model.DataField> IObjectViewModel.GetDataFields(wmsMLC.General.PL.SettingDisplay displaySetting)
        {
            throw new System.NotImplementedException();
        }

        bool IObjectViewModel.IsAdd
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        Business.Objects.WMSBusinessObject IObjectViewModel.SourceBase
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        bool IObjectViewModel.IsVisibleMenuSaveAndContinue
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        bool IObjectViewModel.IsNeedRefresh
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        event System.EventHandler<System.Collections.Specialized.NotifyCollectionChangedEventArgs> IObjectViewModel.CollectionChanged
        {
            add { throw new System.NotImplementedException(); }
            remove { throw new System.NotImplementedException(); }
        }

        event System.EventHandler IObjectViewModel.NeedRefresh
        {
            add { throw new System.NotImplementedException(); }
            remove { throw new System.NotImplementedException(); }
        }

        void IObjectViewModel.InitializeMenus()
        {
            throw new System.NotImplementedException();
        }

        object IModelHandler.GetSource()
        {
            throw new System.NotImplementedException();
        }

        void IModelHandler.SetSource(object source)
        {
            throw new System.NotImplementedException();
        }

        void IModelHandler.RefreshData()
        {
            throw new System.NotImplementedException();
        }

        void IModelHandler.RefreshDataAsync()
        {
            throw new System.NotImplementedException();
        }

        void IModelHandler.RefreshView()
        {
            throw new System.NotImplementedException();
        }

        event System.EventHandler IModelHandler.SourceUpdateStarted
        {
            add { throw new System.NotImplementedException(); }
            remove { throw new System.NotImplementedException(); }
        }

        event System.EventHandler IModelHandler.SourceUpdateCompleted
        {
            add { throw new System.NotImplementedException(); }
            remove { throw new System.NotImplementedException(); }
        }

        event System.EventHandler IModelHandler.RefreshViewEvent
        {
            add { throw new System.NotImplementedException(); }
            remove { throw new System.NotImplementedException(); }
        }

        bool IModelHandler.IsReadEnable
        {
            get { throw new System.NotImplementedException(); }
        }

        bool IModelHandler.IsEditEnable
        {
            get { throw new System.NotImplementedException(); }
        }

        bool IModelHandler.IsNewEnable
        {
            get { throw new System.NotImplementedException(); }
        }

        bool IModelHandler.IsDelEnable
        {
            get { throw new System.NotImplementedException(); }
        }

        object IModelHandler.ParentViewModelSource
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        wmsMLC.General.PL.SettingDisplay IModelHandler.DisplaySetting
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        #endregion IObjectViewModel
    }
}
