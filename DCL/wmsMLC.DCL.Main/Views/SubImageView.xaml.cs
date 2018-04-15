using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using DevExpress.Xpf.Editors;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.General.Views;
using wmsMLC.DCL.Main.Views.Controls;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.PL.Properties;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.DCL.Main.Views
{
    public partial class SubImageView : INotifyPropertyChanged, ISubView
    {
        private double _maxHeight;
        public SubImageView()
        {
            _maxHeight = 0.45 * SystemParameters.PrimaryScreenHeight;
            InitializeComponent();

            //ImageEditControl.MaxHeight = 0.45 * SystemParameters.PrimaryScreenHeight;
            //ImageEditControl.MaxWidth = 0.7 * SystemParameters.PrimaryScreenHeight;

            NewImageCommand = new DelegateCustomCommand(this, OnNewImage, CanNewImage);
            DeleteImageCommand = new DelegateCustomCommand(this, OnDeleteImage, CanDeleteImage);

            InitializeMenus();

            LayoutRoot.DataContext = this;
        }

        #region . Properties .
        public object Source
        {
            get { return GetValue(SourcePropertyInternal); }
            set { SetValue(SourcePropertyInternal, value); }
        }
        private static readonly DependencyProperty SourcePropertyInternal = DependencyProperty.Register("Source", typeof(object), typeof(SubImageView), new PropertyMetadata(OnSourcePropertyChanged));

        private bool _isReadOnly;
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set
            {
                if (_isReadOnly == value) return;
                _isReadOnly = value;
                OnPropertyChanged("IsReadOnly");
            }
        }

        bool ISubView.ShouldUpdateSeparately { get; set; }
        DependencyProperty ISubView.SourceProperty
        {
            get { return SourcePropertyInternal; }
        }

        private bool _showMenu;
        public bool ShowMenu
        {
            get { return _showMenu; } 
            set
            {
                if (_showMenu == value)
                    return;
                _showMenu = value;
                OnPropertyChanged("ShowMenu");
            }
        }

        private MenuViewModel _menu;
        public MenuViewModel Menu
        {
            get
            {
                return (_menu ??
                        (_menu =
                            new MenuViewModel(GetType().GetFullNameWithoutVersion()) { NotUseGlobalLayoutSettings = true }));
            }
            set
            {
                _menu = value;
                OnPropertyChanged("Menu");
            }
        }

        public IModelHandler ParentViewModel { get; set; }

        public ICommand NewImageCommand { get; private set; }
        public ICommand DeleteImageCommand { get; private set; }
        #endregion . Properties .

        #region .  Methods  .
        private static void OnSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SubImageView) d).OnPropertyChanged(string.Empty);
        }

        private bool IsNotFormulaMode()
        {
            if (ParentViewModel == null)
                return false;

            var vmf = ParentViewModel as IFormulaHandler;
            if (vmf == null)
                return true;

            return !vmf.InFormulaMode;
        }

        private bool CanNewImage()
        {
            var result = !IsReadOnly && IsNotFormulaMode();
            ImageEditControl.CanLoadExecute = ImageEditControl.CanPasteExecute = result;
            return result;
        }

        private void OnNewImage()
        {
            if(!CanNewImage())
                return;

            ImageEditControl.Load();
        }

        private bool CanDeleteImage()
        {
            var result = !IsReadOnly && Source is byte[] && IsNotFormulaMode();
            ImageEditControl.CanCutExecute = ImageEditControl.CanDeleteExecute = result;
            return result;
        }

        private void OnDeleteImage()
        {
            if (!CanDeleteImage())
                return;

            if (GetViewService().ShowDialog(StringResources.Confirmation
                    , StringResources.ConfirmationDeleteObject
                    , MessageBoxButton.YesNo
                    , MessageBoxImage.Question
                    , MessageBoxResult.Yes) != MessageBoxResult.Yes)
                return;

            ImageEditControl.Clear();
        }

        private void InitializeMenus()
        {
            var bar = new BarItem
            {
                Name = "BarItemCommands",
                Caption = StringResources.Commands,
                GlyphSize = GlyphSizeType.Small
            };
            Menu.Bars.Add(bar);

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.Open,
                Command = NewImageCommand,
                ImageSmall = ImageResources.DCLAddNew16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLAddNew32.GetBitmapImage(),
                GlyphSize = GlyphSizeType.Small,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                IsVisible = true,
                Priority = 1
            });

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.Clear,
                Command = DeleteImageCommand,
                ImageSmall = ImageResources.DCLDelete16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLDelete32.GetBitmapImage(),
                GlyphSize = GlyphSizeType.Small,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                IsVisible = true,
                Priority = 2
            });
        }

        private IViewService GetViewService()
        {
            return IoC.Instance.Resolve<IViewService>();
        }

        private void OnImageLoadingImageLoading(object sender, ImageLoadingEventArgs e)
        {
            if (string.IsNullOrEmpty(e.FileName))
                return;

            var vm = ParentViewModel as ILoadImageHandler;
            if (vm == null)
                return;

            var fileinfo = new FileInfo(e.FileName);
            if (fileinfo.Exists)
            {
                e.IsCanceled = !vm.ValidateFileSize(fileinfo.Length);
            }
        }

        private void OnMenuPopupOpened(object sender, EventArgs e)
        {
            OnPropertyChanged(string.Empty);
        }

        private void OnEditValueChanged(object sender, EditValueChangedEventArgs e)
        {
            if (ImageEditControl.Source == null)
                return;

            if (ImageEditControl.Source.Height > _maxHeight)
                ImageEditControl.MaxHeight = _maxHeight;
            else
                ImageEditControl.MaxHeight = double.PositiveInfinity;
        }

        #endregion .  Methods  .

        #region .  INotifyPropertyChanged  .
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion .  INotifyPropertyChanged  .

    }
}
