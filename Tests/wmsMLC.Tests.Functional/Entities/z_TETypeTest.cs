using System.ComponentModel;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Managers.Validation.Attributes;
using wmsMLC.Business.Objects;
//using wmsMLC.DCL.Content;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class TETypeTest : BaseWMSObjectTest<TEType>
    {
        public TETypeTest()
        {
            //ContentModule.InitValidateStrategies();
        }

        protected override void FillRequiredFields(TEType obj)
        {
            base.FillRequiredFields(obj);
            obj.AsDynamic().TETYPECODE = TestString;
            obj.AsDynamic().TETYPENAME = TestString;
            obj.AsDynamic().TETYPELENGTH = TestDecimal;
            obj.AsDynamic().TETYPEWIDTH = TestDecimal;
            obj.AsDynamic().TETYPEHEIGHT = TestDecimal;
            obj.AsDynamic().TETYPEMAXWEIGHT = TestDecimal;

            var t2m = new TEType2Mandant();
            t2m.AsDynamic().TETYPE2MANDANTTETYPECODE = TestString;
            t2m.AsDynamic().MANDANTID = 1;

            obj.AsDynamic().TETYPE2MANDANTL = new WMSBusinessCollection<TEType2Mandant> { t2m };
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(TETYPECODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(TEType obj)
        {
            obj.AsDynamic().TETYPENAME = "AutoTest";
        }

        protected override void CheckSimpleChange(TEType source, TEType dest)
        {
            string sourceName = source.AsDynamic().TETYPENAME;
            string destName = dest.AsDynamic().TETYPENAME;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        [Test, Ignore("Нет хистори")]
        public override void ManagerGetHistoryTest()
        {
        }

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
    }
}