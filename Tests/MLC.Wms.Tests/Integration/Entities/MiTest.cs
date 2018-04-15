using System;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class MiTest : BaseEntityTest<MI>
    {
        public static Guid ExistsItem1Code = new Guid("00000000-0000-0000-0000-000000000001");

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "MINAME";
        }

        protected override void FillRequiredFields(MI entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MINAME = TestString;
            obj.MIINVTYPE = TestString;
            obj.MIASKSKU = TestBool;
            obj.MILINE = TestDecimal;
            obj.MILINEPERPAGE = TestDecimal;
            obj.MICALCTYPE = TestString;
            obj.MICALCBAN = TestBool;
            obj.MIPIECE = TestBool;
        }
    }
}
