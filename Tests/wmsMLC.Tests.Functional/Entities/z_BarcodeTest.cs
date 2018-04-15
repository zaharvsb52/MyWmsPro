using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BarcodeTest : BaseWMSObjectTest<Barcode>
    {
        protected override void FillRequiredFields(Barcode obj)
        {
            base.FillRequiredFields(obj);

            var mgr = IoC.Instance.Resolve<ISysObjectManager>();

            obj.AsDynamic().BARCODEKEY = TestString;
            obj.AsDynamic().BARCODEVALUE = TestString;
            obj.AsDynamic().BARCODE2ENTITY = mgr.Get(1);
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(BARCODEKEY = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(Barcode obj)
        {
            obj.AsDynamic().BARCODEVALUE = string.Format("{0}Change", TestString);
        }

        protected override void CheckSimpleChange(Barcode source, Barcode dest)
        {
            string sourceName = source.AsDynamic().BARCODEVALUE;
            string destName = dest.AsDynamic().BARCODEVALUE;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}