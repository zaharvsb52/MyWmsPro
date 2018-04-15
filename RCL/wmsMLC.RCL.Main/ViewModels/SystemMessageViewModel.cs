using wmsMLC.General.PL.WPF.Components.ViewModels;

namespace wmsMLC.RCL.Main.ViewModels
{
    public class SystemMessageViewModel : RclPanelViewModelBase
    {
        public SystemMessageViewModel()
        {
            PanelCaption = "Системное сообщение";
            //PanelCaptionImage = wmsMLC.RCL.Resources.ImageResources.DCLSysMsgReceived32.GetBitmapImage();
        }
        private string _subject;
        public string Subject 
        {
            get { return _subject; }
            set
            {
                _subject = value;
                OnPropertyChanged("Subject");
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
