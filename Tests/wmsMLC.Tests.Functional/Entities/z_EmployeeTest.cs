using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class EmployeeTest : BaseWMSObjectTest<Employee>
    {
        private readonly PartnerTest _partnerTest = new PartnerTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _partnerTest };
        }

        protected override void FillRequiredFields(Employee obj)
        {
            base.FillRequiredFields(obj);

            var partner = _partnerTest.CreateNew();
            
            obj.AsDynamic().EMPLOYEEID = TestDecimal;
            obj.AsDynamic().PARTNERID_R = partner.GetKey();

            obj.AsDynamic().EMPLOYEELASTNAME = TestString;
            obj.AsDynamic().EMPLOYEENAME = TestString;
            obj.AsDynamic().EMPLOYEEMIDDLENAME = TestString;
            obj.AsDynamic().EMPLOYEEMOBILE = TestString;
            obj.AsDynamic().EMPLOYEEPHONEWORK = TestString;
            obj.AsDynamic().EMPLOYEEPHONEINTERNAL = TestString;
            obj.AsDynamic().EMPLOYEEEMAIL = TestString;
            obj.AsDynamic().EMPLOYEEOFFICIALCAPACITY = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(EMPLOYEEID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(Employee obj)
        {
            obj.AsDynamic().EMPLOYEEDEPARTMENT = string.Format("{0}Change", TestString);
        }

        protected override void CheckSimpleChange(Employee source, Employee dest)
        {
            string sourceName = source.AsDynamic().EMPLOYEEDEPARTMENT;
            string destName = dest.AsDynamic().EMPLOYEEDEPARTMENT;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}