using System.Collections.Generic;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Business.Managers
{
    public class InvTaskManager : WMSBusinessObjectManager<InvTask, decimal>
    {
        protected override void BeforeDelete(ref InvTask entity)
        {
            CheckManual(entity);
            base.BeforeDelete(ref entity);
        }

        protected override void BeforeDelete(ref IEnumerable<InvTask> entities)
        {
            foreach (var entity in entities)
                CheckManual(entity);
            base.BeforeDelete(ref entities);
        }

        private void CheckManual(InvTask entity)
        {
            if (!entity.GetProperty<bool>(InvTask.INVTASKMANUALPropertyName))
                throw new OperationException("Задача '{0}', добавленная автоматически, удалению не подлежит!", entity.GetKey());
        }
    }
}
