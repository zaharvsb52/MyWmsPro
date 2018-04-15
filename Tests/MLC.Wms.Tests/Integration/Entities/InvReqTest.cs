using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class InvReqTest : BaseEntityTest<InvReq>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "InvReqHostRef";
        }

        protected override void FillRequiredFields(InvReq entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MandantID = TstMandantId;
            obj.InvReqName = TestString;
        }
    }
}