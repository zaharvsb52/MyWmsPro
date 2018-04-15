using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillTransactionTest : BaseWMSObjectTest<BillTransaction>
    {
        private readonly BillBillerTest _billBillerTest = new BillBillerTest();
        private readonly BillTransactionTypeTest _billTransactionTypeTest = new BillTransactionTypeTest();
        //private readonly MandantTest _mandantTest = new MandantTest();
        private readonly IsoCurrencyTest _isoCurrencyTest = new IsoCurrencyTest();

        public BillTransactionTest()
        {
            TestDecimal = TestDecimal * (-1);
        }

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _billBillerTest, _billTransactionTypeTest, /*_mandantTest,*/ _isoCurrencyTest };
        }

        protected override void FillRequiredFields(BillTransaction obj)
        {
            base.FillRequiredFields(obj);

            var biller = _billBillerTest.CreateNew();
            var transactionType = _billTransactionTypeTest.CreateNew();
            //var manadant = _mandantTest.CreateNew();
            var currency = _isoCurrencyTest.CreateNew();

            obj.AsDynamic().TRANSACTIONID = TestDecimal;

            // Не через тест, ибо EventHeaderTest - ignore 
            using (var mgr = IoC.Instance.Resolve<IBaseManager<EventHeader>>())
            {
                var ar = mgr.GetFiltered("ROWNUM <= 10").ToArray();
                ar.Should().NotBeNull("Not elements in EventHeader");
                if (ar.Length > 0)
                    obj.AsDynamic().EVENTHEADERID_R = ar[0].GetKey();
            }

            obj.AsDynamic().BILLERCODE_R = biller.GetKey();
            obj.AsDynamic().TRANSACTIONTYPECODE_R = transactionType.GetKey();
            obj.AsDynamic().MANDANTID = 1;//manadant.GetKey();
            obj.AsDynamic().TRANSACTIONRECIPIENT = 1;//manadant.GetKey();
            obj.AsDynamic().TRANSACTIONAMMOUNT = TestDouble;
            obj.AsDynamic().CURRENCYCODE_R = currency.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(TRANSACTIONID = '{0}')", TestDecimal);
        }
    }
}