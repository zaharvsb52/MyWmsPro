using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class QSupplyChainTest : BaseWMSObjectTest<QSupplyChain>
    {
        private readonly MandantTest _mandantTest = new MandantTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _mandantTest };
        }

        protected override void FillRequiredFields(QSupplyChain obj)
        {
            base.FillRequiredFields(obj);

            var m = _mandantTest.CreateNew();

            obj.AsDynamic().QSUPPLYCHAINID = TestDecimal;
            obj.AsDynamic().MANDANTID = m.GetKey();
            
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(QSUPPLYCHAINID = '{0}')", TestDecimal);
        }
    }
}