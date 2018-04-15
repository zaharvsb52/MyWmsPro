using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class PLTest : BaseEntityTest<PL>
    {
        public const decimal ExistsItem1Id = -1;

        protected override void FillRequiredFields(PL entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.PLTYPE = TestString;
            obj.MPLCODE_R = TestString;
        }
    }
}