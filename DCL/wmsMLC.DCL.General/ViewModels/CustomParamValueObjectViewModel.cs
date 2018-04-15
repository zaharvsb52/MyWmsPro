using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.General;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Converters;

namespace wmsMLC.DCL.General.ViewModels
{
    public class CustomParamValueObjectViewModel<T> : ObjectViewModelBase<T> where T : CustomParamValue
    {
        private string _cpvValuePropertyName;
        private const string MustSetCaption = "Обязательный параметр";
        private const string MustSetDescription = "Параметр должен иметь значение";
        private const string MustHaveCaption = "Обязательный для сущности";
        private const string MustHaveDescription = "Параметр должен быть задан";

        public override void InitializeMenus()
        {
        }

        private string GetCpvValuePropertyName()
        {
            if (string.IsNullOrEmpty(_cpvValuePropertyName))
                _cpvValuePropertyName = CpvHelper.GetPropertyName(typeof(T), CustomParamValue.CPVValuePropertyName);
            return _cpvValuePropertyName;
        }

        protected override ObservableCollection<DataField> GetFields(SettingDisplay displaySetting)
        {
            var result = base.GetFields(displaySetting);
            var field = result.SingleOrDefault(p => p.FieldName == GetCpvValuePropertyName());
            var countField = result.FirstOrDefault(p => p.FieldName == CpvHelper.GetPropertyName(typeof(T), CustomParamValue.VCUSTOMPARAMCOUNTPropertyName));
            var descField = result.FirstOrDefault(p => p.FieldName == CpvHelper.GetPropertyName(typeof(T), CustomParamValue.VCUSTOMPARAMDESCPropertyName));
            DataField mustSetField = null;
            DataField mustHaveField = null;
            result.Clear();
            
            if (field == null)
                throw new DeveloperException("Can't find item with FieldName '{0}' in Fields.", GetCpvValuePropertyName());

            field.IsChangeLookupCode = true;
            field.IsEnabled = false;

            if (Source != null && Source.Cp != null)
            {
                field.LookupCode = Source.Cp.ObjectlookupCode_R;
                field.DisplayFormat = Source.Cp.CustomParamFormat;
                var isnotlookup = string.IsNullOrEmpty(field.LookupCode);
                
                field.FieldType = CpvHelper.GetValueType(Source);
                field.IsEnabled = !(Source.Cp.CustomparamInputdisable || Source.Cp.IsReadOnly || field.FieldType == null);

                //Пост-обработка
                if (field.FieldType != null)
                {
                    var type = field.FieldType.GetNonNullableType();
                    if (type == typeof(DateTime))
                    {
                        field.Set(ValueDataFieldConstants.BindingIValueConverter, new StringToDateTimeConverter());
                        field.Set(ValueDataFieldConstants.Parameter, CpvHelper.GetDateTimeFormat());
                    }
                    else if (type == typeof (bool))
                    {
                        field.Set(ValueDataFieldConstants.BindingIValueConverter, new StringToDbBool());
                    }
                    else if (type.IsPrimitive || type == typeof(decimal))
                    {
                        field.Set(ValueDataFieldConstants.BindingIValueConverter, new StringToNumericConverter());
                        field.Set(ValueDataFieldConstants.Parameter, field.FieldType);
                        if (isnotlookup)
                            field.Set(ValueDataFieldConstants.UseSpinEdit, true);
                    }

                    //if (isnotlookup && string.IsNullOrEmpty(field.DisplayFormat) &&
                    //    ((type == typeof(float) || type == typeof(double) || type == typeof(decimal))))
                    //    field.DisplayFormat = "N4";
                }

                mustSetField = new DataField()
                {
                    Name = CustomParamValue.IsMustSetPropertyName,
                    FieldName = CustomParamValue.IsMustSetPropertyName,
                    SourceName = CustomParamValue.IsMustSetPropertyName,
                    IsEnabled = false,
                    Visible = true,
                    Caption = MustSetCaption,
                    Description = MustSetDescription,
                    FieldType = typeof(bool)
                };

                mustHaveField = new DataField()
                {
                    Name = CustomParamValue.IsMustHavePropertyName,
                    FieldName = CustomParamValue.IsMustHavePropertyName,
                    SourceName = CustomParamValue.IsMustHavePropertyName,
                    IsEnabled = false,
                    Visible = true,
                    Caption = MustHaveCaption,
                    Description = MustHaveDescription,
                    FieldType = typeof(bool)
                };
            }
            result.Add(field);
            if (countField != null)
            {
                countField.IsEnabled = false;
                result.Add(countField);
            }
            if (descField != null)
            {
                descField.IsEnabled = false;
                result.Add(descField);
            }
            if (mustSetField != null)
                result.Add(mustSetField);
            if (mustHaveField != null)
                result.Add(mustHaveField);
            return result;
        }

        protected override void SourceObjectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.SourceObjectPropertyChanged(sender, e);
            if (Source != null && e.PropertyName == GetCpvValuePropertyName())
            {
                if (string.IsNullOrEmpty(Source.CPVValue) && Source.Cp != null &&
                    !string.IsNullOrEmpty(Source.Cp.CustomParamDefault))
                {
                    Source.CPVValue = Source.Cp.CustomParamDefault;
                    return;
                }
                Source.FormattedValue = CpvHelper.GetFormattedValue(Source);
            }
            OnNeedRefresh();
        }

        protected override bool CanCloseInternal()
        {
            return true;
        }
    }
}
