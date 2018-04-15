namespace wmsMLC.Business.Objects
{
    public class EventParam : WMSBusinessObject
    {
        public const string HeaderIDPropertyName = "EVENTHEADERID_R";
        public const string ClientSessionIDPropertyName = "CLIENTSESSIONID_R";
        public const string UserCodePropertyName = "USERCODE_R";
        public const string EventParamsPropertyName = "EVENTPARAMS";

        //public WMSBusinessCollection<TEventParam> EventParams
        //{
        //    get { return GetProperty<WMSBusinessCollection<TEventParam>>(EventParamsPropertyName); }
        //    set { SetProperty(EventParamsPropertyName, value); }
        //}
    }
}