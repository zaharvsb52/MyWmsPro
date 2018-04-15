using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class PlaceTest : BaseWMSObjectTest<Place>
    {
        private readonly SegmentTest _segmentTest = new SegmentTest();
        private readonly PlaceTypeTest _placeTypeTest = new PlaceTypeTest();
        private readonly PlaceClassTest _placeClassTest = new PlaceClassTest();
        private readonly ReceiveAreaTest _receiveAreaTest = new ReceiveAreaTest();
        private readonly MotionAreaTest _motionAreaTest = new MotionAreaTest();
        private readonly SupplyAreaTest _supplyAreaTest = new SupplyAreaTest();

        //[Test]
        //public void TestCheckNumber()
        //{
        //    var manager = CreateManager();

        //    var places = manager.GetFiltered("rownum <= 1000", GetModeEnum.Partial);

        //    var placeCheckDic = new Dictionary<string, int>();
        //    var placeCheckYDic = new Dictionary<string, int>();

        //    var text = new StringBuilder();
        //    text.AppendLine("PLACECODE,SEGMENTCODE_R,PLACES,PLACEX,PLACEY,PLACEZ,PlaceCheck,PlaceCheckY");

        //    //foreach (var p in places.OrderBy(p => p.SegmentCode).ThenBy(p => p.PlaceS).ThenBy(p => p.PlaceX))
        //    foreach (var p in places)
        //    {
        //        PlaceManager.FillCheckNumbers(p);

        //        var placeCheck = p.PlaceCheck;
        //        if (placeCheckDic.ContainsKey(placeCheck))
        //        {
        //            placeCheckDic[placeCheck]++;
        //        }
        //        else
        //        {
        //            placeCheckDic[placeCheck] = 0;
        //        }

        //        var placeCheckY = p.PlaceCheckY;

        //        if (placeCheckYDic.ContainsKey(placeCheckY))
        //        {
        //            placeCheckYDic[placeCheckY]++;
        //        }
        //        else
        //        {
        //            placeCheckYDic[placeCheckY] = 0;
        //        }

        //        text.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7}", p.GetKey(), p.SegmentCode, p.PlaceS, p.PlaceX, p.PlaceY, p.PlaceZ, p.PlaceCheck,
        //            p.PlaceCheckY));
        //    }

        //    using (var fs = File.Open(@"d:\temp\!\plase2.csv", FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
        //    {
        //        using (var sw = new StreamWriter(fs, Encoding.UTF8))
        //        {
        //            sw.Write(text.ToString());
        //        }
        //    }
        //}

        //[Test]
        //public void TestCollectionTruns() //Тестируем тран. в групповых опер.
        //{
        //    using (var mng = CreateManager())
        //    {
        //        var places = mng.GetFiltered("ROWNUM <= 5", GetModeEnum.Full).ToArray();

        //        var len = places.Length;
        //        for (var i = 0; i < len; i++)
        //        {
        //            var p = places[i];
        //            p.SetProperty("PLACEHOSTREF", i == len - 1 ? new string('+', 20000) : "+");
        //        }

        //        mng.Update(places);
        //    }
        //}

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[]
                {
                    _placeTypeTest, _placeClassTest,_segmentTest, _receiveAreaTest, _motionAreaTest,_supplyAreaTest
                };
        }

        protected override void FillRequiredFields(Place obj)
        {
            base.FillRequiredFields(obj);

            var segment = _segmentTest.CreateNew();
            var placeType = _placeTypeTest.CreateNew();
            var placeClass = _placeClassTest.CreateNew();
            var receiveArea = _receiveAreaTest.CreateNew();
            var motionArea = _motionAreaTest.CreateNew();
            var supplyArea = _supplyAreaTest.CreateNew();

            obj.AsDynamic().PLACECODE = TestString;
            obj.AsDynamic().PLACEX = TestDecimal;
            obj.AsDynamic().PLACEY = TestDecimal;
            obj.AsDynamic().PLACEZ = TestDecimal;
            obj.AsDynamic().PLACEWIDTH = TestDecimal;
            obj.AsDynamic().PLACELENGTH = TestDecimal;
            obj.AsDynamic().PLACEHEIGHT = TestDecimal;
            obj.AsDynamic().PLACECAPACITYMAX = TestDecimal;
            obj.AsDynamic().PLACECAPACITY = TestDecimal;
            obj.AsDynamic().PLACECHECKNUMBER = TestString;
            obj.AsDynamic().PLACECHECKNUMBERY = TestString;
            obj.AsDynamic().PLACENAME = TestString;
            obj.AsDynamic().PLACESORTA = TestDecimal;
            obj.AsDynamic().PLACESORTB = TestDecimal;
            obj.AsDynamic().PLACESORTC = TestDecimal;
            obj.AsDynamic().PLACESORTD = TestDecimal;
            obj.AsDynamic().PLACESORTPICK = TestDecimal;
            obj.AsDynamic().PLACEWEIGHT = TestDecimal;
            obj.AsDynamic().PLACEWEIGHTGROUP = TestDecimal;
            obj.AsDynamic().PLACEHOSTREF = TestString;
            obj.AsDynamic().PLACEGROUPCODE = TestString;
            obj.AsDynamic().PLACES = TestDecimal;
            
            obj.AsDynamic().SEGMENTCODE_R = segment.GetKey();
            obj.AsDynamic().PLACETYPECODE_R = placeType.GetKey();
            obj.AsDynamic().PLACECLASSCODE_R = placeClass.GetKey();
            obj.AsDynamic().RECEIVEAREACODE_R = receiveArea.GetKey();
            obj.AsDynamic().MOTIONAREACODE_R = motionArea.GetKey();
            obj.AsDynamic().SUPPLYAREACODE_R = supplyArea.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(PLACECODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(Place obj)
        {
            obj.AsDynamic().PLACEDESC = TestString;
        }

        protected override void CheckSimpleChange(Place source, Place dest)
        {
            string sourceName = source.AsDynamic().PLACEDESC;
            string destName = dest.AsDynamic().PLACEDESC;
            destName.ShouldBeEquivalentTo(sourceName);
        }
    }
}
