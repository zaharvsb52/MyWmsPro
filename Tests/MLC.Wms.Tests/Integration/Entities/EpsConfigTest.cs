using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class EpsConfigTest : BaseEntityTest<EpsConfig>
    {
        //public const decimal ExistsItem1Code = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "EPSCONFIGDESC";
        }

        protected override void FillRequiredFields(EpsConfig entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.EPSCONFIG2ENTITY = TestString;
            obj.EPSCONFIGKEY = TestString;
            obj.EPSCONFIGPARAMCODE = TestString;
            obj.EPSCONFIGSTRONGUSE = TestBool;
            obj.EPSCONFIGLOCKED = TestBool;
        }

        [Test, Ignore("http://mp-ts-nwms/issue/wmsMLC-11508")]
        public override void Entity_should_have_history()
        {
            base.Entity_should_have_history();
        }
    }
}