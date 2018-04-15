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
        public SalesInvoiceWrapper[] StatusOrderRequest(RequestWrapper request)
        {
            Contract.Requires(request != null);
            Log.InfoFormat("Start of StatusOrderRequest");
            Log.DebugFormat("Сущность - «{0}», фильтр - «{1}», значение - «{2}»", request.ENTITY, request.FILTER, request.VALUES);
            var startAllTime = DateTime.Now;

            decimal? mandantID = null;
            mandantID = CheckMandant(mandantID, request.MandantCode);
            if (mandantID == null) throw new NullReferenceException("MandantCode");
            Log.DebugFormat("Мандант = {0}", request.MandantCode);

            var listOrder = new List<SalesInvoiceWrapper>();
            try
            {
                using (var mgr = IoC.Instance.Resolve<IBaseManager<OWB>>())
                {
                    switch (request.FILTER)
                    {
                        case "NAME":
                            actOrder(mgr, listOrder, request, mandantID);
                            break;
                        case "OUTDATEPLAN":
                            statusOrder(mgr, listOrder, request, mandantID);
                            break;
                        default:
                            var badFilter = new SalesInvoiceWrapper { STATUSCODE_R_NAME = string.Format("Некорректный фильтр - {0}", request.FILTER) };
                            listOrder.Add(badFilter);
                            break;
                    }
                }
            }
            catch (IntegrationLogicalException iex)
            {
                var message = ExceptionHelper.ExceptionToString(iex);
                Log.Error(message, iex);

                var ew = new SalesInvoiceWrapper { STATUSCODE_R_NAME = message };
                listOrder.Add(ew);
            }
            catch (Exception ex)
            {
                var message = ExceptionHelper.ExceptionToString(ex);
                Log.Error(message, ex);

                var ew = new SalesInvoiceWrapper { STATUSCODE_R_NAME = "Системная ошибка: " + message };
                listOrder.Add(ew);
            }
            finally
            {
                Log.DebugFormat("Общее время работы {0}", DateTime.Now - startAllTime);
                Log.InfoFormat("End of StatusOrderRequest");
            }

            return listOrder.ToArray();
        }

        private void actOrder(IBaseManager<OWB> mgr, List<SalesInvoiceWrapper> listOrder, RequestWrapper request, decimal? mandantID)
        {
            var filter = "";
            if (String.IsNullOrEmpty(request.VALUES) || request.VALUES == "")
                throw new IntegrationLogicalException("Не заполнено значение фильтра отбора для «{0}»!", request.ENTITY + request.FILTER);

            if (!request.VALUES.Contains(","))
                filter = request.ENTITY + request.FILTER + "='" + request.VALUES + "'";
            else
                filter = request.ENTITY + request.FILTER + " in ('" + request.VALUES.Replace(",", "','") + "')";
            var filterFull = string.Format("{0} = {1} and {2}",
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(OWB), OWB.MANDANTIDPropertyName), mandantID, filter);

            var orderArray = mgr.GetFiltered(filterFull, GetModeEnum.Partial).ToArray();
            var orderFullArray = mgr.GetFiltered(filterFull).ToArray();
            if (orderArray.Length == 0)
                throw new IntegrationLogicalException("Расходная накладная «{0}» не существует!", request.FILTER + "=" + request.VALUES);

            foreach (var order in orderArray)
            {
                var orderWrapper = new SalesInvoiceWrapper
                {
                    OWBNAME = order.OWBName,
                    OWBDESC = order.OWBDesc,
                    OWBHOSTREF = order.OWBHostRef,
                    OWBOUTDATEPLAN = order.OutDatePlan,
                    OWBTYPE = order.OWBType,
                    OWBRECIPIENT_NAME = order.RecipientName,
                    OWBRECIPIENT_CODE = order.AddressBookRecipient,
                    OWBLOADBEGIN = order.LoadBegin,
                    OWBLOADEND = order.LoadEnd,
                    OWBARRIVED = order.TrafficArrived,
                    OWBDEPARTED = order.TrafficDeparted,
                    MandantCode = request.MandantCode,
                    STATUSCODE_R_NAME = order.StatusName
                };
                var orderFull = orderFullArray.FirstOrDefault(i => i.OWBID == order.OWBID);

                if (orderFull.OWBPosL != null)
                {
                    orderWrapper.OWBPOSL = new List<OWBPosWrapper>();
                    foreach (var orderPos in orderFull.OWBPosL)
                    {
                        var posWrapper = new OWBPosWrapper
                        {
                            OWBPOSNUMBER = orderPos.OWBPosNumber,
                            OWBPOSARTNAME = orderPos.OWBPosArtName,
                            OWBPOSBATCH = orderPos.OWBPosBatch,
                            OWBPOSCOUNT = orderPos.OWBPosCount,
                            OWBPOSRESERVED = orderPos.OWBPosReserved,
                            OWBPOSWANTAGE = orderPos.OWBPosWantage,
                            OWBPOSMEASURE = orderPos.VMeasureName,
                            QLFCODE_R = orderPos.VQlfName
                        };
                        orderWrapper.OWBPOSL.Add(posWrapper);
                    }
                }
                listOrder.Add(orderWrapper);
            }
        }

        private void statusOrder(IBaseManager<OWB> mgr, List<SalesInvoiceWrapper> listOrder, RequestWrapper request, decimal? mandantID)
        {
            var filter = request.ENTITY + request.FILTER + "='" + request.VALUES + "'";
            filter = string.Format("{0} = {1} and {2} >= to_date('{3}','dd.mm.yyyy') and rownum <= 100 order by {2} desc",
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(OWB), OWB.MANDANTIDPropertyName), mandantID,
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(OWB), OWB.OWBOUTDATEPLANPropertyName), request.VALUES);

            var orderArray = mgr.GetFiltered(filter, GetModeEnum.Partial).ToArray();

            foreach (var order in orderArray)
            {
                var orderWrapper = new SalesInvoiceWrapper
                {
                    OWBNAME = order.OWBName,
                    OWBDESC = order.OWBDesc,
                    OWBHOSTREF = order.OWBHostRef,
                    OWBOUTDATEPLAN = order.OutDatePlan,
                    OWBTYPE = order.OWBType,
                    OWBRECIPIENT_NAME = order.RecipientName,
                    OWBRECIPIENT_CODE = order.AddressBookRecipient,
                    OWBLOADBEGIN = order.LoadBegin,
                    OWBLOADEND = order.LoadEnd,
                    OWBARRIVED = order.TrafficArrived,
                    OWBDEPARTED = order.TrafficDeparted,
                    MandantCode = request.MandantCode,
                    STATUSCODE_R_NAME = order.StatusName
                };
                listOrder.Add(orderWrapper);
            }
        }

    }
}
