using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class OWBTest : BaseWMSObjectTest<OWB>
    {
       // private readonly MandantTest _mandantTest = new MandantTest();
        
        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { /*_mandantTest,*/ };
        }

        protected override void FillRequiredFields(OWB obj)
        {
            base.FillRequiredFields(obj);

            //var mandant = _mandantTest.CreateNew();
            
            obj.AsDynamic().OWBID = TestDecimal;
            obj.AsDynamic().MANDANTID = 1;// mandant.GetKey();
            obj.AsDynamic().OWBNAME = TestString;
            obj.AsDynamic().OWBPRIORITY = TestDecimal;
            obj.AsDynamic().OWBOUTDATEPLAN = DateTime.Now;
            obj.AsDynamic().OWBRECIPIENT = 1;//mandant.GetKey();
            obj.AsDynamic().OWBPRODUCTNEED = TestString;
            obj.AsDynamic().OWBGROUP = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(OWBID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(OWB obj)
        {
            obj.AsDynamic().OWBHOSTREF = TestString;
        }

        protected override void CheckSimpleChange(OWB source, OWB dest)
        {
            string sourceName = source.AsDynamic().OWBHOSTREF;
            string destName = dest.AsDynamic().OWBHOSTREF;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        //[Test]
        //public void AddOwb()
        //{
        //    OWB owb;
        //    using (var mgr = CreateManager())
        //    {
        //        owb = mgr.New();
        //        owb.AsDynamic().OWBNAME = "Test for 14000 pos";
        //        owb.AsDynamic().OWBPRIORITY = TestDecimal;
        //        owb.AsDynamic().MANDANTID = 1;
        //        owb.AsDynamic().OWBPRODUCTNEED = TestString;
        //        owb.AsDynamic().OWBOUTDATEPLAN = DateTime.Now;
        //        owb.AsDynamic().OWBRECIPIENT = 1;
        //        mgr.Insert(ref owb);
        //    }

        //    using (var mgr = new OWBPosTest().CreateManager())
        //    {
        //        for (int i = 1; i <= 14000; i++)
        //        {
        //            var owbpos = new OWBPos();
        //            owbpos.AsDynamic().OWBID_R = owb.GetKey();
        //            owbpos.AsDynamic().OWBPOSNUMBER = i;
        //            owbpos.AsDynamic().SKUID_R = 102294;
        //            owbpos.AsDynamic().OWBPOSCOUNT = TestDecimal;
        //            mgr.Insert(ref owbpos);
        //        }
        //    }
        //}
    }
}