using System.Linq;
using System.Windows.Input;
using wmsMLC.DCL.Resources;
using wmsMLC.General.PL.WPF;

namespace wmsMLC.DCL.General.ViewModels
{
    public class ObjectTreeViewModelBase<TModel> : ObjectListViewModelBase<TModel>, IObjectTreeViewModel
    {
        public string KeyPropertyName { get; set; }
        public string ParentIdPropertyName { get; set; }
        public bool ShowNodeImage { get; set; }
        public bool ShowTotalRow { get; protected set; }
        public string DefaultSortingField { get; set; }

        private bool _autoExpandAllNodes;
        public bool AutoExpandAllNodes
        {
            get { return _autoExpandAllNodes; }
            set
            {
                if (_autoExpandAllNodes == value)
                    return;
                _autoExpandAllNodes = value;
                OnPropertyChanged("AutoExpandAllNodes");
            }
        }

        protected override void InitializeSettings()
        {
            base.InitializeSettings();

            ShowTotalRow = true;
            ShowNodeImage = false;
        }

        protected override void CreateMainMenu()
        {
            base.CreateMainMenu();
            if (IsMenuEnable)
            {
                var barTech = Menu.Bars.FirstOrDefault(p => p.Caption == StringResources.CustomizationBarMenu);
                if (barTech != null)
                {
                    var menu = barTech.MenuItems.FirstOrDefault(p => p.Caption == StringResources.AppearanceStyle);
                    if (menu != null)
                        barTech.MenuItems.Remove(menu);
                }
            }
        }

        protected override string ViewServiceRegisterSuffix
        {
            get { return ModuleBase.ViewServiceRegisterSuffixTreeShow; }
        }

        public virtual KeyGesture GetEditKey()
        {
            return null;
        }
    }
}
