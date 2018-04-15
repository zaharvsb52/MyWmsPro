
namespace wmsMLC.General
{
    public class MailInfo
    {
        public string[] To { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public AttachedFileInfo[] AttachedFiles { get; set; }
        public bool IsBodyHtml { get; set; }
    }

    public class AttachedFileInfo
    {
        public string Name { get; set; }
        public byte[] FileStream { get; set; }
    }
}
