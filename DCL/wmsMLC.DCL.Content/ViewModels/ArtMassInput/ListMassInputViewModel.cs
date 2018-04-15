using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using wmsMLC.Business.Managers.Validation;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Validation;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.DCL.Content.ViewModels.ArtMassInput
{
    public abstract class ListMassInputViewModel<T> : wmsMLC.General.PL.WPF.ViewModels.ViewModelBase, IMassInputListViewModel, IActivatable, IMenuHandler
        where T : WMSBusinessObject
    {
        private readonly bool _canHandleHotKeys;

        public event EventHandler<AddItemEventArgs> ItemAdded;

        public MenuItemCollection ContextMenu { get; protected set; }

        public DirtyCollection<T> Items { get; set; }

        private T _selectedItem;

        public T SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged("SelectedItem");
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }

        public DelegateCustomCommand NewCommand { get; private set; }
        public DelegateCustomCommand DeleteCommand { get; private set; }

        private MenuViewModel _menu;

        public MenuViewModel Menu
        {
            get
            {
                return (_menu ?? (_menu = CreateMenuViewModel()));
            }
            set
            {
                _menu = value;
                OnPropertyChanged("Menu");
            }
        }

        private readonly Lazy<ObservableCollection<DataField>> _fields;

        public ObservableCollection<DataField> Fields
        {
            get { return _fields.Value; }
        }

        protected virtual MenuViewModel CreateMenuViewModel()
        {
            return new MenuViewModel("MassInput" + typeof (T).Name) {NotUseGlobalLayoutSettings = true};
        }

        public void CreateMenu()
        {
            var barCommands = Menu.GetOrCreateBarItem(StringResources.Commands, 1);
            barCommands.MenuItems.AddRange(CreateCommandMenuItems());
        }

        protected virtual List<CommandMenuItem> CreateCommandMenuItems()
        {
            NewCommand = new DelegateCustomCommand(New);
            var miNew = new CommandMenuItem
            {
                Caption = StringResources.New,
                Command = NewCommand,
                ImageSmall = ImageResources.DCLAddNew16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLAddNew32.GetBitmapImage(),
                HotKey = CanHandleHotKeys ? new KeyGesture(Key.F7) : null,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                GlyphSize = GlyphSizeType.Small,
                Priority = 10
            };

            DeleteCommand = new DelegateCustomCommand(Delete, CanDelete);
            var miDelete = new CommandMenuItem
            {
                Caption = StringResources.Delete,
                Command = DeleteCommand,
                ImageSmall = ImageResources.DCLDelete16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLDelete32.GetBitmapImage(),
                HotKey = CanHandleHotKeys ? new KeyGesture(Key.F9) : null,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                GlyphSize = GlyphSizeType.Small,
                Priority = 20
            };

            return new List<CommandMenuItem> { miNew, miDelete };
        }

        public static IEnumerable<DataField> GetFields(Type type)
        {
            var fields = DataFieldHelper.Instance.GetDataFields(type, SettingDisplay.Detail);

            FillLookUpFields(fields, type);

            foreach (var field in fields)
            {
                field.AllowAddNewValue = true;
                field.IsEnabled = field.EnableEdit = field.EnableCreate;
            }

            return fields.ToArray();
        }

        private static void FillLookUpFields(IEnumerable<DataField> fields, Type type)
        {
            // получим поля только с лукапами
            var lookUpFields = DataFieldHelper.Instance.GetDataFields(type, SettingDisplay.Detail).Where(i => !string.IsNullOrEmpty(i.LookupCode)).ToArray();
            foreach (var f in lookUpFields)
            {
                var name = f.Name;
                var vf = LookupHelper.GetVirtualFieldName(type, f.Name);
                if (!string.IsNullOrEmpty(vf))
                    name = vf;
                var field = fields.FirstOrDefault(i => string.Equals(i.Name, name));
                if (field != null)
                {
                    field.FieldName = f.FieldName;
                    field.KeyLink = f.KeyLink;
                    field.LookupButtonEnabled = f.LookupButtonEnabled;
                    field.LookupCode = f.LookupCode;
                    field.LookupFilterExt = f.LookupFilterExt;
                    field.LookupVarFilterExt = f.LookupVarFilterExt;
                    field.SourceName = f.SourceName;
                    field.Visible = f.Visible;
                    field.EnableEdit = f.EnableEdit;
                }
            }

            foreach (var f in fields)
                f.IsEnabled = f.EnableEdit;
        }

        protected abstract List<string> ExcludedFields { get; }

        protected virtual List<string> FieldsOrder
        {
            get
            {
                return new List<string>();
            }
        }

        protected virtual int GetSortingOrder(DataField f, int currentOrder)
        {
            if (f == null)
                throw new ArgumentNullException("f");

            int idx = FieldsOrder.FindIndex(e => e.Equals(f.FieldName, StringComparison.OrdinalIgnoreCase));
            return idx < 0 ? FieldsOrder.Count + currentOrder : idx;
        }

        protected ListMassInputViewModel(bool canHandleHotKeys)
        {
            _canHandleHotKeys = canHandleHotKeys;
            Items = new DirtyCollection<T>();

            _fields = new Lazy<ObservableCollection<DataField>>(
                () => new ObservableCollection<DataField>(
                    GetFields(typeof(T))
                        .Where(f => !ExcludedFields.Any(e => e.Equals(f.FieldName, StringComparison.OrdinalIgnoreCase)))
                        .Select((f, i) => new {f, i})
                        .OrderBy(t => GetSortingOrder(t.f, t.i))
                        .Select(t => t.f)),
                LazyThreadSafetyMode.ExecutionAndPublication);

            CreateMenu();
        }

        protected IBaseManager<T> GetManager()
        {
            return IoC.Instance.Resolve<IBaseManager<T>>();
        }

        protected static bool Validate(WMSBusinessObject obj)
        {
            if (obj != null && !obj.IsPersisted())
                DefaultValueSetter.Instance.SetDefaultValues(obj);

            var valid = obj as IValidatable;
            if (valid == null)
                return true;

            valid.Validate();

            // если критических ошибок нет - можно продолжать
            if (!valid.Validator.HasCriticalError())
                return true;

            return false;
        }

        protected virtual void OnItemAdded(object item)
        {
            var ia = ItemAdded;
            if (ia != null)
            {
                ia(this, new AddItemEventArgs() { Item = item });
            }
        }

        protected virtual void SetDefaultValues(T item)
        {

        }

        protected virtual void New()
        {
            using (var mgr = GetManager())
            {
                var newInstance = mgr.New();
                SetDefaultValues(newInstance);
                Validate(newInstance);
                SetPrimaryKeyValue(newInstance);
                Items.Add(newInstance);
                OnItemAdded(newInstance);
            }
        }

        protected virtual object GetNextId()
        {
            return null;
        }

        protected virtual void SetPrimaryKeyValue(T newInstance)
        {
            var kh = newInstance as IKeyHandler;
            if (kh != null)
            {
                var pk = GetNextId();
                if (pk != null)
                    kh.SetKey(pk);
            }
        }

        protected virtual bool CanDelete()
        {
            return SelectedItem != null;
        }

        protected static IViewService GetViewService()
        {
            return wmsMLC.General.IoC.Instance.Resolve<IViewService>();
        }

        protected virtual bool ConfirmDelete()
        {
            return false;
        }

        protected virtual bool DeleteConfirmation()
        {
            var vs = GetViewService();
            var dr = vs.ShowDialog(StringResources.Confirmation
                , string.Format(StringResources.ConfirmationDeleteRecords, 1)
                , MessageBoxButton.YesNo
                , MessageBoxImage.Question
                , MessageBoxResult.Yes);

            return dr == MessageBoxResult.Yes;
        }

        protected virtual void Delete()
        {
            if (SelectedItem == null)
                return;

            if (ConfirmDelete() && !DeleteConfirmation())
                return;

            Items.Remove(SelectedItem);
        }

        public void SetSelectedItem(object item)
        {
            if (item == null)
            {
                SelectedItem = null;
                return;
            }

            var t = item as T;
            if (t == null)
                throw new DeveloperException(string.Format("Not supported type in SetSelectedItem. Expected '{0}', got '{1}'", typeof(T).Name, item.GetType().Name));
            SelectedItem = t;
        }

        public bool IsActive
        {
            get { return CanHandleHotKeys; }
            set { }
        }

        protected bool CanHandleHotKeys
        {
            get { return _canHandleHotKeys; }
        }
    }
}