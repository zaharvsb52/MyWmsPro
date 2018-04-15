using wmsMLC.EPS.wmsEPS.Properties;
using wmsMLC.General.Types;

namespace wmsMLC.EPS.wmsEPS.Helpers
{
    public class MailHelper
    {
        public Sender CreateMailSender()
        {
            var mail = new Sender(Settings.Default.MailHost, Settings.Default.MailHostPort);
            return mail;
        }

        public void SetFrom(Sender mail, string sendFrom = null)
        {
            var from = string.IsNullOrEmpty(sendFrom) ? Settings.Default.MailAccount : sendFrom;
            mail.SetFrom(from);
        }

        public void SetCredentials(Sender mail)
        {
            mail.SetCredentials(Settings.Default.MailLogin, Settings.Default.MailPassword);
        }
    }
}
