using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class Employee2OWBTest : BaseEntityTest<Employee2OWB>
    {
        //public const string ExistsItem1Code = "TST_EMPLOYEE2OWB_1";

        protected override void FillRequiredFields(Employee2OWB entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.EMPLOYEE2OWBEMPLOYEEID = EmployeeTest.ExistsItem1Code;
            obj.EMPLOYEE2OWBOWBID = OwbTest.ExistsItem1Code;
        }
    }
}