using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class SupplyChainTest : BaseEntityTest<SupplyChain>
    {
        // -1 использовать нельзя, т.к. в логике работы поставок -1 - это "петелька"
        public const int ExistsItem1Id = -9;

        protected override void FillRequiredFields(SupplyChain entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MSCCode_r = MSCTest.ExistsItem1Code;
            obj.SupplyChainSourceSupplyArea = SupplyAreaTest.ExistsItem1Code;
            obj.SupplyChainTargetSupplyArea = SupplyAreaTest.ExistsItem1Code;
            obj.OperationCode_r = BillOperationTest.ExistsItem1Code;
        }
    }
}