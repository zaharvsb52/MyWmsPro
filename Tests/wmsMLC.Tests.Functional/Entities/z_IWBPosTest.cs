using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class IWBPosTest : BaseWMSObjectTest<IWBPos>
    {
        public readonly string TestObjectName = "IWBPOS";

        private readonly IWBTest _iwbTest = new IWBTest();
        private readonly SKUTest _skuTest = new SKUTest();

        protected override void FillRequiredFields(IWBPos obj)
        {
            base.FillRequiredFields(obj);

            _iwbTest.TestDecimal = 125;
            var iwb = _iwbTest.CreateNew();

            _skuTest.TestDecimal = 126;
            var sku = _skuTest.CreateNew();
            //var status2Entity = _status2EntityTest.CreateNew(p => p.AsDynamic().STATUS2ENTITYVALUE = TestObjectName);

            //_status2Entity.AsDynamic().STATUS2ENTITYVALUE = TestObjectName;

            obj.AsDynamic().IWBPOSID = TestDecimal;
            obj.AsDynamic().IWBID_R = iwb.GetKey();
            obj.AsDynamic().IWBPOSNUMBER = TestDecimal;
            obj.AsDynamic().SKUID_R = sku.GetKey();
            obj.AsDynamic().IWBPOSCOUNT = TestDecimal;
            //obj.AsDynamic().STATUSCODE_R = status2Entity.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(IWBPOSID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(IWBPos obj)
        {
            obj.AsDynamic().IWBPOSTONE = "new"+TestString;
        }

        [Test,Ignore("Зацикливание теста")]
        public override void ManagerCRUDTest()
        {

        }
        [Test, Ignore("Зацикливание теста")]
        public override void ManagerGetFilteredTest()
        {
             
        }
        [Test, Ignore("Зацикливание теста")]
        public override void ManagerGetAllTest()
        {
            
        }

        protected override void CheckSimpleChange(IWBPos source, IWBPos dest)
        {
            string sourceName = source.AsDynamic().IWBPOSTONE;
            string destName = dest.AsDynamic().IWBPOSTONE;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _iwbTest, _skuTest };
        }
    }
}