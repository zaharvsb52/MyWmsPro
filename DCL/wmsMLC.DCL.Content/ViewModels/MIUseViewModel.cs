using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof (ObjectView))]
    public class MIUseViewModel : ObjectViewModelBase<MIUse>, ICustomFilterControlItemsProvider
    {
        private bool _isStrategyValueEnabled;
        private bool _isOrderTypeEnabled;
        private bool _isToCharEnabled;
        private bool _isGroupCodeEnabled;
        private bool _isFilterEnabled;
        private string _managerMisort = "MANAGER_MISORT";
        private string _managerMigroup = "MANAGER_MIGROUP";
        private string _managerMiGroupFilter = "MANAGER_MIGROUPFILTER";
        private string _managerMiGlobalFilter = "MANAGER_MIGLOBALFILTER";

        protected override void OnSave()
        {
            if (!CanSave() || Source == null)
                return;

            if (!CheckStrategy())
                return;

            base.OnSave();
        }

        protected override void OnSaveAndClose()
        {
            if (!CanSave() || Source == null)
                return;

            if (!CheckStrategy())
                return;

            base.OnSaveAndClose();
        }

        private bool CheckStrategy()
        {
            var strat = new[] { _managerMiGroupFilter, _managerMiGlobalFilter };

            if (!(strat.Contains(Source.StrategyType)))
                return true;

            if ((string.IsNullOrEmpty(Source.GroupCode) || (string.IsNullOrEmpty(Source.MIUseStrategyValue) && string.IsNullOrEmpty(Source.Filter))) &&
                Source.StrategyType.Equals(_managerMiGroupFilter))
            {
                GetViewService()
                    .ShowDialog(StringResources.Error, StringResources.ErrorSaveMIUSE, MessageBoxButton.OK,
                        MessageBoxImage.Error, MessageBoxResult.Yes);
                return false;
            }

            if (string.IsNullOrEmpty(Source.MIUseStrategyValue) && string.IsNullOrEmpty(Source.Filter) && Source.StrategyType.Equals(_managerMiGlobalFilter))
            {
                GetViewService()
                    .ShowDialog(StringResources.Error, StringResources.ErrorSaveMIUSEFilter, MessageBoxButton.OK,
                        MessageBoxImage.Error, MessageBoxResult.Yes);
                return false;
            }
            return true;
        }

        protected override ObservableCollection<DataField> GetFields(SettingDisplay displaySetting)
        {
            var fields = base.GetFields(displaySetting);
            var fieldMiUseOrderType = fields.SingleOrDefault(p => p.Name.EqIgnoreCase(MIUse.MIUseOrderTypePropertyName));
            var fieldMiUseToChar = fields.SingleOrDefault(p => p.Name.EqIgnoreCase(MIUse.MIUseToCharPropertyName));
            var fieldMiUseStrategyValue = fields.SingleOrDefault(p => p.Name.EqIgnoreCase(MIUse.MIUseStrategyValuePropertyName));
            var fieldMiUseGroupCode = fields.SingleOrDefault(p => p.Name.EqIgnoreCase(MIUse.MIUSEGROUPCODEPropertyName));
            var fieldMiUseFilter = fields.SingleOrDefault(p => p.Name.EqIgnoreCase(MIUse.MIUseFilterPropertyName));

            if (fieldMiUseOrderType != null)
            {
                if (Source.StrategyType == _managerMisort)
                    _isOrderTypeEnabled = true;

                fieldMiUseOrderType.IsEnabled = _isOrderTypeEnabled;
                if (!_isOrderTypeEnabled && !string.IsNullOrEmpty(Source.OrderType))
                    Source.OrderType = null;
            }

            if (fieldMiUseToChar != null)
            {
                if (Source.StrategyType == _managerMisort)
                    _isToCharEnabled = true;

                fieldMiUseToChar.IsEnabled = _isToCharEnabled;
                if (!_isToCharEnabled && Source.ToChar == true)
                    Source.ToChar = null;
            }

            if (fieldMiUseStrategyValue != null)
            {
                if (Source.StrategyType == _managerMiGroupFilter || Source.StrategyType == _managerMiGlobalFilter)
                    _isStrategyValueEnabled = true;

                fieldMiUseStrategyValue.IsEnabled = _isStrategyValueEnabled;
                if (!_isStrategyValueEnabled && !string.IsNullOrEmpty(Source.MIUseStrategyValue))
                    Source.MIUseStrategyValue = null;
            }

            if (fieldMiUseFilter != null)
            {
                if (Source.StrategyType == _managerMiGroupFilter || Source.StrategyType == _managerMiGlobalFilter)
                    _isFilterEnabled = true;

                fieldMiUseFilter.IsEnabled = _isFilterEnabled;
                if (!_isFilterEnabled && !string.IsNullOrEmpty(Source.Filter))
                    Source.Filter = null;
            }

            if (fieldMiUseGroupCode != null)
            {
                if (Source.StrategyType == _managerMiGroupFilter || Source.StrategyType == _managerMisort)
                    _isGroupCodeEnabled = true;

                fieldMiUseGroupCode.IsEnabled = _isGroupCodeEnabled;
                if (!_isGroupCodeEnabled && !string.IsNullOrEmpty(Source.GroupCode))
                    Source.GroupCode = null;
            }

            return fields;
        }

        public DataField[] GetFields()
        {
            var retDataFields = new List<DataField>();

            using (var soMgr = IoC.Instance.Resolve<ISysObjectManager>())
            {
                var sysfields =
                    soMgr.GetFiltered(
                        "objectentitycode in (select SYSCONFIG2OBJECT.OBJECTENTITYCODE_R from SYSCONFIG2OBJECT where upper(OBJECTCONFIGCODE_R) in ('MANAGER_MIGLOBALFILTER','MANAGER_MIGROUP','MANAGER_MISORT') and sysobject.objectname = SYSCONFIG2OBJECT.OBJECTNAME_R)");

                foreach (var sysfield in sysfields)
                {
                    var fieldName = sysfield.GetProperty("OBJECTDBNAME").ToString();
                    var entName = sysfield.GetProperty("OBJECTENTITYCODE").ToString();
                    var entType = soMgr.GetTypeByName(entName);
                    var prop = TypeDescriptor.GetProperties(entType);
                    var propType = prop.Find(fieldName, true).PropertyType;

                    var nDataField = new DataField()
                    {
                        Name = String.Format("{0}.{1}", entName, fieldName),
                        SourceName = String.Format("{0}.{1}", entName, fieldName),
                        FieldType = propType,
                        Caption =
                            String.Format("[{0}] {1}", entType.GetDisplayName(), prop.Find(fieldName, true).DisplayName)
                    };
                    retDataFields.Add(nDataField);
                }
            }

            return retDataFields.ToArray();
        }

        protected override void SourceObjectPropertyChanged(object sender,
            System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.SourceObjectPropertyChanged(sender, e);
            bool changed = false;

            if (!e.PropertyName.EqIgnoreCase(MIUse.MIUSESTRATEGYTYPEPropertyName))
                return;
            
            var oldValue = _isOrderTypeEnabled;
            _isOrderTypeEnabled = Source.StrategyType == _managerMisort;
            if (oldValue != _isOrderTypeEnabled)
                changed = true;

            oldValue = _isToCharEnabled;
            _isToCharEnabled = Source.StrategyType == _managerMisort;
            if (oldValue != _isToCharEnabled)
                changed = true;

            oldValue = _isStrategyValueEnabled;
            _isStrategyValueEnabled = (Source.StrategyType == _managerMiGroupFilter || Source.StrategyType == _managerMiGlobalFilter);
            if (oldValue != _isStrategyValueEnabled)
                changed = true;

            oldValue = _isFilterEnabled;
            _isFilterEnabled = (Source.StrategyType == _managerMiGroupFilter || Source.StrategyType == _managerMiGlobalFilter);
            if (oldValue != _isFilterEnabled)
                changed = true;

            oldValue = _isGroupCodeEnabled;
            _isGroupCodeEnabled = (Source.StrategyType == _managerMiGroupFilter || Source.StrategyType == _managerMisort);
            if (oldValue != _isGroupCodeEnabled)
                changed = true;
            
            if (changed)
                RefreshView();
        }
    }
}