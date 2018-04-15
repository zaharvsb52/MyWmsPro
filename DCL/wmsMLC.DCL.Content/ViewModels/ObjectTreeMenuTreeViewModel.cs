using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.Views;
#pragma warning disable 1998

namespace wmsMLC.DCL.Content.ViewModels
{
    /// <summary>
    /// Главное меню.
    /// </summary>
    [View(typeof(ObjectTreeView))]
    public class MainMenuTreeViewModel : ObjectTreeViewModelBase<ObjectTreeMenu>
    {
        public MainMenuTreeViewModel()
        {
            KeyPropertyName = ObjectTreeMenu.ObjectTreeCodePropertyName.ToUpper();
            ParentIdPropertyName = ObjectTreeMenu.ObjectTreeParentPropertyName.ToUpper();
            PanelCaption = StringResources.MainMenu;
            PanelCaptionImage = null;
            DefaultSortingField = ObjectTreeMenu.ObjectTreeOrderPropertyName;

            //HACK: Обнуляем MaxRowCount в фильтре.
            if (Filters != null)
                Filters.MaxRowCount = null;
        }

        protected override void InitializeSettings()
        {
            base.InitializeSettings();

            IsMenuEnable = false;
            IsContextMenuEnable = false;
            DenyBusinessProcessTrigger = true;
            AllowClosePanel = false;
            ShowColumnHeaders = false;
            ShowNodeImage = true;
            ShowTotalRow = false;
        }

        public override void InitializeMenus()
        {
            base.InitializeMenus();
            RefreshData();
        }

        protected override ObservableCollection<DataField> GetFields(Type type, SettingDisplay settings)
        {
            var items = base.GetFields(type, settings);
            if (settings != SettingDisplay.List)
                return items;

            // проверяем наличие обязательных полей
            var nameField = items.FirstOrDefault(i => i.Name.EqIgnoreCase(ObjectTreeMenu.ObjectTreeNamePropertyName));
            if (nameField == null)
                throw new DeveloperException(DeveloperExceptionResources.DoesntHaveFieldName);

            var orderField = items.FirstOrDefault(i => i.Name.EqIgnoreCase(ObjectTreeMenu.ObjectTreeOrderPropertyName));
            if (orderField == null)
                throw new DeveloperException("Can't find filed " + ObjectTreeMenu.ObjectTreeOrderPropertyName);

            orderField.Visible = false;
            return new ObservableCollection<DataField> { nameField, orderField };
        }

        protected override async void Edit()
        {
            try
            {
                WaitStart();
                if (!ConnectionManager.Instance.AllowRequest())
                    return;

                var viewService = GetViewService();
                foreach (var o in SelectedItems)
                {
                    if (!string.IsNullOrEmpty(o.ObjectTreeAction))
                        viewService.Show(o.ObjectTreeAction);
                }
            }
            catch (Exception ex)
            {
                if (!ExceptionHandler(ex, ExceptionResources.ItemCantEdit))
                    throw;
            }
            finally
            {
                WaitStop();
            }
        }

        public override KeyGesture GetEditKey()
        {
            return EditKey;
        }
    }

    /// <summary>
    /// Настройка главного меню.
    /// </summary>
    [View(typeof (ObjectTreeView))]
    public class ObjectTreeMenuTreeViewModel : ObjectTreeViewModelBase<ObjectTreeMenu>
    {
        public ObjectTreeMenuTreeViewModel()
        {
            KeyPropertyName = ObjectTreeMenu.ObjectTreeCodePropertyName.ToUpper();
            ParentIdPropertyName = ObjectTreeMenu.ObjectTreeParentPropertyName.ToUpper();
        }
    }
}