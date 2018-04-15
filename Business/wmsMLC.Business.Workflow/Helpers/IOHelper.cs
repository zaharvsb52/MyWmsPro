using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace wmsMLC.Business.Workflow.Helpers
{
    /// <summary>
    /// This class is helper to deal with all IO related issues (folders, paths, etc.)
    /// </summary>
    internal static class IoHelper
    {
        public static readonly string InstanceFormatString = "{0}.xml";

        private static object _fileLock = new object();

        public static readonly string PersistenceDirectory =
            Path.Combine(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory), "ActivityPersistence");

        public static string GetFileName(Guid id)
        {
            EnsurePersistenceFolderExists();
            return Path.Combine(PersistenceDirectory,
                                string.Format(CultureInfo.InvariantCulture, InstanceFormatString, id));
        }

        public static void DeleteFile(Guid id)
        {
            EnsurePersistenceFolderExists();
            var file = Path.Combine(PersistenceDirectory,
                                    string.Format(CultureInfo.InvariantCulture, InstanceFormatString, id));
            if (!File.Exists(file))
                throw new FileNotFoundException(file);
            File.Delete(file);
        }

        public static IEnumerable<string> GetFileNames()
        {
            EnsurePersistenceFolderExists();
            return Directory.GetFiles(PersistenceDirectory, "*-*-*-*-*.xml");
        }
        
        public static string GetAllProcessUnitsFileName()
        {
            EnsurePersistenceFolderExists();
            return Path.Combine(PersistenceDirectory, "processes.xml");
        }

        public static string GetTrackingFilePath(Guid instanceId)
        {
            EnsurePersistenceFolderExists();
            return Path.Combine(PersistenceDirectory, instanceId.ToString() + ".tracking");
        }

        public static void EnsurePersistenceFolderExists()
        {
            if (!Directory.Exists(PersistenceDirectory))
            {
                Directory.CreateDirectory(PersistenceDirectory);
            }
        }

        public static void EnsureAllProcessUnitFileExists()
        {
            var fileName = GetAllProcessUnitsFileName();
            if (!File.Exists(fileName))
            {
                var root = new XElement("ProcessUnits");
                lock (_fileLock)
                {
                    root.Save(fileName);
                }
            }
        }

        public static bool ProcessUnitExistAndIdle(Guid id)
        {
            EnsureAllProcessUnitFileExists();
            XElement doc;
            lock (_fileLock)
            {
                // load the document
                var fileName = GetAllProcessUnitsFileName();
                using (var fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (var tw = new StreamReader(fs, Encoding.UTF8))
                    {
                        doc = XElement.Load(tw);
                    }
                }
            }
            IEnumerable<XElement> current = from r in doc.Elements("ProcessUnit")
                where r.Attribute("id").Value.Equals(id.ToString())
                select r;
            
            var idle = current.FirstOrDefault(i => i.Attribute("status").Value.Equals("Idle"));
            return idle != null;
        }
    }
}
