using System;
namespace wmsMLC.Business.Objects
{
    public class ClientSession : WMSBusinessObject
    {
        #region .  Constants  .
        public const string ClientCode_RPropertyName = "CLIENTCODE_R";
        public const string ClientSessionBeginPropertyName = "CLIENTSESSIONBEGIN";
        public const string ClientSessionEndPropertyName = "CLIENTSESSIONEND";
        public const string ClientSessionAppKeyPropertyName = "CLIENTSESSIONAPPKEY";
        public const string ClientSessionCorrectlyOffPropertyName = "CLIENTSESSIONCORRECTLYOFF";
        public const string UserCode_RPropertyName = "USERCODE_R";
        public const string ClientSessionIdPropertyName = "CLIENTSESSIONID";
        public const string ClientTypeCode_RPropertyName = "CLIENTTYPECODE_R";
        public const string ClientSessionUserDomainNamePropertyName = "CLIENTSESSIONUSERDOMAINNAME";
        public const string ClientSessionClientVersionPropertyName = "CLIENTSESSIONCLIENTVERSION";
        public const string ClientSessionServiceIdPropertyName = "CLIENTSESSIONSERVICEID";
        public const string TruckCode_RPropertyName = "TRUCKCODE_R";
        public const string WorkerID_RPropertyName = "WORKERID_R";
        #endregion .  Constants  .

        #region .  Properties  .
        public decimal ClientSessionId
        {
            get { return GetProperty<decimal>(ClientSessionIdPropertyName); }
            set { SetProperty(ClientSessionIdPropertyName, value); }
        }

        public string ClientCode_R
        {
            get { return GetProperty<string>(ClientCode_RPropertyName); }
            set { SetProperty(ClientCode_RPropertyName, value); }
        }

        public DateTime ClientSessionBegin
        {
            get { return GetProperty<DateTime>(ClientSessionBeginPropertyName); }
            set { SetProperty(ClientSessionBeginPropertyName, value); }
        }

        public DateTime? ClientSessionEnd
        {
            get { return GetProperty<DateTime?>(ClientSessionEndPropertyName); }
            set { SetProperty(ClientSessionEndPropertyName, value); }
        }

        public string ClientSessionAppKey
        {
            get { return GetProperty<string>(ClientSessionAppKeyPropertyName); }
            set { SetProperty(ClientSessionAppKeyPropertyName, value); }
        }

        public bool? ClientSessionCorrectlyOff
        {
            get { return GetProperty<bool?>(ClientSessionCorrectlyOffPropertyName); }
            set { SetProperty(ClientSessionCorrectlyOffPropertyName, value); }
        }

        public string UserCode_R
        {
            get { return GetProperty<string>(UserCode_RPropertyName); }
            set { SetProperty(UserCode_RPropertyName, value); }
        }

        public string ClientTypeCode_R
        {
            get { return GetProperty<string>(ClientTypeCode_RPropertyName); }
            set { SetProperty(ClientTypeCode_RPropertyName, value); }
        }

        public string ClientSessionUserDomainName
        {
            get { return GetProperty<string>(ClientSessionUserDomainNamePropertyName); }
            set { SetProperty(ClientSessionUserDomainNamePropertyName, value); }
        }

        public string ClientSessionClientVersion
        {
            get { return GetProperty<string>(ClientSessionClientVersionPropertyName); }
            set { SetProperty(ClientSessionClientVersionPropertyName, value); }
        }

        public string ClientSessionServiceId
        {
            get { return GetProperty<string>(ClientSessionServiceIdPropertyName); }
            set { SetProperty(ClientSessionServiceIdPropertyName, value); }
        }

         public string TruckCode_R
        {
            get { return GetProperty<string>(TruckCode_RPropertyName); }
            set { SetProperty(TruckCode_RPropertyName, value); }
        }

         public decimal? WorkerID_R
         {
             get { return GetProperty<decimal?>(WorkerID_RPropertyName); }
             set { SetProperty(ClientSessionIdPropertyName, value); }
         }

        #endregion .  Properties  .
    }
}