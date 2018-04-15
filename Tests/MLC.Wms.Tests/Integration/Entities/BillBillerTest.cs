using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillBillerTest : BaseEntityTest<BillBiller>
    {
        public const string ExistsItem1Code = "TST_BILLBILLER_1";
        public const string ExistsItem2Code = "TST_BILLBILLER_2";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "BILLERLOCKED";
        }

        protected override void FillRequiredFields(BillBiller entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.BILLERNAME = TestString;
            obj.BILLERPROCEDURECALC = TestString;
        }
    }
}