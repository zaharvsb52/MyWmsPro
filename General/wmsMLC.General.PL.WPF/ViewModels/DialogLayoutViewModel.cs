using System.Collections.Generic;
using System.Dynamic;
using System.Windows;
using wmsMLC.General.PL.Model;

namespace wmsMLC.General.PL.WPF.ViewModels
{
    public class DialogLayoutViewModel : ViewModelBase
    {
        #region . Properties .
        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                if (_title == value)
                    return;
                _title = value;
                OnPropertyChanged("Title");
            }
        }

        private string _dialogHeder;
        public string DialogHeder
        {
            get { return _dialogHeder; }
            set
            {
                if (_dialogHeder == value)
                    return;
                _dialogHeder = value;
                OnPropertyChanged("DialogHeder");
            }
        }

        private string _layoutHeder;
        public string LayoutHeder
        {
            get { return _layoutHeder; }
            set
            {
                if (_layoutHeder == value)
                    return;
                _layoutHeder = value;
                OnPropertyChanged("LayoutHeder");
            }
        }

        private FrameworkElement _selectedElement;
        public FrameworkElement SelectedElement
        {
            get { return _selectedElement; }
            set
            {
                if (Equals(_selectedElement, value))
                    return;
                _selectedElement = value;
                OnPropertyChanged("SelectedElement");
            }
        }

        private ExpandoObject _source;
        public ExpandoObject Source
        {
            get { return _source ?? (_source = new ExpandoObject()); }
            set
            {
                if (_source == value)
                    return;
                _source = value;
                OnPropertyChanged("Source");
            }
        }

        private string _layoutValue;
        public string LayoutValue 
        { get { return _layoutValue; }
            set
            {
                if (_layoutValue == value)
                    return;
                _layoutValue = value;
                OnPropertyChanged("LayoutValue");
            }
        }

        private List<ValueDataField> _fields;
        public List<ValueDataField> Fields
        {
            get { return _fields ?? (_fields = new List<ValueDataField>()); }
            set { _fields = value; }
        }

        public double FontSize { get; set; }
        #endregion . Properties .

        #region .  Commands  .
        #endregion .  Commands  .

        public void UpdateSource()
        {
            var source = new ExpandoObject();
            var dict = source as IDictionary<string, object>;
            foreach (var field in Fields)
            {
                dict[field.Name] = field.Value;
            }
            Source = source;
        }
    }
}
