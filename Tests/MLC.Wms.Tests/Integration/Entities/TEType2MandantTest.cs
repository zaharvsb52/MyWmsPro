using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class TEType2MandantTest : BaseEntityTest<TEType2Mandant>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "TETYPE2MANDANTHOSTREF";
        }

        protected override void FillRequiredFields(TEType2Mandant entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.TETYPE2MANDANTTETYPECODE = TETypeTest.ExistsItem2Code;
            obj.MANDANTID = TstMandantId;
            obj.TETYPE2MANDANTAUTOCREATE = TestBool;
            
        }
    }
}