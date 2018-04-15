using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class EntityFileTest : BaseEntityTest<EntityFile>
    {
        //public const decimal ExistsItem1Code = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "FILEDESC";
        }

        protected override void FillRequiredFields(EntityFile entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.FILE2ENTITY = TestString;
            obj.FILEKEY = TestString;
            obj.FILENAME = TestString;
        }

        [Test, Ignore("http://mp-ts-nwms/issue/wmsMLC-11508")]
        public override void Entity_should_have_history()
        {
            base.Entity_should_have_history();
        }
    }
}