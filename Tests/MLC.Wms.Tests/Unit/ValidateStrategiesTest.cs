using System;
using System.ComponentModel;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business;
using wmsMLC.Business.Managers.Validation.Attributes;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace MLC.Wms.Tests.Unit
{
    [TestFixture]
    public class ValidateStrategiesTest
    {
        public class TestClass : ValidatableObject
        {
            public int? Int { get; set; }
            public decimal Decimal { get; set; }
            public Single Single { get; set; }
            public float Float { get; set; }
            public double Double { get; set; }
            public DateTime DateTime { get; set; }
        }

        public class TestClassIsNew : EditableBusinessObject
        {
            public string Test { get; set; }
        }

        private TestClassIsNew _requirementObj;
        private PropertyDescriptorCollection _requirementProps;

        [TestFixtureSetUp]
        public void Setup()
        {
            ValidateStrategiesHelper.Initialize();

            _requirementObj = new TestClassIsNew();
            _requirementProps = TypeDescriptor.GetProperties(_requirementObj);
        }

        #region .  Test range strategies  .

        [Test]
        public void RangesTest()
        {
            var testObj = new TestClass { Int = 9, Single = (Single)9.2, Decimal = (decimal)9.2, Float = (float)9.2, Double = (double)9.2, DateTime = new DateTime(2013, 07, 03, 18, 46, 55) };
            var props = TypeDescriptor.GetProperties(testObj);

            WMSValidateAttribute.TestStrategy("range.min", new ValidateStrategyContext(testObj, props["Int"], null, (testObj.Int - 1).ToString())).Should().BeFalse();
            WMSValidateAttribute.TestStrategy("range.min", new ValidateStrategyContext(testObj, props["Int"], null, (testObj.Int + 0).ToString())).Should().BeFalse();
            WMSValidateAttribute.TestStrategy("range.min", new ValidateStrategyContext(testObj, props["Int"], null, (testObj.Int + 1).ToString())).Should().BeTrue();
            WMSValidateAttribute.TestStrategy("range.max", new ValidateStrategyContext(testObj, props["Int"], null, (testObj.Int - 1).ToString())).Should().BeTrue();
            WMSValidateAttribute.TestStrategy("range.max", new ValidateStrategyContext(testObj, props["Int"], null, (testObj.Int + 0).ToString())).Should().BeFalse();
            WMSValidateAttribute.TestStrategy("range.max", new ValidateStrategyContext(testObj, props["Int"], null, (testObj.Int + 1).ToString())).Should().BeFalse();

            WMSValidateAttribute.TestStrategy("range.min", new ValidateStrategyContext(testObj, props["Decimal"], null, (testObj.Decimal - 1).ToString())).Should().BeFalse();
            WMSValidateAttribute.TestStrategy("range.min", new ValidateStrategyContext(testObj, props["Decimal"], null, (testObj.Decimal + 0).ToString())).Should().BeFalse();
            WMSValidateAttribute.TestStrategy("range.min", new ValidateStrategyContext(testObj, props["Decimal"], null, (testObj.Decimal + 1).ToString())).Should().BeTrue();
            WMSValidateAttribute.TestStrategy("range.max", new ValidateStrategyContext(testObj, props["Decimal"], null, (testObj.Decimal - 1).ToString())).Should().BeTrue();
            WMSValidateAttribute.TestStrategy("range.max", new ValidateStrategyContext(testObj, props["Decimal"], null, (testObj.Decimal + 0).ToString())).Should().BeFalse();
            WMSValidateAttribute.TestStrategy("range.max", new ValidateStrategyContext(testObj, props["Decimal"], null, (testObj.Decimal + 1).ToString())).Should().BeFalse();

            WMSValidateAttribute.TestStrategy("range.min", new ValidateStrategyContext(testObj, props["Single"], null, (testObj.Single - 1).ToString())).Should().BeFalse();
            WMSValidateAttribute.TestStrategy("range.min", new ValidateStrategyContext(testObj, props["Single"], null, (testObj.Single + 0).ToString())).Should().BeFalse();
            WMSValidateAttribute.TestStrategy("range.min", new ValidateStrategyContext(testObj, props["Single"], null, (testObj.Single + 1).ToString())).Should().BeTrue();
            WMSValidateAttribute.TestStrategy("range.max", new ValidateStrategyContext(testObj, props["Single"], null, (testObj.Single - 1).ToString())).Should().BeTrue();
            WMSValidateAttribute.TestStrategy("range.max", new ValidateStrategyContext(testObj, props["Single"], null, (testObj.Single + 0).ToString())).Should().BeFalse();
            WMSValidateAttribute.TestStrategy("range.max", new ValidateStrategyContext(testObj, props["Single"], null, (testObj.Single + 1).ToString())).Should().BeFalse();

            WMSValidateAttribute.TestStrategy("range.min", new ValidateStrategyContext(testObj, props["Float"], null, (testObj.Float - 1).ToString())).Should().BeFalse();
            WMSValidateAttribute.TestStrategy("range.min", new ValidateStrategyContext(testObj, props["Float"], null, (testObj.Float + 0).ToString())).Should().BeFalse();
            WMSValidateAttribute.TestStrategy("range.min", new ValidateStrategyContext(testObj, props["Float"], null, (testObj.Float + 1).ToString())).Should().BeTrue();
            WMSValidateAttribute.TestStrategy("range.max", new ValidateStrategyContext(testObj, props["Float"], null, (testObj.Float - 1).ToString())).Should().BeTrue();
            WMSValidateAttribute.TestStrategy("range.max", new ValidateStrategyContext(testObj, props["Float"], null, (testObj.Float + 0).ToString())).Should().BeFalse();
            WMSValidateAttribute.TestStrategy("range.max", new ValidateStrategyContext(testObj, props["Float"], null, (testObj.Float + 1).ToString())).Should().BeFalse();

            WMSValidateAttribute.TestStrategy("range.min", new ValidateStrategyContext(testObj, props["Double"], null, (testObj.Double - 1).ToString())).Should().BeFalse();
            WMSValidateAttribute.TestStrategy("range.min", new ValidateStrategyContext(testObj, props["Double"], null, (testObj.Double + 0).ToString())).Should().BeFalse();
            WMSValidateAttribute.TestStrategy("range.min", new ValidateStrategyContext(testObj, props["Double"], null, (testObj.Double + 1).ToString())).Should().BeTrue();
            WMSValidateAttribute.TestStrategy("range.max", new ValidateStrategyContext(testObj, props["Double"], null, (testObj.Double - 1).ToString())).Should().BeTrue();
            WMSValidateAttribute.TestStrategy("range.max", new ValidateStrategyContext(testObj, props["Double"], null, (testObj.Double + 0).ToString())).Should().BeFalse();
            WMSValidateAttribute.TestStrategy("range.max", new ValidateStrategyContext(testObj, props["Double"], null, (testObj.Double + 1).ToString())).Should().BeFalse();

            WMSValidateAttribute.TestStrategy("range.min", new ValidateStrategyContext(testObj, props["DateTime"], null, testObj.DateTime.AddDays(-1).ToString("yyyyMMdd HH:mm:ss"))).Should().BeFalse();
            WMSValidateAttribute.TestStrategy("range.min", new ValidateStrategyContext(testObj, props["DateTime"], null, testObj.DateTime.AddDays(0).ToString("yyyyMMdd HH:mm:ss"))).Should().BeFalse();
            WMSValidateAttribute.TestStrategy("range.min", new ValidateStrategyContext(testObj, props["DateTime"], null, testObj.DateTime.AddDays(+1).ToString("yyyyMMdd HH:mm:ss"))).Should().BeTrue();
            WMSValidateAttribute.TestStrategy("range.max", new ValidateStrategyContext(testObj, props["DateTime"], null, testObj.DateTime.AddDays(-1).ToString("yyyyMMdd HH:mm:ss"))).Should().BeTrue();
            WMSValidateAttribute.TestStrategy("range.max", new ValidateStrategyContext(testObj, props["DateTime"], null, testObj.DateTime.AddDays(0).ToString("yyyyMMdd HH:mm:ss"))).Should().BeFalse();
            WMSValidateAttribute.TestStrategy("range.max", new ValidateStrategyContext(testObj, props["DateTime"], null, testObj.DateTime.AddDays(+1).ToString("yyyyMMdd HH:mm:ss"))).Should().BeFalse();
        }

        #endregion

        #region .  Test requirement strategy  .

        [Test]
        [ExpectedException(typeof(NullReferenceException))]
        public void RequirementTestWhenObjectEqualsNull()
        {
            WMSValidateAttribute.TestStrategy("requirement", new ValidateStrategyContext(null, null, null, null));
        }

        [Test]
        [ExpectedException(typeof(DeveloperException))]
        public void RequirementTestWhenParametersEqualsNull()
        {
            WMSValidateAttribute.TestStrategy("requirement", new ValidateStrategyContext(_requirementObj, _requirementProps["Test"], null, null));
        }

        [Test]
        [ExpectedException(typeof(DeveloperException))]
        public void RequirementTestWhenParametersNotEqualsCreateEdit()
        {
            WMSValidateAttribute.TestStrategy("requirement", new ValidateStrategyContext(_requirementObj, _requirementProps["Test"], "test", null));
        }

        [Test]
        public void RequirementTestWhenParametersEqualsCreateEdit()
        {
            WMSValidateAttribute.TestStrategy("requirement", new ValidateStrategyContext(_requirementObj, _requirementProps["Test"], "create;edit", null)).Should().BeTrue();
        }

        [Test]
        public void RequirementTestWhenParametersEqualsEdit()
        {
            _requirementObj.AcceptChanges();
            WMSValidateAttribute.TestStrategy("requirement", new ValidateStrategyContext(_requirementObj, _requirementProps["Test"], "edit", null)).Should().BeTrue();
            _requirementObj.AcceptChanges(true);
            WMSValidateAttribute.TestStrategy("requirement", new ValidateStrategyContext(_requirementObj, _requirementProps["Test"], "edit", null)).Should().BeFalse();
        }

        [Test]
        public void RequirementTestWhenParametersEqualsCreate()
        {
            _requirementObj.AcceptChanges();
            WMSValidateAttribute.TestStrategy("requirement", new ValidateStrategyContext(_requirementObj, _requirementProps["Test"], "create", null)).Should().BeFalse();
            _requirementObj.AcceptChanges(true);
            WMSValidateAttribute.TestStrategy("requirement", new ValidateStrategyContext(_requirementObj, _requirementProps["Test"], "create", null)).Should().BeTrue();
        }

        [Test]
        public void RequirementTestWhenValueEqualsNotNull()
        {
            _requirementObj.Test = "Test";
            WMSValidateAttribute.TestStrategy("requirement", new ValidateStrategyContext(_requirementObj, _requirementProps["Test"], null, null)).Should().BeFalse();
        }

        [Test]
        [ExpectedException(typeof(DeveloperException))]
        public void RequirementTestWhenObjectHaveNotIsNew()
        {
            var testObj = new TestClass() { Int = null};
            WMSValidateAttribute.TestStrategy("requirement", new ValidateStrategyContext(testObj, TypeDescriptor.GetProperties(testObj)["Int"], "create", null));
        }

        #endregion

        #region .  Test gpv strategies  .

        // Тесты для GPV стратегий включены в wmsMLC.Tests.Functional.TETypeTest

        #endregion
    }
}