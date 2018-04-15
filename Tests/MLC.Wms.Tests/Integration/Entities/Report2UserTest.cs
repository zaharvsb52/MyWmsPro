using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class Report2UserTest : BaseEntityTest<Report2User>
    {
        protected override void FillRequiredFields(Report2User entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.REPORT2USERREPORT = ReportTest.ExistsItem1Code;
            obj.REPORT2USERUSERCODE = CurrentUser;
        }
    }
}