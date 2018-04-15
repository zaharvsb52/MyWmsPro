using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class OutputTaskTest : BaseWMSObjectTest<OutputTask>
    {
        private readonly OutputTest _outputTest = new OutputTest();
        private OutputTask _outputTask;

        protected override string GetCheckFilter()
        {
            //return string.Format("(OUTPUTTASKCODE='{0}')", TestString);
            return null; //Нет такого метода
        }

        public override void ManagerGetFilteredTest()
        {
            //Нет такого метода
        }

        protected override void FillRequiredFields(OutputTask obj)
        {
            base.FillRequiredFields(obj);
            var output = _outputTest.CreateNew();

            obj.AsDynamic().OUTPUTID_R = output.GetKey();
            obj.AsDynamic().OUTPUTTASKCODE = TestString;
        }

        protected override void MakeSimpleChange(OutputTask obj)
        {
            obj.AsDynamic().OUTPUTTASKFEEDBACK = TestString;
            _outputTask = obj;
        }

        protected override void CheckSimpleChange(OutputTask source, OutputTask dest)
        {
            string sourceName = source.AsDynamic().OUTPUTTASKFEEDBACK;
            string destName = dest.AsDynamic().OUTPUTTASKFEEDBACK;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        public override void ClearForSelf()
        {
            if (_outputTask != null)
            {
                CreateManager().Delete(_outputTask);
            }
            _outputTest.ClearForSelf();
        }

        [Test, Ignore("Тест не запускаем. Много данных")]
        public override void ManagerGetAllTest()
        {
            //base.ManagerGetAllTest();
        }
        [Test, Ignore("Тест не запускаем.Нет хистори")]
        public override void ManagerGetHistoryTest()
        {
            
        }
        
        [Test, Ignore("В менеджере указан явный вылет при фильтрации")]
        public override void FiltersTest()
        {
            
        }
    }
}
