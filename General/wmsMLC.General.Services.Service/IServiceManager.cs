namespace wmsMLC.General.Services.Service
{
    public interface IServiceManager
    {
        Telegram ProcessTelegram(Telegram telegram);
        void Close();
    }
}
