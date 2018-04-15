using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class UserTest : BaseWMSObjectTest<User>
    {
        [Test, Ignore]
        public void Test()
        {
            var mgr = CreateManager() as IUserManager;
            BLHelper.FillInitialCaches();
            var res = mgr.GetUserRights("DEBUG");
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(LOGIN = '{0}')", TestString);
        }

        protected override void FillRequiredFields(User obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().LOGIN = TestString;
            obj.AsDynamic().USERLASTNAME = TestString;
            obj.AsDynamic().USERNAME = TestString;
            obj.AsDynamic().LANGCODE_R = "RUS";
        }

        protected override void MakeSimpleChange(User obj)
        {
            obj.AsDynamic().USERMIDDLENAME = TestString;
        }

        protected override void CheckSimpleChange(User source, User dest)
        {
            string sourceName = source.AsDynamic().USERMIDDLENAME;
            string destName = dest.AsDynamic().USERMIDDLENAME;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        //[Test(Description = "Manager должен уметь получать объекты частично")]
        //public virtual void PartialGetFilteredTest()
        //{
        //    var mgr = CreateManager();

        //    // задаем несуществующий фильтр
        //    var items = mgr.GetFiltered("1=2");
        //    items.Should().BeEmpty("Фиктивный фильтр не должен работать");

        //    var obj = CreateNew();

        //    // здаем нормальный фильтр
        //    var filter = GetSimpleFilter();
        //    items = mgr.GetFiltered(filter, General.GetModeEnum.Partial);
        //    items.Should().NotBeEmpty("По фильтру должны получить хотя бы одну запись. Проверьте формирование фильтра");

        //    ClearForSelf();
        //}
        [Test, Ignore("Нет хистори")]
        public override void ManagerGetHistoryTest()
        {
        }
    }
}