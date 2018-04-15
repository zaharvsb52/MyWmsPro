using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillEventKind2BillerTest : BaseWMSObjectTest<BillEventKind2Biller>
    {
        private readonly EventKindTest _eventKindTest = new EventKindTest();
        private readonly BillBillerTest _billBillerTest = new BillBillerTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _eventKindTest, _billBillerTest };
        }

        protected override void FillRequiredFields(BillEventKind2Biller obj)
        {
            base.FillRequiredFields(obj);

            var eventKind = _eventKindTest.CreateNew();
            var biller = _billBillerTest.CreateNew();

            obj.AsDynamic().EVENTKIND2BILLERID = TestDecimal;
            obj.AsDynamic().BILLEVENTKIND2BILLEREVENTKINDCODE = eventKind.GetKey();
            obj.AsDynamic().BILLEVENTKIND2BILLERBILLERCODE = biller.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(EVENTKIND2BILLERID = '{0}')", TestDecimal);
        }
    }
}