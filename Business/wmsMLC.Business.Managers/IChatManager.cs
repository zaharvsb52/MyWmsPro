using System;
using System.Collections.Generic;

namespace wmsMLC.Business.Managers
{
    public delegate void MessageEventHandler(string from, string message);
    public delegate void PresenceEventHandler(string from, string status);
    public delegate void SubscribeRequestHandler(string user);

    public interface IChatManager
    {
        #region .  Methods  .

        void Connect(string user, string password);

        void Disconnect();

        void SendMessage(string to, string message);

        void TransferFile(string to, byte[] data);

        void OnMessage(string from, string message);

        void OnPresence(string from, string status);

        void OnSubscribeRequest(string user);

        IEnumerable<string> GetUserList();

        void Subscribe(string user, string nickname, string[] groups);

        void JoinRoom(string room, string description);

        void SendRoomMessage(string room, string message);

        IChatRoom GetRoom(string room);

        IEnumerable<IChatRoom> GetRooms();

        void SetDefaultRooms(Dictionary<string, string> rooms);

        #endregion

        #region .  Events   .

        event MessageEventHandler Message;

        event PresenceEventHandler Presence;

        event SubscribeRequestHandler SubscribeRequest;

        event StateChangedEventHandler StateChanged;

        #endregion
    }

    public delegate void ChatMessageEventHandler(ChatMessage message);
    public delegate void StateChangedEventHandler(MsgState state);
    public interface IChatRoom : IDisposable
    {
        string Name { get; }

        string Description { get; }

        string UserName { get; }

        void SendMessage(string message);

        void SendPrivateMessage(string to, string message);

        IEnumerable<string> GetUsers();

        event ChatMessageEventHandler Message;

        event ChatMessageEventHandler PrivateMessage;

        event PresenceEventHandler Presence;

        event SubscribeRequestHandler SubscribeRequest;

        event StateChangedEventHandler StateChanged;

        IEnumerable<ChatMessage> GetMessages(string from);

        void RefreshState();
    }

    public enum MsgState
    {
        None = 0, Received = 1, NotReaded = 2, Readed = 3
    }

    public class ChatMessage
    {
        public string From { get; private set; }
        public string Body { get; private set; }
        public DateTime Date { get; private set; }
        public bool IsHistory { get; private set; }

        public bool IsEvent { get; private set; }

        public bool IsSelfMessage { get; private set; }

        public MsgState State { get; set; }

        public ChatMessage(string from, string body, DateTime? date = null, bool self = false, bool isEvent = false)
        {
            From = from;
            Body = body;
            IsHistory = date != null;
            Date = IsHistory ? date.Value : DateTime.Now;
            State = IsHistory ? MsgState.Readed : MsgState.Received;
            IsSelfMessage = self;
            IsEvent = isEvent;
        }
    }
}
