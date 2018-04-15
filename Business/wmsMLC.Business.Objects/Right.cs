namespace wmsMLC.Business.Objects
{
    public class Right : WMSBusinessObject
    {
        #region .  Constants  .
        public const string RightNamePropertyName = "RightName";        
        #endregion

        #region .  Properties  .
        public string RightName
        {
            get { return GetProperty<string>(RightNamePropertyName); }
            set { SetProperty(RightNamePropertyName, value); }
        }
        #endregion
    }
}