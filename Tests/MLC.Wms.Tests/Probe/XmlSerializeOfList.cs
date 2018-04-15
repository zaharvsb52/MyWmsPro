using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using NUnit.Framework;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace MLC.Wms.Tests.Probe
{
    [TestFixture]
    public class XmlSerializeOfList
    {
        public class SomeClass
        {
            public string A { get; set; }
            public string B { get; set; }
        }

        public class OtherClass
        {
            public SerializableDictionary<string, SerializableList<SomeClass>> Items { get; set; }
        }

        [Test]
        public void SerializableListTest()
        {
            var source = new OtherClass() { Items = new SerializableDictionary<string, SerializableList<SomeClass>>()} ;
            source.Items.Add("test1", new SerializableList<SomeClass> { new SomeClass() { A = "A1", B = "B1" } });
            source.Items.Add("test2", new SerializableList<SomeClass> { new SomeClass() { A = "A2", B = "B2" } });

            var xmlDoc = XmlDocumentConverter.ConvertFrom(source);
            var target = XmlDocumentConverter.ConvertTo<OtherClass>(xmlDoc);

        }

        [Test]
        public void Test()
        {
            //var res = new OtherClass()
            //{
            //    Items = new SerializableDictionary<string, List<SomeClass>>()
            //};
            var source = new SerializableDictionary<string, List<PMConfig>>();
            source.Add("test1", new List<PMConfig> { new PMConfig() { ObjectName_r = "O1", MethodCode_r = "M1", ObjectEntitycode_R = "E1", PM2OperationCode_r = "OP1" } });
            source.Add("test2", new List<PMConfig> { new PMConfig() { ObjectName_r = "O2", MethodCode_r = "M2", ObjectEntitycode_R = "E2", PM2OperationCode_r = "OP2" } });
            //res.Items.Add("test1", new List<SomeClass> { new SomeClass() { A = "A1", B = "B1" } });
            //res.Items.Add("test2", new List<SomeClass> { new SomeClass() { A = "A2", B = "B2" } });

            var xmlDoc = XmlDocumentConverter.ConvertFrom(source);
            var target = XmlDocumentConverter.ConvertTo<SerializableDictionary<string, List<PMConfig>>>(xmlDoc);
        }
    }
}