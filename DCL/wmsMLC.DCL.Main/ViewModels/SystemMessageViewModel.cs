using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Resources;
using wmsMLC.General.PL.WPF;

namespace wmsMLC.DCL.Main.ViewModels
{

    public class SystemMessageViewModel : PanelViewModelBase
    {
        public SystemMessageViewModel()
        {
            PanelCaption = "Отправить сообщение";
            PanelCaptionImage = ImageResources.DCLSysMsgReceived32.GetBitmapImage();
        }
        private string _user;
        public string User
        {
            get { return _user; }
            set
            {
                _user = value;
                OnPropertyChanged("User");
            }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged("Message");
            }
        }
    }
}
