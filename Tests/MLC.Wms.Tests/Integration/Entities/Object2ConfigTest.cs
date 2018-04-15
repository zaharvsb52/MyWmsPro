using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class Object2ConfigTest : BaseEntityTest<Object2Config>
    {
        protected override void FillRequiredFields(Object2Config entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.OBJECT2CONFIGOBJECTCONFIGCODE = ObjectConfigTest.ExistsItem1Code;
            obj.OBJECT2CONFIGOBJECTNAME = SysObjectTest.ExistsItemEntityCode;
        }
    }
}