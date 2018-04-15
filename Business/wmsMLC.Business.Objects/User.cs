namespace wmsMLC.Business.Objects
{
    public class User : WMSBusinessObject
    {
        #region .  Constants  .
        public const string UserCodePropertyName = "UserCode";
        
        public const string LoginPropertyName = "Login";
        public const string UserPasswordPropertyName = "UserPassword";
        public const string UserLastNamePropertyName = "UserLastName";
        public const string UserNamePropertyName = "UserName";
        public const string UserMiddleNamePropertyName = "UserMiddleName";

    /*UserDomain VARCHAR2(64),
    UserMultipleDeviceEntry NUMBER(1),
    UserAuthentication NUMBER(1),
    LangCode_r VARCHAR2(3),
    UserLocked NUMBER(1),
    UserDepartment VARCHAR2(64),
    UserOfficialCapacity VARCHAR2(255),
    UserWorkPhone VARCHAR2(50),
    UserEmail VARCHAR2(64),
    UserIns VARCHAR2(64),
    DateIns DATE,
    UserUpd VARCHAR2(64),
    DateUpd DATE
         */
        #endregion

        #region .  Properties  .
        public string UserCode
        {
            get { return GetProperty<string>(UserCodePropertyName); }
            set { SetProperty(UserCodePropertyName, value); }
        }
        #endregion
    }
}