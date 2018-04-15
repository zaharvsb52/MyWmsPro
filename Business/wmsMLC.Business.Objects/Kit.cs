using System;

namespace wmsMLC.Business.Objects
{
    public class Kit : WMSBusinessObject
    {
        #region .  Constants  .

        public const string KitCodePropertyName = "KITCODE";
        public const string KitTypeCodeRPropertyName = "KITTYPECODE_R";
        public const string ArtCodeRPropertyName = "ARTCODE_R";
        public const string KitPriorityInPropertyName = "KITPRIORITYIN";
        public const string KitPriorityOutPropertyName = "KITPRIORITYOUT";
        public const string MANDANTIDPropertyName = "MANDANTID";

        #endregion

        #region .  Properties  .

        public string KitCode
        {
            get { return GetProperty<string>(KitCodePropertyName); }
            set { SetProperty(KitCodePropertyName, value); }
        }
        public string KitTypeCodeR
        {
            get { return GetProperty<string>(KitTypeCodeRPropertyName); }
            set { SetProperty(KitTypeCodeRPropertyName, value); }
        }
        public string ArtCodeR
        {
            get { return GetProperty<string>(ArtCodeRPropertyName); }
            set { SetProperty(ArtCodeRPropertyName, value); }
        }
        public decimal KitPriorityIn
        {
            get { return GetProperty<decimal>(KitPriorityInPropertyName); }
            set { SetProperty(KitPriorityInPropertyName, value); }
        }
        public decimal KitPriorityOut
        {
            get { return GetProperty<decimal>(KitPriorityOutPropertyName); }
            set { SetProperty(KitPriorityOutPropertyName, value); }
        }
        public Decimal? MANDANTID
        {
            get { return GetProperty<Decimal?>(MANDANTIDPropertyName); }
            set { SetProperty(MANDANTIDPropertyName, value); }
        }

        #endregion
    }
}