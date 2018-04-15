namespace wmsMLC.Business.DAL
{
    public interface ISystemRepository
    {
        void Ping();
        int GetLastPingTime();
        void SendMessage(string subject, string message);
    }
}