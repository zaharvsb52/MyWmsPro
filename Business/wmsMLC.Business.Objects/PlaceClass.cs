namespace wmsMLC.Business.Objects
{
    public class PlaceClass : WMSBusinessObject
    {
        #region .  Constants  .
        public const string PlaceClassCodePropertyName = "PlaceClassCode";
        #endregion

        #region .  Properties  .
        public string PlaceClassCode
        {
            get { return GetProperty<string>(PlaceClassCodePropertyName); }
            set { SetProperty(PlaceClassCodePropertyName, value); }
        }
        #endregion
    }
}