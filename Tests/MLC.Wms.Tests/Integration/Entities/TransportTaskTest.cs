using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class TransportTaskTest : BaseEntityTest<TransportTask>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "TTASKEND";
        }

        protected override void FillRequiredFields(TransportTask entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.TECode_r = TETest.ExistsItem1Code;
            obj.TTaskTypeCode_r = TransportTaskTypeTest.ExistsItem1Code;
            obj.TTaskLoad = TestBool;
            obj.TTaskStartPlace = PlaceTest.ExistsItem1Code;
            obj.TTaskCurrentPlace = PlaceTest.ExistsItem1Code;
            obj.TTaskFinishPlace = PlaceTest.ExistsItem1Code;
            obj.TTaskPriority = TestDecimal;
            obj.TransportTaskStrategy = "PLACE_FREE";
            obj.MANDANTID = TstMandantId;
        }
    }
}