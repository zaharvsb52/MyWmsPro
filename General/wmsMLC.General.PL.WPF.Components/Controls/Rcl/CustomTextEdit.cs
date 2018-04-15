using System;
using System.Windows.Input;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.EditStrategy;
using wmsMLC.General.PL.WPF.Components.Helpers;
using wmsMLC.General.PL.WPF.Components.Views;

namespace wmsMLC.General.PL.WPF.Components.Controls.Rcl
{
    public class CustomTextEdit : TextEdit, IRclUi
    {
        public event EventHandler ControlKeyDown;

        public CustomTextEdit()
        {
            IsMovedFocusOnNextControl = true;
        }

        /// <summary>
        /// Переходить ли на следующий контрол при нажатии на управляющую клавишу?
        /// </summary>
        public bool IsMovedFocusOnNextControl { get; set; }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            KeyHelper.PreviewKeyDown(this, e);
            base.OnPreviewKeyDown(e);
        }

        void IRclUi.RaiseControlKeyDownEvent()
        {
            var handler = ControlKeyDown;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        protected override EditStrategyBase CreateEditStrategy()
        {
            //return base.TextInputSettings.Return(x => x.CreateEditStrategy(), () => CreateEditStrategy(null));
            //return base.CreateEditStrategy();
            return new CustomTextEditStrategy(this);
        }
    }

    public class CustomTextEditStrategy : TextEditStrategy
    {
        public CustomTextEditStrategy(TextEditBase editor)
            : base(editor)
        {
        }

        protected override bool AllowSpin
        {
            get { return false; }
        }
    }

    public class CustomPasswordBoxEdit : PasswordBoxEdit, IRclUi
    {
        public event EventHandler ControlKeyDown;

        public CustomPasswordBoxEdit()
        {
            IsMovedFocusOnNextControl = true;
        }

        /// <summary>
        /// Переходить ли на следующий контрол при нажатии на управляющую клавишу?
        /// </summary>
        public bool IsMovedFocusOnNextControl { get; set; }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            KeyHelper.PreviewKeyDown(this, e);
            base.OnPreviewKeyDown(e);
        }

        void IRclUi.RaiseControlKeyDownEvent()
        {
            var handler = ControlKeyDown;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}