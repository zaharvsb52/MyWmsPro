using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture, Ignore ("does not contain a definition for 'EventHeaderStartTime_r")]
    public class OutTest : BaseEntityTest<Out>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "OutMessage";
        }

        protected override void FillRequiredFields(Out entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.EventHeaderStartTime_r = TestString;
            obj.StatusCode_r = TestString;
            obj.EventKindCode_r = TestString;
            obj.EventHeaderID_r = TestString;
            obj.OperationCode_r = TestString;
            obj.OutOperationBusiness = TestString;
            obj.OutOperationBusiness = TestString;
            obj.PartnerID_r = TestString;
            obj.MandantCode_r = TestString;

        }
    }
}