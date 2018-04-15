using System;
using System.Collections.Generic;
using System.Linq;
using wmsMLC.Business.DAL;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Managers
{
    public class WorkManager : WMSBusinessObjectManager<Work, decimal>, IWorkManager
    {
        public virtual void FillByGroup(decimal workingId, decimal groupId)
        {
            using (var repo = (IWorkRepository)GetRepository())
                repo.FillByGroup(workingId, groupId);
        }
    }

    public class SvcWorkManager : WorkManager
    {
        public override void FillByGroup(decimal workId, decimal groupId)
        {
            // получаем бригаду
            WorkerGroup wg;
            using (var mgr = GetManager<WorkerGroup>())
                wg = mgr.Get(groupId);

            if (wg == null)
                throw new OperationException("Не найдена бригада с кодом " + groupId);

            // получаем работы
            var work = Get(workId);
            if (work == null)
                throw new OperationException("Не найдена работа с кодом " + workId);

            var addingWorkings = new List<Working>();
            if (wg.Worker2GroupL == null)
                wg.Worker2GroupL = new WMSBusinessCollection<Worker2Group>();
            if (work.WORKINGL == null)
                work.WORKINGL = new WMSBusinessCollection<Working>();
            // идем по все членам бригады
            foreach (var item in wg.Worker2GroupL)
            {
                // проверяем внесен ли уже данный работник в работу
                var exitsWorker = work.WORKINGL.FirstOrDefault(i => i.WORKERID_R.Equals(item.WORKER2GROUPWORKERID));

                // нашли - переходим дальше
                if (exitsWorker != null)
                    continue;

                var newWorking = new Working
                {
                    WORKID_R = work.GetKey<decimal>(),
                    WORKERID_R = item.WORKER2GROUPWORKERID,
                    WORKERGROUPID_R = groupId,
                    WORKINGFROM = DateTime.Now,
                    WORKINGADDL = true
                };
                // TODO: перейти на API-шную функцию
                addingWorkings.Add(newWorking);
            }

            // ничего не добавили
            if (addingWorkings.Count == 0)
                return;

            // добавляем работников
            using (var mgr = GetManager<Working>())
            {
                IEnumerable<Working> res = addingWorkings;
                mgr.Insert(ref res);
            }
        }
    }

    public interface IWorkManager : IBaseManager<Work, decimal>
    {
        void FillByGroup(decimal workId, decimal groupId);
    }
}