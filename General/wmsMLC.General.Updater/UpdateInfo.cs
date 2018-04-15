using System.Xml.Serialization;

namespace wmsMLC.General.Updater
{
    public class UpdateInfo
    {
        public string LastVersion { get; set; }
        public string MinimalSupportVersion { get; set; }
        public string DistributivePath { get; set; }
        public string ProcessName { get; set; }
        public string UpdateTool { get; set; }
        public string Text { get; set; }
        public string LogPath { get; set; }
        public string Commands { get; set; }
        [XmlIgnore]
        public bool Updating { get; set; }
    }
}
