using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using log4net;
using wmsMLC.Business.Managers;
using jabber;
using jabber.client;
using jabber.protocol.client;
using jabber.protocol.iq;
using jabber.connection;
using jabber.protocol;

namespace wmsMLC.Business.JabberManager
{
    public class JabberManager : IChatManager
    {
        #region .  Fields  .

        protected readonly ILog Log;

        private string _serverName;

        private string _networkHost;

        private int _port = 5222;

        private bool _TLS = true;

        private bool _register = false;

        private string _certificateFile;

        private string _certificatePass;

        private bool _untrustedOK = false;

        private bool _initialPresence = true;

        private string _boshURL = null;

        private bool _autoRoster;

        private float _reconnect;

        private bool _loging;

        private string _resource;

        private bool _connected;

        private JabberClient _jc;

        private RosterManager _roster;

        private ConferenceManager _conference;

        private DiscoManager _disco;

        private Dictionary<string, string> _userList = new Dictionary<string, string>();

        private Hashtable _rooms = new Hashtable();

        private JID _user;

        private List<IChatRoom> _chatRooms;

        private MsgState _allStates;

        private Dictionary<string, string> _defaultRooms;

        #endregion

        #region .  Properties  .

        #endregion

        public JabberManager()
        {
            Log = LogManager.GetLogger(GetType());
            _serverName = Properties.Settings.Default.ServerName;
            _networkHost = Properties.Settings.Default.NetworkHost;
            _port = Properties.Settings.Default.Port;
            _TLS = Properties.Settings.Default.AutoTLS;
            _certificateFile = Properties.Settings.Default.CertificateFile;
            _certificatePass = Properties.Settings.Default.CertificatePass;
            _untrustedOK = Properties.Settings.Default.UntrustedCA;
            _initialPresence = Properties.Settings.Default.AutoPresence;
            _boshURL = Properties.Settings.Default.BoshURL;
            _autoRoster = Properties.Settings.Default.AutoRoster;
            _reconnect = Properties.Settings.Default.AutoReconnect;
            _loging = Properties.Settings.Default.AutoLogin;
            //_resource = Properties.Settings.Default.Resource;
            _resource = Properties.Settings.Default.Resource + Guid.NewGuid();
            _chatRooms = new List<IChatRoom>();
            _defaultRooms = new Dictionary<string, string>();
        }

        #region .  Methods  .

        private void Subscribe()
        {
            if (_jc == null)
                return;
            UnSubscribe();
            _jc.OnAuthenticate += _jc_OnAuthenticate;
            _jc.OnAuthError += _jc_OnAuthError;
            _jc.OnError += jc_OnError;
            _jc.OnStreamError += jc_OnStreamError;
            _jc.OnMessage += jc_OnMessage;
            _jc.OnInvalidCertificate += jc_OnInvalidCertificate;
            _jc.OnLoginRequired += jc_OnLoginRequired;
            _jc.OnRegisterInfo += jc_OnRegisterInfo;
            _jc.OnRegistered += jc_OnRegistered;
            _jc.OnConnect += _jc_OnConnect;
            _jc.OnIQ += _jc_OnIQ;
            _jc.OnDisconnect += _jc_OnDisconnect;
            _jc.OnPresence += _jc_OnPresence;
            if (_roster != null)
            {
                _roster.OnRosterEnd += rm_OnRosterEnd;
                _roster.OnSubscription += rm_OnSubscription;
                _roster.OnUnsubscription += rm_OnUnsubscription;
            }
            if (_conference != null)
            {
                _conference.OnAdminMessage += _conference_OnAdminMessage;
                _conference.OnInvite += _conference_OnInvite;
                _conference.OnJoin += _conference_OnJoin;
                _conference.OnLeave += _conference_OnLeave;
            }
        }

        private void UnSubscribe()
        {
            if (_jc == null)
                return;
            _jc.OnAuthenticate -= _jc_OnAuthenticate;
            _jc.OnError -= jc_OnError;
            _jc.OnStreamError -= jc_OnStreamError;
            _jc.OnMessage -= jc_OnMessage;
            _jc.OnInvalidCertificate -= jc_OnInvalidCertificate;
            _jc.OnLoginRequired -= jc_OnLoginRequired;
            _jc.OnRegisterInfo -= jc_OnRegisterInfo;
            _jc.OnRegistered -= jc_OnRegistered;
            _jc.OnConnect -= _jc_OnConnect;
            _jc.OnIQ -= _jc_OnIQ;
            _jc.OnDisconnect -= _jc_OnDisconnect;
            _jc.OnPresence -= _jc_OnPresence;
            if (_roster != null)
            {
                _roster.OnRosterEnd -= rm_OnRosterEnd;
                _roster.OnSubscription -= rm_OnSubscription;
                _roster.OnUnsubscription -= rm_OnUnsubscription;
            }
            if (_conference != null)
            {
                _conference.OnAdminMessage -= _conference_OnAdminMessage;
                _conference.OnInvite -= _conference_OnInvite;
                _conference.OnJoin -= _conference_OnJoin;
                _conference.OnLeave -= _conference_OnLeave;
            }
        }

        #endregion

        #region .  IChatManager  .

        public void Connect(string user, string password)
        {
            Disconnect();
            _jc = new JabberClient();

            _jc.AutoReconnect = _reconnect;
            _user = new JID(user + "@" + _serverName);
            _jc.User = _user.User;
            _jc.Server = _user.Server;
            _jc.NetworkHost = _networkHost;
            _jc.Port = _port;
            _jc.Resource = _resource;
            _jc.Password = password;
            _jc.AutoStartTLS = _TLS;
            _jc.AutoPresence = _initialPresence;
            _jc.AutoRoster = _autoRoster;
            _jc.AutoLogin = _loging;

            if (!string.IsNullOrEmpty(_certificateFile))
            {
                _jc.SetCertificateFile(_certificateFile, _certificatePass);
                Console.WriteLine(_jc.LocalCertificate.ToString(true));
            }

            if (!string.IsNullOrEmpty(_boshURL))
            {
                _jc[Options.POLL_URL] = _boshURL;
                _jc[Options.CONNECTION_TYPE] = ConnectionType.HTTP_Binding;
            }

            if (_register)
            {
                _jc.AutoLogin = false;
                _register = false;
            }

            _conference = new ConferenceManager();
            _conference.Stream = _jc;

            _roster = new RosterManager();
            _roster.AutoAllow = jabber.client.AutoSubscriptionHanding.AllowIfSubscribed;
            _roster.AutoSubscribe = true;
            _roster.Stream = _jc;

            _disco = new DiscoManager();
            _disco.Stream = _jc;

            Subscribe();

            _jc.Connect();
        }

        private void OnDiscoNodeHandler1(DiscoManager sender, DiscoNode node, object state)
        {
            sender.BeginGetItems(node, OnDiscoNodeHandler2, state);
        }

        private void OnDiscoNodeHandler2(DiscoManager sender, DiscoNode node, object state)
        {
            var param = node.Children.Cast<DiscoNode>();
            var rooms = _defaultRooms.Where(i => param.Any(j => i.Key.Equals(j.Name, StringComparison.InvariantCultureIgnoreCase)));
            foreach (var r in rooms)
                JoinRoom(r.Key, r.Value);
        }

        public void Disconnect()
        {
            // удаляем чаты
            foreach (var chat in _chatRooms)
            {
                chat.StateChanged -= ichat_StateChanged;
                chat.Dispose();
            }
            _chatRooms.Clear();
            // удаляем комнаты
            foreach (Room r in _rooms.Values)
                r.Leave(null);
            _rooms.Clear();

            _connected = false;
            if (_disco != null)
                _disco.Dispose();
            if (_conference != null)
                _conference.Dispose();
            if (_roster != null)
                _roster.Dispose();
            if (_jc == null)
                return;

            _jc.Close();
            UnSubscribe();
            _jc.Dispose();
            _jc = null;
        }

        public void SendMessage(string to, string message)
        {
            if (_jc == null || !_connected)
                return;
            _jc.Message(to, message);
        }

        public void TransferFile(string to, byte[] data)
        {
        }

        public void OnMessage(string from, string message)
        {
            //INFO: Fix для Windows 8 - привет мелкомягким...
            if (string.IsNullOrEmpty(message))
                return;
            var h = Message;
            if (h != null)
                h(from, message);
        }

        public void OnPresence(string from, string status)
        {
            var h = Presence;
            if (h != null)
                h(from, status);
        }

        public void OnSubscribeRequest(string user)
        {
            var h = SubscribeRequest;
            if (h != null)
                h(user);
        }

        public IEnumerable<string> GetUserList()
        {
            return _userList.Keys;
        }

        public void Subscribe(string user, string nickname, string[] groups)
        {
            if (_jc == null)
                return;
            var jid = new JID(user, _jc.Server, null);
            _jc.Subscribe(jid, nickname, groups);
        }

        public void JoinRoom(string room, string description)
        {
            var r = GetRoomInternal(room);
            if (r != null)
            {
                r.Join();
                var ichat = new ChatRoom(r, description);
                ichat.StateChanged += ichat_StateChanged;
                _chatRooms.Add(ichat);
            }
        }

        void ichat_StateChanged(MsgState state)
        {
            if (_allStates == state)
                return;
            _allStates = state;
            var h = StateChanged;
            if (h != null)
                h(state);
        }

        public void SendRoomMessage(string room, string message)
        {
            var r = GetRoomInternal(room);
            if (r == null)
                return;
            r.PublicMessage(message);
        }

        public IChatRoom GetRoom(string room)
        {
            return _chatRooms.FirstOrDefault(i => i.Name == room);
        }

        public IEnumerable<IChatRoom> GetRooms()
        {
            return _chatRooms;
        }

        public void SetDefaultRooms(Dictionary<string, string> rooms)
        {
            _defaultRooms.Clear();
            _defaultRooms = rooms;
        }

        public event MessageEventHandler Message;

        public event PresenceEventHandler Presence;

        public event SubscribeRequestHandler SubscribeRequest;

        public event StateChangedEventHandler StateChanged;

        #endregion

        #region .  Methods  .

        private Room GetRoomInternal(string room)
        {
            if (_conference == null)
                return null;
            var jid = new JID(room, "conference." + _serverName, _jc.JID.User);
            var r = (Room)_rooms[jid];
            if (r != null)
                return r;
            r = _conference.GetRoom(new JID(room, "conference." + _serverName, _jc.JID.User));
            if (r == null)
                return null;
            _rooms[jid] = r;
            return r;
        }

        #endregion

        #region .  jabber.Net  .

        void _jc_OnAuthenticate(object sender)
        {
            _disco.BeginFindServiceWithFeature(URI.MUC, OnDiscoNodeHandler1, new object());
        }

        void jc_OnMessage(object sender, Message msg)
        {
            // если сообщение от администратора
            if(string.IsNullOrEmpty(msg.ID))
                OnMessage(msg.From, msg.Body);
        }

        bool jc_OnInvalidCertificate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            Log.DebugFormat("Invalid certificate ({0}):\n{1}", sslPolicyErrors.ToString(), certificate != null ? certificate.ToString(true) : string.Empty);
            return _untrustedOK;
        }

        private void jc_OnError(object sender, Exception ex)
        {
            if ((ex is bedrock.net.AsyncSocketConnectionException) && ex.Message.StartsWith("Bad host"))
            {
                var c = (JabberClient) sender;
                var t = new Timer( delegate { c.Connect(); }, c, (long) (1000*c.AutoReconnect), Timeout.Infinite);
            }
            Log.Error("ERROR: ", ex);
        }

        void _jc_OnAuthError(object sender, System.Xml.XmlElement rp)
        {
            var jc = (JabberClient)sender;
            if (!_register)
            {
                Log.Debug("Try to register");
                _register = true;
                Connect(jc.User, jc.Password);
            }
        }

        private void jc_OnStreamError(object sender, System.Xml.XmlElement rp)
        {
            Log.Debug("Stream ERROR: " + rp.OuterXml);
        }

        private void jc_OnLoginRequired(object sender)
        {
            Log.Debug("Registering");
            var jc = (JabberClient)sender;
            jc.Register(new JID(jc.User, jc.Server, null));
        }

        private void jc_OnRegistered(object sender,
                                     IQ iq)
        {
            var jc = (JabberClient)sender;
            if (iq.Type == IQType.result)
                jc.Login();
        }

        private bool jc_OnRegisterInfo(object sender, Register r)
        {
            return true;
        }

        void _jc_OnConnect(object sender, StanzaStream stream)
        {
            Log.Debug("Connected");
            _connected = true;
        }

        void _jc_OnIQ(object sender, IQ iq)
        {
            Log.DebugFormat("IQ {0}", iq);
        }

        void _jc_OnDisconnect(object sender)
        {
            Log.Debug("Disconnected");
            _connected = false;
        }

        void _jc_OnPresence(object sender, Presence pres)
        {
            if (_user.User == pres.From.User)
                return;
            if (!_userList.ContainsKey(pres.From.User))
                _userList.Add(pres.From.User, pres.Status);
            else
                _userList[pres.From.User] = pres.Status;
        }

        #endregion

        #region .  Roster manager  .

        private void rm_OnSubscription(RosterManager manager, Item ri, Presence pres)
        {
            manager.ReplyAllow(pres);
            OnMessage("WMSMLC", string.Format("Пользовтель {0} добавился в список контактов", pres.From));
        }

        private void rm_OnUnsubscription(RosterManager manager, Presence pres, ref bool remove)
        {
            OnMessage("WMSMLC", string.Format("Пользовтель {0} удалился из списока контактов", pres.From));
        }

        private void rm_OnRosterEnd(object sender)
        {
        }

        #endregion

        #region .  Conference manager  .

        void _conference_OnJoin(Room room)
        {
            OnMessage(room.JID.User, "Вход выполнен");
        }

        void _conference_OnLeave(Room room, Presence pres)
        {
        }

        void _conference_OnInvite(object sender, Message msg)
        {
        }

        void _conference_OnAdminMessage(object sender, Message msg)
        {
            OnMessage(msg.From, msg.Body);
        }

        #endregion
    }
}
