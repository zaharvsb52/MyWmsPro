using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class CargoIWBPosTest : BaseWMSObjectTest<CargoIWBPos>
    {
        private readonly CargoIWBTest _cargoIWBTest = new CargoIWBTest();
        private readonly TETypeTest _teTypeTest = new TETypeTest();
        private readonly QlfTest _QlfTest = new QlfTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _cargoIWBTest, _teTypeTest, _QlfTest };
        }

        protected override void FillRequiredFields(CargoIWBPos obj)
        {
            base.FillRequiredFields(obj);

            var cargoIWB = _cargoIWBTest.CreateNew();
            var teType = _teTypeTest.CreateNew();
            var QLF = _QlfTest.CreateNew();

            obj.AsDynamic().CARGOIWBPOSID = TestDecimal;
            obj.AsDynamic().CARGOIWBID_R = cargoIWB.GetKey();
            obj.AsDynamic().CARGOIWBPOSCOUNT = TestDecimal;
            obj.AsDynamic().TETYPECODE_R = teType.GetKey();
            obj.AsDynamic().CARGOIWBPOSTYPE = TestString;
            obj.AsDynamic().QLFCODE_R = QLF.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(CARGOIWBPOSID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(CargoIWBPos obj)
        {
            obj.AsDynamic().CARGOIWBPOSDESC = TestString;
        }

        protected override void CheckSimpleChange(CargoIWBPos source, CargoIWBPos dest)
        {
            string sourceName = source.AsDynamic().CARGOIWBPOSDESC;
            string destName = dest.AsDynamic().CARGOIWBPOSDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}