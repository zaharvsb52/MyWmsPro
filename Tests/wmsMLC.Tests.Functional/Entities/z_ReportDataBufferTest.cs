using System;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class ReportDataBufferTest : BaseWMSObjectTest<ReportDataBuffer>
    {
         protected override void FillRequiredFields(ReportDataBuffer obj)
        {
            base.FillRequiredFields(obj);

            obj.SetProperty(obj.GetPrimaryKeyPropertyName(), TestDecimal);
            obj.AsDynamic().REPORTDATABUFFERGROUP = Guid.NewGuid().ToString();
            obj.AsDynamic().REPORTDATABUFFERRECORD = Guid.NewGuid().ToString();
            obj.AsDynamic().REPORTDATABUFFERKEY = "TESTKEY";
            obj.AsDynamic().REPORTDATABUFFERVALUE =
                 "Teeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeesssssssssssssssssssssssssssssssssssstttttttttttttttttttt";
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(REPORTDATABUFFERID = {0})", TestDecimal);
        }

        protected override void MakeSimpleChange(ReportDataBuffer obj)
        {
            obj.AsDynamic().REPORTDATABUFFERRECORD = Guid.NewGuid().ToString();
        }

        protected override void CheckSimpleChange(ReportDataBuffer source, ReportDataBuffer dest)
        {
            string sourceName = source.AsDynamic().REPORTDATABUFFERKEY;
            string destName = dest.AsDynamic().REPORTDATABUFFERKEY;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}
