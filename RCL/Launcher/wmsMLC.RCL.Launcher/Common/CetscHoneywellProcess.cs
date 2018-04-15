using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using wmsMLC.RCL.Launcher.Properties;

namespace wmsMLC.RCL.Launcher.Common
{
    public class CetscHoneywellProcess : RdpProcessBase
    {
        private const string RdpFileName = @"Windows\rcl_2000.rdp";
        private const string ServerAttributeName = "ServerName";
        private const string DomainAttributeName = "Domain";
        private const string UserNameAttributeName = "UserName";
        private const string PasswordAttributeName = "Password";
        private const string PasswordTypeAttributeName = ":b:";

        public override void Run()
        {
            if (!Validate())
                return;

            var rdcPath = CreatePath(Settings.Default.RdcPath);

            //Убиваем все, что движется
            KillProcessByFileName(Path.GetFileName(rdcPath));

            //Копируем шаблон
            string text;
            using (var fs = File.Open(GetRdpFileTemplateName(), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var sr = new StreamReader(fs, Encoding.Unicode))
                {
                    text = sr.ReadToEnd();
                }
            }

            //Валидация
            if (!ValidateRdpFileTemplate(text))
                return;

            var rdpf = new StringBuilder();
            rdpf.AppendLine(string.Format("{0}:s:{1}", ServerAttributeName, Settings.Default.Server));
            rdpf.AppendLine(string.Format("{0}:s:{1}", DomainAttributeName, Settings.Default.Domain));
            rdpf.AppendLine(string.Format("{0}:s:{1}", UserNameAttributeName, Login));
            rdpf.AppendLine(string.Format("{0}{1}{2}", PasswordAttributeName, PasswordTypeAttributeName,
                DPAPI.RdpEncrypt(Pwd)));

            var templateTxt = text.Trim();
            while (templateTxt.EndsWith("\n") || templateTxt.EndsWith("\r"))
            {
                templateTxt = templateTxt.Substring(0, templateTxt.Length - 1);
            }

            rdpf.AppendLine(templateTxt);

            using (var fs = File.Open(RdpFileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                using (var sw = new StreamWriter(fs, Encoding.Unicode))
                {
                    sw.Write(rdpf.ToString());
                }
            }

            //Стартуем процесс
            Start(new ProcessStartInfo(RdpFileName, string.Empty)
            {
                Arguments = Path.GetFileName(RdpFileName),
                WorkingDirectory = Path.GetDirectoryName(RdpFileName),
                UseShellExecute = true,
                Verb = "open"
            });
        }

        public override void Clear()
        {
            File.Copy(GetRdpFileTemplateName(), RdpFileName, true);
        }

        public override void Delete()
        {
            try
            {
                if (File.Exists(RdpFileName))
                    File.Delete(RdpFileName);
            }
            catch (Exception)
            {
                Clear();
            }
        }

        private bool ValidateRdpFileTemplate(string text)
        {
            var rdpFileTemplateName = GetRdpFileTemplateName();
            if (string.IsNullOrEmpty(text))
                throw new ApplicationException(string.Format(Resources.RdpFileEmpty, rdpFileTemplateName));

            var result = ValidateRdpFileTemplateAttribute(text, ServerAttributeName, rdpFileTemplateName);
            if (!result)
                return false;

            result = ValidateRdpFileTemplateAttribute(text, DomainAttributeName, rdpFileTemplateName);
            if (!result)
                return false;
            
            result = ValidateRdpFileTemplateAttribute(text, UserNameAttributeName, rdpFileTemplateName);
            if (!result)
                return false;

            result = ValidateRdpFileTemplateAttribute(text, PasswordAttributeName + PasswordTypeAttributeName, rdpFileTemplateName);
            if (!result)
                return false;

            return true;
        }

        private string GetRdpFileTemplateName()
        {
            return CreatePath(Settings.Default.RdpFileTemplatePath);
        }

        private bool ValidateRdpFileTemplateAttribute(string text, string attribute, string rdpFileTemplateName)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException("text");
            if (string.IsNullOrEmpty(attribute))
                throw new ArgumentNullException("attribute");
            if (string.IsNullOrEmpty(rdpFileTemplateName))
                throw new ArgumentNullException("rdpFileTemplateName");

            var txt = text.ToLower();
            if (txt.Contains(attribute.ToLower()))
                throw new ApplicationException(string.Format(Resources.RdpFileAttributeError, rdpFileTemplateName, attribute));

            return true;
        }
    }
}
