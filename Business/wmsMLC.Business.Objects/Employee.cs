namespace wmsMLC.Business.Objects
{
    public class Employee : WMSBusinessObject
    {
        #region .  Constants  .
        public const string EMPLOYEEPropertyName = "TENTEMPLOYEE";
        public const string EMPLOYEEDEPARTMENTPropertyName = "EMPLOYEEDEPARTMENT";
        public const string EMPLOYEEEMAILPropertyName = "EMPLOYEEEMAIL";
        public const string EMPLOYEEHOSTREFPropertyName = "EMPLOYEEHOSTREF";
        public const string EMPLOYEEIDPropertyName = "EMPLOYEEID";
        public const string EMPLOYEEISDEFAULTPropertyName = "EMPLOYEEISDEFAULT";
        public const string EMPLOYEELASTNAMEPropertyName = "EMPLOYEELASTNAME";
        public const string EMPLOYEEMIDDLENAMEPropertyName = "EMPLOYEEMIDDLENAME";
        public const string EMPLOYEEMOBILEPropertyName = "EMPLOYEEMOBILE";
        public const string EMPLOYEENAMEPropertyName = "EMPLOYEENAME";
        public const string EMPLOYEEOFFICIALCAPACITYPropertyName = "EMPLOYEEOFFICIALCAPACITY";
        public const string EMPLOYEEPHONEINTERNALPropertyName = "EMPLOYEEPHONEINTERNAL";
        public const string EMPLOYEEPHONEWORKPropertyName = "EMPLOYEEPHONEWORK";
        public const string PARTNERID_RPropertyName = "PARTNERID_R";
        public const string VEMPLOYEEFIOPropertyName = "VEMPLOYEEFIO";
        public const string VPARTNERNAMEPropertyName = "VPARTNERNAME";
        #endregion

        public string VEMPLOYEEFIO
        {
            get { return GetProperty<string>(VEMPLOYEEFIOPropertyName); }
            set { SetProperty(VEMPLOYEEFIOPropertyName, value); }
        }
    }
}