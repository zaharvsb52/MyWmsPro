using wmsMLC.General.Services;
using wmsMLC.General.Services.Telegrams;

namespace wmsMLC.General.DAL.Service.Telegrams
{
    public class PingTelegramWrapper : BaseTelegrammWrapper
    {
        public override Telegram UnWrap()
        {
            var telegram = base.UnWrap();

            telegram.Type = TelegramType.Query;
            telegram.Content.Name = TelegramBodyType.Cmd.ToString().ToUpper();
            telegram.Content.Value = TelegramSysCommand.Ack.ToString().ToUpper();

            return telegram;
        }

        public static Telegram GetAck(Telegram telegram)
        {
            var tmp = telegram.ToId;
            telegram.ToId = telegram.FromId;
            telegram.Type = TelegramType.Answer;
            telegram.FromId = tmp;
            return telegram;
        }
    }
}