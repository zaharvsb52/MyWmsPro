using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class EventKind2MandantTest : BaseWMSObjectTest<EventKind2Mandant>
    {
        private readonly EventKindTest _eventKindTest = new EventKindTest();
        private readonly MandantTest _mandantTest = new MandantTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _eventKindTest, _mandantTest };
        }

        protected override void FillRequiredFields(EventKind2Mandant obj)
        {
            base.FillRequiredFields(obj);

            var eventKind = _eventKindTest.CreateNew();
            var mandant = _mandantTest.CreateNew();

            obj.AsDynamic().EVENTKIND2MANDANTID = TestDecimal;
            obj.AsDynamic().EVENTKIND2MANDANTEVENTKINDCODE = eventKind.GetKey();
            // Используем зарезервированного манданта для тестов
            obj.AsDynamic().EVENTKIND2MANDANTPARTNERID = mandant.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(EVENTKIND2MANDANTID = '{0}')", TestDecimal);
        }
    }
}