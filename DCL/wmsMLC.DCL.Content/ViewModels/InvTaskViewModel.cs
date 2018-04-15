using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Validation;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof (ObjectView))]
    public class InvTaskViewModel : ObjectViewModelBase<InvTask>
    {
        private List<String> _saveConfigs;

        protected override ObservableCollection<DataField>
            GetFields(SettingDisplay displaySetting)
        {
            var fields = base.GetFields(displaySetting);
            if (!Source.GetProperty<bool>(InvTask.INVTASKMANUALPropertyName))
            {
                foreach (var f in fields)
                    if (!f.FieldName.Equals(InvTask.INVTASKCOUNTPropertyName))
                        f.EnableEdit = false;
            }

            if (_saveConfigs == null) return fields;
            foreach (var pmConfig in _saveConfigs)
            {
                var modelField =
                    fields.SingleOrDefault(p => p.Name.EqIgnoreCase(pmConfig));
                if (modelField != null)
                    modelField.IsChangeLookupCode = true;
            }


            return fields;
        }

        protected override void SourceObjectPropertyChanged(object sender,
            PropertyChangedEventArgs e)
        {
            base.SourceObjectPropertyChanged(sender, e);

            if (Source == null)
                return;

            var editable = Source as IEditable;
            if (editable.IsInRejectChanges)
                return;

            if (!e.PropertyName.EqIgnoreCase(InvTask.SKUID_RPropertyName))
                return;

            if (Source.SKUID_R == null)
            {
                Source.INVTASKCOUNT2SKU = null;
            }
            else
            {
                var mgr = IoC.Instance.Resolve<IBaseManager<SKU>>();
                var sku = mgr.Get(Source.SKUID_R);
                if (sku != null)
                {
                    Source.INVTASKCOUNT2SKU = sku.SKUCount;
                }
            }

            ValidErrors();
            RefreshView();
        }

        protected override void OnSave()
        {
            if (!CanSave() || Source == null)
                return;

            if (!CheckMustSet(Source.SKUID_R))
                return;

            base.OnSave();
        }

        protected override void OnSaveAndClose()
        {
            if (!CanSave() || Source == null)
                return;

            if (!CheckMustSet(Source.SKUID_R))
                return;

            base.OnSaveAndClose();
        }

        private bool CheckMustSet(decimal? sku)
        {
            var mustSetNameList = new List<string>();

            if (sku == null)
                return true;

            var pmConfigs = GetMustSetFields(sku.Value);
            var fields = base.GetFields(SettingDisplay.Detail);

            foreach (var pmConfig in pmConfigs)
            {
                if (Source.GetProperty(pmConfig) != null) continue;
                var firstOrDefault = fields.FirstOrDefault(o => o.SourceName == pmConfig);
                if (firstOrDefault != null)
                {
                    mustSetNameList.Add(firstOrDefault.Caption);
                }
            }

            if (!mustSetNameList.Any()) return true;
            GetViewService()
                .ShowDialog(StringResources.Error,
                    string.Format(StringResources.ErrorSaveShouldNotBeEmpty, string.Join(",", mustSetNameList)),
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
            return false;
        }

        private void ValidErrors()
        {
            var skuId = Source.SKUID_R;

            if (skuId == null)
            {
              if (_saveConfigs == null)  return;

                foreach (var saveConfig in _saveConfigs)
                {
                    Source.Validator.Errors.Remove(saveConfig);
                }
                return;
            }
                
            var pmConfigs = GetMustSetFields(skuId.Value);
            if (_saveConfigs == null)
                _saveConfigs = new List<string>();

            //delete and update old validators
            if (_saveConfigs != null)
            {
                foreach (var saveConfig in _saveConfigs)
                {
                    Source.Validator.Errors.Remove(saveConfig);
                }
            }

            //update new validators
            foreach (var pmConfig in pmConfigs)
            {
                if (Source.GetProperty(pmConfig) == null)
                {
                    if (!_saveConfigs.Contains(pmConfig))
                    _saveConfigs.Add(pmConfig);
                    Source.Validator.Errors.Add(pmConfig,
                        new ValidateError("Поле должно быть заполнено", ValidateErrorLevel.Critical));
                }
            }
        }

        private IEnumerable<String> GetMustSetFields(decimal skuId)
        {
            SKU sku;
            using (var skuMgr = IoC.Instance.Resolve<IBaseManager<SKU>>())
            {
                sku = skuMgr.Get(skuId, GetModeEnum.Partial);
            }

            using (var mgr = IoC.Instance.Resolve<IBPProcessManager>())
            {
                var pmConfigLst = mgr.GetPMConfigByParamListByArtCode(sku.ArtCode, "OP_INV_CREATE", "MUST_SET");
                return !pmConfigLst.Any()
                    ? new string[] {}
                    : pmConfigLst.Select(i => i.ObjectName_r.Replace("PRODUCT", "INVTASK"));
            }
        }
    }
}