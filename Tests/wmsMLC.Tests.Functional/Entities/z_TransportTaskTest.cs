using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class TransportTaskTest : BaseWMSObjectTest<TransportTask>
    {
        private readonly TETest _teTest = new TETest();
        private readonly TransportTaskTypeTest _transportTaskTypeTest = new TransportTaskTypeTest();
        private readonly TruckTest _truckTest = new TruckTest();
        private readonly ClientTest _clientTest = new ClientTest();
        private readonly PlaceTest _placeTest = new PlaceTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _teTest,_placeTest, _transportTaskTypeTest, _truckTest, _clientTest };
        } 

        protected override void FillRequiredFields(TransportTask obj)
        {
            base.FillRequiredFields(obj);

            var te = _teTest.CreateNew();
            var ttType = _transportTaskTypeTest.CreateNew();
            var truck = _truckTest.CreateNew();
            var client = _clientTest.CreateNew();
            // т.к. +1 в TETest
            _placeTest.TestString = TestString + "2";
            _placeTest.TestDecimal = TestDecimal + 2;
            var place = _placeTest.CreateNew();

            obj.AsDynamic().TTASKID = TestDecimal;
            obj.AsDynamic().TECODE_R = te.GetKey();
            obj.AsDynamic().TTASKTYPECODE_R = ttType.GetKey();
            obj.AsDynamic().TTASKSTARTPLACE = place.GetKey();
            obj.AsDynamic().TTASKCURRENTPLACE = place.GetKey();
            obj.AsDynamic().TTASKNEXTPLACE = place.GetKey();
            obj.AsDynamic().TTASKFINISHPLACE = place.GetKey();
            //obj.AsDynamic().STATUSCODE_R = place.GetKey();
            obj.AsDynamic().TRUCKCODE_R = truck.GetKey();
            obj.AsDynamic().CLIENTCODE_R = client.GetKey();
            obj.AsDynamic().TTASKTARGETTE = te.GetKey();
            obj.AsDynamic().TRANSPORTTASKSTRATEGY = MovingUseStrategySysEnum.PLACE_FIX.ToString();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(TTASKID = '{0}')", TestDecimal);
        }


        [Test, Ignore("Надо Наде сказать, ошибка в бд в процедуре получения select * from TABLE(pkgTransportTask.GetTransportTaskHLst(null,null))")]
        public override void ManagerGetHistoryTest()
        {
            
        }
    }
}