using System;
using System.Collections.ObjectModel;
using System.Linq;
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
using System.Windows;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectView))]
    public class MRUseViewModel : ObjectViewModelBase<MRUse>
    {
        private bool _requirement;
        private string _fieldCaption;
        private bool _isLimit;

        protected override void SourceObjectPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.SourceObjectPropertyChanged(sender, e);

            if (Source == null)
                return;

            var editable = Source as IEditable;
            if (editable.IsInRejectChanges)
                return;

            if (!e.PropertyName.EqIgnoreCase(MRUse.MRUseStrategyValuePropertyName) &&
                !e.PropertyName.EqIgnoreCase(MRUse.MRUseStrategyTypePropertyName) &&
                !e.PropertyName.EqIgnoreCase(MRUse.MRUseStrategyPropertyName))
                return;

            if (e.PropertyName.EqIgnoreCase(MRUse.MRUseStrategyValuePropertyName))
            {
                if (!_requirement || Source.MRUseStrategyValue != null)
                    return;

                ValidErrors();
                return;
            }

            if (e.PropertyName.EqIgnoreCase(MRUse.MRUseStrategyTypePropertyName) && Source.MRUseStrategy != null)
            {
                Source.MRUseStrategy = null;
                Source.AcceptChanges(MRUse.MRUseStrategyPropertyName);
            }

            if (Source.MRUseStrategyValue != null)
            {
                Source.MRUseStrategyValue = null;
                Source.AcceptChanges(MRUse.MRUseStrategyValuePropertyName);
            }
            RefreshView();
        }

        private bool IsSourceValid()
        {
            return Source != null && !string.IsNullOrEmpty(Source.MRUseStrategyType) && !string.IsNullOrEmpty(Source.MRUseStrategy);
        }

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
            if (_requirement && string.IsNullOrEmpty(Source.MRUseStrategyValue))
            {
                GetViewService().ShowDialog(StringResources.Error, string.Format(StringResources.ErrorSaveShouldNotBeEmpty, _fieldCaption), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                return false;
            }

            if (!_isLimit)
                return true;

            decimal count;
            if (Decimal.TryParse(Source.MRUseStrategyValue, out count))
            {
                if (count < 1)
                {
                    GetViewService().ShowDialog(StringResources.Error, string.Format(StringResources.StyleOptionThresholdError, _fieldCaption), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                    return false;
                }

                if (count > 100)
                {
                    GetViewService().ShowDialog(StringResources.Error, string.Format(StringResources.StyleOptionMaxThresholdError, _fieldCaption, 100), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                    return false;
                }

                return true;
            }
            else
            {
                GetViewService().ShowDialog(StringResources.Error, string.Format(StringResources.StyleOptionTypeNotDecimal, _fieldCaption), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                return false;
            }
        }

        protected override ObservableCollection<DataField> GetFields(SettingDisplay displaySetting)
        {
            var fields = base.GetFields(displaySetting);

            var fieldUseStrategyValue =
                fields.SingleOrDefault(p => Extensions.EqIgnoreCase(p.Name, MRUse.MRUseStrategyValuePropertyName));
            if (fieldUseStrategyValue == null)
                return fields;

            fieldUseStrategyValue.IsChangeLookupCode = true;
            if (string.IsNullOrEmpty(_fieldCaption))
                _fieldCaption = string.IsNullOrEmpty(fieldUseStrategyValue.Caption) ? fieldUseStrategyValue.SourceName : fieldUseStrategyValue.Caption;
            ChangeField(fieldUseStrategyValue);
            fieldUseStrategyValue.Set(ValueDataFieldConstants.NotUseLookupAttribute, true);
            return fields;
        }

        private void ChangeField(DataField field)
        {
            if (!IsSourceValid())
                return;

            _isLimit = false;

            switch (Source.UseStrategyType)
            {
                case MRUseStrategyTypeSysEnum.WHERE:
                {
                    switch (Source.UseStrategy)
                    {
                        case MRUseStrategySysEnum.TE_FULL:
                        case MRUseStrategySysEnum.TE_PARTIAL:
                            _requirement = false;
                            SetParam(field, null, false);
                            break;
                        case MRUseStrategySysEnum.TE_COMPLETE_MIN:
                        case MRUseStrategySysEnum.TE_COMPLETE_MAX:
                            _requirement = true;
                            _isLimit = true;
                            SetParam(field, null, true);
                            break;
                        case MRUseStrategySysEnum.SUPPLYAREA:
                            _requirement = true;
                            SetParam(field, null, true);
                            break;
                        case MRUseStrategySysEnum.OWNER:
                            _requirement = true;
                            SetParam(field, "MANDANT_MANDANTID", true);
                            break;
                        case MRUseStrategySysEnum.RESERVFULLTE:
                            _requirement = true;
                            SetParam(field, "ENUM_MR_TE", true);
                            break;
                        default:
                            throw new DeveloperException("Неизвестная стратегия '{0}'.", Source.MRUseStrategy);
                    }
                    break;
                }

                case MRUseStrategyTypeSysEnum.ORDER:
                {
                    switch (Source.UseStrategy)
                    {
                        case MRUseStrategySysEnum.COUNT_BEST:
                        case MRUseStrategySysEnum.COUNT_MAX:
                        case MRUseStrategySysEnum.COUNT_MIN:
                        case MRUseStrategySysEnum.COUNT_MAX_PLACE:
                        case MRUseStrategySysEnum.COUNT_MIN_PLACE:
                        case MRUseStrategySysEnum.TE_FULL:
                        case MRUseStrategySysEnum.TE_PARTIAL:
                        case MRUseStrategySysEnum.FEFO:
                        case MRUseStrategySysEnum.LEFO:
                        case MRUseStrategySysEnum.PLACESORTPICK:
                        case MRUseStrategySysEnum.MULTIPLICITY_BEST:
                            _requirement = false;
                            SetParam(field, null, false);
                            break;
                        case MRUseStrategySysEnum.FIFO:
                        case MRUseStrategySysEnum.LIFO:
                            _requirement = true;
                            SetParam(field, "ENUM_ART_FIFO", true);
                            break;

                        case MRUseStrategySysEnum.SUPPLYAREA:
                            _requirement = true;
                            SetParam(field, null, true);
                            break;

                        default:
                            throw new DeveloperException("Неизвестная стратегия '{0}'.", Source.MRUseStrategy);
                    }
                    break;
                }

                case MRUseStrategyTypeSysEnum.ACTION:
                {
                    switch (Source.UseStrategy)
                    {
                        case MRUseStrategySysEnum.ROUND_IGNORE:
                        case MRUseStrategySysEnum.ROUND_FIX:
                        case MRUseStrategySysEnum.ROUND_DOWN:
                        case MRUseStrategySysEnum.ROUND_UP:
                            _requirement = false;
                            SetParam(field, null, false);
                            break;
                        default:
                            throw new DeveloperException("Неизвестная стратегия '{0}'.", Source.MRUseStrategy);
                    }
                    break;
                }

                default:
                    throw new DeveloperException("Неизвестный тип стратегии '{0}'.", Source.MRUseStrategyType);
            }
        }

        private void ValidErrors()
        {
            if (_requirement && Source.MRUseStrategyValue == null)
                Source.Validator.Errors.Add(MRUse.MRUseStrategyValuePropertyName,
                    new ValidateError("Поле должно быть заполнено", ValidateErrorLevel.Critical));
            else
            {
                Source.Validator.Errors.Remove(MRUse.MRUseStrategyValuePropertyName);
            }
        }

        private void SetParam(DataField field, string lookupCode, bool isEnabled)
        {
            ValidErrors();

            field.LookupCode = lookupCode;
            field.IsEnabled = isEnabled;
        }
    }
}