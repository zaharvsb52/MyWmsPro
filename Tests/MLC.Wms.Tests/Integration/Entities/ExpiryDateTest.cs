using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class ExpiryDateTest : BaseEntityTest<ExpiryDate>
    {
        //public const decimal ExistsItem1Code = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = ExpiryDate.ExpiryDateValuePropertyName;
        }

        protected override void FillRequiredFields(ExpiryDate entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MANDANTID = TstMandantId;
            obj.PRIORITY = TestDecimal;
            obj.EXPIRYDATETYPE = TestString;
            obj.EXPIRYDATEVALUE = TestDecimal;
            obj.EXPIRYDATEVALUETYPE = TestString;
            obj.EXPIRYDATEUSINGOPTION = TestString;
        }
    }
}