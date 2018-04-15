using System;
using wmsMLC.General.BL.Validation.Attributes;

namespace wmsMLC.Business.Objects
{
    public class TE : WMSBusinessObject
    {
        #region .  Constants  .

        public const string StatusCodePropertyName = "STATUSCODE_R";
        public const string CreatePlacePropertyName = "TECREATIONPLACE";
        public const string CurrentPlacePropertyName = "TECURRENTPLACE";
        public const string TETypeCodePropertyName = "TETYPECODE_R";
        public const string TEWeightPropertyName = "TEWEIGHT";
        public const string TELengthPropertyName = "TELENGTH";
        public const string TEWidthPropertyName = "TEWIDTH";
        public const string TEHeightPropertyName = "TEHEIGHT";
        public const string TEMaxWeightPropertyName = "TEMAXWEIGHT";
        public const string TECodePropertyName = "TECode";
        public const string TEPackStatusPropertyName = "TEPACKSTATUS";
        public const string TETareWeightPropertyName = "TETAREWEIGHT";
        public const string TECarrierBaseCodePropertyName = "TECARRIERBASECODE";
        public const string TECarrierStreakCodePropertyName = "TECARRIERSTREAKCODE";
        public const string MandantIdPropertyName = "MANDANTID";

        #endregion .  Constants  .

        #region .  Properties  .

        [ValidateParentReference]
        public string TECarrierStreakCode
        {
            get { return GetProperty<string>(TECarrierStreakCodePropertyName); }
            set { SetProperty(TECarrierStreakCodePropertyName, value); }
        }

        public string TECarrierBaseCode
        {
            get { return GetProperty<string>(TECarrierBaseCodePropertyName); }
            set { SetProperty(TECarrierBaseCodePropertyName, value); }
        }

        public string TECode
        {
            get { return GetProperty<string>(TECodePropertyName); }
            set { SetProperty(TECodePropertyName, value); }
        }

        public string TETypeCode
        {
            get { return GetProperty<string>(TETypeCodePropertyName); }
            set { SetProperty(TETypeCodePropertyName, value); }
        }

        public string StatusCode
        {
            get { return GetProperty<string>(StatusCodePropertyName); }
            set { SetProperty(StatusCodePropertyName, value); }
        }

        public TEStates Status
        {
            get
            {
                var status = GetProperty<string>(StatusCodePropertyName);
                return (TEStates) Enum.Parse(typeof (TEStates), status);
            }
            set { SetProperty(StatusCodePropertyName, value.ToString()); }
        }

        public TEPackStatus TEPackStatus
        {
            get
            {
                var st = GetProperty<string>(TEPackStatusPropertyName);
                return (TEPackStatus)Enum.Parse(typeof (TEPackStatus), st);
            }
            set
            {
                SetProperty(TEPackStatusPropertyName, value.ToString());
            }
        }

        public string CreatePlace
        {
            get { return GetProperty<string>(CreatePlacePropertyName); }
            set { SetProperty(CreatePlacePropertyName, value); }
        }

        public string CurrentPlace
        {
            get { return GetProperty<string>(CurrentPlacePropertyName); }
            set { SetProperty(CurrentPlacePropertyName, value); }
        }

        public decimal TEWeight
        {
            get { return GetProperty<decimal>(TEWeightPropertyName); }
            set { SetProperty(TEWeightPropertyName, value); }
        }

        public decimal TELength
        {
            get { return GetProperty<decimal>(TELengthPropertyName); }
            set { SetProperty(TELengthPropertyName, value); }
        }

        public decimal TEWidth
        {
            get { return GetProperty<decimal>(TEWidthPropertyName); }
            set { SetProperty(TEWidthPropertyName, value); }
        }

        public decimal TEHeight
        {
            get { return GetProperty<decimal>(TEHeightPropertyName); }
            set { SetProperty(TEHeightPropertyName, value); }
        }

        public decimal TEMaxWeight
        {
            get { return GetProperty<decimal>(TEMaxWeightPropertyName); }
            set { SetProperty(TEMaxWeightPropertyName, value); }
        }

        public decimal? MandantId
        {
            get { return GetProperty<decimal?>(MandantIdPropertyName); }
            set { SetProperty(MandantIdPropertyName, value); }
        }

        #endregion .  Properties  .

        #region .  For Validation  .

        private bool _inPropertyChanged;

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            if (InSuspendNotifications)
                return;

            if (propertyName == TEWeightPropertyName || 
                propertyName == TELengthPropertyName ||
                propertyName == TEWidthPropertyName || 
                propertyName == TEHeightPropertyName ||
                propertyName == TEMaxWeightPropertyName || 
                propertyName == CurrentPlacePropertyName) return;

            if (_inPropertyChanged)
                return;

            try
            {
                _inPropertyChanged = true;
                base.OnPropertyChanged("Error");
            }
            finally
            {
                _inPropertyChanged = false;
            }
        }

        #endregion .  For Validation  .
    }
}
