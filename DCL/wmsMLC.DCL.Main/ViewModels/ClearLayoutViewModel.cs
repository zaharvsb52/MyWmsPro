using System.Windows.Input;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Main.ViewModels
{
    [View(typeof(ClearLayoutView))]
    public class ClearLayoutViewModel : PanelViewModelBase, IObjectViewModel
    {
        public ClearLayoutViewModel()
        {
            HasViewWindow = true;
            IsClearFormComponents = true;
            IsClearFormSize = true;
            IsClearFormPosition = true;
            DoActionCommand = new DelegateCustomCommand(this, () => { }, OnCanOkClick);
        }

        #region . Propertieds .
        private bool _isClearMenu;
        public bool IsClearMenu
        {
            get { return _isClearMenu; }
            set
            {
                if (_isClearMenu == value)
                    return;
                _isClearMenu = value;
                OnPropertyChanged("IsClearMenu");
            }
        }

        private bool _isClearFormComponents;
        public bool IsClearFormComponents
        {
            get { return _isClearFormComponents; }
            set
            {
                if (_isClearFormComponents == value)
                    return;
                _isClearFormComponents = value;
                OnPropertyChanged("IsClearFormComponents");
            }
        }

        private bool _isClearFormSize;
        public bool IsClearFormSize
        {
            get { return _isClearFormSize; }
            set
            {
                if (_isClearFormSize == value)
                    return;
                _isClearFormSize = value;
                OnPropertyChanged("IsClearFormSize");
            }
        }

        private bool _isClearFormPosition;
        public bool IsClearFormPosition
        {
            get { return _isClearFormPosition; }
            set
            {
                if (_isClearFormPosition == value)
                    return;
                _isClearFormPosition = value;
                OnPropertyChanged("IsClearFormPosition");
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
                if (IsClearMenu)
                    result |= FormComponents.Menu;
                if (IsClearFormComponents)
                    result |= FormComponents.Components;
                if (IsClearFormSize)
                    result |= FormComponents.FormSize;
                if (IsClearFormPosition)
                    result |= FormComponents.FormPosition;
                return result;
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
                    IsClearFormSize = false;
                    IsClearFormPosition = false;
                }
            }
        }

        #endregion . Propertieds .

        #region . Methods .

        private bool OnCanOkClick()
        {
            return !IsReadOnly && (IsClearMenu || IsClearFormComponents || IsClearFormSize || IsClearFormPosition);
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
