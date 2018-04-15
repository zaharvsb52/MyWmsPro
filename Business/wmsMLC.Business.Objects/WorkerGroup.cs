namespace wmsMLC.Business.Objects
{
    public class WorkerGroup : WMSBusinessObject
    {
        public const string Worker2GroupLPropertyName = "Worker2GroupL";

        public WMSBusinessCollection<Worker2Group> Worker2GroupL
        {
            get { return GetProperty<WMSBusinessCollection<Worker2Group>>(Worker2GroupLPropertyName); }
            set { SetProperty(Worker2GroupLPropertyName, value); }
        }
    }
}