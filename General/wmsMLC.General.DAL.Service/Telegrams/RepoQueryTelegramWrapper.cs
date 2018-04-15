using wmsMLC.General.Services;

namespace wmsMLC.General.DAL.Service.Telegrams
{
    public class RepoQueryTelegramWrapper : NodeTelegrammWrapper
    {
        public RepoQueryTelegramWrapper(string entityName, string actionName, TransmitterParam[] parameters) : base(entityName, actionName, parameters) { }

        #region .  Methods  .
        public override Telegram UnWrap()
        {
            // TODO: переносить все поля!
            var telegram = base.UnWrap();
            telegram.Type = TelegramType.Query;
            if (string.IsNullOrEmpty(telegram.Content.Name))
                telegram.Content.Name = TelegramType.Query.ToString().ToUpper();
            if (string.IsNullOrEmpty(telegram.Content.Value))
                telegram.Content.Value = "RepoQuery";
            return telegram;
        }
        #endregion
    }
}