using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace MLC.Wms.Tests.Integration.Entities
{
    //[TestFixture, Ignore("component 'INSMOTIONAREAGROUPTREE' must be declared")]
    [TestFixture]
    public class MotionAreaGroupTrTest : BaseEntityTest<MotionAreaGroupTr>
    {
        protected override void FillRequiredFields(MotionAreaGroupTr entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MotionAreaGroupCode_r = MotionAreaGroupTest.ExistsItem1Code;
            obj.MotionAreaGroupCodeParen = MotionAreaGroupTest.ExistsItem1Code;
        }

        [Test]
        public void GetMotionAreaGroupTree()
        {
            using (var mgr = IoC.Instance.Resolve<IBaseManager<MotionAreaGroup>>())
            {
                var items = mgr.GetFiltered("1=1 and ROWNUM < 2").ToArray();
                items.Should().NotBeEmpty("По фильтру должны получить одну запись. Проверьте формирование фильтра или наличие данных.");
                items.Should().HaveCount(1);
            }
        }

        [Test, Ignore("Нет методв GETMOTIONAREAGROUPTREELST")]
        public override void Filter_should_return_empty_collections()
        {
            base.Filter_should_return_empty_collections();
        }


        [Test, Ignore("Нет метода GETMOTIONAREAGROUPTREELST")]
        public override void Filter_should_return_non_empty_collections()
        {
            base.Filter_should_return_non_empty_collections();
        }

        [Test, Ignore("Нет метода GETMOTIONAREAGROUPTREELST")]
        public override void Filter_should_work_by_all_entity_fields()
        {
            base.Filter_should_work_by_all_entity_fields();
        }

        [Test, Ignore("Нет метода GETMOTIONAREAGROUPTREEHLST")]
        public override void Entity_should_have_history()
        {
            base.Entity_should_have_history();
        }

        [Test, Ignore("Нет метода INSMOTIONAREAGROUPTREE")]
        public override void Entity_should_be_create_read_update_delete()
        {
            base.Entity_should_be_create_read_update_delete();
        }

    }
}