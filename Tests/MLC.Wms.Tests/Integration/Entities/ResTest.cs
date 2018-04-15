using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class ResTest : BaseEntityTest<Res>
    {
        public const int ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "ResGroup";
        }

        protected override void FillRequiredFields(Res entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.ProductID_r = ProductTest.ExistsItem2Id;
            obj.OWBPosID_r = OWBPosTest.ExistsItem1Id;
            obj.MRCode_r = MRTest.ExistsItem1Code;
            obj.ResType = "NORMAL";
        }
    }
}