using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class OutTest : BaseWMSObjectTest<Out>
    {
        private readonly EventKindTest _eventKindTest = new EventKindTest();
        private readonly BillOperationTest _billOperationTest = new BillOperationTest();
        private readonly MandantTest _mandantTest = new MandantTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _eventKindTest, _billOperationTest, _mandantTest };
        }

        protected override void FillRequiredFields(Out obj)
        {
            base.FillRequiredFields(obj);

            var eventKind = _eventKindTest.CreateNew();
            var operation = _billOperationTest.CreateNew();
            var mandant = _mandantTest.CreateNew();
            
            obj.AsDynamic().OUTID = TestDecimal;
            obj.AsDynamic().EVENTHEADERSTARTTIME_R = DateTime.Now;
            obj.AsDynamic().EVENTKINDCODE_R = eventKind.GetKey();

            // Не через тест, ибо EventHeaderTest - ignore 
            using (var mgr = IoC.Instance.Resolve<IBaseManager<EventHeader>>())
            {
                var ar = mgr.GetFiltered("ROWNUM <= 10").ToArray();
                ar.Should().NotBeNull("Not elements in EventHeader");
                if (ar.Length > 0)
                    obj.AsDynamic().EVENTHEADERID_R = ar[0].GetKey();
            }

            obj.AsDynamic().OPERATIONCODE_R = operation.GetKey();
            //Используем зарезервированного манданта для тестов
            obj.AsDynamic().PARTNERID_R = mandant.GetKey();
            obj.AsDynamic().MANDANTCODE_R = mandant.AsDynamic().MANDANTCODE;
            obj.AsDynamic().OUTTRYCOUNT = TestDecimal;
            obj.AsDynamic().STATUS = TestDecimal;
            obj.AsDynamic().TRXID = TestString;
            obj.AsDynamic().OUTOPERATIONBUSINESS = TestString;
            
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(OUTID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(Out obj)
        {
            obj.AsDynamic().OUTTYPE = TestString;
        }

        protected override void CheckSimpleChange(Out source, Out dest)
        {
            string sourceName = source.AsDynamic().OUTTYPE;
            string destName = dest.AsDynamic().OUTTYPE;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}