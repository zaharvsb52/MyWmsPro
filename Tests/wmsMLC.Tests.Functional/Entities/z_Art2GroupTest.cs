using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class Art2GroupTest : BaseWMSObjectTest<Art2Group>
    {
        private readonly ArtTest _artTest = new ArtTest();
        private readonly ArtGroupTest _artGroupTest = new ArtGroupTest();
        private object _selfKey;
        private object _parentKey1;
        private object _parentKey2;
 
        protected override void FillRequiredFields(Art2Group obj)
        {
            base.FillRequiredFields(obj);

            var art = _artTest.CreateNew();
            var artGr = _artGroupTest.CreateNew();

            _selfKey = TestDecimal;
            _parentKey1 = art.GetKey();
            _parentKey2 = artGr.GetKey();

            obj.AsDynamic().ART2GROUPID = TestDecimal;
            obj.AsDynamic().ART2GROUPPRIORITY = TestDecimal;
            obj.AsDynamic().ART2GROUPARTCODE = _parentKey1;
            obj.AsDynamic().ART2GROUPARTGROUPCODE = _parentKey2;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(ART2GROUPID = '{0}')", TestDecimal);
        }

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _artTest, _artGroupTest };
        }

        [Test(Description = DeleteByParentDesc),Ignore("Пока не работает удаление вложенных сущностей")]
        public void DeleteByParentTest()
        {
            DeleteByParent<Art>(_selfKey, _parentKey1);
            DeleteByParent<ArtGroup>(_selfKey, _parentKey2);
        }
    }
}