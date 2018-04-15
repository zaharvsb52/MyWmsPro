using System;
using System.Xml;
using BLToolkit.DataAccess;
using NUnit.Framework;
using wmsMLC.Business;
using wmsMLC.General;
using wmsMLC.Tests.Functional.Entities;

namespace wmsMLC.Tests.Functional
{
    [TestFixture]
    public class TListXmlTest
    {
        protected static double _lastQueryExecutionTime;

        [TestFixtureSetUp]
        public virtual void Setup()
        {
            BLHelper.InitBL(dalType: DALType.Oracle);
            var auth = IoC.Instance.Resolve<IAuthenticationProvider>();
            auth.Authenticate("TECH_AUTOTEST", "dVAdfX0iqheq4yd");
        }

        [Test, Ignore("Больше нет тестовой процедуры procTestXML")]
        public void Test()
        {
            var repo = DataAccessor.CreateInstance<TestDataAccessor>();
            XmlDocument[] teTypeXml;
            repo.procTestXMLInternal(out teTypeXml);
        }

        [Test, Ignore]
        public void XmlRootTest()
        {
            var repo = DataAccessor.CreateInstance<TestDataAccessor>();
            var now = DateTime.Now;
            var res = repo.GetSysObjectXml();
            Console.Write(_lastQueryExecutionTime);
        }
    }
}
