using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture, Ignore("Проблема с 2 ClientSessionTest")]
    public class W2E2WorkingTest : BaseWMSObjectTest<W2E2Working>
    {
        private readonly WorkingTest _workingTest = new WorkingTest();
        private readonly Work2EntityTest _work2EntityTest = new Work2EntityTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _workingTest, _work2EntityTest };
        }

        protected override void FillRequiredFields(W2E2Working obj)
        {
            base.FillRequiredFields(obj);

            var work2Entity = _work2EntityTest.CreateNew();
            _workingTest.TestDecimal = TestDecimal + 1;
            _workingTest.TestString = TestString + "1";
            var working = _workingTest.CreateNew();

            obj.AsDynamic().W2E2WORKINGID = TestDecimal;
            obj.AsDynamic().W2E2WORKINGWORKINGID = working.GetKey();
            obj.AsDynamic().W2E2WORKINGWORK2ENTITYID = work2Entity.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(W2E2WORKINGID = '{0}')", TestDecimal);
        }
    }
}