using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class IWBTest : BaseWMSObjectTest<IWB>
    {
        //[Test]
        //public void TestGetTeProductQuantityFromCargoIwb()
        //{
        //    using (var manager = IoC.Instance.Resolve<IBPProcessManager>())
        //    {
        //        var iwbPosInput = new IWBPosInput
        //        {
        //            SKUID = 75739,
        //            TETypeCode = "EURO",
        //            SKU2TTEQuantity = 1000,
        //            QLFCODE_R = "QLFNORMAL",
        //            IWBPosExpiryDate = new DateTime(2100, 1, 1),
        //            ProductCountSKU = 0,
        //            ProductCount = 60,
        //            ArtCode = "ETN286563",
        //            ArtInputDateMethod = "NONE",
        //            MeasureCode = "кор",
        //            SKU2TTEQuantityMax = 1000
        //        };

        //        decimal val;
        //        var res = manager.GetTeProductQuantityFromCargoIwb("OP_CARGO_INPUT_TERM", 4265, iwbPosInput, true, out val);
        //    }
        //}

        //[Test]
        //public void AddIwb()
        //{
        //    IWB iwb;
        //    using (var mgr = CreateManager())
        //    {
        //        iwb = mgr.New();
        //        iwb.AsDynamic().IWBNAME = "Test for 6000 pos";
        //        iwb.AsDynamic().IWBPRIORITY = TestDecimal;
        //        iwb.AsDynamic().MANDANTID = 11;
        //        mgr.Insert(ref iwb);
        //    }

        //    using (var mgr = new IWBPosTest().CreateManager())
        //    {
        //        for (int i = 1; i <= 6000; i++)
        //        {
        //            var iwbpos = new IWBPos();
        //            iwbpos.AsDynamic().IWBID_R = iwb.GetKey();
        //            iwbpos.AsDynamic().IWBPOSNUMBER = i;
        //            iwbpos.AsDynamic().SKUID_R = 73280;
        //            iwbpos.AsDynamic().IWBPOSCOUNT = 100;
        //            iwbpos.IWBPosBatch = "31.11.2014";
        //            mgr.Insert(ref iwbpos);
        //        }
        //    }
        //}

        protected override void FillRequiredFields(IWB obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().IWBID = TestDecimal;
            obj.AsDynamic().IWBNAME = TestString;
            obj.AsDynamic().IWBPRIORITY = TestDecimal;
            obj.AsDynamic().MANDANTID = 1; 
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(IWBID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(IWB obj)
        {
            obj.AsDynamic().IWBHOSTREF = TestString;
        }

        protected override void CheckSimpleChange(IWB source, IWB dest)
        {
            string sourceName = source.AsDynamic().IWBHOSTREF;
            string destName = dest.AsDynamic().IWBHOSTREF;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}