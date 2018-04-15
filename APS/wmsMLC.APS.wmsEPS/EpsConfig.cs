using wmsMLC.General;
using wmsMLC.General.Services.Service;

namespace wmsMLC.APS.wmsEPS
{
    /// <summary>
    /// Конфигурация сервиса печати
    /// </summary>
    public class EpsConfig : ConfigBase
    {
        public static readonly string BatchSizeParam = "batchsize";
        public static readonly string CronParam = "cron";

        public EpsConfig(ServiceContext context) : base(context)
        {
            Check();
        }

        private void Check()
        {
            if (Handler < 1)
                throw new OperationException("Ошибка проверки параметров запуска. Не задан номер сервиса '{0}'", HandlerParam);

            // Проверка количества максимальных заданий
            if (BatchSize < 1)
                throw new OperationException("Ошибка проверки параметров запуска. Не задан размер очереди заданий '{0}'", BatchSizeParam);

            // Проверка периодичности
            if (string.IsNullOrEmpty(Cron))
                throw new OperationException("Ошибка проверки параметров запуска. Не задано выражение для триггера '{0}'", CronParam);

            if (string.IsNullOrEmpty(Environment))
                throw new DeveloperException("Ошибка проверки параметров запуска. Не указан обязательный параметр запуска '{0}'", EnvironmentParam);
        }

        public int BatchSize
        {
            get
            {
                return Get<int>(BatchSizeParam);
            }
        }

        public string Cron
        {
            get
            {
                return Get<string>(CronParam);
            }
        }
    }
}
