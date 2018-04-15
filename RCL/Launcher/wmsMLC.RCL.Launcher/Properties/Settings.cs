using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml;
using wmsMLC.RCL.Launcher.Common;

namespace wmsMLC.RCL.Launcher.Properties
{
    internal sealed class Settings
    {
        private static readonly Settings DefaultInstance = new Settings();

        private Settings() {}

        public static Settings Default
        {
            get
            {
                return DefaultInstance;
            }
        }

        public string SettingsPath { get; private set; }

        public string Server { get; set; }
        public string Domain { get; set; }
        public string RdcPath { get; set; }
        public bool SetLoginOnLoad { get; set; }
        public RdcType RdcType { get; set; }
        public string RdpFileTemplatePath { get; set; }
        public int DebugLevel { get; set; }
        public int ClearWaitMs { get; set; }

        /// <summary>
        /// Имя компьютера. Если определено и имя отличается, то меняем.
        /// </summary>
        public string ComputerName { get; set; }

        /// <summary>
        /// Недопустимые имена ТСД. Разделитель: ',' (без кавычек). Значение по-умолчанию: WindowsCE.
        /// </summary>
        public string[] BadComputerNames { get; set; }

        /// <summary>
        /// Количество попыток подключения к TS.
        /// </summary>
        public int RetryCount { get; set; }

        public bool Hw6500NeedWriteToRegister { get; set; }

        public void Load()
        {
            Load(null);
        }

        public void Load(string settingsPath)
        {
            SettingsPath = settingsPath;
            if (string.IsNullOrEmpty(SettingsPath))
            {
                SettingsPath =
                    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                SettingsPath += @"\Settings.xml";
            }

            if (!File.Exists(SettingsPath))
                throw new FileNotFoundException(string.Format(Resources.SettingsFileNotFound, SettingsPath));

            var xdoc = new XmlDocument();
            xdoc.Load(SettingsPath);
            
            var root = xdoc.DocumentElement;
            var nodeList = root.ChildNodes.Item(0).ChildNodes;

            var ds = TypeDescriptor.GetProperties(GetType());

            for (var i = 0; i < nodeList.Count; i++)
            {
                var node = nodeList[i];
                if (node.LocalName.EqIgnoreCase("#comment"))
                    continue;

                var keyAttribute = node.Attributes["key"];
                if (keyAttribute == null)
                    continue;

                var key = keyAttribute.Value;
                var property = ds.Find(key, true);
                if (property != null && !property.IsReadOnly)
                {
                    object value = node.Attributes["value"].Value;
                    var svalue = value == null ? string.Empty : value.ToString();

                    if (property.PropertyType == typeof (bool))
                        value = Convert.ToBoolean(svalue);
                    else if (property.PropertyType == typeof (int))
                        value = Convert.ToInt32(svalue);
                    else if (property.PropertyType == typeof(RdcType))
                    {
                        value = string.IsNullOrEmpty(svalue)
                            ? RdcType.MotoTscClient
                            : Enum.Parse(typeof(RdcType), svalue, true);
                    }
                    else if (property.PropertyType == typeof(string[]))
                    {
                        var array = svalue.Split(',');
                        value = array.Where(s => !string.IsNullOrEmpty(s)).ToArray();
                    }

                    property.SetValue(this, value);
                }
            }

            if (BadComputerNames == null || BadComputerNames.Length == 0)
                BadComputerNames = new[] {"WindowsCE"};
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(SettingsPath))
                return;

            var tw = new XmlTextWriter(SettingsPath, System.Text.Encoding.UTF8);
            tw.WriteStartDocument();
            tw.WriteStartElement("configuration");
            tw.WriteStartElement("appSettings");

            var ds = TypeDescriptor.GetProperties(GetType());

            foreach (PropertyDescriptor prop in ds)
            {
                if (prop.IsReadOnly)
                    continue;

                tw.WriteStartElement("add");
                tw.WriteStartAttribute("key", string.Empty);
                tw.WriteRaw(prop.Name);
                tw.WriteEndAttribute();

                tw.WriteStartAttribute("value", string.Empty);
                var value = prop.GetValue(this);

                if (value == null)
                    continue;

                var svalue = value.ToString();

                if (prop.PropertyType == typeof (string[]))
                {
                    svalue = string.Join(",", (string[]) value);
                }

                tw.WriteRaw(svalue);
                tw.WriteEndAttribute();
                tw.WriteEndElement();
            }

            tw.WriteEndElement();
            tw.WriteEndElement();

            tw.Close();
        }
    }
}
