using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class MRSelectTest : BaseEntityTest<MRSelect>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "MRSelectOWBRType";
        }

        protected override void FillRequiredFields(MRSelect entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MRCode_r = MRTest.ExistsItem1Code;
            obj.Priority = TestDecimal;
            obj.MandantID = TstMandantId;
            obj.MRSelectCalcSKU = TestBool;
        }
    }
}