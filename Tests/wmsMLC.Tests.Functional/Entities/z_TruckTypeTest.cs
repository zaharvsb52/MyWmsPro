using BLToolkit.Reflection;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.DAL.Oracle;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    public class TruckTypeTest : BaseWMSObjectTest<TruckType>
    {
        protected override void FillRequiredFields(TruckType obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().TRUCKTYPECODE = TestString;
            obj.AsDynamic().TRUCKTYPENAME = TestString;
            obj.AsDynamic().TRUCKTYPEWEIGHTMAX = TestDecimal;
            obj.AsDynamic().TRUCKTYPEPICKCOUNT = TestDecimal;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(TRUCKTYPECODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(TruckType obj)
        {
            obj.AsDynamic().TRUCKTYPEDESC = TestString;
        }

        protected override void CheckSimpleChange(TruckType source, TruckType dest)
        {
            string sourceName = source.AsDynamic().TRUCKTYPEDESC;
            string destName = dest.AsDynamic().TRUCKTYPEDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }


        [Test, Ignore("Используется новый метод получения истории")]
        public override void ManagerGetHistoryTest()
        {
        }
    
        [Test]
        public void TrackTypeNewHistoryTest()
        {
            using (var repo = TypeAccessor.CreateInstance<TruckTypeRepository>())
            {
                var res = repo.GetHistory(null, null);
            }
        }
    }
}