using System;
using System.Globalization;
using System.Linq;
using wmsMLC.Business.Objects;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Validation;

namespace wmsMLC.Business.Managers.Validation
{
    public class PMConfigValidator : AbstractValidatorEngine
    {
        public const string MinMaxValidateMethodCode = "_$MinMaxMethod_";

        public PMConfig[] Attributes { get; set; }
    
        public PMConfigValidator(IValidatable parent) : base(parent)
        {
        }

        public new CustomExpandoObject ValidatableObject
        {
            get { return (CustomExpandoObject)base.ValidatableObject; }
        }

        protected override void ValidatableObject_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ValidatePMConfig(e.PropertyName);
            base.ValidatableObject_PropertyChanged(sender, e);
        }

        protected override void ValidateProperty_Internal(System.ComponentModel.PropertyDescriptor propertyInfo)
        {
            if (propertyInfo.Name.Equals("Members"))
            {
                foreach (var p in ValidatableObject.Members)
                    ValidatePMConfig(p.Key);
            }
            base.ValidateProperty_Internal(propertyInfo);
        }

        private void ValidatePMConfig(string propertyName)
        {
            if (Attributes != null)
            {
                var attrs = Attributes.Where(i => i.ObjectName_r.Equals(propertyName));
                Errors.Remove(propertyName);
                foreach (var a in attrs)
                {
                    switch (a.MethodCode_r)
                    {
                        case "MUST_SET":
                            if (ValidatableObject.Members[a.ObjectName_r] == null)
                                Errors.Add(a.ObjectName_r,
                                    new ValidateError("Поле должно быть заполнено (MUST_SET)", ValidateErrorLevel.Critical));
                            break;
                        case "MUST_COMPARE":
                            var prevValue = ValidatableObject.GetInitialValue(a.ObjectName_r);
                            if (prevValue != null)
                            {
                                var objType = prevValue.GetType();
                                var newValue =
                                    SerializationHelper.ConvertToTrueType(
                                        ValidatableObject.Members[a.ObjectName_r], objType);
                                if (!Equals(newValue, prevValue))
                                    Errors.Add(a.ObjectName_r,
                                        new ValidateError("Неверно указано значение (MUST_COMPARE)",
                                            ValidateErrorLevel.Critical));
                            }
                            break;
                        case "RANGE_WEEK":
                        case "RANGE_DAY":
                        case "RANGE_MONTH":
                        case "RANGE_YEAR":
                            var rangeValue = ValidatableObject.GetInitialValue(a.ObjectName_r);
                            if (rangeValue != null)
                            {
                                if (!RangeDate((DateTime)rangeValue, 
                                    (DateTime)SerializationHelper.ConvertToTrueType(ValidatableObject.Members[a.ObjectName_r], typeof(DateTime)),
                                    a.MethodCode_r))
                                    Errors.Add(a.ObjectName_r,
                                        new ValidateError(
                                            string.Format("Указан не допустимый диапазон ({0})", a.MethodCode_r),
                                            ValidateErrorLevel.Critical));
                            }
                            break;
                        case MinMaxValidateMethodCode:
                            if (a.ValidatorHandle != null)
                            {
                                var validError = a.ValidatorHandle(ValidatableObject.Members[a.ObjectName_r]);
                                if (!string.IsNullOrEmpty(validError))
                                    Errors.Add(a.ObjectName_r, new ValidateError(validError, ValidateErrorLevel.Critical));
                            }
                            break;
                    }
                }
            }
        }

        private static bool RangeDate(DateTime date1, DateTime date2, string method)
        {
            switch (method)
            {
                case "RANGE_YEAR":
                    return date1.Year == date2.Year;
                case "RANGE_MONTH":
                    return date1.Year == date2.Year 
                        && date1.Month == date2.Month;
                case "RANGE_WEEK":
                    var dfi = DateTimeFormatInfo.CurrentInfo;
                    if (dfi != null)
                    {
                        var cal = dfi.Calendar;
                        return date1.Year == date2.Year
                               && cal.GetWeekOfYear(date1, dfi.CalendarWeekRule, dfi.FirstDayOfWeek) == cal.GetWeekOfYear(date2, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
                    }
                    return false;
                case "RANGE_DAY":
                    return date1.Date == date2.Date;
                default:
                    return false;
            }
        }

    }
}
