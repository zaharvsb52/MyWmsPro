using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class InvReqPosTest : BaseEntityTest<InvReqPos>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "InvReqPosHostRef";
        }

        protected override void FillRequiredFields(InvReqPos entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.InvReqID_r = InvReqTest.ExistsItem1Id;
            obj.InvReqPosNumber = TestDecimal;
            obj.ArtCode_r = ArtTest.ExistsItem1Code;
            obj.InvReqPosArtName = TestString;
        }
    }
}