using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DevExpress.Xpf.Core;
using DevExpress.XtraEditors.DXErrorProvider;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.DCL.Content.Views;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.Properties;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(CalculateShippingCostsView))]
    public class CalculateShippingCostsViewModel : ExpandoObjectViewModelBase
    {
        public event EventHandler NewRowAdded;

        public CalculateShippingCostsViewModel()
        {
            GridControlSelectedItems = new BaseObservableCollection<IwbTirAccountData>();
            GridControlValidators = new Dictionary<string, KeyValuePair<ValidatorTypes, string>>();
            GridControlEditorShowMode = EditorShowMode.Default;
        }

        #region . Properties .
        public string UniqueAccountNumberValidatorMessage { get; set; }
        public string UseAllIwbValidatorMessage { get; set; }

        #region GridControl
        public Dictionary<string, KeyValuePair<ValidatorTypes, string>> GridControlValidators { get; private set; }

        private string _gridControlLabel;
        public string GridControlLabel
        {
            get { return _gridControlLabel; }
            set
            {
                if (_gridControlLabel == value)
                    return;
                _gridControlLabel = value;
                OnPropertyChanged("GridControlLabel");
            }
        }

        public bool GridControlAllowEditing { get; set; }

        private ObservableCollection<IwbTirAccountData> _gridControlSelectedItems;
        public ObservableCollection<IwbTirAccountData> GridControlSelectedItems
        {
            get { return _gridControlSelectedItems; }
            set
            {
                if (_gridControlSelectedItems == value)
                    return;

                if (_gridControlSelectedItems != null)
                    _gridControlSelectedItems.CollectionChanged -= OnSelectedItemsCollectionChanged;

                _gridControlSelectedItems = value;

                if (_gridControlSelectedItems != null)
                    _gridControlSelectedItems.CollectionChanged += OnSelectedItemsCollectionChanged;

                OnPropertyChanged("GridControlSelectedItems");
            }
        }

        private ObservableCollection<IwbTirAccountData> _gridControlSource;
        public ObservableCollection<IwbTirAccountData> GridControlSource
        {
            get { return _gridControlSource; }
            set
            {
                if (_gridControlSource == value)
                    return;
                _gridControlSource = value;
                OnPropertyChanged("GridControlSource");
            }
        }

        private ObservableCollection<DataField> _gridControlFields;
        public ObservableCollection<DataField> GridControlFields
        {
            get { return _gridControlFields ?? (_gridControlFields = new BaseObservableCollection<DataField>()); }
            set
            {
                if (_gridControlFields == value)
                    return;
                _gridControlFields = value;
                OnPropertyChanged("GridControlFields");
            }
        }

        private MenuViewModel _gridControlMenu;
        public MenuViewModel GridControlMenu
        {
            get
            {
                return (_gridControlMenu ?? (_gridControlMenu = CreateGridControlMenuViewModel()));
            }
            set
            {
                _gridControlMenu = value;
                OnPropertyChanged("GridControlMenu");
            }
        }

        public EditorShowMode GridControlEditorShowMode { get; set; }

        public bool GridControlShowTotalRow { get; set; }
        public bool TotalRowItemFilteredSymbolIsVisible { get; protected set; }
        public string TotalRowItemAdditionalInfo { get; protected set; }

        public ICustomCommand GridControlNewCommand { get; private set; }
        public ICustomCommand GridControlDeleteCommand { get; private set; }
        #endregion GridControl
        #endregion . Properties .

        #region . Methods .

        private MenuViewModel CreateGridControlMenuViewModel()
        {
            var menu = new MenuViewModel(string.Format("{0}_GridControlMenu", MenuSuffix))
            {
                NotUseGlobalLayoutSettings = true
            };

            if (GridControlNewCommand == null)
                GridControlNewCommand = new DelegateCustomCommand(this, OnGridControlNew, OnCanGridControlNew);
            if (GridControlDeleteCommand == null)
                GridControlDeleteCommand = new DelegateCustomCommand(this, OnGridControlDelete, OnCanGridControlDelete);

            var bar = new BarItem
            {
                Name = "GridControlMenuBarItemCommands",
                Caption = StringResources.Commands,
                GlyphSize = GlyphSizeType.Small
            };
            menu.Bars.Add(bar);

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.New,
                Command = GridControlNewCommand,
                ImageSmall = ImageResources.DCLAddNew16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLAddNew32.GetBitmapImage(),
                HotKey = new KeyGesture(Key.F7),
                GlyphSize = GlyphSizeType.Small,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                IsVisible = true,
                Priority = 1
            });

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.Delete,
                Command = GridControlDeleteCommand,
                ImageSmall = ImageResources.DCLDelete16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLDelete32.GetBitmapImage(),
                HotKey = new KeyGesture(Key.F9),
                GlyphSize = GlyphSizeType.Small,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                IsVisible = true,
                Priority = 3
            });

            return menu;
        }

        private bool OnCanGridControlNew()
        {
            return GridControlSource != null;
        }

        private void OnGridControlNew()
        {
            if (!OnCanGridControlNew())
                return;
          
            OnNewRowAdded();
        }

        private void OnNewRowAdded()
        {
            var handler = NewRowAdded;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private bool OnCanGridControlDelete()
        {
            return GridControlSelectedItems != null && GridControlSelectedItems.Count > 0;
        }

        private async void OnGridControlDelete()
        {
            if (!OnCanGridControlDelete())
                return;

            var vs = GetViewService();
            var dr = vs.ShowDialog(StringResources.Confirmation
                , string.Format(StringResources.ConfirmationDeleteRecords, GridControlSelectedItems.Count)
                , MessageBoxButton.YesNo
                , MessageBoxImage.Question
                , MessageBoxResult.Yes);

            if (dr != MessageBoxResult.Yes)
                return;

            try
            {
                WaitStart();
                
                await OnGridControlDeleteAsync();

                for (; GridControlSelectedItems.Count > 0;)
                {
                    GridControlSource.Remove(GridControlSelectedItems[0]);
                }
            }
            catch (Exception ex)
            {
                if (!ExceptionHandler(ex, ExceptionResources.ItemsCantDelete))
                    throw;
            }
            finally
            {
                WaitStop();
            }
        }

        private async Task OnGridControlDeleteAsync()
        {
            await Task.Factory.StartNew(() =>
            {
                var iwbids = GridControlSelectedItems.Where(p => p.IwbIds != null && p.IwbIds.Length > 0)
                    .SelectMany(p => p.IwbIds).Distinct().ToArray();
                if (iwbids.Length == 0)
                    return;

                using (var mgr = IoC.Instance.Resolve<IBPProcessManager>())
                {
                    mgr.DeleteTirCpvs(iwbids);
                }
            });
        }

        private void OnSelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Reset)
                RiseCommandsCanExecuteChanged();
        }

        protected override void RiseCommandsCanExecuteChanged()
        {
            base.RiseCommandsCanExecuteChanged();
            if (GridControlNewCommand != null)
                GridControlNewCommand.RaiseCanExecuteChanged();
            if (GridControlDeleteCommand != null)
                GridControlDeleteCommand.RaiseCanExecuteChanged();
        }

        public void SetGridControlEditorShowMode(string mode)
        {
            GridControlEditorShowMode = mode.To(EditorShowMode.Default);
        }

        public void SetValidators(bool useUniqueAccountNumberValidator, bool useAllIwbValidatator)
        {
            IwbTirAccountData.Validators.Clear();

            foreach (var pair in GridControlValidators)
            {
                switch (pair.Value.Key)
                {
                    case ValidatorTypes.IsRequired:
                        IwbTirAccountData.Validators[pair.Key] = value => IsNullValidator(pair.Value.Value, value);
                        break;
                    case ValidatorTypes.PositiveNumber:
                        IwbTirAccountData.Validators[pair.Key] = value => PositiveNumberValidator(pair.Value.Value, value);
                        break;
                }
            }

            if (useUniqueAccountNumberValidator)
            {
                IwbTirAccountData.Validators[IwbTirAccountData.UniqueAccountNumberValidate] = value =>
                {
                    //value is IwbTirAccountData
                    var item = value as IwbTirAccountData;
                    return UniqueAccountNumberValidator(
                        string.Format(UniqueAccountNumberValidatorMessage, item == null ? null : item.AccountNumber),
                            GridControlSource, item);
                };
            }

            if (useAllIwbValidatator)
            {
                IwbTirAccountData.Validators[IwbTirAccountData.UseAllIwbValidate] = value =>
                {
                    //value is IwbTirAccountData
                    var item = value as IwbTirAccountData;
                    return UseAllIwbValidator(string.Format(UseAllIwbValidatorMessage, item == null ? null : item.AccountNumber), item);
                };
            }
        }

        public override bool DoAction()
        {
            var result = base.DoAction();
            if (GridControlSource != null)
            {
                foreach (var item in GridControlSource.Where(p => p.HasErrors(IwbTirAccountData.UniqueAccountNumberValidate)).ToArray())
                {
                    item.OnPropertyChanged(IwbTirAccountData.UniqueAccountNumberValidate);
                }
            }
            return result;
        }

        private static string IsNullValidator(string message, object value)
        {
            return Equals(value, null) || Equals(value, string.Empty) 
                  ? message
                  : null;
        }

        private static string PositiveNumberValidator(string message, object value)
        {

            if (value == null)
                return message;

            var type = value.GetType().GetNonNullableType();
            if (type.IsPrimitive || type == typeof(decimal))
                return Convert.ToDouble(value) <= 0 ? message : null;

            return null;
        }

        private static string UniqueAccountNumberValidator(string message, ObservableCollection<IwbTirAccountData> source, IwbTirAccountData item)
        {
            if (source == null || item == null)
                return null;

            foreach (var p in source.Where(p => p.HasErrors(IwbTirAccountData.UniqueAccountNumberValidate) &&
                IwbTirAccountData.GetGroupKey(p) == IwbTirAccountData.GetGroupKey(item)).ToArray())
            {
                p.ClearError(IwbTirAccountData.UniqueAccountNumberValidate);
            }

            return source.Count(p => IwbTirAccountData.GetGroupKey(p) == IwbTirAccountData.GetGroupKey(item)) > 1 ? message : null;
        }

        private static string UseAllIwbValidator(string message, IwbTirAccountData item)
        {
            if (item == null)
                return null;
            return item.NotUseAllIwb ? message : null;
        }
        #endregion . Methods .
    }

    public class IwbTirAccountData : INotifyPropertyChanged, IDXDataErrorInfo
    {
        public const string AccountNumberPropertyName = "AccountNumber";
        public const string AccountAmountPropertyName = "AccountAmount";
        public const string AccountDatePropertyName = "AccountDate";
        public const string AccountCurrencyPropertyName = "AccountCurrency";
        public const string AccountCommentPropertyName = "AccountComment";
        public const string UniqueAccountNumberValidate = "UniqueAccountNumberValidate";
        public const string UseAllIwbValidate = "UseAllIwbValidate";

        private readonly Dictionary<string, bool> _propertyErrors;

        static IwbTirAccountData()
        {
            Validators = new Dictionary<string, Func<object, string>>();
        }

        public IwbTirAccountData()
        {
            _propertyErrors = new Dictionary<string, bool>();
        }

        #region . Properties .
        public static Dictionary<string, Func<object, string>> Validators { get; private set; }

        private string _accountNumber;

        /// <summary>
        /// Номер счёта.
        /// </summary>
        public string AccountNumber
        {
            get { return _accountNumber; }
            set
            {
                if (_accountNumber == value)
                    return;
                _accountNumber = value;
                OnPropertyChanged(AccountNumberPropertyName);
            }
        }

        private double _accountAmount;

        /// <summary>
        /// Сумма транспортных расходов.
        /// </summary>
        public double AccountAmount
        {
            get { return _accountAmount; }
            set
            {
                if (_accountAmount == value)
                    return;
                _accountAmount = value;
                OnPropertyChanged(AccountAmountPropertyName);
            }
        }

        private DateTime? _accountDate;

        /// <summary>
        /// Дата счёта.
        /// </summary>
        public DateTime? AccountDate
        {
            get { return _accountDate; }
            set
            {
                if (_accountDate == value)
                    return;
                _accountDate = value;
                OnPropertyChanged(AccountDatePropertyName);
            }
        }

        private string _accountCurrency;

        /// <summary>
        /// Валюта.
        /// </summary>
        public string AccountCurrency
        {
            get { return _accountCurrency; }
            set
            {
                if (_accountCurrency == value)
                    return;
                _accountCurrency = value;
                OnPropertyChanged(AccountCurrencyPropertyName);
            }
        }

        private string _accountComment;

        /// <summary>
        /// Комментарий.
        /// </summary>
        public string AccountComment
        {
            get { return _accountComment; }
            set
            {
                if (_accountComment == value)
                    return;
                _accountComment = value;
                OnPropertyChanged(AccountCommentPropertyName);
            }
        }

        public string[] IwbIds { get; set; }
        public bool NotUseAllIwb { get; set; }
        #endregion . Properties .

        #region .  INotifyPropertyChanged  .
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        internal void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion .  INotifyPropertyChanged  .

        #region IDXDataErrorInfo

        void IDXDataErrorInfo.GetPropertyError(string propertyName, ErrorInfo info)
        {
            SetErrorInfo(info, null, ErrorType.None);
            _propertyErrors[propertyName] = false;
            if (Validators.ContainsKey(propertyName))
            {
                var handler = Validators[propertyName];
                if (handler != null)
                {
                    var properties = TypeDescriptor.GetProperties(this);
                    var prop = properties.Find(propertyName, true);
                    if (prop != null)
                    {
                        var value = prop.GetValue(this);
                        var message = handler(value);
                        if (!string.IsNullOrEmpty(message))
                        {
                            SetErrorInfo(info, message, ErrorType.Critical);
                            _propertyErrors[propertyName] = true;
                        }
                    }
                }
            }
        }

        void IDXDataErrorInfo.GetError(ErrorInfo info)
        {
            SetErrorInfo(info, null, ErrorType.None);

            var key = UniqueAccountNumberValidate;
            if (Validators.ContainsKey(key))
            {
                _propertyErrors[key] = false;
                var handler = Validators[key];
                if (handler != null)
                {
                    var message = handler(this);
                    if (!string.IsNullOrEmpty(message))
                    {
                        SetErrorInfo(info, message, ErrorType.Critical);
                        _propertyErrors[key] = true;
                        return;
                    }
                }
            }

            key = UseAllIwbValidate;
            if (Validators.ContainsKey(key))
            {
                _propertyErrors[key] = false;
                var handler = Validators[key];
                if (handler != null)
                {
                    var message = handler(this);
                    if (!string.IsNullOrEmpty(message))
                    {
                        SetErrorInfo(info, message, ErrorType.Information);
                        _propertyErrors[key] = true;
                    }
                }
            }
        }

        private void SetErrorInfo(ErrorInfo info, string errorText, ErrorType errorType)
        {
            info.ErrorText = errorText;
            info.ErrorType = errorType;
        }
        #endregion IDXDataErrorInfo

        public static string GetGroupKey(IwbTirAccountData item)
        {

            return string.Format("{0}_{1}_{2}", item.AccountNumber == null ? null : item.AccountNumber.Trim().ToUpper(),
                item.AccountDate, item.AccountCurrency);
        }

        public void ClearError(string propertyname)
        {
            if (string.IsNullOrEmpty(propertyname))
                return;
            if (_propertyErrors.ContainsKey(propertyname))
                _propertyErrors[propertyname] = false;
        }

        public bool HasErrors(string propertyname)
        {
            if (_propertyErrors == null)
                return false;

            if (string.IsNullOrEmpty(propertyname))
                return HasErrors();

            return _propertyErrors.Any(p => p.Key == propertyname && p.Value);
        }

        public bool HasErrors()
        {
            if (_propertyErrors == null)
                return false;
            return _propertyErrors.Any(p => p.Value);
        }
    }

    public enum ValidatorTypes
    {
        None,
        IsRequired,
        PositiveNumber // != null && > 0
    }
}
