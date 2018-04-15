using System;
using System.Collections.Specialized;
using System.ComponentModel;
using wmsMLC.General;
using wmsMLC.General.BL.Validation;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Components.ViewModels;
using wmsMLC.General.PL.WPF.Components.ViewModels.RclMenu;

namespace wmsMLC.RCL.General.ViewModels
{
    public class SourceViewModelBase<TSourceType> : RclPanelViewModelBase, IModelHandler, IRclMenuHandler
    {
        #region .  Constants & Fields  .
        public const string SourcePropertyName = "Source";
        public const string IsCustomizationPropertyName = "IsCustomization";

        private static volatile Lazy<ISecurityChecker> _securityChecker = new Lazy<ISecurityChecker>(() => IoC.Instance.Resolve<ISecurityChecker>());

        private TSourceType _source;

        private bool _isCustomization;

        #endregion . Fields .

        public SourceViewModelBase()
        {
            ((IModelHandler)this).DisplaySetting = SettingDisplay.Detail;
        }

        #region .  Properties  .

        public TSourceType Source
        {
            get { return _source; }
            set
            {
                if (_source != null)
                {
                    var no = Source as INotifyPropertyChanged;
                    if (no != null)
                        no.PropertyChanged -= SourceObjectPropertyChanged;

                    var clnChanged = _source as INotifyCollectionChanged;
                    if (clnChanged != null)
                        clnChanged.CollectionChanged -= SourceCollectionChanged;
                }

                _source = value;

                if (_source != null)
                {
                    var no = Source as INotifyPropertyChanged;
                    if (no != null)
                        no.PropertyChanged += SourceObjectPropertyChanged;

                    var clnChanged = _source as INotifyCollectionChanged;
                    if (clnChanged != null)
                        clnChanged.CollectionChanged += SourceCollectionChanged;
                }

                OnPropertyChanged(SourcePropertyName);
                OnSourceChanged();
            }
        }

        /// <summary>
        /// Gets or sets whether customization mode of view is active. 
        /// </summary>
        public bool IsCustomization
        {
            get { return _isCustomization; }
            set
            {
                if (_isCustomization == value)
                    return;

                _isCustomization = value;
                OnPropertyChanged(IsCustomizationPropertyName);
            }
        }

        public virtual Action<ValueDataField> MenuAction { get; set; }
        #endregion

        #region .  Methods  .
        public object GetValidatorErrorsSource()
        {
            var valid = Source as IValidatable;
            if (valid == null)
                return null;
            valid.Validate();
            return valid.Validator.Errors;
        }

        protected virtual BarItem GetOrCreateBarItem(string caption, int? priorityForCreate = null)
        {
            //var bar = Menu.Bars.FirstOrDefault(p => p.Caption == caption);
            //if (bar == null)
            //{
            //    bar = new BarItem { Caption = caption, Priority = priorityForCreate.HasValue ? priorityForCreate.Value : Menu.Bars.Count };
            //    Menu.Bars.Add(bar);
            //}
            //return bar;
            return null;
        }

        protected virtual void OnSourceChanged() { }

        protected virtual void SourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) { }

        protected virtual void SourceObjectPropertyChanged(object sender, PropertyChangedEventArgs e) { }

        protected virtual bool Check(string actionName)
        {
            if (_securityChecker.Value == null)
                return false;

            var entityAction = String.Format("{0}.{1}", GetSecurityType().Name, actionName);
            return _securityChecker.Value.Check(entityAction);
        }

        /// <summary>
        /// Возвращает тип сущности для получения права.
        /// Если TSourceType в наследуемом классе отличается то TModel, то надо override этот метод.
        /// </summary>
        /// <returns>Тип сущности</returns>
        protected virtual Type GetSecurityType()
        {
            return typeof(TSourceType);
        }
        #endregion . Methods .

        #region . IModelHandler .
        object IModelHandler.GetSource()
        {
            return GetSourceInternal();
        }

        protected virtual object GetSourceInternal()
        {
            return Source;
        }

        void IModelHandler.SetSource(object source)
        {
            if (source == null)
            {
                Source = default(TSourceType);
                return;
            }

            if (source is TSourceType)
                Source = (TSourceType)source;
            else
                throw new DeveloperException("Try to set source with unknown type. Waiting for {0}, but received {1}.", typeof(TSourceType), source.GetType());
        }

        public virtual ValueDataField[] GetDialogFields()
        {
            return null;
        }

        object IModelHandler.ParentViewModelSource { get; set; }
        SettingDisplay IModelHandler.DisplaySetting { get; set; }
        #endregion . IModelHandler .
    }
}