using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class SequenceTest : BaseEntityTest<Sequence>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "SequenceDesc";
        }
        protected override void FillRequiredFields(Sequence entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.SequenceMinValue = TestDecimal;
            obj.SequenceMaxValue = TestDecimal * 2;
            obj.SequenceStartValue = TestDecimal;
            obj.SequenceIncrement = TestDecimal;
            obj.SequenceCache = TestDecimal;
            obj.SequenceCycle = TestBool;
            obj.SequenceName = TestString;
        }

        [Test, Ignore("Commit при Insert. Запускать нельзя")]
        public override void Entity_should_be_create_read_update_delete()
        {
        }
    }
}