using System;
using System.Collections.Specialized;
using System.ComponentModel;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL;

namespace wmsMLC.DCL.General.ViewModels
{
    public abstract class SourceViewModelBase<TSourceType> : PanelViewModelBase, IModelHandler
    {
        #region .  Constants & Fields  .
        public const string SourcePropertyName = "Source";

        private TSourceType _source;
        #endregion

        protected SourceViewModelBase()
        {
            ((IModelHandler)this).DisplaySetting = SettingDisplay.Detail;
            FillCrudRights();
        }

        #region .  Properties  .
        public TSourceType Source
        {
            get { return _source; }
            set
            {
                try
                {
                    OnSourceChanging(value);
                    OnSourceUpdateStarted();

                    UnSubscribeSource();
                    _source = value;
                    SubscribeSource();

                    OnPropertyChanged(SourcePropertyName);
                }
                finally
                {
                    OnSourceUpdateCompleted();
                    RiseCommandsCanExecuteChanged();
                    OnSourceChanged();
                    //GC.Collect();
                }
            }
        }

        private void OnCollectionChangedInternal(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsInSuspendChanges)
                return;

            SourceCollectionChanged(sender, e);
        }

        private void OnPropertyChangedInternal(object sender, PropertyChangedEventArgs e)
        {
            if (IsInSuspendChanges)
                return; 
            
            SourceObjectPropertyChanged(sender, e);
        }

        public event EventHandler SourceUpdateStarted;
        public event EventHandler SourceUpdateCompleted;
        public event EventHandler RefreshViewEvent;

        public bool IsReadEnable { get; private set; }
        public bool IsEditEnable { get; private set; }
        public bool IsNewEnable { get; private set; }
        public bool IsDelEnable { get; private set; }

        public bool IsInSuspendChanges { get; private set; }
        #endregion

        #region .  Methods  .
        protected override void InitializeSettings()
        {
            base.InitializeSettings();

            IsMenuEnable = true;
            IsCustomizeBarEnabled = true;
            IsContextMenuEnable = true;
        }

        protected void OnSourceUpdateStarted()
        {
            IsInSuspendChanges = true;
            var h = SourceUpdateStarted;
            if (h != null)
                h(this, EventArgs.Empty);
        }
        protected void OnSourceUpdateCompleted()
        {
            IsInSuspendChanges = false;
            var h = SourceUpdateCompleted;
            if (h != null)
                h(this, EventArgs.Empty);
        }

        public void RefreshView()
        {
            var h = RefreshViewEvent;
            if (h != null)
                h(this, EventArgs.Empty);
        }

        protected virtual void OnSourceChanging(TSourceType newValue) { }
        protected virtual void OnSourceChanged() { }
        protected virtual void SourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) { }
        protected virtual void SourceObjectPropertyChanged(object sender, PropertyChangedEventArgs e) { }

        private void FillCrudRights()
        {
            IsReadEnable = Check(SecurityManager<string, int>.ReadRightName);
            IsEditEnable = Check(SecurityManager<string, int>.UpdateRightName);
            IsNewEnable = Check(SecurityManager<string, int>.CreateRightName);
            IsDelEnable = Check(SecurityManager<string, int>.DeleteRightName);
        }

        protected override Type GetSecurityType()
        {
            return typeof (TSourceType);
        }
        #endregion

        #region . IModelHandler .
        object IModelHandler.GetSource()
        {
            return Source;
        }

        void IModelHandler.SetSource(object source)
        {
            OnSetSource(source);
        }

        protected virtual void OnSetSource(object source)
        {
            if (source == null)
            {
                Source = default(TSourceType);
                return;
            }

            if (source is TSourceType)
                Source = (TSourceType)source;
            else
                throw new DeveloperException("Try to set source with unknown type. Waiting for {0}, but received {1}", typeof(TSourceType), source.GetType()); 
        }

        public virtual void RefreshData() { }

        public virtual void RefreshDataAsync() { }

        object IModelHandler.ParentViewModelSource { get; set; }

        SettingDisplay IModelHandler.DisplaySetting { get; set; }
        #endregion . IModelHandler .

        #region . IDisposable .
        protected override void Dispose(bool disposing)
        {
            UnSubscribeSource();
            base.Dispose(disposing);
        }

        protected virtual void SubscribeSource()
        {
            if (Source == null)
                return;

            var no = Source as INotifyPropertyChanged;
            if (no != null)
                no.PropertyChanged += OnPropertyChangedInternal;

            var clnChanged = Source as INotifyCollectionChanged;
            if (clnChanged != null)
                clnChanged.CollectionChanged += OnCollectionChangedInternal;
        }

        protected virtual void UnSubscribeSource()
        {
            if (Source == null)
                return;

            var no = Source as INotifyPropertyChanged;
            if (no != null)
                no.PropertyChanged -= OnPropertyChangedInternal;

            var clnChanged = Source as INotifyCollectionChanged;
            if (clnChanged != null)
                clnChanged.CollectionChanged -= OnCollectionChangedInternal;
        }
        #endregion
    }
}