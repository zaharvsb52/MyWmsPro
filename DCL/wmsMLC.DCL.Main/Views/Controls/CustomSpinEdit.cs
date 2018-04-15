using DevExpress.Xpf.Editors;

namespace wmsMLC.DCL.Main.Views.Controls
{
    public class CustomSpinEdit : SpinEdit
    {
        public bool NotAllowSpin { get; set; }

        protected override EditStrategyBase CreateEditStrategy()
        {
            return new CustomSpinEditStrategy(this);
        }
    }

    public class CustomSpinEditStrategy : SpinEditStrategy
    {
        public CustomSpinEditStrategy(TextEdit editor) : base(editor)
        {
        }

        protected override bool AllowSpin
        {
            get
            {
                var editor = Editor as CustomSpinEdit;
                return editor != null && !editor.NotAllowSpin;
            }
        }
    }
}
