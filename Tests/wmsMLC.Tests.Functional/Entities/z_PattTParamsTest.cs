using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class PattTParamsTest : BaseWMSObjectTest<PattTParams>
    {
        private readonly PattTDataSourceTest _pattTDataSourceTest = new PattTDataSourceTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _pattTDataSourceTest };
        }

        protected override void FillRequiredFields(PattTParams obj)
        {
            base.FillRequiredFields(obj);

            var ds = _pattTDataSourceTest.CreateNew();

            obj.AsDynamic().TEMPLATEPARAMSID = TestDecimal;
            obj.AsDynamic().TEMPLATEDATASOURCECODE_R = ds.GetKey();
            obj.AsDynamic().TEMPLATEPARAMSCODE = TestString;
            obj.AsDynamic().TEMPLATEPARAMSNAME = TestString;
            obj.AsDynamic().TEMPLATEPARAMSDATATYPE = TestDecimal;
            obj.AsDynamic().TEMPLATEPARAMSVALUE = TestDecimal;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(TEMPLATEPARAMSID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(PattTParams obj)
        {
            obj.AsDynamic().TEMPLATEPARAMSDESC = TestString;
        }

        protected override void CheckSimpleChange(PattTParams source, PattTParams dest)
        {
            string sourceName = source.AsDynamic().TEMPLATEPARAMSDESC;
            string destName = dest.AsDynamic().TEMPLATEPARAMSDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}