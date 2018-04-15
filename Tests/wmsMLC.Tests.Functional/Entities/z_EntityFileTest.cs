using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    class EntityFileTest : BaseWMSObjectTest<EntityFile>
    {
        protected override void FillRequiredFields(EntityFile obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().FILEID = TestDecimal;
            obj.AsDynamic().FILE2ENTITY = TestString;
            obj.AsDynamic().FILEKEY = TestString;
            obj.AsDynamic().FILENAME = TestString;
        }

        protected override void MakeSimpleChange(EntityFile obj)
        {
            obj.AsDynamic().FILEDESC = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(FILEID = '{0}')", TestDecimal);
        }

        protected override void CheckSimpleChange(EntityFile source, EntityFile dest)
        {
            string sourceName = source.AsDynamic().FILEDESC;
            string destName = dest.AsDynamic().FILEDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}