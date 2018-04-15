using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class OutputTest : BaseWMSObjectTest<Output>
    {
        private readonly UserTest _userTest = new UserTest();

        public OutputTest()
        {
            _userTest.TestString = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(upper(LOGIN_R) like upper('{0}%'))", AutoTestMagicWord);
        }

        protected override void FillRequiredFields(Output obj)
        {
            base.FillRequiredFields(obj);

            var user = _userTest.CreateNew();

            obj.AsDynamic().OUTPUTID = TestDecimal;
            obj.AsDynamic().LOGIN_R = user.GetKey();
            obj.AsDynamic().HOST_R = TestString;
            obj.AsDynamic().EPSHANDLER = TestDecimal;
            obj.AsDynamic().OUTPUTSTATUS = TestString;
        }

        protected override void MakeSimpleChange(Output obj)
        {
            obj.AsDynamic().OUTPUTFEEDBACK = TestString;
        }

        protected override void CheckSimpleChange(Output source, Output dest)
        {
            string sourceName = source.AsDynamic().OUTPUTFEEDBACK;
            string destName = dest.AsDynamic().OUTPUTFEEDBACK;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _userTest };
        }

        [Test, Ignore("Тест не запускаем. Много данных")]
        public override void ManagerGetAllTest()
        {
            //base.ManagerGetAllTest();
        }

        [Test, Ignore("Хистори нет")]
        public override void ManagerGetHistoryTest()
        {
            
        }
    }
}