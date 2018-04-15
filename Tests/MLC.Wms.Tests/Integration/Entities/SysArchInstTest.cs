using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    class SysArchInstTest : BaseEntityTest<SysArchInst>
    {
        protected override void FillRequiredFields(SysArchInst entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            //obj.ARCHINSTGUID = TestGuid;
            obj.ARCHCODE_R = SysArchTest.ExistsItem1Code;
            obj.ARCHINSTDATE = TestDateTime;
        }
    }
}
