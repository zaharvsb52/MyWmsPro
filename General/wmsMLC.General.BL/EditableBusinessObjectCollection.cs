using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using BLToolkit.Mapping;

namespace wmsMLC.General.BL
{
    public class EditableBusinessObjectCollection<T> : ValidatableObjectCollection<T>, IEditable, ISupportMapping
    {
        #region .  Constants  .

        private const string IsDirtyPropertyName = "IsDirty";

        #endregion

        #region .  Fields & Properties  .

        private readonly List<T> _insertItems = new List<T>();
        private readonly List<T> _removeItems = new List<T>();
        private bool _isDirty;
        private bool _isInRejectChanges;
        private bool _isInSuspendNotifications;

        public bool IsNew { get; private set; }

        #endregion

        #region .  IEditable  .

        public EditableBusinessObjectCollection()
        {
            IsNew = true;
        }

        public EditableBusinessObjectCollection(IEnumerable<T> collection)
            : base(collection)
        {
            IsNew = true;

            foreach (var item in this)
            {
                var notifyItem = item as INotifyPropertyChanged;
                if (notifyItem != null)
                {
                    notifyItem.PropertyChanged -= Item_PropertyChanged;
                    notifyItem.PropertyChanged += Item_PropertyChanged;
                }
            }
        }

        public bool IsDirty
        {
            get { return _isDirty; }
            private set
            {
                if (_isDirty == value)
                    return;

                _isDirty = value;
                OnPropertyChanged(new PropertyChangedEventArgs(IsDirtyPropertyName));
            }
        }

        public void AcceptChanges(bool isNew = false)
        {
            IsNew = isNew;
            _insertItems.Clear();
            _removeItems.Clear();

            foreach (var item in this)
            {
                var edditableObject = item as IEditable;
                if (edditableObject != null)
                    edditableObject.AcceptChanges();
            }
            IsDirty = false;
        }

        public void AcceptChanges(string propertyName)
        {
            _insertItems.Clear();
            _removeItems.Clear();

            foreach (var item in this)
            {
                var edditableObject = item as IEditable;
                if (edditableObject != null)
                    edditableObject.AcceptChanges(true);
            }
            IsDirty = false;
        }

        public void RejectChanges()
        {
            _isInRejectChanges = true;
            try
            {
                foreach (var insert in _insertItems)
                    this.Remove(insert);

                foreach (var remove in _removeItems)
                    this.Add(remove);

                foreach (var item in this)
                {
                    var edditableObject = item as IEditable;
                    if (edditableObject != null)
                        edditableObject.RejectChanges();
                }
            }
            finally
            {
                _isInRejectChanges = false;
            }
            IsDirty = false;
        }

        [NotControlChanges]
        public bool IsInRejectChanges
        {
            get
            {
                return _isInRejectChanges;
            }
        }

        public bool GetPropertyIsDirty(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) || !propertyName.EqIgnoreCase("Count"))
                throw new NotImplementedException(propertyName);
            return (_insertItems.Count + _removeItems.Count) != 0;
        }

        #endregion

        #region .  ObservableCollection  .

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);

            if (_isInRejectChanges)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                    {
                        _insertItems.Add((T)item);
                        var notifyItem = item as INotifyPropertyChanged;
                        if (notifyItem != null)
                            notifyItem.PropertyChanged += Item_PropertyChanged;
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        _removeItems.Add((T)item);
                        var notifyItem = item as INotifyPropertyChanged;
                        if (notifyItem != null)
                            notifyItem.PropertyChanged -= Item_PropertyChanged;
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    //foreach (var item in e.OldItems)
                    //    if (_setItems.ContainsKey(e.OldStartingIndex))
                    //    {
                    //        var notifyOldItem = _setItems[e.OldStartingIndex] as INotifyPropertyChanged;
                    //        if (notifyOldItem != null)
                    //            notifyOldItem.PropertyChanged -= Item_PropertyChanged;
                    //        _setItems[e.OldStartingIndex] = (T) item;
                    //        var notifyNewItem = item as INotifyPropertyChanged;
                    //        if (notifyNewItem != null)
                    //            notifyNewItem.PropertyChanged += Item_PropertyChanged;
                    //    }
                    //    else
                    //    {
                    //        _setItems.Add(e.OldStartingIndex, (T) item);
                    //        var notifyItem = item as INotifyPropertyChanged;
                    //        if (notifyItem != null)
                    //            notifyItem.PropertyChanged += Item_PropertyChanged;
                    //    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    //foreach (var item in e.OldItems)
                    //{
                    //    _removeItems.Add(e.OldStartingIndex, (T) item);
                    //    var notifyItem = item as INotifyPropertyChanged;
                    //    if (notifyItem != null)
                    //    {
                    //        notifyItem.PropertyChanged -= Item_PropertyChanged;
                    //        notifyItem.PropertyChanged += Item_PropertyChanged;
                    //    }
                    //}
                    break;

                case NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException("TODO");
            }

            IsDirty = GetActualIsDirty();
        }

        protected virtual void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // NOTE: хоть событие и лежит в коллекции в sender укладывается объект, чье свойсво изменили
            if (ItemPropertyChanged != null)
                ItemPropertyChanged(sender, e);

            if (e.PropertyName == EditableBusinessObject.IsDirtyPropertyName)
                IsDirty = GetActualIsDirty();
        }

        private bool GetActualIsDirty()
        {
            // если что-то есть в коллекциях - сразу IsDirty
            if (_insertItems.Count > 0 || _removeItems.Count > 0)
                return true;

            // пробегаем по элементам - если в них есть IsDirty, значит мы тоже IsDiry
            foreach (var item in this)
            {
                var eo = item as IEditable;
                if (eo != null && eo.IsDirty)
                    return true;
            }

            return false;
        }

        public event PropertyChangedEventHandler ItemPropertyChanged;

        public void SuspendNotifications()
        {
            _isInSuspendNotifications = true;
        }

        public void ResumeNotifications()
        {
            _isInSuspendNotifications = false;
        }


        #endregion

        #region .  ISupportMapping  .
        void ISupportMapping.BeginMapping(BLToolkit.Reflection.InitContext initContext)
        {
            SuspendValidating();
        }

        void ISupportMapping.EndMapping(BLToolkit.Reflection.InitContext initContext)
        {
            AcceptChanges();
            ResumeValidating();
        }
        #endregion
    }
}
