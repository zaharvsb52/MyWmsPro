using System;
using System.Windows;
using DevExpress.Xpf.Editors.Validation.Native;

namespace wmsMLC.DCL.Main.Views.Controls
{
    public class CustomDateTimeEdit : CustomDateEdit
    {
        public const string TimeFormatDefault = "HH:mm:ss";

        #region . Properties .
        public string TimeMask
        {
            get { return (string)GetValue(TimeMaskProperty); }
            set { SetValue(TimeMaskProperty, value); }
        }
        public static readonly DependencyProperty TimeMaskProperty = DependencyProperty.Register("TimeMask", typeof(string), typeof(CustomDateTimeEdit), new PropertyMetadata(TimeFormatDefault));

        public bool IsShowTimePanel
        {
            get { return (bool)GetValue(IsShowTimePanelProperty); }
            set { SetValue(IsShowTimePanelProperty, value); }
        }
        public static readonly DependencyProperty IsShowTimePanelProperty = DependencyProperty.Register("IsShowTimePanel", typeof(bool), typeof(CustomDateTimeEdit));

        #endregion . Properties .

        #region . Methods .

        protected override void OnDisplayFormatStringChanged()
        {
            base.OnDisplayFormatStringChanged();
            IsShowTimePanel = ExistsTimeOfDate(DisplayFormatString);
            if (IsShowTimePanel && DisplayFormatString != Mask)
                Mask = DisplayFormatString;
        }

        protected override void MaskPropertiesChanged()
        {
            base.MaskPropertiesChanged();
            IsShowTimePanel = ExistsTimeOfDate(Mask);
        }

        internal void SetDateTime(DateTime editValue, UpdateEditorSource updateEditorSource)
        {
            EditStrategy.SetDateTime(editValue, updateEditorSource);
        }

        private bool ExistsTimeOfDate(string format)
        {
            if (string.IsNullOrEmpty(format))
                return false;

            string nowstr;
            try
            {
                nowstr = DateTime.Now.ToString(format);
            }
            catch
            {
                return false;
            }

            DateTime now;
            if (!DateTime.TryParse(nowstr, out now))
                return false;

            return now.TimeOfDay != TimeSpan.Zero;
        }

        #endregion . Methods .
    }
}
