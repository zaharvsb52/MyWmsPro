using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class CargoIWBTest : BaseWMSObjectTest<CargoIWB>
    {
        protected override void FillRequiredFields(CargoIWB obj)
        {
            base.FillRequiredFields(obj);
            obj.AsDynamic().CARGOIWBID = TestDecimal;
            obj.AsDynamic().CARGOIWBNET = TestDecimal;
            // Создается адрес и потом не удаляется
            //obj.AsDynamic().CARGOIWBLOADADDRESS.ADDRESSBOOKTYPECODE = "ADR_LEGAL";
        }

        //[Test]
        //public void TestCargoPos()
        //{
        //    CargoIWB cargoIwb;
        //    using (var manager = CreateManager())
        //    {
        //        cargoIwb = manager.Get(new Decimal(6469), GetModeEnum.Full);
        //    }
        //    if (cargoIwb == null)
        //        return;

        //    var cargoIwbPosDocs = cargoIwb.Get<WMSBusinessCollection<CargoIWBPos>>("CARGOIWBPOSLCLIENT") ??
        //        new WMSBusinessCollection<CargoIWBPos>();
        //    var cargoIwbPosFacts = cargoIwb.Get<WMSBusinessCollection<CargoIWBPos>>("CARGOIWBPOSLFACT") ??
        //        new WMSBusinessCollection<CargoIWBPos>();

        //    foreach (var posdoc in cargoIwbPosDocs)
        //    {
        //        if (
        //            cargoIwbPosFacts.Any(f =>
        //                posdoc.Get<string>("TETYPECODE_R").EqIgnoreCase(f.Get<string>("TETYPECODE_R")) &&
        //                posdoc.Get<decimal>("IWBID_R") == f.Get<decimal>("IWBID_R") &&
        //                posdoc.Get<string>("CARGOIWBPOSBOXNUMBER").EqIgnoreCase(f.Get<string>("CARGOIWBPOSBOXNUMBER"))))
        //        {
        //            continue;
        //        }

        //        var t = 0;
        //    }
        //}

        protected override string GetCheckFilter()
        {
            return string.Format("(CARGOIWBID = {0})", TestDecimal);
        }

        protected override void MakeSimpleChange(CargoIWB obj)
        {
            obj.AsDynamic().CARGOIWBNET = TestDecimal;
        }

        protected override void CheckSimpleChange(CargoIWB source, CargoIWB dest)
        {
            decimal sourceName = source.AsDynamic().CARGOIWBNET;
            decimal destName = dest.AsDynamic().CARGOIWBNET;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}