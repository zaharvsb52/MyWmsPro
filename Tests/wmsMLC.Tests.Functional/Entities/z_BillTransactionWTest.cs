using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillTransactionWTest : BaseWMSObjectTest<BillTransactionW>
    {
        private readonly BillBillerTest _billBillerTest = new BillBillerTest();
        private readonly BillTransactionTypeTest _billTransactionTypeTest = new BillTransactionTypeTest();
        //private readonly MandantTest _mandantTest = new MandantTest();
        private readonly WorkerTest _workerTest = new WorkerTest();
        private readonly IsoCurrencyTest _isoCurrencyTest = new IsoCurrencyTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _billBillerTest, _billTransactionTypeTest, /*_mandantTest,*/ _workerTest, _isoCurrencyTest };
        }

        protected override void FillRequiredFields(BillTransactionW obj)
        {
            base.FillRequiredFields(obj);

            var mgr = IoC.Instance.Resolve<EventHeaderManager>();
            //var evs = (List<EventHeader>) mgr.GetAll();

            var biller = _billBillerTest.CreateNew();
            var transactionType = _billTransactionTypeTest.CreateNew();
            //var mandant = _mandantTest.CreateNew();
            var worker = _workerTest.CreateNew();
            var currency = _isoCurrencyTest.CreateNew();

            obj.AsDynamic().TRANSACTIONWID = TestDecimal;
            //obj.AsDynamic().EVENTHEADERID_R = evs[0].GetKey();
            obj.AsDynamic().EVENTHEADERID_R = TestDecimal;
            obj.AsDynamic().BILLERCODE_R = biller.GetKey();
            obj.AsDynamic().TRANSACTIONTYPECODE_R = transactionType.GetKey();
            obj.AsDynamic().MANDANTID = 1;// mandant.GetKey();
            obj.AsDynamic().WORKERID_R = worker.GetKey();
            obj.AsDynamic().TRANSACTIONWAMMOUNT = TestDouble;
            obj.AsDynamic().CURRENCYCODE_R = currency.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(TRANSACTIONWID = '{0}')", TestDecimal);
        }
    }
}