namespace wmsMLC.General.Telegrams
{
    /// <summary> Системные комманды </summary>
    public enum TelegramSysCommand
    {
        /// <summary> Перезагрузить конфигурацию </summary>
        ReloadConfig,
        /// <summary> Подтвердить получение </summary>
        Ack,
        /// <summary> Приостановить работу </summary>
        Pause,
        /// <summary> Возобновить работу </summary>
        Resume,
        /// <summary> Остановить сервис </summary>
        Stop,
        /// <summary> Немедленно остановить сервис </summary>
        Kill,
        /// <summary> Подключить новый сервис </summary>
        Connect
    }
}