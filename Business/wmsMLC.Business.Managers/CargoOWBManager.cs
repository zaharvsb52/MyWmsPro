using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Business.Managers
{
    public class CargoOWBManager : WMSBusinessObjectManager<CargoOWB, decimal>, ICargoManager
    {
        public void FillData(WMSBusinessObject entity)
        {
            var obj = entity as CargoOWB;
            if (obj == null)
                return;

            obj.Vehicle = null;
            obj.WorkerFIO = null;
            if (obj.InternalTrafficID == null)
                return;

            InternalTraffic it;
            using (var mgr = GetManager<InternalTraffic>())
                it = mgr.Get(obj.InternalTrafficID, GetModeEnum.Partial);

            if (it.ExternalTrafficID == null)
                return;

            ExternalTraffic et = null;
            using (var mgrET = GetManager<ExternalTraffic>())
                et = mgrET.Get(it.ExternalTrafficID);
            if (et == null)
                return;

            if (et.VVehicleRN != null)
                obj.Vehicle = et.VVehicleRN;

            if (et.Driver == null)
                return;

            Worker worker = null;
            using (var mgrWorker = GetManager<Worker>())
                worker = mgrWorker.Get(et.Driver);

            if (worker == null)
                return;

            if (worker.WorkerFIO != null)
                obj.WorkerFIO = worker.WorkerFIO;
        }
    }
}