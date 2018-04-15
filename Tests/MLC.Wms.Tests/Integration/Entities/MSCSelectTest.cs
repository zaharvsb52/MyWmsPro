using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class MSCSelectTest : BaseEntityTest<MSCSelect>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "MSCSelectTEType";
        }

        protected override void FillRequiredFields(MSCSelect entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MSCCode_r = MSCTest.ExistsItem1Code;
            obj.Priority = TestDecimal;
            obj.MSCSelectIsVirtual = TestBool;
            obj.MSCSelectTECompleteOWBGroup = TestBool;
            obj.MSCSelectIsBase = TestBool;
        }
    }
}