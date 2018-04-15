using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using wmsMLC.Business.JabberManager;
using wmsMLC.Business.Managers;
using wmsMLC.DCL.Chat.Views;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.ViewModels;
using wmsMLC.General;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.ViewModels;
using wmsMLC.General.PL.WPF.Views;
using wmsMLC.DCL.Resources;
using wmsMLC.DCL.General.Helpers;

namespace wmsMLC.DCL.Chat.ViewModels
{
    [View(typeof(ChatView))]
    public class ChatViewModel : PanelViewModelBase, IChatViewModelInternal, IHaveUniqueName, IHelpHandler, IModelHandler
    {
        #region .  Fields  .

        private readonly string _caption = StringResources.Chat;
        
        public const string UniqueName = "0C084346-D5B4-442E-83F5-3810106F62F0";
        
        private bool _autoExpandAllNodes;

        private readonly JabberManager _jabber;
        
        #endregion

        #region .  Properties  .

        public string KeyPropertyName { get; set; }
        public string ParentIdPropertyName { get; set; }
        public bool ShowNodeImage { get; set; }
        public bool ShowTotalRow { get; protected set; }
        public string DefaultSortingField { get; set; }

        public bool AutoExpandAllNodes
        {
            get { return _autoExpandAllNodes; }
            set
            {
                if (_autoExpandAllNodes == value)
                    return;
                _autoExpandAllNodes = value;
                OnPropertyChanged("AutoExpandAllNodes");
            }
        }

        public ObservableCollection<JidItem> UserSource { get; set; }

        public ObservableCollection<DataField> Fields { get; set; }

        public ObservableCollection<JidItem> SelectedItems { get; set; }

        public ObservableCollection<ConversationViewModel> ConversationItems { get; set; }

        private ConversationViewModel _currentConversationItem;
        public ConversationViewModel CurrentConversationItem {
            get { return _currentConversationItem; }
            set
            {
                _currentConversationItem = value;
                OnPropertyChanged("CurrentConversationItem");
            }
        }

        public event NewConversationHandler NewConversation;

        #endregion

        #region .  Commands  .

        public ICommand OpenChatWindow { get; private set; }

        #endregion

        public ChatViewModel()
        {
            KeyPropertyName = "UserName";
            ParentIdPropertyName = "GroupName";
            AllowClosePanel = true;
            PanelCaption = _caption;
            PanelCaptionImage = ImageResources.DCLDefault16.GetBitmapImage();

            OpenChatWindow = new DelegateCustomCommand(OnOpenChatWindow, () => true);

            _jabber = (JabberManager)IoC.Instance.Resolve<IChatManager>();
            Fields =
                new ObservableCollection<DataField>(new[]
                {new DataField() {FieldName = "UserName", SourceName = "UserName", Caption = "Список контактов", FieldType = typeof(string)}});
            SelectedItems = new ObservableCollection<JidItem>();
            var rooms = _jabber.GetRooms().ToArray();
            ConversationItems = new ObservableCollection<ConversationViewModel>(rooms.Select(i => new ConversationViewModel(i.Name, i)));
            foreach (var cm in ConversationItems)
                cm.PrivateRoom += conversation_PrivateRoom;
            //INFO: если были входящие сообщения от пользователей, то откроем с ними вкладки
            foreach (var r in rooms)
            {
                foreach (var user in r.GetUsers())
                {
                    if (r.GetMessages(user).Any(i => i.State == MsgState.Received))
                        conversation_PrivateRoom(user, r, false);
                }
            }
            RefreshData();
        }

        #region .  IHaveUniqueName  .
        public string GetUniqueName()
        {
            return UniqueName;
        }
        #endregion

        #region .  IHelpHandler  .
        public string GetHelpLink()
        {
            throw new NotImplementedException();
        }

        public string GetHelpEntity()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region .  IModelHandler  .
        public object GetSource()
        {
            throw new NotImplementedException();
        }

        public void SetSource(object source)
        {
            throw new NotImplementedException();
        }

        public void RefreshData()
        {
            UserSource = new ObservableCollection<JidItem>(_jabber.GetUserList().Select(i => new JidItem() { UserName = i }));
        }

        void IModelHandler.RefreshDataAsync()
        {
            throw new NotImplementedException();
        }

        public void RefreshView()
        {
            throw new NotImplementedException();
        }

        public event EventHandler SourceUpdateStarted;

        public event EventHandler SourceUpdateCompleted;

        public event EventHandler RefreshViewEvent;

        public bool IsReadEnable
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsEditEnable
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsNewEnable
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsDelEnable
        {
            get { throw new NotImplementedException(); }
        }

        public object ParentViewModelSource
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public wmsMLC.General.PL.SettingDisplay DisplaySetting
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        #endregion

        #region .  IChatViewModelInternal  .

        public void CloseTabIndex(int index)
        {
            if (index < 0 || ConversationItems == null)
                return;
            var cm = ConversationItems.ElementAt(index);
            cm.Dispose();
            ConversationItems.RemoveAt(index);
        }

        public bool IsRoom(int index)
        {
            if (index < 0 || ConversationItems == null)
                return false;
            var cm = ConversationItems.ElementAt(index);
            return cm.RoomMode;
        }

        #endregion

        #region .  Methods  .

        private void OnOpenChatWindow()
        {
            if (SelectedItems.Count == 0)
                    return;
            var userName = SelectedItems[0].UserName;
            var cm = GetConversationModel(userName);
            if (cm != null)
            {
                cm.PrivateRoom -= conversation_PrivateRoom;
                cm.PrivateRoom += conversation_PrivateRoom;
                CurrentConversationItem = cm;
            }
        }

        private ConversationViewModel GetConversationModel(string userName, IChatRoom room = null, bool roomMode = true)
        {
            var cm = ConversationItems.FirstOrDefault(i => i.GetUniqueName().EqIgnoreCase(userName));
            if (cm != null)
                return cm;
            var r = room ?? _jabber.GetRoom(userName);
            if (r == null)
                return null;
            var conversation = new ConversationViewModel(userName, room, roomMode);
            DispatcherHelper.BeginInvoke(new Action(() => ConversationItems.Add(conversation)));
            return conversation;
        }

        void conversation_PrivateRoom(string user, IChatRoom room, bool activate)
        {
            var cm = GetConversationModel(user, room, false);
            if (activate)
                CurrentConversationItem = cm;
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var item in ConversationItems)
                {
                    item.PrivateRoom -= conversation_PrivateRoom;
                    item.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }

    public delegate void NewConversationHandler(IViewModel model);

    public class JidItem
    {
        public string UserName { get; set; }

        public string GroupName { get; set; }

        public string Status { get; set; }
    }

    public interface IChatViewModelInternal : IChatViewModel
    {
        event NewConversationHandler NewConversation;

        void CloseTabIndex(int index);

        bool IsRoom(int index);
    }
}
