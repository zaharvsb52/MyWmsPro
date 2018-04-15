using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class TETypeTest : BaseEntityTest<TEType>
    {
        public const string ExistsItem1Code = "TST_TETYPE_1";
        public const string ExistsItem2Code = "TST_TETYPE_2";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = TEType.TETypeNamePropertyName;
        }

        protected override void FillRequiredFields(TEType entity)
        {
            base.FillRequiredFields(entity);

            entity.TETypeName = TestString;
            entity.Length = TestDecimal;
            entity.Width = TestDecimal;
            entity.Height = TestDecimal;
            entity.MaxWeight = TestDecimal;

            // добавляем привязку к манданту (без нее не будет получаться тип ТЕ)
            var t2m = new TEType2Mandant();
            t2m.AsDynamic().TETYPE2MANDANTTETYPECODE = InsertItemStringId;
            t2m.AsDynamic().MANDANTID = TstMandantId;
            entity.TETYPE2MANDANTL = new WMSBusinessCollection<TEType2Mandant>();
            entity.TETYPE2MANDANTL.Add(t2m);
        }
/*
        [Test]
        public void ValidateStrategiesGpvMaxCountTest()
        {
            var teTypeObject = new TEType();
            var property = TypeDescriptor.GetProperties(teTypeObject).Find("GLOBALPARAMVAL", true);
            if (property == null)
                return;
            const string strategyName = "gpv.maxcount";

            WMSValidateAttribute.TestStrategy(strategyName, new ValidateStrategyContext(teTypeObject, property, null, null)).Should().BeFalse();

            var arrayGpv = new WMSBusinessCollection<TETypeGPV>();
            property.SetValue(teTypeObject, arrayGpv);
            WMSValidateAttribute.TestStrategy(strategyName, new ValidateStrategyContext(teTypeObject, property, null, null)).Should().BeFalse();

            var gpv = new TETypeGPV();
            // HACK - знаем что такой GPV есть для TEType
            gpv.AsDynamic().GLOBALPARAMCODE_R_TETYPEGPV = "TETypeSequence2TENumber";
            arrayGpv.Add(gpv);
            property.SetValue(teTypeObject, arrayGpv);
            WMSValidateAttribute.TestStrategy(strategyName, new ValidateStrategyContext(teTypeObject, property, null, null)).Should().BeFalse();

            arrayGpv.Add(gpv);
            property.SetValue(teTypeObject, arrayGpv);
            WMSValidateAttribute.TestStrategy(strategyName, new ValidateStrategyContext(teTypeObject, property, null, null)).Should().BeTrue();
        }

        [Test]
        public void ValidateStrategiesGpvRequirementTest()
        {
            var teTypeObject = new TEType();
            var property = TypeDescriptor.GetProperties(teTypeObject).Find("GLOBALPARAMVAL", true);
            if (property == null)
                return;
            const string strategyName = "gpv.requirement";

            WMSValidateAttribute.TestStrategy(strategyName, new ValidateStrategyContext(teTypeObject, property, null, null)).Should().BeTrue();

            var arrayGpv = new WMSBusinessCollection<TETypeGPV>();
            property.SetValue(teTypeObject, arrayGpv);
            WMSValidateAttribute.TestStrategy(strategyName, new ValidateStrategyContext(teTypeObject, property, null, null)).Should().BeTrue();

            var gpv = new TETypeGPV();
            // HACK - знаем что такой GPV есть для TEType
            gpv.AsDynamic().GLOBALPARAMCODE_R_TETYPEGPV = "TETypeSequence2TENumber";
            arrayGpv.Add(gpv);
            property.SetValue(teTypeObject, arrayGpv);
            WMSValidateAttribute.TestStrategy(strategyName, new ValidateStrategyContext(teTypeObject, property, null, null)).Should().BeFalse();
        }
*/
    }
}