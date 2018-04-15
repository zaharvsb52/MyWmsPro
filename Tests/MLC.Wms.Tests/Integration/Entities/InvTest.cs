using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class InvTest : BaseEntityTest<Inv>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "INVNAME";
        }

        protected override void FillRequiredFields(Inv entity)
        {
            base.FillRequiredFields(entity);
            var obj = entity.AsDynamic();

            obj.INVNAME = TestString;
            obj.MANDANTID = TstMandantId;
            obj.MICODE_R = MiTest.ExistsItem1Code;
        }
    }
}
