using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class Partner2GroupTest : BaseWMSObjectTest<Partner2Group>
    {
        private readonly PartnerGroupTest _partnerGroupTest = new PartnerGroupTest();
        private readonly PartnerTest _partnerTest = new PartnerTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _partnerGroupTest, _partnerTest };
        }

        protected override void FillRequiredFields(Partner2Group obj)
        {
            base.FillRequiredFields(obj);

            var pg = _partnerGroupTest.CreateNew();
            var p = _partnerTest.CreateNew();

            obj.AsDynamic().PARTNER2GROUPID = TestDecimal;
            obj.AsDynamic().PARTNER2GROUPPARTNERGROUPID = pg.GetKey();
            obj.AsDynamic().PARTNER2GROUPPARTNERID = p.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(PARTNER2GROUPID = '{0}')", TestDecimal);
        }
    }
}