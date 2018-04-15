using wmsMLC.General.Services;
using wmsMLC.General.Types;

namespace wmsMLC.Business.Managers
{
    public delegate void SystemMessage(string subject, string message);

    public interface ISystemManager
    {
        #region . Ping .
        
        /// <summary> Отправка запроса соединения </summary>
        /// <param name="timeout">время ожидания</param>
        void Ping();

        /// <summary> Время (мс), за которое прошел отклик </summary>
        int GetPingTime();

        void SendMessage(string subject, string message);

        /// <summary> Получение сообщения </summary>
        /// <param name="telegram">телеграмма сообщения</param>
        void ReceiveMessage(Telegram telegram);
        
        #endregion

        #region . Messaging .
        event SystemMessage OnSystemMessage;
        #endregion
    }
}
