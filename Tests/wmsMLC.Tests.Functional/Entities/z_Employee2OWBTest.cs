using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class Employee2OWBTest : BaseWMSObjectTest<Employee2OWB>
    {
        private readonly EmployeeTest _employeeTest = new EmployeeTest();
        private readonly OWBTest _owbTest = new OWBTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _employeeTest, _owbTest };
        }


        protected override void FillRequiredFields(Employee2OWB obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().EMPLOYEE2OWBID = TestDecimal;
            obj.AsDynamic().EMPLOYEE2OWBEMPLOYEEID = _employeeTest.CreateNew().GetKey();
            obj.AsDynamic().EMPLOYEE2OWBOWBID = _owbTest.CreateNew().GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(EMPLOYEE2OWBID = '{0}')", TestDecimal);
        }
    }
}