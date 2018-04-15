using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.Business.Objects.Processes;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.ViewModels;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectView))]
    public class BPWorkflowViewModel : ObjectViewModelBase<BPWorkflow>
    {
        private bool _initializing;
        private bool? _isReadOnly;
        private bool _isDesignerOpened;

        public BPWorkflowViewModel()
        {
            _initializing = true;

            ShowDesignerCommand = new DelegateCustomCommand<bool?>(ShowDesigner, CanShowDesigner);
        }

        private string _currentUser;
        private string CurrentUser
        {
            get
            {
                if (string.IsNullOrEmpty(_currentUser))
                {
                    var authenticatedUser = WMSEnvironment.Instance.AuthenticatedUser;
                    _currentUser = authenticatedUser == null ? null : authenticatedUser.GetSignature();
                    if (_currentUser == null)
                        throw new DeveloperException("User is not defined.");
                }
                return _currentUser;
            }
        }

        public override void CreateProcessMenu()
        {
            base.CreateProcessMenu();
            var bar = Menu.GetOrCreateBarItem(StringResources.BusinessProcesses, 1, "BarItemWDUCL");
            var listmenu = new ListMenuItem
            {
                Name = "ListMenuWDUCL",
                Caption = StringResources.WDUCL,
                ImageSmall = ImageResources.DCLWFDesigner16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLWFDesigner32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                Priority = 1000
            };

            listmenu.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.WfOpenReadOnly,
                Command = ShowDesignerCommand,
                CommandParameter = true,
                ImageSmall = ImageResources.DCLWFOpenReadOnly16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLWFOpenReadOnly32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                Priority = 1
            });

            listmenu.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.WfOpen,
                Command = ShowDesignerCommand,
                CommandParameter = false,
                ImageSmall = ImageResources.DCLWFOpen16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLWFOpen32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                Priority = 2
            });

            bar.MenuItems.Add(listmenu);

            //Исключаем кнопку OpenInNewWindow
            foreach (var b in Menu.Bars.Where(b => b.Caption == StringResources.Commands).ToArray())
            {
                foreach (var m in b.MenuItems.Where(m => m.Caption == StringResources.OpenInNewWindow).ToArray())
                {
                    b.MenuItems.Remove(m);
                }
            }
        }

        public ICustomCommand ShowDesignerCommand { get; private set; }

        protected override void OnSourceChanged()
        {
            base.OnSourceChanged();
            // при инициализации объекта прочтем его xaml из БД
            if (_initializing)
            {
                _initializing = false;
                var mgr = IoC.Instance.Resolve<IXamlManager<BPWorkflow>>();
                var key = Source.GetKey();
                string xaml = null;
                if (key != null)
                    xaml = mgr.GetXaml(key.ToString());

                if (!string.IsNullOrEmpty(xaml))
                {
                    Source.SuspendNotifications();
                    Source.SuspendValidating();
                    Source.SetProperty(BPWorkflow.XamlPropertyName, xaml);
                    var eo = Source as IEditable;
                    if (eo != null)
                        eo.AcceptChanges(BPWorkflow.XamlPropertyName);
                    Source.ResumeNotifications();
                    Source.ResumeValidating();
                }
            }

            ValidateReadOnly();
        }

        protected override bool CanOpenInNewWindow()
        {
            return false;
        }

        protected override bool CanDelete()
        {
            return !_isDesignerOpened && !ValidateUserLock() && base.CanDelete();
        }

        protected override bool CanSave()
        {
            return (!_isDesignerOpened || _isDesignerOpened && _isReadOnly == true) && !ValidateUserLock() && base.CanSave();
        }

        protected override bool CanSaveAndClose()
        {
            return !_isDesignerOpened && CanSave();
        }

        //protected override bool CanRefresh()
        //{
        //    return (!_isDesignerOpened || _isDesignerOpened && _isReadOnly == true) && base.CanRefresh();
        //}

        protected override void RefreshData(bool usewait)
        {
            try
            {
                _initializing = true;
                base.RefreshData(usewait);
            }
            finally
            {
                //Если нет прав на Refresh
                _initializing = false;
            }
        }

        protected override void OnIsDirtyChanged(IEditable eo)
        {
            base.OnIsDirtyChanged(eo);

            ShowDesignerCommand.RaiseCanExecuteChanged();
        }

        protected override ObservableCollection<DataField> GetFields(SettingDisplay displaySetting)
        {
            var result = base.GetFields(displaySetting);
            {
                foreach (var p in result)
                {
                    if (_isReadOnly == false || ValidateUserLock())
                        p.IsEnabled = false;
                    p.IsChangeLookupCode = true;
                }
            }
            return result;
        }

        private bool CanShowDesigner(bool? isReadOnly)
        {
            var isnew = Source as IIsNew;
            if (isnew == null || isnew.IsNew)
                return false;
            var ed = Source as IEditable;
            return (ed != null && !ed.IsDirty && (!_isReadOnly.HasValue || isReadOnly == _isReadOnly));
        }

        private void ShowDesigner(bool? isReadOnly)
        {
            if (!CanShowDesigner(isReadOnly))
                return;

            try
            {
                WaitStart();

                var vs = GetViewService();
                var version = Environment.OSVersion;
                if (version.Version.Major < 6 && version.Version.Minor < 1)
                {
                    vs.ShowDialog(StringResources.DesignerBp,
                        StringResources.DesignerBpRestriction,
                        MessageBoxButton.OK,
                        MessageBoxImage.Stop, MessageBoxResult.OK);

                    return;
                }
                
                //Обновляем WF перед дизайном
                RefreshData(false);

                if (isReadOnly != true)
                {
                    if (ValidateUserLock())
                    {
                        if (vs.ShowDialog(StringResources.DesignerBp
                            , string.Format(StringResources.DesignerBpLockConfirmation, Source.UserCode_R)
                            , MessageBoxButton.YesNo
                            , MessageBoxImage.Question
                            , MessageBoxResult.Yes) == MessageBoxResult.Yes)
                        {
                            isReadOnly = true;
                        }
                        else
                        {
                            //Выходим. WF залочена другим пользователем
                            _isReadOnly = null;
                            return;
                        }
                    }

                    if (string.IsNullOrEmpty(Source.UserCode_R))
                    {
                        Source.UserCode_R = CurrentUser;
                        if (!base.Save())
                            return;

                        RefreshView();
                    }
                }

                var cm = IoC.Instance.Resolve<IWDUCLViewModel>();
                // если не реализован дизайнер, то и не покажем
                if (cm == null)
                    throw new DeveloperException("Not registered.");

                var source = (WMSBusinessObject)Source.Clone();
                source.AcceptChanges();

                cm.SetSource(source);

                IViewModel viewModel = null;
                vs.Show((ViewModelBase)cm, new ShowContext { DockingType = DockType.Document }, ref viewModel);

                var vm = viewModel is IWDUCLViewModel ? (IWDUCLViewModel) viewModel : cm;

                vm.SetIsReadOnly(isReadOnly == true);
                vm.CanClose -= OnWduclViewModelCanClose;
                vm.CanClose += OnWduclViewModelCanClose;

                _isDesignerOpened = true;
                _isReadOnly = isReadOnly;
            }
            finally
            {
                WaitStop();
            }
        }

        private void OnWduclViewModelCanClose(object sender, EventArgs eventArgs)
        {
            try
            {
                _isDesignerOpened = false;

                var cm = sender as IWDUCLViewModel;
                if (cm == null)
                    return;
                cm.CanClose -= OnWduclViewModelCanClose;

                //Обновляем только сущность
                base.RefreshData(false);
                if (IsLockedByCurrentUser())
                {
                    var vs = GetViewService();
                    if (vs.ShowDialog(StringResources.DesignerBp
                        , StringResources.DesignerBpUnLockConfirmation
                        , MessageBoxButton.YesNo
                        , MessageBoxImage.Question
                        , MessageBoxResult.Yes) == MessageBoxResult.Yes)
                    {
                        Source.UserCode_R = null;
                        base.Save();
                    }
                }
            }
            finally
            {
                ValidateReadOnly();
                RefreshView();
                RiseCommandsCanExecuteChanged();
            }
        }

        /// <summary>
        /// Метод возвращает true, если workflow залочена другим пользователем.
        /// </summary>
        private bool ValidateUserLock()
        {
            return Source != null && !string.IsNullOrEmpty(Source.UserCode_R) && !CurrentUser.EqIgnoreCase(Source.UserCode_R);
        }

        private bool IsLockedByCurrentUser()
        {
            return Source != null && CurrentUser.EqIgnoreCase(Source.UserCode_R);
        }

        private void ValidateReadOnly()
        {
            if (IsLockedByCurrentUser())
            {
                _isReadOnly = false;
            }
            else if (ValidateUserLock())
            {
                _isReadOnly = true;
            }
            else
            {
                if (!_isDesignerOpened)
                    _isReadOnly = null;
            }
        }

        //Поле с xaml'ом readonly не сохраняем больше xaml
        //protected override bool Save()
        //{
        //    // запомним значение xaml иначе оно будет стерто при сохраннии
        //    var xaml = Source.GetProperty(BPWorkflow.XamlPropertyName);
        //    Source.SetProperty(BPWorkflow.XamlPropertyName, null);
        //    var result = base.Save();
        //    if (!result)
        //        return false;

        //    try
        //    {
        //        WaitStart();
        //        var mgr = IoC.Instance.Resolve<IXamlManager<BPWorkflow>>();
        //        Source.SuspendNotifications();
        //        Source.SuspendValidating();
        //        Source.SetProperty(BPWorkflow.XamlPropertyName, xaml);
        //        mgr.SetXaml(Source.GetKey().ToString(), xaml != null ? xaml.ToString() : string.Empty);
        //        var eo = Source as IEditable;
        //        if (eo != null)
        //            eo.AcceptChanges();
        //    }
        //    finally
        //    {
        //        WaitStop();
        //        Source.ResumeNotifications();
        //        Source.ResumeValidating();
        //    }
        //    return true;
        //}
    }
}
