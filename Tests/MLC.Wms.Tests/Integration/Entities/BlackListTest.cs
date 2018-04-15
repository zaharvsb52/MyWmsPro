using System;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BlackListTest : BaseEntityTest<BlackList>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "BLACKLISTDATE";
        }

        protected override void FillRequiredFields(BlackList entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.WORKERID_R = WorkerTest.ExistsItem1Id;
            obj.BLACKLISTDATE = DateTime.Now;
        }
    }
}