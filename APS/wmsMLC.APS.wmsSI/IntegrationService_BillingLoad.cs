using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using wmsMLC.APS.wmsSI.Wrappers;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.APS.wmsSI
{
    public partial class IntegrationService
    {
        public BillWorkActWrapper[] BillWorkActGet(string filter)
        {
            Log.InfoFormat("Start of BillWorkActGet");
            Log.Debug("Получен параметр");
            Log.DebugFormat("фильтр = '{0}'", filter);
            try
            {
                var wal = new List<BillWorkActWrapper>();
                using (var mgr = IoC.Instance.Resolve<IBaseManager<BillWorkAct>>())
                {
                    // TODO: добавить в фильтр проверку -
                    //a)	Акт не отправлен в 1С (Признак отправки акта = «Не отправлен»).
                    //b)	Акт имеет статус «Зафиксирован» или «Отменен».
                    if (String.IsNullOrEmpty(filter)) filter = "STATUSCODE_R not in ('WORKACT_TEMPLATE','WORKACT_CALC')";
                    var actList = mgr.GetFiltered(filter);
                    foreach (var act in actList)
                    {
                        var wrap = new BillWorkActWrapper();
                        wrap = MapTo(act, wrap);

                        //Заполняем 
                        if (act.WorkActDetailExL != null)
                        {
                            List<BillWorkActDetail> detailList;
                            using (var mgrDetail = IoC.Instance.Resolve<IBaseManager<BillWorkActDetail>>())
                            {
                                var details = mgrDetail.GetFiltered(string.Format("WORKACTID_R = {0}", act.GetKey()));
                                if (details == null)
                                    continue;
                                detailList = details.ToList();
                            }

                            var res = detailList.GroupBy(p => new { p.Operation2ContractID, p.WorkActDetailMulti });

                            wrap.WORKACTDETAILEXL = new List<BillWorkActDetailExWrapper>();
                            foreach (var r in res)
                            {
                                var detailWrapper = new BillWorkActDetailExWrapper();
                                detailWrapper.MULT = r.Key.WorkActDetailMulti;
                                detailWrapper.OPERATION2CONTRACTID_R = r.Key.Operation2ContractID;
                                var count = r.Sum(w => w.WorkActDetailCount);
                                detailWrapper.TOTALCOUNT = count.HasValue ? Math.Round(count.Value, 2) : (double?)null;
                                var sum = r.Sum(w => w.WorkActDetailTotalSum);
                                detailWrapper.TOTALSUM = sum.HasValue ? Math.Round(sum.Value, 2) : (double?)null;
                                detailWrapper.WORKACTID_R = act.GetKey<decimal>();
                                wrap.WORKACTDETAILEXL.Add(detailWrapper);
                            }
                        }

                        if (act.CONTRACTID_R != null)
                        {
                            using (var cnt = IoC.Instance.Resolve<IBaseManager<BillContract>>())
                            {
                                var contract = cnt.Get(act.CONTRACTID_R);
                                if (contract != null)
                                {
                                    var contractWrapper = new BillContractWrapper();
                                    contractWrapper = MapTo(contract, contractWrapper);

                                    if (contract.BillOperation2ContractL != null &&
                                        contract.BillOperation2ContractL.Count > 0)
                                    {
                                        contractWrapper.BILLOPERATION2CONTRACTL =
                                            new List<BillOperation2ContractWrapper>();
                                        foreach (var operationContract in contract.BillOperation2ContractL)
                                        {
                                            var operationContractWrapper = new BillOperation2ContractWrapper();
                                            operationContractWrapper = MapTo(operationContract, operationContractWrapper);

                                            if (operationContract.BILLOPERATION2CONTRACTANALYTICSCODE != null)
                                            {
                                                using (var baOwn = IoC.Instance.Resolve<IBaseManager<BillAnalytics>>())
                                                {
                                                    var analiticsOwner =
                                                        baOwn.Get(operationContract.BILLOPERATION2CONTRACTANALYTICSCODE);
                                                    if (analiticsOwner != null)
                                                    {
                                                        var operationAnaliticsWrapper = new BillAnalyticsWrapper();
                                                        operationAnaliticsWrapper = MapTo(analiticsOwner,
                                                            operationAnaliticsWrapper);
                                                        operationContractWrapper.BILLANALYTICS =
                                                            operationAnaliticsWrapper;
                                                    }
                                                }
                                            }

                                            contractWrapper.BILLOPERATION2CONTRACTL.Add(operationContractWrapper);
                                        }
                                    }

                                    if (contract.CONTRACTOWNER != null)
                                    {
                                        using (var cntOwn = IoC.Instance.Resolve<IBaseManager<Mandant>>())
                                        {
                                            var contractOwner = cntOwn.Get(contract.CONTRACTOWNER);
                                            if (contractOwner != null)
                                            {
                                                var mandantWrapper = new MandantWrapper();
                                                mandantWrapper = MapTo(contractOwner, mandantWrapper);
                                                contractWrapper.CONTRACTOWNEROBJ = mandantWrapper;
                                            }
                                        }
                                    }
                                    if (contract.CONTRACTCUSTOMER != null)
                                    {
                                        using (var cntCst = IoC.Instance.Resolve<IBaseManager<Mandant>>())
                                        {
                                            var contractCustom = cntCst.Get(contract.CONTRACTCUSTOMER);
                                            if (contractCustom != null)
                                            {
                                                var mandantWrapper = new MandantWrapper();
                                                mandantWrapper = MapTo(contractCustom, mandantWrapper);
                                                contractWrapper.CONTRACTCUSTOMEROBJ = mandantWrapper;
                                            }
                                        }
                                    }

                                    wrap.CONTRACT = contractWrapper;
                                }
                            }
                        }
                        wal.Add(wrap);
                        Log.Debug(wrap.DumpToXML());
                    }
                }
                return wal.ToArray();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                Log.Debug(ex);
                throw new FaultException<string>(ex.Message, new FaultReason(ex.Message));
            }
            finally
            {
                Log.InfoFormat("End of BillWorkActGet");
            }
        }

        public void BillWorkActCommit(WorkActCommit[] workActCommit)
        {
            Log.InfoFormat("Start of BillWorkActCommit");
            foreach (var workActItem in workActCommit)
            {
                using (var uow = UnitOfWorkHelper.GetUnit())
                {
                    try
                    {
                        uow.BeginChanges();
                        var mgr = IoC.Instance.Resolve<IBaseManager<BillWorkAct>>();
                        mgr.SetUnitOfWork(uow);
                        Log.Debug("Получены параметры:");
                        Log.DebugFormat("акт = '{0}', акт 1С = '{1}', фикс дата = '{2}'", workActItem.ActId,
                            workActItem.ActIdIn1C, workActItem.FixDate1C);
                        var act = mgr.Get(workActItem.ActId);
                        if (act == null)
                            throw new KeyNotFoundException(string.Format("BillWorkAct = '{0}'", workActItem.ActId));
                        act.WORKACTHOSTREF = workActItem.ActIdIn1C;
                        //act.WORKACTFIXDATE = workActItem.FixDate1C;
                        act.WORKACTPOSTINGDATE = workActItem.FixDate1C;
                        act.WORKACTPOSTINGNUMBER = workActItem.ActNumber1C;
                        act.STATUSCODE_R = "WORKACT_COMPLETED";
                        SetXmlIgnore(act, false);
                        mgr.Update(act);
                        uow.CommitChanges();
                    }
                    catch (Exception ex)
                    {
                        uow.RollbackChanges();
                        Log.Error(ex.Message, ex);
                        Log.Debug(ex);
                        throw new FaultException<string>(ex.Message, new FaultReason(ex.Message));
                    }
                    finally
                    {
                        Log.Debug(workActItem.DumpToXML());
                        Log.InfoFormat("End of BillWorkActCommit = '{0}'", workActItem.ActId);
                    }
                }
            }
            Log.InfoFormat("End of BillWorkActCommit");
        }

        public BillWorkActDetailWrapper[] BillWorkActDetailGet(string filter)
        {
            Log.InfoFormat("Start of BillWorkActDetailGet");
            Log.Debug("Получен параметр");
            Log.DebugFormat("фильтр = '{0}'", filter);
            try
            {
                using (var uow = UnitOfWorkHelper.GetUnit())
                {
                    var bwaMgr = IoC.Instance.Resolve<IBaseManager<BillWorkAct>>();
                    bwaMgr.SetUnitOfWork(uow);
                    var filterWa = string.Format("{0} = '{1}'",
                            SourceNameHelper.Instance.GetPropertySourceName(typeof(BillWorkAct), BillWorkAct.WORKACTHOSTREFPropertyName),
                            filter);
                    var billWorkActId = bwaMgr.GetFiltered(filterWa).FirstOrDefault().GetKey();
                    var wad = new List<BillWorkActDetailWrapper>();
                    if (billWorkActId != null)
                    {
                        using (var mgr = IoC.Instance.Resolve<IBaseManager<BillWorkActDetail>>())
                        {
                            var filterWad = string.Format("{0} = {1}",
                                    SourceNameHelper.Instance.GetPropertySourceName(typeof(BillWorkActDetail), BillWorkActDetail.WorkActIDPropertyName),
                                    billWorkActId);
                            var detailList = mgr.GetFiltered(filterWad);
                            wad.AddRange(from detail in detailList
                                         let wrap = new BillWorkActDetailWrapper()
                                         select MapTo(detail, wrap));
                        }
                    }
                    Log.Debug(wad.DumpToXML());
                    return wad.ToArray();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                Log.Debug(ex);
                throw new FaultException<string>(ex.Message, new FaultReason(ex.Message));
            }
            finally
            {
                Log.InfoFormat("End of BillWorkActDetailGet");
            }
        }
    }
}
