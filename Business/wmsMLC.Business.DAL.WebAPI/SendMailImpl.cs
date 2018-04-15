using System.IO;
using wmsMLC.General;
using wmsMLC.General.DAL.WebAPI;

namespace wmsMLC.Business.DAL.WebAPI
{
    public class SendMailImpl
    {
        public void SendMail(MailInfo mailInfo)
        {
            var helper = new WebAPIHelper();
            var result = helper.Post<Stream>("SendMail", mailInfo);
            if (result != null)
                result.Dispose();
        }
    }
}
