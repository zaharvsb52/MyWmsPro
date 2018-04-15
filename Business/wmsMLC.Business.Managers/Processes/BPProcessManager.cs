using System;
using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using wmsMLC.Business.DAL;
using wmsMLC.Business.General;
using wmsMLC.Business.Objects;
using wmsMLC.Business.Objects.Processes;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;
using wmsMLC.General.Services;

namespace wmsMLC.Business.Managers.Processes
{
    public class BPProcessManager : WMSBusinessObjectManager<BPProcess, string>, IBPProcessManager, ISdclConnectInfoProvider
    {
        #region . Properties .

        public Dictionary<string, object> Parameters { get; set; }

        #endregion

        #region .  Constructors & Destructor  .

        public BPProcessManager()
        {
            Parameters = new Dictionary<string, object>();
        }

        #endregion

        #region . Methods .

        public void Run(string code, Action<CompleteContext> completedHandler = null)
        {
            var process = Get(code);
            if (process == null)
                throw new OperationException("Не найден процесс с именем '{0}'", code);

            Run(bpProcess: process, completedHandler: completedHandler);
        }

        public void Run(BPProcess bpProcess, Action<CompleteContext> completedHandler = null)
        {
            if (bpProcess.Disable)
                throw new OperationException("Процесс '{0}' запрещен к выполнению", bpProcess.Name);

            var executor = IoC.Instance.Resolve<IProcessExecutor>(bpProcess.Executor);
            if (executor == null)
                throw new DeveloperException("Executor not registered");

            var context = new ExecutionContext(bpProcess, Parameters);
            executor.Run(context: context, completedHandler: completedHandler);
        }

        public IEnumerable<BPProcess> GetForEntity(Type entityType)
        {
            return null;
        }

        public bool ValidateProcessCode(string processCode)
        {
            return Get(processCode, GetModeEnum.Partial) != null;
        }
        #endregion  . Methods .

        #region . API_BusinessProcesses .

        #region . ВРЕМЕННЫЕ .

        [BP, DisplayName(@"Подобрано")]
        public void OWBPicked(decimal owbid, string placecode, decimal notSamePlaceType)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.OWBPicked(owbid, placecode, notSamePlaceType);
        }

        [BP, DisplayName(@"Статус упаковки расходной накладной")]
        public void GetOWBBPStatus(decimal key, out string status)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.GetOWBBPStatus(key, out status);
        }

        #endregion

        #region . Приемка .

        [BP, DisplayName(@"Активация приходной накладной")]
        public void ActivateIWB(decimal key)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.ActivateIWB(key);
        }

        [BP, DisplayName(@"Создать товар по накладной")]
        public List<Product> CreateProductByPos(ref string manageFlag, ref string manageFlagParam, string operationCode, decimal iwbId, IWBPosInput posInput, int isMigrating = 0, string placeCode = null)
        {
            List<Product> result;
            using (var repo = GetRepository<IBPProcessRepository>())
                result = repo.CreateProductByPos(ref manageFlag, ref manageFlagParam, operationCode, iwbId, posInput, isMigrating, placeCode);
            return result;
        }

        [BP, DisplayName(@"Создать товар по грузу")]
        public List<Product> CreateProductByCargoIwb(ref string manageFlag, ref string manageFlagParam, string operationCode, IWBPosInput posInput, string placeCode, decimal cargoIwbId, int isMigrating = 0, decimal? iwbId = null)
        {
            List<Product> result;
            using (var repo = GetRepository<IBPProcessRepository>())
                result = repo.CreateProductByCargoIwb(ref manageFlag, ref manageFlagParam, operationCode, posInput, placeCode, cargoIwbId, isMigrating, iwbId);
            return result;
        }

        [BP, DisplayName(@"Отмена приемки")]
        public void CancelIwbAccept(decimal iwbid)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CancelIwbAccept(iwbid);
        }

        [BP, DisplayName(@"Частичная отмена приемки")]
        public void CancelProductAccept(decimal? iwbid, IEnumerable<decimal> products, bool isAllTe, decimal? workid)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CancelProductAccept(iwbid, products, isAllTe, workid);
        }

        [BP, DisplayName(@"Закрыть приход")]
        public void FinishIwb(IEnumerable<decimal> iwbs, string operationCode, decimal? comactId)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.FinishIwb(iwbs, operationCode, comactId);
        }

        [BP, DisplayName(@"Создать виртуальные позиции")]
        public List<IWBPosInput> GetIWBPosInputLst(IEnumerable<IWBPos> iwbPosList, int isMigrating = 0)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.GetIWBPosInputLst(iwbPosList, isMigrating);
        }

        [BP, DisplayName(@"Расфиксировать")]
        public void UnfixedIWB(decimal iwbid, string operationcode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.UnfixedIWB(iwbid, operationcode);
        }

        [BP, DisplayName(@"Получить коэффициент пересчета SKU")]
        public decimal ConvertSKUtoSKU(decimal sourceSkuId, decimal destSkuId, int isPrd, decimal? oldqty)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.ConvertSKUtoSKU(sourceSkuId, destSkuId, isPrd, oldqty);
        }

        [BP, DisplayName(@"Получить код алгоритма разбора batch-кода")]
        public BPBatch GetDefaultBatchCode(decimal? mandantID, decimal? sKUID, string artCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.GetDefaultBatchCode(mandantID, sKUID, artCode);
        }

        [BP, DisplayName(@"Получить черный артикул")]
        public IWBPosInput GetBlackArt(CargoIWBPos cargoIwbPos, string teCode, string qlfCode, string iwbType, decimal mandantId)
        {
            IWBPosInput result = null;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<SKU>>())
            {
                //NOTE: Функцией пользоваться нельзя, т.к. это сильно замедляет работу
                //var skuList = mgr.GetFiltered(string.Format("mandantid = {0} and pkgBpInput.isStorageUnit(skuid,'{1}') = 1", mandantId, iwbType)).ToArray();
                var attrEntity = FilterHelper.GetAttrEntity<SKU>(SKU.SKUIDPropertyName, SKU.ArtCodePropertyName);
                var filter = string.Format("mandantid = {0} and skuid in (select s.skuid from wmssku s inner join wmsart a on a.ARTCODE = s.ARTCODE_R where a.artiwbtype = '{1}')", mandantId, iwbType);
                var skuList = mgr.GetFiltered(filter, attrEntity).ToArray();
                if (skuList.Length == 0)
                    throw new OperationException("Для манданта '{0}' по типу приемки '{1}' нет соответствующего 'черного' артикула", mandantId, iwbType);

                if (skuList.Length > 1)
                    throw new OperationException("Для манданта '{0}' типу приемки '{1}' соответствует более одной единицы учета: '{2}'", mandantId, iwbType, string.Join(",", skuList.Select(i => i.SKUID)));

                var boxParamExist = GetPMConfigByParamListByArtCode(skuList[0].ArtCode, BillOperationCode.OP_INPUT_SO.ToString(), null).Any(i => i.ObjectName_r.EqIgnoreCase(IWBPos.IWBPosBoxNumberPropertyName));
                if (!boxParamExist)
                    throw new OperationException("Для манданта '{0}' по типу приемки '{1}' отсутствуют настройки менеджера товара для приемки объекта хранения", mandantId, iwbType);
                var count = cargoIwbPos.GetProperty<decimal>(CargoIWBPos.CARGOIWBPOSCOUNTPropertyName);
                result = new IWBPosInput
                {
                    SKUID = skuList[0].SKUID,
                    IWBPosCount = (double)count,
                    QLFCODE_R = qlfCode,
                    IwbPosBoxNumber = cargoIwbPos.GetProperty<string>(CargoIWBPos.CARGOIWBPOSBOXNUMBERPropertyName),
                    IWBPosTE = teCode,
                    RequiredSKUCount = count
                };
            }
            return result;
        }

        [BP, DisplayName(@"Получить настройки менеджера приемки")]
        public IEnumerable<decimal> GetMinConfig4IwbList(IEnumerable<decimal> iwbIdLst, string cpCode, string cpValue)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.GetMinConfig4IwbList(iwbIdLst, cpCode, cpValue);
        }

        [BP, DisplayName(@"Получить выбор менеджера приемки")]
        public decimal? GetDefaultMIN(decimal iwbId)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.GetDefaultMIN(iwbId);
        }

        /// <summary> Удаление объекта хранения по продукту </summary>
        /// <param name="productId">id продукта объекта хранения</param>
        [BP, DisplayName(@"Удалить объекты хранения")]
        public virtual void RemoveStorageUnit(IEnumerable<decimal> productIdList)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.RemoveStorageUnit(productIdList);
        }

        [BP, DisplayName(@"Удаление приходной накладной")]
        public virtual void CascadeDeleteIWB(decimal iwbId)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CascadeDeleteIWB(iwbId);
        }

        #endregion

        #region . Перемещение .

        [BP, DisplayName(@"Блокировать область")]
        public void BlockArea(string key, string blockingCode, string description, out decimal blockingID)
        {
            // TODO Здесь должна быть проверка прав на блокировку
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.BlockArea(key, blockingCode, description, out blockingID);
            //RaiseManagerChanged<Area2Blocking>();
        }

        [BP, DisplayName(@"Блокировать ТЕ")]
        public void BlockTE(string key, string blockingCode, string description, out decimal blockingID)
        {
            // TODO Здесь должна быть проверка прав на блокировку
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.BlockTE(key, blockingCode, description, out blockingID);
            //RaiseManagerChanged<TE2Blocking>();
        }

        [BP, DisplayName(@"Блокировать место")]
        public void BlockPlace(string key, string blockingCode, string description, out decimal blockingID)
        {
            // TODO Здесь должна быть проверка прав на блокировку
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.BlockPlace(key, blockingCode, description, out blockingID);
            //RaiseManagerChanged<Place2Blocking>();
        }

        [BP, DisplayName(@"Блокировать сектор")]
        public void BlockSegment(string key, string blockingCode, string description, out decimal blockingID)
        {
            // TODO Здесь должна быть проверка прав на блокировку
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.BlockSegment(key, blockingCode, description, out blockingID);
            //RaiseManagerChanged<Segment2Blocking>();
        }

        [BP, DisplayName(@"Получение мест для ЗНТ")]
        public List<Place> GetPlaceLstByStrategy(string strategy, string sourceTECode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.GetPlaceLstByStrategy(strategy, sourceTECode);
        }

        [BP, DisplayName(@"Создание ЗНТ")]
        public void CreateTransportTask(string sourceTECode, string destPlaceCode, string strategy, string destTECode, out decimal transportTaskID, decimal? productID)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CreateTransportTask(sourceTECode, destPlaceCode, strategy, destTECode, out transportTaskID, productID);
            //RaiseManagerChanged<TE>();
            //RaiseManagerChanged<TransportTask>();
            //RaiseManagerChanged<Place>();
        }

        [BP, DisplayName(@"Создание нескольких ЗНТ")]
        public List<TransportTask> MoveManyTE2Place(IEnumerable<TE> teList, string destPlaceCode, string strategy, string destTECode, IEnumerable<Product> productList, int isManual)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.MoveManyTE2Place(teList, destPlaceCode, strategy, destTECode, productList, isManual);
        }

        /// <summary>
        /// Получение Места и стратегии для ТЕ
        /// </summary>
        /// <param name="MMCode">код менеджера (может быть пустым)</param>
        /// <param name="SourceTeCode">код ТЕ</param>
        /// <param name="MMStrategy">стратегия перемещения</param>
        /// <param name="OrderClause">условия сортировки мест (может быть пустым)</param>
        /// <param name="PlaceCode">код места</param>
        [BP, DisplayName(@"Получение Места для ТЕ")]
        public void BpGetPlaceByMM(string MMCode, string SourceTeCode, ref string MMStrategy, string OrderClause, out string PlaceCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.BpGetPlaceByMM(MMCode, SourceTeCode, ref MMStrategy, OrderClause, out PlaceCode);
        }

        // Переехало в WmsAPI
        //[BP, DisplayName(@"Получить количество доступных ЗНТ")]
        //public void GetAvailableTransportTaskCount(string filter, out int count)
        //{
        //    using (var repo = GetRepository<IBPProcessRepository>())
        //        repo.GetAvailableTransportTaskCount(filter, out count);
        //}

        [BP, DisplayName(@"Создать виртуальные ТЕ")]
        public void CreateVirtualTE(IEnumerable<string> places, string teTypeCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CreateVirtualTE(places, teTypeCode);
        }

        /// <summary>
        /// Получаем и резервируем за клиентом ЗНТ. Если currentTtaskCode != null, то получаем и резервируем следующую за currentTtaskCode ЗНТ.
        /// </summary>
        [BP, DisplayName(@"Зарезервировать ЗНТ")]
        public TransportTask ReserveTransportTask(decimal? currentTransportTaskCode, string filter)
        {
            TransportTask result;
            using (var repo = GetRepository<IBPProcessRepository>())
                result = repo.ReserveTransportTask(currentTransportTaskCode, filter);
            return result;
        }

        [BP, DisplayName(@"Активировать или резервировать ЗНТ")]
        public virtual void ReserveOrActivateTransportTask(decimal tTaskId, string clientCode, string truckCode, DateTime? taskBegin, BillOperationCode operationCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.ReserveOrActivateTransportTask(tTaskId, clientCode, truckCode, taskBegin, operationCode);
        }

        [BP, DisplayName(@"Освободить зарезервированную ЗНТ")]
        public void ResetTransportTask(decimal currentTransportTaskCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.ResetTransportTask(currentTransportTaskCode);
        }

        [BP, DisplayName(@"Квитировать ЗНТ")]
        public void CompleteTransportTask(IEnumerable<decimal> tTaskIDLst, decimal? workerCode, string truckCode, DateTime? startDate, DateTime? endDate, string teTypeCode, IEnumerable<string> teCodeLst, bool isNotNeededCreateWork)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CompleteTransportTask(tTaskIDLst, workerCode, truckCode, startDate, endDate, teTypeCode, teCodeLst, isNotNeededCreateWork);
        }

        [BP, DisplayName(@"Отмена ЗНТ")]
        public void CancelTransportTask(decimal Key, string operationCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CancelTransportTask(Key, operationCode);
        }

        [BP, DisplayName(@"Получение стратегии перемещения")]
        public void GetPlaceStrategy(decimal productId, string placeCode, decimal raiseError, out decimal strategy)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.GetPlaceStrategy(productId, placeCode, raiseError, out strategy);
        }

        [BP, DisplayName(@"Переместить ТЕ")]
        public void MoveTe(string teCode, out decimal tTaskId)
        {
            var strategy = string.Empty;
            string placeCode;
            BpGetPlaceByMM(null, teCode, ref strategy, null, out placeCode);
            if (string.IsNullOrEmpty(strategy) || string.IsNullOrEmpty(placeCode))
                throw new OperationException("Проверьте настройки менеджера перемещения");
            CreateTransportTask(teCode, placeCode, strategy, null, out tTaskId, null);
        }

        [BP, DisplayName(@"Получить ТЕ для загруза")]
        public string FindTargetTE(string sourceTECode, string destPlaceCode, decimal? productId, decimal? raiseError, string destTECode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.FindTargetTE(sourceTECode, destPlaceCode, productId, raiseError, destTECode);
        }

        [BP, DisplayName(@"Получить несущую ТЕ")]
        public string FindCarrierTE(string sourceTECode, string destPlaceCode, string strategy, decimal? productId, decimal checkActiveTt,string destTECode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.FindCarrierTE(sourceTECode, destPlaceCode, strategy, productId, checkActiveTt, destTECode);
        }

        [BP, DisplayName(@"Перемещение товара по SKU")]
        public void MoveProductsBySku(string sourceTeCode, string destTeCode, decimal skuId, IEnumerable<decimal> productIds, decimal count, string truckCode, bool isNotNeededCreateWork, out decimal transportTaskID)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.MoveProductsBySku(sourceTeCode, destTeCode, skuId, productIds, count, truckCode, isNotNeededCreateWork, out transportTaskID);
        }

        [BP, DisplayName(@"Частичная отмена товара к перемещению")]
        public void CancelMoveProduct(decimal ttaskid, decimal count)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CancelMoveProduct(ttaskid, count);
        }

        [BP, DisplayName(@"Определить стратегию перемещения ТЕ")]
        public string GetDefaultMMStrategy(string TECode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.GetDefaultMMStrategy(TECode);
        }

        [BP, DisplayName(@"Создание ЗНТ дозагруза для автоматического размещения товара")]
        public void CreTransportTaskAuto(string currentPlaceCode, string sourceTECode, IEnumerable<decimal> prdInputs, ref string strategy, IEnumerable<string> skipPlaceLst, out TransportTask trasportTask, out decimal productCount)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CreTransportTaskAuto(currentPlaceCode, sourceTECode, prdInputs, ref strategy, skipPlaceLst, out trasportTask, out productCount);
        }

        [BP, DisplayName(@"Проверки возможности перемещения ТЕ")]
        public decimal CheckTE2Move(string teCode, decimal? productID, decimal raiseError)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.CheckTE2Move(teCode, productID, raiseError);
        }

        [BP, DisplayName(@"Доступные ТЕ для дозагруза")]
        public List<string> GetAvailableTE(string teCode, string placeCode, decimal? productID, decimal productCount)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.GetAvailableTE(teCode, placeCode, productID, productCount);
        }

        [BP, DisplayName(@"Изменить статус места")]
        public void ChangePlaceStatus(string placeCode, string operation)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.ChangePlaceStatus(placeCode, operation);
        }

        #endregion

        #region . Поставки .

        [BP, DisplayName(@"Добавление в очередь поставок")]
        public void CreateQSupplyChain(string operationCode, decimal? mandantID, decimal? resGroup, string tECode, decimal? supplyChainID, decimal? raiseErr, string process)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CreateQSupplyChain(operationCode, mandantID, resGroup, tECode, supplyChainID, raiseErr, process);
        }

        [BP, DisplayName(@"Добавление в очередь поставок и возврат id очереди")]
        public void CreateQSupplyChainTt(string operationCode, decimal? mandantID, decimal? resGroup, string tECode,
            decimal? supplyChainID, decimal? raiseErr, string process, out decimal qSupplyChainID)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CreateQSupplyChainTt(operationCode, mandantID, resGroup, tECode, supplyChainID, raiseErr, process,
                    out qSupplyChainID);
        }

        [BP, DisplayName(@"БП 'Подобрано' (с учетом поставок)")]
        public void OWBPickedBySupplyChain(decimal owbid, string operationCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.OWBPickedBySupplyChain(owbid, operationCode);
        }

        [BP, DisplayName(@"Сформировать группу резервирования")]
        public void CreateResGroup(string entity, string key, out decimal resGroup)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CreateResGroup(entity, key, out resGroup);
        }

        [BP, DisplayName(@"Создать поставку")]
        public void CreateSupplyChain(decimal qSupplyChainID, out decimal ttaskID)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CreateSupplyChain(qSupplyChainID, out ttaskID);
        }

        [BP, DisplayName(@"Отменить поставку для ТЕ (с отменой ЗНТ)")]
        public void CancelSupplyChainForTE(string teCode, string operationCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CancelSupplyChainForTE(teCode, operationCode);
        }

        #endregion

        #region . Счетчики ТЕ .
        [BP, DisplayName(@"Диапазон кодов ТЕ по коду счетчика ТЕ")]
        public void GetTeLabelRange(string sequenceCode, int count, out int rangeBegin, out int rangeEnd)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.GetTeLabelRange(sequenceCode, count, out rangeBegin, out rangeEnd);
        }
        #endregion Счетчики ТЕ

        #region . Товар .

        [BP, DisplayName(@"Создать товар по позиции")]
        public void CreateProduct(ref Product product, ref TE te)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CreateProduct(ref product, ref te);
        }

        [BP, DisplayName(@"Получить свойства товара для корректировки")]
        public List<PMConfig> GetProductPropertyForEdit(decimal productid, string operationcode)
        {
            using (var pmconfigmgr = (IPMConfigManager)GetManager<PMConfig>())
                return pmconfigmgr.GetPMConfigByParamListByProductId(productid, operationcode, null);
        }

        [BP, DisplayName(@"Получить свойства артикула для корректировки")]
        public List<PMConfig> GetArtPropertyForEdit(string artcode, string operationcode)
        {
            using (var pmconfigmgr = (IPMConfigManager)GetManager<PMConfig>())
                return pmconfigmgr.GetPMConfigByParamListByArtCode(artcode, operationcode, null);
        }

        [BP, DisplayName(@"Расщепление товара")]
        public void splitProduct(decimal Key, decimal SplitCount, decimal FreeIfBusy, string operationCode, out decimal NewKey)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.splitProduct(Key, SplitCount, FreeIfBusy, operationCode, out NewKey);
        }

        [BP, DisplayName(@"Изменение Товара с повышенными правами")]
        public void UpdateProduct(Product entity)
        {
            EntityCudOperation(entity, CudOperation.Update, false);
        }

        [BP, DisplayName(@"Удаление Товара с повышенными правами")]
        public void DeleteProduct(Product entity)
        {
            EntityCudOperation(entity, CudOperation.Delete, false);
        }

        [BP, DisplayName(@"Изменение Коммерческого акта с повышенными правами")]
        public void UpdateCommAct(CommAct entity)
        {
            EntityCudOperation(entity, CudOperation.Update, false);
        }

        // выставить ЗНТ товару
        [BP, DisplayName(@"Указать ЗНТ для товара")]
        public Product AssignTransportTaskToProduct(Product product, decimal transportTaskId)
        {
            product.TRANSPORTTASKID_R = transportTaskId;
            return EntityCudOperation(product, CudOperation.Update, false) as Product;
        }

        [BP, DisplayName(@"Конвертировать товар")]
        public IEnumerable<decimal> SplitProductWithSKU(decimal productId, decimal skuId, decimal countSku, double countInSku)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.SplitProductWithSKU(productId, skuId, countSku, countInSku);
        }

        [BP, DisplayName(@"Проверка кратности е.у.")]
        public virtual bool IsMultipleSku(decimal skuId, decimal count, IEnumerable<decimal> skuList)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.IsMultipleSku(skuId, count, skuList);
        }

        [BP, DisplayName(@"Разукомплектация")]
        public void DismantleKit(IEnumerable<decimal> productLst, string kitCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
            {
                repo.DismantleKit(productLst,kitCode);
            }
        }


        #endregion

        #region . Резервирование .

        [BP, DisplayName(@"Заменить зарезервированный товар")]
        public void ChangeReserved(decimal oldProductId, decimal newProductId, decimal countNeeded)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.ChangeReserved(oldProductId, newProductId, countNeeded);
        }

        [BP, DisplayName(@"Ручное резервирование")]
        public void ManualReserve(decimal? owbPosId, decimal? owbId, decimal productId, decimal owbProductNeed, out decimal outParam)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.ManualReserve(owbPosId, owbId, productId, owbProductNeed, out outParam);
        }

        [BP, DisplayName(@"Резервировать список накладных")]
        public void ReserveOWBLst(IEnumerable<OWB> owbLst, string operationCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.ReserveOWBLst(owbLst, operationCode);
        }

        [BP, DisplayName(@"Отмена резервирования")]
        public void CancelReserve(string entity, IEnumerable<decimal> idLst, string operationCode, string eventKindCode, decimal count)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CancelReserve(entity, idLst, operationCode, eventKindCode, count);
        }

        #endregion

        #region . Подбор .

        [BP, DisplayName(@"Создание списка пикинга")]
        public void CreatePickList(IEnumerable<OWB> owbList, string truckCode = null)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CreatePickList(owbList, truckCode);
        }

        [BP, DisplayName(@"Удаление списка пикинга")]
        public void DeletePickList(IEnumerable<PL> pickList)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.DeletePickList(pickList);
        }

        // переехало в WmsAPI
        //[BP, DisplayName(@"Кол-во доступных списков пикинга (по коду погрузчика)")]
        //public decimal GetPickListCount(string truckCode, decimal? plid)
        //{
        //    using (var repo = GetRepository<IBPProcessRepository>())
        //        return repo.GetPickListCount(truckCode, plid);
        //}

        [BP, DisplayName(@"Резервировать список пикинга")]
        public void ReservePickList(decimal? plid, string truckCode, string operationCode, out PL pl, out Work work, string mplCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.ReservePickList(plid, truckCode, operationCode, out pl, out work, mplCode);
        }

        [BP, DisplayName(@"Обработка позиции пикинга")]
        public PLPos ProcessPlPos(IEnumerable<decimal> plLst, decimal? plPosId, string targetTeCode, decimal? count, string action,
            bool getNext = false, string operationCode = null)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.ProcessPlPos(plLst, plPosId, targetTeCode, count, action, getNext, operationCode);
        }

        [BP, DisplayName(@"Завершить подбор на ТЕ")]
        public void CompletePickTE(string teCode, out decimal? tTaskID, string operationCode, bool instantReserveTtask)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CompletePickTE(teCode, out tTaskID, operationCode, instantReserveTtask);
        }

        [BP, DisplayName(@"Завершить позицию списка пикинга")]
        public void CompletePlPos(decimal idPlPos)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CompletePlPos(idPlPos);
        }

        [BP, DisplayName(@"Деактивация позиций списка пикинга")]
        public void DeactivatePlPos(IEnumerable<decimal> plPosLst)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.DeactivatePlPos(plPosLst);
        }

        [BP, DisplayName(@"Поиск позиции, ТЕ которой содержит товар с заданным ШК")]
        public virtual PLPos FindNextPlPosByTeByBarcode(decimal plid, string tecode, string barcode, decimal? currentPlPosId, bool needActivated, out SKU sku)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.FindNextPlPosByTeByBarcode(plid, tecode, barcode, currentPlPosId, needActivated, out sku);
        }

        #endregion

        #region . Упаковка .

        [BP, DisplayName(@"Создать короб")]
        public virtual TE CreateBox(string teCode, string teTypeCode, string creationPlaceCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.CreateBox(teCode, teTypeCode, creationPlaceCode);
        }

        [BP, DisplayName(@"Открыть короб")]
        public void OpenBox(string operationCode, string teCode, string packPlaceCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.OpenBox(operationCode, teCode, packPlaceCode);
        }

        [BP, DisplayName(@"Закрыть короб")]
        public void CloseBox(string operationCode, string teCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CloseBox(operationCode, teCode);
        }

        [BP, DisplayName(@"Упаковано")]
        public void Packed(decimal owbId)
        {
            using (var mgr = IoC.Instance.Resolve<IBaseManager<OWB>>())
            {
                var stateMgr = mgr as IStateChange;
                if (stateMgr == null)
                    throw new NotImplementedException("IStateChange");
                if (CurrentUnitOfWork != null)
                    mgr.SetUnitOfWork(CurrentUnitOfWork);
                stateMgr.ChangeStateByKey(owbId, "OP_PACKING_CLOSE_FULL");
            }
        }

        [BP, DisplayName(@"Упаковать товары")]
        public virtual void PackProductLst(IEnumerable<decimal> productIdLst, IEnumerable<Product> changedProducts, string packTE, decimal packCount, bool packFullProduct)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.PackProductLst(productIdLst, changedProducts, packTE, packCount, packFullProduct);
        }

        [BP, DisplayName(@"Упаковать по SKU")]
        public virtual void PackProductLstBySKU(IEnumerable<decimal> productIdLst, IEnumerable<Product> changedProducts,
            string packTECode, decimal skuId, decimal packProductCountSKU, bool isEnablePackOtherSKU)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.PackProductLstBySKU(productIdLst, changedProducts, packTECode, skuId, packProductCountSKU, isEnablePackOtherSKU);
        }

        public void PackProduct(decimal productId, string packTE, decimal packCount)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.PackProduct(productId, packTE, packCount);
        }

        [BP, DisplayName(@"Распаковать")]
        public Product UnPack(decimal productID)
        {
            Product result = null;
            using (var repo = GetRepository<IBPProcessRepository>())
                result = repo.UnPack(productID);
            return result;
        }

        [BP, DisplayName(@"Получить вес ТЕ")]
        public decimal GetTEWeight(string teCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.GetTEWeight(teCode);
        }

        [BP, DisplayName(@"Получить погрешность и вес ТЕ (Контроль веса)")]
        public void GetTEWeightControl(string teCode, out decimal weight, out decimal? dev)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.GetTEWeightControl(teCode, out weight, out dev);
        }

        [BP, DisplayName(@"Вернуть на исходную ТЕ")]
        public void ReturnOnSourceTE(IEnumerable<decimal> productIDLst, string placeCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.ReturnOnSourceTE(productIDLst, placeCode);
        }

        [BP, DisplayName(@"Проверки упакованости заказа")]
        public string CheckPackOWB(decimal productID, object checkWh)
        {
            int? needCheckWh = null;
            int tmp;
            if (int.TryParse((string)checkWh, out tmp))
                needCheckWh = tmp;
            string result;
            using (var repo = GetRepository<IBPProcessRepository>())
                result = repo.CheckPackOWB(productID, needCheckWh);
            return result;
        }

        [BP, DisplayName(@"Уникальность накладной на ТЕ")]
        public bool UniqueOfOWBOnTE(List<Decimal> idList)
        {
            var filter =
                string.Format(
                    "select count(distinct op2.owbid_r) from wmsowbpos op1 left join wmsproduct p1 on p1.owbposid_r = op1.owbposid left join wmste te on p1.tecode_r = te.tecode left join wmsproduct p2 on te.tecode = p2.tecode_r left join wmsowbpos op2 on p2.owbposid_r = op2.owbposid where p1.statuscode_r='PRODUCT_BUSY' and te.tepackstatus <> 'TE_PKG_NONE'  and p2.statuscode_r='PRODUCT_BUSY' and op1.owbid_r in ({0}) group by op1.owbid_r",
                    string.Join(",", idList.Select(x => x).ToArray()));

            var tb = ExecuteDataTable(filter);
            if (tb == null)
                throw new OperationException("Не удалось получить данные, при определении уникальности накладной на ТЕ"); ;

            var isError = false;
            var count = 0;

            while (!isError && count < tb.Rows.Count)
            {
                if (Convert.ToDecimal(tb.Rows[count].ItemArray[0]) > 1)
                    isError = true;
                count++;
            }

            return !isError;
        }

        public virtual void UnpackTe(string teCode, string placeCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.UnpackTe(teCode, placeCode);
        }
        #endregion

        #region . Отгрузка .

        [BP, DisplayName(@"Завершение отгрузки по расходной накладной")]
        public void CompleteOWB(decimal key, decimal needTraffic, string operationCode, decimal? itid)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CompleteOWB(key, needTraffic, operationCode, itid);
        }

        [BP, DisplayName(@"Завершение отгрузки по внутреннему рейсу")]
        public void CompleteCargoOWB(decimal key, string operationCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CompleteCargoOWB(key, operationCode);
        }

        [BP, DisplayName(@"Поместить комплекты (отгрузочная часть)")]
        public void ConvertToKit(decimal owbid)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.ConvertToKit(owbid);
        }

        [BP, DisplayName(@"Аннуляция расходной накладной")]
        public void CancelOWB(decimal owbid, string operationcode, string eventKind)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CancelOWB(owbid, operationcode, eventKind);
        }

        [BP, DisplayName(@"Возврат накладной")]
        public void ReturnOwb(string operationcode, decimal key, string returnplacecode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.ReturnOwb(operationcode, key, returnplacecode);
        }

        [BP, DisplayName(@"Отгрузка ТЕ")]
        public void CompleteTE(string teCode, decimal cargoOWBID, string operationcode, out bool isLastTE)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CompleteTE(teCode, cargoOWBID, operationcode, out isLastTE);
        }

        [BP, DisplayName(@"Отгрузка нескольких ТЕ")]
        public void CompleteManyTE(IEnumerable<string> teCodes, decimal cargoOWBID, string operationcode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CompleteManyTE(teCodes, cargoOWBID, operationcode);
        }

        [BP, DisplayName(@"Создание позиций прихода из расхода")]
        public void FromOwb2Iwb(string entity, decimal owbid, decimal iwbid)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.FromOwb2Iwb(entity, owbid, iwbid);
        }

        #endregion

        #region . Биллинг .

        [BP, DisplayName(@"Рассчитать акт")]
        public void CalcBillWorkAct(decimal workActId, decimal? workAct2Op2cId, decimal? trace, bool fictional)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CalcBillWorkAct(workActId, workAct2Op2cId, trace, fictional);
        }

        [BP, DisplayName(@"Очистить акт")]
        public void ClearBillWorkAct(decimal workActId, decimal? workAct2Op2cId, decimal? isManualOnly)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.ClearBillWorkAct(workActId, workAct2Op2cId, isManualOnly);
        }

        [BP, DisplayName(@"Фиксировать акт")]
        public virtual void FixBillWorkAct(decimal workActId)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.FixBillWorkAct(workActId);
        }

        [BP, DisplayName(@"Вернуть акт")]
        public virtual void UnFixBillWorkAct(decimal workActId)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.UnFixBillWorkAct(workActId);
        }
        #endregion

        #region . Двор .

        [BP, DisplayName(@"Информация о приходном грузе (груз, работа, внут. рейс, рейс)")]
        public void GetCargoIWBInfo(decimal cargoIWBID, string operationCode, out CargoIWB cargoIWB, out Work work, out InternalTraffic internalTraffic, out ExternalTraffic externalTraffic)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.GetCargoIWBInfo(cargoIWBID, operationCode, out cargoIWB, out work, out internalTraffic, out externalTraffic);
        }

        [BP, DisplayName(@"Информация о расходном грузе (груз, работа, внут. рейс, рейс)")]
        public void GetCargoOWBInfo(decimal cargoOWBID, string operationCode, out CargoOWB cargoOWB, out Work work, out InternalTraffic internalTraffic, out ExternalTraffic externalTraffic, out decimal existTE)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.GetCargoOWBInfo(cargoOWBID, operationCode, out cargoOWB, out work, out internalTraffic, out externalTraffic, out existTE);
        }

        [BP, DisplayName(@"Маршрутизировать")]
        public void RouteTE2CargoOWB(decimal cargoOWBID, IEnumerable<TE> TELstXml)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.RouteTE2CargoOWB(cargoOWBID, TELstXml);
        }

        [BP, DisplayName(@"Поставить ТС на ворота")]
        public void SetTrafficGate(decimal internalTrafficID, string gateCode, string operationCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.SetTrafficGate(internalTrafficID, gateCode, operationCode);
        }

        #endregion . Двор .

        #region .  Маршрутизация  .

        [BP, DisplayName(@"Рассчитать маршруты")]
        public List<string> ChangeOWBRoute(IEnumerable<decimal> OWBIDLst, decimal? routeID, DateTime? planDate = null,
            bool changeDate = true, bool changeRoute = true)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.ChangeOWBRoute(OWBIDLst, routeID, planDate, changeDate, changeRoute);
        }

        #endregion

        #region .  Менеджер товара  .

        [BP, DisplayName(@"Настройки менеджера товара по артикулу")]
        public List<PMConfig> GetPMConfigByParamListByArtCode(string artCode, string operationCode, string methodName)
        {
            using (var mgr = (IPMConfigManager)GetManager<PMConfig>())
                return mgr.GetPMConfigByParamListByArtCode(artCode, operationCode, methodName);
        }

        [BP, DisplayName(@"Настройки менеджера товара")]
        public List<PMConfig> GetPMConfigByParamListByProductId(decimal? productId, string operationCode, string methodName)
        {
            using (var mgr = (IPMConfigManager)GetManager<PMConfig>())
                return mgr.GetPMConfigByParamListByProductId(productId, operationCode, methodName);
        }

        [BP, DisplayName(@"Настройка менеджера товара по списку товаров")]
        public List<PMConfig> GetPMConfigListByProductList(IEnumerable<decimal> productIdList, string operationCode, string methodCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.GetPMConfigListByProductList(productIdList, operationCode, methodCode);
        }

        [BP, DisplayName(@"Настройка менеджера товара по списку артикулов")]
        public List<PMConfig> GetPMConfigListByArtCodeList(IEnumerable<string> artCodeList, string operationCode, string methodCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.GetPMConfigListByArtCodeList(artCodeList, operationCode, methodCode);
        }

        #endregion . Менеджер товара .

        #region . Инвентаризация .

        [BP, DisplayName(@"Товар, не вошедший в инвентаризацию")]
        public List<Product> GetInvMissedProduct(string filter, IEnumerable<decimal> invIdLst)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.GetInvMissedProduct(filter, invIdLst);
        }

        [BP, DisplayName(@"Создать инвентаризацию")]
        public void CreateInv(decimal invID, string operationCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CreateInv(invID, operationCode);
        }

        [BP, DisplayName(@"Зафиксировать просчет")]
        public void FixInvTaskStep(IEnumerable<decimal> invTaskGroupIDLst, string operationCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.FixInvTaskStep(invTaskGroupIDLst, operationCode);
        }

        [BP, DisplayName(@"Зафиксировать инвентаризацию")]
        public void FixInv(decimal invID, string operationCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.FixInv(invID, operationCode);
        }

        [BP, DisplayName(@"Очистить инвентаризацию")]
        public void CleanInv(decimal invID)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CleanInv(invID);
        }

        [BP, DisplayName(@"Расфиксировать инвентаризацию")]
        public void UnFixInv(decimal invId, string operationCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.UnFixInv(invId, operationCode);
        }

        [BP, DisplayName(@"Расчет расхождений")]
        public void FindDifference(decimal invGroupID, IEnumerable<InvTaskGroup> invTaskGroupIDLst, string operationCode, out decimal flag)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.FindDifference(invGroupID, invTaskGroupIDLst, operationCode, out flag);
        }

        [BP, DisplayName(@"Корректировка инвентаризации")]
        public void InvCorrect(decimal invID, string operationCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.InvCorrect(invID, operationCode);
        }

        [BP, DisplayName(@"Подготовить инвентаризацию")]
        public void PrepareInv(decimal invReqID, decimal pageRecCount, DateTime dateBegin, out Inv inv)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.PrepareInv(invReqID, pageRecCount, dateBegin, out inv);
        }

        [BP, DisplayName(@"Получить группу заданий инвентаризации")]
        public void ReserveInvGroup(decimal invID, ref decimal? invTaskGroupID, string placeCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.ReserveInvGroup(invID, ref invTaskGroupID, placeCode);
        }

        [BP, DisplayName(@"Закрыть группу заданий инвентаризации")]
        public string AcceptPlace(decimal invTaskGroupID, string action)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.AcceptPlace(invTaskGroupID, action);
        }

        [BP, DisplayName(@"Запись задания инвентаризации в буфер")]
        public void AcceptInvTask(InvTask invTask, string action)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.AcceptInvTask(invTask, action);
        }

        [BP, DisplayName(@"Получение очередного задания по группе")]
        public InvTask GetInvTask(decimal invTaskGroupID, decimal? invTaskID, bool recalc)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.GetInvTask(invTaskGroupID, invTaskID, recalc);
        }

        [BP, DisplayName(@"Получить места инвентаризации")]
        public IEnumerable<string> GetPlaceNameLst(decimal invID)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.GetPlaceNameLst(invID);
        }

        #endregion . Инвентаризация .

        #region . СТН .

        [BP, DisplayName(@"Занесения данных в СТН")]
        public void InsWtv(decimal? productId, decimal? diff, decimal? comactId, decimal? transact = null, IEnumerable<decimal> stnIdLst = null)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.InsWtv(productId, diff, comactId, transact, stnIdLst);
        }

        # endregion

        #region . Корректировка единицы измерения .
        [BP, DisplayName(@"Использование SKU в событиях")]
        public bool IsUsedSkuInEventDetail(decimal skuId)
        {
            using (var manager = GetManager<EventDetail>())
            {
                if (CurrentUnitOfWork != null)
                    manager.SetUnitOfWork(CurrentUnitOfWork);
                return manager.GetFiltered(string.Format("SKUID_R = {0}", skuId), GetModeEnum.Partial).Any();
            }
        }

        [BP, DisplayName(@"Изменение ЕУ (SKU) с повышенными правами")]
        public void UpdateSku(SKU entity)
        {
            EntityCudOperation(entity, CudOperation.Update, false);
        }

        [BP, DisplayName(@"Изменить ОВХ SKU и пересчитать ТЕ")]
        public virtual string ChangeSKUAndRecalculationTE(List<SKU> skuList)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.ChangeSKUAndRecalculationTE(skuList);
        }

        //[BP, DisplayName(@"Пересчет ТЕ")]
        public void RecalculationTE(SKU sku)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.RecalculationTE(sku);
        }

        #endregion . Корректировка единицы измерения .
       
        #region . RCL .
        [BP, DisplayName(@"Валидация ворот")]
        public void ValidateGate(string placeCode, bool checkPhysics = false)
        {
            if (placeCode == null)
                throw new OperationException("Не указан код места.");

            Place place;
            using (var manager = GetManager<Place>())
                place = manager.Get(placeCode);

            if (place == null)
                throw new OperationException("Не найдено место с кодом '{0}'.", placeCode);

            Area area = null;
            if (!string.IsNullOrEmpty(place.AreaCode))
                using (var manager = GetManager<Area>())
                    area = manager.Get(place.AreaCode, GetModeEnum.Partial);

            if (area == null)
                throw new OperationException("Для ворот с ШК '{0}' не найдена область '{1}'.", placeCode, place.AreaCode);

            using (var manager = GetManager<AreaType>())
            {
                var areaTypeCode = area.GetProperty("AREATYPECODE_R");
                var areaType = manager.Get(areaTypeCode);
                if (areaType == null)
                    throw new OperationException("Для ворот с ШК '{0}' не найден тип области '{1}'.", placeCode, areaTypeCode);

                var areaTypeGpvs = areaType.GetProperty<WMSBusinessCollection<AreaTypeGPV>>("GLOBALPARAMVAL");

                //Проверяем, что место принадлежит области ворот приемки (у типа области GPV «AreaDestination=’ IN’»)
                if (!areaTypeGpvs.Any(p => p != null && p.GlobalParamCode_R.EqIgnoreCase("AreaDestination") && p.GparamValValue.EqIgnoreCase("IN")))
                    throw new OperationException("Место '{0}' не является воротами приемки.", placeCode);
            }

            if (checkPhysics)
            {
                //Проверяем, что у места прописаны физические ворота через CPV (PlaceGateL2)
                var gateCode =
                    GetGateCodeFromPlaceCpv(place.GetProperty<WMSBusinessCollection<PlaceCpv>>("CUSTOMPARAMVAL"));
                if (string.IsNullOrEmpty(gateCode))
                    throw new OperationException("У места с ШК '{0}' нет назначенных ворот.", placeCode);
            }
        }

        #region Obsolete API
        //[BP, DisplayName(@"Получить количество открытых работы (Work) по операции")]
        //public int GetWorkNotCompleteByOperationCount(string operationCode)
        //{
        //    var result = GetWorkNotCompleteByOperation(operationCode, null).Length;
        //    return result;
        //}

        //[BP, DisplayName(@"Получить открытые работы (Work) по операции")]
        //public Work[] GetWorkNotCompleteByOperation(string operationCode, decimal? cargoIwbId)
        //{
        //    string filter;
        //    if (cargoIwbId.HasValue)
        //    {
        //        //select * from wmswork where
        //        //    OPERATIONCODE_R='OP_CARGO_INPUT_TERM' AND WORKTILL is null AND STATUSCODE_R='WORK_CREATED'
        //        //    AND WORKID in (select v.WORKID_R from VWTENTEVENTDETAIL v where V.CARGOIWBID_R = 801 AND  V.EVENTHEADERID_R in (select h.EVENTHEADERID from WmsEventHeader h where h.EVENTKINDCODE_R = 'WORK_CREATE' AND h.OPERATIONCODE_R = 'OP_CARGO_INPUT_TERM')) 

        //        filter = string.Format("OPERATIONCODE_R = '{0}' AND WORKTILL is null AND STATUSCODE_R = '{1}'" +
        //           " AND WORKID in (select v.WORKID_R from VWTENTEVENTDETAIL v where V.CARGOIWBID_R = {2}" +
        //           " AND V.EVENTHEADERID_R in (select h.EVENTHEADERID from WmsEventHeader h where h.EVENTKINDCODE_R = '{3}' AND h.OPERATIONCODE_R = '{0}'))",
        //           operationCode, WorkStatus.WORK_CREATED, cargoIwbId, "WORK_CREATE");
        //    }
        //    else
        //    {
        //        filter = string.Format("OPERATIONCODE_R = '{0}' AND WORKTILL is null AND STATUSCODE_R = '{1}'" +
        //            " AND exists(select 1 from SysClientSession s where s.CLIENTSESSIONID = CLIENTSESSIONID_R AND s.USERCODE_R = '{2}')",
        //            operationCode,
        //            WorkStatus.WORK_CREATED,
        //            WMSEnvironment.Instance.AuthenticatedUser.GetSignature());
        //    }

        //    using (var manager = GetManager<Work>())
        //        return manager.GetFiltered(filter, GetModeEnum.Partial).ToArray();
        //}

        //[BP, DisplayName(@"Валидация: ТС стоит на воротах")]
        //public void ValidateVehicleAtGate(decimal cargoIwbId)
        //{
        //    CargoIWB cargoIwb;
        //    using (var manager = GetManager<CargoIWB>())
        //        cargoIwb = manager.Get(cargoIwbId, GetModeEnum.Partial);

        //    InternalTraffic internalTrafficInternal;
        //    object internalTrafficId;
        //    using (var manager = GetManager<InternalTraffic>())
        //    {
        //        internalTrafficId = cargoIwb.GetProperty("INTERNALTRAFFICID_R");
        //        internalTrafficInternal = manager.Get(internalTrafficId, GetModeEnum.Partial);
        //        if (internalTrafficInternal == null)
        //            throw new OperationException("Груз '{0}' не имеет внутреннего рейса.", cargoIwbId);
        //    }

        //    //фактические дата и время прибытия внутреннего рейса заданы и меньше текущего времени [InternalTrafficFactArrived]
        //    var internalTrafficFactArrived = internalTrafficInternal.GetProperty<DateTime?>("INTERNALTRAFFICFACTARRIVED");
        //    if (!internalTrafficFactArrived.HasValue)
        //        throw new OperationException("Для груза '{0}' не задано время прибытия внутреннего рейса.", cargoIwbId);
        //    var datenow = DateTime.Now;
        //    if (internalTrafficFactArrived > datenow)
        //        throw new OperationException("Для груза '{0}' время прибытия внутреннего рейса превышает текущую дату {1}.", cargoIwbId, datenow);

        //    //статус внешнего рейса «CAR_TRANSITTERRITORY»
        //    using (var manager = GetManager<ExternalTraffic>())
        //    {
        //        var externalTrafficId = internalTrafficInternal.GetProperty("EXTERNALTRAFFICID_R");
        //        var externalTrafficInternal = manager.Get(externalTrafficId, GetModeEnum.Partial);

        //        if (externalTrafficInternal == null)
        //            throw new OperationException("Груз '{0}' не имеет рейса.", cargoIwbId);

        //        if (externalTrafficInternal.Status != ExternalTrafficStatus.CAR_TRANSITTERRITORY)
        //            throw new OperationException("Статус рейса '{0}' не '{1}'.", externalTrafficId, ExternalTrafficStatus.CAR_TRANSITTERRITORY);
        //    }

        //    //фактические дата и время убытия внутреннего рейса не заданы INTERNALTRAFFICFACTDEPARTED
        //    var internalTrafficfActDeparted = internalTrafficInternal.GetProperty<DateTime?>("INTERNALTRAFFICFACTDEPARTED");
        //    if (internalTrafficfActDeparted.HasValue)
        //        throw new OperationException("Для груза '{0}' задано время убытия внутреннего рейса.", cargoIwbId);

        //    //const string formatSql = "EVENTKINDCODE_R = '{0}' AND OPERATIONCODE_R = '{1}'" +
        //    //    "AND EXISTS(select 1 from WMSEVENTDETAIL d " +
        //    //    "where D.EVENTHEADERID_R = WMSEVENTHEADER.EVENTHEADERID AND d.INTERNALTRAFFICID_R = {2})";

        //    const string formatSql =
        //        "EVENTHEADERID_R in (select h.EVENTHEADERID from WmsEventHeader h where h.EVENTKINDCODE_R = '{0}' AND h.OPERATIONCODE_R = '{1}')" +
        //        " AND INTERNALTRAFFICID_R = {2}";

        //    using (var manager = GetManager<EventDetail>())
        //    {
        //        //для внутреннего рейса есть событие «CAR_ARRIVEDWAREHOUSE»
        //        var filter = string.Format(formatSql, "CAR_ARRIVEDWAREHOUSE", "OP_CAR_ARRIVEDWAREHOUSE", internalTrafficId);
        //        if (manager.GetFiltered(filter, GetModeEnum.Partial).ToArray().Length == 0)
        //            throw new OperationException("Для груза '{0}' нет события '{1}'.", cargoIwbId, "ТС прибыло на склад и посталено на ворота (CAR_ARRIVEDWAREHOUSE)");

        //        //для внутреннего рейса нет события «CAR_DEPARTEDWAREHOUSE»
        //        filter = string.Format(formatSql, "CAR_DEPARTEDWAREHOUSE", "OP_CAR_DEPARTEDWAREHOUSE", internalTrafficId);
        //        if (manager.GetFiltered(filter, GetModeEnum.Partial).ToArray().Length > 0)
        //            throw new OperationException("Для груза '{0}' есть событие(я) '{1}'.", cargoIwbId, "ТС убыло со склада (CAR_DEPARTEDWAREHOUSE)");
        //    }
        //}

        //[BP, DisplayName(@"Существование событий работы (Work) по грузу")]
        //public bool ExistsWorkEventByCargoIwbId(decimal cargoIwbId)
        //{
        //    const string formatSql =
        //       "EVENTHEADERID_R in (select h.EVENTHEADERID from WmsEventHeader h where h.EVENTKINDCODE_R = '{0}' AND h.OPERATIONCODE_R = '{1}')" +
        //       " AND CARGOIWBID_R = {2}";
        //    const string operation = "OP_CARGO_INPUT_TERM";

        //    var filter = string.Format(formatSql, "WORK_CREATE", operation, cargoIwbId);
        //    using (var manager = GetManager<EventDetail>())
        //    {
        //        var result = manager.GetFiltered(filter, GetModeEnum.Partial).ToArray();

        //        if (result.Length == 0)
        //            return false;

        //        filter = string.Format(formatSql, "WORK_COMPLETED", operation, cargoIwbId);
        //        result = manager.GetFiltered(filter, GetModeEnum.Partial).ToArray();
        //        return result.Length == 0;
        //    }
        //}
        #endregion Obsolete API

        [BP, DisplayName(@"Количество товара на ТЕ")]
        public virtual void GetProductQuantityOnTe(string teCode, decimal skuId, out decimal skuQuantity2Te, out decimal skuQuantity2TeMax)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.GetProductQuantityOnTe(teCode, skuId, out skuQuantity2Te, out skuQuantity2TeMax);
        }

        [BP, DisplayName(@"Осталось принять товара на ТЕ из груза")]
        public decimal GetTeProductQuantityFromCargoIwb(string operationCode, decimal cargoIwbId, IWBPosInput posInput, bool raiseError, out decimal baseSkuCount, out decimal productCount)
        {
            decimal result;
            using (var repo = GetRepository<IBPProcessRepository>())
                result = repo.GetTeProductQuantityFromCargoIwb(operationCode, cargoIwbId, posInput, raiseError, out baseSkuCount, out productCount);
            return result;
        }
        #endregion . RCL .

        #region . System .
        //[BP, DisplayName(@"Получить дату и время")]
        public DateTime GetSystemDate()
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.GetSystemDate();
        }

        public SdclConnectInfo GetSdclConnectInfo(string clientCode, string prevSdclCode)
        {
            //STUB
            using (var repo = GetRepository<IBPProcessRepository>())
            {
                var endPoint = repo.GetSdclEndPoint(clientCode, ref prevSdclCode);
                var result = new SdclConnectInfo { Code = prevSdclCode, Endpoint = endPoint };
                return result;
            }
        }

        public SdclConnectInfo GetSdclConnectInfo(string clientCode, SdclConnectInfo lastSdclConnectInfo)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
            {
                var code = lastSdclConnectInfo == null ? null : lastSdclConnectInfo.Code;
                var endPoint = repo.GetSdclEndPoint(clientCode, ref code);
                var result = new SdclConnectInfo { Code = code, Endpoint = endPoint };
                return result;
            }
        }

        [BP, DisplayName(@"Запустить архивирование (по конфигурации)")]
        public void ProcessArch(string archCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.ProcessArch(archCode);
        }

        #endregion System

        #endregion . API_BusinessProcesses .

        #region . Общие .

        [BP, DisplayName(@"Изменение с повышенными правами")]
        public void UpdateEntity(WMSBusinessObject entity)
        {
            EntityCudOperation(entity, CudOperation.Update, false);
        }

        [BP, DisplayName(@"Удаление с повышенными правами")]
        public void DeleteEntity(WMSBusinessObject entity)
        {
            EntityCudOperation(entity, CudOperation.Delete, false);
        }

        [BP, DisplayName(@"Удаление списка с повышенными правами")]
        public void DeleteEntityList(IEnumerable<WMSBusinessObject> entityList)
        {
            foreach (var entity in entityList)
            {
                EntityCudOperation(entity, CudOperation.Delete, false);
            }
        }

        [BP, DisplayName(@"Проверка существования записи")]
        public int CheckInstanceEntity(string entity, string key)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.CheckInstanceEntity(entity, key);
        }
        #endregion . Общие .

        #region . Сущности .

        [BP, DisplayName(@"Проверка признака виртуальности ТЕ")]
        public int IsVirtualTE(string teCode, string teTypeCode = null)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.IsVirtualTE(teCode, teTypeCode);
        }

        [BP, DisplayName(@"Проверка ТЕ на моно")]
        public bool IsMonoTE(string teCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.IsMonoTE(teCode);
        }

        #endregion

        #region  .  Работа  .

        [BP, DisplayName(@"Получить время начала/окончания работы")]
        public void GetDateFrDateTill(string entity, string key, string operationCode, decimal? workerId, out Working working)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.GetDateFrDateTill(entity, key, operationCode, workerId, out working);
        }

        [BP, DisplayName(@"Завершение работы")]
        public virtual void WorkComleted(decimal workId, string operation, DateTime? workTill)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.WorkComleted(workId, operation, workTill);
        }

        [BP, DisplayName(@"Начать работу, создать выполнение")]
        public void StartWorking(string entity, string key, string operationCode, decimal? workerId, decimal? mandantID, DateTime? workingFrom, string workingDoc, out Work work)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.StartWorking(entity, key, operationCode, workerId, mandantID, workingFrom, workingDoc, out work);
        }

        [BP, DisplayName(@"Cоздать выполнения работ")]
        public virtual void StartWorkings(decimal workId, string truckCode, decimal myWorkerId, IEnumerable<decimal> workerIds, DateTime? workingFrom)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.StartWorkings(workId, truckCode, myWorkerId, workerIds, workingFrom);
        }

        [BP, DisplayName(@"Завершить выполнение работы данного работника")]
        public void CompleteWorking(string entity, string key, string operationCode, decimal? workerId)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CompleteWorking(entity, key, operationCode, workerId);
        }

        [BP, DisplayName(@"Завершить выполнения работ")]
        public virtual void CompleteWorkings(IEnumerable<decimal> workingIds, DateTime? dateTill)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CompleteWorkings(workingIds, dateTill);
        }

        [BP, DisplayName(@"Сменить статус работы")]
        public virtual void ChangeWorkStatus(decimal workId, string operation)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.ChangeWorkStatus(workId, operation);
        }

        [BP, DisplayName(@"Получить работу по операции")]
        public Work GetWorkByOperation(string entity, string key, string operationCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.GetWorkByOperation(entity, key, operationCode);
        }
        #endregion  .  Работа  .

        #region  .  Мандант  .

        [BP, DisplayName(@"Проверка наличия job-а")]
        public int? ChkJob(string jobName, string mandantCode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.ChkJob(jobName, mandantCode);
        }

        [BP, DisplayName(@"Создание/изменение job-a")]
        public void CreJob(string jobName, string mandantCode, int interval)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.CreJob(jobName, mandantCode, interval);
        }
        #endregion
        
        #region . CPV .
        [BP, DisplayName(@"Получить значение пользовательского параметра")]
        public object GetCpvValue(string entity, string key, string cpvcode)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.GetCpvValue(entity, key, cpvcode);
        }

        [BP, DisplayName(@"Сохранить значения пользовательских параметров группы TIR")]
        public virtual void SaveTirCpvs(IEnumerable<CustomParamValue> cpvs, IEnumerable<decimal> mandantids)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.SaveTirCpvs(cpvs, mandantids);
        }

        [BP, DisplayName(@"Удалить значения пользовательских параметров группы TIR")]
        public virtual void DeleteTirCpvs(IEnumerable<string> iwbids)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.DeleteTirCpvs(iwbids);
        }

        [BP, DisplayName(@"Удалить значения пользовательских параметров по сущности, коду и ид. сущности")]
        public virtual void DeleteCpvsByEntityByCodeByKey(string entity, IEnumerable<string> codes, IEnumerable<string> keys)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.DeleteCpvsByEntityByCodeByKey(entity, codes, keys);
        }

        #endregion . CPV .

        #region . Configurator .

        /// <summary>
        /// Получение данных для Configurator'а.
        /// </summary>
        public virtual void GetPmConfiguratorData(ref IEnumerable<BillOperation> operations,
            ref IEnumerable<decimal> entityids, ref IEnumerable<SysObject> attributes,
            ref IEnumerable<PM> pms, ref IEnumerable<PMMethod> pmMethods,
            ref IEnumerable<PMMethod2Operation> detailsPmMethod, ref DataTable pmdata,
            ref DataTable pmMethod2OperationsAllowed)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.GetPmConfiguratorData(ref operations, ref entityids, ref attributes, ref pms, ref pmMethods,
                    ref detailsPmMethod, ref pmdata, ref pmMethod2OperationsAllowed);
        }

        public virtual List<string> SavePmConfiguratorData(ICollection<PM2Operation> pm2Operations,
            ICollection<PMConfig> pmConfigs, ICollection<PM2Operation> deletePm2Operations,
            ICollection<PMConfig> deletePmConfigs)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.SavePmConfiguratorData(pm2Operations, pmConfigs, deletePm2Operations, deletePmConfigs);
        }

        public virtual void DeletePmConfiguratorData(ICollection<PM2Operation> pm2Operations, ICollection<PMConfig> pmConfigs)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                repo.DeletePmConfiguratorData(pm2Operations, pmConfigs);
        }
        #endregion . Configurator .

        public List<String> GetLastProductAttr(decimal skuId)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.GetLastProductAttr(skuId);
        }

        public List<SKU> GetSKUWithCache(string filter, string attrEntity)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.GetSKUWithCache(filter, attrEntity);
        }

        /// <summary>
        /// CUD-операции над сущностью с повышенными правами.
        /// </summary>
        protected WMSBusinessObject EntityCudOperation(WMSBusinessObject entity, CudOperation operation, bool updateInnerEntities)
        {
            WMSBusinessObject result = null;

            if (entity == null)
                throw new ArgumentNullException("entity");
            if (operation == CudOperation.None)
                throw new DeveloperException("Not define CUD-operation.");

            var type = entity.GetType();
            var mto = IoC.Instance.Resolve<IManagerForObject>();
            var mgrType = mto.GetManagerByTypeName(type.Name);
            if (mgrType == null)
                throw new DeveloperException(string.Format("Unknown source type '{0}'.", type.Name));

            var managerInstance = IoC.Instance.Resolve(mgrType, null) as IBaseManager;
            if (managerInstance == null)
                throw new DeveloperException(string.Format("Can't resolve IBaseManager by '{0}'.", mgrType.Name));

            try
            {
                if (CurrentUnitOfWork != null)
                    managerInstance.SetUnitOfWork(CurrentUnitOfWork);

                ((ISecurityAccess)managerInstance).SuspendRightChecking();
                switch (operation)
                {
                    case CudOperation.Create:
                        object o = entity;
                        managerInstance.Insert(ref o);
                        result = o as WMSBusinessObject;
                        break;
                    case CudOperation.Update:
                        if (updateInnerEntities)
                        {
                            var serializable = entity as ICustomXmlSerializable;
                            if (serializable != null)
                                serializable.OverrideIgnore = false;
                        }
                        managerInstance.Update(entity);
                        break;
                    case CudOperation.Delete:
                        managerInstance.Delete(entity);
                        break;
                }
                return result;
            }
            finally
            {
                ((ISecurityAccess)managerInstance).ResumeRightChecking();
            }
        }

        private string GetGateCodeFromPlaceCpv(ICollection<PlaceCpv> placeCpvs)
        {
            if (placeCpvs == null || placeCpvs.Count == 0)
                return null;

            var cpvs = placeCpvs.Where(p => p != null && p.GetProperty<string>("CUSTOMPARAMCODE_R_PLACECPV").EqIgnoreCase("PlaceGateL2")).ToArray();
            if (cpvs.Length == 0)
                return null;

            var gates = cpvs.Select(p => p.CPVValue).ToArray();
            if (gates.Length > 1)
                throw new OperationException("Параметр CPV PlaceGateL2 назначен больше одного раза.");

            return gates[0];
        }
        
        [BP, DisplayName(@"Получение ворот из CPV")]
        public string GetGateCodeFromPlaceCpv(string placeCode)
        {
            if (placeCode == null)
                return null;

            Place place;
            using (var manager = GetManager<Place>())
                place = manager.Get(placeCode);

            if (place == null)
                return null;
            
            var gateCode = GetGateCodeFromPlaceCpv(place.GetProperty<WMSBusinessCollection<PlaceCpv>>("CUSTOMPARAMVAL"));
            return gateCode;
        }

        public DataTable ExecuteDataTable(string query)
        {
            using (var repo = GetRepository<IBPProcessRepository>())
                return repo.ExecuteDataTable(query);
        }

        public static void RunFromXaml(string workflowXaml, ref BpContext bpContext)
        {
            DynamicActivity activity;
            using (var reader = new StringReader(workflowXaml))
                activity = (DynamicActivity)ActivityXamlServices.Load(reader);

            if (bpContext != null)
            {
                var inputs = new Dictionary<string, object> { { BpContext.BpContextArgumentName, bpContext } };
                WorkflowInvoker.Invoke(activity, inputs);
            }
            else
                WorkflowInvoker.Invoke(activity);
        }

        public static void RunByCode(string bpCode, bool notUseActivityStackTrace, bool notUsePersistAndLog, ref BpContext bpContext)
        {
            using (var mgr = IoC.Instance.Resolve<IBPProcessManager>())
            {
                mgr.Parameters.Add(BpContext.BpContextArgumentName, bpContext);
                mgr.Run(code: bpCode);
            }
        }

    }

    public enum CudOperation
    {
        None,
        Create,
        Update,
        Delete
    }
}