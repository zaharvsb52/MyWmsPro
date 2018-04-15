using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Validation;

namespace wmsMLC.General.PL.WPF.Components.Controls.Rcl
{
    public class CustomLabelItem : DevExpress.Xpf.Editors.TextEdit
    {
        private const string TextPropertyName = "Text";
        private CuctomMv _model;

        public CustomLabelItem()
        {
            IsReadOnly = true;
            ShowBorder = false;
            Background = new SolidColorBrush(Colors.Transparent);

            SetBinding(EditValueProperty, new Binding {
                Path = new PropertyPath(TextPropertyName),
                Mode = BindingMode.TwoWay,
                ValidatesOnDataErrors = true,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

            _model = new CuctomMv();
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
        #endregion Properties

        protected override void OnLoadedInternal()
        {
            base.OnLoadedInternal();
            _model.Initialize();
        }

        public class CuctomMv : ValidatableObject //ViewModelBase, IDataErrorInfo //
        {
            public void Initialize()
            {
                if (ParentData == null || string.IsNullOrEmpty(PropertyName)) return;
                ((INotifyPropertyChanged)ParentData).PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == PropertyName) OnPropertyTextChanged();
                    };

                //var propdsc = TypeDescriptor.GetProperties(ParentData.GetType());
                //var dataprop = propdsc.Find(PropertyName, true);
                //if (dataprop == null) return;
                //var dataobj = dataprop.GetValue(ParentData);
                //if (dataobj == null) dataprop.SetValue(ParentData, Activator.CreateInstance(dataprop.PropertyType));

                if (ParentData.Validator == null) return;
                ParentData.Validator.ValidateMe += args =>
                    {
                        if (ComparePropertyName(args)) OnPropertyTextChanged();
                    };
            }

            private bool ComparePropertyName(ValidateEventsArgs arg)
            {
                if (arg == null) return false;
                if (string.IsNullOrEmpty(arg.PropertyName)) return false;
                if (arg.PropertyName == PropertyName) return true;
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
                            if (ParentData.Validator.Errors[PropertyName] != null && ParentData.Validator.Errors[PropertyName].Count > 0)
                            {
                                var errorsList = Validator.Errors.GetOrCreate(TextPropertyName);
                                foreach (var e in ParentData.Validator.Errors[PropertyName])
                                {
                                    errorsList.Add(e);
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
