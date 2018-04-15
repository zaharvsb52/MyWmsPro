using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture, Ignore]
    public class BillEvent2BillerTest : BaseWMSObjectTest<BillEvent2Biller>
    {
        private readonly BillBillerTest _billBillerTest= new BillBillerTest();
        
        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _billBillerTest };
        }

        protected override void FillRequiredFields(BillEvent2Biller obj)
        {
            base.FillRequiredFields(obj);

            var biller = _billBillerTest.CreateNew();
            var events = (List<EventHeader>)IoC.Instance.Resolve<EventHeaderManager>().GetAll();

            obj.AsDynamic().EVENT2BILLERID = TestDecimal;
            obj.AsDynamic().BILLEVENT2BILLEREVENTHEADERID = events[0].GetKey();
            obj.AsDynamic().BILLEVENT2BILLERBILLERCODE = biller.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(EVENT2BILLERID = '{0}')", TestDecimal);
        }
    }
}