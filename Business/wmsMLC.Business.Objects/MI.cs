using System;

namespace wmsMLC.Business.Objects
{
    public class MI : WMSBusinessObject
    {
        #region .  Constants  .
        public const string MIINVTYPEPropertyName = "MIINVTYPE";
        public const string MILINEPropertyName = "MILINE";
        #endregion .  Constants  .

        #region .  Properties  .

        public string MIInvType
        {
            get { return GetProperty<string>(MIINVTYPEPropertyName); }
            set { SetProperty(MIINVTYPEPropertyName, value); }
        }


        public decimal? MILine
        {
            get { return GetProperty<decimal?>(MILINEPropertyName); }
            set { SetProperty(MILINEPropertyName, value); }
        }

        public InvTypeEnum InvType
        {
            get
            {
                var prop = GetProperty<string>(MIINVTYPEPropertyName);
                return (InvTypeEnum)Enum.Parse(typeof(InvTypeEnum), prop);
            }
            set { SetProperty(MIINVTYPEPropertyName, value.ToString()); }
        }
        #endregion .  Properties  .
    }
}