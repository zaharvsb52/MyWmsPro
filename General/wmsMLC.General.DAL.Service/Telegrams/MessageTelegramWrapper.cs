using wmsMLC.General.Services;
using wmsMLC.General.Services.Telegrams;

namespace wmsMLC.General.DAL.Service.Telegrams
{
    /*
    public class MessageTelegramWrapper : BaseTelegrammWrapper
    {
        private string _subject;
        private string _message;

        public MessageTelegramWrapper(string subject, string message) : base(new Telegram(TelegramType.Answer))
        {
            _subject = subject;
            _message = message;
        }

        public override Telegram UnWrap()
        {
            var telegram = base.UnWrap();

            telegram.Type = TelegramType.Answer;
            telegram.Content.Name = TelegramBodyType.Cmd.ToString().ToUpper();
            telegram.Content.Value = TelegramSysCommand.Message.ToString().ToUpper();
            telegram.Content.Nodes.Add(new Node { Name = "SUBJECT", Value = _subject });
            telegram.Content.Nodes.Add(new Node { Name = "MESSAGE", Value = _message });

            return telegram;
        }
    }
    */
}