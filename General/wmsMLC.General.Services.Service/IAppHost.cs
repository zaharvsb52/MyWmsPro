namespace wmsMLC.General.Services.Service
{
    public interface IAppHost
    {
        void Start(string[] args);
        void Stop();
    }
}
