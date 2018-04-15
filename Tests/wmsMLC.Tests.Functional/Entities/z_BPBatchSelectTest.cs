using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BPBatchSelectTest : BaseWMSObjectTest<BPBatchSelect>
    {
        private readonly BPBatchTest _bpBatchTest= new BPBatchTest();
        private readonly PartnerTest _partnerTest = new PartnerTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _bpBatchTest, _partnerTest };
        }

        protected override void FillRequiredFields(BPBatchSelect obj)
        {
            base.FillRequiredFields(obj);

            var b = _bpBatchTest.CreateNew();
            var p = _partnerTest.CreateNew();

            obj.AsDynamic().BATCHSELECTID = TestDecimal;
            obj.AsDynamic().BATCHCODE_R = b.GetKey();
            obj.AsDynamic().PRIORITY = TestDouble;
            obj.AsDynamic().PARTNERID_R = p.GetKey();
            obj.AsDynamic().BATCHSELECTSKUCRITBATCH = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(BATCHSELECTID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(BPBatchSelect obj)
        {
            obj.AsDynamic().BATCHSELECTARTGROUPCRITBATCH = TestString;
        }

        protected override void CheckSimpleChange(BPBatchSelect source, BPBatchSelect dest)
        {
            string sourceName = source.AsDynamic().BATCHSELECTARTGROUPCRITBATCH;
            string destName = dest.AsDynamic().BATCHSELECTARTGROUPCRITBATCH;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}