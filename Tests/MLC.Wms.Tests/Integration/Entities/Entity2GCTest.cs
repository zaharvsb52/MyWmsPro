using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    class Entity2GCTest : BaseEntityTest<Entity2GC>
    {
        //public const decimal ExistsItem1Code = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "ENTITY2GCENTITY";
        }

        protected override void FillRequiredFields(Entity2GC entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.ENTITY2GCID = TestDecimal;
            obj.ENTITY2GCENTITY = TestString;
            obj.ENTITY2GCGCCODE = GCTest.ExistsItem1Code;
            obj.ENTITY2GCKEY = TestDecimal;
            obj.ENTITY2GCATTR = TestString;
        }
    }
}