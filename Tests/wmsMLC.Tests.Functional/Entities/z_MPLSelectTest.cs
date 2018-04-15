using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class MPLSelectTest : BaseWMSObjectTest<MPLSelect>
    {
        private readonly PLTest _mplTest = new PLTest();
        private readonly AreaTest _areaTest = new AreaTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _mplTest, _areaTest };
        }

        protected override void FillRequiredFields(MPLSelect obj)
        {
            base.FillRequiredFields(obj);

            var mpl = _mplTest.CreateNew();
            var area = _areaTest.CreateNew();

            obj.AsDynamic().MPLSELECTID = TestDecimal;
            obj.AsDynamic().MPLCODE_R = mpl.GetKey();
            obj.AsDynamic().PRIORITY = TestDecimal;
            obj.AsDynamic().MANDANTID = 1;
            obj.AsDynamic().AREACODE_R = area.GetKey();
            obj.AsDynamic().SUPPLYAREACODE_R = TestString;
            obj.AsDynamic().MPLSELECTOWBCRITPL = TestString;
            obj.AsDynamic().MPLSELECTSKUCRITPL = TestString;
            obj.AsDynamic().MPLSELECTARTGROUPCRITPL = TestString;
            obj.AsDynamic().QLFCODE_R = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(MPLSELECTID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(MPLSelect obj)
        {
            obj.AsDynamic().MPLSELECTARTGROUPDANGERCRITPL = TestString;
        }

        protected override void CheckSimpleChange(MPLSelect source, MPLSelect dest)
        {
            string sourceName = source.AsDynamic().MPLSELECTARTGROUPDANGERCRITPL;
            string destName = dest.AsDynamic().MPLSELECTARTGROUPDANGERCRITPL;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}