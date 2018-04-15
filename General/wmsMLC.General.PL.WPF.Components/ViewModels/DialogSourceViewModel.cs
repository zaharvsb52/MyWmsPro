using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Components.ViewModels.RclMenu;

namespace wmsMLC.General.PL.WPF.Components.ViewModels
{
    public class DialogSourceViewModel : RclPanelViewModelBase, IRclMenuHandler
    {
        public DialogSourceViewModel()
        {
            MenuCommand = new DelegateCustomCommand<ValueDataField>(this, OnMenuCommand, CanMenuCommand);
            Properties = new Dictionary<string, object>();
        }

        #region . Properties .
        private List<ValueDataField> _fields;
        public List<ValueDataField> Fields
        {
            get { return _fields ?? (_fields = new List<ValueDataField>()); }
            set { _fields = value; }
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

        private List<ValueDataField> _menuItems;
        public List<ValueDataField> MenuItems
        {
            get { return _menuItems ?? (_menuItems = new List<ValueDataField>()); }
        }

        public object this[string key]
        {
            get
            {
                if (string.IsNullOrEmpty(key))
                    return null;
                var source = Source as IDictionary<string, object>;
                if (source != null && source.ContainsKey(key))
                    return source[key];
                return null;
            }
            set
            {
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException("key");

                ((IDictionary<string, object>)Source)[key] = value;
            }
        }

        private bool _isMenuVisible;
        public bool IsMenuVisible
        {
            get { return _isMenuVisible; }
            set
            {
                _isMenuVisible = value;
                foreach (var p in Menu.Bars)
                {
                    p.IsVisible = _isMenuVisible;
                }
                OnPropertyChanged(MenuPropertyName);
            }
        }

        public string MenuResult { get; private set; }

        public string MenuActionName { get; private set; }

        public virtual Action<ValueDataField> MenuAction { get; set; }

        public IDictionary<string, object> Properties { get; private set; }

        #endregion . Properties .

        #region . Commands .
        public ICommand MenuCommand { get; set; }

        private bool CanMenuCommand(ValueDataField parameter)
        {
            return !WaitIndicatorVisible && Fields != null && Fields.Count > 0;
        }

        private void OnMenuCommand(ValueDataField parameter)
        {
            MenuResult = null;
            if (!CanMenuCommand(parameter))
                return;

            var hotkey = parameter.Value.To(Key.None);
            MenuResult = hotkey == Key.None ? parameter.Value.To(string.Empty) : hotkey.ToString();
            MenuActionName = parameter.Name;
            if (MenuAction != null)
                MenuAction(parameter);
        }
        #endregion . Commands .

        #region . Methods .
        public bool ContainsKey(string key)
        {
            var source = Source as IDictionary<string, object>;
            return source != null && source.ContainsKey(key);
        }

        public void CreateMenu()
        {
            Menu.Bars.Clear();
            var bar = Menu.GetOrCreateBarItem("0");

            if (MenuItems.Count == 0)
                return;
            foreach (var item in MenuItems)
            {
                var hotkey = item.Value.To(Key.None);

                bar.MenuItems.Add(new CommandMenuItem
                {
                    Name = item.Name,
                    GlyphSize = GlyphSizeType.Small,
                    Caption = item.Caption,
                    HotKey = hotkey == Key.None ? null : new KeyGesture(hotkey),
                    Command = MenuCommand,
                    CommandParameter = hotkey == Key.None ? null : hotkey.ToString()
                });

            }
            OnPropertyChanged(MenuPropertyName);
        }

        public void UpdateSource()
        {
            var source = new ExpandoObject();
            var dict = source as IDictionary<string, object>;
            foreach (var field in Fields.Where(p => p.FieldType != null && p.FieldType != typeof(Button)))
            {
                dict[field.Name] = field.Value;
            }

            dict[ValueDataFieldConstants.Properties] = Properties;

            Source = source;
        }

        public ValueDataField GetField(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            if (ContainsKey(key))
                return Fields.FirstOrDefault(p => p.Name == key);

            return null;
        }
        #endregion . Methods .
    }

    public class DialogSourceViewModel<T> : DialogSourceViewModel, IDialogSourceViewModel<T>
    {
        public virtual void SetEntityFilter(string filter)
        {
        }
    }
}
