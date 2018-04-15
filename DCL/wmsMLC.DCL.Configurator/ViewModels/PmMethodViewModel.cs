using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.Configurator.Helpers;
using wmsMLC.General.BL;
using wmsMLC.General.PL.Properties;

namespace wmsMLC.DCL.Configurator.ViewModels
{
    public class PmMethodViewModel : INotifyPropertyChanged
    {
        private const string PmConfigByProductPropertyName = "PmConfigByProduct";
        private const string IsEnabledByProductPropertyName = "IsEnabledByProduct";
        private const string VisibilityByProductPropertyName = "VisibilityByProduct";
        private const string PmConfigByInputMaskPropertyName = "PmConfigByInputMask";
        private const string IsEnabledByInputMaskPropertyName = "IsEnabledByInputMask";
        private const string VisibilityByInputMaskPropertyName = "VisibilityByInputMask";

        private const string PmConfigByInputMassPropertyName = "PmConfigByInputMass";
        private const string IsEnabledByInputMassPropertyName = "IsEnabledByInputMass";
        private const string VisibilityByInputMassPropertyName = "VisibilityByInputMass";
        private readonly PmConfigViewModel _owner;

        public PmMethodViewModel(PmConfigViewModel owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            _owner = owner;
            PmByProductHeader = Properties.Resources.MethodDetail;
        }

        private string _pmName;
        public string PmName
        {
            get { return _pmName; }
            set
            {
                if (_pmName == value)
                    return;
                _pmName = value;
                OnPropertyChanged("PmName");
            }
        }

        private PmConfiguratorData _selectedItem;
        public PmConfiguratorData SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem == value)
                    return;
                _selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }

        private object _operationName;
        public object OperationName
        {
            get { return _operationName; }
            set
            {
                if (_operationName == value)
                    return;
                _operationName = value;
                OnPropertyChanged("OperationName");
            }
        }

        public string OperationCode { get; set; }

        private CheckedPmMethod _selectedMethod;
        public CheckedPmMethod SelectedMethod
        {
            get { return _selectedMethod; }
            set
            {
                _selectedMethod = value;
                OnPropertyChanged("SelectedMethod");
                OnPropertyChanged(PmConfigByProductPropertyName);
                OnPropertyChanged(PmConfigByInputMaskPropertyName);
                OnPropertyChanged(PmConfigByInputMassPropertyName);
                ValidatePmDetail();
                //PmByProductHeader = string.Format("{0}{1}", Properties.Resources.MethodDetail,
                //    _selectedMethod == null || _selectedMethod.Method == null
                //        ? null
                //        : string.Format(" метода '{0}'", _selectedMethod.Method.PMMETHODNAME));
            }
        }

        private string _pmByProductHeader;
        public string PmByProductHeader
        {
            get { return _pmByProductHeader; }
            set
            {
                if (_pmByProductHeader == value)
                    return;
                _pmByProductHeader = value;
                OnPropertyChanged("PmByProductHeader");
            }
        }

        private bool _isEnabledByProduct;
        public bool IsEnabledByProduct
        {
            get { return _isEnabledByProduct && IsValid == true; }
            set
            {
                if (_isEnabledByProduct == value)
                    return;
                _isEnabledByProduct = value;
                OnPropertyChanged(IsEnabledByProductPropertyName);
            }
        }

        private bool _visibilityByProduct;
        public bool VisibilityByProduct
        {
            get { return _visibilityByProduct; }
            set
            {
                if (_visibilityByProduct == value)
                    return;
                _visibilityByProduct = value;
                OnPropertyChanged(VisibilityByProductPropertyName);
            }
        }

        private bool _isEnabledByInputMask;
        public bool IsEnabledByInputMask
        {
            get { return _isEnabledByInputMask && IsValid == true; }
            set
            {
                if (_isEnabledByInputMask == value)
                    return;
                _isEnabledByInputMask = value;
                OnPropertyChanged(IsEnabledByInputMaskPropertyName);
            }
        }

        private bool _visibilityByInputMask;
        public bool VisibilityByInputMask
        {
            get { return _visibilityByInputMask; }
            set
            {
                if (_visibilityByInputMask == value)
                    return;
                _visibilityByInputMask = value;
                OnPropertyChanged(VisibilityByInputMaskPropertyName);
            }
        }

        private bool _isEnabledByInputMass;
        public bool IsEnabledByInputMass
        {
            get { return _isEnabledByInputMass && IsValid == true; }
            set
            {
                if (_isEnabledByInputMass == value)
                    return;
                _isEnabledByInputMass = value;
                OnPropertyChanged(IsEnabledByInputMassPropertyName);
            }
        }

        private bool _visibilityByInputMass;
        public bool VisibilityByInputMass
        {
            get { return _visibilityByInputMass; }
            set
            {
                if (_visibilityByInputMass == value)
                    return;
                _visibilityByInputMass = value;
                OnPropertyChanged(VisibilityByInputMassPropertyName);
            }
        }

        private List<object> _selectedMethods;
        public List<object> SelectedMethods
        {
            get { return _selectedMethods; }
            set
            {
                if (_selectedMethods == value)
                    return;
                _selectedMethods = value;
                OnPropertyChanged("SelectedMethods");
                //UpdateMethods();
            }
        }

        private bool? _isValid;
        public bool? IsValid
        {
            get { return _isValid; }
            set
            {
                if (_isValid == value)
                    return;
                _isValid = value;
                OnPropertyChanged("IsMethodsUnavailable");
                OnPropertyChanged("IsMethodsVisible");
                OnPropertyChanged("IsMethodsNotVisible");
                OnPropertyChanged(IsEnabledByProductPropertyName);
                OnPropertyChanged(IsEnabledByInputMaskPropertyName);
                OnPropertyChanged(IsEnabledByInputMassPropertyName);
            }
        }

        public bool IsMethodsUnavailable
        {
            get { return IsValid == false; }
        }

        public bool IsMethodsVisible
        {
            get { return IsValid == true; }
        }

        public bool IsMethodsNotVisible
        {
            get { return !IsValid.HasValue; }
        }

        private List<CheckedPmMethod> _itemsSource;
        public List<CheckedPmMethod> ItemsSource
        {
            get { return _itemsSource; }
            set
            {
                if (_itemsSource == value)
                    return;
                _itemsSource = value;
                OnPropertyChanged("ItemsSource");
            }
        }

        public bool PmConfigByProduct
        {
            get
            {
                var result = false;
                if (SelectedItem != null)
                {
                    if (SelectedMethod != null && SelectedMethod.Method != null)
                    {
                        var key = SelectedItem.GetPmMethodDetailsKey(operationCode: OperationCode,
                            methodCode: SelectedMethod.Method.GetKey<string>());
                        if (SelectedItem.PmMethodByProduct.ContainsKey(key))
                            result = SelectedItem.PmMethodByProduct[key];
                    }
                }

                return result;
            }
            set
            {
                if (PmConfigByProduct == value)
                    return;

                if (SelectedItem != null)
                {
                    if (SelectedMethod != null && SelectedMethod.Method != null)
                    {
                        var key = SelectedItem.GetPmMethodDetailsKey(operationCode: OperationCode,
                            methodCode: SelectedMethod.Method.GetKey<string>());
                        SelectedItem.PmMethodByProduct[key] = value;
                    }
                }
                OnPropertyChanged(PmConfigByProductPropertyName);

                if (!NotNeedRefresh)
                    SetIsDirty();
            }
        }

        public string PmConfigByInputMask
        {
            get
            {
                string result = null;
                if (SelectedItem != null)
                {
                    if (SelectedMethod != null && SelectedMethod.Method != null)
                    {
                        var key = SelectedItem.GetPmMethodDetailsKey(operationCode: OperationCode,
                            methodCode: SelectedMethod.Method.GetKey<string>());
                        if (SelectedItem.PmMethodByInputMask.ContainsKey(key))
                            result = SelectedItem.PmMethodByInputMask[key];
                    }
                }

                return result;
            }
            set
            {
                if (PmConfigByInputMask == value)
                    return;

                if (SelectedItem != null)
                {
                    if (SelectedMethod != null && SelectedMethod.Method != null)
                    {
                        var key = SelectedItem.GetPmMethodDetailsKey(operationCode: OperationCode,
                            methodCode: SelectedMethod.Method.GetKey<string>());
                        SelectedItem.PmMethodByInputMask[key] = value;
                    }
                }
                OnPropertyChanged(PmConfigByInputMaskPropertyName);

                if (!NotNeedRefresh)
                    SetIsDirty();
            }
        }

        public bool PmConfigByInputMass
        {
            get
            {
                var result = false;
                if (SelectedItem != null)
                {
                    if (SelectedMethod != null && SelectedMethod.Method != null)
                    {
                        var key = SelectedItem.GetPmMethodDetailsKey(operationCode: OperationCode,
                            methodCode: SelectedMethod.Method.GetKey<string>());
                        if (SelectedItem.PmMethodByInputMass.ContainsKey(key))
                            result = SelectedItem.PmMethodByInputMass[key];
                    }
                }

                return result;
            }
            set
            {
                if (PmConfigByInputMass == value)
                    return;

                if (SelectedItem != null)
                {
                    if (SelectedMethod != null && SelectedMethod.Method != null)
                    {
                        var key = SelectedItem.GetPmMethodDetailsKey(operationCode: OperationCode,
                            methodCode: SelectedMethod.Method.GetKey<string>());
                        SelectedItem.PmMethodByInputMass[key] = value;
                    }
                }
                OnPropertyChanged(PmConfigByInputMassPropertyName);

                if (!NotNeedRefresh)
                    SetIsDirty();
            }
        }

        public bool NotNeedRefresh { get; set; }

        public void SetSelectionMetods()
        {
            if (ItemsSource == null || SelectedMethods == null)
                return;

            foreach (var p in ItemsSource.Where(p => p.Method != null))
            {
                foreach (var s in SelectedMethods)
                {
                    if (Equals(p.Method.GetKey(), s))
                        p.IsChecked = true;
                }
            }
        }

        public void ValidatePmDetail()
        {
            IsEnabledByProduct = false;
            IsEnabledByInputMask = false;
            IsEnabledByInputMass = false;
            if (SelectedMethod != null)
            {
                if (SelectedMethod.Method != null && _owner != null)
                {
                    var methodCode = SelectedMethod.Method.GetKey<string>();
                    if (!string.IsNullOrEmpty(OperationCode)
                        && SelectedItem != null && !string.IsNullOrEmpty(SelectedItem.OjectEntityCode) &&
                        !string.IsNullOrEmpty(SelectedItem.ObjectName) && !string.IsNullOrEmpty(methodCode))
                    {
                        var key = ConfiguratorHelper.CreateAllowedDetailsPmMethodKey(operationCode: OperationCode,
                            objectEntityCode: SelectedItem.OjectEntityCode, objectName: SelectedItem.ObjectName,
                            methodCode: methodCode, property: PMConfig.PMCONFIGBYPRODUCTPropertyName);
                        VisibilityByProduct = _owner.AllowedDetailsPmMethod.ContainsKey(key) &&
                            _owner.AllowedDetailsPmMethod[key];
                        IsEnabledByProduct = VisibilityByProduct && SelectedMethod.IsChecked;

                        key = ConfiguratorHelper.CreateAllowedDetailsPmMethodKey(operationCode: OperationCode,
                            objectEntityCode: SelectedItem.OjectEntityCode, objectName: SelectedItem.ObjectName,
                            methodCode: methodCode, property: PMConfig.PMCONFIGINPUTMASKPropertyName);
                        VisibilityByInputMask = _owner.AllowedDetailsPmMethod.ContainsKey(key) &&
                            _owner.AllowedDetailsPmMethod[key];
                        IsEnabledByInputMask = VisibilityByInputMask && SelectedMethod.IsChecked;

                        key = ConfiguratorHelper.CreateAllowedDetailsPmMethodKey(operationCode: OperationCode,
                            objectEntityCode: SelectedItem.OjectEntityCode, objectName: SelectedItem.ObjectName,
                            methodCode: methodCode, property: PMConfig.PMCONFIGINPUTMASSPropertyName);
                        VisibilityByInputMass = _owner.AllowedDetailsPmMethod.ContainsKey(key) &&
                            _owner.AllowedDetailsPmMethod[key];
                        IsEnabledByInputMass = VisibilityByInputMass && SelectedMethod.IsChecked;
                    }
                }
            }
        }

        private void UpdateMethods()
        {
            if (IsValid != true || SelectedItem == null || string.IsNullOrEmpty(OperationCode))
                return;

            var methods = SelectedItem.PmMethodCodes;
            var emptyresult = new EditableBusinessObjectCollection<object>();
            emptyresult.AcceptChanges();

            if (!methods.ContainsKey(OperationCode))
                methods[OperationCode] = emptyresult;

            if (methods[OperationCode] == null)
                methods[OperationCode] = emptyresult;

            var result = methods[OperationCode];

            var id = SelectedItem.Id;
            var key = string.Format("{0}:{1}", id, OperationCode);
            if (!_owner.IsDeleted.ContainsKey(key))
            {
                _owner.IsDeleted[key] = true;
                if (!_owner.DataForDelete.ContainsKey(id))
                    _owner.DataForDelete[id] = SelectedItem.Clone();
                var oldvalue = result == null
                    ? new EditableBusinessObjectCollection<object>()
                    : new EditableBusinessObjectCollection<object>(result);
                oldvalue.AcceptChanges();
                _owner.DataForDelete[id].PmMethodCodes[OperationCode] = oldvalue;
            }

            //Показываем выбранные методы в гриде
            result.Clear();
            if (SelectedMethods != null)
            {
                foreach (var m in SelectedMethods)
                {
                    result.Add(m);
                }
                result.AcceptChanges();
            }

            if (!NotNeedRefresh)
                SetIsDirty();
        }

        private void SetIsDirty()
        {
            if (SelectedItem == null)
                return;

            SelectedItem.PmMethodCodesPropertyChanged();
            _owner.RiseCommandsCanExecute();
        }

        #region .  INotifyPropertyChanged  .
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public class CheckedPmMethod : INotifyPropertyChanged
        {
            private readonly PmMethodViewModel _owner;

            public CheckedPmMethod(PmMethodViewModel owner)
            {
                if (owner == null)
                    throw new ArgumentNullException("owner");

                _owner = owner;
            }

            private bool _isChecked;
            public bool IsChecked
            {
                get { return _isChecked; }
                set
                {
                    if (_isChecked == value)
                        return;
                    _isChecked = value;
                    OnPropertyChanged("IsChecked");

                    _owner.ValidatePmDetail();

                    if (!_owner.NotNeedRefresh)
                    {
                        _owner.SelectedMethods =
                            _owner.ItemsSource.Where(p => p.IsChecked && p.Method != null)
                                .Select(p => p.Method.GetKey())
                                .ToList();
                        _owner.UpdateMethods();
                    }
                }
            }
            public PMMethod Method { get; set; }

            #region .  INotifyPropertyChanged  .
            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged(string propertyName)
            {
                var handler = PropertyChanged;
                if (handler != null)
                    handler(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion
        }
    }
}
