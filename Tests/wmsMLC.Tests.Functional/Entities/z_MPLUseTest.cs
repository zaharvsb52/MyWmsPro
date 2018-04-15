using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class MPLUseTest : BaseWMSObjectTest<MPLUse>
    {
        private readonly PLTest _mplTest = new PLTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _mplTest };
        }

        protected override void FillRequiredFields(MPLUse obj)
        {
            base.FillRequiredFields(obj);

            var mpl = _mplTest.CreateNew();

            obj.AsDynamic().MPLUSEID = TestDecimal;
            obj.AsDynamic().MPLCODE_R = mpl.GetKey();
            obj.AsDynamic().MPLUSEBYOWB = false;
            obj.AsDynamic().MPLUSEBYART = false;
            obj.AsDynamic().MPLUSEBYARTGROUP = false;
            obj.AsDynamic().MPLUSEWEIGHT = TestDecimal;
            obj.AsDynamic().MPLUSEVOLUME = TestDecimal;
            obj.AsDynamic().MPLUSEBYSEGMENT = false;
            obj.AsDynamic().MPLUSELINE = TestDecimal;
            obj.AsDynamic().MPLUSEPICKCONTROLMETHOD = "PLPOSNONE";
            
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(MPLUSEID = '{0}')", TestDecimal);
        }
    }
}