using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class MPLSelectTest : BaseEntityTest<MPLSelect>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "MPLSelectReservTE";
        }

        protected override void FillRequiredFields(MPLSelect entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MPLCode_r = MPLTest.ExistsItem1Code;
            obj.Priority = TestDecimal;
            obj.MPLSelectTECompleteOWBGroup = TestBool;
            obj.MandantID = TstMandantId;
        }
    }
}