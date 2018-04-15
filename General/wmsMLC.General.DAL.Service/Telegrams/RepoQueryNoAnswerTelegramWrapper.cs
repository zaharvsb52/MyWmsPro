using wmsMLC.General.Services;

namespace wmsMLC.General.DAL.Service.Telegrams
{
    public class RepoQueryNoAnswerTelegramWrapper : RepoQueryTelegramWrapper
    {
        public RepoQueryNoAnswerTelegramWrapper(string entityName, string actionName, TransmitterParam[] parameters) : base(entityName, actionName, parameters){}

        #region .  Methods  .
        public override Telegram UnWrap()
        {
            // TODO: переносить все поля!
            var telegram = base.UnWrap();
            telegram.Type = TelegramType.QueryNoAnswer;
            if (string.IsNullOrEmpty(telegram.Content.Name))
                telegram.Content.Name = TelegramType.QueryNoAnswer.ToString().ToUpper();
            if (string.IsNullOrEmpty(telegram.Content.Value))
                telegram.Content.Value = "RepoQuery";
            return telegram;
        }
        #endregion
    }
}
