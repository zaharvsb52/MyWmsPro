namespace wmsMLC.General.Services.Telegrams
{
    /// <summary>
    /// ������� ����� ��� �������� ��������� ������ � ������������ �� ��������� ������������
    /// </summary>
    public abstract class BaseTelegrammWrapper : Telegram, ITelegramWrapper
    {
        protected Telegram TransportTelegram;

        protected BaseTelegrammWrapper()
        {
            TransportTelegram = new Telegram();
        }

        public virtual Telegram UnWrap()
        {
            TransportTelegram.TransactionNumber = this.TransactionNumber;
            TransportTelegram.UserInfo = this.UserInfo;
            TransportTelegram.TimeOut = this.TimeOut;
            TransportTelegram.UnitOfWork = this.UnitOfWork;

            return TransportTelegram;
        }

        public virtual void FillBy(Telegram telegram)
        {
        }
    }
}