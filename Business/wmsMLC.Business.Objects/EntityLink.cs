namespace wmsMLC.Business.Objects
{
    public class EntityLink : WMSBusinessObject
    {
        #region .  Constants  .
        public const string EntityLinkNamePropertyName = "ENTITYLINKNAME";
        public const string EntityLinkFilterPropertyName = "ENTITYLINKFILTER";
        public const string EntityLinkFromPropertyName = "ENTITYLINKFROM";
        public const string EntityLinkToPropertyName = "ENTITYLINKTO";
        public const string EntityLinkTypePropertyName = "ENTITYLINKTYPE";
        #endregion .  Constants  .

        #region .  Properties  .
        public string EntityLinkName
        {
            get { return GetProperty<string>(EntityLinkNamePropertyName); }
            set { SetProperty(EntityLinkNamePropertyName, value); }
        }

        public string EntityLinkFilter
        {
            get { return GetProperty<string>(EntityLinkFilterPropertyName); }
            set { SetProperty(EntityLinkFilterPropertyName, value); }
        }

        public string EntityLinkFrom
        {
            get { return GetProperty<string>(EntityLinkFromPropertyName); }
            set { SetProperty(EntityLinkFromPropertyName, value); }
        }

        public string EntityLinkTo
        {
            get { return GetProperty<string>(EntityLinkToPropertyName); }
            set { SetProperty(EntityLinkToPropertyName, value); }
        }

        public string EntityLinkType
        {
            get { return GetProperty<string>(EntityLinkTypePropertyName); }
            set { SetProperty(EntityLinkTypePropertyName, value); }
        }
        #endregion .  Properties  .
    }
}