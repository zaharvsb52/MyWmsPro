using System;
using System.ComponentModel;
using wmsMLC.DCL.Resources;

namespace wmsMLC.DCL.General.ViewModels
{
    public class CustomParamValueViewModel : ExpandoObjectValidateViewModel, ICPV
    {
        public event EventHandler<ValidateEventArgs> Validating;

        protected override void InitializeSettings()
        {
            base.InitializeSettings();

            IsMenuEnable = false;
            IsCustomizeBarEnabled = false;
        }

        public override bool DoAction()
        {
            try
            {
                WaitStart();

                if (!base.DoAction())
                    return false;

                var e = new ValidateEventArgs();
                OnValidating(e);
                if (e.Cancel)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                if (!ExceptionHandler(ex, ExceptionResources.ItemCantSave))
                    throw;
                return false;
            }
            finally
            {
                WaitStop();
            }
        }

        private void OnValidating(ValidateEventArgs e)
        {
            var handler = Validating;
            if (handler != null)
                handler(this, e);
        }
    }

    /// <summary>
    /// Интерфейс, VM является CPV
    /// </summary>
    public interface ICPV
    {
    }

    public class ValidateEventArgs : CancelEventArgs
    {
    }
}
