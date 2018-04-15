namespace wmsMLC.Business.Objects
{
    public class Truck2MotionAreaGroup : WMSBusinessObject
    {
        #region .  Constants  .
        public const string Truck2MotionAreaGroupMotionAreaGroupCodePropertyName = "TRUCK2MOTIONAREAGROUPMOTIONAREAGROUPCODE";
        #endregion .  Constants  .

        #region .  Properties  .
        public string Truck2MotionAreaGroupMotionAreaGroupCode
        {
            get { return GetProperty<string>(Truck2MotionAreaGroupMotionAreaGroupCodePropertyName); }
            set { SetProperty(Truck2MotionAreaGroupMotionAreaGroupCodePropertyName, value); }
        }
       #endregion .  Properties  .
    }
}