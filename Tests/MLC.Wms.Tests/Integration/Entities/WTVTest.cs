using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class WTVTest : BaseEntityTest<WTV>
    {
        protected override void FillRequiredFields(WTV entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.WTVProductHistory = TestDecimal;
            obj.WTVCountSKU = TestDecimal;
        }
    }
}