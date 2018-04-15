using System;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects.Processes;
using wmsMLC.Tests.Functional.Entities;

namespace wmsMLC.Tests.Functional.Processes
{
    [TestFixture,Ignore("Временно в игноре")]
    public class BPLogTest : BaseWMSObjectTest<BPLog>
    {
        private const string TestString = "AutoTest";
        private const string TestInstanceStr = "{EF21F1CF-4507-4B07-B978-B8F4C1E8ED5A}";

        protected override string GetCheckFilter()
        {
            return string.Format("({0} = '{1}')", "BPLOGINSTANCE", TestInstanceStr);
        }

        protected override void FillRequiredFields(BPLog obj)
        {
            // создаем обязательные объекты
            var bp = new BPProcessTest().CreateNew();
            var user = new UserTest().CreateNew();

            // заполняем поля
            //area.AsDynamic().BPLOGID = TestString;
            obj.AsDynamic().PROCESSCODE_R = bp.GetKey();
            obj.AsDynamic().USERCODE_R = user.GetKey();
            obj.AsDynamic().BPLOGINSTANCE = new Guid(TestInstanceStr).ToString();
            obj.AsDynamic().BPLOGCURRENTSTEP = TestString;
            obj.AsDynamic().BPLOGSTATUS = TestString;
            obj.AsDynamic().BPLOGSTARTTIME = DateTime.Now.AddDays(-5);
            obj.AsDynamic().BPLOGENDTIME = DateTime.Now;
        }

        public override void ClearForSelf(BPLog obj)
        {
            base.ClearForSelf(obj);

            // очищаем зависимости
            new BPProcessTest().ClearForSelfByKey(obj.AsDynamic().PROCESSCODE_R);
            new UserTest().ClearForSelfByKey(obj.AsDynamic().USERCODE_R);
        }

        protected override void MakeSimpleChange(BPLog obj)
        {
            obj.AsDynamic().BPLOGENDTIME = DateTime.Now.AddMinutes(-5);
        }

        protected override void CheckSimpleChange(BPLog source, BPLog dest)
        {
            string sourceChange = source.AsDynamic().BPLOGENDTIME.ToString("yyyyMMdd HH:mm:ss");
            string destChange = dest.AsDynamic().BPLOGENDTIME.ToString("yyyyMMdd HH:mm:ss");
            // проверяем
            sourceChange.ShouldBeEquivalentTo(destChange);
        }
    }
}