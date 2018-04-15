using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class User2MandantTest : BaseEntityTest<User2Mandant>
    {
        protected override void FillRequiredFields(User2Mandant entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.USER2MANDANTUSERCODE = UserTest.ExistsItem1Code;
            obj.MANDANTID = TstMandantId;
            obj.User2MandantIsActive = TestBool;
        }
    }
}