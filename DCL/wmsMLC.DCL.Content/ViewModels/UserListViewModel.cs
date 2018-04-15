using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.PL;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectListView))]
    public class UserListViewModel : ObjectListViewModelBase<User>
    {
        public UserListViewModel()
        {
            PanelCaption = StringResources.UserListViewModelPanelCaption;
            PanelCaptionImage = ImageResources.DCLDefault16.GetBitmapImage();
        }

        protected override void CreateMainMenu()
        {
            base.CreateMainMenu();
            var adUsers = new BarItem { Caption = StringResources.UserListViewModelBarItemDomenUsers, Name = "BarItemUserListViewModelBarItemDomenUsers" };
            Menu.Bars.Add(adUsers);

            var menuItem = new CommandMenuItem
            {
                Name = "ActiveDirectoryUsers",
                Caption = StringResources.UserListViewModelMenuItemAddUsers,
                ImageSmall = ImageResources.DCLDefault16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLDefault32.GetBitmapImage(),
                Command = new DelegateCustomCommand(ShowAdUsers, CanShowAdUsers),
                DisplayMode = DisplayModeType.Default,
                //GlyphSize = GlyphSizeType.Large,
                GlyphAlignment = GlyphAlignmentType.Top
            };
            adUsers.MenuItems.Add(menuItem);
        }

        private bool CanShowAdUsers()
        {
            return true;
        }

        private async void ShowAdUsers()
        {
            var model = IoC.Instance.Resolve<MultipleSelectionViewModel<User>>();
            model.PanelCaption = StringResources.UserListViewModelBarItemDomenUsers;
            model.PanelCaptionImage = ImageResources.DCLDefault16.GetBitmapImage();
            try
            {
                WaitStart();
                model.AddFields(GetFields(typeof(User), SettingDisplay.List));
                //var items = await Task<IEnumerable<User>>.Factory.StartNew(() =>
                //    {
                //        var mgr = GetManager<IUserManager>();
                //        return mgr.GetAllFromActiveDirectory();
                //    });

                //var excludeItems = await Task<IEnumerable<User>>.Factory.StartNew(() =>
                //    {
                //        var mgr = GetManager<IUserManager>();
                //        return mgr.GetAll();
                //    });

                var res = await Task<bool>.Factory.StartNew(() =>
                    {
                        using (var mgr = GetManager<IUserManager>())
                        {
                            var items = mgr.GetAllFromActiveDirectory();
                            var excludeItems = mgr.GetAll(GetModeEnum.Partial);
                            model.SetSource(User.LoginPropertyName, items, excludeItems);
                        }
                        return true;
                    });

                //model.SetSource(User.LoginPropertyName, items, excludeItems);
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
            if (GetViewService().ShowDialogWindow(model, true, width: "50%", height: "40%") != true) return;
            var modelItems = model.Items;
            try
            {
                WaitStart();

                var insertList =
                    (from item in modelItems where item.Checked && item.IsEnabled select item.ItemObject).ToList();

                if (insertList.Count <= 0) 
                    return;
                var obj = insertList as IEnumerable<User>;
                GetManager().Insert(ref obj);
            }
            catch (Exception ex)
            {
                if (!ExceptionHandler(ex, ExceptionResources.ItemCantInsert))
                    throw;
            }
            finally
            {
                WaitStop();
            }
        }
    }
}
