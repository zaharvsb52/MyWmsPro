using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Components.Controls.Rcl;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.General.PL.WPF.Components.Helpers
{
    public static class MenuHelper
    {
        public static void CreateFooterMenu(FooterMenu footerMenuControl, ValueDataField field, ICommand command, double fontSize, bool isWfDesignMode)
        {
            if (footerMenuControl == null)
                footerMenuControl = new FooterMenu();

            footerMenuControl.Visibility = field.Visible ? Visibility.Visible : Visibility.Collapsed;

            ValueDataField[] menu;
            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.FooterMenu, isWfDesignMode, out menu))
            {
                var index = 0;
                foreach (var menuItem in menu.Where(p => p != null).OrderBy(p => p.Order))
                {
                    AddFooterMenuItem(footerMenuControl, menuItem, index++, command, fontSize, isWfDesignMode);
                }
            }

            if (fontSize > 0)
                footerMenuControl.FontSize = fontSize;

            string focusNavigationDirection;
            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.NavigationDirectionOnGotFocus, isWfDesignMode, out focusNavigationDirection))
                footerMenuControl.NavigationDirectionOnGotFocus = (FocusNavigationDirection)Enum.Parse(typeof(FocusNavigationDirection), focusNavigationDirection);

            if (field.SetFocus)
                footerMenuControl.BackgroundFocus();
        }

        public static void AddFooterMenuItem(FooterMenu footerMenuControl, ValueDataField menuItem, int index, ICommand command, double fontSize, bool isWfDesignMode)
        {
            var button = CreateCustomButton(menuItem, command, fontSize, isWfDesignMode);
            FooterMenu.SetRow(button, GetRow(menuItem, index));
            FooterMenu.SetColumn(button, GetColumn(menuItem, index));
            footerMenuControl.Menu.Add(button);
        }

        public static int GetRow(ValueDataField menuItem, int index)
        {
            if (menuItem == null || !menuItem.Properties.ContainsKey((ValueDataFieldConstants.Row)))
                return index / 2;
            return menuItem.Get<int>(ValueDataFieldConstants.Row);
        }

        public static int GetColumn(ValueDataField menuItem, int index)
        {
            if (menuItem == null || !menuItem.Properties.ContainsKey((ValueDataFieldConstants.Column)))
                return index % 2;
            return menuItem.Get<int>(ValueDataFieldConstants.Column);
        }

        public static CustomButton CreateCustomButton(ValueDataField field, ICommand command, double fontSize, bool isWfDesignMode)
        {
            var hotkey = field.Value.To(Key.None);

            var hotkey2 = Key.None;
            string svalue;
            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.HotKey2, isWfDesignMode, out svalue))
                hotkey2 = svalue.To(Key.None);

            var button = new CustomButton
            {
                Text = field.Caption,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                HotKey = hotkey,
                HotKey2 = hotkey2,
                Command = command,
                CommandParameter = field,
                IsEnabled = field.IsEnabled.HasValue && field.IsEnabled.Value,
                Visibility = field.Visible ? Visibility.Visible : Visibility.Collapsed,
            };

            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.SuffixText, isWfDesignMode, out svalue))
                button.SuffixText = svalue;

            bool isNotMenuButton;
            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.IsNotMenuButton, isWfDesignMode, out isNotMenuButton))
                button.IsNotMenuButton = isNotMenuButton;

            bool transferHotKeyToControls;
            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.TransferHotKeyToControls, isWfDesignMode, out transferHotKeyToControls))
                button.TransferHotKeyToControls = transferHotKeyToControls;

            ICommand icommand;
            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.Command, isWfDesignMode, out icommand))
                button.Command = icommand;

            if (fontSize > 0)
                button.FontSize = fontSize;

            if (field.SetFocus)
                button.BackgroundFocus();

            return button;
        }
    }
}
