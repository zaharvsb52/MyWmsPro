using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using BLToolkit.Aspects;
using BLToolkit.Data;
using BLToolkit.DataAccess;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using wmsMLC.Business.Objects;
using wmsMLC.Business.Objects.Processes;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.DAL.Oracle;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class BPProcessRepository : BaseHistoryRepository<BPProcess, string>, IBPProcessRepository
    {
        #region . DB Packages Names .

        private const string PkgBpArchiveName = "pkgBpArchive";
        private const string PkgBpProcessName = "pkgBpProcess";
        private const string PkgBpInputName = "pkgBpInput";
        private const string PkgBpMoveName = "pkgBpMove";
        private const string PkgBillingName = "pkgBilling";
        private const string PkgBpReserveName = "pkgBpReserv";
        private const string PkgBpPackName = "pkgBpPack";
        private const string PkgBpOutputName = "pkgBpOutput";
        private const string PkgBp_ApiName = "pkgBp_Api";
        private const string PkgSKUName = "pkgSKU";
        private const string PkgBpTrafficName = "pkgBpTraffic";
        private const string PkgBpPickName = "pkgBpPick";
        private const string PkgBpInvName = "pkgBpInv";
        private const string PkgBpSupplyChainName = "pkgBpSupplyChain";
        private const string PkgBpPack = "PkgBpPack";
        private const string PkgBpWorkName = "PkgBpWork";
        private const string PkgTEName = "PkgTE";
        private const string PkgBpProductName = "pkgBpProduct";
        private const string PkgBpRouteName = "PkgBpRoute";
        private const string PkgCustomParamValueName = "pkgCustomParamValue";
        private const string PkgBpWtvName = "pkgBpWtv";
        private const string PkgPlaceName = "pkgPlace";
        private const string PkgBpKitName = "pkgBpKit";
        private const string PkgPmConig = "pkgPMConfig";

        #endregion

        #region . DB Procedures Names .

        private const string BlockAreaName = "bpBlockArea";
        private const string BlockTEName = "bpBlockTE";
        private const string BlockPlaceName = "bpBlockPlace";
        private const string BlockSegmentName = "bpBlockSegment";
        private const string GetPlaceLstByStrategyName = "bpGetPlaceLstByStrategy";
        private const string CreateTransportTaskName = "bpCreTransportTask";
        private const string MoveManyTE2PlaceName = "bpMoveManyTE2Place";
        private const string CreateProductName = "bpCreProduct";
        private const string ActivateIWBName = "bpActivateIWB";
        private const string CompleteTransportTaskName = "bpCompleteManyTTask";
        private const string CreateProductByPosName = "bpCreProductByPos";
        private const string GetPlaceByMM = "bpGetPlaceByMM";
        private const string OWBPickedName = "bpOWBPicked";
        private const string CompleteOWBName = "bpCompleteOWB";
        private const string CompleteCargoOWBName = "bpCompleteCargoOWB";
        private const string ChangeReservedName = "bpChangeReserved";
        private const string CancelIwbAcceptName = "bpCancelIWBAccept";
        private const string GetOWBBPStatusName = "getOWBBPStatus";
        private const string CalcWorkActName = "bpCalcWorkAct";
        private const string ClearWorkActName = "bpClearWorkAct";
        private const string ManualReserveName = "bpManualReserve";
        private const string splitProductProcName = "splitProduct";
        private const string UnpackProductName = "bpUnpackProduct";
        private const string CancelTransportTaskName = "bpCancelTransportTask";
        private const string GetPlaceStrategyName = "bpGetPlaceStrategy";
        private const string ConvertToKitName = "bpConvertToKit";
        private const string CancelOWBName = "bpCancelOWB";
        private const string ReturnOwbName = "bpReturnOWB";
        private const string FromOwb2IwbName = "bpFromOwb2Iwb";
        private const string UnfixedIWBName = "bpUnfixedIWB";
        private const string convertSKUtoSKUName = "convertSKUtoSKU";
        private const string GetCargoIWBInfoName = "bpGetCargoIWBInfo";
        private const string GetCargoOWBInfoName = "bpGetCargoOWBInfo";
        private const string CreatePickListName = "bpCrePickList";
        private const string DeletePickListName = "bpDelPickList";
        private const string GetSdclEndPointName = "bpgetSDCLEndPoint";
        private const string CreateInvName = "bpCreateInv";
        private const string FixInvTaskStepName = "bpFixInvTaskStep";
        private const string FixInvName = "bpFixInv";
        private const string CleanInvName = "bpCleanInv";
        private const string FindDifferenceName = "bpFindDifference";
        private const string InvCorrectName = "bpInvCorrect";
        private const string SetTrafficGateName = "bpSetTrafficGate";
        private const string CheckInstanceEntityName = "bpCheckInstanceEntity";
        private const string CompleteTEName = "bpCompleteTE";
        private const string CreateQSupplyChainName = "bpCreateQSupplyChain";
        private const string OWBPickedBySupplyChainName = "bpOWBPickedBySupplyChain";
        private const string CloseBoxName = "bpCloseBox";
        private const string OpenBoxName = "bpOpenBox";
        private const string CreateResGroupName = "bpCreateResGroup";
        private const string CreateSupplyChainName = "bpCreateSupplyChain";
        private const string IsVirtualTEName = "IsVirtualTE";
        private const string IsMonoTEName = "IsMonoTE";
        private const string ReservePickListName = "bpReservePickList";
        private const string GetDateFrDateTillName = "bpGetDateFrDateTill";
        private const string ProcessPLPosName = "bpProcessPLPos";
        private const string CompletePickTEName = "bpCompletePickTE";
        private const string GetLastProductAttrName = "bpgetLastProductAttr";
        private const string CompletePlPosName = "bpCompletePlPos";
        private const string CheckPackOWBName = "bpCheckPackOWB";
        private const string FindTargetTeName = "findTargetTE";
        private const string FindCarrierTeName = "findCarrierTE";
        private const string MoveProductsBySkuName = "bpMoveProductsBySku";
        private const string ChangeOWBRouteName = "bpChangeOWBRoute";
        private const string InsWtvName = "bpInsWtv";
        private const string DeactivatePlPosName = "bpDeactivatePlPos";
        private const string SplitProductWithSKUName = "bpSplitProductWithSKU";
        private const string DismantleKitName = "bpDismantleKit";
        private const string GetPmConfigByParamListFunctionName = "getPMConfigByParamLst";

        #endregion

        public void Run(ExecutionContext context)
        {
            throw new NotImplementedException();
        }

        #region . DB API .

        #region . ВРЕМЕННЫЕ .

        // Статус упаковки расходной накладной
        [SprocName(PkgBpOutputName + "." + GetOWBBPStatusName)]
        [DiscoverParameters(false)]
        public abstract void GetOWBBPStatus(decimal key, out string status);

        [SprocName(PkgBpOutputName + "." + OWBPickedName)]
        [DiscoverParameters(false)]
        public abstract void OWBPicked(decimal owbid, string placecode, decimal notSamePlaceType);

        #endregion . ВРЕМЕННЫЕ .

        #region . Приемка .

        [SprocName(PkgBpInputName + "." + ActivateIWBName)]
        [DiscoverParameters(false)]
        public abstract void ActivateIWB(decimal key);

        public virtual void CreateProductByPosInternal(ref string manageFlag, ref string manageFlagParam,
            string operationCode, decimal? iwbId,
            XmlDocument iwbPosInputXml, int isMigrating, string inputPlaceCode, out XmlDocument[] productXml,
            decimal? cargoIwbId)
        {
            //   procedure pkgBpInput.bpCreProductByPos (pOperationCode in VARCHAR2, pIWBID in INTEGER default NULL, pIWBPosInputXml in XMLType, pIsMigrating in INTEGER, pInputPlaceCode in VARCHAR2 default Null, pProductXml out TListXml, pCargoIWBID in INTEGER default null, pManageFlag in out VARCHAR2, pManageParam in out VARCHAR2);
            var mf = manageFlag;
            var mp = manageFlagParam;
            var res = RunManualDbOperation(db =>
            {
                var pProductXml = new OracleParameter("pProductXml", OracleDbType.Object, ParameterDirection.Output)
                {
                    UdtTypeName = typeof (TLISTXML).Name
                };
                var pManageFlag = new OracleParameter("pManageFlag", OracleDbType.Varchar2, 4000, mf,
                    ParameterDirection.InputOutput);
                var pManageParam = new OracleParameter("pManageParam", OracleDbType.Varchar2, 4000, mp,
                    ParameterDirection.InputOutput);

                var ps = new IDbDataParameter[]
                {
                    pManageFlag,
                    pManageParam,
                    new OracleParameter("pOperationCode", OracleDbType.Varchar2, operationCode, ParameterDirection.Input),
                    new OracleParameter("pIWBID", OracleDbType.Int32, iwbId, ParameterDirection.Input),
                    new OracleParameter("pIWBPosInputXml", OracleDbType.XmlType, iwbPosInputXml,
                        ParameterDirection.Input),
                    new OracleParameter("pIsMigrating", OracleDbType.Int32, isMigrating, ParameterDirection.Input),
                    new OracleParameter("pInputPlaceCode", OracleDbType.Varchar2, inputPlaceCode,
                        ParameterDirection.Input),
                    pProductXml,
                    new OracleParameter("pCargoIWBID", OracleDbType.Int32, cargoIwbId, ParameterDirection.Input)
                };
                db.SetCommand(CommandType.StoredProcedure, PkgBpInputName + "." + CreateProductByPosName, ps)
                    .ExecuteNonQuery();

                mf = null;
                if ((pManageFlag.Value is OracleString) && !((OracleString) pManageFlag.Value).IsNull)
                    mf = Convert.ToString(pManageFlag.Value);

                mp = null;
                if ((pManageParam.Value is OracleString) && !((OracleString) pManageParam.Value).IsNull)
                    mp = Convert.ToString(pManageParam.Value);

                var xmlDocs = ((TLISTXML) pProductXml.Value).Value;
                return xmlDocs;
            });
            // возвращаем out-параметры
            manageFlag = mf;
            manageFlagParam = mp;
            productXml = res;
        }

        public List<Product> CreateProductByPos(ref string manageFlag, ref string manageFlagParam, string operationCode,
            decimal iwbId,
            IWBPosInput posInput, int isMigrating = 0, string placeCode = null)
        {
            var iwbPosInputXml = XmlDocumentConverter.ConvertFrom(posInput);

            XmlDocument[] productXml;
            CreateProductByPosInternal(ref manageFlag, ref manageFlagParam, operationCode, iwbId, iwbPosInputXml,
                isMigrating, placeCode, out productXml, null);
            var result = XmlDocumentConverter.ConvertToListOf<Product>(new List<XmlDocument>(productXml));

            return result;
        }

        public List<Product> CreateProductByCargoIwb(ref string manageFlag, ref string manageFlagParam,
            string operationCode, IWBPosInput posInput,
            string placeCode,
            decimal cargoIwbId, int isMigrating = 0, decimal? iwbId = null)
        {
            XmlDocument[] productXml;
            var iwbPosInputXml = XmlDocumentConverter.ConvertFrom(posInput);
            CreateProductByPosInternal(ref manageFlag, ref manageFlagParam, operationCode, iwbId, iwbPosInputXml,
                isMigrating, placeCode, out productXml, cargoIwbId);
            var result = XmlDocumentConverter.ConvertToListOf<Product>(new List<XmlDocument>(productXml));
            return result;
        }

        // Отмена приемки
        [SprocName(PkgBpInputName + "." + CancelIwbAcceptName)]
        [DiscoverParameters(false)]
        public abstract void CancelIwbAccept(decimal iwbid);

        // Частичная отмена приемки
        public void CancelProductAccept(decimal? iwbid, IEnumerable<decimal> products, bool isAllTe, decimal? workid)
        {
            var sprocName = string.Format("{0}.{1}", PkgBpInputName, "bpCancelProductAccept");
            RunManualDbOperation<object>(db =>
            {
                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("pIwbId", OracleDbType.Decimal, iwbid, ParameterDirection.Input),
                    new OracleParameter("pProducts", OracleDbType.Object, ParameterDirection.Input)
                    {
                        UdtTypeName = typeof (TLISTINT).Name,
                        Value = products != null ? new TLISTINT(products.ToArray()) : null
                    },
                    new OracleParameter("pIsAllTE", OracleDbType.Decimal, isAllTe ? 1 : 0, ParameterDirection.Input),
                    new OracleParameter("pWorkId", OracleDbType.Decimal, workid, ParameterDirection.Input)
                };
                return db.SetCommand(CommandType.StoredProcedure, sprocName, ps).ExecuteNonQuery();
            });
        }

        // Закрыть приход
        public void FinishIwb(IEnumerable<decimal> iwbs, string operationCode, decimal? comactId)
        {
            var sprocName = string.Format("{0}.{1}", PkgBpInputName, "bpFinishIWB");
            RunManualDbOperation<object>(db =>
            {
                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("pIWBLst", OracleDbType.Object, ParameterDirection.Input)
                    {
                        UdtTypeName = typeof (TLISTINT).Name,
                        Value = new TLISTINT(iwbs.ToArray())
                    },
                    new OracleParameter("pOperationCode", OracleDbType.Varchar2, operationCode, ParameterDirection.Input),
                    new OracleParameter("pCommactId", OracleDbType.Decimal, comactId, ParameterDirection.Input)
                };
                return db.SetCommand(CommandType.StoredProcedure, sprocName, ps).ExecuteNonQuery();
            });
        }

        // Создать виртуальные позиции
        public virtual List<IWBPosInput> GetIWBPosInputLst(IEnumerable<IWBPos> iwbPosList, int isMigrating = 0)
        {
            if (iwbPosList == null)
                throw new ArgumentNullException("iwbPosList");

            // выходим без позиции
            var items = iwbPosList.ToArray();
            if (items.Length == 0)
                return new List<IWBPosInput>();

            const string spName = PkgBpInputName + ".bpgetIWBPosInputLst";
            var result = RunManualDbOperation(db =>
            {
                //ВАРИАНТ 1: передача списком, через ","
                // ERROR: если много параметров, то будет возвращать - "ORA-00939: too many arguments for function"
                //                var items = iwbPosList.Select(i => i.GetKey().ToString()).ToArray();
                //                var stm = string.Format("select SYS.XMLTYPE.GETCLOBVAL(COLUMN_VALUE) from TABLE({0}(TLISTINT({1})))", spName, string.Join(",", items));
                //                var resXml = db.SetCommand(stm).ExecuteScalarList<XmlDocument>();
                //                return XmlDocumentConverter.ConvertToListOf<IWBPosInput>(resXml);

                //ВАРИАНТ 2: через UDT
                var ps = new OracleParameter("pIWBPosLst", OracleDbType.Object, ParameterDirection.Input);
                ps.UdtTypeName = "TLISTINT";
                ps.Value = new TLISTINT(iwbPosList.Select(i => i.GetKey<decimal>()).ToArray());
                var pIsMigrating = db.InputParameter("pIsMigrating", isMigrating);
                var stm = string.Format("select SYS.XMLTYPE.GETCLOBVAL(COLUMN_VALUE) from TABLE({0}(:{1}, :{2}))",
                    spName,
                    ps.ParameterName, pIsMigrating.ParameterName);
                var resXml = db.SetCommand(stm, ps, pIsMigrating).ExecuteScalarList<XmlDocument>();
                return XmlDocumentConverter.ConvertToListOf<IWBPosInput>(resXml);

                //ВАРИАНТ 3: НЕ РАБОТАЕТ - "ORA-01484: arrays can only be bound to PL/SQL statements"
                //                var ps = new OracleParameter("pIWBPosLst", OracleDbType.Int32, ParameterDirection.Input);
                //                ps.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
                //                ps.Value = iwbPosList.Select(i => i.GetKey<decimal>()).ToArray();
                //                var stm = string.Format("select SYS.XMLTYPE.GETCLOBVAL(COLUMN_VALUE) from TABLE({0}(TLISTINT(:{1})))", spName, ps.ParameterName);
                //                var resXml = db.SetCommand(stm, ps).ExecuteScalarList<XmlDocument>();
                //                return XmlDocumentConverter.ConvertToListOf<IWBPosInput>(resXml);

                //                var ps = new OracleParameter("pIWBPosLst", OracleDbType.Decimal, ParameterDirection.Input);
                //                ps.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
                //                ps.Value = iwbPosList.Select(i => i.GetKey()).Cast<decimal>().ToArray();
                //                var vRes = new OracleParameter("vRes", OracleDbType.XmlType, ParameterDirection.Output);
                //                vRes.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
                //                var stm = string.Format("begin select COLUMN_VALUE into :vRes from TABLE({0}(TLISTINT(5823))); end;", spName, ps.ParameterName);
                //                var resXml = db.SetCommand(stm, new IDbDataParameter[] {vRes}).ExecuteNonQuery();// .ExecuteScalarList<XmlDocument>();
                //                return new List<IWBPosInput>();

                //                var ps = new OracleParameter("pIWBPosXmlLst", OracleDbType.XmlType, ParameterDirection.Input);
                //                ps.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
                //                ps.Value = XmlDocumentConverter.ConvertFromListOf(iwbPosList).ToArray();
                //                var stm = string.Format("select SYS.XMLTYPE.GETCLOBVAL(COLUMN_VALUE) from TABLE({0}(TListXml(:{1})))", spName, ps.ParameterName);
                //                var resXml = db.SetCommand(stm, ps).ExecuteScalarList<XmlDocument>();
                //                return XmlDocumentConverter.ConvertToListOf<IWBPosInput>(resXml);
            });
            return result;
        }

        // Расфиксировать
        [SprocName(PkgBpInputName + "." + UnfixedIWBName)]
        [DiscoverParameters(false)]
        public abstract void UnfixedIWB(decimal iwbid, string operationcode);

        [SprocName(PkgSKUName + "." + convertSKUtoSKUName)]
        //        [DiscoverParameters(false)]
        [ScalarSource(ScalarSourceType.ReturnValue)]
        public virtual decimal ConvertSKUtoSKU(decimal sourceSkuId, decimal destSkuId, int isPrd, decimal? oldqty)
        {
            var ret = RunManualDbOperation((db) =>
            {
                var ps = db.GetSpParameters(PkgSKUName + "." + convertSKUtoSKUName, false, false);
                ps[0].Value = sourceSkuId;
                ps[1].Value = destSkuId;
                ps[2].Value = isPrd;
                ps[3].Value = oldqty;
                var cmd = string.Format("select ROUND(pkgSKu.convertSKUToSKU(:{0}, :{1}, :{2}, :{3}),16) from dual",
                    ps[0].ParameterName, ps[1].ParameterName, ps[2].ParameterName, ps[3].ParameterName);
                var val = db.SetCommand(cmd, ps).ExecuteScalar();
                return val;
            });
            return decimal.Parse(ret.ToString());
        }

        [SprocName(PkgSKUName + "." + convertSKUtoSKUName)]
        //        [DiscoverParameters(false)]
        [ScalarSource(ScalarSourceType.ReturnValue)]
        protected abstract string ConvertSKUtoSKUInternal(decimal sourceSkuId, decimal destSkuId, int isPrd,
            decimal? oldqty);


        public BPBatch GetDefaultBatchCode(decimal? mandantID, decimal? sKUID, string artCode)
        {
            return RunManualDbOperation(db =>
            {
                var xmlDoc = GetDefaultBatchCodeInternal(mandantID, sKUID, artCode);
                return xmlDoc == null ? null : (BPBatch) XmlDocumentConverter.ConvertTo(typeof (BPBatch), xmlDoc);
            });
        }

        [SprocName("pkgBP_api.bpgetDefaultBatchSelect")]
        [ScalarSource(ScalarSourceType.ReturnValue)]
        [DiscoverParameters(false)]
        protected abstract XmlDocument GetDefaultBatchCodeInternal(decimal? mandantID, decimal? sKUID, string artCode);

        public List<String> GetLastProductAttr(decimal skuId)
        {
            var result = RunManualDbOperation<List<String>>(db =>
            {
                var pSkuId = new OracleParameter("pSKUID", OracleDbType.Int32, skuId, ParameterDirection.Input);
                pSkuId.Value = skuId;

                var sql = string.Format("select * from TABLE({0}.{1}(:{2}))", PkgBpProductName, GetLastProductAttrName,
                    pSkuId.ParameterName);
                var res = db.SetCommand(sql, pSkuId).ExecuteScalarList<string>();
                return res;
            });
            return result;
        }

        public IEnumerable<decimal> GetMinConfig4IwbList(IEnumerable<decimal> iwbIdLst, string cpCode, string cpValue)
        {
            const string spName = PkgBpInputName + ".getMINConfig4IWBList";
            var result = RunManualDbOperation<IEnumerable<decimal>>(db =>
            {
                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("pIWBIDLst", OracleDbType.Object, ParameterDirection.Input)
                    {
                        UdtTypeName = "TLISTINT",
                        Value = new TLISTINT(iwbIdLst.ToArray())
                    },
                    new OracleParameter("pCPCode", OracleDbType.Varchar2, cpCode, ParameterDirection.Input),
                    new OracleParameter("pCPValue", OracleDbType.Varchar2, cpValue, ParameterDirection.Input)
                };
                var stm = string.Format("select * from TABLE({0}(:{1},:{2},:{3}))", spName, ps[0].ParameterName,
                    ps[1].ParameterName, ps[2].ParameterName);
                return db.SetCommand(stm, ps).ExecuteScalarList<decimal>();
            });
            return result;
        }

        public decimal? GetDefaultMIN(decimal iwbId)
        {
            //function bpGetDefaultMIN(pIWBID in INTEGER) return INTEGER as
            const string spName = PkgBpInputName + ".bpGetDefaultMIN";
            var result = RunManualDbOperation(db =>
            {
                var pIwbId = db.InputParameter("pIWBID", iwbId);
                var stm = string.Format("select {0}(:{1}) from dual", spName, pIwbId.ParameterName);
                return db.SetCommand(stm, pIwbId).ExecuteScalar<decimal?>();
            });
            return result;
        }

        public void RemoveStorageUnit(IEnumerable<decimal> productId)
        {
            throw new NotImplementedException("Должен реализовываться в BpManagerOracle.");
        }

        public void CascadeDeleteIWB(decimal iwbId)
        {
            throw new NotImplementedException("Должен реализовываться в BpManagerOracle.");
        }

        #endregion

        #region . Перемещение .

        [SprocName(PkgBpProcessName + "." + BlockAreaName)]
        [DiscoverParameters(false)]
        public abstract void BlockArea(string key, string blockingCode, string description, out decimal blockingID);

        [SprocName(PkgBpProcessName + "." + BlockTEName)]
        [DiscoverParameters(false)]
        public abstract void BlockTE(string key, string blockingCode, string description, out decimal blockingID);

        [SprocName(PkgBpProcessName + "." + BlockPlaceName)]
        [DiscoverParameters(false)]
        public abstract void BlockPlace(string key, string blockingCode, string description, out decimal blockingID);

        [SprocName(PkgBpProcessName + "." + BlockSegmentName)]
        [DiscoverParameters(false)]
        public abstract void BlockSegment(string key, string blockingCode, string description, out decimal blockingID);

        public List<Place> GetPlaceLstByStrategy(string strategy, string sourceTECode)
        {
            var result = RunManualDbOperation(db =>
            {
                var pkg = PkgBpMoveName + "." + GetPlaceLstByStrategyName;
                var ps = db.GetSpParameters(pkg, false, false);
                ps[0].Value = strategy;
                ps[1].Value = sourceTECode;

                var stm = string.Format("select SYS.XMLTYPE.GETCLOBVAL(COLUMN_VALUE) FROM TABLE({0}(:{1}, :{2}))", pkg,
                    ps[0].ParameterName, ps[1].ParameterName);
                var resXml = db.SetCommand(stm, ps).ExecuteScalarList<XmlDocument>();
                return XmlDocumentConverter.ConvertToListOf<Place>(resXml);
            });
            return result;
        }

        [SprocName(PkgBpMoveName + "." + CreateTransportTaskName)]
        [DiscoverParameters(false)]
        public abstract void CreateTransportTask(string sourceTECode, string destPlaceCode, string strategy,
            string destTECode, out decimal transportTaskID, decimal? productID);

        public List<TransportTask> MoveManyTE2Place(IEnumerable<TE> teList, string destPlaceCode, string strategy,
            string destTECode, IEnumerable<Product> productList, int isManual)
        {
            var sprocName = PkgBpMoveName + "." + MoveManyTE2PlaceName;

            return RunManualDbOperation(db =>
            {
                var outParam = new OracleParameter("pTransportTaskList", OracleDbType.Object, ParameterDirection.Output)
                {
                    UdtTypeName = "TLISTINT"
                };

                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("pTEList", OracleDbType.Object, ParameterDirection.Input)
                    {
                        UdtTypeName = "TLISTSTRINGS",
                        Value = new TLISTSTRINGS(teList.Select(i => i.GetKey<string>()).ToArray())
                    },
                    new OracleParameter("pDestPlaceCode", OracleDbType.Varchar2, destPlaceCode, ParameterDirection.Input),
                    new OracleParameter("pStrategy", OracleDbType.Varchar2, strategy, ParameterDirection.Input),
                    new OracleParameter("pDestTECode", OracleDbType.Varchar2, destTECode, ParameterDirection.Input),
                    outParam,
                    new OracleParameter("pProductList", OracleDbType.Object, ParameterDirection.Input)
                    {
                        UdtTypeName = "TLISTINT",
                        Value = new TLISTINT(productList.Select(i => i.GetKey<decimal>()).ToArray())
                    },
                    new OracleParameter("pIsManual", OracleDbType.Int32, isManual, ParameterDirection.Input)
                };
                var sqlGetItems = string.Format("begin {0}({1}); end;", sprocName, GetParamsString(ps));
                db.SetCommand(sqlGetItems, ps).ExecuteNonQuery();

                var res = (TLISTINT) outParam.Value;

                if (res == null || res.IsNull || res.Value.Length == 0)
                    return null;

                var filterString = string.Format("TTASKID in ({0})", string.Join(",", res.Value));
                var pFilter = db.InputParameter("pFilter", filterString);
                sqlGetItems =
                    string.Format(
                        "select SYS.XMLTYPE.GETCLOBVAL(COLUMN_VALUE) from TABLE(pkgTransportTask.getTransportTaskLst(null,:pFilter))");
                var items = db.SetCommand(sqlGetItems, pFilter).ExecuteScalarList<XmlDocument>();
                return XmlDocumentConverter.ConvertToListOf<TransportTask>(items);
            }, true);
        }

        [SprocName(PkgBpMoveName + "." + GetPlaceByMM)]
        [DiscoverParameters(false)]
        public abstract void BpGetPlaceByMM(string MMCode, string SourceTeCode, ref string MMStrategy,
            string OrderClause, out string PlaceCode);

        // Переехало в WmsAPI
        //[SprocName(PkgBpMoveName + ".bpGetTransportTaskCount")]
        //[DiscoverParameters(false)]
        //public abstract void GetAvailableTransportTaskCount(string filter, out int count);

        //Создать виртуальные ТЕ
        public void CreateVirtualTE(IEnumerable<string> places, string teTypeCode)
        {
            var sprocName = PkgBp_ApiName + ".bpCreateVirtualTEonPlaceList";
            RunManualDbOperation<object>(db =>
            {
                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("pPlaceCodeLst", OracleDbType.Object, ParameterDirection.Input)
                    {
                        UdtTypeName = typeof (TLISTSTRINGS).Name,
                        Value = new TLISTSTRINGS(places.ToArray())
                    },
                    new OracleParameter("pTETypeCode", OracleDbType.Varchar2, teTypeCode, ParameterDirection.Input)
                };
                return db.SetCommand(CommandType.StoredProcedure, sprocName, ps).ExecuteNonQuery();
            });
        }

        [SprocName(PkgBpMoveName + ".bpReserveTransportTask")]
        [DiscoverParameters(false)]
        public abstract void ReserveTransportTaskInternal(string filter, decimal? currentTtaskId, ref XmlDocument entXml);

        public TransportTask ReserveTransportTask(decimal? currentTransportTaskCode, string filter)
        {
            XmlDocument resultXml = null;
            ReserveTransportTaskInternal(filter, currentTransportTaskCode, ref resultXml);
            return resultXml == null
                ? null
                : (TransportTask) XmlDocumentConverter.ConvertTo(typeof (TransportTask), resultXml);
        }

        [SprocName(PkgBpMoveName + ".bpResetTransportTask")]
        [DiscoverParameters(false)]
        public abstract void ResetTransportTask([ParamName("TTaskID")] decimal currentTransportTaskCode);

        public void ReserveOrActivateTransportTask(decimal tTaskId, string clientCode, string truckCode,
            DateTime? taskBegin, BillOperationCode operationCode)
        {
            throw new DeveloperException("Реализация в Manager-е");
        }

        public void CompleteTransportTask(IEnumerable<decimal> tTaskIDLst, decimal? workerCode, string truckCode,
            DateTime? startDate, DateTime? endDate, string teTypeCode, IEnumerable<string> teCodeLst,
            bool isNotNeededCreateWork)
        {
            var sprocName = string.Format("{0}.{1}", PkgBpMoveName, CompleteTransportTaskName);
            RunManualDbOperation<object>(db =>
            {
                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("pTTaskIDLst", OracleDbType.Object, ParameterDirection.Input)
                    {
                        UdtTypeName = typeof (TLISTINT).Name,
                        Value = new TLISTINT(tTaskIDLst.ToArray())
                    },
                    new OracleParameter("pWorkerID", OracleDbType.Decimal, workerCode, ParameterDirection.Input),
                    new OracleParameter("pTruckCode", OracleDbType.Varchar2, truckCode, ParameterDirection.Input),
                    new OracleParameter("pTTaskBegin", OracleDbType.Date, startDate, ParameterDirection.Input),
                    new OracleParameter("pTTaskEnd", OracleDbType.Date, endDate, ParameterDirection.Input),
                    new OracleParameter("pTETypeCode", OracleDbType.Varchar2, teTypeCode, ParameterDirection.Input),
                    new OracleParameter("pTECodeLst", OracleDbType.Object, ParameterDirection.Input)
                    {
                        UdtTypeName = typeof (TLISTSTRINGS).Name,
                        Value = new TLISTSTRINGS(teCodeLst.ToArray())
                    },
                    new OracleParameter("pIsNotNeededCreateWork", OracleDbType.Int16, isNotNeededCreateWork ? 1 : 0,
                        ParameterDirection.Input)
                };
                return db.SetCommand(CommandType.StoredProcedure, sprocName, ps).ExecuteNonQuery();
            });
        }

        //Отмена ЗНТ
        [SprocName(PkgBpMoveName + "." + CancelTransportTaskName)]
        [DiscoverParameters(false)]
        public abstract void CancelTransportTask(decimal Key, string operationCode);

        // Получение стратегии перемещения
        [SprocName(PkgBpMoveName + "." + GetPlaceStrategyName)]
        [DiscoverParameters(false)]
        public abstract void GetPlaceStrategy(decimal productId, string placeCode, decimal raiseError,
            out decimal strategy);

        // Получение ТЕ для дозагруза
        [SprocName(PkgBpMoveName + "." + FindTargetTeName)]
        [DiscoverParameters(false)]
        public string FindTargetTE(string sourceTECode, string destPlaceCode, decimal? productId, decimal? raiseError,
            string destTECode)
        {
            return RunManualDbOperation(db =>
            {
                const string funcname = PkgBpMoveName + "." + FindTargetTeName;

                var ps = db.GetSpParameters(funcname, false, false);
                ps[0].Value = sourceTECode;
                ps[1].Value = destPlaceCode;
                ps[2].Value = productId;
                ps[3].Value = raiseError;
                ps[4].Value = destTECode;
                var cmd = string.Format("select {0}(:{1}, :{2}, :{3}, :{4}, :{5}) from dual", funcname,
                    ps[0].ParameterName,
                    ps[1].ParameterName, ps[2].ParameterName, ps[3].ParameterName, ps[4].ParameterName);
                var val = db.SetCommand(cmd, ps).ExecuteScalar();
                return val == null ? String.Empty : Convert.ToString(val);
            });
        }

        [SprocName(PkgBpMoveName + "." + FindCarrierTeName)]
        [DiscoverParameters(false)]
        public string FindCarrierTE(string sourceTECode, string destPlaceCode, string strategy, decimal? productId,
            decimal checkActiveTt, string destTECode)
        {
            return RunManualDbOperation(db =>
            {
                const string funcname = PkgBpMoveName + "." + FindCarrierTeName;

                var ps = db.GetSpParameters(funcname, false, false);
                ps[0].Value = sourceTECode;
                ps[1].Value = destPlaceCode;
                ps[2].Value = strategy;
                ps[3].Value = productId;
                ps[4].Value = checkActiveTt;
                ps[5].Value = destTECode;
                var cmd = string.Format("select {0}(:{1}, :{2}, :{3}, :{4}, :{5}, :{6}) from dual", funcname,
                    ps[0].ParameterName,
                    ps[1].ParameterName, ps[2].ParameterName, ps[3].ParameterName, ps[4].ParameterName,
                    ps[5].ParameterName);
                var val = db.SetCommand(cmd, ps).ExecuteScalar();
                return val == null ? String.Empty : Convert.ToString(val);
            });
        }

        // перемещение товара по SKU и TE
        public void MoveProductsBySku(string sourceTeCode, string destTeCode, decimal skuId,
            IEnumerable<decimal> productIds, decimal count, string truckCode, bool isNotNeededCreateWork,
            out decimal transportTaskID)
        {
            var sprocName = string.Format("{0}.{1}", PkgBpMoveName, MoveProductsBySkuName);
            decimal resTransportTaskId = 0;
            RunManualDbOperation<object>(db =>
            {
                var pTransportTaskId = new OracleParameter("pTransportTaskID", OracleDbType.Decimal,
                    ParameterDirection.Output);
                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("pSourceTeCode", OracleDbType.Varchar2, sourceTeCode, ParameterDirection.Input),
                    new OracleParameter("pDestTeCode", OracleDbType.Varchar2, destTeCode, ParameterDirection.Input),
                    new OracleParameter("pSkuId", OracleDbType.Decimal, skuId, ParameterDirection.Input),
                    new OracleParameter("pPrdInputs", OracleDbType.Object, ParameterDirection.Input)
                    {
                        UdtTypeName = typeof (TLISTINT).Name,
                        Value = productIds != null ? new TLISTINT(productIds.ToArray()) : null
                    },
                    new OracleParameter("pCount", OracleDbType.Decimal, count, ParameterDirection.Input),
                    new OracleParameter("pTruckCode", OracleDbType.Varchar2, truckCode, ParameterDirection.Input),
                    new OracleParameter("pIsNotNeededCreateWork", OracleDbType.Int16, isNotNeededCreateWork ? 1 : 0,
                        ParameterDirection.Input),
                    pTransportTaskId
                };

                var result = db.SetCommand(CommandType.StoredProcedure, sprocName, ps).ExecuteNonQuery();
                resTransportTaskId = ((OracleDecimal) (pTransportTaskId.Value)).Value;
                return result;
            });

            transportTaskID = resTransportTaskId;
        }

        // Частичная отмена товара к перемещению
        [SprocName(PkgBpMoveName + ".bpCancelMoveProduct")]
        [DiscoverParameters(false)]
        public abstract void CancelMoveProduct(decimal ttaskid, decimal count);

        // Определить стратегию перемещения ТЕ
        [SprocName(PkgBpMoveName + ".bpGetDefaultMMStrategy")]
        [ScalarSource(ScalarSourceType.ReturnValue)]
        public string GetDefaultMMStrategy(string TECode)
        {
            return RunManualDbOperation(db =>
            {
                const string funcname = PkgBpMoveName + ".bpGetDefaultMMStrategy";

                var ps = db.GetSpParameters(funcname, false, false);
                ps[0].Value = TECode;
                var cmd = string.Format("select {0}(:{1}) from dual", funcname, ps[0].ParameterName);
                var val = db.SetCommand(cmd, ps).ExecuteScalar();
                return val == null ? String.Empty : Convert.ToString(val);
            });
        }

        // Создание ЗНТ дозагруза для автоматического размещения товара
        public void CreTransportTaskAuto(string currentPlaceCode, string sourceTECode, IEnumerable<decimal> prdInputs,
            ref string strategy, IEnumerable<string> skipPlaceLst, out TransportTask trasportTask,
            out decimal productCount)
        {
            const string sprocName = PkgBpMoveName + ".bpCreTransportTaskAuto";
            TransportTask resTrasportTask = null;
            decimal resProductCount = 0;
            var resStrategy = strategy;

            RunManualDbOperation<object>(db =>
            {
                var pCurrentPlaceCode = db.InputParameter("pCurrentPlaceCode", currentPlaceCode);
                var pSourceTECode = db.InputParameter("pSourceTECode", sourceTECode);
                var pPrdInputs = new OracleParameter("pPrdInputs", OracleDbType.Object, ParameterDirection.Input)
                {
                    UdtTypeName = TLISTINT.Name,
                    Value = new TLISTINT(prdInputs.ToArray())
                };
                var pStrategy = new OracleParameter("pStrategy", OracleDbType.Varchar2, 400, resStrategy,
                    ParameterDirection.InputOutput);
                var pSkipPlaceLst = new OracleParameter("pSkipPlaceLst", OracleDbType.Object, ParameterDirection.Input)
                {
                    UdtTypeName = TLISTSTRINGS.Name,
                    Value = new TLISTSTRINGS(skipPlaceLst.ToArray())
                };
                var pTrasportTaskXml = new OracleParameter("pTrasportTaskXml", OracleDbType.XmlType,
                    ParameterDirection.Output);
                var pTtProductCount = new OracleParameter("pTTProductCount", OracleDbType.Decimal,
                    ParameterDirection.Output);

                var ps = new[]
                {
                    pCurrentPlaceCode, pSourceTECode, pPrdInputs, pStrategy, pSkipPlaceLst, pTrasportTaskXml,
                    pTtProductCount
                };
                var stm = string.Format("begin {0}({1}); end;", sprocName, GetParamsString(ps));
                db.SetCommand(stm, ps).ExecuteNonQuery();

                resProductCount = ((OracleDecimal) (pTtProductCount.Value)).Value;
                var oraXml = (OracleXmlType) pTrasportTaskXml.Value;
                resTrasportTask = XmlDocumentConverter.ConvertTo<TransportTask>(oraXml.GetXmlDocument());
                return resTrasportTask;
            });

            strategy = resStrategy;
            productCount = resProductCount;
            trasportTask = resTrasportTask;
        }

        [SprocName("pkgBP_api.bpCheckTE2Move")]
        [ScalarSource(ScalarSourceType.ReturnValue)]
        [DiscoverParameters(false)]
        public abstract decimal CheckTE2Move(string teCode, decimal? prodictID, decimal raiseError);

        public List<String> GetAvailableTE(string teCode, string placeCode, decimal? productID, decimal productCount)
        {
            var res = RunManualDbOperation(db =>
            {
                const string funcname = PkgBpMoveName + ".bpGetAvailableTE";

                var ps = db.GetSpParameters(funcname, false, false);
                ps[0].Value = teCode;
                ps[1].Value = placeCode;
                ps[2].Value = productID;
                ps[3].Value = productCount;

                var cmd = string.Format("select {0}(:{1}, :{2}, :{3}, :{4}) from dual", funcname, ps[0].ParameterName,
                    ps[1].ParameterName, ps[2].ParameterName, ps[3].ParameterName);
                var val = db.SetCommand(cmd, ps).ExecuteScalar();
                return (TLISTSTRINGS) val;
            });
            if (res == null)
                return null;

            return res.Value == null ? null : res.Value.ToList();
        }

        public void ChangePlaceStatus(string placeCode, string operation)
        {
            var sprocName = string.Format("{0}.chngPlaceStatus", PkgPlaceName);
            RunManualDbOperation<object>(db =>
            {
                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("pKey", OracleDbType.Varchar2, placeCode, ParameterDirection.Input),
                    new OracleParameter("pOperationCode", OracleDbType.Varchar2, operation, ParameterDirection.Input),
                };
                return db.SetCommand(CommandType.StoredProcedure, sprocName, ps).ExecuteNonQuery();
            });
        }

        #endregion Перемещение

        #region . Поставки .

        [SprocName(PkgBpSupplyChainName + "." + CreateQSupplyChainName)]
        [DiscoverParameters(false)]
        public abstract void CreateQSupplyChain(string operationCode, decimal? mandantID, decimal? resGroup,
            string tECode, decimal? supplyChainID, decimal? raiseErr, string process);


        public void CreateQSupplyChainTt(string operationCode, decimal? mandantID, decimal? resGroup,
            string tECode,
            decimal? supplyChainID, decimal? raiseErr, string process, out decimal qSupplyChainID)
        {
            decimal resqSCHId = 0;
            const string pkg = PkgBpSupplyChainName + "." + CreateQSupplyChainName;
            RunManualDbOperation<object>(db =>
            {
                var poperationCode = db.InputParameter("pOperationCode", operationCode);
                var pmandantID = db.InputParameter("pMandantID", mandantID);
                var pResGroup = db.InputParameter("pResGroup", resGroup);
                var pTECode = db.InputParameter("pTECode", tECode);
                var pSupplyChainID = db.InputParameter("pSupplyChainID", supplyChainID);
                var pRaiseErr = db.InputParameter("pRaiseErr", raiseErr);
                var pProcess = db.InputParameter("pProcess", process);
                var pQSupplyChainID = new OracleParameter("pQSupplyChainID", OracleDbType.Decimal,
                    ParameterDirection.Output);

                var ps = new[]
                {poperationCode, pmandantID, pResGroup, pTECode, pSupplyChainID, pRaiseErr, pProcess, pQSupplyChainID};
                var stm = string.Format("begin {0}({1}); end;", pkg, GetParamsString(ps));

                db.SetCommand(stm, ps).ExecuteNonQuery();


                if (pQSupplyChainID.Value == null)
                {
                    resqSCHId = 0;
                }
                else
                {
                    resqSCHId = ((OracleDecimal) (pQSupplyChainID.Value)).Value;
                }

                return resqSCHId;
            });
            qSupplyChainID = resqSCHId;
        }

        [SprocName(PkgBpSupplyChainName + "." + OWBPickedBySupplyChainName)]
        [DiscoverParameters(false)]
        public abstract void OWBPickedBySupplyChain(decimal owbid, string operationCode);

        [SprocName(PkgBpSupplyChainName + "." + CreateResGroupName)]
        [DiscoverParameters(false)]
        public abstract void CreateResGroup(string entity, string key, out decimal resGroup);

        [SprocName(PkgBpSupplyChainName + "." + CreateSupplyChainName)]
        [DiscoverParameters(false)]
        public abstract void CreateSupplyChain(decimal qSupplyChainID, out decimal ttaskID);

        // Отменить поставку для ТЕ (с отменой ЗНТ)
        public void CancelSupplyChainForTE(string teCode, string operationCode)
        {
            const string pkg = PkgBpSupplyChainName + ".bpCancelSupplyChainForTE";
            RunManualDbOperation<object>(db =>
            {
                var pTECode = db.InputParameter("pTECode", teCode);
                var pOperationCode = db.InputParameter("pOperationCode", operationCode);
                return db.SetSpCommand(pkg, pTECode, pOperationCode).ExecuteNonQuery();
            });
        }

        #endregion

        #region . Счетчики ТЕ .

        //Диапазон кодов ТЕ по коду счетчика ТЕ
        [SprocName(PkgBp_ApiName + ".bpgetSequenceRange")]
        [DiscoverParameters(false)]
        public abstract void GetTeLabelRange(string sequenceCode, int count, out int rangeBegin, out int rangeEnd);

        #endregion Счетчики ТЕ

        #region . Товар .

        [SprocName(PkgBpInputName + "." + CreateProductName)]
        //[ScalarSource(ScalarSourceType.OutputParameter)]
        [DiscoverParameters(false)]
        public abstract void CreateProductInternal(ref XmlDocument entProduct, ref XmlDocument entTE);

        public void CreateProduct(ref Product product, ref TE te)
        {
            var pXml = XmlDocumentConverter.ConvertFrom(product);
            var tXml = XmlDocumentConverter.ConvertFrom(te);
            CreateProductInternal(ref pXml, ref tXml);
            product = (Product) XmlDocumentConverter.ConvertTo(typeof (Product), pXml);
            te = (TE) XmlDocumentConverter.ConvertTo(typeof (TE), tXml);
        }

        //расщепление товара
        [SprocName(PkgBpProductName + "." + splitProductProcName)]
        [DiscoverParameters(false)]
        public abstract void splitProduct(decimal Key, decimal SplitCount, decimal FreeIfBusy, string operationCode,
            out decimal NewKey);

        // конвертация товара
        public IEnumerable<decimal> SplitProductWithSKU(decimal productId, decimal skuId, decimal countSku,
            double countInSku)
        {
            var res = RunManualDbOperation<TLISTINT>(db =>
            {
                var outParam = new OracleParameter("pNewProductIDLst", OracleDbType.Object, ParameterDirection.Output)
                {
                    UdtTypeName = typeof (TLISTINT).Name
                };

                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("pProductID", OracleDbType.Int32, productId, ParameterDirection.Input),
                    new OracleParameter("pNewSKUID", OracleDbType.Int32, skuId, ParameterDirection.Input),
                    new OracleParameter("pNewCountSKU", OracleDbType.Int32, countSku, ParameterDirection.Input),
                    new OracleParameter("pNewCount", OracleDbType.Decimal, countInSku, ParameterDirection.Input),
                    outParam
                };
                var stm = string.Format("begin {0}({1}); end;", PkgBpProductName + "." + SplitProductWithSKUName,
                    GetParamsString(ps));
                db.SetCommand(stm, ps).ExecuteNonQuery();
                return (TLISTINT) outParam.Value;
            });
            return res.Value;
        }

        // Разукомплектация
        public void DismantleKit(IEnumerable<decimal> productLst, string kitCode)
        {
            var sprocName = string.Format("{0}.{1}", PkgBpKitName, DismantleKitName);
            RunManualDbOperation<object>(db =>
            {
                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("pProductLst", OracleDbType.Object, ParameterDirection.Input)
                    {
                        UdtTypeName = typeof (TLISTINT).Name,
                        Value = productLst != null ? new TLISTINT(productLst.ToArray()) : null
                    },
                    new OracleParameter("pKitCode", OracleDbType.Varchar2, kitCode, ParameterDirection.Input)
                };
                return db.SetCommand(CommandType.StoredProcedure, sprocName, ps).ExecuteNonQuery();
            });
        }

        #endregion

        #region . Резервирование .

        [SprocName(PkgBpReserveName + "." + ManualReserveName)]
        [DiscoverParameters(false)]
        public abstract void ManualReserveInternal(decimal? owbPosId, decimal? owbId, decimal productId,
            decimal owbProductNeed, out decimal outParam);

        // ручное резервирование
        public void ManualReserve(decimal? owbPosId, decimal? owbId, decimal productId, decimal owbProductNeed,
            out decimal outParam)
        {
            ManualReserveInternal(owbPosId, owbId, productId, owbProductNeed, out outParam);
        }

        [SprocName(PkgBpReserveName + "." + ChangeReservedName)]
        [DiscoverParameters(false)]
        public abstract void ChangeReserved(decimal oldProductId, decimal newProductId, decimal countNeeded);

        // Резервировать список накладных
        public void ReserveOWBLst(IEnumerable<OWB> owbLst, string operationCode)
        {
            const string pkg = PkgBpReserveName + ".bpReserveOWBLst";
            RunManualDbOperation<object>(db =>
            {
                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("pOWBIDLst", OracleDbType.Object, ParameterDirection.Input)
                    {
                        UdtTypeName = "TLISTINT",
                        Value = new TLISTINT(owbLst.Select(i => i.GetKey<decimal>()).ToArray())
                    },
                    new OracleParameter("pOperationCode", OracleDbType.Varchar2, operationCode, ParameterDirection.Input)
                };
                return db.SetCommand(CommandType.StoredProcedure, pkg, ps).ExecuteNonQuery();
            });
        }

        // БП "Отмена резервирования"
        public void CancelReserve(string entity, IEnumerable<decimal> idLst, string operationCode, string eventKindCode,
            decimal count)
        {
            const string pkg = PkgBpReserveName + ".bpCancelReserve";
            RunManualDbOperation<object>(db =>
            {
                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("pEntity", OracleDbType.Varchar2, entity, ParameterDirection.Input),
                    new OracleParameter("pIDLst", OracleDbType.Object, ParameterDirection.Input)
                    {
                        UdtTypeName = "TLISTINT",
                        Value = new TLISTINT(idLst.ToArray())
                    },
                    new OracleParameter("pOperationCode", OracleDbType.Varchar2, operationCode, ParameterDirection.Input),
                    new OracleParameter("pEventKindCode", OracleDbType.Varchar2, eventKindCode, ParameterDirection.Input),
                    new OracleParameter("pCount", OracleDbType.Int32, count, ParameterDirection.Input)
                };
                var stm = string.Format("begin {0}({1}); end;", pkg, GetParamsString(ps));
                return db.SetCommand(stm, ps).ExecuteNonQuery();
            });
        }

        #endregion

        #region . Подбор .

        public void CreatePickList(IEnumerable<OWB> owbList, string truckCode = null)
        {
            // сигнатура в БД: pkgBpPick.bpCrePickList(pOWBXmlLst in TListXml, pTruckCode in VARCHAR2 default NULL);
            var sprocName = string.Format("{0}.{1}", PkgBpPickName, CreatePickListName);
            RunManualDbOperation<object>(db =>
            {
                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("pOWBIDLst", OracleDbType.Object, ParameterDirection.Input)
                    {
                        UdtTypeName = typeof (TLISTINT).Name,
                        Value = new TLISTINT(owbList.Select(i => i.GetKey<decimal>()).ToArray())
                    },
                    new OracleParameter("pTruckCode", OracleDbType.Varchar2, truckCode, ParameterDirection.Input)
                };
                var reader =
                    db.SetCommand(CommandType.StoredProcedure, sprocName, ps)
                        .ExecuteReader(CommandBehavior.CloseConnection);
                // TODO: проверить на необходимость таймаута
                if (!reader.IsClosed)
                    reader.Close();
                reader.Dispose();
                return null;
            });
        }

        // Удаление списка пикинга
        public void DeletePickList(IEnumerable<PL> pickList)
        {
            const string pkg = PkgBpPickName + "." + DeletePickListName;
            RunManualDbOperation<object>(db =>
            {
                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("pPLIDLst", OracleDbType.Object, ParameterDirection.Input)
                    {
                        UdtTypeName = typeof (TLISTINT).Name,
                        Value = new TLISTINT(pickList.Select(i => i.GetKey<decimal>()).ToArray())
                    },
                };
                var reader =
                    db.SetCommand(CommandType.StoredProcedure, pkg, ps).ExecuteReader(CommandBehavior.CloseConnection);

                if (!reader.IsClosed)
                    reader.Close();

                reader.Dispose();
                return null;
            });
        }

        // Переехало в WmsAPI
        //public decimal GetPickListCount(string truckCode, decimal? plid)
        //{
        //    return RunManualDbOperation(db =>
        //    {
        //        const string funcname = PkgBpPickName + "." + GetPickListCountName;

        //        var ps = db.GetSpParameters(funcname, false, false);
        //        ps[0].Value = truckCode;
        //        ps[1].Value = plid;
        //        var cmd = string.Format("select {0}(:{1}, :{2}) from dual", funcname, ps[0].ParameterName, ps[1].ParameterName);
        //        var val = db.SetCommand(cmd, ps).ExecuteScalar();
        //        return val == null ? 0 : Convert.ToDecimal(val);
        //    });
        //}

        [SprocName(PkgBpPickName + "." + ReservePickListName)]
        [DiscoverParameters(false)]
        public abstract void ReservePickListInternal(decimal? plid, string truckCode, string operationCode,
            out XmlDocument pl, out XmlDocument work, string mplCode);

        public void ReservePickList(decimal? plid, string truckCode, string operationCode, out PL pl, out Work work,
            string mplCode)
        {
            XmlDocument plXml;
            XmlDocument workXml;
            pl = null;
            work = null;
            ReservePickListInternal(plid, truckCode, operationCode, out plXml, out workXml, mplCode);
            if (plXml != null)
                pl = (PL) XmlDocumentConverter.ConvertTo(typeof (PL), plXml);
            if (workXml != null)
                work = (Work) XmlDocumentConverter.ConvertTo(typeof (Work), workXml);
        }


        public void ProcessPlPosInternal(IEnumerable<decimal> plLst, decimal? plPosId, string targetTeCode,
            decimal? count, string action, out XmlDocument entXml, bool getNext = false, string operationCode = null)
        {
            const string pkg = PkgBpPickName + "." + ProcessPLPosName;
            entXml = RunManualDbOperation(db =>
            {
                var pEntXml = new OracleParameter("pEntXml", OracleDbType.XmlType, ParameterDirection.Output);

                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("pPlLst", OracleDbType.Object, ParameterDirection.Input)
                    {
                        UdtTypeName = "TLISTINT",
                        Value = new TLISTINT(plLst.ToArray())
                    },
                    new OracleParameter("pPLPosID", OracleDbType.Decimal, plPosId, ParameterDirection.Input),
                    new OracleParameter("pTargetTECode", OracleDbType.Varchar2, targetTeCode, ParameterDirection.Input),
                    new OracleParameter("pCount", OracleDbType.Decimal, count, ParameterDirection.Input),
                    new OracleParameter("pAction", OracleDbType.Varchar2, action, ParameterDirection.Input),
                    pEntXml,
                    new OracleParameter("pGetNext", OracleDbType.Int32, getNext, ParameterDirection.Input),
                    new OracleParameter("pOperationCode", OracleDbType.Varchar2, operationCode, ParameterDirection.Input)
                };
                db.SetCommand(CommandType.StoredProcedure, pkg, ps).ExecuteNonQuery();

                var oraXmlValue = (OracleXmlType) pEntXml.Value;
                return oraXmlValue.IsNull ? null : oraXmlValue.GetXmlDocument();
            });
        }

        public PLPos ProcessPlPos(IEnumerable<decimal> plLst, decimal? plPosId, string targetTeCode, decimal? count,
            string action, bool getNext = false, string operationCode = null)
        {
            XmlDocument ret;
            ProcessPlPosInternal(plLst, plPosId, targetTeCode, count, action, out ret, getNext, operationCode);
            return XmlDocumentConverter.ConvertTo<PLPos>(ret);
        }

        [SprocName(PkgBpPickName + "." + CompletePickTEName)]
        [DiscoverParameters(false)]
        public abstract void CompletePickTE(string teCode, out decimal? tTaskID, string operationCode,
            bool instantReserveTtask);

        [SprocName(PkgBpPickName + "." + CompletePlPosName)]
        [DiscoverParameters(false)]
        public abstract void CompletePlPos(decimal idPlPos);

        public void DeactivatePlPos(IEnumerable<decimal> plPosLst)
        {
            var sprocName = string.Format("{0}.{1}", PkgBpPickName, DeactivatePlPosName);
            RunManualDbOperation<object>(db =>
            {
                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("plPosLst", OracleDbType.Object, ParameterDirection.Input)
                    {
                        UdtTypeName = typeof (TLISTINT).Name,
                        Value = new TLISTINT(plPosLst.ToArray())
                    },
                };
                return db.SetCommand(CommandType.StoredProcedure, sprocName, ps).ExecuteNonQuery();
            });
        }

        //Поиск позиции, ТЕ которой содержит товар с заданным ШК
        public PLPos FindNextPlPosByTeByBarcode(decimal plid, string tecode, string barcode, decimal? currentPlPosId, bool needActivated, out SKU sku)
        {
            throw new NotImplementedException("Должен реализовываться в BpManagerOracle.");
        }

        // Проверка кратности единицы учета
        public bool IsMultipleSku(decimal skuId, decimal count, IEnumerable<decimal> skuList)
        {
            throw new DeveloperException("Реализация в Manager-е");
        }

        #endregion

        #region . Упаковка .

        // создать короб
        public TE CreateBox(string teCode, string teTypeCode, string creationPlaceCode)
        {
            throw new DeveloperException("Реализация в Manager-е");
        }

        // упаковано
        public void Packed(decimal owbId)
        {
            //INFO: заглушка
        }

        public void PackProductLst(IEnumerable<decimal> productIdLst, IEnumerable<Product> changedProducts,
            string packTE, decimal packCount, bool packFullProduct)
        {
            throw new DeveloperException("Реализация в Manager-е");
        }

        public void PackProductLstBySKU(IEnumerable<decimal> productIdLst, IEnumerable<Product> changedProducts,
            string packTECode, decimal skuId, decimal packProductCountSKU, bool isEnablePackOtherSKU)
        {
            throw new DeveloperException("Реализация в Manager-е");
        }

        /// <summary>
        /// Упаковка единицы товара
        /// </summary>
        /// <param name="productId">ID товара</param>
        /// <param name="packTE">код ТЕ упаковки</param>
        /// <param name="packCount">кол-во упаковываемого товара</param>
        public void PackProduct(decimal productId, string packTE, decimal packCount)
        {
            //сигнатура в БД: procedure bpPackProduct (pProductID in integer,pPackTE in varchar2,pPackCount in integer)
            RunManualDbOperation<object>(db =>
            {
                var pProductId = db.InputParameter("pProductID", productId);
                var pPackTE = db.InputParameter("pPackTE", packTE);
                var pPackCount = db.InputParameter("pPackCount", packCount);

                var stm = string.Format("begin {0}.{1}(:{2},:{3},:{4}); end;", PkgBpPackName, "bpPackProduct",
                    pProductId.ParameterName, pPackTE.ParameterName, pPackCount.ParameterName);
                return db.SetCommand(stm, pProductId, pPackTE, pPackCount).ExecuteNonQuery();
            });
        }

        // распаковать
        public Product UnPack(decimal productID)
        {
            //function  bpUnpackProduct (pProductID in INTEGER) return XMLType;
            return RunManualDbOperation(db =>
            {
                var pOperationCode = db.InputParameter("pProductID", productID);
                var name = string.Format("{0}.{1}", PkgBpPackName, UnpackProductName);
                var xml = db.SetSpCommand(name, pOperationCode).ExecuteScalar<XmlDocument>(ScalarSourceType.ReturnValue);
                ;
                return XmlDocumentConverter.ConvertTo<Product>(xml);
            });
        }

        // закрыть короб
        public void CloseBox(string operationCode, string teCode)
        {
            //сигнатура в БД:    procedure bpCloseBox (pOperationCode in varchar2, pTECode in varchar2);
            RunManualDbOperation<object>(db =>
            {
                var pOperationCode = db.InputParameter("pOperationCode", operationCode);
                var pTECode = db.InputParameter("pTECode", teCode);

                var stm = string.Format("begin {0}.{1}(:{2},:{3}); end;", PkgBpPack, CloseBoxName,
                    pOperationCode.ParameterName, pTECode.ParameterName);
                return db.SetCommand(stm, pOperationCode, pTECode).ExecuteNonQuery();
            });
        }

        public void OpenBox(string operationCode, string teCode, string packPlaceCode)
        {
            //procedure bpOpenBox (pOperationCode in VARCHAR2, pTECode in VARCHAR2, pPackPlaceCode in VARCHAR2) as
            RunManualDbOperation<object>(db =>
            {
                var pOperationCode = db.InputParameter("pOperationCode", operationCode);
                var pTECode = db.InputParameter("pTECode", teCode);
                var pPackPlaceCode = db.InputParameter("pPackPlaceCode", packPlaceCode);

                var stm = string.Format("begin {0}.{1}(:{2},:{3},:{4}); end;", PkgBpPack, OpenBoxName,
                    pOperationCode.ParameterName, pTECode.ParameterName, pPackPlaceCode.ParameterName);
                return db.SetCommand(stm, pOperationCode, pTECode, pPackPlaceCode).ExecuteNonQuery();
            });
        }

        /// <summary>
        /// Получить вес ТЕ.
        /// </summary>
        public decimal GetTEWeight(string teCode)
        {
            return RunManualDbOperation(db =>
            {
                var pTECode = new OracleParameter("pTECode", OracleDbType.Varchar2, teCode, ParameterDirection.Input);
                var pWeight = new OracleParameter("pWeight", OracleDbType.Decimal, ParameterDirection.Output);

                db.SetCommand(CommandType.StoredProcedure, PkgTEName + ".bpGetTEWeight", pTECode, pWeight)
                    .ExecuteNonQuery();
                return ((OracleDecimal) (pWeight.Value)).Value;
            });
        }

        /// <summary>
        /// Получить вес ТЕ и погрешность, если определена.
        /// </summary>
        public void GetTEWeightControl(string teCode, out decimal weight, out decimal? dev)
        {
            var weighti = decimal.Zero;
            decimal? devi = null;

            RunManualDbOperation<object>(db =>
            {
                var pTECode = new OracleParameter("pTECode", OracleDbType.Varchar2, teCode, ParameterDirection.Input);
                var pDev = new OracleParameter("pDev", OracleDbType.Decimal, ParameterDirection.Output);

                var result =
                    db.SetCommand(CommandType.StoredProcedure, PkgBp_ApiName + ".bpGetTEWeightControl", pTECode, pDev)
                        .ExecuteNonQuery();

                var oracleDecimal = (OracleDecimal) pDev.Value;
                devi = oracleDecimal.IsNull ? (decimal?) null : oracleDecimal.Value;
                weighti = GetTEWeight(teCode);
                return result;
            });

            weight = weighti;
            dev = devi;
        }

        // Вернуть на исходную ТЕ
        public void ReturnOnSourceTE(IEnumerable<decimal> productIDLst, string placeCode)
        {
            const string pkg = PkgBpPackName + ".bpReturnOnSourceTE";
            RunManualDbOperation<object>(db =>
            {
                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("pProductIDLst", OracleDbType.Object, ParameterDirection.Input)
                    {
                        UdtTypeName = "TLISTINT",
                        Value = new TLISTINT(productIDLst.ToArray())
                    },
                    new OracleParameter("pPlaceCode", OracleDbType.Varchar2, placeCode, ParameterDirection.Input)
                };
                return db.SetCommand(CommandType.StoredProcedure, pkg, ps).ExecuteNonQuery();
            });
        }

        // проверка упакованности
        public string CheckPackOWB(decimal productID, int? checkWh)
        {
            const string pkg = PkgBpPackName + "." + CheckPackOWBName;
            return RunManualDbOperation(db =>
            {
                var pProductId = db.InputParameter("pProductId", productID);
                var pCheckWh = db.InputParameter("pCheckWh", checkWh);
                var cmd = string.Format("select {0}(:{1}, :{2}) from dual", pkg, pProductId.ParameterName,
                    pCheckWh.ParameterName);
                return db.SetCommand(cmd, pProductId, pCheckWh).ExecuteScalar<string>();
            });
        }

        public void UnpackTe(string teCode, string placeCode)
        {
            throw new NotImplementedException("Реализуется на уровне Manager-а");
        }

        #endregion

        #region . Отгрузка .

        // Завершение отгрузки по расходной накладной
        [SprocName(PkgBpOutputName + "." + CompleteOWBName)]
        [DiscoverParameters(false)]
        public abstract void CompleteOWB(decimal key, decimal needTraffic, string operationCode, decimal? itid);

        // Завершение отгрузки по внутреннему рейсу
        [SprocName(PkgBpOutputName + "." + CompleteCargoOWBName)]
        [DiscoverParameters(false)]
        public abstract void CompleteCargoOWB(decimal key, string operationCode);

        // Поместить комплекты (отгрузочная часть)
        [SprocName(PkgBpOutputName + "." + ConvertToKitName)]
        [DiscoverParameters(false)]
        public abstract void ConvertToKit(decimal owbid);

        // Аннуляция расходной накланой
        [SprocName(PkgBpOutputName + "." + CancelOWBName)]
        [DiscoverParameters(false)]
        public abstract void CancelOWB(decimal owbid, string operationcode, string eventKind);

        // Возврат накладной
        [SprocName(PkgBpOutputName + "." + ReturnOwbName)]
        [DiscoverParameters(false)]
        public abstract void ReturnOwb(string operationcode, decimal key, string returnplacecode);

        // Отгрузка ТЕ
        public void CompleteTE(string teCode, decimal cargoOWBID, string operationcode, out bool isLastTE)
        {
            CompleteTEInternal(teCode, cargoOWBID, operationcode, out isLastTE);
        }

        //Отгрузка нескольких ТЕ
        public void CompleteManyTE(IEnumerable<string> teCodes, decimal cargoOWBID, string operationcode)
        {
            var sprocName = PkgBpOutputName + ".bpCompleteManyTE";
            RunManualDbOperation<object>(db =>
            {
                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("pTeCodeLst", OracleDbType.Object, ParameterDirection.Input)
                    {
                        UdtTypeName = typeof (TLISTSTRINGS).Name,
                        Value = new TLISTSTRINGS(teCodes.ToArray())
                    },
                    new OracleParameter("pCargoOWBID", OracleDbType.Decimal, cargoOWBID, ParameterDirection.Input),
                    new OracleParameter("pOperationCode", OracleDbType.Varchar2, operationcode, ParameterDirection.Input)
                };
                return db.SetCommand(CommandType.StoredProcedure, sprocName, ps).ExecuteNonQuery();
            });
        }

        [SprocName(PkgBpOutputName + "." + CompleteTEName)]
        [DiscoverParameters(false)]
        protected abstract void CompleteTEInternal(string teCode, decimal cargoOWBID, string operationcode,
            out bool isLastTE);

        public void FromOwb2Iwb(string entity, decimal owbid, decimal iwbid)
        {
            FromOwb2IwbInternal(entity, owbid, iwbid);
        }

        [SprocName(PkgBpOutputName + "." + FromOwb2IwbName)]
        [DiscoverParameters(false)]
        protected abstract void FromOwb2IwbInternal(string entity, decimal owbid, decimal iwbid);

        #endregion

        #region . Биллинг .

        [SprocName(PkgBillingName + "." + CalcWorkActName)]
        [DiscoverParameters(false)]
        public abstract void CalcBillWorkActInternal(decimal workActId, decimal? workAct2Op2cId, decimal? trace,
            bool fictional);

        public void CalcBillWorkAct(decimal workActId, decimal? workAct2Op2cId, decimal? trace, bool fictional)
        {
            CalcBillWorkActInternal(workActId, workAct2Op2cId, trace, fictional);
        }

        [SprocName(PkgBillingName + "." + ClearWorkActName)]
        [DiscoverParameters(false)]
        public abstract void ClearBillWorkActInternal(decimal workActId, decimal? workAct2Op2cId, decimal? isManualOnly);

        public void ClearBillWorkAct(decimal workActId, decimal? workAct2Op2cId, decimal? isManualOnly)
        {
            ClearBillWorkActInternal(workActId, workAct2Op2cId, isManualOnly);
        }

        public void FixBillWorkAct(decimal workActId)
        {
            throw new NotImplementedException();
        }

        public void UnFixBillWorkAct(decimal workActId)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region . Двор .

        [SprocName(PkgBpTrafficName + "." + GetCargoIWBInfoName)]
        [DiscoverParameters(false)]
        public abstract void GetCargoIWBInfoInternal(decimal cargoIWBID, string operationCode, out XmlDocument cargoIWB,
            out XmlDocument work, out XmlDocument internalTraffic, out XmlDocument externalTraffic);

        public void GetCargoIWBInfo(decimal cargoIWBID, string operationCode, out CargoIWB cargoIWB, out Work work,
            out InternalTraffic internalTraffic, out ExternalTraffic externalTraffic)
        {
            cargoIWB = null;
            XmlDocument xmlcargoIWB;

            work = null;
            XmlDocument xmlwork;

            internalTraffic = null;
            XmlDocument xmlinternalTraffic;

            externalTraffic = null;
            XmlDocument xmlexternalTraffic;

            GetCargoIWBInfoInternal(cargoIWBID, operationCode, out xmlcargoIWB, out xmlwork, out xmlinternalTraffic,
                out xmlexternalTraffic);

            if (xmlcargoIWB != null)
                cargoIWB = (CargoIWB) XmlDocumentConverter.ConvertTo(typeof (CargoIWB), xmlcargoIWB);

            if (xmlwork != null)
                work = (Work) XmlDocumentConverter.ConvertTo(typeof (Work), xmlwork);

            if (xmlinternalTraffic != null)
                internalTraffic =
                    (InternalTraffic) XmlDocumentConverter.ConvertTo(typeof (InternalTraffic), xmlinternalTraffic);

            if (xmlexternalTraffic != null)
                externalTraffic =
                    (ExternalTraffic) XmlDocumentConverter.ConvertTo(typeof (ExternalTraffic), xmlexternalTraffic);
        }

        [SprocName(PkgBpTrafficName + "." + GetCargoOWBInfoName)]
        [DiscoverParameters(false)]
        public abstract void GetCargoOWBInfoInternal(decimal cargoOWBID, string operationCode, out XmlDocument cargoOWB,
            out XmlDocument work, out XmlDocument internalTraffic, out XmlDocument externalTraffic, out decimal existTE);

        public void GetCargoOWBInfo(decimal cargoOWBID, string operationCode, out CargoOWB cargoOWB, out Work work,
            out InternalTraffic internalTraffic, out ExternalTraffic externalTraffic, out decimal existTE)
        {
            cargoOWB = null;
            XmlDocument xmlcargoOWB;

            work = null;
            XmlDocument xmlwork;

            internalTraffic = null;
            XmlDocument xmlinternalTraffic;

            externalTraffic = null;
            XmlDocument xmlexternalTraffic;

            GetCargoOWBInfoInternal(cargoOWBID, operationCode, out xmlcargoOWB, out xmlwork, out xmlinternalTraffic,
                out xmlexternalTraffic, out existTE);

            if (xmlcargoOWB != null)
                cargoOWB = (CargoOWB) XmlDocumentConverter.ConvertTo(typeof (CargoOWB), xmlcargoOWB);

            if (xmlwork != null)
                work = (Work) XmlDocumentConverter.ConvertTo(typeof (Work), xmlwork);

            if (xmlinternalTraffic != null)
                internalTraffic =
                    (InternalTraffic) XmlDocumentConverter.ConvertTo(typeof (InternalTraffic), xmlinternalTraffic);

            if (xmlexternalTraffic != null)
                externalTraffic =
                    (ExternalTraffic) XmlDocumentConverter.ConvertTo(typeof (ExternalTraffic), xmlexternalTraffic);
        }

        // Маршрутизировать
        public void RouteTE2CargoOWB(decimal cargoOWBID, IEnumerable<TE> TELstXml)
        {
            const string pkg = PkgBpTrafficName + ".bpRouteTE2CargoOWB";
            RunManualDbOperation<object>(db =>
            {
                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("pTECodeLst", OracleDbType.Object, ParameterDirection.Input)
                    {
                        UdtTypeName = typeof (TLISTSTRINGS).Name,
                        Value = new TLISTSTRINGS(TELstXml.Select(i => i.GetKey<string>()).ToArray())
                    },
                    new OracleParameter("pCargoOWBID", OracleDbType.Int32, cargoOWBID, ParameterDirection.Input)
                };
                var stm = string.Format("begin {0}({1}); end;", pkg, GetParamsString(ps));

                return db.SetCommand(stm, ps).ExecuteNonQuery();
            });
        }

        [SprocName(PkgBpTrafficName + "." + SetTrafficGateName)]
        [DiscoverParameters(false)]
        public abstract void SetTrafficGate(decimal internalTrafficID, string gateCode, string operationCode);

        #endregion

        #region .  Маршрутизация  .

        public List<string> ChangeOWBRoute(IEnumerable<decimal> OWBIDLst, decimal? routeID, DateTime? planDate = null,
            bool changeDate = true, bool changeRoute = true)
        {
            const string pkg = PkgBpRouteName + "." + ChangeOWBRouteName;
            //procedure bpChangeOWBRoute(pOWBIDLst in TListINT, pPlanDate in DATE, pChangeDate in INTEGER default 0, pRouteID in INTEGER, pChangeRoute in INTEGER default 0, pResults out TListStrings)
            return RunManualDbOperation<List<string>>(db =>
            {
                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("pOWBIDLst", OracleDbType.Object, ParameterDirection.Input)
                    {
                        UdtTypeName = typeof (TLISTINT).Name,
                        Value = new TLISTINT(OWBIDLst.ToArray())
                    },
                    new OracleParameter("pPlanDate", OracleDbType.Date, planDate, ParameterDirection.Input),
                    new OracleParameter("pChangeDate", OracleDbType.Int32, changeDate, ParameterDirection.Input),
                    new OracleParameter("pRouteID", OracleDbType.Int32, routeID, ParameterDirection.Input),
                    new OracleParameter("pChangeRoute", OracleDbType.Int32, changeRoute, ParameterDirection.Input),
                    new OracleParameter("pResults", OracleDbType.Object, ParameterDirection.Output)
                    {
                        UdtTypeName = typeof (TLISTSTRINGS).Name,
                        Value = new TLISTSTRINGS()
                    }
                };
                var stm = string.Format("begin {0}({1}); end;", pkg, GetParamsString(ps));
                db.SetCommand(stm, ps).ExecuteNonQuery();
                var result = (TLISTSTRINGS) ps[5].Value;
                return new List<string>(result.Value);
            });
        }

        #endregion

        #region .  Менеджер товара  .

        // Настройка менеджера товара по списку товаров
        public List<PMConfig> GetPMConfigListByProductList(IEnumerable<decimal> productIdList, string operationCode,
            string methodCode)
        {
            var result = RunManualDbOperation(db =>
            {
                var pkg = PkgBpProductName + ".bpGetPMConfigLstByProductLst";

                var pProductIDLst = new OracleParameter("pProductIDLst", OracleDbType.Object, ParameterDirection.Input)
                {
                    UdtTypeName = typeof (TLISTINT).Name,
                    Value = productIdList == null || !productIdList.Any() ? null : new TLISTINT(productIdList.ToArray())
                };
                var pOperationCode = db.InputParameter("pOperationCode", operationCode);
                var pMethodCode = db.InputParameter("pMethodCode", methodCode);

                var stm = string.Format(
                    "select SYS.XMLTYPE.GETCLOBVAL(COLUMN_VALUE) FROM TABLE({0}(:{1}, :{2}, :{3}))", pkg,
                    pProductIDLst.ParameterName, pOperationCode.ParameterName, pMethodCode.ParameterName);
                var resXml =
                    db.SetCommand(stm, pProductIDLst, pOperationCode, pMethodCode).ExecuteScalarList<XmlDocument>();
                return XmlDocumentConverter.ConvertToListOf<PMConfig>(resXml);
            });
            return result;
        }

        //Настройка менеджера товара по списку артикулов
        public List<PMConfig> GetPMConfigListByArtCodeList(IEnumerable<string> artCodeList, string operationCode, string methodCode)
        {
            var arrayArtCode = artCodeList.Distinct().ToArray();
            //var expDates = new SerializableDictionary<string, SerializableList<PMConfig>>();

            if (arrayArtCode == null || arrayArtCode.Length == 0)
                throw new DeveloperException("Parameters artCodeList[] can't be empty");

            var pmConfigFuncFullName = String.Format("{0}.{1}", PkgPmConig, GetPmConfigByParamListFunctionName);
            var sbCommand = new StringBuilder();

            foreach (var artCode in arrayArtCode)
            {
                if (sbCommand.Length > 0)
                    sbCommand.AppendLine("union  all");
                sbCommand.AppendLine(
                    string.Format(
                        "select '{1}' as art, SYS.XMLTYPE.GETCLOBVAL(COLUMN_VALUE) as pmConf from TABLE({0}(null, '{1}', '{2}', '{3}', null, null))",
                        pmConfigFuncFullName, artCode, operationCode, methodCode)
                    );
            }

            var expDates = new List<PMConfig>();
            if (sbCommand.Length > 0)
            {
                var sql = sbCommand.ToString();
                using (var table = ExecuteDataTable(sql))
                {

                    foreach (DataRow rowArtPm in table.Rows)
                    {
                        var xmlDoc = XmlDocumentConverter.XmlDocFromString((string) rowArtPm["pmConf"]);
                        var pmConfig = XmlDocumentConverter.ConvertTo<PMConfig>(xmlDoc);
                        var existArtCode = (string) rowArtPm["art"];
                        pmConfig.SetProperty("ARTCODE_R", existArtCode);
                        expDates.Add(pmConfig);
                    }
                }
            }
            
            return expDates;
        }

        #endregion . Менеджер товара .

        #region . Инвентаризация .

        // Товар, не вошедший в инвентаризацию
        public List<Product> GetInvMissedProduct(string filter, IEnumerable<decimal> invIdLst)
        {
            var result = RunManualDbOperation(db =>
            {
                var pkg = PkgBpInvName + ".bpCheckInvProduct";
                //var ps = db.GetSpParameters(pkg, false, false);

                var pFilter = db.InputParameter("pFilter", filter);
                var pInvLst = new OracleParameter("pInvLst", OracleDbType.Object, ParameterDirection.Input)
                {
                    UdtTypeName = typeof (TLISTINT).Name,
                    Value = invIdLst == null || !invIdLst.Any() ? null : new TLISTINT(invIdLst.ToArray())
                };

                var stm = string.Format("select SYS.XMLTYPE.GETCLOBVAL(COLUMN_VALUE) FROM TABLE({0}(:{1}, :{2}))", pkg,
                    pFilter.ParameterName, pInvLst.ParameterName);
                var resXml = db.SetCommand(stm, pFilter, pInvLst).ExecuteScalarList<XmlDocument>();
                return XmlDocumentConverter.ConvertToListOf<Product>(resXml);
            });
            return result;
        }

        [SprocName(PkgBpInvName + "." + CreateInvName)]
        [DiscoverParameters(false)]
        public abstract void CreateInv(decimal invID, string operationCode);

        public void FixInvTaskStep(IEnumerable<decimal> invTaskGroupIDLst, string operationCode)
        {
            const string pkg = PkgBpInvName + "." + FixInvTaskStepName;
            RunManualDbOperation<object>(db =>
            {
                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("pInvTaskGroupIDLst", OracleDbType.Object, ParameterDirection.Input)
                    {
                        UdtTypeName = "TLISTINT",
                        Value = new TLISTINT(invTaskGroupIDLst.ToArray())
                    },
                    new OracleParameter("pOperationCode", OracleDbType.Varchar2, operationCode, ParameterDirection.Input)
                };
                var stm = string.Format("begin {0}({1}); end;", pkg, GetParamsString(ps));
                return db.SetCommand(stm, ps).ExecuteNonQuery();
            });
        }

        [SprocName(PkgBpInvName + "." + CleanInvName)]
        [DiscoverParameters(false)]
        public abstract void CleanInv(decimal invID);

        [SprocName(PkgBpInvName + "." + FixInvName)]
        [DiscoverParameters(false)]
        public abstract void FixInv(decimal invID, string operationCode);

        [SprocName(PkgBpInvName + ".bpUnFixInv")]
        [DiscoverParameters(false)]
        public abstract void UnFixInv(decimal invId, string operationCode);

        public void FindDifference(decimal invGroupID, IEnumerable<InvTaskGroup> invTaskGroupIDLst, string operationCode,
            out decimal flag)
        {
            flag = 0;
            const string pkg = PkgBpInvName + "." + FindDifferenceName;
            var ps = new IDbDataParameter[]
            {
                new OracleParameter("pInvGroupID", OracleDbType.Int32, invGroupID, ParameterDirection.Input),
                new OracleParameter("pInvTaskGroupIDLst", OracleDbType.Object, ParameterDirection.Input)
                {
                    UdtTypeName = "TLISTINT",
                    Value = new TLISTINT(invTaskGroupIDLst.Select(i => i.GetKey<decimal>()).ToArray())
                },
                new OracleParameter("pOperationCode", OracleDbType.Varchar2, operationCode, ParameterDirection.Input),
                new OracleParameter("pFlag", OracleDbType.Decimal, flag, ParameterDirection.Output)
            };
            RunManualDbOperation<object>(db =>
            {
                var stm = string.Format("begin {0}({1}); end;", pkg, GetParamsString(ps));
                return db.SetCommand(stm, ps).ExecuteNonQuery();
            });
            flag = ((OracleDecimal) (ps[3].Value)).Value;
        }

        [SprocName(PkgBpInvName + "." + InvCorrectName)]
        [DiscoverParameters(false)]
        public abstract void InvCorrect(decimal invID, string operationCode);

        //  Подготовить инвентаризацию
        public void PrepareInv(decimal invReqID, decimal pageRecCount, DateTime dateBegin, out Inv inv)
        {
            const string pkg = PkgBpInvName + ".bpPrepareInv";
            Inv resInv = null;

            RunManualDbOperation<object>(db =>
            {
                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("pInvReqID", OracleDbType.Decimal, invReqID, ParameterDirection.Input),
                    new OracleParameter("pPageRecCount", OracleDbType.Decimal, pageRecCount, ParameterDirection.Input),
                    new OracleParameter("pDateBegin", OracleDbType.Date, dateBegin, ParameterDirection.Input),
                    new OracleParameter("pInv", OracleDbType.XmlType, ParameterDirection.Output)
                };
                var stm = string.Format("begin {0}({1}); end;", pkg, GetParamsString(ps));

                db.SetCommand(stm, ps).ExecuteNonQuery();

                var oraXml = (OracleXmlType) ps[3].Value;
                resInv = XmlDocumentConverter.ConvertTo<Inv>(oraXml.GetXmlDocument());
                return resInv;
            });

            inv = resInv;
        }

        // Получение группы заданий
        public void ReserveInvGroup(decimal invID, ref decimal? invTaskGroupID, string placeCode)
        {
            const string pkg = PkgBpInvName + ".bpReserveInvGroup";
            var tmp = invTaskGroupID;
            invTaskGroupID = RunManualDbOperation<decimal?>(db =>
            {
                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("pInvID", OracleDbType.Decimal, invID, ParameterDirection.Input),
                    new OracleParameter("pInvTaskGroupID", OracleDbType.Decimal, tmp, ParameterDirection.InputOutput),
                    new OracleParameter("pPlaceCode", OracleDbType.Varchar2, placeCode, ParameterDirection.Input)
                };
                var stm = string.Format("begin {0}({1}); end;", pkg, GetParamsString(ps));
                db.SetCommand(stm, ps).ExecuteNonQuery();
                var result = (OracleDecimal) (ps[1].Value);
                return result.IsNull ? (decimal?) null : result.Value;
            });
        }

        public string AcceptPlace(decimal invTaskGroupID, string action)
        {
            const string pkg = PkgBpInvName + ".bpAcceptPlace";
            return RunManualDbOperation<string>(db =>
            {
                var pPlace = new OracleParameter("pPlace", OracleDbType.Varchar2, 128, null, ParameterDirection.Output);
                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("pInvTaskGroupId", OracleDbType.Decimal, invTaskGroupID,
                        ParameterDirection.Input),
                    new OracleParameter("pAction", OracleDbType.Varchar2, action, ParameterDirection.Input),
                    pPlace
                };
                db.SetCommand(CommandType.StoredProcedure, pkg, ps).ExecuteNonQuery();
                var result = (OracleString) pPlace.Value;
                return result.IsNull ? null : result.Value;
            });
        }

        public void AcceptInvTask(InvTask invTask, string action)
        {
            const string pkg = PkgBpInvName + ".bpAcceptInvTask";
            RunManualDbOperation(db =>
            {
                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("pInvTask", OracleDbType.XmlType, XmlDocumentConverter.ConvertFrom(invTask),
                        ParameterDirection.Input),
                    new OracleParameter("pAction", OracleDbType.Varchar2, action, ParameterDirection.Input)
                };
                var stm = string.Format("begin {0}({1}); end;", pkg, GetParamsString(ps));
                return db.SetCommand(stm, ps).ExecuteNonQuery();
            });
        }

        public InvTask GetInvTask(decimal invTaskGroupID, decimal? invTaskID, bool recalc)
        {
            const string pkg = PkgBpInvName + ".bpGetInvTask";

            return RunManualDbOperation(db =>
            {
                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("pInvTaskGroupId", OracleDbType.Decimal, invTaskGroupID,
                        ParameterDirection.Input),
                    new OracleParameter("pInvTaskId", OracleDbType.Decimal, invTaskID, ParameterDirection.Input),
                    new OracleParameter("pRecalc", OracleDbType.Int32, Convert.ToInt32(recalc), ParameterDirection.Input),
                    new OracleParameter("pInvTaskOut", OracleDbType.XmlType, ParameterDirection.Output)
                };
                var stm = string.Format("begin {0}({1}); end;", pkg, GetParamsString(ps));

                db.SetCommand(stm, ps).ExecuteNonQuery();

                var oraXml = (OracleXmlType) ps[3].Value;
                if (oraXml.IsNull)
                    return null;

                var resInvTask = XmlDocumentConverter.ConvertTo<InvTask>(oraXml.GetXmlDocument());
                return resInvTask;
            });
        }

        public IEnumerable<string> GetPlaceNameLst(decimal invID)
        {
            const string pkg = PkgBpInvName + ".bpGetPlaceNameLst";

            return RunManualDbOperation(db =>
            {
                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("pInvId", OracleDbType.Decimal, invID, ParameterDirection.Input)
                };
                var stm = string.Format("select * from TABLE({0}(:{1}))", pkg, ps[0].ParameterName);
                return db.SetCommand(stm, ps).ExecuteScalarList<string>();
            });
        }

        #endregion . Инвентаризация .

        #region .  СТН  .

        public void InsWtv(decimal? productId, decimal? diff, decimal? comactId, decimal? transact = null,
            IEnumerable<decimal> stnIdLst = null)
        {
            RunManualDbOperation<object>(db =>
            {
                var pProductId = db.InputParameter("pProductId", productId);
                var pDiff = db.InputParameter("pDiff", diff);
                var pComactId = db.InputParameter("pComactId", comactId);
                var pTransact = db.InputParameter("pTransact", transact);
                var pStnIdLst = new OracleParameter("pStnIdLst", OracleDbType.Object, ParameterDirection.Input)
                {
                    UdtTypeName = typeof (TLISTINT).Name,
                    Value = stnIdLst == null || !stnIdLst.Any() ? null : new TLISTINT(stnIdLst.ToArray())
                };

                var stm = string.Format("begin {0}.{1}(:{2},:{3},:{4},:{5},:{6}); end;", PkgBpWtvName, InsWtvName,
                    pProductId.ParameterName, pDiff.ParameterName, pComactId.ParameterName, pTransact.ParameterName,
                    pStnIdLst.ParameterName);
                return db.SetCommand(stm, pProductId, pDiff, pComactId, pTransact, pStnIdLst).ExecuteNonQuery();
            });
        }

        #endregion

        #region . Корректировка единицы измерения .

        // Пересчет ТЕ
        public string ChangeSKUAndRecalculationTE(List<SKU> skuList)
        {
            throw new DeveloperException("Реализация в Manager-е");
        }

        // Пересчет ТЕ
        public void RecalculationTE(SKU sku)
        {
            const string pkg = PkgBp_ApiName + ".bpRecalculationTE";
            var xmlSKU = sku == null ? null : XmlDocumentConverter.ConvertFrom(sku);
            RunManualDbOperation<object>(db =>
            {
                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("pSKU", OracleDbType.XmlType, xmlSKU, ParameterDirection.Input),
                };
                return db.SetCommand(CommandType.StoredProcedure, pkg, ps).ExecuteNonQuery();
            });
        }

        #endregion . Корректировка единицы измерения .

        #region . RCL .

        //Количество товара на ТЕ
        public void GetProductQuantityOnTe(string teCode, decimal skuId, out decimal skuQuantity2Te,
            out decimal skuQuantity2TeMax)
        {
            throw new DeveloperException("Реализация в Manager-е");
        }

        //Осталось принять товара на ТЕ из груза
        //[SprocName(PkgBpInputName + ".bpGetRest2InputByPosInput")]
        //[DiscoverParameters(false)]
        //[ScalarSource(ScalarSourceType.ReturnValue)]
        //public abstract decimal GetTeProductQuantityFromCargoIwbInternal(string operationCode, decimal cargoIwbId, XmlDocument iwbPosInputXml, bool raiseError, out decimal baseSKUCount);
        //При использовании BlToolkit.Extension возникают проблемы - ошибка: арифметическое переполнение. см. http://mp-ts-nwms/issue/wmsMLC-6285.
        //public decimal GetTeProductQuantityFromCargoIwb(string operationCode, decimal cargoIwbId, IWBPosInput posInput, bool raiseError, out decimal baseSKUCount)
        //{
        //    var iwbPosInputXml = XmlDocumentConverter.ConvertFrom(posInput);
        //    var result = GetTeProductQuantityFromCargoIwbInternal(operationCode, cargoIwbId, iwbPosInputXml, raiseError, out baseSKUCount);
        //    return result;
        //}

        /// <summary>
        /// Осталось принять товара на ТЕ из груза.
        /// </summary>
        public decimal GetTeProductQuantityFromCargoIwb(string operationCode, decimal cargoIwbId, IWBPosInput posInput,
            bool raiseError, out decimal baseSkuCount, out decimal productCount)
        {
            //тестирование метода в IWBTest
            var iwbPosInputXml = XmlDocumentConverter.ConvertFrom(posInput);
            var baseSkuCountInternal = decimal.Zero;
            var productCountInternal = decimal.Zero;

            var result = RunManualDbOperation(db =>
            {
                var ps = new IDbDataParameter[]
                {
                    new OracleParameter(string.Empty, OracleDbType.Decimal, 0, ParameterDirection.ReturnValue),
                    new OracleParameter("pOperationCode", OracleDbType.Varchar2, operationCode, ParameterDirection.Input),
                    new OracleParameter("pCargoIWBID", OracleDbType.Int32, cargoIwbId, ParameterDirection.Input),
                    new OracleParameter("pIWBPosInputXml", OracleDbType.XmlType, iwbPosInputXml,
                        ParameterDirection.Input),
                    new OracleParameter("pRaiseError", OracleDbType.Int16, raiseError ? 1 : 0, ParameterDirection.Input),
                    new OracleParameter("pBaseSKUCount", OracleDbType.Decimal, 0, ParameterDirection.Output),
                    new OracleParameter("pProductCount", OracleDbType.Decimal, 0, ParameterDirection.Output)
                };

                db.SetCommand(CommandType.StoredProcedure, PkgBpInputName + ".bpGetRest2InputByPosInput", ps)
                    .ExecuteNonQuery();

                Func<object, decimal> convertToDecimalHandler = value =>
                {
                    if (value == null)
                        return decimal.Zero;

                    var dresult = (decimal) SerializationHelper.ConvertToTrueType(value.ToString(), typeof (decimal));
                    var tresult = decimal.Truncate(dresult);
                    return dresult == tresult ? tresult : dresult;
                };

                baseSkuCountInternal = convertToDecimalHandler(ps[5].Value);
                productCountInternal = convertToDecimalHandler(ps[6].Value);
                return convertToDecimalHandler(ps[0].Value);
            });

            baseSkuCount = baseSkuCountInternal;
            productCount = productCountInternal;
            return result;
        }

        #endregion . RCL .

        #region . System .

        //Получить дату и время
        public DateTime GetSystemDate()
        {
            return RunManualDbOperation(db =>
            {
                var result = db.SetCommand(CommandType.Text, "select sysdate from dual").ExecuteScalar<DateTime>();
                return result;
            });
        }

        [SprocName(PkgBpProcessName + "." + GetSdclEndPointName)]
        [DiscoverParameters(false)]
        [ScalarSource(ScalarSourceType.ReturnValue)]
        public abstract string GetSdclEndPoint(string clientCode, ref string prevServiceCode);

        /// <summary>
        /// Запустить архивирование (по конфигурации)
        /// </summary>
        /// <param name="archCode">код конфигурации архива</param>
        public void ProcessArch(string archCode)
        {
            var sprocName = PkgBpArchiveName + ".jobProcessArchRecord";
            RunManualDbOperation<object>(db =>
            {
                var pArchCode_r = db.InputParameter("pArchCode_r", archCode);
                return db.SetCommand(CommandType.StoredProcedure, sprocName, pArchCode_r).ExecuteNonQuery();
            });
        }

        #endregion System

        #region . Общие .

        [SprocName(PkgBp_ApiName + "." + CheckInstanceEntityName)]
        [DiscoverParameters(false)]
        [ScalarSource(ScalarSourceType.ReturnValue)]
        public abstract int CheckInstanceEntity(string entity, string key);

        #endregion

        #region . Сущности .

        [SprocName(PkgTEName + "." + IsVirtualTEName)]
        [DiscoverParameters(false)]
        [ScalarSource(ScalarSourceType.ReturnValue)]
        public abstract int IsVirtualTE(string teCode, string teTypeCode = null);

        [SprocName(PkgTEName + "." + IsMonoTEName)]
        [DiscoverParameters(false)]
        [ScalarSource(ScalarSourceType.ReturnValue)]
        public abstract bool IsMonoTE(string teCode);

        #endregion

        #region  .  Работа  .

        [SprocName(PkgBpWorkName + "." + GetDateFrDateTillName)]
        [DiscoverParameters(false)]
        public abstract void GetDateFrDateTillInternal(string entity, string key, string operationCode,
            decimal? workerId, out XmlDocument working);

        public void GetDateFrDateTill(string entity, string key, string operationCode, decimal? workerId,
            out Working working)
        {
            working = null;
            XmlDocument xmlWorking;

            GetDateFrDateTillInternal(entity, key, operationCode, workerId, out xmlWorking);

            if (xmlWorking != null)
                working = (Working) XmlDocumentConverter.ConvertTo(typeof (Working), xmlWorking);
        }

        // Завершение работы
        public void WorkComleted(decimal workId, string operation, DateTime? workTill)
        {
            throw new NotImplementedException("Реализуется на уровне Manager-а");
        }

        // Начать работу, создать выполнение
        public void StartWorking(string entity, string key, string operationCode, decimal? workerId, decimal? mandantID,
            DateTime? workingFrom, string workingDoc, out Work work)
        {
            work = null;
            XmlDocument xmlWork = null;
            const string pkg = PkgBpWorkName + ".bpStartWorking";
            RunManualDbOperation(db =>
            {
                var pEntityName = new OracleParameter("pEntityName", OracleDbType.Varchar2, entity,
                    ParameterDirection.Input);
                var pEntityKey = new OracleParameter("pEntityKey", OracleDbType.Varchar2, key, ParameterDirection.Input);
                var pOperationCode = new OracleParameter("pOperationCode", OracleDbType.Varchar2, operationCode,
                    ParameterDirection.Input);
                var pWorkerID = new OracleParameter("pWorkerID", OracleDbType.Decimal, workerId,
                    ParameterDirection.Input);
                var pMandantID = new OracleParameter("pMandantID", OracleDbType.Decimal, mandantID,
                    ParameterDirection.Input);
                var pWorkingFrom = new OracleParameter("pWorkingFrom", OracleDbType.Date, workingFrom,
                    ParameterDirection.Input);
                var pWorkingDoc = new OracleParameter("pWorkingDoc", OracleDbType.Varchar2, key,
                    ParameterDirection.Input);
                var pWork = new OracleParameter("pWork", OracleDbType.XmlType, ParameterDirection.Output);
                db.SetCommand(CommandType.StoredProcedure, pkg, pEntityName, pEntityKey, pOperationCode, pWorkerID,
                    pMandantID, pWorkingFrom, pWorkingDoc, pWork).ExecuteNonQuery();
                var isNull = ((OracleXmlType) pWork.Value).IsNull;
                if (!isNull)
                    xmlWork = ((OracleXmlType) pWork.Value).GetXmlDocument();
                return true;
            });
            if (xmlWork != null)
                work = (Work) XmlDocumentConverter.ConvertTo(typeof (Work), xmlWork);
        }

        // Cоздать выполнения работ
        public void StartWorkings(decimal workId, string truckCode, decimal myWorkerId, IEnumerable<decimal> workerIds,
            DateTime? workingFrom)
        {
            throw new NotImplementedException("Реализуется на уровне Manager-а");
        }

        // Завершить выполнения работ
        public void CompleteWorking(string entity, string key, string operationCode, decimal? workerId)
        {
            const string pkg = PkgBpWorkName + ".bpCompleteWorking";
            RunManualDbOperation(db =>
            {
                var pEntityName = new OracleParameter("pEntityName", OracleDbType.Varchar2, entity,
                    ParameterDirection.Input);
                var pEntityKey = new OracleParameter("pEntityKey", OracleDbType.Varchar2, key, ParameterDirection.Input);
                var pOperationCode = new OracleParameter("pOperationCode", OracleDbType.Varchar2, operationCode,
                    ParameterDirection.Input);
                var pWorkerID = new OracleParameter("pWorkerID", OracleDbType.Decimal, workerId,
                    ParameterDirection.Input);
                return
                    db.SetCommand(CommandType.StoredProcedure, pkg, pEntityName, pEntityKey, pOperationCode, pWorkerID)
                        .ExecuteNonQuery();
            });
        }

        // Завершить все выполнения работ данного работника
        public void CompleteWorkings(IEnumerable<decimal> workingIds, DateTime? dateTill)
        {
            throw new NotImplementedException("Реализуется на уровне Manager-а");
        }

        // Сменить статус работы
        public void ChangeWorkStatus(decimal workId, string operation)
        {
            const string pkg = "PkgWork.chngWorkStatus";
            RunManualDbOperation(db =>
            {
                var pKey = new OracleParameter("pKey", OracleDbType.Decimal, workId, ParameterDirection.Input);
                var pOperationCode = new OracleParameter("pOperationCode", OracleDbType.Varchar2, operation,
                    ParameterDirection.Input);
                return db.SetCommand(CommandType.StoredProcedure, pkg, pKey, pOperationCode).ExecuteNonQuery();
            });
        }

        public Work GetWorkByOperation(string entity, string key, string operationCode)
        {
            const string pkg = PkgBpWorkName + ".bpGetWorkByOperation";
            return RunManualDbOperation(db =>
            {
                var pEntityName = new OracleParameter("pEntity", OracleDbType.Varchar2, entity, ParameterDirection.Input);
                var pEntityKey = new OracleParameter("pKey", OracleDbType.Varchar2, key, ParameterDirection.Input);
                var pOperationCode = new OracleParameter("pOperationCode", OracleDbType.Varchar2, operationCode,
                    ParameterDirection.Input);
                var pWork = new OracleParameter("pWork", OracleDbType.XmlType, ParameterDirection.Output);
                db.SetCommand(CommandType.StoredProcedure, pkg, pEntityName, pEntityKey, pOperationCode, pWork)
                    .ExecuteNonQuery();
                var orawork = (OracleXmlType) pWork.Value;
                return orawork.IsNull
                    ? null
                    : (Work) XmlDocumentConverter.ConvertTo(typeof (Work), orawork.GetXmlDocument());
            });
        }

        #endregion  .  Работа  .

        #region  .  Мандант  .

        public int? ChkJob(string jobName, string mandantCode)
        {
            const string pkg = "pkgBpMandant" + ".bpChkJob";
            return RunManualDbOperation(db =>
            {
                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("pJobName", OracleDbType.Varchar2, jobName, ParameterDirection.Input),
                    new OracleParameter("pMandantCode", OracleDbType.Varchar2, mandantCode, ParameterDirection.Input),
                };
                var cmd = string.Format("select {0}(:{1}, :{2}) from dual", pkg, ps[0].ParameterName,
                    ps[1].ParameterName);
                var val = db.SetCommand(cmd, ps).ExecuteScalar();

                if (val is DBNull)
                    return null;

                return (int?) SerializationHelper.ConvertToTrueType(val, typeof (int?));
            });
        }

        [SprocName("pkgBpMandant" + ".bpCreJob")]
        [DiscoverParameters(false)]
        public abstract void CreJob(string jobName, string mandantCode, int interval);

        #endregion

        #region . CPV .

        public object GetCpvValue(string entity, string key, string cpvcode)
        {
            const string funcname = PkgCustomParamValueName + ".getCPVValue";

            return RunManualDbOperation(db =>
            {
                var ps = db.GetSpParameters(funcname, false, false);
                ps[0].Value = entity;
                ps[1].Value = key;
                ps[2].Value = cpvcode;
                var cmd = string.Format("select {0}(:{1}, :{2}, :{3}) from dual", funcname, ps[0].ParameterName,
                    ps[1].ParameterName, ps[2].ParameterName);
                var val = db.SetCommand(cmd, ps).ExecuteScalar();
                return val;
            });
        }

        public void SaveTirCpvs(IEnumerable<CustomParamValue> cpvs, IEnumerable<decimal> mandantids)
        {
            throw new NotImplementedException("Должен реализовываться в BpManagerOracle.");
        }

        public void DeleteTirCpvs(IEnumerable<string> iwbids)
        {
            throw new NotImplementedException("Должен реализовываться в BpManagerOracle.");
        }

        public void DeleteCpvsByEntityByCodeByKey(string entity, IEnumerable<string> codes, IEnumerable<string> keys)
        {
            throw new NotImplementedException("Должен реализовываться в BpManagerOracle.");
        }
        #endregion . CPV .

        #endregion . DB API .

        #region . Configurator .
        //Получение данных для Configurator'а
        public void GetPmConfiguratorData(ref IEnumerable<BillOperation> operations, ref IEnumerable<decimal> entityids,
            ref IEnumerable<SysObject> attributes,
            ref IEnumerable<PM> pms, ref IEnumerable<PMMethod> pmMethods,
            ref IEnumerable<PMMethod2Operation> detailsPmMethod, ref DataTable pmdata,
            ref DataTable pmMethod2OperationsAllowed)
        {
            throw new NotImplementedException("Должен реализовываться в BpManagerOracle.");
        }

        public List<string> SavePmConfiguratorData(ICollection<PM2Operation> pm2Operations,
            ICollection<PMConfig> pmConfigs, ICollection<PM2Operation> deletePm2Operations,
            ICollection<PMConfig> deletePmConfigs)
        {
            throw new NotImplementedException("Должен реализовываться в BpManagerOracle.");
        }

        public void DeletePmConfiguratorData(ICollection<PM2Operation> pm2Operations,
            ICollection<PMConfig> pmConfigs)
        {
            throw new NotImplementedException("Должен реализовываться в BpManagerOracle.");
        }
        #endregion . Configurator .

        public virtual DataTable ExecuteDataTable(string query)
        {
            var result = RunManualDbOperation(db => db.SetCommand(query).ExecuteDataTable());
            if (result != null)
                result.TableName = "res";
            return result;
        }

        #region . OracleParamsHelper .

        //public static string GetParamsString(IDbDataParameter[] parameters)
        //{
        //    var result = new List<string>();
        //    foreach (var p in parameters)
        //    {
        //        if (p.Direction == ParameterDirection.ReturnValue)
        //            continue;
        //        var op = p as OracleParameter;
        //        if (op != null && op.CollectionType == OracleCollectionType.PLSQLAssociativeArray &&
        //            op.OracleDbType == OracleDbType.XmlType && op.Direction != ParameterDirection.Output)
        //        {
        //            result.Add(op.Value != null ? string.Format("TListXml(:{0})", p.ParameterName) : "NULL");
        //            continue;
        //        }
        //        result.Add(string.Format(":{0}", p.ParameterName));
        //    }
        //    return string.Join(",", result);
        //}

        public static string GetParamsString(params IDbDataParameter[] parameters)
        {
            var result = new List<string>();
            foreach (var p in parameters)
            {
                if (p.Direction == ParameterDirection.ReturnValue)
                    continue;
                var op = p as OracleParameter;
                if (op != null && op.CollectionType == OracleCollectionType.PLSQLAssociativeArray &&
                    op.OracleDbType == OracleDbType.XmlType && op.Direction != ParameterDirection.Output)
                {
                    result.Add(op.Value != null ? string.Format("TListXml(:{0})", p.ParameterName) : "NULL");
                    continue;
                }
                result.Add(string.Format(":{0}", p.ParameterName));
            }
            return string.Join(",", result);
        }

        #endregion

        [Cache]
        public virtual List<SKU> GetSKUWithCache(string filter, string attrEntity)
        {
            using (var mgr = IoC.Instance.Resolve<IBaseManager<SKU>>())
                return mgr.GetFiltered(filter, attrEntity).ToList();
        }
    }
}