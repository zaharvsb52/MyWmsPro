using System;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Validation;
using wmsMLC.RCL.Resources;

namespace wmsMLC.RCL.General.ViewModels
{
    public class EditViewModelBase<TModel> : SourceViewModelBase<TModel>
    {
        public const string IsDirtyCaptionPref = "*";

        protected virtual bool RejectChanges()
        {
            try
            {
                WaitStart();

                var editable = Source as IEditable;
                if (editable == null)
                    throw new DeveloperException(DeveloperExceptionResources.CantRejectChangesInNonEditableObject);

                editable.RejectChanges();
                return true;
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex, ExceptionResources.ItemCantReject);
                return false;
            }
            finally
            {
                WaitStop();
            }
        }

        protected virtual void OnIsDirtyChanged(IEditable eo)
        {
            var currentCaption = string.IsNullOrEmpty(PanelCaption) ? string.Empty : PanelCaption;

            if (eo.IsDirty)
            {
                if (!currentCaption.StartsWith(IsDirtyCaptionPref))
                    currentCaption = IsDirtyCaptionPref + currentCaption;
            }
            else
                if (currentCaption.StartsWith(IsDirtyCaptionPref))
                    currentCaption = currentCaption.Substring(IsDirtyCaptionPref.Length, currentCaption.Length - IsDirtyCaptionPref.Length);

            PanelCaption = currentCaption;
        }

        protected override void SourceObjectPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.SourceObjectPropertyChanged(sender, e);

            var eo = Source as IEditable;
            if (eo == null)
                return;

            if (e.PropertyName == EditableBusinessObject.IsDirtyPropertyName)
                OnIsDirtyChanged(eo);
        }

        protected override void OnSourceChanged()
        {
            base.OnSourceChanged();

            var validatable = Source as IValidatable;
            if (validatable != null)
                validatable.Validate();

            var eo = Source as IEditable;
            if (eo != null)
                OnIsDirtyChanged(eo);
        }
    }
}
