namespace wmsMLC.General.PL.WPF.Events
{
    public class CloseRequestEvent
    {
        public CloseRequestEvent(object requestor)
        {
            Requestor = requestor;
        }

        public object Requestor { get; private set; }
    }
}