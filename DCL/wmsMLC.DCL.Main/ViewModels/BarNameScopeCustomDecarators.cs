using System.Windows.Input;
using DevExpress.Mvvm.Native;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Bars.Customization;
using DevExpress.Xpf.Bars.Native;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Native;
using wmsMLC.DCL.Main.Views.Controls;
using wmsMLC.General.PL.WPF;

namespace wmsMLC.DCL.Main.ViewModels
{
    #region CustomCustomizationDecorator
    public class CustomCustomizationDecorator : CustomizationDecorator
    {
        protected override BarManagerCustomizationHelper CreateCustomizationHelper()
        {
            return new CustomBarManagerCustomizationHelper();
        }
    }

    internal class CustomCustomizationService : ICustomizationService
    {
        private CustomizationDecorator _decorator;
        private bool _customizationFormHidden;

        public CustomCustomizationService(CustomizationDecorator decorator)
        {
            _decorator = decorator;
        }

        bool ICustomizationService.IsPreparedToQuickCustomizationMode
        {
            get
            {
                return MayBe.Return(_decorator, x => x.CustomizationHelper.IsPreparedToQuickCustomizationMode, () => false);
            }
            set
            {
                MayBe.Do(_decorator, x => x.CustomizationHelper.IsPreparedToQuickCustomizationMode = value);
            }
        }

        bool ICustomizationService.IsCustomizationMode
        {
            get
            {
                return MayBe.Return(_decorator, x => x.CustomizationHelper.IsCustomizationMode, () => false);
            }
            set
            {
                if (_decorator == null)
                    return;

                //_decorator.CustomizationHelper.IsCustomizationMode = value;
                var helper = _decorator.CustomizationHelper as CustomBarManagerCustomizationHelper;
                if (helper != null)
                    helper.SetIsCustomizationMode(value);
            }
        }

        BarManagerCustomizationHelper ICustomizationService.CustomizationHelper
        {
            get
            {
                return MayBe.With(_decorator, x => x.CustomizationHelper);
            }
        }

        void ICustomizationService.ShowCustomizationForm()
        {
            if (_decorator == null)
                return;
            _decorator.CustomizationHelper.ShowCustomizationForm();
        }

        void ICustomizationService.CloseCustomizationForm()
        {
            if (_decorator == null)
                return;
            _decorator.CustomizationHelper.CloseCustomizationForm();
        }

        void ICustomizationService.HideCustomizationMenu(object target)
        {
            if (_decorator == null)
                return;
            if (target == null)
            {
                _decorator.CustomizationHelper.HideCustomizationMenus();
            }
            else
            {
                var barControl = target as BarControl;
                if (barControl != null && MayBe.Return(barControl.Bar, x => x.AllowCustomizationMenu, () => false))
                {
                    _decorator.CustomizationHelper.HideToolbarsCustomizationMenu(barControl);
                }
                else
                {
                    var customizationButton = target as BarQuickCustomizationButton;
                    if (customizationButton != null)
                    {
                        if (_decorator.CustomizationHelper.IsCustomizationMode)
                        {
                            _decorator.CustomizationHelper.HideCustomizationMenu();
                        }
                        else
                        {
                            LayoutHelper.FindParentObject<BarControl>(customizationButton);
                            _decorator.CustomizationHelper.HideCustomizationMenu(customizationButton);
                        }
                    }
                    else
                    {
                        var barItemLinkControl = target as BarItemLinkControl;
                        if (barItemLinkControl == null)
                            return;
                        _decorator.CustomizationHelper.HideItemCustomizationMenu(barItemLinkControl);
                    }
                }
            }
        }

        bool ICustomizationService.ShowCustomizationMenu(object target)
        {
            if (_decorator == null)
                return false;
            var barControl = target as BarControl;
            if (barControl != null && MayBe.Return(barControl.Bar, x => x.AllowCustomizationMenu, () => false))
                return _decorator.CustomizationHelper.ShowToolbarsCustomizationMenu(barControl);
            var customizationButton = target as BarQuickCustomizationButton;
            if (customizationButton != null)
            {
                if (_decorator.CustomizationHelper.IsCustomizationMode)
                    return false;
                return _decorator.CustomizationHelper.ShowCustomizationMenu(MayBe.With(LayoutHelper.FindParentObject<BarControl>(customizationButton), x => x.Bar), customizationButton);
            }
            var linkControl = target as BarItemLinkControl;
            if (linkControl != null)
                return _decorator.CustomizationHelper.ShowItemCustomizationMenu(linkControl);
            return false;
        }

        void ICustomizationService.HideCustomizationForm()
        {
            if (_decorator == null)
                return;
            _customizationFormHidden = true;

            //MayBe.Do(_decorator.CustomizationHelper.CustomizationForm, x => x.IsOpen = false);
            var helper = _decorator.CustomizationHelper as CustomBarManagerCustomizationHelper;
            if (helper != null)
                MayBe.Do(helper.CustomizationForm, x => x.IsOpen = false);
        }

        void ICustomizationService.RestoreCustomizationForm()
        {
            if (_decorator == null || !_decorator.CustomizationHelper.IsCustomizationMode || !_customizationFormHidden)
                return;
            _customizationFormHidden = false;

            // MayBe.Do(_decorator.CustomizationHelper.CustomizationForm, x => x.IsOpen = true);
            var helper = _decorator.CustomizationHelper as CustomBarManagerCustomizationHelper;
            if (helper != null)
                MayBe.Do(helper.CustomizationForm, x => x.IsOpen = true);
        }

        void ICustomizationService.Select(BarItemLinkControl lControl)
        {
            if (_decorator == null || !_decorator.CustomizationHelper.IsCustomizationMode || BarManagerCustomizationHelper.IsInCustomizationMenu(lControl))
                return;
            _decorator.CustomizationHelper.SelectedLinkControl = lControl;
        }
    }

    public class CustomBarManagerCustomizationHelper : BarManagerCustomizationHelper
    {
        internal new FloatingContainer CustomizationForm
        {
            get { return base.CustomizationForm; }
            set { base.CustomizationForm = value; }
        }

        protected override CustomizationControl CreateDefaultCustomizationControl()
        {
            var result = base.CreateDefaultCustomizationControl();

            //HACK: Megahack
            CustomBarManager manager = null;
            if (Scope != null)
                manager = Scope.Target as CustomBarManager;
            result.OptionsControl.Content = new CustomOptionsControl { Manager = manager };
            return result;
        }

        internal void SetIsCustomizationMode(bool mode)
        {
            IsCustomizationMode = mode;
        }
    }

    #endregion CustomCustomizationDecorator

    #region CustomItemCommandSourceStrategy
    //Как работает KeyGestureWorkingMode
    //protected virtual void OnScopeTargetKeyDown(object sender, KeyEventArgs e)
    //{
    //    this.CheckExecuteGesture(e, DevExpress.Xpf.Bars.KeyGestureWorkingMode.UnhandledKeyGesture);
    //}

    //protected virtual void OnScopeTargetPreviewKeyDown(object sender, KeyEventArgs e)
    //{
    //    this.CheckExecuteGesture(e, DevExpress.Xpf.Bars.KeyGestureWorkingMode.AllKeyGesture);
    //}

    //protected virtual void OnTopElementPreviewKeyDown(object sender, KeyEventArgs e)
    //{
    //    this.CheckExecuteGesture(e, DevExpress.Xpf.Bars.KeyGestureWorkingMode.AllKeyGestureFromRoot);
    //}

    public class CustomItemCommandSourceStrategy : ItemCommandSourceStrategy
    {
        private BarManager _scopeTarget;

        protected override void OnScopeTargetKeyDown(object sender, KeyEventArgs e)
        {
            if(IsActive(e) == false)
                return;

            base.OnScopeTargetKeyDown(sender, e);
        }

        protected override void OnScopeTargetPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (IsActive(e) == false)
                return;

            base.OnScopeTargetPreviewKeyDown(sender, e);
        }

        protected override void OnTopElementPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (IsActive(e) == false)
                return;

            base.OnTopElementPreviewKeyDown(sender, e);
        }

        private void SetScopeTarget(KeyEventArgs e)
        {
            if (_scopeTarget != null)
                return;

            var keyBinding = GetKeyBinding(e);
            if (keyBinding == null)
                return;

            var shortCutCommand = keyBinding.Command as ShortCutCommand;
            if (shortCutCommand != null && shortCutCommand.Item != null)
            {
                try
                {
                    var item = shortCutCommand.Item;
                    var parent = LayoutHelper.FindAmongParents<BarManager>(item, null);
                    _scopeTarget = parent;
                }
                catch
                {
                    _scopeTarget = null;
                }
            }
        }

        private bool? IsActive(KeyEventArgs e)
        {
            SetScopeTarget(e);
            if (_scopeTarget == null)
                return null;

            try
            {
                //DependencyObject dob = _scopeTarget;
                //while (dob != null)
                //{
                //    dob = LayoutHelper.GetParent(dob);
                //    var layoutItem = dob as BaseLayoutItem;
                //    if (layoutItem != null && layoutItem.IsFocused)
                //        return true;
                //    var window = dob as Window;
                //    if (window != null && window.IsActive)
                //        return true;
                //}

                //Используем старый алгоритм
                var ctx = _scopeTarget.DataContext as IActivatable;
                return ctx == null ? (bool?)null : ctx.IsActive;
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            _scopeTarget = null;
        }

        //private UIElement GetContainingUIElement(DependencyObject obj)
        //{
        //    return obj as UIElement ?? LayoutHelper.FindLayoutOrVisualParentObject(obj, p => p is UIElement, true) as UIElement;
        //}
    }

    internal class CustomCommandSourceService : ICommandSourceService, ICustomICommandSourceService
    {
        private CustomItemCommandSourceStrategy _strategy;

        public CustomCommandSourceService()
        {
        }

        public CustomCommandSourceService(CustomItemCommandSourceStrategy strategy)
        {
            _strategy = strategy;
        }

        void ICommandSourceService.CommandChanged(BarItem element, ICommand oldValue, ICommand newValue)
        {
            if (_strategy != null)
                _strategy.CommandChanged(element, oldValue, newValue);
        }

        void ICommandSourceService.KeyGestureChanged(BarItem element, KeyGesture oldValue, KeyGesture newValue)
        {
            if (_strategy != null)
                _strategy.KeyGestureChanged(element, oldValue, newValue);
        }

        void ICustomICommandSourceService.Dispose()
        {
            if (_strategy != null)
                _strategy.Dispose();
        }
    }

    public interface ICustomICommandSourceService
    {
        void Dispose();
    }

    #endregion CustomItemCommandSourceStrategy
}
