using System.Windows.Input;

namespace wmsMLC.General.PL.WPF.Components.Controls.Rcl
{
// ReSharper disable InconsistentNaming
    public class CustomDXDialog : CustomWindow
// ReSharper restore InconsistentNaming
    {
        #region .  Constructors  .
        public CustomDXDialog()
        {
            TrueResult = null;
        }
        #endregion .  Constructors  .

        public bool? TrueResult { get; private set; }

        public void Close(bool? dialogresult)
        {
            TrueResult = dialogresult;
            Close();
        }

        protected override void OnWindowClose(KeyEventArgs e)
        {
            if (e.Handled)
                return;

            switch (e.Key)
            {
                case Key.Enter:
                    e.Handled = true;
                    Close(true);
                    return;

                case Key.Escape:
                    e.Handled = true;
                    Close(false);
                    return;
            }
        }
    }
}