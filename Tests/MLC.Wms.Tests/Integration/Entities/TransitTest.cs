using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class TransitTest : BaseEntityTest<Transit>
    {
        public const int ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "TransitDesc";
        }

        protected override void FillRequiredFields(Transit entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.TransitName = TestString;
            obj.MandantID = TstMandantId;
            obj.Transit2Entity = SysObjectTest.ExistsItemEntityCode;
            obj.TransitV2GUI = TestBool;
        }
    }
}