using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using wmsMLC.Business.General;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;
using wmsMLC.General.DAL;

namespace wmsMLC.Business.Managers.Processes
{
    public class BPProcessManagerOracle : BPProcessManager
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Synchronizer<decimal> _packSynchronizer = new Synchronizer<decimal>();
        private static Hashtable _packedProducts = new Hashtable();

        private readonly string[] _cpTirIwbAccounts =
        {
            "IWBTIRAccountNumber", "IWBTIRAccountAmount", "IWBTIRAccountDate", "IWBTIRAccountCurrency",
            "IWBTIRAccountComment"
        };

        private readonly string[] _cpTirIwbPosAccounts =
        {
            "IWBPosAccountRef", "IWBPosAccountAmount"
        };

        public override TE CreateBox(string teCode, string teTypeCode, string creationPlaceCode)
        {
            var box = new TE
            {
                TECode = teCode,
                TETypeCode = teTypeCode,
                TEPackStatus = TEPackStatus.TE_PKG_CREATED,
                CreatePlace = creationPlaceCode,
                CurrentPlace = creationPlaceCode
            };

            using (var mgr = GetManager<TE>())
                mgr.Insert(ref box);

            return box;
        }

        public override void StartWorkings(decimal workId, string truckCode, decimal myWorkerId, IEnumerable<decimal> workerIds, DateTime? workingFrom)
        {
            var dateStart = workingFrom ?? GetSystemDate();
            var filter = string.Format("WORKID_R = {0} and WORKINGTILL is null and {1}", workId,
                FilterHelper.GetFilterIn("WORKERID_R", workerIds.Cast<object>()));

            DoInCurrentUnitOfWork(uow =>
            {
                using (var mgr = GetManager<Working>())
                {
                    var openworkings = mgr.GetFiltered(filter, GetModeEnum.Partial).ToArray();

                    foreach (var workerid in workerIds)
                    {
                        if (openworkings.Any(p => p.WORKERID_R == workerid))
                            continue;

                        var w = new Working
                        {
                            WORKERID_R = workerid,
                            WORKID_R = workId,
                            WORKINGFROM = dateStart,
                            TruckCode = truckCode,
                            WORKINGADDL = myWorkerId != workerid
                        };

                        mgr.Insert(ref w);
                    }
                }
            }, true);
        }

        public override void WorkComleted(decimal workId, string operation, DateTime? workTill)
        {
            if (string.IsNullOrEmpty(operation))
                throw new ArgumentNullException("operation");

            var now = GetSystemDate();
            var workTillInternal = workTill ?? now;

            DoInCurrentUnitOfWork(uow =>
            {
                Work work;
                using (var managerWork = GetManager<Work>())
                {
                    work = managerWork.Get(workId);
                    if (work == null)
                        throw new OperationException("Работа '{0}' не найдена.", workId);
                }
                // меняем статус работы на "Завершена"
                ChangeWorkStatus(workId, "FINISH");

                // обновляем даты окончания для выполнений
                if (work.WORKINGL != null && work.WORKINGL.Count > 0)
                {
                    var workings = work.WORKINGL.Where(p => !p.WORKINGTILL.HasValue).ToArray();
                    if (workings.Length > 0)
                    {
                        foreach (var p in workings)
                            p.WORKINGTILL = workTillInternal;

                        using (var managerWorking = GetManager<Working>())
                            managerWorking.Update(workings);
                    }
                }

                //Закомментировано по задаче http://mp-ts-nwms/issue/wmsMLC-10237
                //Событие
                //var instance = string.Format("API WorkComleted {0}", now);
                //var eventHeader = new EventHeader
                //{
                //    EventKindCode = "WORK_COMPLETED",
                //    OperationCode = operation,
                //    StartTime = now,
                //    Instance = instance
                //};

                //var evDetail = new EventDetail();
                //evDetail.SetProperty("WORKID_R", workId);
                //evDetail.SetProperty("WORKFROM_R", work.GetProperty(Work.VWORKFROMPropertyName));
                //evDetail.SetProperty("WORKTILL_R", work.GetProperty(Work.VWORKTILLPropertyName));

                //using (var eventHeaderMgr = (IEventHeaderManager)GetManager<EventHeader>())
                //    eventHeaderMgr.RegEvent(ref eventHeader, evDetail);
            }, true);
        }

        public override void CompleteWorkings(IEnumerable<decimal> workingIds, DateTime? dateTill)
        {
            var workingtill = dateTill ?? GetSystemDate();

            DoInCurrentUnitOfWork(uow =>
            {
                using (var mngWorking = GetManager<Working>())
                {
                    foreach (var id in workingIds)
                    {
                        var working = mngWorking.Get(id, GetModeEnum.Partial);
                        if (working == null)
                            continue;

                        working.WORKINGTILL = workingtill;
                        mngWorking.Update(working);
                    }
                }
            }, true);
        }

        public override void UnpackTe(string teCode, string placeCode)
        {
            DoInCurrentUnitOfWork(uow =>
            {
                using (var teMgr = GetManager<TE>())
                {
                    var te = teMgr.Get(teCode, FilterHelper.GetAttrEntity(typeof(TE), TE.TEPackStatusPropertyName));
                    if (te == null)
                        throw new DeveloperException("Can't find TE with TECODE = '{0}'", teCode);

                    //Если упаковка закрыта - открываем
                    if (te.TEPackStatus == TEPackStatus.TE_PKG_COMPLETED)
                        OpenBox("OP_PACKING_OPEN", teCode, placeCode);

                    //Меняем статус упаковки на не упаковка
                    var stateMgr = (IStateChange)teMgr;
                    stateMgr.ChangeStateByKey(teCode, "OP_PRODUCT_UNPACKING");
                }
            }, true);
        }

        public override void FixBillWorkAct(decimal workActId)
        {
            DoInCurrentUnitOfWork(uow =>
            {
                using (var mgr = GetManager<BillWorkAct>())
                {
                    var act = mgr.Get(workActId);
                    if (act == null)
                        throw new DeveloperException("Акт '{0}' не существует", workActId);

                    act.STATUSCODE_R = BillWorkActStatus.WORKACT_COMPLETED.ToString();
                    act.WORKACTFIXDATE = DateTime.Now;

                    mgr.Update(act);
                }
            });
        }

        public override PLPos FindNextPlPosByTeByBarcode(decimal plid, string tecode, string barcode, decimal? currentPlPosId, bool needActivated, out SKU sku)
        {
            PLPos result = null;
            SKU skuinternal = null;

            DoInCurrentUnitOfWork(uow =>
            {
                //Получим позиции
                var sql = "select distinct sku.skuid, plpos.plposid from wmsplpos plpos" +
                          " join wmssku sku on plpos.skuid_r = sku.skuid" +
                          " join wmsbarcode bc on to_char(sku.skuid) = bc.barcodekey" +
                          string.Format(" where plpos.plid_r = {0} and plpos.tecode_r = '{1}'", plid, tecode) +
                          " and plpos.statuscode_r in('PLPOS_PART_PICKED', 'PLPOS_MISSED', 'PLPOS_CREATED')" +
                          string.Format("and bc.barcode2entity = 'SKU' and BC.BARCODEVALUE = '{0}'", barcode);
                
                DataTable data;
                using (var manager = IoC.Instance.Resolve<IBPProcessManager>())
                {
                    data = manager.ExecuteDataTable(sql);
                }

                decimal? nextplposid = null;
                decimal? skuid = null;
                if (data != null && data.Rows.Count > 0)
                {
                    var row = data.Rows[0];
                    skuid = row["skuid"].To<decimal>();
                    nextplposid = row["plposid"].To<decimal>();
                }

                if (nextplposid == null)
                    return;

                using (var mgr = GetManager<SKU>())
                {
                    skuinternal = mgr.Get(skuid.Value);
                }

                using (var mgr = GetManager<PLPos>())
                {
                    if (needActivated)
                    {
                        var stateMgr = mgr as IStateChange;
                        if (stateMgr == null)
                            throw new NotImplementedException("IStateChange");

                        //Смена статуса текущей позиции
                        if (currentPlPosId.HasValue)
                        {
                            //Перечитаем позицию
                            var currentplpos = mgr.Get(currentPlPosId.Value, GetModeEnum.Partial);
                            if ((!currentplpos.PLPosCountSKUFact.HasValue || currentplpos.PLPosCountSKUFact == 0) &&
                                (currentplpos.StatusCode_r == "PLPOS_ACTIVATED" ||
                                 currentplpos.StatusCode_r == "PLPOS_MISSED"))
                            {
                                stateMgr.ChangeStateByKey(currentPlPosId.Value, "OP_PICK_EXIT");
                            }
                            else if (currentplpos.PLPosCountSKUFact > 0 && currentplpos.StatusCode_r == "PLPOS_ACTIVATED")
                            {
                                stateMgr.ChangeStateByKey(currentPlPosId.Value, "OP_PICK_POS_PART");
                            }
                        }

                        //Берем в работу найденную позицию
                        stateMgr.ChangeStateByKey(nextplposid.Value, "OP_PICK_BEGIN");
                    }

                    //Получим позицию
                    result = mgr.Get(nextplposid.Value, GetModeEnum.Partial);
                }

            }, needActivated);

            sku = skuinternal;
            return result;
        }

        public override void UnFixBillWorkAct(decimal workActId)
        {
            DoInCurrentUnitOfWork(uow =>
            {
                using (var mgr = GetManager<BillWorkAct>())
                {
                    var act = mgr.Get(workActId);
                    if (act == null)
                        throw new DeveloperException("Акт '{0}' не существует", workActId);

                    var stateMgr = mgr as IStateChange;
                    if (stateMgr == null)
                        throw new NotImplementedException("IStateChange");
                    stateMgr.ChangeStateByKey(workActId, BillOperationCode.OP_BILLWORKACT_UNCLOSE.ToString());

                    //TODO: записать событие ... (будет известно чуть позже)
                }
            });
        }

        public override void PackProductLst(IEnumerable<decimal> productIdLst, IEnumerable<Product> changedProducts,
            string packTE, decimal packCount, bool packFullProduct)
        {
            PackProductLstInternal(productIdLst, changedProducts, packTE, packCount, packFullProduct);
        }

        public override void PackProductLstBySKU(IEnumerable<decimal> productIdLst, IEnumerable<Product> changedProducts,
            string packTECode, decimal skuId, decimal packProductCountSKU, bool isEnablePackOtherSKU)
        {
            if (CurrentUnitOfWork != null && CurrentUnitOfWork.IsInTransaction())
                throw new DeveloperException("Существует внешняя транзакция!");

            DoInCurrentUnitOfWork(uow =>
            {
                using (var mgr = GetManager<Product>())
                {
                    var productList =
                        mgr.GetFiltered(
                            string.Join(";", FilterHelper.GetArrayFilterIn(Product.ProductIdPropertyName, productIdLst.Cast<object>())),
                            FilterHelper.GetAttrEntity<Product>(new[] { Product.ProductIdPropertyName, Product.SKUIDPropertyName, Product.ProductCountPropertyName, Product.ProductCountSKUPropertyName })).ToArray();

                    var packComplete = false;
                    var firstPack = true;
                    var needPackCount = packProductCountSKU;
                    decimal? currentSkuId = skuId;
                    var changedProductList = changedProducts.ToArray();
                    IEnumerator<DataRow> skuTree = null;

                    // признак окончания дерева SKU
                    var skuTreeEnd = false;

                    while (!packComplete)
                    {
                        decimal[] partProductLst = null;
                        if (!firstPack)
                        {
                            SKU sku;
                            using (var skuMgr = GetManager<SKU>())
                                sku = skuMgr.Get(currentSkuId, FilterHelper.GetAttrEntity<SKU>(new[] { SKU.SKUIDPropertyName, SKU.SKUNamePropertyName }));

                            if (!isEnablePackOtherSKU)
                                throw new OperationException("Недостаточно товара с единицей учета '{0}'", sku.SKUName);

                            //INFO: поиск SKU по дереву
                            if (skuTree == null)
                            {
                                var query =
                                    string.Format(
                                        "select TO_CHAR(DECODE(sign(t.SKULevelPrev - t.SKULevel), 1, t.SKUCountBasePrev/t.SKUCountBase, t.SKUCountBase/t.SKUCountBasePrev))" +
                                        " SKUCoef , DECODE(sign(t.SKULevelPrev - t.SKULevel), 1, 0, 1) isDiv, t.SKUID, t.SKUCOUNT from (select NVL(t.isDiv, 1) isDiv," +
                                        " lag(skutree.SKUCountBase, 1, skuinput.SKUCountBase) over(partition by 1 order by t.SKULevel nulls last," +
                                        " skutree.SKUlevel, skutree.SKUCountBase) SKUCountBasePrev, lag(skutree.SKULevel, 1, skuinput.SKULevel)" +
                                        " over(partition by 1 order by t.SKULevel nulls last, skutree.SKUlevel, skutree.SKUCountBase) SKULevelPrev," +
                                        " row_number() over(partition by 1 order by t.SKULevel nulls last, skutree.SKUlevel, skutree.SKUCountBase) rn," +
                                        " skutree.* from wmsSKU2Base skuinput inner join wmsSKU2Base skutree on skutree.SKUParentRoot = skuinput.SKUBase" +
                                        " left join (select SKUID, SKUName, level SKUlevel, 0 isDiv from wmsSKU start with SKUID = {0}" +
                                        " connect by prior SKUParent = SKUID union all select SKUID, SKUName, 100 + level, 1 from wmsSKU" +
                                        " start with SKUID = {0} connect by prior SKUID = SKUParent) t on t.SKUID = skutree.SKUID" +
                                        " and t.SKUID!= skuinput.SKUID where skuinput.SKUID = {0} and skuinput.SKUParentRoot = {0}" +
                                        " and skutree.SKUID != skuinput.SKUID ) t  order by t.rn",
                                        skuId);
                                skuTree = ExecuteDataTable(query).Rows.Cast<DataRow>().GetEnumerator();
                                skuTree.MoveNext();
                            }

                            var correctSku = false;
                            currentSkuId = null;
                            while (skuTree.Current != null && !skuTreeEnd && !correctSku)
                            {
                                var isDiv = skuTree.Current["ISDIV"].ConvertTo<int>();
                                var coeff = skuTree.Current["SKUCOEF"].ConvertTo<decimal>();
                                var skuCount = skuTree.Current["SKUCOUNT"].ConvertTo<double>();
                                currentSkuId = skuTree.Current["SKUID"].ConvertTo<decimal>();
                                needPackCount = isDiv == 1 ? needPackCount / coeff : needPackCount * coeff;

                                var productSkuCount = (decimal)skuCount;

                                // если получаем некратное число на упаковку, попробуем найти такой товар
                                if (needPackCount % 1 != 0)
                                {
                                    // получим относительно SKU
                                    // обязательно округляем до 16го знака!!!
                                    productSkuCount = Math.Round(needPackCount * productSkuCount, 16);
                                    needPackCount = Math.Ceiling(needPackCount * (decimal)skuCount / productSkuCount);
                                }

                                // попробуем найти товар с нужным количеством в SKU
                                // TODO: подбирать нужное кол-во товара с разными кол-во в SKU
                                partProductLst =
                                    productList.Where(
                                        i =>
                                            i.SKUID == currentSkuId &&
                                            (decimal)i.Get<double>(Product.ProductCountPropertyName) == productSkuCount)
                                        .Select(i => i.GetKey<decimal>())
                                        .ToArray();
                                
                                if (productList.Length == 0)
                                    currentSkuId = null;
                                else
                                    correctSku = true;
                                skuTreeEnd = !skuTree.MoveNext();
                            }

                            if (currentSkuId == null)
                                throw new OperationException("Недостаточно товара");
                        }
                        firstPack = false;
                        var skuProductListId = partProductLst ?? productList.Where(i => i.SKUID == currentSkuId).Select(i => i.GetKey<decimal>()).ToArray();
                        if (skuProductListId.Length == 0)
                            continue;
                        var skuChangedProducts =
                            changedProductList.Where(i => skuProductListId.Any(j => j == i.GetKey<decimal>()));
                        needPackCount = PackProductLstInternal(skuProductListId, skuChangedProducts, packTECode,
                            needPackCount, false);
                        if (needPackCount == 0)
                            packComplete = true;
                    }
                }
            }, true);
        }

        /// <summary>
        /// Упаковка товара
        /// </summary>
        /// <param name="productIdLst">список id упаковываемого товара</param>
        /// <param name="changedProducts">список измененных товаров</param>
        /// <param name="packTE">код ТЕ упаковки</param>
        /// <param name="packCount">кол-во на упаковку</param>
        /// <param name="packFullProduct">упаковать все ТЕ</param>
        /// <returns>кол-во товара, которое осталось упаковать</returns>
        public decimal PackProductLstInternal(IEnumerable<decimal> productIdLst, IEnumerable<Product> changedProducts, string packTE, decimal packCount, bool packFullProduct)
        {
            if (string.IsNullOrEmpty(packTE))
                throw new OperationException("Не указан обязательный параметр packTE");

            if (packCount == 0)
                throw new OperationException("Кол-во упаковываемого товара должно быть > 0");

            var resultValue = packCount;

            // чтобы избавиться от необходимости передавать TLISTXML реализуем обход упаковываемых позиций самостоятельно
            // разделеям процессы обновления товаров и упаковки
            // все делаем в транзации
            DoInCurrentUnitOfWork(uow =>
            {
                using (var mgr = GetManager<Product>())
                {
                    // сохраняем товары, по которым были изменения
                    if (changedProducts != null && changedProducts.Any())
                        mgr.Update(changedProducts);

                    // упаковываем
                    var needPackCount = packCount;
                    foreach (var id in productIdLst)
                    {
                        try
                        {
                            lock (_packSynchronizer[id])
                            {
                                var syncPackedProducts = Hashtable.Synchronized(_packedProducts);
                                // пропускаем, чтобы не делать лишний запрос
                                if (syncPackedProducts[id] != null)
                                    continue;
                                // получаем товар
                                // получать сразу все нельзя, т.к. при быстрой упаковке нужно иметь максимально последнее состояние объекта
                                var product = mgr.Get(id);

                                // такое может быть при быстром вводе ШК
                                if (product == null)
                                {
                                    // запоминаем что товар уже упаковали
                                    syncPackedProducts[id] = id;
                                    continue;
                                }

                                if (packFullProduct)
                                {
                                    PackProduct(product.ProductId.Value, packTE, product.ProductCountSKU);
                                }
                                else
                                {
                                    // если этот продукт уже упаковали - переходим кследующем
                                    // такое может случаться при быстром вводе ШК
                                    if (product.TECode == packTE)
                                        continue;

                                    // сможем ли мы одной позицией все упаковать
                                    var prdCount = product.ProductCountSKU;
                                    if (prdCount >= needPackCount)
                                        prdCount = needPackCount;

                                    // пакуем позицию в нужном кол-ве
                                    // ReSharper disable once PossibleInvalidOperationException
                                    PackProduct(product.ProductId.Value, packTE, prdCount);

                                    // пересчитываем оставшееся кол-во
                                    needPackCount -= prdCount;

                                    // если все упаковали - выходим
                                    if (needPackCount == 0)
                                        break;

                                    if (needPackCount < 0)
                                        throw new DeveloperException("Отрицательное итоговое количество после упаковки");
                                }
                            }
                        }
                        finally
                        {
                            resultValue = needPackCount;
                        }
                    }
                }
            }, true);
            return resultValue;
        }

        public override void RemoveStorageUnit(IEnumerable<decimal> productIdList)
        {
            var productIds = productIdList as decimal[] ?? productIdList.ToArray();

            DoInCurrentUnitOfWork(uow =>
            {
                Product[] products = null;
                using (var mgr = GetManager<Product>())
                {
                    // получаем товары
                    var attrEntity = FilterHelper.GetAttrEntity<Product>(Product.ProductIdPropertyName, Product.IWBPosIDPropertyName);
                    var filter = FilterHelper.GetFilterIn(Product.ProductIdPropertyName, productIds.Cast<object>());
                    filter += " and pkgBpInput.isStorageUnit(skuid_r) = 1";
                    products = mgr.GetFiltered(filter, attrEntity).ToArray();
                }

                foreach (var productId in productIds)
                {
                    var product = products.FirstOrDefault(i => i.ProductId == productId);
                    // проверяем
                    if (product == null)
                        throw new OperationException("Can't find product with id {0}. Maybe you do not have permission on it or product is not the storage unit", productId);

                    bool canDeleteIwbPos = false;

                    // удаляем товар
                    using (var mgr = GetManager<Product>())
                    {
                        // удаляем товар
                        mgr.DeleteByKey(productId);

                        // проверяем есть ли еще товар по этой позиции приходной наколадной (могли принять > 1 ОХ)
                        var attrEntity = FilterHelper.GetAttrEntity<Product>(Product.ProductIdPropertyName);
                        var iwbPosIdFieldName = SourceNameHelper.Instance.GetPropertySourceName(typeof(Product), Product.IWBPosIDPropertyName);
                        var filter = string.Format("{0} = {1}", iwbPosIdFieldName, product.IWBPosID);
                        var otherProducts = mgr.GetFiltered(filter, attrEntity);
                        canDeleteIwbPos = otherProducts == null || !otherProducts.Any();
                    }

                    if (canDeleteIwbPos)
                    {
                        // удаляем позицию накладной
                        using (var iwbPosMgr = GetManager<IWBPos>())
                        {
                            var iwbPos = iwbPosMgr.Get(product.IWBPosID);
                            if (iwbPos == null)
                                throw new OperationException("Can't find iwbpos with id {0}. Maybe you do not have permission on it", productId);

                            // обновляем статус, чтобы можно было удалить
                            iwbPos.StatusCode = IWBPosStates.IWBPOS_CREATED.ToString();
                            iwbPosMgr.Update(iwbPos);

                            iwbPosMgr.DeleteByKey(iwbPos.IWBPosID);
                        }
                    }
                    //TODO: узанть о необходимости писать событие
                }
            }, true);
        }

        public override void ReserveOrActivateTransportTask(decimal tTaskId, string clientCode, string truckCode, DateTime? taskBegin, BillOperationCode operationCode)
        {
            DoInCurrentUnitOfWork(uow =>
            {
                using (var taskMgr = GetManager<TransportTask>())
                {
                    var task = taskMgr.Get(tTaskId);

                    if (task == null)
                        throw new OperationException(string.Format("ЗНТ с кодом '{0}' не существует", tTaskId));

                    switch (operationCode)
                    {
                        case BillOperationCode.OP_MOVE_ACTIVATED:
                            // Если ЗНТ зарезервировали сами - то можно работать дальше
                            if (task.StatusCode == TTaskStates.TTASK_ACTIVATED && !string.IsNullOrEmpty(clientCode) && clientCode.Equals(task.ClientCode))
                                return;
                            task.ClientCode = clientCode;
                            break;
                        case BillOperationCode.OP_MOVE_TE:
                            task.ClientCode = clientCode;
                            task.TruckCode_R = truckCode;
                            task.TtaskBegin = taskBegin ?? BPH.GetSystemDate();
                            break;
                        default:
                            throw new OperationException(string.Format("Данная операция '{1}' не допустима для ЗНТ '{0}' в рамках метода ReserveOrActivate", tTaskId, operationCode));
                    }
                    taskMgr.Update(task);
                    ((IStateChange)taskMgr).ChangeStateByKey(tTaskId, operationCode.ToString());
                }
            }, true);
        }

        public override void GetProductQuantityOnTe(string teCode, decimal skuId, out decimal skuQuantity2Te, out decimal skuQuantity2TeMax)
        {
            if (string.IsNullOrEmpty(teCode))
                throw new OperationException("Не указан обязательный параметр teCode");

            decimal count = 0;
            decimal countMax = 0;

            DoInCurrentUnitOfWork(uow =>
            {
                using (var mgrPrd = GetManager<Product>())
                {
                    var products = mgrPrd.GetFiltered(string.Format("{0} = '{1}' and {2} = {3}", Product.TECodePropertyName, teCode, Product.SKUIDPropertyName, skuId),
                                                     FilterHelper.GetAttrEntity<Product>(Product.ProductIdPropertyName, Product.ProductCountSKUPropertyName));
                    count = products.Sum(x => x.ProductCountSKU);
                }
                using (var mgrS2T = GetManager<SKU2TTE>())
                {
                    var sku2ttes = mgrS2T.GetFiltered(string.Format("SKUID_R = {0} and TETYPECODE_R in (select wmste.tetypecode_r from wmste where wmste.tecode = '{1}') ", skuId, teCode),
                                                     FilterHelper.GetAttrEntity<SKU2TTE>(SKU2TTE.SKU2TTEIDPropertyName, SKU2TTE.SKU2TTEQuantityMaxPropertyName));
                    countMax = sku2ttes.Sum(x => x.SKU2TTEQuantityMax);
                }
            }, true);

            skuQuantity2Te = count;
            skuQuantity2TeMax = countMax;
        }

        public override string ChangeSKUAndRecalculationTE(List<SKU> skuList)
        {
            if (skuList == null || skuList.Count == 0)
                throw new OperationException("Не указан обязательный параметр skuList");

            var resultMes = string.Empty;
            foreach (var s in skuList)
            {
                try
                {
                    RecalculationTE(s);
                }
                catch (Exception ex)
                {
                    resultMes = string.IsNullOrEmpty(resultMes)
                        ? string.Format("Для SKU {0} (ID-{1}) возникла ошибка: {2}", s.SKUName, s.SKUID, ex.Message)
                        : string.Format("{0}\nДля SKU {1} (ID-{2}) возникла ошибка: {3}", resultMes, s.SKUName, s.SKUID, ex.Message);
                }
            }

            return resultMes;
        }

        public override void CascadeDeleteIWB(decimal iwbId)
        {
            DoInCurrentUnitOfWork(uow =>
            {
                IWB iwbFull;

                using (var mgr = GetManager<IWB>())
                {
                    iwbFull = mgr.Get(iwbId);
                }

                if (iwbFull.CustomParamVal != null && iwbFull.CustomParamVal.Count > 0)
                {
                    using (var mgr = GetManager<CustomParamValue>())
                    {
                        mgr.Delete(iwbFull.CustomParamVal);
                    }
                }

                if (iwbFull.IWBPosL != null && iwbFull.IWBPosL.Count > 0)
                {
                    using (var mgr = GetManager<IWBPos>())
                    {
                        mgr.Delete(iwbFull.IWBPosL);
                    }
                }

                if (iwbFull.IWB2CargoL != null && iwbFull.IWB2CargoL.Count > 0)
                {
                    using (var mgr = GetManager<IWB2Cargo>())
                    {
                        mgr.Delete(iwbFull.IWB2CargoL);
                    }
                }

                using (var mgr = GetManager<IWB>())
                {
                    mgr.DeleteByKey(iwbId);
                }
            }, true);
        }

        #region . IsMultipleSku  .
        public override bool IsMultipleSku(decimal skuId, decimal count, IEnumerable<decimal> skuList)
        {
            var list = skuList as decimal[] ?? skuList.ToArray();
            var query =
                string.Format("SELECT t.SKUCoef, t.IsDiv, t.SKUID, t.SKUCount, t.SKUPARENT, t.SKUINDIVISIBLE FROM (SELECT skutree.SKUID, skutree.skuLevel, skutree.SKUCountBase, skutree.SKUCount, skutree.SKUPARENT, skutree.SKUINDIVISIBLE, skuinput.SKULevel, skuinput.SKUCountBase," +
                " DECODE(sign(skutree.skuLevel-skuinput.SKULevel), 1, ROUND(skutree.SKUCountBase/skuInput.SKUcountBase,16), ROUND(skuInput.SKUcountBase/skutree.SKUCountBase,16)) SKUCoef," +
                " DECODE(sign(skutree.skuLevel-skuinput.SKULevel), 1, 0, 1) IsDiv, row_number() over(order by t.SKULevel nulls last, skutree.SKUlevel, skutree.SKUCountBase) rn" +
                " FROM wmsSKU2Base skuinput INNER JOIN wmsSKU2Base skutree ON skutree.SKUParentRoot = skuinput.SKUBase LEFT JOIN (" + 
                " SELECT SKUID, SKUName, level SKUlevel, 0 isDiv FROM wmsSKU START WITH SKUID = {0} CONNECT BY prior SKUParent = SKUID" +
                " UNION SELECT SKUID, SKUName, DECODE(SKUID, {0}, 0, 100) + level, DECODE(SKUID, {0}, 0, 1) FROM wmsSKU START WITH SKUID = {0} CONNECT BY prior SKUID = SKUParent) t" +
                " ON t.SKUID   = skutree.SKUID AND t.SKUID= skuinput.SKUID WHERE skuinput.SKUID = {0} AND skuinput.SKUParentRoot = {0}) t ORDER BY  t.rn", skuId);
            var skuTree = ExecuteDataTable(query).Rows.Cast<DataRow>().ToArray();
            
            var indivisibles = skuTree.Where(x => x["SKUINDIVISIBLE"].ConvertTo<decimal>() == 1);

            //получим родительские SKU для неделимых
            var indLst = new List<DataRow>();
            Parallel.ForEach(indivisibles, x => indLst.AddRange(GetAllSkuParents(x, skuTree)));

            var localLockObject = new object();
            var result = false;

            Parallel.ForEach(list, s =>
            {
                if (result)
                    return;
                // если текущая SKU является родителем неделимой, то она нам не подходит
                if (indLst.Any(x => x["SKUID"].To<decimal>() == s))
                    return;
                // найдем текущую SKU в дереве
                var skuRow = skuTree.FirstOrDefault(x => x["SKUID"].To<decimal>() == s);
                if (skuRow == null)
                    return;
                var isDiv = skuRow["ISDIV"].ConvertTo<int>();
                var coeff = skuRow["SKUCOEF"].ConvertTo<decimal>();
                // переводим SKU в SKU
                var indCount = isDiv == 0 ? count / coeff : count * coeff;
                // есть отстаток от деления
                if (indCount%1 != 0)
                    return;
                // нашли подходящую
                lock (localLockObject)
                    result = true;
            });
            return result;
        }

        private static IEnumerable<DataRow> GetAllSkuParents(DataRow row, IEnumerable<DataRow> rows)
        {
            var parentId = row["SKUPARENT"].To<decimal>();
            var dataRows = rows as DataRow[] ?? rows.ToArray();
            var parents = dataRows.Where(x => parentId == x["SKUID"].To<decimal>());
            foreach (var p in parents)
            {
                yield return p;
                foreach (var pp in GetAllSkuParents(p, dataRows))
                    yield return pp;
            }
        }
        #endregion

        #region . CPV .

        public override void SaveTirCpvs(IEnumerable<CustomParamValue> cpvs, IEnumerable<decimal> mandantids)
        {
            if (cpvs == null)
                return;

            DoInCurrentUnitOfWork(uow =>
            {
                var dbCpvCache = new Dictionary<string, bool>();

                Action<CustomParamValue[], CustomParamValue, decimal> updateParentIdHandler = (childsinternal, currentcpvinternal, parentidinternal) =>
                {
                    //Обновляем ид. родителя у детей
                    foreach (var cpvchildinternal in childsinternal)
                    {
                        cpvchildinternal.CPVParent = parentidinternal;
                    }
                    //Обновляем ид. родителя
                    currentcpvinternal.CPVID = parentidinternal;
                };

                var iwbPosAccountRefCpvs = cpvs.Where(p =>
                    p.CPV2Entity == "IWBPOS" && p.CustomParamCode == "IWBPosAccountRef" &&
                        !string.IsNullOrEmpty(p.CPVValue)).ToArray();
                Action<decimal, decimal> updateIwbPosAccountRefHandler = (id, dbid) =>
                {
                    //Oбновляем IWBPosAccountRef
                    var cpvsinternal = iwbPosAccountRefCpvs.Where(p => Convert.ToDecimal(p.CPVValue) == id).ToArray();
                    foreach (var cpv in cpvsinternal)
                    {
                        cpv.CPVValue = dbid.ToString(CultureInfo.InvariantCulture);
                    }
                };

                var cpvtype = typeof(CustomParamValue);
                var cpv2EntityPropertyName = SourceNameHelper.Instance.GetPropertySourceName(cpvtype,
                    CustomParamValue.CPV2EntityPropertyName);
                var cpvKeyPropertyName = SourceNameHelper.Instance.GetPropertySourceName(cpvtype,
                    CustomParamValue.CPVKeyPropertyName);
                var customParamCodePropertyName = SourceNameHelper.Instance.GetPropertySourceName(cpvtype,
                    CustomParamValue.CustomParamCodePropertyName);

                var sources = cpvs.OrderByDescending(cpv => cpv.CPVParent, new SpecialComparer()).ToArray();
                var mgr = GetManager<CustomParamValue>();
                mgr.SetUnitOfWork(uow);

                try
                {
                    //Удаляем
                    var cps = new List<string>(_cpTirIwbAccounts);
                    cps.AddRange(_cpTirIwbPosAccounts);
                    var sourcereverse = sources.Reverse().Where(p => cps.Contains(p.CustomParamCode)).ToArray();
                    foreach (var cpv in sourcereverse)
                    {
                        //Ищем cpv
                        var filter = string.Format("{0} = '{1}' AND {2} = '{3}' AND {4} = '{5}'",
                            cpv2EntityPropertyName, cpv.CPV2Entity,
                            cpvKeyPropertyName, cpv.CPVKey,
                            customParamCodePropertyName, cpv.CustomParamCode);

                        if (dbCpvCache.ContainsKey(filter))
                            continue; //Уже удалили

                        var dbcpvs = mgr.GetFiltered(filter, GetModeEnum.Partial).ToArray();
                        if (dbcpvs.Length == 0)
                            continue;

                        mgr.Delete(dbcpvs);
                        dbCpvCache[filter] = true;
                    }

                    //Добавляем
                    foreach (var cpv in sources)
                    {
                        if (cpv.CustomParamCode == "IWBTIR" || cpv.CustomParamCode == "IWBPosCustoms")
                        {
                            //Ищем cpv
                            var filter = string.Format("{0} = '{1}' AND {2} = '{3}' AND {4} = '{5}'",
                                cpv2EntityPropertyName, cpv.CPV2Entity,
                                cpvKeyPropertyName, cpv.CPVKey,
                                customParamCodePropertyName, cpv.CustomParamCode);

                            var dbcpvs = mgr.GetFiltered(filter, GetModeEnum.Partial).ToArray();
                            if (dbcpvs.Length > 1)
                                throw new DeveloperException("Find not single cpv by criteria '{0}'.", filter);
                            if (dbcpvs.Length == 1)
                            {
                                //Обновляем ид.
                                updateParentIdHandler(sources.Where(p => p.CPVParent == cpv.CPVID).ToArray(), cpv, dbcpvs[0].CPVID.Value);
                                mgr.Update(cpv);
                                continue;
                            }
                        }

                        object item = cpv;
                        mgr.Insert(ref item);
                        var newid = ((CustomParamValue)item).CPVID.Value;
                        if (cpv.CustomParamCode == "IWBTIRAccountNumber")
                            updateIwbPosAccountRefHandler(cpv.CPVID.Value, newid);
                        updateParentIdHandler(sources.Where(p => p.CPVParent == cpv.CPVID).ToArray(), cpv, newid);
                    }

                    if (mandantids == null || !mandantids.Any())
                        return;

                    //Добавляем связь параметры -> мандант
                    var cpvcodes = cpvs.Select(p => p.CustomParamCode).Distinct().ToArray();
                    if (cpvcodes.Length == 0)
                        return;

                    using (var mgrcp2Mandant = GetManager<CP2Mandant>())
                    {
                        mgrcp2Mandant.SetUnitOfWork(uow);
                        foreach (var mandantid in mandantids)
                        {
                            foreach (var code in cpvcodes)
                            {
                                var filter = string.Format("MANDANTID = {0} and CUSTOMPARAMCODE_R = '{1}'",
                                    mandantid, code);

                                if (mgrcp2Mandant.GetFiltered(filter, GetModeEnum.Partial).ToArray().Length == 0)
                                {
                                    var item = new CP2Mandant();
                                    item.SetProperty("MANDANTID", mandantid);
                                    item.SetProperty("CP2MANDANTCUSTOMPARAMCODE", code);
                                    item.SetProperty("CP2MANDANTORDER", 7);
                                    //С повышенными правами
                                    EntityCudOperation(item, CudOperation.Create, false);
                                }
                            }
                        }
                    }
                }
                finally
                {
                   if (mgr != null)
                        mgr.Dispose();
                }

            }, true);
        }

        private void DeleteCpvsByEntityByCodeByKeyInternal(IUnitOfWork uow, string entity, IEnumerable<string> codes, IEnumerable<string> keys)
        {
            if (string.IsNullOrEmpty(entity) || codes == null || !codes.Any() || keys == null || !keys.Any())
                return;

            var cpvtype = typeof(CustomParamValue);
            var cpv2EntityPropertyName = SourceNameHelper.Instance.GetPropertySourceName(cpvtype,
                CustomParamValue.CPV2EntityPropertyName);
            var cpvKeyPropertyName = SourceNameHelper.Instance.GetPropertySourceName(cpvtype,
                CustomParamValue.CPVKeyPropertyName);
            var customParamCodePropertyName = SourceNameHelper.Instance.GetPropertySourceName(cpvtype,
                CustomParamValue.CustomParamCodePropertyName);

            var exfilter = string.Format(" AND {0} = '{1}' AND {2}", cpv2EntityPropertyName, entity, FilterHelper.GetFilterIn(customParamCodePropertyName, codes));
            var filters = FilterHelper.GetArrayFilterIn(cpvKeyPropertyName, keys, exfilter);

            var cpvs = new List<CustomParamValue>();
            using (var mgr = GetManager<CustomParamValue>())
            {
                if (uow != null)
                    mgr.SetUnitOfWork(uow);

                foreach (var filter in filters)
                {
                    var dbcpvs = mgr.GetFiltered(filter, GetModeEnum.Partial).ToArray();
                    if (dbcpvs.Length > 0)
                        cpvs.AddRange(dbcpvs);
                }

                if (cpvs.Count > 0)
                {
                    var sources = cpvs.OrderByDescending(cpv => cpv.CPVParent).ToArray();
                    mgr.Delete(sources);
                }
            }
        }

        public override void DeleteCpvsByEntityByCodeByKey(string entity, IEnumerable<string> codes, IEnumerable<string> keys)
        {
            DoInCurrentUnitOfWork(uow =>
            {
                DeleteCpvsByEntityByCodeByKeyInternal(uow, entity, codes, keys);
            }, true);
        }

        public override void DeleteTirCpvs(IEnumerable<string> iwbids)
        {
            if (iwbids == null)
                return;

            DoInCurrentUnitOfWork(uow =>
            {
                //Получаем позиции соответствующие переданным ид.
                var filters = FilterHelper.GetArrayFilterIn("IWBID_R", iwbids);
                var iwbposlist = new List<IWBPos>();
                using (var mgr = GetManager<IWBPos>())
                {
                    foreach (var filter in filters)
                    {
                        var data = mgr.GetFiltered(filter, GetModeEnum.Partial).ToArray();
                        if (data.Length > 0)
                            iwbposlist.AddRange(data);
                    }
                }

                DeleteCpvsByEntityByCodeByKeyInternal(uow, "IWB", _cpTirIwbAccounts, iwbids);
                if (iwbposlist.Count > 0)
                {
                    DeleteCpvsByEntityByCodeByKeyInternal(uow, "IWBPOS", _cpTirIwbPosAccounts
                        , iwbposlist.Select(p => p.GetKey<decimal>().ToString(CultureInfo.InvariantCulture)));
                }
            }, true);
        }
        #endregion . CPV .

        #region . Configurator .
        //Получение данных для Configurator'а
        public override void GetPmConfiguratorData(ref IEnumerable<BillOperation> operations,
            ref IEnumerable<decimal> entityids, ref IEnumerable<SysObject> attributes,
            ref IEnumerable<PM> pms, ref IEnumerable<PMMethod> pmMethods,
            ref IEnumerable<PMMethod2Operation> detailsPmMethod, ref DataTable pmdata,
            ref DataTable pmMethod2OperationsAllowed)
        {
            List<BillOperation> operationsinternal = null;
            List<SysObject> entityidsinternal = null;
            List<SysObject> attributesinternal = null;
            List<PM> pmsinternal = null;
            List<PMMethod> pmMethodsinternal = null;
            List<PMMethod2Operation> detailsPmMethodinternal = null;
            DataTable pmdatainternal = null;
            DataTable pmMethod2OperationsAllowedinternal = null;

            DoInCurrentUnitOfWork(uow =>
            {
                using (var billOperationManager = IoC.Instance.Resolve<IBaseManager<BillOperation>>())
                {
                    //Получаем операции
                    operationsinternal = billOperationManager.GetFiltered(
                        "operationcode in (select distinct m2o.operationcode_r from wmspmmethod2operation m2o)",
                        GetModeEnum.Partial).ToList();
                }

                using (var sysObjectManager = IoC.Instance.Resolve<IBaseManager<SysObject>>())
                {
                    //Получаем настроенные сущности
                    entityidsinternal = sysObjectManager.GetFiltered(
                        "objectdatatype = 0 and objectentitycode in (select distinct so.objectentitycode_r from sysconfig2object so" +
                        " join wmspmmethod2operation mo on so.objectconfigcode_r = mo.pmmethodcode_r)",
                        GetModeEnum.Partial).ToList();

                    //Получаем настроенные атрибуты
                    attributesinternal = sysObjectManager.GetFiltered(
                        "objectparentid > 0 and objectname in (" +
                        "select distinct so.objectname_r from sysconfig2object so" +
                        " join wmspmmethod2operation mo on so.objectconfigcode_r = mo.pmmethodcode_r and mo.config2objectid_r = so.config2objectid"
                        + " where objectentitycode = so.objectentitycode_r" + ")",
                        GetModeEnum.Partial).ToList();
                }

                using (var pmManager = IoC.Instance.Resolve<IBaseManager<PM>>())
                {
                    pmsinternal = pmManager.GetAll(GetModeEnum.Partial).ToList();
                }

                using (var pmMethodManager = IoC.Instance.Resolve<IBaseManager<PMMethod>>())
                {
                    pmMethodsinternal = pmMethodManager.GetAll(GetModeEnum.Partial).ToList();
                }

                using (var pmMethod2OperationManager = IoC.Instance.Resolve<IBaseManager<PMMethod2Operation>>())
                {
                    detailsPmMethodinternal = pmMethod2OperationManager.GetAll(GetModeEnum.Partial).ToList();
                }

                var sqlpm = "select" +
                            " PM.PMCODE, PM2OP.PM2OPERATIONCODE, PM2OP.OPERATIONCODE_R AS OPERATIONCODE, PMCONFIG.OBJECTENTITYCODE_R AS OBJECTENTITYCODE" +
                            ", PMCONFIG.OBJECTNAME_R AS OBJECTNAME, PMCONFIG.PMMETHODCODE_R AS PMMETHODCODE, PMCONFIG.PMCONFIGBYPRODUCT, PMCONFIG.PMCONFIGINPUTMASK" +
                            ", PMCONFIG.PMCONFIGINPUTMASS" +
                            " from wmspm pm" +
                            " left join wmspm2operation pm2op on pm.pmcode = PM2OP.PMCODE_R" +
                            " left join wmspmconfig pmconfig on pm2op.pm2operationcode = pmconfig.pm2operationcode_r" +
                            " where PMCONFIG.OBJECTENTITYCODE_R is not null and PMCONFIG.OBJECTNAME_R is not null" +
                            " order by PM.PMNAME, PMCONFIG.OBJECTENTITYCODE_R, PMCONFIG.OBJECTNAME_R";

                var sqlpm2Op = "select" +
                           " pm2op.pmmethodcode_r as PmMethodCode, pm2op.operationcode_r as OperationCode" +
                           ",config2ob.objectentitycode_r as ObjectEntityCode, config2ob.objectname_r as ObjectName" +
                           " from wmspmmethod2operation pm2op" +
                           " join sysconfig2object config2ob on pm2op.config2objectid_r = config2ob.config2objectid";

                using (var manager = IoC.Instance.Resolve<IBPProcessManager>())
                {
                    pmdatainternal = manager.ExecuteDataTable(sqlpm);
                    pmMethod2OperationsAllowedinternal= manager.ExecuteDataTable(sqlpm2Op);
                }

            }, false);

            operations = operationsinternal;
            entityids = entityidsinternal.Select(p => p.GetKey<decimal>()).ToList();
            attributes = attributesinternal;
            pms = pmsinternal;
            pmMethods = pmMethodsinternal;
            detailsPmMethod = detailsPmMethodinternal;
            pmdata = pmdatainternal;
            pmMethod2OperationsAllowed = pmMethod2OperationsAllowedinternal;
        }

        public override List<string> SavePmConfiguratorData(ICollection<PM2Operation> pm2Operations,
            ICollection<PMConfig> pmConfigs, ICollection<PM2Operation> deletePm2Operations,
            ICollection<PMConfig> deletePmConfigs)
        {
            //Валидация
            var errors = Validate(pm2Operations, pmConfigs);
            if (errors.Count > 0)
                return errors;

            //Проверяем операции - PMMethod2Operation
            foreach (var pm2Operation in pm2Operations)
            {
                var pmcode = pm2Operation.PM2OperationPMCode;
                if (string.IsNullOrEmpty(pmcode))
                {
                    errors.Add("Неопределен ПМ.");
                    return errors;
                }

                if (string.IsNullOrEmpty(pm2Operation.OperationCode_r))
                    errors.Add(string.Format("Неопределена операция для ПМ '{0}'.", pmcode));
            }

            if (errors.Count > 0)
                return errors;

            using (var pmMethod2OperationManager = GetManager<PMMethod2Operation>())
            {
                using (var config2ObjectManager = GetManager<Config2Object>())
                {
                    foreach (var pmConfig in pmConfigs)
                    {
                        var pm2Operation = pm2Operations.FirstOrDefault(
                            p => p.GetKey<string>().EqIgnoreCase(pmConfig.PM2OperationCode_r));

                        if (pm2Operation == null)
                            throw new DeveloperException(
                                "Ошибка данных. Не найдена запись в pm2Operations с PM2OPERATIONCODE = '{0}'.",
                                pmConfig.PM2OperationCode_r);

                        var pmcode = pm2Operation.PM2OperationPMCode;

                        var entitycode = pmConfig.ObjectEntitycode_R;
                        if (string.IsNullOrEmpty(entitycode))
                            errors.Add(string.Format("Неопределена сущность для ПМ '{0}'.", pmcode));

                        if (string.IsNullOrEmpty(pmConfig.ObjectName_r))
                            errors.Add(string.Format("Неопределен атрибут для ПМ '{0}'.", pmcode));

                        if (string.IsNullOrEmpty(pmConfig.MethodCode_r))
                        {
                            errors.Add(string.Format("Неопределен метод для ПМ '{0}'.", pmcode));
                            continue;
                        }

                        var config2Objects = config2ObjectManager.GetFiltered(string.Format(
                            "OBJECTCONFIGCODE_R = '{0}' AND OBJECTENTITYCODE_R = '{1}' AND OBJECTNAME_R = '{2}'",
                            pmConfig.MethodCode_r, entitycode, pmConfig.ObjectName_r),
                            GetModeEnum.Partial).ToArray();

                        if (config2Objects.Length == 0)
                        {
                            errors.Add(
                               string.Format(
                                   "Запрещено использование сущности '{0}', атрибута '{1}', метода '{2}' для ПМ '{3}'.",
                                   entitycode, pmConfig.ObjectName_r, pmConfig.MethodCode_r, pmcode));
                            continue;
                        }

                        foreach (var config2Object in config2Objects)
                        {
                            var pmMethod2Operations = pmMethod2OperationManager.GetFiltered(string.Format(
                                "OPERATIONCODE_R = '{0}' AND PMMETHODCODE_R = '{1}' AND CONFIG2OBJECTID_R = {2}", pm2Operation.OperationCode_r, pmConfig.MethodCode_r, config2Object.GetKey()),
                                GetModeEnum.Partial).ToArray();

                            if (pmMethod2Operations.Length == 0)
                                errors.Add(
                                    string.Format("ПМ '{0}'. Запрещено использование операции '{1}' для метода '{2}'.",
                                        pmcode, pm2Operation.OperationCode_r, pmConfig.MethodCode_r));
                        }
                    }
                }
            }

            if (errors.Count > 0)
                return errors;


            DoInCurrentUnitOfWork(uow =>
            {
                if (deletePm2Operations != null && deletePmConfigs != null &&
               deletePm2Operations.Count > 0 && deletePmConfigs.Count > 0)
                    DeletePmConfiguratorDataInternal(uow, deletePm2Operations, deletePmConfigs);

                using (var pmConfigManager = GetManager<PMConfig>())
                {
                    if (uow != null)
                        pmConfigManager.SetUnitOfWork(uow);

                    using (var pm2OperationManager = GetManager<PM2Operation>())
                    {
                        if (uow != null)
                            pm2OperationManager.SetUnitOfWork(uow);

                        foreach (var pm2Operation in pm2Operations)
                        {
                            var pmcode = pm2Operation.PM2OperationPMCode;

                            //Поиск данных в pm2Operation, если нет - добавляем.
                            var pm2Operationdb = pm2OperationManager.GetFiltered(
                                string.Format("PMCODE_R = '{0}' AND OPERATIONCODE_R = '{1}'",
                                    pmcode, pm2Operation.OperationCode_r), GetModeEnum.Partial).SingleOrDefault();

                            if (pm2Operationdb == null)
                            {
                                pm2Operationdb = (PM2Operation) pm2Operation.Clone();
                                pm2Operationdb.SetProperty(pm2Operationdb.GetPrimaryKeyPropertyName(), null);
                                pm2OperationManager.Insert(ref pm2Operationdb);
                            }

                            var pm2Operationcode = pm2Operationdb.GetKey<string>();

                            //Удаляем все, что можно в pmconfig и добавляем измененные настройки
                            foreach (var pmConfig in pmConfigs.Where(pmConfig => pmConfig.PM2OperationCode_r.EqIgnoreCase(pm2Operation.GetKey<string>())))
                            {
                                var pmConfigdbs = pmConfigManager.GetFiltered(string.Format(
                                    "PM2OPERATIONCODE_R = '{0}' AND OBJECTENTITYCODE_R = '{1}' AND OBJECTNAME_R = '{2}' AND PMMETHODCODE_R = '{3}'",
                                    pm2Operationcode, pmConfig.ObjectEntitycode_R, pmConfig.ObjectName_r,
                                    pmConfig.MethodCode_r),
                                    GetModeEnum.Partial).ToArray();

                                if (pmConfigdbs.Length > 0)
                                    pmConfigManager.Delete(pmConfigdbs);

                                var pmConfigdb = (PMConfig) pmConfig.Clone();
                                pmConfigdb.PM2OperationCode_r = pm2Operationcode;
                                pmConfigManager.Insert(ref pmConfigdb);
                            }
                        }
                    }
                }

                //Очищаем внутренний кеш для БП "Корректировка товара"
                ClearCache();

            }, true);

            return errors;
        }

        public override void DeletePmConfiguratorData(ICollection<PM2Operation> pm2Operations, ICollection<PMConfig> pmConfigs)
        {
            DoInCurrentUnitOfWork(uow =>
            {
                DeletePmConfiguratorDataInternal(uow, pm2Operations, pmConfigs);
            }, true);
        }

        private void DeletePmConfiguratorDataInternal(IUnitOfWork uow, ICollection<PM2Operation> pm2Operations,
            ICollection<PMConfig> pmConfigs)
        {
            var errors = Validate(pm2Operations, pmConfigs);
            if (errors.Count > 0)
                throw new PassThroughException(string.Join(" ", errors));

            foreach (var pm2Operation in pm2Operations)
            {
                if (string.IsNullOrEmpty(pm2Operation.PM2OperationPMCode))
                    throw new PassThroughException("Не заполнено поле '{0}'.",
                        PM2Operation.PM2OperationPMCodePropertyName);
                if (string.IsNullOrEmpty(pm2Operation.OperationCode_r))
                    throw new PassThroughException("Не заполнено поле '{0}'.",
                        PM2Operation.PM2OperationOperationCodePropertyName);

                foreach (var pmConfig in pmConfigs.Where(pmConfig => pmConfig.PM2OperationCode_r.EqIgnoreCase(pm2Operation.OperationCode_r)))
                {
                    if (string.IsNullOrEmpty(pmConfig.ObjectEntitycode_R))
                        throw new PassThroughException("Не заполнено поле '{0}'.",
                            PMConfig.ObjectEntitycode_RPropertyName);
                    if (string.IsNullOrEmpty(pmConfig.ObjectName_r))
                        throw new PassThroughException("Не заполнено поле '{0}'.", PMConfig.ObjectNamePropertyName);
                    if (string.IsNullOrEmpty(pmConfig.MethodCode_r))
                        throw new PassThroughException("Не заполнено поле '{0}'.", PMConfig.PMMethodCodePropertyName);
                }
            }

            using (var pmConfigManager = GetManager<PMConfig>())
            {
                if (uow != null)
                    pmConfigManager.SetUnitOfWork(uow);

                using (var pm2OperationManager = GetManager<PM2Operation>())
                {
                    if (uow != null)
                        pm2OperationManager.SetUnitOfWork(uow);

                    foreach (var pm2Operation in pm2Operations)
                    {
                        var pmcode = pm2Operation.PM2OperationPMCode;

                        //Поиск данных в pm2Operation
                        var pm2OperationDbArray = pm2OperationManager.GetFiltered(
                            string.Format("PMCODE_R = '{0}' AND OPERATIONCODE_R = '{1}'",
                                pmcode, pm2Operation.OperationCode_r), GetModeEnum.Partial).ToArray();

                        foreach (var operation in pm2OperationDbArray)
                        {
                            var pm2Operationcode = operation.GetKey<string>();

                            PMConfig[] pmConfigdbs;
                            foreach (var pmConfig in pmConfigs.Where(pmConfig => pmConfig.PM2OperationCode_r.EqIgnoreCase(pm2Operation.GetKey<string>())))
                            {
                                pmConfigdbs = pmConfigManager.GetFiltered(
                                    string.Format(
                                        "PM2OPERATIONCODE_R = '{0}' AND OBJECTENTITYCODE_R = '{1}' AND OBJECTNAME_R = '{2}' AND PMMETHODCODE_R = '{3}'",
                                        pm2Operationcode, pmConfig.ObjectEntitycode_R, pmConfig.ObjectName_r,
                                        pmConfig.MethodCode_r),
                                    GetModeEnum.Partial).ToArray();
                                if (pmConfigdbs.Length > 0)
                                    pmConfigManager.Delete(pmConfigdbs);
                            }

                            pmConfigdbs = pmConfigManager.GetFiltered(string.Format("PM2OPERATIONCODE_R = '{0}'", pm2Operationcode),
                                    GetModeEnum.Partial).ToArray();
                            if (pmConfigdbs.Length == 0)
                                pm2OperationManager.Delete(pm2OperationDbArray);
                        }
                    }
                }
            }

            //Очищаем внутренний кеш для БП "Корректировка товара"
            ClearCache();
        }

        private List<string> Validate(ICollection<PM2Operation> pm2Operations, ICollection<PMConfig> pmConfigs)
        {
            var errors = new List<string>();
            if (pm2Operations == null || pm2Operations.Count == 0)
            {
                errors.Add("Неопределена коллекция настроек ПМ к операциям.");
                return errors;
            }

            if (pmConfigs == null || pmConfigs.Count == 0)
            {
                errors.Add("Неопределена коллекция настроек ПМ к сущностям, атрибутам и методам.");
                return errors;
            }

            return errors;
        }

        #endregion . Configurator .
    }

    public class Synchronizer<T>
    {
        private readonly Dictionary<T, object> _locks;
        private readonly object _myLock;

        public Synchronizer()
        {
            _locks = new Dictionary<T, object>();
            _myLock = new object();
        }

        public object this[T index]
        {
            get
            {
                lock (_myLock)
                {
                    object result;
                    if (_locks.TryGetValue(index, out result))
                        return result;

                    result = new object();
                    _locks[index] = result;
                    return result;
                }
            }
        }
    }

    internal class SpecialComparer : IComparer<decimal?>
    {
        public int Compare(decimal? x, decimal? y)
        {
            if (x == null && y == null)
                return 0;
            if (x == null)
                return 1;
            if (y == null)
                return -1;
            if (x.Value == y.Value)
                return 0;
            return x.Value > y.Value ? 1 : -1;
        }
    }
}