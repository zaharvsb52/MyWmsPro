using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Validation;

namespace wmsMLC.DCL.General.ViewModels
{
    public abstract class EditViewModelBase<TModel> : SourceViewModelBase<TModel>, IPropertyEditHandler
    {
        public const string IsDirtyCaptionPref = "*";

        #region . Properties .

        protected IEnumerable<TModel> PropertyEditSource { get; set; }
        
        #endregion

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
                if (!ExceptionHandler(ex, ExceptionResources.ItemCantReject))
                    throw;
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

        protected override void SourceObjectPropertyChanged(object sender, PropertyChangedEventArgs e)
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
            var validatable = Source as IValidatable;
            if (validatable != null)
                validatable.Validate();

            var eo = Source as IEditable;
            if (eo != null)
                OnIsDirtyChanged(eo);

            base.OnSourceChanged();
        }

        #region . IPropertyEditHandler .

        public bool InPropertyEditMode { get; set; }

        public virtual void SetSource(IEnumerable source)
        {
            PropertyEditSource = source.Cast<TModel>();
            InPropertyEditMode = true;
            Source = CreatePropertyEditSource(PropertyEditSource);
            var currentCaption = string.IsNullOrEmpty(PanelCaption) ? string.Empty : PanelCaption;
            PanelCaption = string.Format("{0}: {1}", currentCaption, StringResources.GroupEdit);
        }

        public bool IsMergedPropery(string propertyName)
        {
            var editable = Source as IEditable;
            if (editable == null)
                throw new DeveloperException("source must implement IEditable");
            if (editable.GetPropertyIsDirty(propertyName))
                return true;
            return PropertyEditSource.Cast<WMSBusinessObject>().DistinctBy(i => i.GetProperty(propertyName)).ToArray().Length == 1;
        }
        #endregion

        #region . Methods .

        protected virtual IEnumerable<TModel> GetPropertyEditCollection(TModel source, IEnumerable<TModel> collection)
        {
            var editable = source as IEditable;
            if (editable == null)
                throw new DeveloperException("source must implement IEditable");
            var so = source as WMSBusinessObject;
            if (so == null)
                throw new DeveloperException("{0} is not WMSBusinessObject", Source.GetType());
            var properties = TypeDescriptor.GetProperties(source).Cast<PropertyDescriptor>().ToArray();
            var propertyEditCollection = collection as TModel[] ?? collection.ToArray();
            foreach (var item in propertyEditCollection)
            {
                var bo = item as WMSBusinessObject;
                if (bo == null)
                    throw new DeveloperException("{0} is not WMSBusinessObject", item.GetType());
                foreach (var p in properties)
                {
                    if (editable.GetPropertyIsDirty(p.Name))
                        bo.SetProperty(p.Name, so.GetProperty(p.Name));
                }
            }
            return propertyEditCollection;
        }

        protected virtual TModel CreatePropertyEditSource(IEnumerable<TModel> collection)
        {
            var items = collection.ToArray();
            var obj = (TModel)IoC.Instance.Resolve(typeof(TModel));            
            var bo = obj as WMSBusinessObject;
            if (bo == null)
                throw new DeveloperException("TModel is not WMSBusinessObject");
            var properties = TypeDescriptor.GetProperties(typeof(TModel)).Cast<PropertyDescriptor>().ToArray();
            bo.SuspendNotifications();
            foreach (PropertyDescriptor p in properties)
            {
                // игнорирует вложенные коллекции
                if (typeof(IList).IsAssignableFrom(p.PropertyType))
                    continue;
                //var dist = items.Cast<WMSBusinessObject>().DistinctBy(i => i.GetProperty(p.Name)).ToArray();
                var dist = BPH.GetDistinctPropertyByName(items, p.Name).Cast<WMSBusinessObject>().ToArray();
                if (dist.Length == 1)
                    bo.SetProperty(p.Name, dist[0].GetProperty(p.Name));
            }
            bo.ResumeNotifications();
            bo.AcceptChanges();
            return obj;
        }

        #endregion
    }
}
