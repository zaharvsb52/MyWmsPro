using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class Work2EntityTest : BaseEntityTest<Work2Entity>
    {
        public const decimal ExistsItem1Id = -1;
        protected override void FillRequiredFields(Work2Entity entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.WORK2ENTITYWORKID = WorkTest.ExistsItem1Id;
            obj.Work2EntityEntity = SysObjectTest.ExistsItemEntityCode;
            obj.Work2EntityKey = SysObjectTest.ExistsItemEntityCode;
        }
    }
}