using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class PM2ArtTest : BaseWMSObjectTest<PM2Art>
    {
        private readonly ArtTest _artTest = new ArtTest();
        private readonly ArtGroupTest _artGroupTest = new ArtGroupTest();
        private readonly PMTest _pmTest = new PMTest();
        
        public PM2ArtTest()
        {
            _artTest.TestString = TestString;
            _artGroupTest.TestString = TestString;
            _pmTest.TestString = TestString;
        }

        protected override void FillRequiredFields(PM2Art obj)
        {
            base.FillRequiredFields(obj);

            var art = _artTest.CreateNew();
            var artGr = _artGroupTest.CreateNew();
            var pm = _pmTest.CreateNew();

            obj.AsDynamic().PM2ARTID = TestDecimal;
            obj.AsDynamic().PM2ARTARTCODE = art.GetKey();
            obj.AsDynamic().PM2ARTARTGROUPCODE = artGr.GetKey();
            obj.AsDynamic().PM2ARTPMCODE = pm.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(PM2ARTID = '{0}')", TestDecimal);
        }
        
        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _artGroupTest, _artTest, _pmTest };
        }
    }
}