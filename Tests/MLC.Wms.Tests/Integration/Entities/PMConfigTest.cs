using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class PMConfigTest : BaseEntityTest<PMConfig>
    {
        protected override void FillRequiredFields(PMConfig entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.PM2OPERATIONCODE_R = PM2OperationTest.ExistsItem1Code;
            obj.OBJECTENTITYCODE_R = entity.GetType().Name.ToUpper();
            obj.OBJECTNAME_R = PMConfig.PM2OperationCodePropertyName;
            obj.PMMETHODCODE_R = PMMethodTest.ExistsItem1Code;
        }
    }
}