using wmsMLC.General.BL;

namespace wmsMLC.Business.Objects
{
    public class EventDetail : WMSBusinessObject
    {
        public const string EventDetailIDPropertyName = "EVENTDETAILID";
        public const string EventHeaderIDPropertyName = "EVENTHEADERID_R";

        [HardCodedProperty]
        public decimal? EventDetailID
        {
            get { return GetProperty<decimal?>(EventDetailIDPropertyName); }
            set { SetProperty(EventDetailIDPropertyName, value); }
        }

        [HardCodedProperty(EventHeaderIDPropertyName)]
        public decimal? EventHeaderID
        {
            get { return GetProperty<decimal?>(EventHeaderIDPropertyName); }
            set { SetProperty(EventHeaderIDPropertyName, value); }
        }
    }
}
