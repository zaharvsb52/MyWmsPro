using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace wmsMLC.RCL.Main.Helpers
{
    public enum MsgState
    {
        None = 0, Received = 1, NotReaded = 2, Readed = 3
    }

    public class SystemMessageManager
    {
        private volatile List<SystemMessage> _messageQueue = new List<SystemMessage>();

        private readonly Timer _receivedTime;

        private const int ReceivedMessageTime = 10000;
        
        public delegate void StateChanged(int state);
        
        public event StateChanged OnStateChanged;

        private MsgState _msgState;

        public SystemMessageManager()
        {
            _receivedTime = new Timer(CheckState, null, -1, -1);
        }

        public void AddMessage(string subject, string message)
        {
            AddMessage(new SystemMessage(subject, message));            
        }

        public void AddMessage(SystemMessage message)
        {
            _messageQueue.Add(message);
            _msgState = MsgState.Received;
            OnStateChanged((int)_msgState);
            _receivedTime.Change(ReceivedMessageTime, ReceivedMessageTime);
        }

        public IEnumerable<SystemMessage> GetMessages()
        {
            return _messageQueue;
        }

        public SystemMessage GetLastMessage()
        {
            if (_messageQueue.Count < 1)
                return null;
            return _messageQueue.Last();
        }

        public void DeleteMessage(SystemMessage message)
        {
            _messageQueue.Remove(message);
            CheckState(null);
        }

        public void SetReaded(SystemMessage message)
        {
            message.Readed = true;
            CheckState(null);
        }

        public int GetMessagesCount()
        {
            return _messageQueue.Count;
        }

        private void CheckState(object obj)
        {            
            _receivedTime.Change(-1, -1);
            if (_messageQueue.Count < 1)
            {
                _msgState = MsgState.None;
            }
            else
            {
                var notReaded = _messageQueue.FirstOrDefault(i => !i.Readed);
                if (notReaded != null)
                    _msgState = MsgState.NotReaded;
                else
                    _msgState = MsgState.Readed;
            }
            OnStateChanged((int)_msgState);
        }

        public MsgState GetState()
        {
            return _msgState;
        }
    }
}
