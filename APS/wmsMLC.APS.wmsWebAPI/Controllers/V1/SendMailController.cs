using System;
using System.IO;
using System.Net;
using System.Net.Http;
using wmsMLC.EPS.wmsEPS.Helpers;
using wmsMLC.General;

namespace wmsMLC.APS.wmsWebAPI.Controllers.V1
{
    public class SendMailController : BaseController
    {
        public HttpResponseMessage Post()
        {
            var mailinfo = GetFromRequestBody<MailInfo>();
            if (mailinfo == null)
                throw new Exception("MailInfo is null. Nothing to send.");

            var mailHelper = new MailHelper();
            using (var mail = mailHelper.CreateMailSender())
            {
                mailHelper.SetFrom(mail, mailinfo.From);
                mailHelper.SetCredentials(mail);

                foreach (var p in mailinfo.To)
                {
                    mail.AddTo(p);
                }

                mail.SetSubject(mailinfo.Subject);
                mail.SetBody(mailinfo.Body, mailinfo.IsBodyHtml);

                if (mailinfo.AttachedFiles != null)
                {
                    foreach (var att in mailinfo.AttachedFiles)
                    {
                        mail.AddAttach(new MemoryStream(att.FileStream), att.Name);
                    }
                }

                mail.Send();
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}