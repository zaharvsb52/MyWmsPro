using System.Linq;
using wmsMLC.Business.DAL;
using wmsMLC.General;
using wmsMLC.General.Services;
using wmsMLC.General.Types;

namespace wmsMLC.Business.Managers
{
    public class SystemManager : ISystemManager
    {
        private readonly ISystemRepository _repository;

        private const string SubjectPropertyName = "SUBJECT";
        private const string MessagePropertyName = "MESSAGE";

        public SystemManager(ISystemRepository repository)
        {
            _repository = repository;
        }

        #region . Messaging .

        public void SendMessage(string subject, string message)
        {
            var repo = GetRepository();
            repo.SendMessage(subject, message);
        }

        public void ReceiveMessage(Telegram telegram)
        {
            var subject = telegram.Content.Nodes.FirstOrDefault(i => i.Name.Equals(SubjectPropertyName));
            var message = telegram.Content.Nodes.FirstOrDefault(i => i.Name.Equals(MessagePropertyName));
            if (subject == null || message == null)
                throw new DeveloperException("Wrong message telegram format");
            OnSystemMessage(subject.Value, message.Value);
        }

        public event SystemMessage OnSystemMessage;

        #endregion

        #region . Ping .
        public void Ping()
        {
            var repo = GetRepository();
            repo.Ping();
        }

        public int GetPingTime()
        {
            var repo = GetRepository();
            return repo.GetLastPingTime();
        }
        #endregion

        private ISystemRepository GetRepository()
        {
            return _repository;
        }
    }
}
