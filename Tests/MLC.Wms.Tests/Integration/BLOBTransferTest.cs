using System;
using System.Text;
using System.Xml;
using BLToolkit.DataAccess;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.DAL.Oracle;

namespace MLC.Wms.Tests.Integration
{
    [TestFixture]
    public class BLOBTransferTest
    {
        #region .  Classes  .
        public abstract class BLOBRepository : BaseDataAccessor<TestObject>
        {
            public TestObject GetXMLDocTestObject()
            {
                XmlDocument xmlDoc = null;
                using (var db = GetDbManager())
                {
                    xmlDoc = db.SetCommand("select spftstgetXMLDoc from dual").ExecuteScalar<XmlDocument>();
                }
                return XmlDocumentConverter.ConvertTo<TestObject>(xmlDoc);
            }

            public TestObject GetImageTestObject()
            {
                XmlDocument xmlDoc = null;
                using (var db = GetDbManager())
                {
                    xmlDoc = db.SetCommand("select spftstgetImage from dual").ExecuteScalar<XmlDocument>();
                }
                return XmlDocumentConverter.ConvertTo<TestObject>(xmlDoc);
            }
        }

        [SourceName("TSTBLOB")]
        public class TestObject : BusinessObject
        {
            public TestObject()
            {
                UnknownPropertySet = UnknownPropertySetMode.Ignore;
            }

            public string FKey
            {
                get { return GetProperty<string>("FKey"); }
                set { SetProperty("FKey", value); }
            }
            public DateTime FDate
            {
                get { return GetProperty<DateTime>("FDate"); }
                set { SetProperty("FDate", value); }
            }

            //public DateTime FTimeStamp { get; set; }
            public int FInt
            {
                get { return GetProperty<int>("FInt"); }
                set { SetProperty("FInt", value); }
            }

            public float FFloat
            {
                get { return GetProperty<float>("FFloat"); }
                set { SetProperty("FFloat", value); }
            }

            public string FBlob
            {
                get { return GetProperty<string>("FBlob"); }
                set { SetProperty("FBlob", value); }
            }

            //            protected override CustomPropertyCollection CreateCustomProperties()
            //            {
            //                var cln = new CustomPropertyCollection();
            //                var props = GetType().GetProperties();
            //                foreach (var p in props)
            //                {
            //                    var property = new CustomProperty(p.Name, p.PropertyType, Activator.CreateInstance(p.PropertyType));
            //                    property.Attributes.Add(new SourceNameAttribute(p.Name.ToUpper()));
            //                    property.Attributes.Add(new XmlElementAttribute(p.Name.ToUpper()));
            //
            //                    cln.Add(property);
            //                }
            //                return cln;
            //            }
        }
        #endregion

        #region .  Test methods  .
        [SetUp]
        public void Setup()
        {
            BLHelper.InitBL();
        }

        [Test]
        public void TestXml()
        {
            var da = DataAccessor.CreateInstance<BLOBRepository>();
            var testObj = da.GetXMLDocTestObject();
            var textXML = FromHexString(testObj.FBlob, Encoding.UTF8);
            textXML.Should().NotBeNull();
        }

        [Test]
        public void TestImage()
        {
            var da = DataAccessor.CreateInstance<BLOBRepository>();
            var testObj = da.GetImageTestObject();
            var bytes = HexString2Bytes(testObj.FBlob);
            bytes.Should().NotBeNull();
            //File.WriteAllBytes(@"F:\okos\Temp\file.png", bytes);
        }

        [Test, Ignore("TODO: Передача TimeStamp пока не работает - нужно договориться с Надей как будем поступать")]
        public void TestTimeStamp()
        {
            var da = DataAccessor.CreateInstance<BLOBRepository>();
            var testObj = da.GetImageTestObject();
        }

        #endregion

        private static byte[] HexString2Bytes(string hexString)
        {
            int bytesCount = (hexString.Length) / 2;
            byte[] bytes = new byte[bytesCount];
            for (int x = 0; x < bytesCount; ++x)
            {
                bytes[x] = Convert.ToByte(hexString.Substring(x * 2, 2), 16);
            }

            return bytes;
        }

        private static string FromHexString(string hexString, Encoding encoding)
        {
            int j = 0;
            var tmp = new byte[(hexString.Length) / 2];
            for (int i = 0; i <= (hexString.Length - 2); i += 2)
            {
                tmp[j] = (byte)Convert.ToChar(Int32.Parse(hexString.Substring(i, 2), System.Globalization.NumberStyles.HexNumber));

                j++;
            }
            return encoding.GetString(tmp);
        }
    }
}