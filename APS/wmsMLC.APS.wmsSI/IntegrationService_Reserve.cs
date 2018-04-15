using System;
using System.ServiceModel;
using wmsMLC.APS.wmsSI.Messages;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.APS.wmsSI
{
    public partial interface IIntegrationService
    {
        /// <summary>
        /// Резервирование накладной
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(string))]
        ReserveResponse Reserve(ReserveRequest request);
    }

    public partial class IntegrationService
    {
        private const string ReserveOperationCode = "OP_OPERATOR_OWBPROCESSING";

        public ReserveResponse Reserve(ReserveRequest request)
        {
            CheckReserveRequest(request);

            using (var mgr = IoC.Instance.Resolve<IBPProcessManager>())
            {
                OWB owb;
                using (var owbMgr = IoC.Instance.Resolve<IBaseManager<OWB>>())
                    owb = owbMgr.Get(request.OrderId);

                if (owb == null)
                    throw new IntegrationLogicalException(string.Format("Can't find order with id '{0}' or you do not have permission", request.OrderId));

                mgr.ReserveOWBLst(new[] {owb}, ReserveOperationCode);
            }

            // пока все через очередь
            return new ReserveResponse {OrderId = request.OrderId, ReserveState = ReserveStates.Queued};
        }

        private static void CheckReserveRequest(ReserveRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            if (!request.OrderId.HasValue)
                throw new ArgumentException("OrderId can't empty");
        }
    }
}