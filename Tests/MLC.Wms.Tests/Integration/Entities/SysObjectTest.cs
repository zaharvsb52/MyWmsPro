using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class SysObjectTest : BaseEntityTest<SysObject>
    {
        public const decimal ExistsItem1Code = -1;
        public const string ExistsItemEntityCode = "TST_SYSOBJECT_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            // обновлять SysObject нельзя, т.к. он "хитро" кэшируемый и мы не найдем записи
            SimpleChangePropertyName = null;
        }

        protected override void FillRequiredFields(SysObject entity)
        {
            base.FillRequiredFields(entity);
            entity.ObjectName = TestString;
            entity.ObjectEntityCode = TestString;
        }
    }
}