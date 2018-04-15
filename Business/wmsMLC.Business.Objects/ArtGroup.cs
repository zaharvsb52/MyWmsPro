namespace wmsMLC.Business.Objects
{
    public class ArtGroup : WMSBusinessObject
    {
        #region .  Constants  .
        public const string ArtGroupCodePropertyName = "ARTGROUPCODE";
        public const string ArtGroupNamePropertyName = "ARTGROUPNAME";
        public const string ARTGROUPHOSTREFPropertyName = "ARTGROUPHOSTREF";
        public const string MANDANTIDPropertyName = "MANDANTID";
        #endregion .  Constants  .

        #region .  Properties  .
        public string ArtGroupCode
        {
            get { return GetProperty<string>(ArtGroupCodePropertyName); }
            set { SetProperty(ArtGroupCodePropertyName, value); }
        }

        public string ArtGroupName
        {
            get { return GetProperty<string>(ArtGroupNamePropertyName); }
            set { SetProperty(ArtGroupNamePropertyName, value); }
        }

        public string ARTGROUPHOSTREF
        {
            get { return GetProperty<string>(ARTGROUPHOSTREFPropertyName); }
            set { SetProperty(ARTGROUPHOSTREFPropertyName, value); }
        }

        public decimal? MANDANTID
        {
            get { return GetProperty<decimal?>(MANDANTIDPropertyName); }
            set { SetProperty(MANDANTIDPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}