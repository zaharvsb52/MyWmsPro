using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BLToolkit.Aspects;
using wmsMLC.Business.Objects;
using wmsMLC.Business.Objects.Processes;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.DAL.Service;
using wmsMLC.General.DAL.Service.Telegrams;

namespace wmsMLC.Business.DAL.Service
{
    public abstract class BPProcessRepository : BaseHistoryCacheableRepository<BPProcess, string>, IBPProcessRepository
    {

        #region  . Fields&Consts .
        private const string pkgBpMove = "pkgBpMove";
        #endregion

        public void Run(ExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public override BPProcess Get(string key, string attrentity)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            // получаем процессы из кэшированной коллекции
            return GetAll(attrentity).FirstOrDefault(i => key.EqIgnoreCase(i.PROCESSCODE));
        }

        public DataTable ExecuteDataTable(string query)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(DataTable), IsOut = true };
            var signatureParam = new TransmitterParam { Name = "query", Type = typeof(string), IsOut = false, Value = query };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "ExecuteDataTable", new[] { resultParam, signatureParam });
            ProcessTelegramm(telegram);
            return (DataTable)resultParam.Value;
        }

        #region .  API DB  .

        #region . ВРЕМЕННЫЕ .

        // БП "Подобрано"
        public void OWBPicked(decimal owbid, string placecode, decimal notSamePlaceType)
        {
            var owbidParam = new TransmitterParam { Name = "owbid", Type = typeof(decimal), IsOut = false, Value = owbid };
            var placecodeParam = new TransmitterParam { Name = "placecode", Type = typeof(string), IsOut = false, Value = placecode };
            var notSamePlaceTypeParam = new TransmitterParam { Name = "notSamePlaceType", Type = typeof(decimal), IsOut = false, Value = notSamePlaceType };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "OWBPicked", new[] { owbidParam, placecodeParam, notSamePlaceTypeParam });
            ProcessTelegramm(telegram);
        }

        // Статус упаковки расходной накладной
        public void GetOWBBPStatus(decimal key, out string status)
        {
            status = string.Empty;
            var keyParam = new TransmitterParam { Name = "key", Type = typeof(decimal), IsOut = false, Value = key };
            var statusParam = new TransmitterParam { Name = "status", Type = typeof(string), IsOut = true, Value = status };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "GetOWBBPStatus", new[] { keyParam, statusParam });
            ProcessTelegramm(telegram);
            status = statusParam.Value.ToString();
        }

        #endregion

        #region . Приемка .

        public void ActivateIWB(decimal key)
        {
            var keyParam = new TransmitterParam { Name = "key", Type = typeof(decimal), IsOut = false, Value = key };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "ActivateIWB", new[] { keyParam });
            ProcessTelegramm(telegram);
        }

        public List<Product> CreateProductByPos(ref string manageFlag, ref string manageFlagParam, string operationCode, decimal iwbId, IWBPosInput posInput, int isMigrating = 0, string placeCode = null)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(List<Product>), IsOut = true };
            var pManageFlag = new TransmitterParam { Name = "manageFlag", Type = typeof(string), IsOut = true, Value = manageFlag };
            var pManageFlagParam = new TransmitterParam { Name = "manageFlagParam", Type = typeof(string), IsOut = true, Value = manageFlagParam };
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var iwbIdParam = new TransmitterParam { Name = "iwbId", Type = typeof(decimal), IsOut = false, Value = iwbId };
            var posInputParam = new TransmitterParam { Name = "posInput", Type = typeof(IWBPosInput), IsOut = false, Value = posInput };
            var isMigratingParam = new TransmitterParam { Name = "isMigrating", Type = typeof(int), IsOut = false, Value = isMigrating };
            var placeCodeParam = new TransmitterParam { Name = "placeCode", Type = typeof(string), IsOut = false, Value = placeCode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CreateProductByPos", new[]
            {
                resultParam, pManageFlag, pManageFlagParam, operationCodeParam, iwbIdParam, posInputParam, isMigratingParam, placeCodeParam
            });
            ProcessTelegramm(telegram);
            manageFlag = (string)SerializationHelper.ConvertToTrueType(pManageFlag.Value, typeof(string));
            manageFlagParam = (string)SerializationHelper.ConvertToTrueType(pManageFlagParam.Value, typeof(string));
            return (List<Product>)resultParam.Value;
        }

        public List<Product> CreateProductByCargoIwb(ref string manageFlag, ref string manageFlagParam, string operationCode, IWBPosInput posInput, string placeCode, decimal cargoIwbId, int isMigrating = 0, decimal? iwbId = null)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(List<Product>), IsOut = true };
            var pManageFlag = new TransmitterParam { Name = "manageFlag", Type = typeof(string), IsOut = true, Value = manageFlag };
            var pManageFlagParam = new TransmitterParam { Name = "manageFlagParam", Type = typeof(string), IsOut = true, Value = manageFlagParam };
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var posInputParam = new TransmitterParam { Name = "posInput", Type = typeof(IWBPosInput), IsOut = false, Value = posInput };
            var placeCodeParam = new TransmitterParam { Name = "placeCode", Type = typeof(string), IsOut = false, Value = placeCode };
            var cargoIwbIdParam = new TransmitterParam { Name = "cargoIwbId", Type = typeof(decimal), IsOut = false, Value = cargoIwbId };
            var isMigratingParam = new TransmitterParam { Name = "isMigrating", Type = typeof(int), IsOut = false, Value = isMigrating };
            var iwbIdParam = new TransmitterParam { Name = "iwbId", Type = typeof(decimal?), IsOut = false, Value = iwbId };

            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CreateProductByCargoIwb", new[]
            {
                resultParam, pManageFlag, pManageFlagParam, operationCodeParam, posInputParam, placeCodeParam, cargoIwbIdParam, isMigratingParam, iwbIdParam
            });
            ProcessTelegramm(telegram);
            manageFlag = (string)SerializationHelper.ConvertToTrueType(pManageFlag.Value, typeof(string));
            manageFlagParam = (string)SerializationHelper.ConvertToTrueType(pManageFlagParam.Value, typeof(string));
            return (List<Product>)resultParam.Value;
        }

        // Отмена приемки
        public void CancelIwbAccept(decimal iwbid)
        {
            var iwbidParam = new TransmitterParam { Name = "iwbid", Type = typeof(decimal), IsOut = false, Value = iwbid };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CancelIwbAccept", new[] { iwbidParam });
            ProcessTelegramm(telegram);
        }

        // Частичная отмена приемки
        public void CancelProductAccept(decimal? iwbid, IEnumerable<decimal> products, bool isAllTe, decimal? workid)
        {
            var iwbidParam = new TransmitterParam { Name = "iwbid", Type = typeof(decimal?), IsOut = false, Value = iwbid };
            var productsParam = new TransmitterParam { Name = "products", Type = typeof(IEnumerable<decimal>), IsOut = false, Value = products };
            var isAllTeParam = new TransmitterParam { Name = "isAllTe", Type = typeof(bool), IsOut = false, Value = isAllTe };
            var workidParam = new TransmitterParam { Name = "workid", Type = typeof(decimal?), IsOut = false, Value = workid };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CancelProductAccept", new[] { iwbidParam, productsParam, isAllTeParam, workidParam });
            ProcessTelegramm(telegram);
        }

        // Закрыть приход
        public void FinishIwb(IEnumerable<decimal> iwbs, string operationCode, decimal? comactId)
        {
            var iwbsParam = new TransmitterParam { Name = "iwbs", Type = typeof(IEnumerable<decimal>), IsOut = false, Value = iwbs };
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var comactIdParam = new TransmitterParam { Name = "comactId", Type = typeof(decimal?), IsOut = false, Value = comactId };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "FinishIwb", new[] { iwbsParam, operationCodeParam, comactIdParam });
            ProcessTelegramm(telegram);
        }

        // Создать виртуальные позиции
        public List<IWBPosInput> GetIWBPosInputLst(IEnumerable<IWBPos> iwbPosList, int isMigrating = 0)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(List<IWBPosInput>), IsOut = true };
            var iwbPosListParam = new TransmitterParam { Name = "iwbPosList", Type = typeof(IEnumerable<IWBPos>), IsOut = false, Value = iwbPosList };
            var isMigratingParam = new TransmitterParam { Name = "isMigrating", Type = typeof(int), IsOut = false, Value = isMigrating };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "GetIWBPosInputLst", new[] { resultParam, iwbPosListParam, isMigratingParam });
            ProcessTelegramm(telegram);
            return (List<IWBPosInput>)resultParam.Value;
        }

        //Расфиксировать
        public void UnfixedIWB(decimal iwbid, string operationcode)
        {
            var iwbidParam = new TransmitterParam { Name = "iwbid", Type = typeof(decimal), IsOut = false, Value = iwbid };
            var operationcodeParam = new TransmitterParam { Name = "operationcode", Type = typeof(decimal), IsOut = false, Value = operationcode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "UnfixedIWB", new[] { iwbidParam, operationcodeParam });
            ProcessTelegramm(telegram);
        }

        //Получить коэффициент пересчета SKU
        public decimal ConvertSKUtoSKU(decimal sourceSkuId, decimal destSkuId, int isPrd, decimal? oldqty)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(decimal), IsOut = true };
            var sourceSkuIdParam = new TransmitterParam { Name = "sourceSkuId", Type = typeof(decimal), IsOut = false, Value = sourceSkuId };
            var destSkuIdParam = new TransmitterParam { Name = "destSkuId", Type = typeof(decimal), IsOut = false, Value = destSkuId };
            var isPrdParam = new TransmitterParam { Name = "isPrd", Type = typeof(int), IsOut = false, Value = isPrd };
            var oldqtyParam = new TransmitterParam { Name = "oldqty", Type = typeof(decimal?), IsOut = false, Value = oldqty };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "ConvertSKUtoSKU", new[] { resultParam, sourceSkuIdParam, destSkuIdParam, isPrdParam, oldqtyParam });
            ProcessTelegramm(telegram);
            var result = (decimal)SerializationHelper.ConvertToTrueType(resultParam.Value, typeof(decimal));
            return result;
        }

        public BPBatch GetDefaultBatchCode(decimal? mandantID, decimal? sKUID, string artCode)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(BPBatch), IsOut = true };
            var mandantIDParam = new TransmitterParam { Name = "mandantID", Type = typeof(decimal?), IsOut = false, Value = mandantID };
            var sKUIDParam = new TransmitterParam { Name = "sKUID", Type = typeof(decimal?), IsOut = false, Value = sKUID };
            var artCodeParam = new TransmitterParam { Name = "artCode", Type = typeof(string), IsOut = false, Value = artCode };

            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "GetDefaultBatchCode", new[] { resultParam, mandantIDParam, sKUIDParam, artCodeParam });
            ProcessTelegramm(telegram);
            var result = (BPBatch)SerializationHelper.ConvertToTrueType(resultParam.Value, typeof(BPBatch));
            return result;
        }

        public List<String> GetLastProductAttr(decimal skuId)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(List<String>), IsOut = true };
            var skuParam = new TransmitterParam { Name = "skuId", Type = typeof(decimal), IsOut = false, Value = skuId };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "GetLastProductAttr", new[] { resultParam, skuParam });
            ProcessTelegramm(telegram);
            return (List<String>)resultParam.Value;
        }

        public IEnumerable<decimal> GetMinConfig4IwbList(IEnumerable<decimal> iwbIdLst, string cpCode, string cpValue)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(IEnumerable<decimal>), IsOut = true };
            var iwbIdLstParam = new TransmitterParam { Name = "iwbIdLst", Type = typeof(IEnumerable<decimal>), IsOut = false, Value = iwbIdLst };
            var cpCodeParam = new TransmitterParam { Name = "cpCode", Type = typeof(string), IsOut = false, Value = cpCode };
            var cpValueParam = new TransmitterParam { Name = "cpValue", Type = typeof(string), IsOut = false, Value = cpValue };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "GetMinConfig4IwbList", new[] { resultParam, iwbIdLstParam, cpCodeParam, cpValueParam });
            ProcessTelegramm(telegram);
            return (IEnumerable<decimal>)resultParam.Value;
        }

        public decimal? GetDefaultMIN(decimal iwbId)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(decimal?), IsOut = true };
            var iwbIdParam = new TransmitterParam { Name = "iwbId", Type = typeof(decimal), IsOut = false, Value = iwbId };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "GetDefaultMIN", new[] { resultParam, iwbIdParam });
            ProcessTelegramm(telegram);
            return (decimal?)resultParam.Value;
        }

        public void RemoveStorageUnit(IEnumerable<decimal> productIdList)
        {
            var productIdListParam = new TransmitterParam { Name = "productIdList", Type = typeof(IEnumerable<decimal>), IsOut = false, Value = productIdList };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "RemoveStorageUnit", new[] { productIdListParam });
            ProcessTelegramm(telegram);
        }

        public void CascadeDeleteIWB(decimal iwbId)
        {
            var iwbIdParam = new TransmitterParam { Name = "iwbId", Type = typeof(decimal), IsOut = false, Value = iwbId };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CascadeDeleteIWB", new[] { iwbIdParam });
            ProcessTelegramm(telegram);
        }

        #endregion

        #region . Перемещение .

        public void BlockArea(string key, string blockingCode, string description, out decimal blockingID)
        {
            blockingID = 0;
            var BlockingIDParam = new TransmitterParam { Name = "blockingID", Type = typeof(decimal), IsOut = true, Value = blockingID };
            var keyParam = new TransmitterParam { Name = "key", Type = typeof(string), IsOut = false, Value = key };
            var BlockingCodeParam = new TransmitterParam { Name = "blockingCode", Type = typeof(string), IsOut = false, Value = blockingCode };
            var DescriptionParam = new TransmitterParam { Name = "description", Type = typeof(string), IsOut = false, Value = description };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "BlockArea", new[] { BlockingIDParam, keyParam, BlockingCodeParam, DescriptionParam });
            ProcessTelegramm(telegram);
            blockingID = Convert.ToInt32(BlockingIDParam.Value);
        }

        public void BlockTE(string key, string blockingCode, string description, out decimal blockingID)
        {
            blockingID = 0;
            var BlockingIDParam = new TransmitterParam { Name = "blockingID", Type = typeof(decimal), IsOut = true, Value = blockingID };
            var keyParam = new TransmitterParam { Name = "key", Type = typeof(string), IsOut = false, Value = key };
            var BlockingCodeParam = new TransmitterParam { Name = "blockingCode", Type = typeof(string), IsOut = false, Value = blockingCode };
            var DescriptionParam = new TransmitterParam { Name = "description", Type = typeof(string), IsOut = false, Value = description };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "BlockTE", new[] { BlockingIDParam, keyParam, BlockingCodeParam, DescriptionParam });
            ProcessTelegramm(telegram);
            blockingID = Convert.ToInt32(BlockingIDParam.Value);
        }

        public void BlockPlace(string key, string blockingCode, string description, out decimal blockingID)
        {
            blockingID = 0;
            var BlockingIDParam = new TransmitterParam { Name = "blockingID", Type = typeof(decimal), IsOut = true, Value = blockingID };
            var keyParam = new TransmitterParam { Name = "key", Type = typeof(string), IsOut = false, Value = key };
            var BlockingCodeParam = new TransmitterParam { Name = "blockingCode", Type = typeof(string), IsOut = false, Value = blockingCode };
            var DescriptionParam = new TransmitterParam { Name = "description", Type = typeof(string), IsOut = false, Value = description };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "BlockPlace", new[] { BlockingIDParam, keyParam, BlockingCodeParam, DescriptionParam });
            ProcessTelegramm(telegram);
            blockingID = Convert.ToInt32(BlockingIDParam.Value);
        }

        public void BlockSegment(string key, string blockingCode, string description, out decimal blockingID)
        {
            blockingID = 0;
            var BlockingIDParam = new TransmitterParam { Name = "blockingID", Type = typeof(decimal), IsOut = true, Value = blockingID };
            var keyParam = new TransmitterParam { Name = "key", Type = typeof(string), IsOut = false, Value = key };
            var BlockingCodeParam = new TransmitterParam { Name = "blockingCode", Type = typeof(string), IsOut = false, Value = blockingCode };
            var DescriptionParam = new TransmitterParam { Name = "description", Type = typeof(string), IsOut = false, Value = description };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "BlockSegment", new[] { BlockingIDParam, keyParam, BlockingCodeParam, DescriptionParam });
            ProcessTelegramm(telegram);
            blockingID = Convert.ToInt32(BlockingIDParam.Value);
        }

        public List<Place> GetPlaceLstByStrategy(string strategy, string sourceTECode)
        {
            ClearStatistics();
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(List<Place>), IsOut = true };
            var strategyParam = new TransmitterParam { Name = "strategy", Type = typeof(string), Value = strategy };
            var sourceTECodeParam = new TransmitterParam { Name = "sourceTECode", Type = typeof(string), Value = sourceTECode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "GetPlaceLstByStrategy", new[] { resultParam, strategyParam, sourceTECodeParam });
            ProcessTelegramm(telegram);
            return (List<Place>)resultParam.Value;
        }

        public void CreateTransportTask(string sourceTECode, string destPlaceCode, string strategy, string destTECode, out decimal transportTaskID, decimal? productID)
        {
            transportTaskID = 0;
            var transportTaskIDParam = new TransmitterParam { Name = "transportTaskID", Type = typeof(decimal), IsOut = true, Value = transportTaskID };
            var sourceTECodeParam = new TransmitterParam { Name = "sourceTECode", Type = typeof(string), IsOut = false, Value = sourceTECode };
            var destPlaceCodeParam = new TransmitterParam { Name = "destPlaceCode", Type = typeof(string), IsOut = false, Value = destPlaceCode };
            var strategyParam = new TransmitterParam { Name = "strategy", Type = typeof(string), IsOut = false, Value = strategy };
            var destTECodeParam = new TransmitterParam { Name = "destTECode", Type = typeof(string), IsOut = false, Value = destTECode };
            var prodidParam = new TransmitterParam { Name = "productID", Type = typeof(decimal?), IsOut = false, Value = productID };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CreateTransportTask", new[] { transportTaskIDParam, sourceTECodeParam, destPlaceCodeParam, strategyParam, destTECodeParam, prodidParam });
            ProcessTelegramm(telegram);
            transportTaskID = Convert.ToInt32(transportTaskIDParam.Value);
        }

        // создание нескольких ЗНТ
        public List<TransportTask> MoveManyTE2Place(IEnumerable<TE> teList, string destPlaceCode, string strategy, string destTECode, IEnumerable<Product> productList, int isManual)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(List<TransportTask>), IsOut = true };
            var teListParam = new TransmitterParam { Name = "teList", Type = typeof(IEnumerable<TE>), IsOut = false, Value = teList };
            var destPlaceCodeParam = new TransmitterParam { Name = "destPlaceCode", Type = typeof(string), IsOut = false, Value = destPlaceCode };
            var strategyParam = new TransmitterParam { Name = "strategy", Type = typeof(string), IsOut = false, Value = strategy };
            var destTECodeParam = new TransmitterParam { Name = "destTECode", Type = typeof(string), IsOut = false, Value = destTECode };
            var productListParam = new TransmitterParam { Name = "productList", Type = typeof(IEnumerable<Product>), IsOut = false, Value = productList };
            var isManualParam = new TransmitterParam { Name = "isManual", Type = typeof(int), IsOut = false, Value = isManual };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "MoveManyTE2Place", new[] { resultParam, teListParam, destPlaceCodeParam, strategyParam, destTECodeParam, productListParam, isManualParam });
            ProcessTelegramm(telegram);
            return (List<TransportTask>)resultParam.Value;
        }

        public void BpGetPlaceByMM(string MMCode, string SourceTeCode, ref string MMStrategy, string OrderClause, out string PlaceCode)
        {
            PlaceCode = string.Empty;
            MMStrategy = string.Empty;

            var pMmCodeParam = new TransmitterParam { Name = "MMCode", Type = typeof(string), IsOut = false, Value = MMCode };
            var pSourceTeCodeParam = new TransmitterParam { Name = "SourceTeCode", Type = typeof(string), IsOut = false, Value = SourceTeCode };
            var pMMStrategyParam = new TransmitterParam { Name = "MMStrategy", Type = typeof(string), IsOut = true, Value = MMStrategy };
            var pOrderClauseParam = new TransmitterParam { Name = "OrderClause", Type = typeof(string), IsOut = false, Value = OrderClause };
            var pPlaceCodeParam = new TransmitterParam { Name = "PlaceCode", Type = typeof(string), IsOut = true, Value = PlaceCode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "BpGetPlaceByMM", new[] { pMmCodeParam, pSourceTeCodeParam, pMMStrategyParam, pOrderClauseParam, pPlaceCodeParam });
            ProcessTelegramm(telegram);

            PlaceCode = pPlaceCodeParam.Value.ToString();
            MMStrategy = pMMStrategyParam.Value.ToString();
        }

        public void CompleteTransportTask(IEnumerable<decimal> tTaskIDLst, decimal? workerCode, string truckCode, DateTime? startDate, DateTime? endDate, string teTypeCode, IEnumerable<string> teCodeLst, bool isNotNeededCreateWork)
        {
            var tTaskIDLstParam = new TransmitterParam { Name = "tTaskIDLst", Type = typeof(IEnumerable<decimal>), IsOut = false, Value = tTaskIDLst };
            var workerCodeParam = new TransmitterParam { Name = "workerCode", Type = typeof(decimal?), IsOut = false, Value = workerCode };
            var truckCodeParam = new TransmitterParam { Name = "truckCode", Type = typeof(string), IsOut = false, Value = truckCode };
            var startDateParam = new TransmitterParam { Name = "startDate", Type = typeof(DateTime?), IsOut = false, Value = startDate };
            var endDateParam = new TransmitterParam { Name = "endDate", Type = typeof(DateTime?), IsOut = false, Value = endDate };
            var teTypeCodeParam = new TransmitterParam { Name = "teTypeCode", Type = typeof(string), IsOut = false, Value = teTypeCode };
            var teCodeLstParam = new TransmitterParam { Name = "teCodeLst", Type = typeof(IEnumerable<string>), IsOut = false, Value = teCodeLst };
            var isNotNeededCreateWorkParam = new TransmitterParam { Name = "isNotNeededCreateWork", Type = typeof(bool), IsOut = false, Value = isNotNeededCreateWork };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CompleteTransportTask", new[] { tTaskIDLstParam, workerCodeParam, truckCodeParam, startDateParam, endDateParam, teTypeCodeParam, teCodeLstParam, isNotNeededCreateWorkParam });
            ProcessTelegramm(telegram);
        }

        // Переехало в WmsAPI
        //Получить количество доступных ЗНТ
        //public void GetAvailableTransportTaskCount(string filter, out int count)
        //{
        //    count = 0;
        //    var filterParam = new TransmitterParam { Name = "filter", Type = typeof(string), IsOut = false, Value = filter };
        //    var countParam = new TransmitterParam { Name = "count", Type = typeof(int), IsOut = true, Value = count };
        //    var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "GetAvailableTransportTaskCount", new[] { filterParam, countParam });
        //    ProcessTelegramm(telegram);
        //    count = countParam.Value.To(0);
        //}

        //Создать виртуальные ТЕ
        public void CreateVirtualTE(IEnumerable<string> places, string teTypeCode)
        {
            var placesParam = new TransmitterParam { Name = "places", Type = typeof(IEnumerable<string>), IsOut = false, Value = places };
            var teTypeCodeParam = new TransmitterParam { Name = "teTypeCode", Type = typeof(string), IsOut = false, Value = teTypeCode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CreateVirtualTE", new[] { placesParam, teTypeCodeParam });
            ProcessTelegramm(telegram);
        }

        //Зарезервировать ЗНТ
        public TransportTask ReserveTransportTask(decimal? currentTransportTaskCode, string filter)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(TransportTask), IsOut = true };
            var currentTransportTaskCodeParam = new TransmitterParam { Name = "currentTransportTaskCode", Type = typeof(decimal?), IsOut = false, Value = currentTransportTaskCode };
            var filterParam = new TransmitterParam { Name = "filter", Type = typeof(string), IsOut = false, Value = filter };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "ReserveTransportTask", new[] { resultParam, currentTransportTaskCodeParam, filterParam });
            ProcessTelegramm(telegram);
            return (TransportTask)resultParam.Value;
        }

        //Освободить зарезервированную ЗНТ
        public void ResetTransportTask(decimal currentTransportTaskCode)
        {
            var currentTransportTaskCodeParam = new TransmitterParam { Name = "currentTransportTaskCode", Type = typeof(decimal), IsOut = false, Value = currentTransportTaskCode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "ResetTransportTask", new[] { currentTransportTaskCodeParam });
            ProcessTelegramm(telegram);
        }

        //Активировать или резервировать ЗНТ
        public void ReserveOrActivateTransportTask(decimal tTaskId, string clientCode, string truckCode, DateTime? taskBegin, BillOperationCode operationCode)
        {
            var tTaskIdParam = new TransmitterParam { Name = "tTaskId", Type = typeof(decimal), IsOut = false, Value = tTaskId };
            var clientCodeParam = new TransmitterParam { Name = "clientCode", Type = typeof(string), IsOut = false, Value = clientCode };
            var truckCodeParam = new TransmitterParam { Name = "truckCode", Type = typeof(string), IsOut = false, Value = truckCode };
            var taskBeginParam = new TransmitterParam { Name = "taskBegin", Type = typeof(DateTime?), IsOut = false, Value = taskBegin };
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(BillOperationCode), IsOut = false, Value = operationCode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "ReserveOrActivateTransportTask", new[] { tTaskIdParam, clientCodeParam, truckCodeParam, taskBeginParam, operationCodeParam });
            ProcessTelegramm(telegram);
        }

        // Отменить ЗНТ
        public void CancelTransportTask(decimal Key, string operationCode)
        {
            var keyParam = new TransmitterParam { Name = "Key", Type = typeof(decimal), IsOut = false, Value = Key };
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CancelTransportTask", new[] { keyParam, operationCodeParam });
            ProcessTelegramm(telegram);
        }

        // Получение стратегии перемещения
        public void GetPlaceStrategy(decimal productId, string placeCode, decimal raiseError, out decimal strategy)
        {
            strategy = 0;
            var productIdParam = new TransmitterParam { Name = "productId", Type = typeof(decimal), IsOut = false, Value = productId };
            var placeCodeParam = new TransmitterParam { Name = "placeCode", Type = typeof(string), IsOut = false, Value = placeCode };
            var raiseErrorParam = new TransmitterParam { Name = "raiseError", Type = typeof(decimal), IsOut = false, Value = raiseError };
            var strategyparam = new TransmitterParam { Name = "strategy", Type = typeof(decimal), IsOut = true, Value = strategy };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "GetPlaceStrategy", new[] { productIdParam, placeCodeParam, raiseErrorParam, strategyparam });
            ProcessTelegramm(telegram);
            strategy = strategyparam.Value.To(0);
        }

        public string FindTargetTE(string sourceTECode, string destPlaceCode, decimal? productId, decimal? raiseError, string destTECode)
        {

            productId = productId == 0 ? null : productId;

            var resultParam = new TransmitterParam { Name = "result", Type = typeof(string), IsOut = true };
            var sourceTECodeParam = new TransmitterParam { Name = "sourceTECode", Type = typeof(string), Value = sourceTECode };
            var destPlaceCodeParam = new TransmitterParam { Name = "destPlaceCode", Type = typeof(string), Value = destPlaceCode };
            var productIdParam = new TransmitterParam { Name = "productId", Type = typeof(decimal?), Value = productId };
            var raiseErrorParam = new TransmitterParam { Name = "raiseError", Type = typeof(decimal?), Value = raiseError };
            var destTECodeParam = new TransmitterParam { Name = "destTECode", Type = typeof(string), Value = destTECode };

            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "FindTargetTE", new[] { resultParam, sourceTECodeParam, destPlaceCodeParam, productIdParam, raiseErrorParam, destTECodeParam });
            ProcessTelegramm(telegram);
            return (string)resultParam.Value;
        }

        public string FindCarrierTE(string sourceTECode, string destPlaceCode, string strategy, decimal? productId, decimal checkActiveTt, string destTECode)
        {
            productId = productId == 0 ? null : productId;

            var resultParam = new TransmitterParam { Name = "result", Type = typeof(string), IsOut = true };
            var sourceTECodeParam = new TransmitterParam { Name = "sourceTECode", Type = typeof(string), Value = sourceTECode };
            var destPlaceCodeParam = new TransmitterParam { Name = "destPlaceCode", Type = typeof(string), Value = destPlaceCode };
            var strategyParam = new TransmitterParam { Name = "strategy", Type = typeof(string), Value = strategy };
            var productIdParam = new TransmitterParam { Name = "productId", Type = typeof(decimal?), Value = productId };
            var checkActiveParam = new TransmitterParam { Name = "checkActiveTt", Type = typeof(decimal?), Value = checkActiveTt };
            var destTECodeParam = new TransmitterParam { Name = "destTECode", Type = typeof(string), Value = destTECode };


            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "FindCarrierTE", new[] { resultParam, sourceTECodeParam, destPlaceCodeParam,strategyParam, productIdParam, checkActiveParam, destTECodeParam });
            ProcessTelegramm(telegram);
            return (string)resultParam.Value;
        }

        public void MoveProductsBySku(string sourceTeCode, string destTeCode, decimal skuId, IEnumerable<decimal> productIds, decimal count, string truckCode, bool isNotNeededCreateWork, out decimal transportTaskID)
        {
            var sourceTECodeParam = new TransmitterParam { Name = "sourceTeCode", Type = typeof(string), Value = sourceTeCode };
            var destTeCodeParam = new TransmitterParam { Name = "destTeCode", Type = typeof(string), Value = destTeCode };
            var skuIdParam = new TransmitterParam { Name = "skuId", Type = typeof(decimal), Value = skuId, };
            var productIdsParam = new TransmitterParam { Name = "productIds", Type = typeof(IEnumerable<decimal>), IsOut = false, Value = productIds };
            var countParam = new TransmitterParam { Name = "count", Type = typeof(decimal), Value = count };
            var truckCodeParam = new TransmitterParam { Name = "truckCode", Type = typeof(string), Value = truckCode };
            var isNotNeededCreateWorkParam = new TransmitterParam { Name = "isNotNeededCreateWork", Type = typeof(bool), IsOut = false, Value = isNotNeededCreateWork };
            transportTaskID = 0;
            var transportTaskIDParam = new TransmitterParam { Name = "transportTaskID", Type = typeof(decimal), IsOut = true, Value = transportTaskID };

            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "MoveProductsBySku", new[] { sourceTECodeParam, destTeCodeParam, skuIdParam, productIdsParam, countParam, truckCodeParam, isNotNeededCreateWorkParam, transportTaskIDParam });
            ProcessTelegramm(telegram);
            transportTaskID = Convert.ToInt32(transportTaskIDParam.Value);
        }

        public void CancelMoveProduct(decimal ttaskid, decimal count)
        {
            var ttaskidParam = new TransmitterParam { Name = "ttaskid", Type = typeof(decimal), Value = ttaskid, };
            var countParam = new TransmitterParam { Name = "count", Type = typeof(decimal), Value = count };

            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CancelMoveProduct", new[] { ttaskidParam, countParam });
            ProcessTelegramm(telegram);
        }

        public string GetDefaultMMStrategy(string TECode)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(string), IsOut = true };
            var TECodeParam = new TransmitterParam { Name = "TECode", Type = typeof(string), Value = TECode };

            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "GetDefaultMMStrategy", new[] { resultParam, TECodeParam });
            ProcessTelegramm(telegram);
            return (string)resultParam.Value;
        }

        public void CreTransportTaskAuto(string currentPlaceCode, string sourceTECode, IEnumerable<decimal> prdInputs, ref string strategy, IEnumerable<string> skipPlaceLst, out TransportTask trasportTask, out decimal productCount)
        {
            trasportTask = new TransportTask();
            productCount = 0;

            var currentPlaceCodeParam = new TransmitterParam { Name = "currentPlaceCode", Type = typeof(string), IsOut = false, Value = currentPlaceCode };
            var sourceTECodeParam = new TransmitterParam { Name = "sourceTECode", Type = typeof(string), IsOut = false, Value = sourceTECode };
            var prdInputsParam = new TransmitterParam { Name = "prdInputs", Type = typeof(IEnumerable<decimal>), IsOut = false, Value = prdInputs };
            var strategyParam = new TransmitterParam { Name = "strategy", Type = typeof(string), IsOut = true, Value = strategy };
            var skipPlaceLstParam = new TransmitterParam { Name = "skipPlaceLst", Type = typeof(IEnumerable<string>), IsOut = false, Value = skipPlaceLst };
            var trasportTaskParam = new TransmitterParam { Name = "trasportTask", Type = typeof(TransportTask), IsOut = true, Value = trasportTask };
            var productCountParam = new TransmitterParam { Name = "productCount", Type = typeof(decimal), IsOut = true, Value = productCount };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CreTransportTaskAuto", new[] { currentPlaceCodeParam, sourceTECodeParam, prdInputsParam, strategyParam, skipPlaceLstParam, trasportTaskParam, productCountParam });
            ProcessTelegramm(telegram);

            trasportTask = (TransportTask)trasportTaskParam.Value;
            productCount = (decimal)SerializationHelper.ConvertToTrueType(productCountParam.Value, typeof(decimal));
        }

        public decimal CheckTE2Move(string teCode, decimal? productID, decimal raiseError)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(decimal), IsOut = true };
            var teCodeParam = new TransmitterParam { Name = "teCode", Type = typeof(string), IsOut = false, Value = teCode };
            var productIDParam = new TransmitterParam { Name = "productID", Type = typeof(decimal?), IsOut = false, Value = productID };
            var raiseErrorParam = new TransmitterParam { Name = "raiseError", Type = typeof(decimal), IsOut = false, Value = raiseError };

            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CheckTE2Move", new[] { resultParam, teCodeParam, productIDParam, raiseErrorParam });
            ProcessTelegramm(telegram);
            return (decimal)SerializationHelper.ConvertToTrueType(resultParam.Value, typeof(decimal));
        }

        public List<string> GetAvailableTE(string teCode, string placeCode, decimal? productID, decimal productCount)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(List<string>), IsOut = true };
            var teCodeParam = new TransmitterParam { Name = "teCode", Type = typeof(string), IsOut = false, Value = teCode };
            var placeCodeParam = new TransmitterParam { Name = "placeCode", Type = typeof(string), IsOut = false, Value = placeCode };
            var productIDParam = new TransmitterParam { Name = "productID", Type = typeof(decimal?), IsOut = false, Value = productID };
            var productCountParam = new TransmitterParam { Name = "productCount", Type = typeof(decimal), IsOut = false, Value = productCount };

            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "GetAvailableTE", new[] { resultParam, teCodeParam, placeCodeParam, productIDParam, productCountParam });
            ProcessTelegramm(telegram);
            return (List<string>)SerializationHelper.ConvertToTrueType(resultParam.Value, typeof(List<string>));
        }

        public void ChangePlaceStatus(string placeCode, string operation)
        {
            var placeCodeParam = new TransmitterParam { Name = "placeCode", Type = typeof(decimal), Value = placeCode, };
            var operationParam = new TransmitterParam { Name = "operation", Type = typeof(decimal), Value = operation };

            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "ChangePlaceStatus", new[] { placeCodeParam, operationParam });
            ProcessTelegramm(telegram);
        }

        #endregion

        #region . Поставки .

        // Добавление в очередь поставок
        public void CreateQSupplyChain(string operationCode, decimal? mandantID, decimal? resGroup, string tECode, decimal? supplyChainID, decimal? raiseErr, string process)
        {
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var mandantIDParam = new TransmitterParam { Name = "mandantID", Type = typeof(decimal?), IsOut = false, Value = mandantID };
            var resGroupParam = new TransmitterParam { Name = "resGroup", Type = typeof(decimal?), IsOut = false, Value = resGroup };
            var tECodeParam = new TransmitterParam { Name = "tECode", Type = typeof(string), IsOut = false, Value = tECode };
            var supplyChainIDParam = new TransmitterParam { Name = "supplyChainID", Type = typeof(decimal?), IsOut = false, Value = supplyChainID };
            var raiseErrParam = new TransmitterParam { Name = "raiseErr", Type = typeof(decimal?), IsOut = false, Value = raiseErr };
            var processParam = new TransmitterParam { Name = "process", Type = typeof(string), IsOut = false, Value = process };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CreateQSupplyChain", new[] { operationCodeParam, mandantIDParam, resGroupParam, tECodeParam, supplyChainIDParam, raiseErrParam, processParam });
            ProcessTelegramm(telegram);
        }

        public void CreateQSupplyChainTt(string operationCode, decimal? mandantID, decimal? resGroup, string tECode,
            decimal? supplyChainID, decimal? raiseErr, string process, out decimal qSupplyChainID)
        {
            qSupplyChainID = 0;

            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var mandantIDParam = new TransmitterParam { Name = "mandantID", Type = typeof(decimal?), IsOut = false, Value = mandantID };
            var resGroupParam = new TransmitterParam { Name = "resGroup", Type = typeof(decimal?), IsOut = false, Value = resGroup };
            var tECodeParam = new TransmitterParam { Name = "tECode", Type = typeof(string), IsOut = false, Value = tECode };
            var supplyChainIDParam = new TransmitterParam { Name = "supplyChainID", Type = typeof(decimal?), IsOut = false, Value = supplyChainID };
            var raiseErrParam = new TransmitterParam { Name = "raiseErr", Type = typeof(decimal?), IsOut = false, Value = raiseErr };
            var processParam = new TransmitterParam { Name = "process", Type = typeof(string), IsOut = false, Value = process };
            var qSupplyChainIDParam = new TransmitterParam { Name = "qSupplyChainID", Type = typeof(decimal), IsOut = true, Value = qSupplyChainID };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CreateQSupplyChainTt", new[] { operationCodeParam, mandantIDParam, resGroupParam, tECodeParam, supplyChainIDParam, raiseErrParam, processParam, qSupplyChainIDParam });
            ProcessTelegramm(telegram);
            qSupplyChainID =
                (decimal) SerializationHelper.ConvertToTrueType(qSupplyChainIDParam.Value, typeof (decimal));
        }

        // БП "Подобрано" (с учетом поставок)
        public void OWBPickedBySupplyChain(decimal owbid, string operationCode)
        {
            var owbidParam = new TransmitterParam { Name = "owbid", Type = typeof(decimal), IsOut = false, Value = owbid };
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "OWBPickedBySupplyChain", new[] { owbidParam, operationCodeParam });
            ProcessTelegramm(telegram);
        }

        // Сформировать группу резервирования
        public void CreateResGroup(string entity, string key, out decimal resGroup)
        {
            resGroup = 0;
            var entityParam = new TransmitterParam { Name = "entity", Type = typeof(string), IsOut = false, Value = entity };
            var keyParam = new TransmitterParam { Name = "key", Type = typeof(string), IsOut = false, Value = key };
            var resGroupParam = new TransmitterParam { Name = "resGroup", Type = typeof(decimal), IsOut = true, Value = resGroup };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CreateResGroup", new[] { entityParam, keyParam, resGroupParam });
            ProcessTelegramm(telegram);
            resGroup = (decimal)SerializationHelper.ConvertToTrueType(resGroupParam.Value, typeof(decimal));
        }

        //создать поставку
        public void CreateSupplyChain(decimal qSupplyChainID, out decimal ttaskID)
        {
            ttaskID = 0;
            var schidParam = new TransmitterParam {Name = "qSupplyChainID", Type = typeof(decimal), IsOut = false, Value = qSupplyChainID};
            var ttaskIdParam = new TransmitterParam {Name = "ttaskID", Type = typeof(decimal), IsOut = true, Value = ttaskID};
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CreateSupplyChain", new[] { schidParam, ttaskIdParam });
            ProcessTelegramm(telegram);
            ttaskID = (decimal) SerializationHelper.ConvertToTrueType(ttaskIdParam.Value, typeof (decimal));
        }

        // Отменить поставку для ТЕ (с отменой ЗНТ)
        public void CancelSupplyChainForTE(string teCode, string operationCode)
        {
            var teCodeParam = new TransmitterParam { Name = "teCode", Type = typeof(string), IsOut = false, Value = teCode };
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CancelSupplyChainForTE", new[] { teCodeParam, operationCodeParam });
            ProcessTelegramm(telegram);
        }

        #endregion

        #region Счетчики ТЕ
        //Диапазон кодов ТЕ по коду счетчика ТЕ
        public void GetTeLabelRange(string sequenceCode, int count, out int rangeBegin, out int rangeEnd)
        {
            rangeBegin = 0;
            rangeEnd = 0;

            var sequenceCodeParam = new TransmitterParam { Name = "sequenceCode", Type = typeof(string), Value = sequenceCode, IsOut = false };
            var countParam = new TransmitterParam { Name = "count", Type = typeof(int), Value = count, IsOut = false };
            var rangeBeginParam = new TransmitterParam { Name = "rangeBegin", Type = typeof(int), Value = rangeBegin, IsOut = true };
            var rangeEndParam = new TransmitterParam { Name = "rangeEnd", Type = typeof(int), Value = rangeEnd, IsOut = true };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "GetTeLabelRange", new[] { sequenceCodeParam, countParam, rangeBeginParam, rangeEndParam });
            ProcessTelegramm(telegram);
            rangeBegin = Convert.ToInt32(rangeBeginParam.Value);
            rangeEnd = Convert.ToInt32(rangeEndParam.Value);
        }
        #endregion Счетчики ТЕ

        #region . Товар .

        public void CreateProduct(ref Product product, ref TE te)
        {
            var productParam = new TransmitterParam { Name = "product", Type = typeof(Product), Value = product, IsOut = true };
            var teParam = new TransmitterParam { Name = "te", Type = typeof(TE), Value = te, IsOut = true };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CreateProduct", new[] { productParam, teParam });
            ProcessTelegramm(telegram);
            product = (Product)productParam.Value;
            te = (TE)teParam.Value;
        }

        // расщепление товара
        public void splitProduct(decimal Key, decimal SplitCount, decimal FreeIfBusy, string operationCode, out decimal NewKey)
        {
            NewKey = 0;
            var KeyParam = new TransmitterParam { Name = "Key", Type = typeof(decimal), IsOut = false, Value = Key };
            var SplitCountParam = new TransmitterParam { Name = "SplitCount", Type = typeof(decimal), IsOut = false, Value = SplitCount };
            var FreeIfBusyParam = new TransmitterParam { Name = "FreeIfBusy", Type = typeof(decimal), IsOut = false, Value = FreeIfBusy };
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var NewKeyParam = new TransmitterParam { Name = "NewKey", Type = typeof(decimal), IsOut = true, Value = NewKey };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "splitProduct", new[] { KeyParam, SplitCountParam, FreeIfBusyParam, operationCodeParam, NewKeyParam });
            ProcessTelegramm(telegram);
            NewKey = NewKeyParam.Value.To(0);
        }

        // конвертация товара
        public IEnumerable<decimal> SplitProductWithSKU(decimal productId, decimal skuId, decimal countSku, double countInSku)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(IEnumerable<decimal>), IsOut = true };
            var productIdParam = new TransmitterParam { Name = "productId", Type = typeof(decimal), Value = productId, IsOut = false };
            var skuIdParam = new TransmitterParam { Name = "skuId", Type = typeof(decimal), Value = skuId, IsOut = false };
            var countSkuParam = new TransmitterParam { Name = "countSku", Type = typeof(decimal), Value = countSku, IsOut = false };
            var countInSkuParam = new TransmitterParam { Name = "countInSku", Type = typeof(decimal), Value = countInSku, IsOut = false };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "SplitProductWithSKU", new[] { resultParam, productIdParam, skuIdParam, countSkuParam, countInSkuParam });
            ProcessTelegramm(telegram);
            return (IEnumerable<decimal>)resultParam.Value;
        }
        
        // Разукомплектация
        public void DismantleKit(IEnumerable<decimal> productLst, string kitCode)
        {
            var productsParam = new TransmitterParam() {Name = "productLst",Type = typeof (IEnumerable<decimal>), Value = productLst, IsOut = false};
            var kitCodeParam = new TransmitterParam() { Name = "kitCode", Type = typeof(string), Value = kitCode, IsOut = false };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "DismantleKit", new[] { productsParam, kitCodeParam });
            ProcessTelegramm(telegram);
        }

        #endregion

        #region . Резервирование .

        // БП "Заменить зарезервированный товар"
        public void ChangeReserved(decimal oldProductId, decimal newProductId, decimal countNeeded)
        {
            var oldProductIdParam = new TransmitterParam { Name = "oldProductId", Type = typeof(decimal), IsOut = false, Value = oldProductId };
            var newProductIdParam = new TransmitterParam { Name = "newProductId", Type = typeof(decimal), IsOut = false, Value = newProductId };
            var countNeededParam = new TransmitterParam { Name = "countNeeded", Type = typeof(decimal), IsOut = false, Value = countNeeded };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "ChangeReserved", new[] { oldProductIdParam, newProductIdParam, countNeededParam });
            ProcessTelegramm(telegram);
        }

        // ручное резервирование
        public void ManualReserve(decimal? owbPosId, decimal? owbId, decimal productId, decimal owbProductNeed, out decimal outParam)
        {
            outParam = 0;
            var owbPosIdParam = new TransmitterParam { Name = "owbPosId", Type = typeof(decimal?), IsOut = false, Value = owbPosId };
            var owbIdParam = new TransmitterParam { Name = "owbId", Type = typeof(decimal?), IsOut = false, Value = owbId };
            var productIdParam = new TransmitterParam { Name = "productId", Type = typeof(decimal), IsOut = false, Value = productId };
            var owbProductNeedParam = new TransmitterParam { Name = "owbProductNeed", Type = typeof(decimal), IsOut = false, Value = owbProductNeed };
            var outParamParam = new TransmitterParam { Name = "outParam", Type = typeof(decimal), IsOut = true, Value = outParam };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "ManualReserve", new[] { owbPosIdParam, owbIdParam, productIdParam, owbProductNeedParam, outParamParam });
            ProcessTelegramm(telegram);
            outParam = outParamParam.Value.To(0);
        }

        // Резервировать список накладных
        public void ReserveOWBLst(IEnumerable<OWB> owbLst, string operationCode)
        {
            var owbLstParam = new TransmitterParam { Name = "owbLst", Type = typeof(IEnumerable<OWB>), IsOut = false, Value = owbLst };
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "ReserveOWBLst", new[] { owbLstParam, operationCodeParam });
            ProcessTelegramm(telegram);
        }

        // БП "Отмена резервирования"
        public void CancelReserve(string entity, IEnumerable<decimal> idLst, string operationCode, string eventKindCode, decimal count)
        {
            var entityParam = new TransmitterParam { Name = "entity", Type = typeof(string), IsOut = false, Value = entity };
            var idLstParam = new TransmitterParam { Name = "idLst", Type = typeof(IEnumerable<decimal>), IsOut = false, Value = idLst };
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var eventKindCodeParam = new TransmitterParam { Name = "eventKindCode", Type = typeof(string), IsOut = false, Value = eventKindCode };
            var countParam = new TransmitterParam { Name = "count", Type = typeof(decimal), IsOut = false, Value = count };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CancelReserve", new[] { entityParam, idLstParam, operationCodeParam, eventKindCodeParam, countParam });
            ProcessTelegramm(telegram);
        }
        #endregion

        #region . Подбор .

        public void CreatePickList(IEnumerable<OWB> owbList, string truckCode = null)
        {
            var owbListParam = new TransmitterParam { Name = "owbList", Type = typeof(IEnumerable<OWB>), IsOut = false, Value = owbList };
            var truckCodeParam = new TransmitterParam { Name = "truckCode", Type = typeof(string), IsOut = false, Value = truckCode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CreatePickList", new[] { owbListParam, truckCodeParam });
            ProcessTelegramm(telegram);
        }

        public void DeletePickList(IEnumerable<PL> pickList)
        {
            var pickListParam = new TransmitterParam { Name = "pickList", Type = typeof(IEnumerable<PL>), IsOut = false, Value = pickList };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "DeletePickList", new[] { pickListParam });
            ProcessTelegramm(telegram);
        }

        // Переехало в WmsAPI
        //public decimal GetPickListCount(string truckCode, decimal? plid)
        //{
        //    var resultParam = new TransmitterParam { Name = "result", Type = typeof(decimal), IsOut = true };
        //    var truckCodeParam = new TransmitterParam { Name = "truckCode", Type = typeof(string), IsOut = false, Value = truckCode };
        //    var plidParam = new TransmitterParam { Name = "plid", Type = typeof(decimal?), IsOut = false, Value = plid };
        //    var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "GetPickListCount", new[] { resultParam, truckCodeParam, plidParam });
        //    ProcessTelegramm(telegram);
        //    var result = (decimal)SerializationHelper.ConvertToTrueType(resultParam.Value, typeof(decimal));
        //    return result;
        //}

        // Резервировать список пикинга
        public void ReservePickList(decimal? plid, string truckCode, string operationCode, out PL pl, out Work work, string mplCode)
        {
            pl = null;
            work = null;

            var plidParam = new TransmitterParam { Name = "plid", Type = typeof(decimal?), IsOut = false, Value = plid };
            var truckCodeParam = new TransmitterParam { Name = "truckCode", Type = typeof(string), IsOut = false, Value = truckCode };
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var plParam = new TransmitterParam { Name = "pl", Type = typeof(PL), IsOut = true, Value = pl };
            var workParam = new TransmitterParam { Name = "work", Type = typeof(Work), IsOut = true, Value = work };
            var mplParam = new TransmitterParam { Name = "mplCode", Type = typeof(string), IsOut = false, Value = mplCode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "ReservePickList", new[] { plidParam, truckCodeParam, operationCodeParam, plParam, workParam, mplParam });
            ProcessTelegramm(telegram);
            pl = (PL)plParam.Value;
            work = (Work)workParam.Value;
        }

        // Обработка позиции пикинга
        public PLPos ProcessPlPos(IEnumerable<decimal> plLst, decimal? plPosId, string targetTeCode, decimal? count, string action,
            bool getNext = false, string operationCode = null)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(PLPos), IsOut = true };
            var plIdParam = new TransmitterParam { Name = "plLst", Type = typeof(IEnumerable<decimal>), IsOut = false, Value = plLst };
            var plPosIdParam = new TransmitterParam { Name = "plPosId", Type = typeof(decimal?), IsOut = false, Value = plPosId };
            var targetTeCodeParam = new TransmitterParam { Name = "targetTeCode", Type = typeof(string), IsOut = false, Value = targetTeCode };
            var countParam = new TransmitterParam { Name = "count", Type = typeof(decimal?), IsOut = false, Value = count };
            var actionParam = new TransmitterParam { Name = "action", Type = typeof(string), IsOut = false, Value = action };
            var getNextParam = new TransmitterParam { Name = "getNext", Type = typeof(bool), IsOut = false, Value = getNext };
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "ProcessPlPos", new[] { resultParam, plIdParam, plPosIdParam, targetTeCodeParam, countParam, actionParam, getNextParam, operationCodeParam });
            ProcessTelegramm(telegram);
            var result = (PLPos)resultParam.Value;
            return result;
        }

        // Завершить подбор на ТЕ
        public void CompletePickTE(string teCode, out decimal? tTaskID, string operationCode, bool instantReserveTtask)
        {
            tTaskID = null;
            var teCodeParam = new TransmitterParam { Name = "teCode", Type = typeof(decimal), IsOut = false, Value = teCode };
            var tTaskIDParam = new TransmitterParam { Name = "tTaskID", Type = typeof(decimal?), IsOut = true, Value = tTaskID };
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var instantReserveTtaskParam = new TransmitterParam { Name = "instantReserveTtask", Type = typeof(bool), IsOut = false, Value = instantReserveTtask };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CompletePickTE", new[] { teCodeParam, tTaskIDParam, operationCodeParam, instantReserveTtaskParam });
            ProcessTelegramm(telegram);
            tTaskID = (tTaskIDParam.Value != null)
                ? (decimal)SerializationHelper.ConvertToTrueType(tTaskIDParam.Value, typeof(decimal))
                : (decimal?)null;
        }

        public void CompletePlPos(decimal idPlPos)
        {

            var IdPlPosParam = new TransmitterParam { Name = "idPlPos", Type = typeof(decimal), IsOut = false, Value = idPlPos };

            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CompletePlPos", new[] { IdPlPosParam });
            ProcessTelegramm(telegram);
        }


        public void DeactivatePlPos(IEnumerable<decimal> plPosLst)
        {
            var plPosParam = new TransmitterParam { Name = "plPosLst", Type = typeof(IEnumerable<decimal>), IsOut = false, Value = plPosLst };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "DeactivatePlPos", new[] { plPosParam });
            ProcessTelegramm(telegram);
        }

        public PLPos FindNextPlPosByTeByBarcode(decimal plid, string tecode, string barcode, decimal? currentPlPosId, bool needActivated, out SKU sku)
        {
            sku = null;
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(PLPos), IsOut = true };
            var plidParam = new TransmitterParam { Name = "plid", Type = typeof(decimal), IsOut = false, Value = plid };
            var tecodeParam = new TransmitterParam { Name = "tecode", Type = typeof(string), IsOut = false, Value = tecode };
            var barcodeParam = new TransmitterParam { Name = "barcode", Type = typeof(string), IsOut = false, Value = barcode };
            var currentPlPosIdParam = new TransmitterParam { Name = "currentPlPosId", Type = typeof(decimal?), IsOut = false, Value = currentPlPosId };
            var needActivatedParam = new TransmitterParam { Name = "needActivated", Type = typeof(bool), IsOut = false, Value = needActivated };
            var skuParam = new TransmitterParam { Name = "sku", Type = typeof(SKU), IsOut = true, Value = null };

            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "FindNextPlPosByTeByBarcode", new[] { resultParam, plidParam, tecodeParam, barcodeParam, currentPlPosIdParam, needActivatedParam, skuParam });
            ProcessTelegramm(telegram);
            sku = (SKU) skuParam.Value;
            var result = (PLPos) resultParam.Value;
            return result;
        }

        // Проверка кратности единицы учета
        public bool IsMultipleSku(decimal skuId, decimal count, IEnumerable<decimal> skuList)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(bool), IsOut = true };
            var countParam = new TransmitterParam { Name = "count", Type = typeof(decimal), IsOut = false, Value = count };
            var skuIdParam = new TransmitterParam { Name = "skuId", Type = typeof(decimal), IsOut = false, Value = skuId };
            var skuListParam = new TransmitterParam { Name = "skuList", Type = typeof(IEnumerable<decimal>), IsOut = false, Value = skuList };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "IsMultipleSku", new[] { resultParam, skuIdParam, countParam, skuListParam });
            ProcessTelegramm(telegram);
            return (bool)SerializationHelper.ConvertToTrueType(resultParam.Value, typeof(bool));
        }

        #endregion

        #region . Упаковка .
        // создать короб
        public TE CreateBox(string teCode, string teTypeCode, string creationPlaceCode)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(TE), IsOut = true };
            var teCodeParam = new TransmitterParam { Name = "teCode", Type = typeof(string), IsOut = false, Value = teCode };
            var teTypeCodeParam = new TransmitterParam { Name = "teTypeCode", Type = typeof(string), IsOut = false, Value = teTypeCode };
            var creationPlaceCodeParam = new TransmitterParam { Name = "creationPlaceCode", Type = typeof(string), IsOut = false, Value = creationPlaceCode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CreateBox", new[] { resultParam, teCodeParam, teTypeCodeParam, creationPlaceCodeParam });
            ProcessTelegramm(telegram);
            return (TE)resultParam.Value;
        }

        // упаковано
        public void Packed(decimal owbId)
        {
            var owbIdParam = new TransmitterParam { Name = "owbId", Type = typeof(decimal), IsOut = false, Value = owbId };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "Packed", new[] { owbIdParam });
            ProcessTelegramm(telegram);
        }

        // упаковать
        public void Pack(IEnumerable<Product> products)
        {
            var productsParam = new TransmitterParam { Name = "products", Type = typeof(IEnumerable<Product>), IsOut = false, Value = products };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "Pack", new[] { productsParam });
            ProcessTelegramm(telegram);
        }

        public void PackProductLst(IEnumerable<decimal> productIdLst, IEnumerable<Product> changedProducts, string packTE, decimal packCount, bool packFullProduct)
        {
            var productIdsParam = new TransmitterParam { Name = "productIdLst", Type = typeof(IEnumerable<decimal>), IsOut = false, Value = productIdLst };
            var changedProductsParam = new TransmitterParam { Name = "changedProducts", Type = typeof(IEnumerable<Product>), IsOut = false, Value = changedProducts };
            var packTEParam = new TransmitterParam { Name = "packTE", Type = typeof(string), IsOut = false, Value = packTE };
            var packCountParam = new TransmitterParam { Name = "packCount", Type = typeof(decimal), IsOut = false, Value = packCount };
            var packFullProductParam = new TransmitterParam { Name = "packFullProduct", Type = typeof(bool), IsOut = false, Value = packFullProduct };

            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "PackProductLst", new[] { productIdsParam, changedProductsParam, packTEParam, packCountParam, packFullProductParam });
            ProcessTelegramm(telegram);
        }

        public void PackProductLstBySKU(IEnumerable<decimal> productIdLst, IEnumerable<Product> changedProducts, string packTECode, decimal skuId, decimal packProductCountSKU, bool isEnablePackOtherSKU)
        {
            var productIdsParam = new TransmitterParam { Name = "productIdLst", Type = typeof(IEnumerable<decimal>), IsOut = false, Value = productIdLst };
            var changedProductsParam = new TransmitterParam { Name = "changedProducts", Type = typeof(IEnumerable<Product>), IsOut = false, Value = changedProducts };
            var packTECodeParam = new TransmitterParam { Name = "packTECode", Type = typeof(string), IsOut = false, Value = packTECode };
            var skuIdParam = new TransmitterParam { Name = "skuId", Type = typeof(decimal), IsOut = false, Value = skuId };
            var packProductCountSKUParam = new TransmitterParam { Name = "packProductCountSKU", Type = typeof(decimal), IsOut = false, Value = packProductCountSKU };
            var isEnablePackOtherSKUParam = new TransmitterParam { Name = "isEnablePackOtherSKU", Type = typeof(bool), IsOut = false, Value = isEnablePackOtherSKU };

            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "PackProductLstBySKU", new[] { productIdsParam, changedProductsParam, packTECodeParam, skuIdParam, packProductCountSKUParam, isEnablePackOtherSKUParam });
            ProcessTelegramm(telegram);
        }

        public void PackProduct(decimal productId, string packTE, decimal packCount)
        {
            var productIdParam = new TransmitterParam { Name = "productId", Type = typeof(decimal), IsOut = false, Value = productId };
            var packTEParam = new TransmitterParam { Name = "packTE", Type = typeof(string), IsOut = false, Value = packTE };
            var packCountParam = new TransmitterParam { Name = "packCount", Type = typeof(decimal), IsOut = false, Value = packCount };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "PackProduct", new[] { productIdParam, packTEParam, packCountParam });
            ProcessTelegramm(telegram);
        }

        // распаковать
        public Product UnPack(decimal productID)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(Product), IsOut = true };
            var productIDParam = new TransmitterParam { Name = "productID", Type = typeof(decimal), IsOut = false, Value = productID };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "UnPack", new[] { resultParam, productIDParam });
            ProcessTelegramm(telegram);
            return (Product)resultParam.Value;
        }

        // закрыть короб
        public void CloseBox(string operationCode, string teCode)
        {
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var teParam = new TransmitterParam { Name = "teCode", Type = typeof(string), IsOut = false, Value = teCode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CloseBox", new[] { operationCodeParam, teParam });
            ProcessTelegramm(telegram);
        }

        // открыть короб
        public void OpenBox(string operationCode, string teCode, string packPlaceCode)
        {
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var teCodeParam = new TransmitterParam { Name = "teCode", Type = typeof(string), IsOut = false, Value = teCode };
            var packPlaceCodeParam = new TransmitterParam { Name = "packPlaceCode", Type = typeof(string), IsOut = false, Value = packPlaceCode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "OpenBox", new[] { operationCodeParam, teCodeParam, packPlaceCodeParam });
            ProcessTelegramm(telegram);
        }

        // Получить вес ТЕ
        public decimal GetTEWeight(string teCode)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(decimal), IsOut = true };
            var teParam = new TransmitterParam { Name = "teCode", Type = typeof(string), IsOut = false, Value = teCode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "GetTEWeight", new[] { resultParam, teParam });
            ProcessTelegramm(telegram);
            return (decimal) SerializationHelper.ConvertToTrueType(resultParam.Value, typeof (decimal));
        }

        // Получить вес ТЕ и погрешность, если определена
        public void GetTEWeightControl(string teCode, out decimal weight, out decimal? dev)
        {
            weight = 0;
            var teParam = new TransmitterParam { Name = "teCode", Type = typeof(string), IsOut = false, Value = teCode };
            var weightParam = new TransmitterParam { Name = "weight", Type = typeof(decimal), IsOut = true, Value = weight };
            var devParam = new TransmitterParam { Name = "dev", Type = typeof(decimal?), IsOut = true, Value = null };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "GetTEWeightControl", new[] { teParam, weightParam, devParam });
            ProcessTelegramm(telegram);
            weight = (decimal)SerializationHelper.ConvertToTrueType(weightParam.Value, typeof(decimal));
            dev = devParam.Value == null
                ? (decimal?)null
                : (decimal)SerializationHelper.ConvertToTrueType(devParam.Value, typeof(decimal));
        }

        // Вернуть на исходную ТЕ
        public void ReturnOnSourceTE(IEnumerable<decimal> productIDLst, string placeCode)
        {
            var productIDLstParam = new TransmitterParam { Name = "productIDLst", Type = typeof(IEnumerable<decimal>), IsOut = false, Value = productIDLst };
            var placeCodeParam = new TransmitterParam { Name = "placeCode", Type = typeof(string), IsOut = false, Value = placeCode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "ReturnOnSourceTE", new[] { productIDLstParam, placeCodeParam });
            ProcessTelegramm(telegram);
        }

        // Проверка упакованности заказа
        public string CheckPackOWB(decimal productID, int? checkWh)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(string), IsOut = true };
            var productIDParam = new TransmitterParam { Name = "productID", Type = typeof(decimal), IsOut = false, Value = productID };
            var checkWhParam = new TransmitterParam { Name = "checkWh", Type = typeof(int?), IsOut = false, Value = checkWh };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CheckPackOWB", new[] { resultParam, productIDParam, checkWhParam });
            ProcessTelegramm(telegram);
            return (string)SerializationHelper.ConvertToTrueType(resultParam.Value, typeof(string));
        }

        public void UnpackTe(string teCode, string placeCode)
        {
            var teCodeParam = new TransmitterParam { Name = "teCode", Type = typeof(string), IsOut = false, Value = teCode };
            var placeCodeParam = new TransmitterParam { Name = "placeCode", Type = typeof(string), IsOut = false, Value = placeCode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "UnpackTe", new[] { teCodeParam, placeCodeParam });
            ProcessTelegramm(telegram);
        }

        #endregion

        #region . Отгрузка .

        // Завершение отгрузки по расходной накладной
        public void CompleteOWB(decimal key, decimal needTraffic, string operationCode, decimal? itid)
        {
            var keyParam = new TransmitterParam { Name = "key", Type = typeof(decimal), IsOut = false, Value = key };
            var needTrafficParam = new TransmitterParam { Name = "needTraffic", Type = typeof(decimal), IsOut = false, Value = needTraffic };
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var itidParam = new TransmitterParam { Name = "itid", Type = typeof(decimal?), IsOut = false, Value = itid };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CompleteOWB", new[] { keyParam, needTrafficParam, operationCodeParam, itidParam });
            ProcessTelegramm(telegram);
        }

        // Завершение отгрузки по внутреннему рейсу
        public void CompleteCargoOWB(decimal key, string operationCode)
        {
            var keyParam = new TransmitterParam { Name = "key", Type = typeof(decimal), IsOut = false, Value = key };
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CompleteCargoOWB", new[] { keyParam, operationCodeParam });
            ProcessTelegramm(telegram);
        }

        // БП "Поместить комплекты" (отгрузочная часть)
        public void ConvertToKit(decimal owbid)
        {
            var owbidParam = new TransmitterParam { Name = "owbid", Type = typeof(decimal), IsOut = false, Value = owbid };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "ConvertToKit", new[] { owbidParam });
            ProcessTelegramm(telegram);
        }

        // Аннуляция расходной накланой
        public void CancelOWB(decimal owbid, string operationcode, string eventKind)
        {
            var owbidParam = new TransmitterParam { Name = "owbid", Type = typeof(decimal), IsOut = false, Value = owbid };
            var operationcodeParam = new TransmitterParam { Name = "operationcode", Type = typeof(string), IsOut = false, Value = operationcode };
            var eventParam = new TransmitterParam { Name = "eventKind", Type = typeof(string), IsOut = false, Value = eventKind };

            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CancelOWB", new[] { owbidParam, operationcodeParam, eventParam });
            ProcessTelegramm(telegram);
        }

        // Возврат накладной
        public void ReturnOwb(string operationcode, decimal key, string returnplacecode)
        {
            var operationcodeParam = new TransmitterParam { Name = "operationcode", Type = typeof(string), IsOut = false, Value = operationcode };
            var keyParam = new TransmitterParam { Name = "key", Type = typeof(decimal), IsOut = false, Value = key };
            var returnplacecodeParam = new TransmitterParam { Name = "returnplacecode", Type = typeof(string), IsOut = false, Value = returnplacecode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "ReturnOWB", new[] { operationcodeParam, keyParam, returnplacecodeParam });
            ProcessTelegramm(telegram);
        }

        // Отгрузка ТЕ
        public void CompleteTE(string teCode, decimal cargoOWBID, string operationcode, out bool isLastTE)
        {
            isLastTE = false;
            var teCodeParam = new TransmitterParam { Name = "teCode", Type = typeof(string), IsOut = false, Value = teCode };
            var cargoOWBIDParam = new TransmitterParam { Name = "cargoOWBID", Type = typeof(decimal), IsOut = false, Value = cargoOWBID };
            var operationcodeParam = new TransmitterParam { Name = "operationcode", Type = typeof(string), IsOut = false, Value = operationcode };
            var isLastTEParam = new TransmitterParam { Name = "isLastTE", Type = typeof(bool), IsOut = true, Value = isLastTE };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CompleteTE", new[] { teCodeParam, cargoOWBIDParam, operationcodeParam, isLastTEParam });
            ProcessTelegramm(telegram);
            isLastTE = (bool)SerializationHelper.ConvertToTrueType(isLastTEParam.Value, typeof(bool));
        }

        // Отгрузка нескольких ТЕ
        public void CompleteManyTE(IEnumerable<string> teCodes, decimal cargoOWBID, string operationcode)
        {
            var teCodesParam = new TransmitterParam { Name = "teCodes", Type = typeof(IEnumerable<string>), IsOut = false, Value = teCodes };
            var cargoOWBIDParam = new TransmitterParam { Name = "cargoOWBID", Type = typeof(decimal), IsOut = false, Value = cargoOWBID };
            var operationcodeParam = new TransmitterParam { Name = "operationcode", Type = typeof(string), IsOut = false, Value = operationcode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CompleteManyTE", new[] { teCodesParam, cargoOWBIDParam, operationcodeParam });
            ProcessTelegramm(telegram);
        }

        // Создание позиций прихода из расхода
        public void FromOwb2Iwb(string entity, decimal owbid, decimal iwbid)
        {
            var entityParam = new TransmitterParam { Name = "entity", Type = typeof(string), IsOut = false, Value = entity };
            var owbidParam = new TransmitterParam { Name = "owbid", Type = typeof(decimal), IsOut = false, Value = owbid };
            var iwbidParam = new TransmitterParam { Name = "iwbid", Type = typeof(decimal), IsOut = false, Value = iwbid };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "FromOwb2Iwb", new[] { entityParam, owbidParam, iwbidParam });
            ProcessTelegramm(telegram);
        }
        #endregion

        #region . Биллинг .

        // Рассчитать акт
        public void CalcBillWorkAct(decimal workActId, decimal? workAct2Op2cId, decimal? trace, bool fictional)
        {
            var workActIdParam = new TransmitterParam { Name = "workActId", Type = typeof(decimal), IsOut = false, Value = workActId };
            var workAct2Op2cIdParam = new TransmitterParam { Name = "workAct2Op2cId", Type = typeof(decimal?), IsOut = false, Value = workAct2Op2cId };
            var traceParam = new TransmitterParam { Name = "trace", Type = typeof(decimal?), IsOut = false, Value = trace };
            var fictionalParam = new TransmitterParam { Name = "fictional", Type = typeof(bool), IsOut = false, Value = fictional };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CalcBillWorkAct", new[] { workActIdParam, workAct2Op2cIdParam, traceParam, fictionalParam });
            ProcessTelegramm(telegram);
        }

        // Очистить акт
        public void ClearBillWorkAct(decimal workActId, decimal? workAct2Op2cId, decimal? isManualOnly)
        {
            var workActIdParam = new TransmitterParam { Name = "workActId", Type = typeof(decimal), IsOut = false, Value = workActId };
            var workAct2Op2cIdParam = new TransmitterParam { Name = "workAct2Op2cId", Type = typeof(decimal?), IsOut = false, Value = workAct2Op2cId };
            var isManualOnlyParam = new TransmitterParam { Name = "isManualOnly", Type = typeof(decimal?), IsOut = false, Value = isManualOnly };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "ClearBillWorkAct", new[] { workActIdParam, workAct2Op2cIdParam, isManualOnlyParam });
            ProcessTelegramm(telegram);
        }

        // Фиксировать акт
        public void FixBillWorkAct(decimal workActId)
        {
            var workActIdParam = new TransmitterParam { Name = "workActId", Type = typeof(decimal), IsOut = false, Value = workActId };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "FixBillWorkAct", new[] { workActIdParam });
            ProcessTelegramm(telegram);
        }

        // Вернуть акт
        public void UnFixBillWorkAct(decimal workActId)
        {
            var workActIdParam = new TransmitterParam { Name = "workActId", Type = typeof(decimal), IsOut = false, Value = workActId };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "UnFixBillWorkAct", new[] { workActIdParam });
            ProcessTelegramm(telegram);
        }
        #endregion

        #region . Двор .

        // Информация о приходном грузе (груз, работа, внут. рейс, рейс)
        public void GetCargoIWBInfo(decimal cargoIWBID, string operationCode, out CargoIWB cargoIWB, out Work work, out InternalTraffic internalTraffic, out ExternalTraffic externalTraffic)
        {
            cargoIWB = null;
            work = null;
            internalTraffic = null;
            externalTraffic = null;

            var cargoIWBIDParam = new TransmitterParam { Name = "cargoIWBID", Type = typeof(decimal), IsOut = false, Value = cargoIWBID };
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var cargoIWBParam = new TransmitterParam { Name = "cargoIWB", Type = typeof(CargoIWB), IsOut = true, Value = cargoIWB };
            var workParam = new TransmitterParam { Name = "work", Type = typeof(Work), IsOut = true, Value = work };
            var internalTrafficParam = new TransmitterParam { Name = "internalTraffic", Type = typeof(InternalTraffic), IsOut = true, Value = internalTraffic };
            var externalTrafficParam = new TransmitterParam { Name = "externalTraffic", Type = typeof(ExternalTraffic), IsOut = true, Value = externalTraffic };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "GetCargoIWBInfo", new[] { cargoIWBIDParam, operationCodeParam, cargoIWBParam, workParam, internalTrafficParam, externalTrafficParam });
            ProcessTelegramm(telegram);
            cargoIWB = (CargoIWB)cargoIWBParam.Value;
            work = (Work)workParam.Value;
            internalTraffic = (InternalTraffic)internalTrafficParam.Value;
            externalTraffic = (ExternalTraffic)externalTrafficParam.Value;
        }

        // Информация о расходном грузе (груз, работа, внут. рейс, рейс)
        public void GetCargoOWBInfo(decimal cargoOWBID, string operationCode, out CargoOWB cargoOWB, out Work work, out InternalTraffic internalTraffic, out ExternalTraffic externalTraffic, out decimal existTE)
        {
            cargoOWB = null;
            work = null;
            internalTraffic = null;
            externalTraffic = null;
            existTE = 1;

            var cargoOWBIDParam = new TransmitterParam { Name = "cargoOWBID", Type = typeof(decimal), IsOut = false, Value = cargoOWBID };
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var cargoOWBParam = new TransmitterParam { Name = "cargoOWB", Type = typeof(CargoOWB), IsOut = true, Value = cargoOWB };
            var workParam = new TransmitterParam { Name = "work", Type = typeof(Work), IsOut = true, Value = work };
            var internalTrafficParam = new TransmitterParam { Name = "internalTraffic", Type = typeof(InternalTraffic), IsOut = true, Value = internalTraffic };
            var externalTrafficParam = new TransmitterParam { Name = "externalTraffic", Type = typeof(ExternalTraffic), IsOut = true, Value = externalTraffic };
            var existTEParam = new TransmitterParam { Name = "existTE", Type = typeof(decimal), IsOut = true, Value = existTE };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "GetCargoOWBInfo", new[] { cargoOWBIDParam, operationCodeParam, cargoOWBParam, workParam, internalTrafficParam, externalTrafficParam, existTEParam });
            ProcessTelegramm(telegram);
            cargoOWB = (CargoOWB)cargoOWBParam.Value;
            work = (Work)workParam.Value;
            internalTraffic = (InternalTraffic)internalTrafficParam.Value;
            externalTraffic = (ExternalTraffic)externalTrafficParam.Value;
            existTE = (decimal)SerializationHelper.ConvertToTrueType(existTEParam.Value, typeof(decimal));
        }

        // Маршрутизировать
        public void RouteTE2CargoOWB(decimal cargoOWBID, IEnumerable<TE> TELstXml)
        {
            var cargoOWBIDParam = new TransmitterParam { Name = "cargoOWBID", Type = typeof(decimal), IsOut = false, Value = cargoOWBID };
            var TELstXmlParam = new TransmitterParam { Name = "TELstXml", Type = typeof(IEnumerable<TE>), IsOut = false, Value = TELstXml };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "RouteTE2CargoOWB", new[] { cargoOWBIDParam, TELstXmlParam });
            ProcessTelegramm(telegram);
        }

        // Поставить ТС на ворота
        public void SetTrafficGate(decimal internalTrafficID, string gateCode, string operationCode)
        {
            var internalTrafficIDParam = new TransmitterParam { Name = "internalTrafficID", Type = typeof(decimal), IsOut = false, Value = internalTrafficID };
            var gateCodeParam = new TransmitterParam { Name = "gateCode", Type = typeof(string), IsOut = false, Value = gateCode };
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "SetTrafficGate", new[] { internalTrafficIDParam, gateCodeParam, operationCodeParam });
            ProcessTelegramm(telegram);
        }

        #endregion

        #region .  Маршрутизация  .

        // Рассчитать маршруты
        public List<string> ChangeOWBRoute(IEnumerable<decimal> OWBIDLst, decimal? routeID, DateTime? planDate = null,
            bool changeDate = true, bool changeRoute = true)
        {
            var OWBIDLstParam = new TransmitterParam { Name = "OWBIDLst", Type = typeof(IEnumerable<decimal>), IsOut = false, Value = OWBIDLst };
            var routeIDParam = new TransmitterParam { Name = "routeID", Type = typeof(decimal?), IsOut = false, Value = routeID };
            var planDateParam = new TransmitterParam { Name = "planDate", Type = typeof(DateTime?), IsOut = false, Value = planDate };
            var changeDateParam = new TransmitterParam { Name = "changeDate", Type = typeof(bool), IsOut = false, Value = changeDate };
            var changeRouteParam = new TransmitterParam { Name = "changeRoute", Type = typeof(bool), IsOut = false, Value = changeRoute };
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(List<string>), IsOut = true };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "ChangeOWBRoute", new[] { OWBIDLstParam, routeIDParam, planDateParam, changeDateParam, changeRouteParam, resultParam });
            ProcessTelegramm(telegram);
            return (List<string>)resultParam.Value;
        }

        #endregion

        #region .  Менеджер товара  .

        // Настройка менеджера товара по списку товаров
        public List<PMConfig> GetPMConfigListByProductList(IEnumerable<decimal> productIdList, string operationCode, string methodCode)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(List<PMConfig>), IsOut = true };
            var productIdListParam = new TransmitterParam { Name = "productIdList", Type = typeof(IEnumerable<decimal>), Value = productIdList, IsOut = false };
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), Value = operationCode, IsOut = false };
            var methodCodeParam = new TransmitterParam { Name = "methodCode", Type = typeof(string), Value = methodCode, IsOut = false };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "GetPMConfigListByProductList", new[] { resultParam, productIdListParam, operationCodeParam, methodCodeParam });
            ProcessTelegramm(telegram);
            return (List<PMConfig>)resultParam.Value;
        }

        // Настройка менеджера товара по списку артикулов
        public List<PMConfig> GetPMConfigListByArtCodeList(IEnumerable<string> artCodeList, string operationCode, string methodCode)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(List<PMConfig>), IsOut = true };
            var artCodeListParam = new TransmitterParam { Name = "artCodeList", Type = typeof(IEnumerable<string>), Value = artCodeList, IsOut = false };
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), Value = operationCode, IsOut = false };
            var methodCodeParam = new TransmitterParam { Name = "methodCode", Type = typeof(string), Value = methodCode, IsOut = false };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "GetPMConfigListByArtCodeList", new[] { resultParam, artCodeListParam, operationCodeParam, methodCodeParam });
            ProcessTelegramm(telegram);
            return (List<PMConfig>)resultParam.Value;
        }

        #endregion . Менеджер товара .

        #region . Инвентаризация .

        // Товар, не вошедший в инвентаризацию
        public List<Product> GetInvMissedProduct(string filter, IEnumerable<decimal> invIdLst)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(List<Product>), IsOut = true };
            var filterParam = new TransmitterParam { Name = "filter", Type = typeof(string), Value = filter, IsOut = false };
            var invIdLstParam = new TransmitterParam { Name = "invIdLst", Type = typeof(IEnumerable<decimal>), Value = invIdLst, IsOut = false };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "GetInvMissedProduct", new[] { resultParam, filterParam, invIdLstParam });
            ProcessTelegramm(telegram);
            return (List<Product>)resultParam.Value;
        }

        // Создать инвентаризацию
        public void CreateInv(decimal invID, string operationCode)
        {
            var invIDParam = new TransmitterParam { Name = "invID", Type = typeof(decimal), IsOut = false, Value = invID };
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CreateInv", new[] { invIDParam, operationCodeParam });
            ProcessTelegramm(telegram);
        }

        public void FixInvTaskStep(IEnumerable<decimal> invTaskGroupIDLst, string operationCode)
        {
            var listParam = new TransmitterParam { Name = "invTaskGroupIDLst", Type = typeof(IEnumerable<decimal>), IsOut = false, Value = invTaskGroupIDLst };
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "FixInvTaskStep", new[] { listParam, operationCodeParam });
            ProcessTelegramm(telegram);
        }

        // Зафиксировать инвентаризацию 
        public void FixInv(decimal invID, string operationCode)
        {
            var invIDParam = new TransmitterParam { Name = "invID", Type = typeof(decimal), IsOut = false, Value = invID };
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "FixInv", new[] { invIDParam, operationCodeParam });
            ProcessTelegramm(telegram);
        }

        // Очистить инвентаризацию 
        public void CleanInv(decimal invID)
        {
            var invIDParam = new TransmitterParam { Name = "invID", Type = typeof(decimal), IsOut = false, Value = invID };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CleanInv", new[] { invIDParam });
            ProcessTelegramm(telegram);
        }

        // Расфиксировать инвентаризацию
        public void UnFixInv(decimal invId, string operationCode)
        {
            var invIdParam = new TransmitterParam { Name = "invId", Type = typeof(decimal), IsOut = false, Value = invId };
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "UnFixInv", new[] { invIdParam, operationCodeParam });
            ProcessTelegramm(telegram);
        }

        //  Расчет расхождений
        public void FindDifference(decimal invGroupID, IEnumerable<InvTaskGroup> invTaskGroupIDLst, string operationCode, out decimal flag)
        {
            flag = 0;

            var invGroupDParam = new TransmitterParam { Name = "invGroupID", Type = typeof(decimal), IsOut = false, Value = invGroupID };
            var listParam = new TransmitterParam { Name = "invTaskGroupIDLst", Type = typeof(IEnumerable<InvTaskGroup>), IsOut = false, Value = invTaskGroupIDLst };
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var flagParam = new TransmitterParam { Name = "flag", Type = typeof(decimal), IsOut = true, Value = flag };

            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "FindDifference", new[] { invGroupDParam, listParam, operationCodeParam, flagParam });
            ProcessTelegramm(telegram);
            flag = (decimal)SerializationHelper.ConvertToTrueType(flagParam.Value, typeof(decimal));
        }

        //  Корректировка
        public void InvCorrect(decimal invID, string operationCode)
        {
            var invIDParam = new TransmitterParam { Name = "invID", Type = typeof(decimal), IsOut = false, Value = invID };
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "InvCorrect", new[] { invIDParam, operationCodeParam });
            ProcessTelegramm(telegram);
        }

        //  Подготовить инвентаризацию
        public void PrepareInv(decimal invReqID, decimal pageRecCount, DateTime dateBegin, out Inv inv)
        {
            inv = new Inv();
            var invReqIDParam = new TransmitterParam { Name = "invReqID", Type = typeof(decimal), IsOut = false, Value = invReqID };
            var pageRecCountParam = new TransmitterParam { Name = "pageRecCount", Type = typeof(decimal), IsOut = false, Value = pageRecCount };
            var dateBeginParam = new TransmitterParam { Name = "dateBegin", Type = typeof(DateTime), IsOut = false, Value = dateBegin };
            var invParam = new TransmitterParam { Name = "inv", Type = typeof(Inv), IsOut = true, Value = inv };

            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "PrepareInv", new[] { invReqIDParam, pageRecCountParam, dateBeginParam, invParam });
            ProcessTelegramm(telegram);
            inv = (Inv)SerializationHelper.ConvertToTrueType(invParam.Value, typeof(Inv));
        }

        // Получение группы заданий
        public void ReserveInvGroup(decimal invID, ref decimal? invTaskGroupID, string placeCode)
        {
            var invIDParam = new TransmitterParam { Name = "invID", Type = typeof(decimal), IsOut = false, Value = invID };
            var invTaskGroupIDParam = new TransmitterParam { Name = "invTaskGroupID", Type = typeof(decimal?), IsOut = true, Value = invTaskGroupID };
            var placeCodeParam = new TransmitterParam { Name = "placeCode", Type = typeof(string), IsOut = false, Value = placeCode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "ReserveInvGroup", new[] { invIDParam, invTaskGroupIDParam, placeCodeParam });
            ProcessTelegramm(telegram);
            invTaskGroupID = (decimal?)SerializationHelper.ConvertToTrueType(invTaskGroupIDParam.Value, typeof(decimal?));
        }

        public string AcceptPlace(decimal invTaskGroupID, string action)
        {
            var invTaskGroupIDParam = new TransmitterParam { Name = "invTaskGroupID", Type = typeof(decimal), IsOut = false, Value = invTaskGroupID };
            var actionParam = new TransmitterParam { Name = "action", Type = typeof(string), IsOut = false, Value = action };
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(string), IsOut = true };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "AcceptPlace", new[] { invTaskGroupIDParam, actionParam, resultParam });
            ProcessTelegramm(telegram);
            return (string)resultParam.Value;
        }

        // Запись задания инвентаризации в буфер
        public void AcceptInvTask(InvTask invTask, string action)
        {
            var invTaskParam = new TransmitterParam { Name = "invTask", Type = typeof(InvTask), IsOut = false, Value = invTask };
            var actionParam = new TransmitterParam { Name = "action", Type = typeof(string), IsOut = false, Value = action };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "AcceptInvTask", new[] { invTaskParam, actionParam });
            ProcessTelegramm(telegram);
        }

        // Получение очереднего задания
        public InvTask GetInvTask(decimal invTaskGroupID, decimal? invTaskID, bool recalc)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(InvTask), IsOut = true };
            var invTaskGroupIDParam = new TransmitterParam { Name = "invTaskGroupID", Type = typeof(decimal), IsOut = false, Value = invTaskGroupID };
            var invTaskIDParam = new TransmitterParam { Name = "invTaskID", Type = typeof(decimal?), IsOut = false, Value = invTaskID };
            var recalcParam = new TransmitterParam { Name = "recalc", Type = typeof(Boolean), IsOut = false, Value = recalc };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "GetInvTask", new[] { resultParam, invTaskGroupIDParam, invTaskIDParam, recalcParam });
            ProcessTelegramm(telegram);
            return (InvTask)resultParam.Value;
        }

        public IEnumerable<string> GetPlaceNameLst(decimal invID)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(IEnumerable<string>), IsOut = true };
            var invIDParam = new TransmitterParam { Name = "invID", Type = typeof(decimal), IsOut = false, Value = invID };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "GetPlaceNameLst", new[] { resultParam, invIDParam });
            ProcessTelegramm(telegram);
            return (IEnumerable<string>)resultParam.Value;
        }

        #endregion . Инвентаризация .

        #region . СТН .

        // Занесения данных в СТН
        public void InsWtv(decimal? productId, decimal? diff, decimal? comactId, decimal? transact = null, IEnumerable<decimal> stnIdLst = null)
        {
            var productIdParam = new TransmitterParam { Name = "productId", Type = typeof(decimal), IsOut = false, Value = productId };
            var diffParam = new TransmitterParam { Name = "diff", Type = typeof(decimal), IsOut = false, Value = diff };
            var comactIdParam = new TransmitterParam { Name = "comactId", Type = typeof(decimal), IsOut = false, Value = comactId };
            var transactParam = new TransmitterParam { Name = "transact", Type = typeof(decimal), IsOut = false, Value = transact };
            var stnIdLstParam = new TransmitterParam { Name = "stnIdLst", Type = typeof(IEnumerable<decimal>), IsOut = false, Value = stnIdLst };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "InsWtv", new[] { productIdParam, diffParam, comactIdParam, transactParam, stnIdLstParam });
            ProcessTelegramm(telegram);
        }

        #endregion

        #region . Корректировка единицы измерения .
        //Изменение ОВХ SKU
        public string ChangeSKUAndRecalculationTE(List<SKU> skuList)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(string), IsOut = true };
            var skuListParam = new TransmitterParam { Name = "skuList", Type = typeof(List<SKU>), IsOut = false, Value = skuList };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "ChangeSKUAndRecalculationTE", new[] { resultParam, skuListParam });
            ProcessTelegramm(telegram);
            return (string)resultParam.Value;
        }

        public void RecalculationTE(SKU sku)
        {
            var skuParam = new TransmitterParam { Name = "sku", Type = typeof(SKU), IsOut = false, Value = sku };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "RecalculationTE", new[] { skuParam });
            ProcessTelegramm(telegram);
        }

        #endregion . Корректировка единицы измерения .

        #region . RCL .
        //Количество товара на ТЕ
        public void GetProductQuantityOnTe(string teCode, decimal skuId, out decimal skuQuantity2Te, out decimal skuQuantity2TeMax)
        {
            skuQuantity2Te = 0;
            skuQuantity2TeMax = 0;
            var teCodeParam = new TransmitterParam { Name = "teCode", Type = typeof(string), IsOut = false, Value = teCode };
            var skuIdParam = new TransmitterParam { Name = "skuId", Type = typeof(decimal), IsOut = false, Value = skuId };
            var skuQuantity2TeParam = new TransmitterParam { Name = "skuQuantity2Te", Type = typeof(decimal), IsOut = true, Value = skuQuantity2Te };
            var skuQuantity2TeMaxParam = new TransmitterParam { Name = "skuQuantity2TeMax", Type = typeof(decimal), IsOut = true, Value = skuQuantity2TeMax };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "GetProductQuantityOnTe", new[] { teCodeParam, skuIdParam, skuQuantity2TeParam, skuQuantity2TeMaxParam });
            ProcessTelegramm(telegram);
            var decimaltype = typeof(decimal);
            if (skuQuantity2TeParam.Value != null)
                skuQuantity2Te = (decimal)SerializationHelper.ConvertToTrueType(skuQuantity2TeParam.Value, decimaltype);
            if (skuQuantity2TeMaxParam.Value != null)
                skuQuantity2TeMax =
                    (decimal)SerializationHelper.ConvertToTrueType(skuQuantity2TeMaxParam.Value, decimaltype);
        }

        //Осталось принять товара на ТЕ из груза
        public decimal GetTeProductQuantityFromCargoIwb(string operationCode, decimal cargoIwbId, IWBPosInput posInput, bool raiseError, out decimal baseSkuCount, out decimal productCount)
        {
            baseSkuCount = decimal.Zero;
            productCount = decimal.Zero;
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var cargoIwbIdParam = new TransmitterParam { Name = "cargoIwbId", Type = typeof(decimal), IsOut = false, Value = cargoIwbId };
            var posInputParam = new TransmitterParam { Name = "posInput", Type = typeof(IWBPosInput), IsOut = false, Value = posInput };
            var raiseErrorParam = new TransmitterParam { Name = "raiseError", Type = typeof(bool), IsOut = false, Value = raiseError };
            var baseSkuCountParam = new TransmitterParam { Name = "baseSkuCount", Type = typeof(decimal), IsOut = true, Value = baseSkuCount };
            var productCountParam = new TransmitterParam { Name = "productCount", Type = typeof(decimal), IsOut = true, Value = productCount };
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(decimal), IsOut = true };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "GetTeProductQuantityFromCargoIwb", new[]
                {
                    operationCodeParam, cargoIwbIdParam, posInputParam, raiseErrorParam, baseSkuCountParam,
                    productCountParam, resultParam
                });

            ProcessTelegramm(telegram);
            var decimaltype = typeof(decimal);
            if (baseSkuCountParam.Value != null)
                baseSkuCount = (decimal)SerializationHelper.ConvertToTrueType(baseSkuCountParam.Value, decimaltype);
            if (productCountParam.Value != null)
                productCount = (decimal)SerializationHelper.ConvertToTrueType(productCountParam.Value, decimaltype);
            return (decimal)SerializationHelper.ConvertToTrueType(resultParam.Value, decimaltype);
        }
        #endregion . RCL .

        #region . System .
        //Получить дату и время
        public DateTime GetSystemDate()
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(DateTime), IsOut = true };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "GetSystemDate", new[] { resultParam });
            ProcessTelegramm(telegram);
            return (DateTime)SerializationHelper.ConvertToTrueType(resultParam.Value, typeof(DateTime));
        }

        public virtual string GetSdclEndPoint(string clientCode, ref string prevServiceCode)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(string), IsOut = true };
            var clientCodeParam = new TransmitterParam { Name = "clientCode", Type = typeof(string), IsOut = false, Value = clientCode };
            var prevServiceCodeParam = new TransmitterParam { Name = "prevServiceCode", Type = typeof(string), IsOut = true, Value = prevServiceCode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "GetSdclEndPoint", new[] { resultParam, clientCodeParam, prevServiceCodeParam });
            ProcessTelegramm(telegram);
            prevServiceCode = prevServiceCodeParam.Value.To<string>();
            return resultParam.Value.To<string>();
        }
        
        //Запустить архивирование (по конфигурации)
        public void ProcessArch(string archCode)
        {
            var archCodeParam = new TransmitterParam { Name = "archCode", Type = typeof(string), IsOut = false, Value = archCode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "ProcessArch", new[] { archCodeParam });
            ProcessTelegramm(telegram);
        }

        #endregion . System .

        #region . Общие .

        public int CheckInstanceEntity(string entity, string key)
        {
            var pEntityParam = new TransmitterParam { Name = "entity", Type = typeof(string), IsOut = false, Value = entity };
            var pKeyParam = new TransmitterParam { Name = "key", Type = typeof(string), IsOut = false, Value = key };
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(int), IsOut = true };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CheckInstanceEntity", new[] { pEntityParam, pKeyParam, resultParam });
            ProcessTelegramm(telegram);
            return Convert.ToInt32(resultParam.Value);
        }

        #endregion

        #region . Сущности .

        public int IsVirtualTE(string teCode, string teTypeCode = null)
        {
            var teCodeParam = new TransmitterParam { Name = "teCode", Type = typeof(string), IsOut = false, Value = teCode };
            var teTypeCodeParam = new TransmitterParam { Name = "teTypeCode", Type = typeof(string), IsOut = false, Value = teTypeCode };
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(int), IsOut = true };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "IsVirtualTE", new[] { teCodeParam, teTypeCodeParam, resultParam });
            ProcessTelegramm(telegram);
            return Convert.ToInt32(resultParam.Value);
        }

        public bool IsMonoTE(string teCode)
        {
            var teCodeParam = new TransmitterParam { Name = "teCode", Type = typeof(string), IsOut = false, Value = teCode };
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(bool), IsOut = true };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "IsMonoTE", new[] { teCodeParam, resultParam });
            ProcessTelegramm(telegram);
            return Convert.ToBoolean(resultParam.Value);
        }

        #endregion

        #region  .  Работа  .

        public void GetDateFrDateTill(string entity, string key, string operationCode, decimal? workerId, out Working working)
        {
            var entityParam = new TransmitterParam { Name = "entity", Type = typeof(string), IsOut = false, Value = entity };
            var keyParam = new TransmitterParam { Name = "key", Type = typeof(string), IsOut = false, Value = key };
            var operationParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var workerIdParam = new TransmitterParam { Name = "worker", Type = typeof(decimal?), IsOut = false, Value = workerId };
            var workingParam = new TransmitterParam { Name = "working", Type = typeof(Working), IsOut = true };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "GetDateFrDateTill", new[] { entityParam, keyParam, operationParam, workerIdParam, workingParam });
            ProcessTelegramm(telegram);
            working = (Working)workingParam.Value;
        }
        
        // Завершение работы
        public void WorkComleted(decimal workId, string operation, DateTime? workTill)
        {
            var workIdParam = new TransmitterParam { Name = "workId", Type = typeof(decimal), IsOut = false, Value = workId };
            var operationParam = new TransmitterParam { Name = "operation", Type = typeof(string), IsOut = false, Value = operation };
            var workTillParam = new TransmitterParam { Name = "workTill", Type = typeof(DateTime?), IsOut = false, Value = workTill };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "WorkComleted", new[] { workIdParam, operationParam, workTillParam });
            ProcessTelegramm(telegram);
        }

        // Начать работу, создать выполнение
        public void StartWorking(string entity, string key, string operationCode, decimal? workerId, decimal? mandantID, DateTime? workingFrom, string workingDoc, out Work work)
        {
            var entityParam = new TransmitterParam { Name = "entity", Type = typeof(string), IsOut = false, Value = entity };
            var keyParam = new TransmitterParam { Name = "key", Type = typeof(string), IsOut = false, Value = key };
            var operationParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var workerIdParam = new TransmitterParam { Name = "workerId", Type = typeof(decimal?), IsOut = false, Value = workerId };
            var mandantIDParam = new TransmitterParam { Name = "mandantID", Type = typeof(decimal?), IsOut = false, Value = mandantID };
            var workingFromParam = new TransmitterParam { Name = "workingFrom", Type = typeof(DateTime?), IsOut = false, Value = workingFrom };
            var workingDocParam = new TransmitterParam { Name = "workingDoc", Type = typeof(string), IsOut = false, Value = operationCode };
            var workParam = new TransmitterParam { Name = "work", Type = typeof(Work), IsOut = true };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "StartWorking", new[] { entityParam, keyParam, operationParam, workerIdParam, mandantIDParam, workingFromParam, workingDocParam, workParam });
            ProcessTelegramm(telegram);
            work = (Work)workParam.Value;
        }

        // Cоздать выполнения работ
        public void StartWorkings(decimal workId, string truckCode, decimal myWorkerId, IEnumerable<decimal> workerIds, DateTime? workingFrom)
        {
            var workIdParam = new TransmitterParam { Name = "workId", Type = typeof(decimal), IsOut = false, Value = workId };
            var truckCodeParam = new TransmitterParam { Name = "truckCode", Type = typeof(string), IsOut = false, Value = truckCode };
            var myWorkerIdParam = new TransmitterParam { Name = "myWorkerId", Type = typeof(decimal), IsOut = false, Value = myWorkerId };
            var workerIdsParam = new TransmitterParam { Name = "workerIds", Type = typeof(IEnumerable<decimal>), IsOut = false, Value = workerIds };
            var workingFromParam = new TransmitterParam { Name = "workingFrom", Type = typeof(DateTime?), IsOut = false, Value = workingFrom };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "StartWorkings", new[] { workIdParam, truckCodeParam, myWorkerIdParam, workerIdsParam, workingFromParam });
            ProcessTelegramm(telegram);
        }

        // Завершить выполнения работ
        public void CompleteWorking(string entity, string key, string operationCode, decimal? workerId)
        {
            var entityParam = new TransmitterParam { Name = "entity", Type = typeof(string), IsOut = false, Value = entity };
            var keyParam = new TransmitterParam { Name = "key", Type = typeof(string), IsOut = false, Value = key };
            var operationParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var workerIdParam = new TransmitterParam { Name = "workerId", Type = typeof(decimal?), IsOut = false, Value = workerId };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CompleteWorking", new[] { entityParam, keyParam, operationParam, workerIdParam });
            ProcessTelegramm(telegram);
        }

        // Завершить все выполнения работ данного работника
        public void CompleteWorkings(IEnumerable<decimal> workingIds, DateTime? dateTill)
        {
            var workingIdsParam = new TransmitterParam { Name = "workingIds", Type = typeof(IEnumerable<decimal>), IsOut = false, Value = workingIds };
            var dateTillParam = new TransmitterParam { Name = "dateTill", Type = typeof(DateTime?), IsOut = false, Value = dateTill };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CompleteWorkings", new[] { workingIdsParam, dateTillParam });
            ProcessTelegramm(telegram);
        }

        // Сменить статус работы
        public void ChangeWorkStatus(decimal workId, string operation)
        {
            var workIdParam = new TransmitterParam { Name = "workId", Type = typeof(decimal), IsOut = false, Value = workId };
            var operationParam = new TransmitterParam { Name = "operation", Type = typeof(string), IsOut = false, Value = operation };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "ChangeWorkStatus", new[] { workIdParam, operationParam });
            ProcessTelegramm(telegram);
        }

        // Получить работу по операции
        public Work GetWorkByOperation(string entity, string key, string operationCode)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(Work), IsOut = true };
            var entityParam = new TransmitterParam { Name = "entity", Type = typeof(string), IsOut = false, Value = entity };
            var keyParam = new TransmitterParam { Name = "key", Type = typeof(string), IsOut = false, Value = key };
            var operationParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "GetWorkByOperation", new[] { resultParam, entityParam, keyParam, operationParam });
            ProcessTelegramm(telegram);
            return resultParam.Value as Work;
        }

        #endregion

        #region  .  Мандант  .

        public int? ChkJob(string jobName, string mandantCode)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(int?), IsOut = true };
            var jobNameParam = new TransmitterParam { Name = "jobName", Type = typeof(string), IsOut = false, Value = jobName };
            var mandantCodeParam = new TransmitterParam { Name = "mandantCode", Type = typeof(string), IsOut = false, Value = mandantCode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "ChkJob", new[] { resultParam, jobNameParam, mandantCodeParam });
            ProcessTelegramm(telegram);
            return (int?)SerializationHelper.ConvertToTrueType(resultParam.Value, typeof(int?));
        }

        public void CreJob(string jobName, string mandantCode, int interval)
        {
            var jobNameParam = new TransmitterParam { Name = "jobName", Type = typeof(string), IsOut = false, Value = jobName };
            var mandantCodeParam = new TransmitterParam { Name = "mandantCode", Type = typeof(string), IsOut = false, Value = mandantCode };
            var intervalParam = new TransmitterParam { Name = "interval", Type = typeof(int), IsOut = false, Value = interval };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "CreJob", new[] { jobNameParam, mandantCodeParam, intervalParam });
            ProcessTelegramm(telegram);
        }
        #endregion

        #region . CPV .
        public object GetCpvValue(string entity, string key, string cpvcode)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(object), IsOut = true };
            var entityParam = new TransmitterParam { Name = "entity", Type = typeof(string), IsOut = false, Value = entity };
            var keyParam = new TransmitterParam { Name = "key", Type = typeof(string), IsOut = false, Value = key };
            var cpvcodeParam = new TransmitterParam { Name = "cpvcode", Type = typeof(string), IsOut = false, Value = cpvcode };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "GetCpvValue", new[] { resultParam, entityParam, keyParam, cpvcodeParam });
            ProcessTelegramm(telegram);
            return resultParam.Value;
        }

        public void SaveTirCpvs(IEnumerable<CustomParamValue> cpvs, IEnumerable<decimal> mandantids)
        {
            var cpvsParam = new TransmitterParam { Name = "cpvs", Type = typeof(IEnumerable<CustomParamValue>), IsOut = false, Value = cpvs };
            var mandantidsParam = new TransmitterParam { Name = "mandantids", Type = typeof(IEnumerable<decimal>), IsOut = false, Value = mandantids };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "SaveTirCpvs", new[] { cpvsParam, mandantidsParam });
            ProcessTelegramm(telegram);
        }

        public void DeleteTirCpvs(IEnumerable<string> iwbids)
        {
            var iwbidsParam = new TransmitterParam { Name = "iwbids", Type = typeof(IEnumerable<string>), IsOut = false, Value = iwbids };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "DeleteTirCpvs", new[] { iwbidsParam });
            ProcessTelegramm(telegram);
        }

        public void DeleteCpvsByEntityByCodeByKey(string entity, IEnumerable<string> codes, IEnumerable<string> keys)
        {
            var entityParam = new TransmitterParam { Name = "entity", Type = typeof(string), IsOut = false, Value = entity };
            var codesParam = new TransmitterParam { Name = "codes", Type = typeof(IEnumerable<string>), IsOut = false, Value = codes };
            var keysParam = new TransmitterParam { Name = "keys", Type = typeof(IEnumerable<string>), IsOut = false, Value = keys };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "DeleteCpvsByEntityByCodeByKey", new[] { entityParam, codesParam, keysParam });
            ProcessTelegramm(telegram);
        }
        #endregion . CPV .
        #endregion .  API DB  .

        #region . Configurator .
        // Получение данных для Configurator'а
        public void GetPmConfiguratorData(ref IEnumerable<BillOperation> operations, ref IEnumerable<decimal> entityids,
            ref IEnumerable<SysObject> attributes,
            ref IEnumerable<PM> pms, ref IEnumerable<PMMethod> pmMethods,
            ref IEnumerable<PMMethod2Operation> detailsPmMethod, ref DataTable pmdata, ref DataTable pmMethod2OperationsAllowed)
        {
            var operationsParam = new TransmitterParam { Name = "operations", Type = typeof(IEnumerable<BillOperation>), IsOut = true, Value = operations };
            var entityidsParam = new TransmitterParam { Name = "entityids", Type = typeof(IEnumerable<decimal>), IsOut = true, Value = entityids };
            var attributesParam = new TransmitterParam { Name = "attributes", Type = typeof(IEnumerable<SysObject>), IsOut = true, Value = attributes };
            var pmsParam = new TransmitterParam { Name = "pms", Type = typeof(IEnumerable<PM>), IsOut = true, Value = pms };
            var pmMethodsParam = new TransmitterParam { Name = "pmMethods", Type = typeof(IEnumerable<PMMethod>), IsOut = true, Value = pmMethods };
            var detailsPmMethodParam = new TransmitterParam { Name = "detailsPmMethod", Type = typeof(IEnumerable<PMMethod2Operation>), IsOut = true, Value = detailsPmMethod };
            var pmdataParam = new TransmitterParam { Name = "pmdata", Type = typeof(DataTable), IsOut = true, Value = pmdata };
            var pmMethod2OperationsAllowedParam = new TransmitterParam { Name = "pmMethod2OperationsAllowed", Type = typeof(DataTable), IsOut = true, Value = pmMethod2OperationsAllowed };
            var telegram = new RepoQueryTelegramWrapper(typeof (BPProcess).Name, "GetPmConfiguratorData",
                new[]
                {
                    operationsParam, entityidsParam, attributesParam, pmsParam, pmMethodsParam, detailsPmMethodParam,
                    pmdataParam, pmMethod2OperationsAllowedParam
                });
            ProcessTelegramm(telegram);
            operations = (IEnumerable<BillOperation>) operationsParam.Value;
            entityids = (IEnumerable<decimal>) entityidsParam.Value;
            attributes = (IEnumerable<SysObject>) attributesParam.Value;
            pms = (IEnumerable<PM>) pmsParam.Value;
            pmMethods = (IEnumerable<PMMethod>) pmMethodsParam.Value;
            detailsPmMethod = (IEnumerable<PMMethod2Operation>) detailsPmMethodParam.Value;
            pmdata = (DataTable) pmdataParam.Value;
            pmMethod2OperationsAllowed = (DataTable) pmMethod2OperationsAllowedParam.Value;
        }

        public List<string> SavePmConfiguratorData(ICollection<PM2Operation> pm2Operations,
            ICollection<PMConfig> pmConfigs, ICollection<PM2Operation> deletePm2Operations,
            ICollection<PMConfig> deletePmConfigs)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(List<string>), IsOut = true };
            var pm2OperationsParam = new TransmitterParam { Name = "pm2Operations", Type = typeof(ICollection<PM2Operation>), IsOut = false, Value = pm2Operations };
            var pmConfigsParam = new TransmitterParam { Name = "pmConfigs", Type = typeof(ICollection<PMConfig>), IsOut = false, Value = pmConfigs };
            var deletePm2OperationsParam = new TransmitterParam { Name = "deletePm2Operations", Type = typeof(ICollection<PM2Operation>), IsOut = false, Value = deletePm2Operations };
            var deletePmConfigsParam = new TransmitterParam { Name = "deletePmConfigs", Type = typeof(ICollection<PMConfig>), IsOut = false, Value = deletePmConfigs };
            var telegram = new RepoQueryTelegramWrapper(typeof (BPProcess).Name, "SavePmConfiguratorData",
                new[] {resultParam, pm2OperationsParam, pmConfigsParam, deletePm2OperationsParam, deletePmConfigsParam});
            ProcessTelegramm(telegram);
            return (List<string>) resultParam.Value;
        }

        public void DeletePmConfiguratorData(ICollection<PM2Operation> pm2Operations, ICollection<PMConfig> pmConfigs)
        {
            var pm2OperationsParam = new TransmitterParam { Name = "pm2Operations", Type = typeof(ICollection<PM2Operation>), IsOut = false, Value = pm2Operations };
            var pmConfigsParam = new TransmitterParam { Name = "pmConfigs", Type = typeof(ICollection<PMConfig>), IsOut = false, Value = pmConfigs };
            var telegram = new RepoQueryTelegramWrapper(typeof (BPProcess).Name, "DeletePmConfiguratorData", new[] {pm2OperationsParam, pmConfigsParam});
            ProcessTelegramm(telegram);
        }
        #endregion . Configurator .

        [Cache]
        public virtual List<SKU> GetSKUWithCache(string filter, string attrEntity)
        {
            var filterParam = new TransmitterParam { Name = "filter", Type = typeof(string), IsOut = false, Value = filter };
            var attrEntityParam = new TransmitterParam { Name = "attrEntity", Type = typeof(string), IsOut = false, Value = attrEntity };
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(List<SKU>), IsOut = true };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "GetSKUWithCache", new[] { filterParam, attrEntityParam, resultParam });
            ProcessTelegramm(telegram);
            return (List<SKU>)resultParam.Value;

        }
    }
}
