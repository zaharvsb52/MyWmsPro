using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.ViewModels;

namespace wmsMLC.DCL.General.ViewModels
{
    public class CustomObjectListViewModelBase<TModel> : EditViewModelBase<ObservableCollection<TModel>>, ICustomListViewModel<TModel>
    {
        #region .  Fields && Consts  .

        private ObservableCollection<DataField> _fields;
        private ObservableCollection<TModel> _selectedItems;

        private bool _allowMultipleSelect;
        private bool _showColumnHeaders;
        private bool _showDetail;
        private bool _allowEditing;

        #endregion

        public CustomObjectListViewModelBase()
        {
            ShowColumnHeaders = true;
            ShowDetail = false;

            SelectedItems = new ObservableCollection<TModel>();

            NewCommand = new DelegateCustomCommand(New, CanNew);
            DeleteCommand = new DelegateCustomCommand(Delete, CanDelete);
            EditCommand = new DelegateCustomCommand(Edit, CanEdit);
            Commands.AddRange(new[] { NewCommand, DeleteCommand, EditCommand });
        }

        #region . Properties .
        [Dependency]
        public IViewService ViewService { get; set; }

        public bool AllowEditing {
            get
            {
                return _allowEditing;
            }
            set
            {
                _allowEditing = value;
                OnPropertyChanged("AllowEditing");
            }
        }

        public ICommand NewCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand EditCommand { get; private set; }

        public ObservableCollection<DataField> Fields
        {
            get { return _fields ?? (_fields = GetDataFields()); }
        }

        public ObservableCollection<TModel> SelectedItems
        {
            get { return _selectedItems; }
            set
            {
                if (_selectedItems == value)
                    return;

                if (_selectedItems != null)
                    _selectedItems.CollectionChanged -= SelectedItemsCollectionChanged;

                _selectedItems = value;

                if (_selectedItems != null)
                    _selectedItems.CollectionChanged += SelectedItemsCollectionChanged;

                OnPropertyChanged("SelectedItems");
            }
        }

        public IObjectViewModel<TModel> ObjectViewModel { get; set; }
        public virtual void InitializeMenus()
        {
            InitializeCustomizationBar();
            InitializeContextMenu();
            CreateMainMenu();
        }

        public bool AllowMultipleSelect
        {
            get { return _allowMultipleSelect; }
            set
            {
                if (_allowMultipleSelect == value)
                    return;

                _allowMultipleSelect = value;
                OnPropertyChanged("AllowMultipleSelect");
            }
        }

        /// <summary>
        /// Признак необходимости отображать заголовки в списках.
        /// </summary>
        public bool ShowColumnHeaders
        {
            get { return _showColumnHeaders; }
            set
            {
                _showColumnHeaders = value;
                OnPropertyChanged("ShowColumnHeaders");
            }
        }

        /// <summary>
        /// Признак необходимости отображать detail.
        /// </summary>
        public bool ShowDetail
        {
            get { return _showDetail; }
            set
            {
                _showDetail = value;
                OnPropertyChanged("ShowDetail");
            }
        }

        public bool TotalRowItemFilteredSymbolIsVisible { get; protected set; }

        public string TotalRowItemAdditionalInfo { get; protected set; }

        #endregion

        #region . Methods .

        protected virtual void CreateMainMenu()
        {
            var bar = Menu.GetOrCreateBarItem(StringResources.Commands, 1);
            bar.MenuItems.Add(new SeparatorMenuItem());

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.New,
                Command = NewCommand,
                ImageSmall = ImageResources.DCLAddNew16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLAddNew32.GetBitmapImage(),
                HotKey = new KeyGesture(Key.F7),
                //GlyphSize = GlyphSizeType.Large,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 110
            });

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.Delete,
                Command = DeleteCommand,
                ImageSmall = ImageResources.DCLDelete16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLDelete32.GetBitmapImage(),
                HotKey = new KeyGesture(Key.F9),
                //GlyphSize = GlyphSizeType.Large,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 120
            });

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.Edit,
                Command = EditCommand,
                HotKey = new KeyGesture(Key.F6),
                ImageSmall = ImageResources.DCLEdit16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLEdit32.GetBitmapImage(),
                //GlyphSize = GlyphSizeType.Large,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 111
            });
        }

        protected virtual ObservableCollection<DataField> GetDataFields()
        {
            return DataFieldHelper.Instance.GetDataFields(typeof(TModel), SettingDisplay.List);
        }

        protected virtual void SelectedItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RiseCommandsCanExecuteChanged();
        }

        protected virtual bool CanEdit()
        {
            return HasSelectedItems();
        }

        protected virtual async void Edit()
        {
            if (!ConnectionManager.Instance.AllowRequest())
                return;

            if (!CanEdit())
                return;

            try
            {
                WaitStart();

                if (SelectedItems.Count == 0)
                    return;

                if (SelectedItems.Count == 1)
                {
                    await Show(SelectedItems[0]);
                }
                else
                {
                    await Show(SelectedItems);
                }
            }
            catch (Exception ex)
            {
                if (!ExceptionHandler(ex, ExceptionResources.ItemCantCreate))
                    throw;
            }
            finally
            {
                WaitStop();
            }
        }

        protected virtual bool CanDelete()
        {
            return HasSelectedItems();
        }

        protected virtual void Delete()
        {
            try
            {
                WaitStart();

                if (!ConnectionManager.Instance.AllowRequest())
                    return;

                if (!DeleteConfirmation()) return;

                // удаляем запись
                var itemsToDelete = SelectedItems.ToArray();
                foreach (var itemToDelete in itemsToDelete)
                    Source.Remove(itemToDelete);
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

        protected virtual bool DeleteConfirmation()
        {
            var vs = GetViewService();
            var dr = vs.ShowDialog(StringResources.Confirmation
                , string.Format(StringResources.ConfirmationDeleteRecords, SelectedItems.Count)
                , MessageBoxButton.YesNo //MessageBoxButton.YesNoCancel
                , MessageBoxImage.Question
                , MessageBoxResult.Yes);

            return dr == MessageBoxResult.Yes;
        }

        protected virtual bool CanNew()
        {
            return true;
        }

        protected virtual async void New()
        {
            try
            {
                WaitStart();

                if (!ConnectionManager.Instance.AllowRequest())
                    return;

                var newItem = CreateNewItem();
                var result = await Show(newItem);
                if (result)
                    Source.Add(newItem);
                var editable = Source as IEditable;
                if (editable != null)
                    editable.AcceptChanges();
            }
            catch (Exception ex)
            {
                if (!ExceptionHandler(ex, ExceptionResources.ItemCantCreate))
                    throw;
            }
            finally
            {
                WaitStop();
            }
        }

        protected virtual TModel CreateNewItem()
        {
            return Activator.CreateInstance<TModel>();
        }

        protected bool HasSelectedItems()
        {
            if (SelectedItems == null)
                return false;

            return SelectedItems.Count > 0;
        }

        protected virtual async Task<bool> Show(TModel model)
        {            
            var vm = await WrappModelIntoVM(model);
            return ViewService.ShowDialogWindow(vm, true, true) == true;            
        }

        protected virtual async Task<bool> Show(IEnumerable<TModel> model)
        {
            var vm = await WrappModelIntoVM(model);
            return ViewService.ShowDialogWindow(vm, true, true) == true;            
        }

        protected virtual async Task<IViewModel> WrappModelIntoVM(TModel model)
        {
            return await Task.Factory.StartNew(() =>
            {
                if (ObjectViewModel == null)
                    throw new DeveloperException("IObjectViewModel is not set");
                ObjectViewModel.SetSource(model);
                return ObjectViewModel;
            });
        }

        protected virtual async Task<IViewModel> WrappModelIntoVM(IEnumerable<TModel> model)
        {
            return await Task.Factory.StartNew(() =>
            {
                if (ObjectViewModel == null)
                    throw new DeveloperException("IObjectViewModel is not set");
                var pem = ObjectViewModel as IPropertyEditHandler;
                if (pem == null)
                    throw new DeveloperException("Модель не реализует IPropertyEditHandler");
                pem.SetSource(model);
                return ObjectViewModel;
            });
        }

        protected override void OnSourceChanged()
        {
            //HACK: т.к. у нас кастомный объект и заполнен произвольно, скажем что он без изменений
            foreach (var item in Source)
            {
                var editable = item as IEditable;
                if (editable != null)
                    editable.AcceptChanges(true);
            }
            base.OnSourceChanged();
        }

        protected override void Dispose(bool disposing)
        {
            if (ObjectViewModel != null)
            {
                var disposable = ObjectViewModel as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
