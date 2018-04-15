using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DevExpress.Xpf.LayoutControl;
using wmsMLC.General.PL.WPF.Components.Controls.Rcl;
using wmsMLC.General.PL.WPF.Components.Views;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.General.PL.WPF.Components.Helpers
{
    public static class KeyHelper
    {
        private const string CloseDialog = "_CLOSEDIALOG_;";
        private const string DoNotMovedFocusOnNextControl = "_DONOTMOVEDFOCUSONNEXTCONTROL_;";

        public static void PreviewKeyDown(FrameworkElement control, KeyEventArgs e)
        {
            if (e.Key == UiCore.ReturnedScannerKey || e.Key == UiCore.NextControlKey)
            {
                var termilalUi = control as IRclUi;

                if (termilalUi == null)
                {
                    if (IsCloseDialog(control.Tag))
                        return;
                    if (IsDoNotMovedFocusOnNextControl(control.Tag))
                    {
                        e.Handled = true;
                        return;
                    }
                    control.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    e.Handled = true;
                }
                else
                {
                    if (termilalUi.IsMovedFocusOnNextControl)
                    {
                        control.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                        e.Handled = true;
                    }
                    termilalUi.RaiseControlKeyDownEvent();
                }
            }
            else
            {
                switch (e.Key)
                {
                    case Key.Up:
                        var parent = control.Parent;
                        if (parent != null)
                        {
                            var layoutControl = ((FrameworkElement)parent).Parent as LayoutControl;
                            if (layoutControl != null && layoutControl.Children != null && layoutControl.Children.Count > 0)
                            {
                                var first = layoutControl.Children.OfType<LayoutItem>().FirstOrDefault();
                                if (first != null && ReferenceEquals(first, parent))
                                {
                                    e.Handled = true;
                                    return;
                                }
                            }
                        }

                        control.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                        e.Handled = true;
                        break;
                    case Key.Down:
                        parent = control.Parent;
                        if (parent != null)
                        {
                            var layoutControl = ((FrameworkElement)parent).Parent as LayoutControl;
                            if (layoutControl != null && layoutControl.Children != null && layoutControl.Children.Count > 0)
                            {
                                var last = layoutControl.Children.OfType<LayoutItem>().LastOrDefault();
                                if (last != null && ReferenceEquals(last, parent))
                                {
                                    e.Handled = true;
                                    return;
                                }
                            }
                        }

                        control.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                        e.Handled = true;
                        break;
                }
            }
        }

        public static void SetFocusElement(UIElementCollection children)
        {
            if (children == null || children.Count <= 0) 
                return;

            var layoutItems = GetLayoutItems(children);
            if (layoutItems.Length > 0)
            {
                var element = layoutItems.FirstOrDefault(p => p.IsEnabled && p.Visibility == Visibility.Visible) ?? layoutItems[0];
                if (element != null)
                    element.BackgroundFocus();
            }
        }

        public static LayoutItem[] GetLayoutItems(UIElementCollection children)
        {

            if (children == null || children.Count <= 0)
                return new LayoutItem[0];

            var layoutItems = new List<LayoutItem>();
            foreach (var p in children)
            {
                var layoutGroup = p as LayoutGroup;
                if (layoutGroup != null)
                {
                    var items = GetLayoutItems(layoutGroup.Children);
                    if (items.Length > 0)
                        layoutItems.AddRange(items);
                    continue;
                }

                var layoutItem = p as LayoutItem;
                if (layoutItem != null)
                    layoutItems.Add(layoutItem);
            }

            return layoutItems.ToArray();
        }

        private static bool IsTagContains(object tag, string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException("text");

            return tag.To(string.Empty).ToUpper().Contains(text);
        }

        public static bool IsCloseDialog(object tag)
        {
            return IsTagContains(tag, CloseDialog);
        }

        public static bool IsDoNotMovedFocusOnNextControl(object tag)
        {
            return IsTagContains(tag, DoNotMovedFocusOnNextControl);
        }

        private static void SetTagElement(FrameworkElement element, string text)
        {
            if (element == null)
                return;

            if (!IsTagContains(element.Tag, text))
                element.Tag += text;
        }

        public static void SetCloseDialogElement(FrameworkElement element)
        {
            SetTagElement(element, CloseDialog);
        }
        public static void SetDoNotMovedFocusOnNextControl(FrameworkElement element)
        {
            SetTagElement(element, DoNotMovedFocusOnNextControl);
        }

        public static int? GetNumKey(Key key)
        {
            switch (key)
            {
                case Key.NumPad0:
                case Key.D0:
                case Key.NumPad1:
                case Key.D1:
                case Key.NumPad2:
                case Key.D2:
                case Key.NumPad3:
                case Key.D3:
                case Key.NumPad4:
                case Key.D4:
                case Key.NumPad5:
                case Key.D5:
                case Key.NumPad6:
                case Key.D6:
                case Key.NumPad7:
                case Key.D7:
                case Key.NumPad8:
                case Key.D8:
                case Key.NumPad9:
                case Key.D9:
                    var str = key.ToString();
                    return int.Parse(str.Substring(str.Length - 1));
            }
            return null;
        }

        public static void ViewPreviewKeyDown(DependencyObject obj, KeyEventArgs e)
        {
            if (e.Handled)
                return;

            //Обработчик горячих клавиш меню
            foreach (var p in VisualTreeHelperExt.FindChildsByType<CustomButton>(obj).Where(p => p.IsHotKey(e.Key)))
            {
                if (p.PreviewHotKey(e))
                    return;
            }

            //Лукапы
            if (!e.Handled)
            {
                foreach (var p in VisualTreeHelperExt.FindChildsByType<CustomComboBoxEditRcl>(obj))
                {
                    if (p.PreviewHotKey(e))
                        return;
                }
            }

            if (!e.Handled)
            {
                var customSelectControl = VisualTreeHelperExt.FindChildsByType(obj, typeof(CustomSelectControl)).FirstOrDefault() as CustomSelectControl;
                if (customSelectControl != null)
                {
                    customSelectControl.PreviewHotKey(e);
                    if (customSelectControl.ParentKeyPreview)
                        customSelectControl.PreviewItemHotKey(e);
                }
            }
        }
    }
}
