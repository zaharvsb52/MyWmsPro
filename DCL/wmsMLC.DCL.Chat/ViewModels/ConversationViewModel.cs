using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using wmsMLC.Business.Managers;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Helpers;
#pragma warning disable 162

namespace wmsMLC.DCL.Chat.ViewModels
{
    public delegate void PrivateRoomHandler(string user, IChatRoom room, bool activate = false);
    public class ConversationViewModel : PanelViewModelBase, IHaveUniqueName
    {
        #region .  Fields  .

        private const string PublicGroupName = "PUBLIC";

        private const string ChatPublicWrightName = "ChatPublicWright";

        private string _uniqueName;

        private IChatRoom _room;

        private bool _roomMode;

        private bool _canPosMessage;

        private string _lastMessage;

        #endregion

        #region .  Properties  .

        //public ObservableCollection<string> MessageItems { get; private set; }

        //private List<ChatMessage> _messageList;

        public ObservableCollection<JidItem> ConversationItems { get; private set; }

        public ObservableCollection<DataField> UserListFields { get; private set; }

        public JidItem CurrentConversationItem { get; set; }

        public string Message { get; set; }

        public bool CanPostMessage
        {
            get { return _canPosMessage; }
            set
            {
                _canPosMessage = value;
                OnPropertyChanged("CanPostMessage");
            }
        }

        public string LastMessage
        {
            get { return _lastMessage; }
            set
            {
                _lastMessage = value;
                OnPropertyChanged("LastMessage");
            }
        }

        public string HtmlContent
        {
            get
            {
                var htmlBuilder = new StringBuilder();
                const string head = "<!DOCTYPE html><html lang='ru'><head><meta http-equiv='X-UA-Compatible' content='IE=edge'/><meta http-equiv=Content-Type content='text/html;charset=UTF-8'>" +
                                    "<style type='text/css'>" +
                                    ".event { font-size: 8pt;" + 
                                    "        color: #c0c0c0;} " +
                                    ".bubble {" +
                                    "        background-color:#e9e9e9;" +
                                    "        margin: 0;" +
                                    "        padding: 10px;" +
                                    "        text-align:left;" +
                                    "        border-radius:10px;" +
                                    "        box-shadow: 0px 0 3px rgba(0,0,0,0.25);" +
                                    "        word-wrap: break-word;}" +
                                    ".bubble_my_message {" +
                                    "        background-color:#d3dffb;" +
                                    "        margin: 0;" +
                                    "        padding: 10px;" +
                                    "        text-align:left;" +
                                    "        border-radius: 10px;" +
                                    "        box-shadow: 0px 0 3px rgba(0,0,0,0.25);" +
                                    "        word-wrap: break-word;}" +
                                    ".from {" +
                                    "        position: relative;" +
                                    "        font-size: 10pt;}" +
                                    ".bubblewrapper { overflow:auto; padding: 5px; }" +
                                    ".leftblock { float:left; }  " +
                                    ".rightblock { float:right; }</style>" +
                                    "<script language='javascript' type='text/javascript'>window.onload=toBottom;function toBottom() {window.scrollTo(0, document.body.scrollHeight);}</script></head>";
                htmlBuilder.Append(head);
                htmlBuilder.Append("<body oncontextmenu='return false;' bgcolor=white>");

                var mess = _room.GetMessages(_uniqueName);
                if (mess != null)
                {
                    foreach (var msg in mess)
                    {
                        htmlBuilder.Append("<div class='bubblewrapper'>");
                        var dateString = msg.IsHistory ? msg.Date.ToString() : msg.Date.ToLongTimeString();
                        if (msg.IsEvent)
                        {
                            var realMsg = ProcessEvent(msg);
                            if (!string.IsNullOrEmpty(realMsg))
                                htmlBuilder.AppendFormat("<div class='event'>{0} {1}: {2}</div>", dateString, msg.From, realMsg);
                        }
                        else
                        {
                            var realMsg = msg.Body.Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
                            if (!msg.IsSelfMessage)
                            {
                                htmlBuilder.Append("<div class='leftblock'>");
                                htmlBuilder.AppendFormat("<div class='from'>{0}<color='Black'>&nbsp;<b>{1}</b></color></div>" +
                                    "<div class='bubble'>{2}</div></div>", dateString, msg.From, realMsg);
                            }

                            else
                            {
                                htmlBuilder.Append("<div class='rightblock'>");
                                htmlBuilder.AppendFormat("<div class='from'>{0}<color='Black'>&nbsp;<b>{1}</b></color></div>" +
                                                         "<div class='bubble_my_message'>{2}</div></div>", dateString, msg.From, realMsg);
                            }

                        }
                        htmlBuilder.Append("</div>");
                        msg.State = MsgState.Readed;
                    }
                }
                htmlBuilder.Append("<br><br></body></html>");
                RefreshContent(false);
                return htmlBuilder.ToString();
            }
        }

        public bool RoomMode
        {
            get { return _roomMode; }
            set
            {
                _roomMode = value;
                OnPropertyChanged("RoomMode");
            }
        }

        public event PrivateRoomHandler PrivateRoom;

        #endregion

        #region .  Commands  .

        public ICommand NewChatCommand { get; private set; }

        #endregion

        public ConversationViewModel(string name, IChatRoom room, bool roomMode = true)
        {
            RoomMode = roomMode;
            _uniqueName = name;
            _room = room;
            AllowClosePanel = !RoomMode;
            PanelCaption = room.Description?? name;
            PanelCaptionImage = ImageResources.DCLDefault16.GetBitmapImage();
            // разрешаем/запрещаем писать сообщения в чаты
            CanPostMessage = !RoomMode || !PublicGroupName.EqIgnoreCase(_uniqueName) || IoC.Instance.Resolve<ISecurityChecker>().Check(ChatPublicWrightName);
            Subscribe();
            ConversationItems = new ObservableCollection<JidItem>(_room.GetUsers().Select(i => new JidItem() { UserName = i, GroupName = _uniqueName }));
            UserListFields =
                new ObservableCollection<DataField>(new[]
                {
                    new DataField() { FieldName = "UserName", SourceName = "UserName", Caption = "Контакты", FieldType = typeof(string) }
                });
            NewChatCommand = new DelegateCustomCommand(OnPrivateChatWindow, () => true);
            RefreshContent();
        }

        #region .  Methods  .

        public void OnActivate()
        {
            RefreshContent();
        }

        private string ProcessEvent(ChatMessage message)
        {
            //INFO: отключили уведомления событий зашел/вышел
            //оставлено, если появятся новые события
            return null;
            string msg = null;
            switch (message.Body)
            {
                case "offline":
                    msg = "Вышел из системы";
                    break;
                case "online":
                    msg = "Выполнен вход в систему";
                    break;
                case "available":
                    break;
                default:
                    return message.Body;
            }
            return msg;
        }

        private void Subscribe()
        {
            if (_room == null)
                return;
            UnSubscribe();
            if (RoomMode)
                _room.Message += _room_Message;
            _room.PrivateMessage += _room_PrivateMessage;
            _room.Presence += _room_Presence;
            _room.SubscribeRequest += _room_SubscribeRequest;
        }

        private void UnSubscribe()
        {
            if (_room == null)
                return;
            _room.Message -= _room_Message;
            _room.PrivateMessage -= _room_PrivateMessage;
            _room.Presence -= _room_Presence;
            _room.SubscribeRequest -= _room_SubscribeRequest;
        }

        public void OnPrivateChatWindow()
        {
            OnPrivateChatWindowInternal(CurrentConversationItem.UserName, true);
        }

        private void OnPrivateChatWindowInternal(string userName, bool activate = false)
        {
            var h = PrivateRoom;
            if (h != null)
                h(userName, _room, activate);
        }

        void _room_SubscribeRequest(string user)
        {
        }

        private void _room_Presence(string from, string status)
        {
            if (!RoomMode && from != _uniqueName)
                return;
            switch (status)
            {
                case "offline":
                    var user = ConversationItems.FirstOrDefault(i => i.UserName.EqIgnoreCase(from));
                    if (user != null)
                    {
                        DispatcherHelper.BeginInvoke(new Action(() => ConversationItems.Remove(user)));
                        //INFO: если были не просмотренные сообщения, то покажем их
                        var receivedMess = _room.GetMessages(user.UserName);
                        if (receivedMess.Any(i => i.State == MsgState.Received))
                            OnPrivateChatWindowInternal(user.UserName);
                    }
                    break;
                case "online":
                    DispatcherHelper.BeginInvoke(
                        new Action(
                            () =>
                                ConversationItems.Add(new JidItem()
                                {
                                    UserName = from,
                                    GroupName = _uniqueName,
                                    Status = status
                                })));
                    break;
                default:
                    break;
            }
            RefreshContent();
        }

        private void _room_Message(ChatMessage message)
        {
            RefreshContent();
        }

        private void _room_PrivateMessage(ChatMessage message)
        {
            if (RoomMode)
            {
                OnPrivateChatWindowInternal(message.From);
                return;
            }
            if (message.From == _uniqueName)
                RefreshContent();
        }

        public void RefreshContent(bool htmlChanged = true)
        {
            if (htmlChanged)
                OnPropertyChanged("HtmlContent");
            var count = _room.GetMessages(_uniqueName).Count(i => i.State != MsgState.Readed);
            var realName = (_room.Name == _uniqueName) ? _room.Description : _uniqueName;
            if (count > 0)
                PanelCaption = string.Format("{0} ({1})", realName, count);
            else
                PanelCaption = realName;
            _room.RefreshState();
        }

        public void OnMessage(string message)
        {
            if (string.IsNullOrEmpty(message) || !CanPostMessage)
                return;
            if (RoomMode)
                _room.SendMessage(message);
            else
            {
                _room.SendPrivateMessage(_uniqueName, message);
                RefreshContent();
            }

            Message = string.Empty;
        }

        #endregion

        #region .  IModelHandler  .

        protected override void Dispose(bool disposing)
        {
            UnSubscribe();
            _room = null;
            base.Dispose(disposing);
        }

        #endregion

        #region .  IHaveUniqueName  .

        public string GetUniqueName()
        {
            return _uniqueName;
        }

        #endregion
    }
}
