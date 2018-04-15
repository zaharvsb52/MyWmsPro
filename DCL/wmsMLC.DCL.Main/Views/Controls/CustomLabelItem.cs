using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Validation;

namespace wmsMLC.DCL.Main.Views.Controls
{
    public class CustomLabelItem : DevExpress.Xpf.Editors.TextEdit
    {
        private const string TextPropertyName = "Text";
        private readonly ModelView _model;

        public CustomLabelItem()
        {
            IsReadOnly = true;
            ShowBorder = false;
            Background = new SolidColorBrush(Colors.Transparent);

            SetBinding(EditValueProperty, new Binding
            {
                Path = new PropertyPath(TextPropertyName),
                Mode = BindingMode.TwoWay,
                ValidatesOnDataErrors = true,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            _model = new ModelView();
            DataContext = _model;
        }

        #region Properties
        public string Content
        {
            get { return _model.Text; }
            set { _model.Text = value; }
        }

        public IValidatable ParentData
        {
            get { return _model.ParentData; }
            set { _model.ParentData = value; }
        }

        public string PropertyName
        {
            get { return _model.PropertyName; }
            set { _model.PropertyName = value; }
        }

        public bool ShowValidationError { get; set; }
        #endregion Properties

        protected override void OnLoadedInternal()
        {
            base.OnLoadedInternal();
            if (ShowValidationError)
                _model.Initialize();
        }

        public class ModelView : ValidatableObject //ViewModelBase, IDataErrorInfo //
        {
            public void Initialize()
            {
                if (ParentData == null || string.IsNullOrEmpty(PropertyName))
                    return;

                ((INotifyPropertyChanged)ParentData).PropertyChanged -= OnPropertyChanged;
                ((INotifyPropertyChanged)ParentData).PropertyChanged += OnPropertyChanged;

                if (ParentData.Validator == null)
                    return;

                ParentData.Validator.ValidateMe -= OnValidateMe;
                ParentData.Validator.ValidateMe += OnValidateMe;
            }

            private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == PropertyName)
                    OnPropertyTextChanged();
            }

            private void OnValidateMe(ValidateEventsArgs e)
            {
                if (ComparePropertyName(e))
                    OnPropertyTextChanged();
            }

            private bool ComparePropertyName(ValidateEventsArgs arg)
            {
                if (arg == null)
                    return false;
                if (string.IsNullOrEmpty(arg.PropertyName))
                    return false;
                if (arg.PropertyName == PropertyName)
                    return true;
                //if (arg.PropertyName == PropertyName || arg.PropertyName.EqIgnoreCase("isnew")) return true;
                return ComparePropertyName(arg.InnerArgs);
            }

            private string _text;
            public string Text
            {
                get { return _text; }
                set
                {
                    if (_text == value) return;
                    _text = value;
                    OnPropertyTextChanged();
                }
            }

            public IValidatable ParentData { get; set; }
            public string PropertyName { get; set; }

            private void OnPropertyTextChanged()
            {
                OnPropertyChanged(TextPropertyName);
            }

            public override string this[string columnName]
            {
                get
                {
                    if (ParentData != null && ParentData.Validator != null && ParentData.Validator.Errors != null)
                    {
                        try
                        {
                            Validator.Errors.Clear();
                        }
                        // ReSharper disable EmptyGeneralCatchClause
                        catch { }
                        // ReSharper restore EmptyGeneralCatchClause

                        try
                        {
                            var errors = ParentData.Validator.GetAllErrors(PropertyName);
                            if (errors != null && errors.Any())
                            {
                                //Показываем только критические ошибки.
                                var errs = errors[PropertyName].Where(p => p != null && (p.Level == ValidateErrorLevel.Critical || p.Level == ValidateErrorLevel.Warning)).ToArray();
                                if (errs.Length > 0)
                                {
                                    var errorsList = Validator.Errors.GetOrCreate(TextPropertyName);
                                    errorsList.AddRange(errs);
                                }
                            }
                        }
                        // ReSharper disable EmptyGeneralCatchClause
                        catch { }
                        // ReSharper restore EmptyGeneralCatchClause
                    }
                    return base[columnName];
                }
            }
        }
    }
}
