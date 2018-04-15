namespace wmsMLC.General.PL.WPF.Events
{
    public class WaitEvent
    {
        public WaitEvent(object sender, WaitEventType type)
        {
            Sender = sender;
            Type = type;
        }

        public object Sender { get; private set; }

        public WaitEventType Type { get; private set; }
    }

    public enum WaitEventType
    {
        Start,
        Stop,
        Progress
    }
}