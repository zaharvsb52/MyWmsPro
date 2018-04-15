namespace wmsMLC.Business.Objects
{
    public class UserGroup : WMSBusinessObject
    {
        #region .  Constants  .
        public const string UserGroupCodePropertyName = "UserGroupCode";
        public const string UserGroupParentPropertyName = "UserGroupParent";
        public const string USERGROUPNAMEPropertyName = "USERGROUPNAME";
        #endregion

        #region .  Properties  .
        public string UserGroupCode
        {
            get { return GetProperty<string>(UserGroupCodePropertyName); }
            set { SetProperty(UserGroupCodePropertyName, value); }
        }        
        public string UserGroupParent
        {
            get { return GetProperty<string>(UserGroupParentPropertyName); }
            set { SetProperty(UserGroupParentPropertyName, value); }
        }
        #endregion
    }
}
