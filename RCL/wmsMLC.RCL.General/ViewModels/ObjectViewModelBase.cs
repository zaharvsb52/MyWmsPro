using System;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using wmsMLC.Business.Managers.Expressions;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Validation;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.ViewModels;
using wmsMLC.RCL.Resources;

namespace wmsMLC.RCL.General.ViewModels
{
    public class ObjectViewModelBase<TModel> : EditViewModelBase<TModel>, IFormulaHandler, IObjectViewModel<TModel>
    {
        #region .  Const & Fields  .

        private IBaseManager<TModel> _managerCache;
        private ExpandoObject _formulaValues;

        #endregion

        public ObjectViewModelBase()
        {
            AllowClosePanel = true;
            //PanelCaption = typeof(TModel).Name;
            PanelCaption = typeof(TModel).GetDisplayName();
            RefreshCommand = new DelegateCustomCommand(this, RefreshData, CanRefreshData);
            SaveCommand = new DelegateCustomCommand(this, OnSave, CanSave);
            DeleteCommand = new DelegateCustomCommand(this, OnDelete, CanDelete);

            FormulaValues = new ExpandoObject();
        }

        #region .  Methods  .

        protected static bool CheckOnCriticalErrors(object obj)
        {
            var valid = obj as IValidatable;
            if (valid == null)
                return true;

            valid.Validate();

            // если критических ошибок нет - можно продолжать
            if (!valid.Validator.HasCriticalError())
                return true;

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

        protected virtual bool Save()
        {
            try
            {
                WaitStart();

                var mgr = GetManager();
                if (Source is ViewModelBase)
                    throw new DeveloperException(DeveloperExceptionResources.CantOperateWithViewmodel);

                var isNew = Source as IIsNew;
                if (isNew == null)
                    throw new DeveloperException(DeveloperExceptionResources.SourceMustImplementIIsNew);

                if (isNew.IsNew)
                {
                    var inFormulaMode = FormulaValues != null && FormulaValues.Any();
                    if (inFormulaMode)
                    {
                        var clonable = Source as ICloneable;
                        if (clonable == null)
                            throw new DeveloperException("Source must implement IClonable");

                        var items = ExpressionHelper.Process(clonable, new ExpressionContext(FormulaValues)).Cast<TModel>();

                        if (items.Any(i => !CheckOnCriticalErrors(i)))
                            return false;

                        mgr.Insert(ref items);
                        var count = items.Count();

                        // выводим сообщени о добавленых записях
                        GetViewService()
                            .ShowDialog(StringResources.Information, string.Format(StringResources.FormulaSaveItemsTemplateMessage, count),
                                        MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                    }
                    else
                    {
                        if (!CheckOnCriticalErrors(Source))
                            return false;

                        var item = Source;
                        mgr.Insert(ref item);
                        Source = item;
                    }

                    //FormulaValues = new ExpandoObject();
                }
                else
                {
                    if (!CheckOnCriticalErrors(Source))
                        return false;

                    mgr.Update(Source);
                    RefreshDataInternal(false);
                }

                return true;
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex, ExceptionResources.ItemCantSave);
                return false;
            }
            finally
            {
                WaitStop();
            }
        }

        protected virtual bool Delete()
        {
            try
            {
                WaitStart();

                var mgr = GetManager();
                if (Source is ViewModelBase)
                    throw new DeveloperException(DeveloperExceptionResources.CantOperateWithViewmodel);

                mgr.Delete(Source);

                return true;
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex, ExceptionResources.ItemCantDelete);
                return false;
            }
            finally
            {
                WaitStop();
            }
        }

        protected virtual IBaseManager<TModel> GetManager()
        {
            if (_managerCache != null)
                return _managerCache;

            _managerCache = IoC.Instance.Resolve<IBaseManager<TModel>>();
            return _managerCache;
        }

        #region .  Commands  .
        public ICommand RefreshCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICustomCommand SaveCommand { get; private set; }

        protected virtual bool CanDelete()
        {
            return Check(BusinessObjectManager<string, int>.DeleteMethodName);
        }
        protected virtual void OnDelete()
        {
            if (!CanDelete())
                throw new DeveloperException(DeveloperExceptionResources.CommandCanEditError);

            if (!DeleteConfirmation())
                return;

            if (Delete())
                DoCloseRequest();
        }

        protected virtual bool CanRefreshData()
        {
            var result = Check(BusinessObjectManager<string, int>.GetFilteredMethodName);
            if (!result) 
                return false;
            var iisnew = Source as IIsNew;
            if (iisnew == null) 
                return false;
            return !iisnew.IsNew;
        }

        protected void RefreshData()
        {
            RefreshDataInternal(true);
        }

        private void RefreshDataInternal(bool usewait)
        {
            if (!CanRefreshData())
                return;

            var editable = Source as IEditable;
            if (editable == null)
                throw new DeveloperException("Source is not IEditable.");

            if (editable.IsDirty && GetViewService().ShowDialog(StringResources.Confirmation
                    , StringResources.ConfirmationSaveDataOnRefresh
                    , MessageBoxButton.YesNo
                    , MessageBoxImage.Question
                    , MessageBoxResult.No) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                if (usewait)
                    WaitStart();

                RejectChanges();

                var mgr = GetManager();
                var keyHandler = Source as IKeyHandler;
                if (keyHandler == null)
                    throw new DeveloperException("Source is not IKeyHandler.");

                // очищаем кэш
                mgr.ClearCache();

                // получаем данные
                Source = mgr.Get(keyHandler.GetKey());

                // обновляем все связанные списки
                mgr.RiseManagerChanged();
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex, ExceptionResources.ItemCantRefresh);
            }
            finally
            {
                if (usewait)
                    WaitStop();
            }
        }

        protected virtual bool CanSave()
        {
            var ed = Source as IEditable;
            if (ed != null && !ed.IsDirty)
                return false;

            return Check(BusinessObjectManager<string, int>.UpdateMethodName);
        }

        protected virtual void OnSave()
        {
            if (!CanSave())
                throw new DeveloperException(DeveloperExceptionResources.CommandCanEditError);

            Save();
        }

        public virtual bool DoAction()
        {
            return CanSave() && Save();
        }

        public virtual ICommand DoActionCommand { get { return SaveCommand; } }

        protected virtual bool DeleteConfirmation()
        {
            var vs = GetViewService();
            var dr = vs.ShowDialog(StringResources.Confirmation
                , StringResources.ConfirmationDeleteObject
                , MessageBoxButton.YesNo //MessageBoxButton.YesNoCancel
                , MessageBoxImage.Question
                , MessageBoxResult.Yes);

            return dr == MessageBoxResult.Yes;
        }

        protected virtual bool CanCustomization()
        {
            return Check(CustomizationMethodName);
        }

        protected virtual void OnCustomization()
        {
            if (!CanCustomization())
                throw new DeveloperException(DeveloperExceptionResources.CommandCanEditError);
            IsCustomization = !IsCustomization;
        }
        #endregion

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
                    return Save();

                if (dr == MessageBoxResult.No)
                    return RejectChanges();

                throw new DeveloperException(DeveloperExceptionResources.UnknownDialogResult);
            }
            finally
            {
                WaitStop();
            }
        }

        protected override void OnIsDirtyChanged(IEditable eo)
        {
            base.OnIsDirtyChanged(eo);

            SaveCommand.RaiseCanExecuteChanged();
        }

        #endregion

        public ExpandoObject FormulaValues
        {
            get { return _formulaValues; }
            private set
            {
                _formulaValues = value;
                OnPropertyChanged("FormulaValues");
            }
        }

        protected override void OnSourceChanged()
        {
            base.OnSourceChanged();
            PanelCaption = (Source == null ? "NULL" : Source.ToString());
        }
    }

    public interface IFormulaHandler
    {
        ExpandoObject FormulaValues { get; }
    }
}