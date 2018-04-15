using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class IWB2CargoTest : BaseWMSObjectTest<IWB2Cargo>
    {
        private readonly IWBTest _iwbTest = new IWBTest();
        private readonly CargoIWBTest _cargoIWBTest = new CargoIWBTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _iwbTest, _cargoIWBTest };
        }

        protected override void FillRequiredFields(IWB2Cargo obj)
        {
            base.FillRequiredFields(obj);

            var iwb = _iwbTest.CreateNew();
            var cargoIWB = _cargoIWBTest.CreateNew();
            
            obj.AsDynamic().IWB2CARGOID = TestDecimal;
            obj.AsDynamic().IWB2CARGOIWBID = iwb.GetKey();
            obj.AsDynamic().IWB2CARGOCARGOIWBID = cargoIWB.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(IWB2CARGOID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(IWB2Cargo obj)
        {
        }

        protected override void CheckSimpleChange(IWB2Cargo source, IWB2Cargo dest)
        {
        }

        [Test(Description = DeleteByParentDesc)]
        public void DeleteByParentTest()
        {
            DeleteByParent<IWB>(TestDecimal, TestDecimal);
        }

        //[Test,Ignore("Ошибка при получении IWBID_R по запросу  select * from TABLE(pkgIWB2Cargo.GetIWB2CargoLst(null, 'IWB2CARGOID is null AND IWBID_R is null AND CARGOIWBID_R is null AND USERINS is null AND DATEINS is null AND USERUPD is null AND DATEUPD is null AND TRANSACT is null'))")]
        //public override void FiltersTest()
        //{
        //}
    }
}