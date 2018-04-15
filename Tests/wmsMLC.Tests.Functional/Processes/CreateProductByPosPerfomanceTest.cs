using System.Diagnostics;
using System.Xml;
using NUnit.Framework;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Tests.Functional.Processes
{
    [TestFixture]
    public class CreateProductByPosPerfomanceTest : BaseWMSTest
    {
        [Test]
        public void SimpleTest()
        {
            const int cnt = 5;

            using (var mgr = IoC.Instance.Resolve<IBPProcessManager>())
            {
                string manageFlag = string.Empty;
                string manageParam = string.Empty;

                var xmlPos = new XmlDocument();
                xmlPos.LoadXml("<TENTIWBPOSINPUT><IWBPOSID>-1</IWBPOSID><SKUID>54651</SKUID><IWBPOSCOUNT>0</IWBPOSCOUNT><REQUIREDSKUCOUNT>1</REQUIREDSKUCOUNT><TETYPECODE>EURO</TETYPECODE><SKU2TTEQUANTITY>10000</SKU2TTEQUANTITY><SKU2TTEHEIGHT /><QLFCODE_R>QLFNORMAL</QLFCODE_R><IWBPOSBLOCKING /><IWBPOSEXPIRYDATE /><PRODUCTCOUNTSKU>0</PRODUCTCOUNTSKU><PRODUCTCOUNT>1</PRODUCTCOUNT><ARTCODE>ETN010145</ARTCODE><ARTDESC>F3A-D</ARTDESC><ARTINPUTDATEMETHOD>DAY</ARTINPUTDATEMETHOD><MEASURECODE>шт</MEASURECODE><IWBPOSCOLOR /><IWBPOSTONE /><IWBPOSSIZE /><IWBPOSBATCH /><IWBPOSPRODUCTDATE /><IWBPOSSERIALNUMBER /><FACTORYID_R /><IWBPOSTE>EUROTO201412081020</IWBPOSTE><IWBPOSLOT /><IWBPOSQLFDETAILDESCL /><SKU2TTEQUANTITYMAX>10000</SKU2TTEQUANTITYMAX><IWBPOSINPUTBATCHCODE /><IWBPOSBOXNUMBER /><IWBPOSKITARTNAME /><IWBPOSOWNER /><REMAINCOUNT /><POSSKUID /><POSIWBPOSCOUNT /><POSPRODUCTCOUNT /><SKUNAME /><TETYPENAME /><ARTNAME /><MEASURESHORTNAME /><QLFNAME /><PRODUCTBLOCKINGNAME /><ARTINPUTDATEMETHODNAME /><VFACTORYNAME /><VOWNERCODE /></TENTIWBPOSINPUT>");
                var posInput = XmlDocumentConverter.ConvertTo<IWBPosInput>(xmlPos);

                long totalElapsedMilliseconds = 0;

                for (int i = 0; i < cnt; i++)
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    var products = mgr.CreateProductByCargoIwb(ref manageFlag, ref manageParam, "OP_INPUT_PRD", posInput, "41D01001000000000", 6508);
                    sw.Stop();
                    totalElapsedMilliseconds += sw.ElapsedMilliseconds;
                    Debug.WriteLine("create {0} products in {1} ms", products == null ? 0 : products.Count, sw.ElapsedMilliseconds);
                }
                Debug.WriteLine("Total time is {0} ms ({1} per call)", totalElapsedMilliseconds, totalElapsedMilliseconds / cnt);
            }
        }
    }


    [TestFixture]
    public class XmlSimpleTest
    {
        [Test]
        public void Test()
        {
            var xml = new XmlDocument();
            xml.LoadXml("<CLASS><f1></f1><f2><i1></i1><i2></i2></f2></CLASS>");

            using (var reader = new XmlNodeReader(xml))
            {
                ReadXmlInternal(reader);
            }
        }

        private void ReadXmlInternal(XmlReader reader)
        {
            // пропускаем root элемент
            //            if (!reader.Read())
            //                return;
            //
            //            if (!reader.Read())
            //                return;
            if (reader.ReadState == ReadState.Initial)
                reader.Read();
            var rootName = reader.Name;
            var instanceCount = 1;
            // внутри - содержимое
            while (reader.Read())
            {
                if (reader.Name == rootName)
                {
                    // дочитали до конца - выходим
                    if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        instanceCount--;
                        if (instanceCount == 0)
                            break;
                    }
                    instanceCount++;
                }

                // нас интересуют только заполненные элементы
                if (reader.NodeType != XmlNodeType.Element || reader.IsEmptyElement)
                    continue;

                // вычитываем содержимое
                var name = reader.Name;
            }
        }

    }
}