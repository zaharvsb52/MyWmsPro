namespace wmsMLC.Business.Objects
{
    public class Truck : WMSBusinessObject
    {
        #region .  Constants  .
        public const string TruckTypeCode_RPropertyName = "TRUCKTYPECODE_R";
        public const string Truck2MotionAreaGroupLPropertyName = "TRUCK2MOTIONAREAGROUPL";
        public const string PLACECODE_RPropertyName = "PLACECODE_R";
        #endregion .  Constants  .

        #region .  Properties  .
        public string TruckTypeCode_R
        {
            get { return GetProperty<string>(TruckTypeCode_RPropertyName); }
            set { SetProperty(TruckTypeCode_RPropertyName, value); }
        }

        public WMSBusinessCollection<Truck2MotionAreaGroup> Truck2MotionAreaGroupL
        {
            get { return GetProperty<WMSBusinessCollection<Truck2MotionAreaGroup>>(Truck2MotionAreaGroupLPropertyName); }
            set { SetProperty(Truck2MotionAreaGroupLPropertyName, value); }
        }

        public string PLACECODE_R
        {
            get { return GetProperty<string>(PLACECODE_RPropertyName); }
            set { SetProperty(PLACECODE_RPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}
