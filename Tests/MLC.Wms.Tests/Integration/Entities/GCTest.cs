using System;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    class GCTest : BaseEntityTest<GlobalConfig>
    {
        public static Guid ExistsItem1Code = new Guid("00000000-0000-0000-0000-000000000001");

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "GCENTITY";
        }

        protected override void FillRequiredFields(GlobalConfig entity)
        {
            base.FillRequiredFields(entity);
            var obj = entity.AsDynamic();

            obj.GCENTITY = SysObjectTest.ExistsItemEntityCode;
            obj.GCKEY = TestString;
        }
    }
}