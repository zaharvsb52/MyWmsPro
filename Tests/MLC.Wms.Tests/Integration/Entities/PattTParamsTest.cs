using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class PattTParamsTest : BaseEntityTest<PattTParams>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "TEMPLATEPARAMSDESC";
        }

        protected override void FillRequiredFields(PattTParams entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.TEMPLATEDATASOURCECODE_R = PattTDataSourceTest.ExistsItem1Code;
            obj.TEMPLATEPARAMSCODE = TestString;
            obj.TEMPLATEPARAMSNAME = TestString;
            obj.TEMPLATEPARAMSDATATYPE = TestDecimal;
        }
    }
}