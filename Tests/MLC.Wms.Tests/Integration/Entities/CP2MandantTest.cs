using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class CP2MandantTest : BaseEntityTest<CP2Mandant>
    {
        //public const decimal ExistsItem1Code = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "CP2MANDANTORDER";
        }

        protected override void FillRequiredFields(CP2Mandant entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MANDANTID = TstMandantId;
            obj.CP2MANDANTCUSTOMPARAMCODE = CustomParamTest.ExistsItem1Code;
            obj.CP2MANDANTMUSTSET = false;
            obj.CP2MANDANTORDER = TestDecimal;
        }
    }
}