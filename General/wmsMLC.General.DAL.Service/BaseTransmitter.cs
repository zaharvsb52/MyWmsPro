using System;
using System.Threading;
using wmsMLC.General.Services;
using wmsMLC.General.Services.Telegrams;

namespace wmsMLC.General.DAL.Service
{
    public class BaseTransmitter : ITransmitter
    {
        #region .  Fields  .
        private readonly IServiceClient _clnt;

        private volatile Telegram _receivedTelegram;

        private readonly Guid _transactionNumber;
        #endregion

        public BaseTransmitter(IServiceClient serviceClient)
        {
            _clnt = serviceClient;
            _transactionNumber = Guid.NewGuid();
        }

        #region .  ITransmitter  .

        public void Process(TelegramBodyType bodyType, Telegram telegram)
        {
            // присваиваем транзакцию
            telegram.TransactionNumber = _transactionNumber;

            //HACK: пока клиент общается только с SDCL (по правильному эти параметры должны передаваться через настройки)
            telegram.ToId.ServiceId = "SDCL";
            telegram.ToId.SessionId = 0;

            var tt = telegram as ITelegramWrapper;
            var realTelegram = tt != null ? tt.UnWrap() : telegram;
            _receivedTelegram = _clnt.Process(bodyType, realTelegram);

            if (tt != null)
                tt.FillBy(_receivedTelegram);
            _receivedTelegram = null;
        }

        readonly AutoResetEvent _waitResult = new AutoResetEvent(false);

        #endregion

        public void Dispose()
        {
            _receivedTelegram = null;
            if (_waitResult != null)
                _waitResult.Dispose();
        }
    }

    public class ResultContainer
    {
        public string TransactionNumber { get; set; }
        public string Name { get; set; }
        public object Value { get; set; }
    }
}