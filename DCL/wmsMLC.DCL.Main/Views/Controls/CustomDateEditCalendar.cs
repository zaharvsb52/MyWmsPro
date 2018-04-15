using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Popups.Calendar;
using DevExpress.Xpf.Editors.Validation.Native;
using wmsMLC.General.PL.WPF.Commands;

namespace wmsMLC.DCL.Main.Views.Controls
{
    public class CustomDateEditCalendar : DateEditCalendar
    {
        public CustomDateEditCalendar()
        {
            TodayCommand = new DelegateCustomCommand(OnToday, CanToday);
            ClearCommand = new DelegateCustomCommand(OnClear, CanClear);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var buttonClear = GetTemplateChild("PART_Clear") as Button;
            if (buttonClear != null)
                buttonClear.Visibility = Visibility.Collapsed;
        }

        #region . Commands .
        public ICommand TodayCommand { get; set; }

        private bool CanToday()
        {
            return OwnerDateEdit != null && !OwnerDateEdit.IsReadOnly;
        }

        private void OnToday()
        {
            if (!CanToday())
                return;

            if (OwnerDateEdit != null)
            {
                OwnerDateEdit.ClearError();
                OwnerDateEdit.EditValue = DateTime.Now;
                OwnerDateEdit.IsPopupOpen = false;
            }
        }

        public ICommand ClearCommand { get; set; }

        private bool CanClear()
        {
            return OwnerDateEdit != null && !OwnerDateEdit.IsReadOnly;
        }

        private void OnClear()
        {
            if (!CanClear())
                return;
            base.OnClearButtonClick(this, new RoutedEventArgs());
        }

        #endregion . Commands .

        public new DateTime GetDateTime(DependencyObject obj)
        {
            return ((DateTime)DateEditCalendarBase.GetDateTime(obj)).Date.Add(DateTime.TimeOfDay);
        }

        protected override void OnDayCellButtonClick(Button button)
        {
            //base.OnDayCellButtonClick(button);
            if (OwnerDateEdit == null)
            {
                if (button != null)
                {
                    DateTime = GetDateTime(button);
                }
            }
            else
            {
                if (!OwnerDateEdit.IsReadOnly)
                {
                    ((CustomDateTimeEdit)OwnerDateEdit).SetDateTime(GetDateTime(button),
                        UpdateEditorSource.ValueChanging);
                }
                OwnerDateEdit.IsPopupOpen = false;
            }
        }
    }
}
