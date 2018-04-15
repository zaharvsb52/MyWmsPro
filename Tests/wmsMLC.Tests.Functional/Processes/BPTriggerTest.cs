using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects.Processes;
using wmsMLC.General;

namespace wmsMLC.Tests.Functional.Processes
{
    [TestFixture, Ignore("Временно в игноре")]
    public class BPTriggerTest : BaseWMSObjectTest<BPTrigger>
    {
        private const string TestString = "AutoTest";

        protected override string GetCheckFilter()
        {
            return string.Format("({0} = '{1}')", "TriggerCode", TestString);
        }

        protected override void FillRequiredFields(BPTrigger obj)
        {
            // создаем обязательные объекты
            var bp = new BPProcessTest().CreateNew();

            // заполняем поля
            obj.AsDynamic().TriggerCode = TestString;
            obj.AsDynamic().ProcessCode_r = bp.GetKey();

            // отслеживаем себя же
            obj.AsDynamic().ObjectName_r = SourceNameHelper.Instance.GetSourceName(typeof(BPTrigger));
        }

        public override void ClearForSelf(BPTrigger obj)
        {
            base.ClearForSelf(obj);

            // очищаем зависимости
            new BPProcessTest().ClearForSelfByKey(obj.AsDynamic().ProcessCode_r);
        }

        protected override void MakeSimpleChange(BPTrigger obj)
        {
            obj.AsDynamic().TriggerAction = TestString;
        }

        protected override void CheckSimpleChange(BPTrigger source, BPTrigger dest)
        {
            string sourceChange = source.AsDynamic().TriggerAction;
            string destChange = dest.AsDynamic().TriggerAction;
            // проверяем
            sourceChange.ShouldBeEquivalentTo(destChange);
        }
    }
}