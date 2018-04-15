using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using wmsMLC.APS.wmsSI.Wrappers;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.APS.wmsSI
{
    public partial class IntegrationService
    {
        public PurchaseInvoiceWrapper[] StatusArrivalRequest(RequestWrapper request)
        {
            Contract.Requires(request != null);
            Log.InfoFormat("Start of StatusArrivalRequest");
            Log.DebugFormat("Сущность - «{0}», фильтр - «{1}», значение - «{2}»", request.ENTITY, request.FILTER, request.VALUES);
            var startAllTime = DateTime.Now;
            Log.Debug(request.DumpToXML());

            decimal? mandantID = null;
            mandantID = CheckMandant(mandantID, request.MandantCode);
            if (mandantID == null) 
                throw new NullReferenceException("MandantCode");
            Log.DebugFormat("Мандант = {0}", request.MandantCode);

            var listArrival = new List<PurchaseInvoiceWrapper>();
            try
            {
                using (var mgr = IoC.Instance.Resolve<IBaseManager<IWB>>())
                {
                    switch (request.FILTER)
                    {
                        case "NAME":
                            actArrival(mgr, listArrival, request, mandantID);
                            break;
                        case "INDATEPLAN":
                            statusArrival(mgr, listArrival, request, mandantID);
                            break;
                        default:
                            var badFilter = new PurchaseInvoiceWrapper { STATUSCODE_R_NAME = string.Format("Некорректный фильтр - {0}", request.FILTER) };
                            listArrival.Add(badFilter);
                            break;
                    }
                }
            }
            catch (IntegrationLogicalException iex)
            {
                var message = ExceptionHelper.ExceptionToString(iex);
                Log.Error(message, iex);

                var ew = new PurchaseInvoiceWrapper { STATUSCODE_R_NAME = message };
                listArrival.Add(ew);
            }
            catch (Exception ex)
            {
                var message = ExceptionHelper.ExceptionToString(ex);
                Log.Error(message, ex);

                var ew = new PurchaseInvoiceWrapper { STATUSCODE_R_NAME = "Системная ошибка: " + message };
                listArrival.Add(ew);
            }
            finally
            {
                Log.DebugFormat("Общее время работы {0}", DateTime.Now - startAllTime);
                Log.InfoFormat("End of StatusArrivalRequest");
            }
            return listArrival.ToArray();
        }

        private void actArrival(IBaseManager<IWB> mgr, List<PurchaseInvoiceWrapper> listArrival, RequestWrapper request, decimal? mandantID)
        {
            string filter;
            if (string.IsNullOrEmpty(request.VALUES))
                throw new IntegrationLogicalException("Не заполнено значение фильтра отбора для «{0}»!", request.ENTITY + request.FILTER);

            if (!request.VALUES.Contains(","))
                filter = request.ENTITY + request.FILTER + "='" + request.VALUES + "'";
            else
                filter = request.ENTITY + request.FILTER + " in ('" + request.VALUES.Replace(",", "','") + "')";
            var filterFull = string.Format("{0} = {1} and {2}",
                SourceNameHelper.Instance.GetPropertySourceName(typeof(IWB), IWB.MANDANTIDPropertyName), mandantID, filter);

            var arrivalFullArray = mgr.GetFiltered(filterFull).ToArray();
            if (arrivalFullArray.Length == 0)
                throw new IntegrationLogicalException("Приходная накладная «{0}» не существует!", request.FILTER + "=" + request.VALUES);
            foreach (var arrival in arrivalFullArray)
            {
                var arrivalWrapper = new PurchaseInvoiceWrapper
                {
                    IWBNAME = arrival.IWBName,
                    IWBDESC = arrival.IWBDesc,
                    IWBHOSTREF = arrival.IWBHostRef,
                    IWBINDATEPLAN = arrival.InDatePlan,
                    IWBTYPE = arrival.IWBType,
                    IWBSENDER_NAME = arrival.SenderName,
                    IWBLOADBEGIN = arrival.LoadBegin,
                    IWBLOADEND = arrival.LoadEnd,
                    IWBARRIVED = arrival.TrafficArrived,
                    IWBDEPARTED = arrival.TrafficDeparted,
                    MandantCode = request.MandantCode,
                    STATUSCODE_R_NAME = arrival.StatusName
                };

                if (arrival.IWBPosL != null)
                {
                    arrivalWrapper.IWBPOSL = new List<IWBPosWrapper>();
                    foreach (var arrivalPos in arrival.IWBPosL)
                    {
                        var posWrapper = new IWBPosWrapper
                        {
                            IWBPOSNUMBER = arrivalPos.IWBPosNumber,
                            IWBPOSARTNAME = arrivalPos.IWBPosArtName,
                            IWBPOSBATCH = arrivalPos.IWBPosBatch,
                            IWBPOSCOUNT = arrivalPos.IWBPosCount,
                            IWBPOSINVOICENUMBER = arrivalPos.IWBPosInvoiceNumber,
                            IWBPOSMEASURE = arrivalPos.VMeasureName,
                            IWBPOSPRODUCTCOUNT = arrivalPos.IWBPosProductCount,
                            QLFCODE_R = arrivalPos.VQlfName
                        };
                        arrivalWrapper.IWBPOSL.Add(posWrapper);
                    }
                }
                listArrival.Add(arrivalWrapper);
            }
        }

        private void statusArrival(IBaseManager<IWB> mgr, List<PurchaseInvoiceWrapper> listArrival, RequestWrapper request, decimal? mandantID)
        {
            var filter = string.Format("{0} = {1} and {2} >= to_date('{3}','dd.mm.yyyy') and rownum <= 100 order by {2} desc",
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(IWB), IWB.MANDANTIDPropertyName), mandantID,
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(IWB), IWB.IWBINDATEPLANPropertyName), request.VALUES);

            var arrivalArray = mgr.GetFiltered(filter, GetModeEnum.Partial).ToArray();

            foreach (var arrival in arrivalArray)
            {
                var arrivalWrapper = new PurchaseInvoiceWrapper
                {
                    IWBNAME = arrival.IWBName,
                    IWBDESC = arrival.IWBDesc,
                    IWBHOSTREF = arrival.IWBHostRef,
                    IWBINDATEPLAN = arrival.InDatePlan,
                    IWBTYPE = arrival.IWBType,
                    IWBSENDER_NAME = arrival.SenderName,
                    IWBLOADBEGIN = arrival.LoadBegin,
                    IWBLOADEND = arrival.LoadEnd,
                    IWBARRIVED = arrival.TrafficArrived,
                    IWBDEPARTED = arrival.TrafficDeparted,
                    MandantCode = request.MandantCode,
                    STATUSCODE_R_NAME = arrival.StatusName
                };
                listArrival.Add(arrivalWrapper);
            }
        }
    }
}
