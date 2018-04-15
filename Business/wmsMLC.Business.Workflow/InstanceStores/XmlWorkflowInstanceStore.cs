using System;
using System.Activities.DurableInstancing;
using System.Collections.Generic;
using System.IO;
using System.Runtime.DurableInstancing;
using System.Runtime.Serialization;
using System.Workflow.Runtime.Hosting;
using System.Xml;
using wmsMLC.Business.Workflow.Helpers;

namespace wmsMLC.Business.Workflow.InstanceStores
{
    public class XmlWorkflowInstanceStore : InstanceStore
    {
        Guid ownerInstanceID;

        public XmlWorkflowInstanceStore()
            : this(Guid.NewGuid())
        {

        }

        public XmlWorkflowInstanceStore(Guid id)
        {
            ownerInstanceID = id;
        }

        //Synchronous version of the Begin/EndTryCommand functions
        protected override bool TryCommand(InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout)
        {
            return EndTryCommand(BeginTryCommand(context, command, timeout, null, null));
        }

        //The persistence engine will send a variety of commands to the configured InstanceStore,
        //such as CreateWorkflowOwnerCommand, SaveWorkflowCommand, and LoadWorkflowCommand.
        //This method is where we will handle those commands
        protected override IAsyncResult BeginTryCommand(InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state)
        {
            IDictionary<System.Xml.Linq.XName, InstanceValue> data = null;

            //The CreateWorkflowOwner command instructs the instance store to create a new instance owner bound to the instanace handle
            if (command is CreateWorkflowOwnerCommand)
            {
                context.BindInstanceOwner(ownerInstanceID, Guid.NewGuid());
            }
            //The SaveWorkflow command instructs the instance store to modify the instance bound to the instance handle or an instance key
            else if (command is SaveWorkflowCommand)
            {
                var saveCommand = (SaveWorkflowCommand)command;
                data = saveCommand.InstanceData;

                Save(data);
            }
            //The LoadWorkflow command instructs the instance store to lock and load the instance bound to the identifier in the instance handle
            else if (command is LoadWorkflowCommand)
            {
                var fileName = IoHelper.GetFileName(this.ownerInstanceID);

                try
                {
                    using (var inputStream = new FileStream(fileName, FileMode.Open))
                    {
                        data = LoadInstanceDataFromFile(inputStream);
                        //load the data into the persistence Context
                        context.LoadedInstance(InstanceState.Initialized, data, null, null, null);
                    }
                }
                catch (Exception exception)
                {
                    throw new PersistenceException(exception.Message);
                }
            }

            return new CompletedAsyncResult<bool>(true, callback, state);
        }

        protected override bool EndTryCommand(IAsyncResult result)
        {
            return CompletedAsyncResult<bool>.End(result);
        }

        //Reads data from xml file and creates a dictionary based off of that.
        IDictionary<System.Xml.Linq.XName, InstanceValue> LoadInstanceDataFromFile(Stream inputStream)
        {
            IDictionary<System.Xml.Linq.XName, InstanceValue> data = new Dictionary<System.Xml.Linq.XName, InstanceValue>();

            var s = new NetDataContractSerializer();

            var doc = new XmlDocument();
            using(var rdr = XmlReader.Create(inputStream))
                doc.Load(rdr);

            var instances = doc.GetElementsByTagName("InstanceValue");
            foreach (XmlElement instanceElement in instances)
            {
                var keyElement = (XmlElement)instanceElement.SelectSingleNode("descendant::key");
                var key = (System.Xml.Linq.XName)DeserializeObject(s, keyElement);

                var valueElement = (XmlElement)instanceElement.SelectSingleNode("descendant::value");
                var value = DeserializeObject(s, valueElement);
                var instVal = new InstanceValue(value);

                data.Add(key, instVal);
            }

            return data;
        }

        object DeserializeObject(NetDataContractSerializer serializer, XmlElement element)
        {
            object deserializedObject = null;

            using (var stm = new MemoryStream())
            {
                var wtr = XmlDictionaryWriter.CreateTextWriter(stm);
                element.WriteContentTo(wtr);
                wtr.Flush();
                stm.Position = 0;

                deserializedObject = serializer.Deserialize(stm);
            }

            return deserializedObject;
        }

        //Saves the persistance data to an xml file.
        void Save(IDictionary<System.Xml.Linq.XName, InstanceValue> instanceData)
        {
            var fileName = IoHelper.GetFileName(this.ownerInstanceID);
            var doc = new XmlDocument();
            doc.LoadXml("<InstanceValues/>");

            foreach (KeyValuePair<System.Xml.Linq.XName, InstanceValue> valPair in instanceData)
            {
                var newInstance = doc.CreateElement("InstanceValue");

                var newKey = SerializeObject("key", valPair.Key, doc);
                newInstance.AppendChild(newKey);

                var newValue = SerializeObject("value", valPair.Value.Value, doc);
                newInstance.AppendChild(newValue);

                if (doc.DocumentElement != null) doc.DocumentElement.AppendChild(newInstance);
            }
            doc.Save(fileName);
        }

        XmlElement SerializeObject(string elementName, object o, XmlDocument doc)
        {
            var s = new NetDataContractSerializer();
            var newElement = doc.CreateElement(elementName);
            using (var stm = new MemoryStream())
            {
                s.Serialize(stm, o);
                stm.Position = 0;
                using(var rdr = new StreamReader(stm))
                    newElement.InnerXml = rdr.ReadToEnd();
            }

            return newElement;
        }
    }
}
