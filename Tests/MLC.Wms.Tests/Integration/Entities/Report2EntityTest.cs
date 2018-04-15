using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class Report2EntityTest : BaseEntityTest<Report2Entity>
    {
        protected override void FillRequiredFields(Report2Entity entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.REPORT2ENTITYREPORT = ReportTest.ExistsItem1Code;
            obj.REPORT2ENTITYOBJECTNAME = SysObjectTest.ExistsItemEntityCode;
        }
    }
}