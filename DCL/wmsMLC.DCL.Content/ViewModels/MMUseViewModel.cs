using System.Collections.ObjectModel;
using System.Linq;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Validation;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof (ObjectView))]
    public class MMUseViewModel : ObjectViewModelBase<MMUse>
    {
        private string _currentLookupCode;
        private bool _isEnabled = true;
        private bool _requirement = true;

        protected override void SourceObjectPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.SourceObjectPropertyChanged(sender, e);

            if (Source == null)
                return;

            var editable = Source as IEditable;
            if (editable.IsInRejectChanges)
                return;

            if (!e.PropertyName.EqIgnoreCase(MMUse.MMUseStrategyPropertyName))
                return;

            var lookupcode = _currentLookupCode;
            var isEnabled = _isEnabled;
            var requirement = _requirement;

            GetLookupCode(null, ref _currentLookupCode, ref _isEnabled, ref _requirement);

            if (lookupcode == _currentLookupCode && isEnabled == _isEnabled && requirement == _requirement)
                return;

            if (Source.MMUseStrategyValue != null)
            {
                Source.MMUseStrategyValue = null;
                Source.AcceptChanges(MMUse.MMUseStrategyValuePropertyName);
            }

            ValidErrors();
            RefreshView();
        }

        protected override ObservableCollection<DataField> GetFields(SettingDisplay displaySetting)
        {
            var fields = base.GetFields(displaySetting);

            var fieldUseStrategyValue =
                fields.SingleOrDefault(p => p.Name.EqIgnoreCase(MMUse.MMUseStrategyValuePropertyName));
            if (fieldUseStrategyValue == null)
                return fields;

            fieldUseStrategyValue.IsChangeLookupCode = true;
            fieldUseStrategyValue.Set(ValueDataFieldConstants.NotUseLookupAttribute, true);
            GetLookupCode(fieldUseStrategyValue, ref _currentLookupCode, ref _isEnabled, ref _requirement);
            _currentLookupCode = fieldUseStrategyValue.LookupCode;
            return fields;
        }

        private bool IsSourceValid()
        {
            return Source != null && !string.IsNullOrEmpty(Source.MMUseStrategy);
        }

        private void GetLookupCode(DataField field, ref string lookupCode, ref bool isEnabled, ref bool requirement)
        {
            if (!IsSourceValid())
                return;

            switch (Source.UseStrategy)
            {
                case MovingUseStrategySysEnum.PLACE_FIX:
                case MovingUseStrategySysEnum.PLACE_FIX_LOAD:
                    lookupCode = "PLACE_PLACECODE";
                    isEnabled = true;
                    requirement = true;
                    break;
                case MovingUseStrategySysEnum.PLACE_FREE:
                case MovingUseStrategySysEnum.PLACE_FREE_LOAD:
                case MovingUseStrategySysEnum.PLACE_TE2TE:
                case MovingUseStrategySysEnum.PLACE_TE2TE_CREATE:
                case MovingUseStrategySysEnum.PLACE_PART_LOAD_ALL:
                case MovingUseStrategySysEnum.PLACE_PART_LOAD_FREE:
                case MovingUseStrategySysEnum.PLACE_PART_LOAD_BUSY:
                case MovingUseStrategySysEnum.PLACE_OWB_FREE:
                case MovingUseStrategySysEnum.PLACE_OWB_LOAD:
                case MovingUseStrategySysEnum.PLACE_OWB_NEAR:
                case MovingUseStrategySysEnum.PLACE_OWB_TE2TE:
                case MovingUseStrategySysEnum.PLACE_ROUTE_FREE:
                case MovingUseStrategySysEnum.PLACE_ROUTE_LOAD:
                case MovingUseStrategySysEnum.PLACE_ROUTE_NEAR:
                case MovingUseStrategySysEnum.PLACE_ROUTE_TE2TE:
                    lookupCode = "RECEIVEAREA_RECEIVEAREACODE";
                    isEnabled = true;
                    requirement = true;
                    break;
                case MovingUseStrategySysEnum.PLACE_ROUTEGATE_FREE:
                case MovingUseStrategySysEnum.PLACE_ROUTEGATE_LOAD:
                case MovingUseStrategySysEnum.PLACE_ROUTEGATE_TE2TE:
                case MovingUseStrategySysEnum.PLACE_ROUTEGATE_TE2TE_CREATE:
                case MovingUseStrategySysEnum.PLACE_ITGATE_FREE:
                case MovingUseStrategySysEnum.PLACE_ITGATE_LOAD:
                case MovingUseStrategySysEnum.PLACE_ITGATE_TE2TE:
                case MovingUseStrategySysEnum.PLACE_ITGATE_TE2TE_CREATE:
                    lookupCode = null;
                    isEnabled = false;
                    requirement = false;
                    break;
                default:
                    throw new DeveloperException("Неизвестная стратегия '{0}'.", Source.UseStrategy);
            }

            if (field == null) 
                return;

            field.LookupCode = lookupCode;
            field.IsEnabled = isEnabled;
        }

        private void ValidErrors()
        {
            if (_requirement && Source.MMUseStrategyValue == null)
                Source.Validator.Errors.Add(MMUse.MMUseStrategyValuePropertyName,
                    new ValidateError("Поле должно быть заполнено", ValidateErrorLevel.Critical));
            else
            {
                Source.Validator.Errors.Remove(MMUse.MMUseStrategyValuePropertyName);
            }
        }
    }
}