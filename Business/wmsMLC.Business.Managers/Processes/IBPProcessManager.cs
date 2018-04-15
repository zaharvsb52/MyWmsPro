using System;
using System.Collections.Generic;
using System.Data;
using wmsMLC.Business.Objects;
using wmsMLC.Business.Objects.Processes;
using wmsMLC.General.BL;
using wmsMLC.General.Services;

namespace wmsMLC.Business.Managers.Processes
{
    public interface IBPProcessManager : ITrueBaseManager
    {
        #region . Methods .
        void Run(string code, Action<CompleteContext> completedHandler = null);
        void Run(BPProcess bpProcess, Action<CompleteContext> completedHandler = null);
        IEnumerable<BPProcess> GetForEntity(Type entityType);
        Dictionary<string, object> Parameters { get; set; }
        bool ValidateProcessCode(string processCode);
        #endregion . Methods .

        #region . API_BusinessProcesses .

        // Произвольный запрос к таблицам в БД
        DataTable ExecuteDataTable(string query);

        #region . ВРЕМЕННЫЕ .

        // Статус упаковки расходной накладной
        void GetOWBBPStatus(decimal key, out string status);

        // БП "Подобрано"
        void OWBPicked(decimal owbid, string placecode, decimal notSamePlaceType);

        #endregion . ВРЕМЕННЫЕ .

        #region . Приемка .

        // Активировать приходную накладную
        void ActivateIWB(decimal key);

        // создание товара по всевдо позиции
        List<Product> CreateProductByPos(ref string manageFlag, ref string manageFlagParam, string operationCode, decimal iwbId, IWBPosInput posInput, int isMigrating = 0, string placeCode = null);

        //Создать товар по грузу
        List<Product> CreateProductByCargoIwb(ref string manageFlag, ref string manageFlagParam, string operationCode, IWBPosInput posInput, string placeCode, decimal cargoIwbId, int isMigrating = 0, decimal? iwbId = null);

        // Отмена приемки
        void CancelIwbAccept(decimal iwbid);

        // Частичная отмена приемки
        void CancelProductAccept(decimal? iwbid, IEnumerable<decimal> products, bool isAllTe, decimal? workid);

        // Закрыть приход
        void FinishIwb(IEnumerable<decimal> iwbs, string operationCode, decimal? comactId);

        // Создать виртуальные позиции
        List<IWBPosInput> GetIWBPosInputLst(IEnumerable<IWBPos> iwbPosList, int isMigrating = 0);

        // Расфиксировать
        void UnfixedIWB(decimal iwbid, string operationcode);

        // Получение коэффициента пересчета количества
        decimal ConvertSKUtoSKU(decimal sourceSkuId, decimal destSkuId, int isPrd, decimal? oldqty);

        BPBatch GetDefaultBatchCode(decimal? mandantID, decimal? sKUID, string artCode);

        // Получение черного артикула
        IWBPosInput GetBlackArt(CargoIWBPos cargoIwbPos, string teCode, string qlfCode, string iwbType, decimal mandantId);

        // полученить настройки менеджера приемки
        IEnumerable<decimal> GetMinConfig4IwbList(IEnumerable<decimal> iwbIdLst, string cpCode, string cpValue);

        // получить менеджер приемки
        decimal? GetDefaultMIN(decimal iwbId);

        // каскадное удаление накладной
        void CascadeDeleteIWB(decimal iwbId);
        #endregion

        #region . Перемещение .

        // блокировка области
        void BlockArea(string key, string blockingCode, string description, out decimal blockingID);
        // блокировка ТЕ
        void BlockTE(string key, string blockingCode, string description, out decimal blockingID);
        // блокировка места
        void BlockPlace(string key, string blockingCode, string description, out decimal blockingID);
        // блокировка сектора
        void BlockSegment(string key, string blockingCode, string description, out decimal blockingID);

        // получение мест для ЗНТ
        List<Place> GetPlaceLstByStrategy(string strategy, string sourceTECode);

        // создание ЗНТ
        void CreateTransportTask(string sourceTECode, string destPlaceCode, string strategy, string destTECode, out decimal transportTaskID, decimal? productID);

        // создание нескольких ЗНТ
        List<TransportTask> MoveManyTE2Place(IEnumerable<TE> teList, string destPlaceCode, string strategy, string destTECode, IEnumerable<Product> productList, int isManual);

        // Переехало в WmsAPI
        //Получить количество доступных ЗНТ
        //void GetAvailableTransportTaskCount(string filter, out int count);

        //Создать виртуальные ТЕ
        void CreateVirtualTE(IEnumerable<string> places, string teTypeCode);

        //Зарезервировать ЗНТ
        TransportTask ReserveTransportTask(decimal? currentTransportTaskCode, string filter);

        //Активировать ЗНТ
        void ReserveOrActivateTransportTask(decimal tTaskId, string clientCode, string truckCode, DateTime? taskBegin, BillOperationCode operationCode);

        //Освободить зарезервированную ЗНТ
        void ResetTransportTask(decimal currentTransportTaskCode);


        //Квитировать ЗНТ
        void CompleteTransportTask(IEnumerable<decimal> tTaskIDLst, decimal? workerCode, string truckCode, DateTime? startDate, DateTime? endDate, string teTypeCode, IEnumerable<string> teCodeLst, bool isNotNeededCreateWork);

        //Отмена ЗНТ
        void CancelTransportTask(decimal Key, string operationCode);

        // получение стратегии перемещения
        void GetPlaceStrategy(decimal productId, string placeCode, decimal raiseError, out decimal strategy);

        // создание ЗНТ для ТЕ по менеджеру перемещения
        void MoveTe(string teCode, out decimal tTaskId);

        // Получение ТЕ для дозагруза
        string FindTargetTE(string sourceTECode, string destPlaceCode, decimal? productId, decimal? raiseError, string destTECode);

        // Проверка/поиск несущей те
        string FindCarrierTE(string sourceTECode, string destPlaceCode, string strategy, decimal? productId, decimal checkActiveTt, string destTECode);

        // перемещение товара по SKU и TE
        void MoveProductsBySku(string sourceTeCode, string destTeCode, decimal skuId, IEnumerable<decimal> productIds, decimal count, string truckCode, bool isNotNeededCreateWork, out decimal transportTaskID);

        // Частичная отмена товара к перемещению
        void CancelMoveProduct(decimal ttaskid, decimal count);

        // Определить стратегию перемещения ТЕ
        string GetDefaultMMStrategy(string TECode);

        // Создание ЗНТ дозагруза для автоматического размещения товара
        void CreTransportTaskAuto(string currentPlaceCode, string sourceTECode, IEnumerable<decimal> prdInputs, ref string strategy, IEnumerable<string> skipPlaceLst, out TransportTask trasportTask, out decimal productCount);

        // Проверки  возможности перемещения ТЕ
        decimal CheckTE2Move(string teCode, decimal? productID, decimal raiseError);

        // Доступные ТЕ для дозагруза
        List<string> GetAvailableTE(string teCode, string placeCode, decimal? productID, decimal productCount);

        // Меняем статус места
        void ChangePlaceStatus(string placeCode, string operation);
        
        #endregion

        #region . Поставки .

        // Добавление в очередь поставок
        void CreateQSupplyChain(string operationCode, decimal? mandantID, decimal? resGroup, string tECode, decimal? supplyChainID, decimal? raiseErr, string process);

        void CreateQSupplyChainTt(string operationCode, decimal? mandantID, decimal? resGroup, string tECode, decimal? supplyChainID, decimal? raiseErr, string process,out decimal qSupplyChainID);

        // БП "Подобрано" (с учетом поставок)
        void OWBPickedBySupplyChain(decimal owbid, string operationCode);

        // Сформировать группу резервирования
        void CreateResGroup(string entity, string key, out decimal resGroup);

        // Отменить поставку для ТЕ (с отменой ЗНТ)
        void CancelSupplyChainForTE(string teCode, string operationCode);

        //Создать поставку
        void CreateSupplyChain(decimal qSupplyChainID, out decimal ttaskID);

        #endregion

        #region Счетчики ТЕ
        //Диапазон кодов ТЕ по коду счетчика ТЕ
        void GetTeLabelRange(string sequenceCode, int count, out int rangeBegin, out int rangeEnd);
        #endregion Счетчики ТЕ

        #region . Товар .

        //Получить свойства товара для корректировки
        List<PMConfig> GetProductPropertyForEdit(decimal productid, string operationcode);

        //Получить свойства артикула для корректировки
        List<PMConfig> GetArtPropertyForEdit(string artcode, string operationcode);

        //Изменение Товара с повышенными правами
        void UpdateProduct(Product entity);

        //Удаление Товара с повышенными правами
        void DeleteProduct(Product entity);

        //Изменение Коммерческого акта с повышенными правами
        void UpdateCommAct(CommAct entity);

        // расщепление товара
        void splitProduct(decimal Key, decimal SplitCount, decimal FreeIfBusy, string operationCode, out decimal NewKey);

        // Создать товар
        void CreateProduct(ref Product product, ref TE te);

        // выставить ЗНТ товару
        Product AssignTransportTaskToProduct(Product product, decimal transportTaskId);

        // конвертация товара
        IEnumerable<decimal> SplitProductWithSKU(decimal productId, decimal skuId, decimal countSku, double countInSku);
        
        // Разукомплектация
        void DismantleKit(IEnumerable<decimal> productLst, string kitCode);
        #endregion

        #region . Резервирование .

        // ручное резервирование
        void ManualReserve(decimal? owbPosId, decimal? owbId, decimal productId, decimal owbProductNeed, out decimal outParam);

        // Заменить зарезервированный товар
        void ChangeReserved(decimal oldProductId, decimal newProductId, decimal countNeeded);

        // Резервировать список накладных
        void ReserveOWBLst(IEnumerable<OWB> owbLst, string operationCode);

        //Получение времени начала/окончания по работе
        void GetDateFrDateTill(string entity, string key, string operationCode, decimal? workerId, out Working working);

        // БП "Отмена резервирования"
        void CancelReserve(string entity, IEnumerable<decimal> idLst, string operationCode, string eventKindCode, decimal count);

        #endregion

        #region . Подбор .

        // Создание списка пикинга
        void CreatePickList(IEnumerable<OWB> owbList, string truckCode = null);

        // Удаление списка пикинга
        void DeletePickList(IEnumerable<PL> pickList);

        // переехало в WmsAPI
        // Кол-во доступных списков пикинга (по коду погрузчика)
        //decimal GetPickListCount(string truckCode, decimal? plid);

        // Резервировать список пикинга
        void ReservePickList(decimal? plid, string truckCode, string operationCode, out PL pl, out Work work, string mplCode);

        // Обработка позиции пикинга
        PLPos ProcessPlPos(IEnumerable<decimal> plLst, decimal? plPosId, string targetTeCode, decimal? count, string action,
            bool getNext = false, string operationCode = null);

        // Завершить подбор на ТЕ
        void CompletePickTE(string teCode, out decimal? tTaskID, string operationCode, bool instantReserveTtask);

        //Завершить позицию списка пикинга
        void CompletePlPos(decimal idPlPos);

        //Деактивировать позиции списка пикинга
        void DeactivatePlPos(IEnumerable<decimal> plPosLst);

        //Поиск позиции, ТЕ которой содержит товар с заданным ШК
        PLPos FindNextPlPosByTeByBarcode(decimal plid, string tecode, string barcode, decimal? currentPlPosId,
            bool needActivated, out SKU sku);

        // Проверка кратности единицы учета требуемого количества
        bool IsMultipleSku(decimal skuId, decimal count, IEnumerable<decimal> skuList);
        #endregion

        #region . Упаковка .
        /// <summary>
        /// создать короб
        /// </summary>
        /// <param name="teCode">код ТЕ</param>
        /// <param name="teTypeCode">код типа ТЕ</param>
        /// <returns>ТЕ типа короб</returns>
        TE CreateBox(string teCode, string teTypeCode, string creationPlaceCode);

        /// <summary>
        /// закрыть короб
        /// </summary>
        /// <param name="operationCode">код операции</param>
        /// <param name="teCode">ТЕ</param>
        void CloseBox(string operationCode, string teCode);


        /// <summary>
        /// открыть короб
        /// </summary>
        /// <param name="operationCode">код операции</param>
        /// <param name="teCode">ТЕ</param>
        /// <param name="packPlaceCode">код места упаковки</param>
        void OpenBox(string operationCode, string teCode, string packPlaceCode);

        /// <summary>
        /// упаковано
        /// </summary>
        /// <param name="owbId">код расходной накладной</param>
        void Packed(decimal owbId);

        /// <summary>
        /// Упаковать товары
        /// </summary>
        void PackProductLst(IEnumerable<decimal> productIdLst, IEnumerable<Product> changedProducts, string packTE, decimal packCount, bool packFullProduct);

        /// <summary>
        /// Упаковать по SKU
        /// </summary>
        void PackProductLstBySKU(IEnumerable<decimal> productIdLst, IEnumerable<Product> changedProducts, string packTECode, decimal skuId, decimal packProductCountSKU, bool isEnablePackOtherSKU);

        /// <summary>
        /// распаковать
        /// </summary>
        /// <param name="productID">ID товара для распаковки</param>
        Product UnPack(decimal productID);

        /// <summary>
        /// Получить вес ТЕ
        /// </summary>
        decimal GetTEWeight(string teCode);

        /// <summary>
        /// Получить вес ТЕ и погрешность, если определена
        /// </summary>
        /// <param name="teCode">код те</param>
        /// <param name="weight">вес</param>
        /// <param name="dev">погрешность</param>
        void GetTEWeightControl(string teCode, out decimal weight, out decimal? dev);

        /// <summary>
        /// Вернуть на исходную ТЕ
        /// </summary>
        /// <param name="productIDLst">список ID товаров</param>
        /// <param name="placeCode">код места упаковки</param>
        void ReturnOnSourceTE(IEnumerable<decimal> productIDLst, string placeCode);

        /// <summary>
        /// Проверки упакованости заказа 
        /// </summary>
        /// <param name="productId">ID товаров</param>
        /// <param name="checkWh">необходимость проверять упаковку по складу</param>
        /// <returns>статус упаковки заказа</returns>
        string CheckPackOWB(decimal productId, object checkWh);

        /// <summary>
        /// Отмена упаковки. ТЕ прекращает быть упаковкой. Товар остается на ТЕ.
        /// </summary>
        void UnpackTe(string teCode, string placeCode);
        #endregion

        #region . Отгрузка .

        // Завершение отгрузки по расходной накладной
        void CompleteOWB(decimal key, decimal needTraffic, string operationCode, decimal? itid);

        // Завершение отгрузки по внутреннему рейсу
        void CompleteCargoOWB(decimal key, string operationCode);

        // Поместить комплекты (отгрузочная часть)
        void ConvertToKit(decimal owbid);

        // Аннуляция расходной накладной
        void CancelOWB(decimal owbid, string operationcode, string eventKind);

        // БП Возврат накладной
        void ReturnOwb(string operationcode, decimal key, string returnplacecode);

        // Отгрузка ТЕ
        void CompleteTE(string teCode, decimal cargoOWBID, string operationcode, out bool isLastTE);

        // Отгрузка нескольких ТЕ
        void CompleteManyTE(IEnumerable<string> teCodes, decimal cargoOWBID, string operationcode);

        // Создание позиций прихода из расхода
        void FromOwb2Iwb(string entity, decimal owbid, decimal iwbid);
        #endregion

        #region . Биллинг .

        // Рассчитать акт
        void CalcBillWorkAct(decimal workActId, decimal? workAct2Op2cId, decimal? trace, bool fictional);

        // Очистить акт
        void ClearBillWorkAct(decimal workActId, decimal? workAct2Op2cId, decimal? isManualOnly);

        // Фиксировать акт
        void FixBillWorkAct(decimal workActId);

        // Вернуть акт
        void UnFixBillWorkAct(decimal workActId);

        #endregion

        #region . Двор .

        // Информация о приходном грузе (груз, работа, внут. рейс, рейс)
        void GetCargoIWBInfo(decimal cargoIWBID, string operationCode, out CargoIWB cargoIWB, out Work work, out InternalTraffic internalTraffic, out ExternalTraffic externalTraffic);

        // Информация о расходном грузе (груз, работа, внут. рейс, рейс)
        void GetCargoOWBInfo(decimal cargoOWBID, string operationCode, out CargoOWB cargoOWB, out Work work, out InternalTraffic internalTraffic, out ExternalTraffic externalTraffic, out decimal existTE);

        // Маршрутизировать
        void RouteTE2CargoOWB(decimal cargoOWBID, IEnumerable<TE> TELstXml);

        // Поставить ТС на ворота
        void SetTrafficGate(decimal internalTrafficID, string gateCode, string operationCode);

        #endregion

        #region .  Маршрутизация  .

        // Рассчитать маршруты
        List<string> ChangeOWBRoute(IEnumerable<decimal> OWBIDLst, decimal? routeID, DateTime? pPlanDate = null, bool changeDate = true, bool changeRoute = true);

        #endregion

        #region . Инвентаризация .

        // Создать инвентаризацию
        void CreateInv(decimal invID, string operationCode);
        
        // Зафиксировать просчет 
        void FixInvTaskStep(IEnumerable<decimal> invTaskGroupIDLst, string operationCode);

        // Зафиксировать инвентаризацию 
        void FixInv(decimal invID, string operationCode);

        // Очистить инвентаризацию 
        void CleanInv(decimal invID);

        // Расфиксировать инвентаризацию
        void UnFixInv(decimal invId, string operationCode);

        //  Расчет расхождений
        void FindDifference(decimal invGroupID, IEnumerable<InvTaskGroup> invTaskGroupIDLst, string operationCode, out decimal flag);

        // Корректировка инвентаризации 
        void InvCorrect(decimal invID, string operationCode);

        // Подготовить инвентаризацию
        void PrepareInv(decimal invReqID, decimal pageRecCount, DateTime dateBegin, out Inv inv);

        // Получение группы заданий
        void ReserveInvGroup(decimal invID, ref decimal? invTaskGroupID, string placeCode);

        // Закрытие группы заданий и получение следующего места инвентаризации
        string AcceptPlace(decimal invTaskGroupID, string action);

        // Запись задания инвентаризации в буфер
        void AcceptInvTask(InvTask invTask, string action);

        // Получение очереднего задания
        InvTask GetInvTask(decimal invTaskGroupID, decimal? invTaskID, bool recalc);

        // Получение мест инвентаризации
        IEnumerable<string> GetPlaceNameLst(decimal invID);

        // Товар, не вошедший в инвентаризацию
        List<Product> GetInvMissedProduct(string filter, IEnumerable<decimal> invIdLst);

        #endregion . Инвентаризация .

        #region . СТН .

        // Занесения данных в СТН
        void InsWtv(decimal? productId, decimal? diff, decimal? comactId, decimal? transact = null, IEnumerable<decimal> stnIdLst = null);
        #endregion

        #region . Корректировка единицы измерения .
        //Использование SKU в событиях
        bool IsUsedSkuInEventDetail(decimal skuId);

        //Изменение ЕУ (SKU) с повышенными правами
        void UpdateSku(SKU entity);

        //Изменение ОВХ SKU
        string ChangeSKUAndRecalculationTE(List<SKU> skuList);

        //Пересчет TE
        void RecalculationTE(SKU sku);

        #endregion . Корректировка единицы измерения .

        #region . RCL .

        //Валидация физических ворот
        void ValidateGate(string placeCode, bool checkPhysics = false);

        // Получение ворот из CPV
        string GetGateCodeFromPlaceCpv(string placeCode);
        
        #region Obsolete API
        //Получить количество открытых работы (Work) по операции
        //int GetWorkNotCompleteByOperationCount(string operationCode);

        //Получить открытые работы (Work) по операции
        //Work[] GetWorkNotCompleteByOperation(string operationCode, decimal? cargoIwbId);

        //Валидация: ТС стоит на воротах
        //void ValidateVehicleAtGate(decimal cargoIwbId);

        //Существование событий работы (Work) по грузу
        //bool ExistsWorkEventByCargoIwbId(decimal cargoIwbId);
        #endregion Obsolete API

        //Количество товара на ТЕ
        void GetProductQuantityOnTe(string teCode, decimal skuId, out decimal skuQuantity2Te, out decimal skuQuantity2TeMax);

        //Осталось принять товара на ТЕ из груза
        decimal GetTeProductQuantityFromCargoIwb(string operationCode, decimal cargoIwbId, IWBPosInput posInput, bool raiseError, out decimal baseSkuCount, out decimal productCount);
        #endregion . RCL .

        #region . System .

        //Получить дату и время
        DateTime GetSystemDate();

        SdclConnectInfo GetSdclConnectInfo(string clientCode, string prevSdclCode);
        
        //Запустить архивирование (по конфигурации)
        void ProcessArch(string archCode);

        #endregion . System .

        #region . Mandant .

        // проверка наличия джоба
        int? ChkJob(string jobName, string mandantCode);
        // создание джоба
        void CreJob(string jobName, string mandantCode, int interval);
        #endregion

        #region .  Общие  .

        //Изменение с повышенными правами
        void UpdateEntity(WMSBusinessObject entity);

        //Удаление с повышенными правами
        void DeleteEntity(WMSBusinessObject entity);

        //Удаление списка с повышенными правами
        void DeleteEntityList(IEnumerable<WMSBusinessObject> entityList);

        // проверка существования записи
        int CheckInstanceEntity(string entity, string key);

        #endregion

        #region . Сущности .

        int IsVirtualTE(string teCode, string teTypeCode = null);

        bool IsMonoTE(string teCode);

        #endregion
        
        #region . Менеджер товара .
        List<PMConfig> GetPMConfigByParamListByArtCode(string artCode, string operationCode, string methodName);
        List<PMConfig> GetPMConfigByParamListByProductId(decimal? productId, string operationCode, string methodName);
        // Настройка менеджера товара по списку товаров
        List<PMConfig> GetPMConfigListByProductList(IEnumerable<decimal> productIdList, string operationCode, string methodCode);
        // Настройка менеджера товара по списку артикулов
        List<PMConfig> GetPMConfigListByArtCodeList(IEnumerable<string> artCodeList, string operationCode, string methodCode);
        #endregion . Менеджер товара .

        #region . CPV .
        //Получить значение пользовательского параметра
        object GetCpvValue(string entity, string key, string cpvcode);
        //Сохранить значения пользовательских параметров группы TIR
        void SaveTirCpvs(IEnumerable<CustomParamValue> cpvs, IEnumerable<decimal> mandantids);
        //"Удалить значения пользовательских параметров группы TIR
        void DeleteTirCpvs(IEnumerable<string> iwbids);
        //Удалить значения пользовательских параметров по сущности, коду и ид.сущности
        void DeleteCpvsByEntityByCodeByKey(string entity, IEnumerable<string> codes, IEnumerable<string> keys);
        #endregion . CPV .

        #region . Работа .

        // Завершение работы
        void WorkComleted(decimal workId, string operation, DateTime? workTill);

        // Начать работу, создать выполнение
        void StartWorking(string entity, string key, string operationCode, decimal? workerId, decimal? mandantID, DateTime? workingFrom, string workingDoc, out Work work);

        // Cоздать выполнения работ
        void StartWorkings(decimal workId, string truckCode, decimal myWorkerId, IEnumerable<decimal> workerIds, DateTime? workingFrom);

        // Завершить выполнение работы данного работника
        void CompleteWorking(string entity, string key, string operationCode, decimal? workerId);

        // Завершить выполнения работ
        void CompleteWorkings(IEnumerable<decimal> workingIds, DateTime? dateTill);
        
        // Сменить статус работы
        void ChangeWorkStatus(decimal workId, string operation);

        //Получить работу по операции
        Work GetWorkByOperation(string entity, string key, string operationCode);
        #endregion . Работа .

        List<String> GetLastProductAttr(decimal skuId);

        #endregion . API_BusinessProcesses .

        #region . Configurator .
        /// <summary>
        ///  Получение данных для Configurator'а.
        /// </summary>
        void GetPmConfiguratorData(ref IEnumerable<BillOperation> operations, ref IEnumerable<decimal> entityids,
            ref IEnumerable<SysObject> attributes,
            ref IEnumerable<PM> pms, ref IEnumerable<PMMethod> pmMethods,
            ref IEnumerable<PMMethod2Operation> detailsPmMethod,
            ref DataTable pmdata, ref DataTable pmMethod2OperationsAllowed);

        List<string> SavePmConfiguratorData(ICollection<PM2Operation> pm2Operations,
            ICollection<PMConfig> pmConfigs, ICollection<PM2Operation> deletePm2Operations,
            ICollection<PMConfig> deletePmConfigs);

        void DeletePmConfiguratorData(ICollection<PM2Operation> pm2Operations, ICollection<PMConfig> pmConfigs);
        #endregion . Configurator .

        List<SKU> GetSKUWithCache(string filter, string attrEntity);
    }
}
