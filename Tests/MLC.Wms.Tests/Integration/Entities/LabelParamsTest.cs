using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class LabelParamsTest : BaseEntityTest<LabelParams>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = LabelParams.LabelParamsNamePropertyName;
        }

        protected override void FillRequiredFields(LabelParams entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.LABELCODE_R = LabelTest.ExistsItem1Code;
            obj.LABELPARAMSNAME = TestString;
        }
    }
}