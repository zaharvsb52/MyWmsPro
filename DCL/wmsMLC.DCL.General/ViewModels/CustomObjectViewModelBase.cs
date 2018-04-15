using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using wmsMLC.Business.Managers.Validation;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Validation;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.DCL.General.ViewModels
{
    public class CustomObjectViewModelBase<TModel> : EditViewModelBase<TModel>, IObjectViewModel<TModel>, IHelpHandler
    {
        #region . Fields .
        
        #endregion

        public CustomObjectViewModelBase()
        {
            AllowClosePanel = true;
            PanelCaption = typeof(TModel).GetDisplayName();
        }

        #region .  Commands  .
        public virtual ICommand DoActionCommand { get; private set; }
        #endregion

        #region . IObjectViewModel & IHelpHandler .

        public ObjectViewModelMode Mode { get; set; }

        public virtual bool DoAction()
        {
            if (InPropertyEditMode)
            {
                var values = GetPropertyEditCollection(Source, PropertyEditSource);
                if (values.Any(item => !CheckOnCriticalErrors(item)))
                {
                    foreach (var editItem in PropertyEditSource)
                    {
                        var editableItem = editItem as IEditable;
                        if (editableItem == null)
                            throw new DeveloperException(DeveloperExceptionResources.CantRejectChangesInNonEditableObject);
                        editableItem.RejectChanges();
                    }
                    return false;
                }
                var editable = Source as IEditable;
                if (editable == null)
                    throw new DeveloperException(DeveloperExceptionResources.CantRejectChangesInNonEditableObject);
                editable.AcceptChanges();
                return true;
            }
            return CheckOnCriticalErrors(Source);
        }
        
        public string GetHelpLink()
        {
            return null;
        }

        public string GetHelpEntity()
        {
            return typeof(TModel).Name;
        }

        public virtual ObservableCollection<DataField> GetDataFields(SettingDisplay displaySetting)
        {
            return DataFieldHelper.Instance.GetDataFields(typeof(TModel), displaySetting);
        }

        public bool IsAdd { get; set; }
        public WMSBusinessObject SourceBase { get; set; }
        public bool IsNeedRefresh { get; set; }
        public event EventHandler<NotifyCollectionChangedEventArgs> CollectionChanged;
        public event EventHandler NeedRefresh;
        public virtual void InitializeMenus()
        {
            InitializeCustomizationBar();
            InitializeContextMenu();
        }

        #endregion

        #region . CanDelegate .
        
        protected override bool CanCustomization()
        {
            return IsCustomizeEnabled;
        }

        protected override bool CanCloseInternal()
        {
            var res = base.CanCloseInternal();
            if (!res)
                return false;

            var eo = Source as EditableBusinessObject;
            if (eo == null)
                return true;

            if (!eo.IsDirty)
                return true;

            try
            {
                WaitStart();

                var vs = GetViewService();
                var dr = vs.ShowDialog(StringResources.Confirmation
                    , StringResources.ConfirmationUnsavedData
                    , MessageBoxButton.YesNoCancel
                    , MessageBoxImage.Question
                    , MessageBoxResult.Yes);

                if (dr == MessageBoxResult.Cancel)
                    return false;

                if (dr == MessageBoxResult.Yes)
                {
                    var editable = Source as IEditable;
                    editable.AcceptChanges();
                    return true;
                }

                if (dr == MessageBoxResult.No)
                {
                    if (InPropertyEditMode)
                    {
                        foreach (var editItem in PropertyEditSource)
                        {
                            var editableItem = editItem as IEditable;
                            if (editableItem == null)
                                throw new DeveloperException(
                                    DeveloperExceptionResources.CantRejectChangesInNonEditableObject);
                            editableItem.RejectChanges();
                        }
                    }
                    return RejectChanges();
                }

                throw new DeveloperException(DeveloperExceptionResources.UnknownDialogResult);
            }
            finally
            {
                WaitStop();
            }
        }
        
        #endregion

        #region . Methods .

        protected override void OnSourceChanged()
        {
            base.OnSourceChanged();
            PanelCaption = (Source == null ? "NULL" : Source.ToString());
        }

        protected static bool CheckOnCriticalErrors(object obj)
        {
            var eo = obj as EditableBusinessObject;
            // будем выставлять дефолтное значение, если объект новый
            if (eo != null && eo.IsNew)
                DefaultValueSetter.Instance.SetDefaultValues((BusinessObject)obj);

            var valid = obj as IValidatable;
            if (valid == null)
                return true;

            valid.Validate();

            // если критических ошибок нет - можно продолжать
            if (!valid.Validator.HasCriticalError())
            {
                if (eo != null)
                    eo.AcceptChanges();
                return true;
            }

            var properties = TypeDescriptor.GetProperties(obj);
            var desc = new StringBuilder();
            foreach (var validateError in valid.Validator.Errors)
            {
                if (!validateError.Value.HasCriticalErrors)
                    continue;

                // получаем описание
                desc.AppendLine(properties[validateError.Key].DisplayName + ":");
                // получаем ошибки
                foreach (var v in validateError.Value)
                    desc.AppendLine("  - " + v.Description);
            }

            GetViewService().ShowDialog
                (StringResources.Error
                    , string.Format("{0}{1}{2}", StringResources.ErrorSave, Environment.NewLine, desc)
                    , MessageBoxButton.OK
                    , MessageBoxImage.Error
                    , MessageBoxResult.Yes);

            return false;
        }
        #endregion
    }
}
