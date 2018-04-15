using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class CargoOWBTest : BaseWMSObjectTest<CargoOWB>
    {
        private readonly InternalTrafficTest _internalTrafficTest = new InternalTrafficTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _internalTrafficTest };
        }

        protected override void FillRequiredFields(CargoOWB obj)
        {
            base.FillRequiredFields(obj);


            var it = _internalTrafficTest.CreateNew();

            obj.AsDynamic().CARGOOWBID = TestDecimal;
            obj.AsDynamic().INTERNALTRAFFICID_R = it.GetKey();
            obj.AsDynamic().CARGOOWBNET = TestDecimal;
            obj.AsDynamic().CARGOOWBBRUTTO = TestDecimal;
            obj.AsDynamic().CARGOOWBVOLUME = TestDecimal;
            obj.AsDynamic().CARGOOWBCOUNT = TestDecimal;
            obj.AsDynamic().CARGOOWBLOADBEGIN = DateTime.Now;
            obj.AsDynamic().CARGOOWBLOADEND = DateTime.Now;
            obj.AsDynamic().CARGOOWBSTAMP = TestString;

            // Созданный адрес не удаляется
            //using (var mgr = IoC.Instance.Resolve<IBaseManager<SysEnum>>())
            //{
            //    var adr = mgr.GetFiltered("(ENUMGROUP = 'ADDRESSTYPE')").ToArray();
            //    var address = obj.AsDynamic().CARGOOWBUNLOADADDRESS as AddressBook;
            //    address.AsDynamic().ADDRESSBOOKINDEX = TestDecimal;
            //    address.AsDynamic().ADDRESSBOOKTYPECODE = adr[0].AsDynamic().ENUMVALUE;
            //}

        }

        protected override string GetCheckFilter()
        {
            return string.Format("(CARGOOWBID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(CargoOWB obj)
        {
            obj.AsDynamic().CARGOOWBCONTAINER = TestString;
        }

        protected override void CheckSimpleChange(CargoOWB source, CargoOWB dest)
        {
            string sourceName = source.AsDynamic().CARGOOWBCONTAINER;
            string destName = dest.AsDynamic().CARGOOWBCONTAINER;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}