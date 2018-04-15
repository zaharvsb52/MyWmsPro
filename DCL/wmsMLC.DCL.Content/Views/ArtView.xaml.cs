using System.Linq;
using System.Windows;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.LayoutControl;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.DCL.Content.Views
{
    public partial class ArtView
    {
        private LayoutGroup[] _childGroups;

        public ArtView()
        {
            InitializeComponent();
            Initialize();
        }

        #region .  Methods  .

        private void Initialize()
        {
            Group = ObjectDataLayout;
            MenuView = MainMenuView;
        }

        protected override void OnAfterRestoreLayout()
        {
            base.OnAfterRestoreLayout();
            if (ObjectDataLayout.Children == null)
                return;

            //Подписываемся на событие SelectedTabChildChanged
            _childGroups = ObjectDataLayout.Children.OfType<LayoutGroup>().Where(p => p.View == LayoutGroupView.Tabs).ToArray();
            SubscribeOnSelectedTabChildChanged();
        }

        private void OnSelectedTabChildChanged(object sender, ValueChangedEventArgs<FrameworkElement> e)
        {
            var group = e.NewValue as LayoutGroup;
            if (group == null || group.Children == null)
                return;

            var vm = DataContext as ILoadImageHandler;
            if (vm == null)
                return;

            if (VisualTreeHelperExt.FindChildsByType<SubImageView>(group).Any())
            {
                vm.LoadImage();
                UnsubscribeOnSelectedTabChildChanged();
            }
        }

        private void SubscribeOnSelectedTabChildChanged()
        {
            if (_childGroups != null)
            {
                UnsubscribeOnSelectedTabChildChanged();
                foreach (var gr in _childGroups.Where(p => p != null))
                {
                    gr.SelectedTabChildChanged += OnSelectedTabChildChanged;
                }
            }
        }

        private void UnsubscribeOnSelectedTabChildChanged()
        {
            if (_childGroups != null)
            {
                foreach (var gr in _childGroups.Where(p => p != null))
                {
                    gr.SelectedTabChildChanged -= OnSelectedTabChildChanged;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnsubscribeOnSelectedTabChildChanged();
            }

            base.Dispose(disposing);
        }

        #endregion .  Methods  .
    }
}
