using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Business.Managers
{
    public class CargoIWBManager : WMSBusinessObjectManager<CargoIWB, decimal>, ICargoManager
    {
        public void FillData(WMSBusinessObject entity)
        {
            var obj = entity as CargoIWB;
            if (obj == null)
                return;

            obj.Vehicle = null;
            obj.WorkerFIO = null;

            InternalTraffic it;
            if (obj.InternalTrafficID == null)
                return;

            using (var mgr = GetManager<InternalTraffic>())
                it = mgr.Get(obj.InternalTrafficID, GetModeEnum.Partial);

            if (it == null || it.ExternalTrafficID == null)
                return;

            ExternalTraffic et;
            using (var mgrEx = GetManager<ExternalTraffic>())
                et = mgrEx.Get(it.ExternalTrafficID, GetModeEnum.Partial);

            if (et == null)
                return;

            if (et.VVehicleRN != null)
                obj.Vehicle = et.VVehicleRN;

            if (et.Driver == null)
                return;

            Worker worker;
            using (var mgrWorker = GetManager<Worker>())
                worker = mgrWorker.Get(et.Driver, GetModeEnum.Partial);

            if (worker == null)
                return;

            if (worker.WorkerFIO != null)
                obj.WorkerFIO = worker.WorkerFIO;
        }
    }
}