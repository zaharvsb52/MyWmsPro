using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class IWBTest : BaseEntityTest<IWB>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "IWBDesc";
        }

        protected override void FillRequiredFields(IWB entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MandantID = TstMandantId;
            obj.IWBName = TestString;
            obj.IWBPriority = TestDecimal;
            obj.IWBType = TestString;
        }
    }
}