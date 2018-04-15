using System;
using System.Collections.Generic;
using System.Linq;
using jabber.connection;
using jabber.protocol.client;
using wmsMLC.Business.Managers;

namespace wmsMLC.Business.JabberManager
{
    public class ChatRoom : IChatRoom
    {
        private readonly Room _room;

        private MsgState _state;

        private Lazy<Dictionary<string, List<ChatMessage>>> _messages = new Lazy<Dictionary<string,List<ChatMessage>>>();
        private string _description;

        private const int HistoryDayCount = 2;

        public string Name
        {
            get
            {
                if (_room == null)
                    return null;
                return _room.JID.User;
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }
        }

        public string UserName
        {
            get
            {
                if (_room == null)
                    return null;
                return _room.Nickname;
            }
        }

        public ChatRoom(Room room, string description)
        {
            _room = room;
            _description = description;
            Subscribe();
        }

        #region .  Methods  .

        public IEnumerable<ChatMessage> GetMessages(string from)
        {
            return _messages.Value.ContainsKey(@from) ? _messages.Value[@from] : new List<ChatMessage>();
        }

        public void RefreshState()
        {
            var newState = MsgState.Readed;
            
            if (_messages.Value.Any(msg => msg.Value.Any(i => i.State == MsgState.Received)))
                newState = MsgState.Received;

            if (_state != newState)
            {
                _state = newState;
                var h = StateChanged;
                if (h != null)
                    h(_state);
            }
        }

        private void Subscribe()
        {
            if (_room == null)
                return;
            UnSubscribe();
            _room.OnAdminMessage += _room_OnAdminMessage;
            _room.OnJoin += _room_OnJoin;
            _room.OnLeave += _room_OnLeave;
            _room.OnParticipantJoin += _room_OnParticipantJoin;
            _room.OnParticipantLeave += _room_OnParticipantLeave;
            _room.OnParticipantPresenceChange += _room_OnParticipantPresenceChange;
            _room.OnPresenceError += _room_OnPresenceError;
            _room.OnPrivateMessage += _room_OnPrivateMessage;
            _room.OnRoomConfig += _room_OnRoomConfig;
            _room.OnRoomMessage += _room_OnRoomMessage;
            _room.OnSelfMessage += _room_OnSelfMessage;
            _room.OnSubjectChange += _room_OnSubjectChange;
        }

        private void UnSubscribe()
        {
            if (_room == null)
                return;
            _room.OnAdminMessage -= _room_OnAdminMessage;
            _room.OnJoin -= _room_OnJoin;
            _room.OnLeave -= _room_OnLeave;
            _room.OnParticipantJoin -= _room_OnParticipantJoin;
            _room.OnParticipantLeave -= _room_OnParticipantLeave;
            _room.OnParticipantPresenceChange -= _room_OnParticipantPresenceChange;
            _room.OnPresenceError -= _room_OnPresenceError;
            _room.OnPrivateMessage -= _room_OnPrivateMessage;
            _room.OnRoomConfig -= _room_OnRoomConfig;
            _room.OnRoomMessage -= _room_OnRoomMessage;
            _room.OnSelfMessage -= _room_OnSelfMessage;
            _room.OnSubjectChange -= _room_OnSubjectChange;
        }

        private void OnMessage(ChatMessage message)
        {
            AddMessage(Name, message);
            var h = Message;
            if (h != null)
                h(message);
            RefreshState();
        }

        private void OnPrivateMessage(ChatMessage message)
        {
            AddMessage(message.From, message);
            var h = PrivateMessage;
            if (h != null)
                h(message);
            RefreshState();
        }

        private void OnPresence(string from, string status)
        {
            var h = Presence;
            if (h != null)
                h(from, status);
        }

        private void OnSubscribeRequest(string from)
        {
            var h = SubscribeRequest;
            if (h != null)
                h(from);
        }

        public void SendMessage(string message)
        {
            _room.PublicMessage(message);
        }

        public void SendPrivateMessage(string to, string message)
        {
            _room.PrivateMessage(to, message);
            AddMessage(to, new ChatMessage(UserName, message, null, true));
        }

        private void AddMessage(string from, ChatMessage message)
        {
            if (!_messages.Value.ContainsKey(from))
                _messages.Value.Add(from, new List<ChatMessage>());
            _messages.Value[from].Add(message);
        }

        public IEnumerable<string> GetUsers()
        {
            return _room.Participants.Cast<RoomParticipant>().Select(i => i.Nick);
        }

        private DateTime? GetTimeStamp(Message msg)
        {
            DateTime stamp;
            if (msg.X == null)
                return null;
            var attr = msg.X.GetAttribute("stamp");
            if (DateTime.TryParseExact(attr, "yyyyMMddTHH:mm:ss", null, System.Globalization.DateTimeStyles.None, out stamp))
                return stamp;
            return null;
        }

        private void OnMessageInternal(Message msg, bool isSelf = false, bool isEvent = false)
        {
            if (string.IsNullOrEmpty(msg.Body))
                return;
            var date = GetTimeStamp(msg);
            if (date != null && date.Value.AddDays(HistoryDayCount) < DateTime.Now)
                return;
            OnMessage(new ChatMessage(msg.From.Resource, msg.Body, date, isSelf, isEvent));
        }

        #endregion

        #region .  Room  .
        void _room_OnSubjectChange(object sender, Message msg)
        {
        }

        void _room_OnSelfMessage(object sender, Message msg)
        {
            OnMessageInternal(msg, true);
        }

        void _room_OnRoomMessage(object sender, Message msg)
        {
            OnMessageInternal(msg);
        }

        void _room_OnPrivateMessage(object sender, Message msg)
        {
            if (string.IsNullOrEmpty(msg.Body))
                return;
            if (msg.Type == MessageType.error)
                OnPrivateMessage(new ChatMessage(msg.From.Resource, "Сообщение не доставлено", GetTimeStamp(msg), false, true));
            else
                OnPrivateMessage(new ChatMessage(msg.From.Resource, msg.Body, GetTimeStamp(msg)));
        }

        IQ _room_OnRoomConfig(Room room, IQ parent)
        {
            return null;
        }


        void _room_OnPresenceError(Room room, Presence pres)
        {
        }

        void _room_OnParticipantPresenceChange(Room room, RoomParticipant participant)
        {
            //ProcessEvent(participant.Nick, "available");
        }

        void _room_OnParticipantLeave(Room room, RoomParticipant participant)
        {
            ProcessEvent(participant.Nick, "offline");
        }

        void _room_OnParticipantJoin(Room room, RoomParticipant participant)
        {
            ProcessEvent(participant.Nick, "online");
        }

        private void ProcessEvent(string from, string status)
        {
            // INFO: очень грубо, но пока только так убираем первые сообщения статусов пользователей
            if (_messages.Value.Any())
            {
                AddMessage(Name, new ChatMessage(from, status, DateTime.Now, false, true));
                AddMessage(from, new ChatMessage(from, status, DateTime.Now, false, true));
            }
            OnPresence(from, status);
        }

        void _room_OnLeave(Room room, Presence pres)
        {
            UnSubscribe();
        }

        void _room_OnJoin(Room room)
        {
            Subscribe();
        }

        void _room_OnAdminMessage(object sender, Message msg)
        {
            OnMessageInternal(msg);
        }

        #endregion

        public event ChatMessageEventHandler Message;
        public event ChatMessageEventHandler PrivateMessage;
        public event PresenceEventHandler Presence;
        public event SubscribeRequestHandler SubscribeRequest;
        public event StateChangedEventHandler StateChanged;
        public void Dispose()
        {
            UnSubscribe();
        }
    }
}