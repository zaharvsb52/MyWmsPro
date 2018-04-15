namespace wmsMLC.Business.Objects
{
    public class Art2Group : WMSBusinessObject
    {
        #region .  Constants  .
        public const string Art2GroupIDPropertyName = "ART2GROUPID";
        public const string Art2GroupArtCodePropertyName = "ART2GROUPARTCODE";
        public const string Art2GroupArtGroupCodePropertyName = "ART2GROUPARTGROUPCODE";
        public const string Art2GroupPriorityPropertyName = "ART2GROUPPRIORITY";
        public const string Art2GroupMandantPropertyName = "MANDANTID";

        #endregion

        #region .  Properties  .
        public string Art2GroupArtCode
        {
            get { return GetProperty<string>(Art2GroupArtCodePropertyName); }
            set { SetProperty(Art2GroupArtCodePropertyName, value); }
        }
        public string Art2GroupArtGroupCode
        {
            get { return GetProperty<string>(Art2GroupArtGroupCodePropertyName); }
            set { SetProperty(Art2GroupArtGroupCodePropertyName, value); }
        }
        public decimal? Art2GroupPriority
        {
            get { return GetProperty<decimal?>(Art2GroupPriorityPropertyName); }
            set { SetProperty(Art2GroupPriorityPropertyName, value); }
        }

        #endregion
    }
}