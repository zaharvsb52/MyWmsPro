using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects.Processes;
using wmsMLC.General;

namespace wmsMLC.Tests.Functional.Processes
{
    [TestFixture,Ignore("Временно в игноре")]
    public class BPProcess2ObjectTest : BaseWMSObjectTest<BPProcess2Object>
    {
        //private const string TestString = "AutoTest";

        protected override string GetCheckFilter()
        {
            return null;
            //return string.Format("({0} = '{1}')", "TriggerCode", TestString);
        }

        protected override void FillRequiredFields(BPProcess2Object obj)
        {
            // создаем обязательные объекты
            var bp = new BPProcessTest().CreateNew();

            // заполняем поля
            obj.AsDynamic().BPPROCESS2OBJECTPROCESSCODE = bp.GetKey();
            obj.AsDynamic().BPPROCESS2OBJECTOBJECTNAME = SourceNameHelper.Instance.GetSourceName(typeof(BPProcess2Object));
        }

        public override void ClearForSelf(BPProcess2Object obj)
        {
            base.ClearForSelf(obj);

            // очищаем зависимости
            new BPProcessTest().ClearForSelfByKey(obj.AsDynamic().ProcessCode_r);
        }

        protected override void MakeSimpleChange(BPProcess2Object obj)
        {
            obj.AsDynamic().BPPROCESS2OBJECTOBJECTNAME = SourceNameHelper.Instance.GetSourceName(typeof(BPProcess));
        }

        protected override void CheckSimpleChange(BPProcess2Object source, BPProcess2Object dest)
        {
            string sourceChange = source.AsDynamic().BPPROCESS2OBJECTOBJECTNAME;
            string destChange = dest.AsDynamic().BPPROCESS2OBJECTOBJECTNAME;
            // проверяем
            sourceChange.ShouldBeEquivalentTo(destChange);
        }
    }
}