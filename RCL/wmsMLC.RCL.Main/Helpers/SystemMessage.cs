using System;

namespace wmsMLC.RCL.Main.Helpers
{
    public class SystemMessage
    {
        private readonly string _subject;
        private readonly string _message;
        private readonly Guid _id;
        private readonly DateTime _messageTime;

        public SystemMessage(string subject, string message)
        {
            _messageTime = DateTime.Now;
            _id = Guid.NewGuid();
            Readed = false;
            _subject = subject;
            _message = message;
        }

        public string Subject
        {
            get { return _subject; }            
        }

        public string Message
        {
            get { return _message; }
        }

        public bool Readed { get; set; }

        public Guid Id
        {
            get
            {
                return _id;
            }
        }

        public DateTime MessageTime
        {
            get { return _messageTime; }
        }
    }
}
