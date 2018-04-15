using System;
using wmsMLC.General.BL.Validation;

namespace wmsMLC.Business.Objects
{
    public class ObjectValid : WMSBusinessObject
    {
        #region .  Constants  .
        public const string ObjectValidNamePropertyName = "ObjectValidName";
        public const string ObjectValidLevelPropertyName = "ObjectValidLevel";
        public const string ObjectValidMessagePropertyName = "ObjectValidMessage";
        public const string ObjectValidParametersPropertyName = "ObjectValidParameters";
        public const string ObjectValidValuePropertyName = "ObjectValidValue";
        public const string ObjectValidPriorityPropertyName = "ObjectValidPriority";
        public const string ObjectValidIdPropertyName = "OBJECTVALIDID";
        #endregion

        #region .  Properties  .

        public decimal ObjectValidId
        {
            get { return GetProperty<decimal>(ObjectValidIdPropertyName); }
            set { SetProperty(ObjectValidIdPropertyName, value); }
        }

        public string ObjectValidName
        {
            get { return GetProperty<string>(ObjectValidNamePropertyName); }
            set { SetProperty(ObjectValidNamePropertyName, value); }
        }

        public ValidateErrorLevel ObjectValidLevel
        {
            get
            {
                var str = GetProperty<string>(ObjectValidLevelPropertyName);
                return (ValidateErrorLevel)Enum.Parse(typeof(ValidateErrorLevel), str, true);
            }
            set { SetProperty(ObjectValidLevelPropertyName, value.ToString()); }
        }

        public string ObjectValidMessage
        {
            get { return GetProperty<string>(ObjectValidMessagePropertyName); }
            set { SetProperty(ObjectValidMessagePropertyName, value); }
        }

        public string ObjectValidParameters
        {
            get { return GetProperty<string>(ObjectValidParametersPropertyName); }
            set { SetProperty(ObjectValidParametersPropertyName, value); }
        }

        public string ObjectValidValue
        {
            get { return GetProperty<string>(ObjectValidValuePropertyName); }
            set { SetProperty(ObjectValidValuePropertyName, value); }
        }

        public decimal ObjectValidPriority
        {
            get { return GetProperty<decimal>(ObjectValidPriorityPropertyName); }
            set { SetProperty(ObjectValidPriorityPropertyName, value); }
        }

        #endregion
    }
}