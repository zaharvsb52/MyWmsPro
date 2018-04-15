using System;

namespace wmsMLC.General.Services
{
    /// <summary>
    /// Интерфейс протокола связи клиент-сервис
    /// </summary>
    public interface ITransmitter : IDisposable
    {
        void Process(TelegramBodyType bodyType, Telegram telegram);
    }
}
