using System;
using System.ComponentModel;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;

namespace wmsMLC.Tests.Functional
{
    [TestFixture]
    public class FilterHelperTests : BaseWMSTest
    {
        [Test]
        public void WhenParametersNotSet_ResieveFullAttrEntity()
        {
            var sb = new StringBuilder();
            sb.Append("<TENTCARTYPE>");
            var props = TypeDescriptor.GetProperties(typeof(CarType));
            foreach (DynamicPropertyDescriptor prop in props)
                sb.Append(string.Format("<{0} />", prop.SourceName.ToUpper()));
            sb.Append("</TENTCARTYPE>");

            var attrEntity = FilterHelper.GetAttrEntity<CarType>();

            attrEntity.Should().Be(sb.ToString());
        }

        [Test]
        public void WhenSetOneProperty_ResieveOneProperty()
        {
            var sb = new StringBuilder();
            sb.Append("<TENTCARTYPE>");
            sb.Append(string.Format("<{0} />", SourceNameHelper.Instance.GetPropertySourceName(typeof(CarType), CarType.CarTypeIDPropertyName).ToUpper()));
            sb.Append("</TENTCARTYPE>");
            var expected = sb.ToString().ToUpper();

            var attrEntity = FilterHelper.GetAttrEntity<CarType>(CarType.CarTypeIDPropertyName);
            attrEntity.Should().Be(expected);

            // поиск и выдача регистронезависимые
            attrEntity = FilterHelper.GetAttrEntity<CarType>(CarType.CarTypeIDPropertyName.ToLower());
            attrEntity.Should().Be(expected);
        }


        [Test]
        public void WhenCreateAttrEntityWithCollection_CanFindInnerObjectDescription()
        {
            var res = FilterHelper.GetAttrEntity<IWB>();
            res.Should().Contain("TENTIWBPOS");
            res.Should().Contain("TENTIWB2CARGO");
        }
    }
}