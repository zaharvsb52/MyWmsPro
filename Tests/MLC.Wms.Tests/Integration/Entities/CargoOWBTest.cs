using System;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class CargoOWBTest : BaseEntityTest<CargoOWB>
    {
        public const decimal ExistsItem1Code = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = CargoOWB.CARGOOWBCONTAINERPropertyName;
            InsertItemTransact = 2;
            UpdateItemTransact = 4;
        }

        protected override void FillRequiredFields(CargoOWB entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.INTERNALTRAFFICID_R = InternalTrafficTest.ExistsItem1Code;
            obj.CARGOOWBNET = TestDecimal;
            obj.CARGOOWBBRUTTO = TestDecimal;
            obj.CARGOOWBVOLUME = TestDecimal;
            obj.CARGOOWBCOUNT = TestDecimal;
            obj.CARGOOWBLOADBEGIN = DateTime.Now;
            obj.CARGOOWBLOADEND = DateTime.Now;
            obj.CARGOOWBSTAMP = TestString;
        }
    }
}