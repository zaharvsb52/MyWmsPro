using System;

namespace wmsMLC.Business.Objects
{
    public class Work : WMSBusinessObject
    {
        #region . Constants .
        public const string CLIENTSESSIONID_RPropertyName = "CLIENTSESSIONID_R";
        public const string WORKID_PropertyName = "WORKID";
        public const string OPERATIONCODE_RPropertyName = "OPERATIONCODE_R";
        public const string STATUSCODE_RPropertyName = "STATUSCODE_R";
        public const string WORKCPVLPropertyName = "WORKCPVL";
        public const string WORKDESCPropertyName = "WORKDESC";
        public const string WORKINGLPropertyName = "WORKINGL";
        public const string WORK2ENTITYLPropertyName = "WORK2ENTITYL";
        public const string VWORKFROMPropertyName = "VWORKFROM";
        public const string VWORKTILLPropertyName = "VWORKTILL";
        public const string WORKGROUPID_RPropertyName = "WORKGROUPID_R";
        #endregion . Constants .

        #region . Properties .
        public decimal? CLIENTSESSIONID_R
        {
            get { return GetProperty<decimal?>(CLIENTSESSIONID_RPropertyName); }
            set { SetProperty(CLIENTSESSIONID_RPropertyName, value); }
        }

        public decimal? WORKID
        {
            get { return GetProperty<decimal?>(WORKID_PropertyName); }
            set { SetProperty(WORKID_PropertyName, value); }
        }

        public string OPERATIONCODE_R
        {
            get { return GetProperty<string>(OPERATIONCODE_RPropertyName); }
            set { SetProperty(OPERATIONCODE_RPropertyName, value); }
        }

        public string WORKDESC
        {
            get { return GetProperty<string>(WORKDESCPropertyName); }
            set { SetProperty(WORKDESCPropertyName, value); }
        }

        public string StatusCode
        {
            get { return GetProperty<string>(STATUSCODE_RPropertyName); }
            set { SetProperty(STATUSCODE_RPropertyName, value); }
        }

        public WorkStatus Status
        {
            get
            {
                var status = GetProperty<string>(STATUSCODE_RPropertyName);
                return (WorkStatus)Enum.Parse(typeof(WorkStatus), status);
            }
            set { SetProperty(STATUSCODE_RPropertyName, value.ToString()); }
        }

        public WMSBusinessCollection<Working> WORKINGL
        {
            get { return GetProperty<WMSBusinessCollection<Working>>(WORKINGLPropertyName); }
            set { SetProperty(WORKINGLPropertyName, value); }
        }

        public WMSBusinessCollection<Work2Entity> WORK2ENTITYL
        {
            get { return GetProperty<WMSBusinessCollection<Work2Entity>>(WORK2ENTITYLPropertyName); }
            set { SetProperty(WORK2ENTITYLPropertyName, value); }
        }

        public DateTime? VWORKFROM
        {
            get { return GetProperty<DateTime?>(VWORKFROMPropertyName); }
            set { SetProperty(VWORKFROMPropertyName, value); }
        }

        public DateTime? VWORKTILL
        {
            get { return GetProperty<DateTime?>(VWORKTILLPropertyName); }
            set { SetProperty(VWORKTILLPropertyName, value); }
        }

        public decimal? WORKGROUPID_R
        {
            get { return GetProperty<decimal?>(WORKGROUPID_RPropertyName); }
            set { SetProperty(WORKGROUPID_RPropertyName, value); }
        }
        #endregion . Properties .
    }
}