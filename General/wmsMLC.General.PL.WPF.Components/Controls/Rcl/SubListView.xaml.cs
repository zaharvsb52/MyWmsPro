using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Annotations;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.General.PL.WPF.Components.Controls.Rcl
{
    public partial class SubListView : INotifyPropertyChanged
    {
        private readonly Type _itemsType;
        private readonly Type _itemType;

        public SubListView()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;

            gridcontrol.GotFocus += (s, e) =>
            {
                if (gridcontrol.RowsCount > 0 && gridcontrol.View.FocusedRowHandle < 0)
                    gridcontrol.View.FocusedRowHandle = 0;
            };
        }

        public SubListView(Type itemsType)
            : this()
        {
            if (itemsType == null) throw new ArgumentNullException("itemsType");
            _itemsType = itemsType;

            if (_itemsType.IsGenericType) _itemType = _itemsType.GetGenericArguments().FirstOrDefault();
            if (_itemType == null)
                throw new DeveloperException("Type of itemsType is not generic.");
            OnItemTypeChanged();

            //DataContextChanged += delegate { };
        }

        #region . Properties .
        public IList Source
        {
            get { return (IList)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(IList), typeof(SubListView));

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

        private ObservableCollection<DataField> _fields;
        public ObservableCollection<DataField> Fields
        {
            get { return _fields; }
            set
            {
                if (value == _fields) 
                    return;
                _fields = value;
                OnPropertyChanged("Fields");
            }
        }

        private EditableBusinessObject _currentItem;
        public EditableBusinessObject CurrentItem
        {
            get { return _currentItem; }
            set
            {
                if (_currentItem == value) 
                    return;
                _currentItem = value;
                OnPropertyChanged("CurrentItem");
            }
        }
        #endregion . Properties .

        private void OnItemTypeChanged()
        {
            var result = DataFieldHelper.Instance.GetDataFields(_itemType, SettingDisplay.LookUp);
            foreach (var field in result)
            {
                field.AllowAddNewValue = true;
            }
            Fields = result;
        }

        #region .  INotifyPropertyChanged  .
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion .  INotifyPropertyChanged  .

        public string GetSuffix()
        {
            return _itemType == null ? null : _itemType.FullName;
        }
    }
}