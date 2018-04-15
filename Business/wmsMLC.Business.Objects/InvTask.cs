namespace wmsMLC.Business.Objects
{
    public class InvTask : WMSBusinessObject
    {
        public const string INVTASKCOUNTPropertyName = "INVTASKCOUNT";
        public const string INVTASKMANUALPropertyName = "INVTASKMANUAL";
        public const string InvTaskIdPropertyName = "INVTASKID";
        public const string SKUID_RPropertyName = "SKUID_R";
        public const string INVTASKCOUNT2SKUPropertyName = "INVTASKCOUNT2SKU";
        public const string TETYPECODE_RPropertyName = "TETYPECODE_R";

        public decimal? InvTaskCount
        {
            get { return GetProperty<decimal?>(INVTASKCOUNTPropertyName); }
            set { SetProperty(INVTASKCOUNTPropertyName, value); }
        }

        public decimal InvTaskId
        {
            get { return GetProperty<decimal>(InvTaskIdPropertyName); }
            set { SetProperty(InvTaskIdPropertyName, value); }
        }

        public decimal? SKUID_R
        {
            get { return GetProperty<decimal?>(SKUID_RPropertyName); }
            set { SetProperty(SKUID_RPropertyName, value); }
        }

        public double? INVTASKCOUNT2SKU
        {
            get { return GetProperty<double?>(INVTASKCOUNT2SKUPropertyName); }
            set { SetProperty(INVTASKCOUNT2SKUPropertyName, value); }
        }

        public string TETYPECODE_R
        {
            get { return GetProperty<string>(TETYPECODE_RPropertyName); }
            set { SetProperty(TETYPECODE_RPropertyName, value); }
        }
    }
}