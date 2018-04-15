using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class OWB2CargoTest : BaseWMSObjectTest<OWB2Cargo>
    {
        private readonly OWBTest _owbTest = new OWBTest();
        private readonly CargoOWBTest _cargoOwbTest = new CargoOWBTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _owbTest, _cargoOwbTest };
        }

        protected override void FillRequiredFields(OWB2Cargo obj)
        {
            base.FillRequiredFields(obj);


            var owb = _owbTest.CreateNew();
            var cargoOwb = _cargoOwbTest.CreateNew();

            obj.AsDynamic().OWB2CARGOID = TestDecimal;
            obj.AsDynamic().OWB2CARGOOWBID = owb.GetKey();
            obj.AsDynamic().OWB2CARGOCARGOOWBID = cargoOwb.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(OWB2CARGOID = '{0}')", TestDecimal);
        }
    }
}