using System;
using System.Activities.Persistence;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using wmsMLC.Business.Workflow.Helpers;

namespace wmsMLC.Business.Workflow.PersistenceParticipants
{
    public class XmlPersistenceParticipant : PersistenceParticipant
    {
        const string PropertiesNamespace = "urn:schemas-microsoft-com:System.Activities/4.0/properties";
        private Guid _id;
        private Guid _activityId;

        public XmlPersistenceParticipant(Guid id, Guid activityId)
        {
            _id = id;
            _activityId = activityId;
        }

        //Add any additional necessary data to persist here
        //protected override void CollectValues(out IDictionary<XName, object> readWriteValues, out IDictionary<XName, object> writeOnlyValues)
        //{
        //    base.CollectValues(out readWriteValues, out writeOnlyValues);
        //}

        //Implementations of MapValues are given all the values collected from all participants implementations of CollectValues
        protected override IDictionary<XName, object> MapValues(IDictionary<XName, object> readWriteValues, IDictionary<XName, object> writeOnlyValues)
        {
            var statusXname = XName.Get("Status", PropertiesNamespace);

            var mappedValues = base.MapValues(readWriteValues, writeOnlyValues);

            var processUnit = new ProcessUnit { Id = _id, ActivityId = _activityId };
            string status = string.Empty;
            object value;

            //retrieve the status of the workflow
            if (writeOnlyValues.TryGetValue(statusXname, out value))
            {
                status = (string)value;
                processUnit.Status = status;
            }

            /*foreach (KeyValuePair<System.Xml.Linq.XName, object> item in writeOnlyValues)
            {                
            }*/

            IoHelper.EnsureAllProcessUnitFileExists();

            var fileName = IoHelper.GetAllProcessUnitsFileName();

            // load the document
            XElement doc;
            using (var fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var tw = new StreamReader(fs, Encoding.UTF8))
                {
                    doc = XElement.Load(tw);
                }
            }

            IEnumerable<XElement> current =
                from r in doc.Elements("ProcessUnit")
                where r.Attribute("id").Value.Equals(_id.ToString())
                select r;

            if (status == "Closed")
            {
                // erase nodes for the current processUnit                    
                foreach (var xe in current)
                {
                    xe.Attribute("status").Value = "finished";
                }
            }
            else
            {
                // erase nodes for the current processUnit                    
                foreach (XElement xe in current)
                {
                    xe.Remove();
                }

                // get the Xml version of the Rfp, add it to the document and save it
                var e = SerializeProcessUnit(processUnit);
                doc.Add(e);                
            }

            doc.Save(fileName);
            return mappedValues;
        }

        // serialize a ProcessUnit to Xml using Linq to Xml
        private XElement SerializeProcessUnit(ProcessUnit processUnit)
        {
            // main body of the processUnit
            var ret =
               new XElement("ProcessUnit",
                   new XAttribute("id", processUnit.Id.ToString()),
                   new XAttribute("status", processUnit.Status),
                       new XElement("creationDate", processUnit.CreationDate),
                       new XElement("completionDate", processUnit.CompletionDate),
                       new XElement("title", processUnit.Title),
                       new XElement("description", processUnit.Description),
                       new XElement("activityId", processUnit.ActivityId.ToString()));
            return ret;
        }

        //All of the values loaded from the InstanceData property bag are provided to implementations of PublishValues.  
        //protected override void PublishValues(IDictionary<XName, object> readWriteValues)
        //{
        //    base.PublishValues(readWriteValues);
        //}
    }
}
