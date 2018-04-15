using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class EmployeeTest : BaseEntityTest<Employee>
    {
        public const decimal ExistsItem1Code = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = Employee.EMPLOYEEDEPARTMENTPropertyName;
        }

        protected override void FillRequiredFields(Employee entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.PARTNERID_R = PartnerTest.ExistsItem1Id;
            obj.EMPLOYEELASTNAME = TestString;
            obj.EMPLOYEENAME = TestString;
            obj.EMPLOYEEMIDDLENAME = TestString;
            obj.EMPLOYEEMOBILE = TestString;
            obj.EMPLOYEEPHONEWORK = TestString;
            obj.EMPLOYEEPHONEINTERNAL = TestString;
            obj.EMPLOYEEEMAIL = TestString;
            obj.EMPLOYEEOFFICIALCAPACITY = TestString;
        }
    }
}