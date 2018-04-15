using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class Config2ObjectTest : BaseEntityTest<Config2Object>
    {
        public const int ExistsItem1Id = -1;

        protected override void FillRequiredFields(Config2Object entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.CONFIG2OBJECTOBJECTCONFIGCODE = ObjectConfigTest.ExistsItem1Code;
            obj.CONFIG2OBJECTOBJECTENTITYCODE = SysObjectTest.ExistsItem1Code;
            obj.CONFIG2OBJECTOBJECTNAME = SysObjectTest.ExistsItem1Code;
        }
    }
}