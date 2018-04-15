using System.Windows.Controls;
using System.Windows.Input;
using DevExpress.Xpf.Editors;

namespace wmsMLC.General.PL.WPF.Components.Controls.Rcl
{
    public class CustomDateEdit : DateEdit
    {
        public CustomDateEdit()
        {
            ShowEditorButtons = false;
        }

        protected override EditStrategyBase CreateEditStrategy()
        {
            return new CustomDateEditStrategy(this);
        }

        protected override TextInputSettingsBase CreateTextInputSettings()
        {
            return new CustomTextInputMaskSettings(this);
        }

        protected override void SubscribeEditEventsCore()
        {
            var editCore = EditCore as TextBox;
            if (editCore != null)
                editCore.PreviewKeyUp += EditCoreOnPreviewKeyUp;

            base.SubscribeEditEventsCore();
        }

        protected override void UnsubscribeEditEventsCore()
        {
            var editCore = EditCore as TextBox;
            if (editCore != null)
                editCore.PreviewKeyUp -= EditCoreOnPreviewKeyUp;

            base.UnsubscribeEditEventsCore();
        }

        private void EditCoreOnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (TextInputSettings == null)
                TextInputSettings = CreateTextInputSettings();

            if (e.Handled)
                return;

            switch (e.Key)
            {
                case Key.Left:
                case Key.Right:
                    ((CustomTextInputMaskSettings) TextInputSettings).ProcessPreviewKey(e);
                    e.Handled = true;
                    return;
            }
        }

        protected override bool CanShowPopup
        {
            get { return false; }
        }
    }

    public class CustomDateEditStrategy : DateEditStrategy
    {
        public CustomDateEditStrategy(DateEdit editor)
            : base(editor)
        {
        }

        protected override bool AllowSpin
        {
            get { return false; }
        }
    }

    public class CustomTextInputMaskSettings : TextInputMaskSettings
    {
        public CustomTextInputMaskSettings(TextEditBase editor) : base(editor)
        {
        }

        public void ProcessPreviewKey(KeyEventArgs e)
        {
            base.ProcessPreviewKeyDown(e);
        }

        protected override void ProcessPreviewKeyDown(KeyEventArgs e)
        {

        }
    }
}
