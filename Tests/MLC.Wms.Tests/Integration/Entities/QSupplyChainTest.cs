using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class QSupplyChainTest : BaseEntityTest<QSupplyChain>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "QSUPPLYCHAINPROCESS";
        }

        protected override void FillRequiredFields(QSupplyChain entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MANDANTID = TstMandantId;
        }
    }
}