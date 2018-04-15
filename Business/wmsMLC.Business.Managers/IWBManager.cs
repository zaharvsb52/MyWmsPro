using System;
using System.Collections.Generic;
using System.Linq;
using wmsMLC.Business.DAL;
using wmsMLC.Business.General;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Business.Managers
{
    public class IWBManager : WMSBusinessObjectManager<IWB, decimal>, IIWBManager
    {
        public const string ActivateMethodName = "Activate";
        public const string CancelMethodName = "Cancel";
        public const string CompleteMethodName = "Complete";

        private static readonly Lazy<IStatusStatemachine> _stateMachine 
            = new Lazy<IStatusStatemachine>(() => IoC.Instance.Resolve<IStatusStatemachine>(typeof(IWB).Name));

        public override void Insert(ref IWB entity)
        {
            var tmp = entity.StatusCode;
            try
            {
                FillStatus(entity, InsertMethodName);
                base.Insert(ref entity);
            }
            catch
            {
                //TODO: в этом месте взведется IsDirty
                entity.StatusCode = tmp;
                throw;
            }
        }

        public override void Insert(ref IEnumerable<IWB> entities)
        {
            var tmp = entities.Select(ent => ent.StatusCode).ToList();
            try
            {
                FillStatus(entities, InsertMethodName);
                base.Insert(ref entities);
            }
            catch
            {
                for (int i = 0; i < tmp.Count; i++)
                    entities.ElementAt(i).StatusCode = tmp[i];
                throw;
            }
        }

        public override void Update(IWB entity)
        {
            FillStatus(entity, UpdateMethodName);
            base.Update(entity);
        }

        public override void Update(IEnumerable<IWB> entities)
        {
            FillStatus(entities, UpdateMethodName);
            base.Update(entities);
        }

        private static void FillStatus(IWB entity, string action)
        {
            var newStatus = _stateMachine.Value.GetNextState(entity.StatusCode, action);
            entity.StatusCode = newStatus.ToString();
        }

        private static void FillStatus(IEnumerable<IWB> entities, string action)
        {
            foreach (var item in entities)
                FillStatus(item, action);
        }

        #region . IIWBManager .
        
        public void Activate(ref IWB entity)
        {
            var newStatus = _stateMachine.Value.GetNextState(entity.StatusCode, ActivateMethodName);
            entity.StatusCode = newStatus.ToString();
            using (var repo = GetRepository<IIWBRepository>())
                repo.Activate(ref entity);
        }

        public void Cancel(ref IWB entity)
        {
            var newStatus = _stateMachine.Value.GetNextState(entity.StatusCode, CancelMethodName);
            entity.StatusCode = newStatus.ToString();
            using (var repo = GetRepository<IIWBRepository>())
                repo.Cancel(ref entity);
        }

        public void Complete(ref IWB entity)
        {
            var newStatus = _stateMachine.Value.GetNextState(entity.StatusCode, CompleteMethodName);
            entity.StatusCode = newStatus.ToString();
            using (var repo = GetRepository<IIWBRepository>())
                repo.Complete(ref entity);
        }

        #endregion
    }
}
