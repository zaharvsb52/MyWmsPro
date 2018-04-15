using System;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class OwbTest : BaseEntityTest<OWB>
    {
        public const decimal ExistsItem1Code = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = OWB.OWBHOSTREFPropertyName;
        }

        protected override void FillRequiredFields(OWB entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MANDANTID = TstMandantId;
            obj.OWBNAME = TestString;
            obj.OWBPRIORITY = TestDecimal;
            obj.OWBOUTDATEPLAN = DateTime.Now;
            obj.OWBRECIPIENT = 1;//mandant.GetKey();
            obj.OWBPRODUCTNEED = TestString;
            obj.OWBGROUP = TestString;
        }
    }
}