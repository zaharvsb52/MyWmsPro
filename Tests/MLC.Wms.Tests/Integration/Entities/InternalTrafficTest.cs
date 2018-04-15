using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public sealed class InternalTrafficTest : BaseEntityTest<InternalTraffic>
    {
        public const decimal ExistsItem1Code = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = InternalTraffic.InternalTrafficOrderPropertyName;
        }

        protected override void FillRequiredFields(InternalTraffic entity)
        {
            base.FillRequiredFields(entity);
            var obj = entity.AsDynamic();

            obj.INTERNALTRAFFICORDER = TestDecimal;
            obj.EXTERNALTRAFFICID_R = ExternalTrafficTest.ExistsItem1Code;
            obj.WAREHOUSECODE_R = WarehouseTest.ExistsItem1Code;
            obj.PURPOSEVISITID_R = PurposeVisitTest.ExistsItem1Code;
            obj.STATUSCODE_R = StatusTest.ExistsItem1Code;
            obj.MANDANTID = TstMandantId;
        }
    }
}