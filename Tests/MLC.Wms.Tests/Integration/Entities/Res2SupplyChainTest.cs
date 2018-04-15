using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class Res2SupplyChainTest : BaseEntityTest<Res2SupplyChain>
    {
        protected override void FillRequiredFields(Res2SupplyChain entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.RES2SUPPLYCHAINRESID = ResTest.ExistsItem1Id;
            obj.RES2SUPPLYCHAINSUPPLYCHAINID = SupplyChainTest.ExistsItem1Id;
        }
    }
}