using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Windows;
using log4net;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Managers.Validation;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;
using wmsMLC.General.DAL;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.DCL.Packing.ViewModels
{
    public class Packer
    {
        #region .  Fields  .
        private static double? _cachedFontSize;
        private static bool _cachedFontSizeLoaded;
        private const string UseStoredProductCountSkuFieldName = "UseStoredProductCountSku";
        public const string NOT_PACKED = "NOT_PACKED";
        public const string WH_PACKED = "WH_PACKED";
        public const string PACKED = "PACKED";

        private object _packLock = new object();

        private readonly ILog _log = LogManager.GetLogger(typeof(Packer));
        #endregion .  Fields  .

        public Packer()
        {
            // чтобы не лезть каждый раз в БД - читаем из кэша
            FontSize = GetFontSize();
        }

        #region .  Properties  .
        private double? FontSize { get; set; }

        #endregion

        #region .  Methods  .

        public bool Pack(Product[] products, string teCode, decimal? startSkuId, bool isByBarcode, bool isNeedPackSourceTE, ref bool isUnpack, bool isAllPack, ref decimal? storedProductCountSku)
        {
            // извлекаем параметры
            bool askForQuantity = true;

            IUnitOfWork uow = null;

            // проверяем входные данные
            if (string.IsNullOrEmpty(teCode))
                throw new OperationException("Не задан код упаковки");

            if (products.Length == 0)
                throw new DeveloperException("Список товара на упаковку пуст");

            // проверям, что все, что хотим упаковать из одной заявки
            if (!CheckProductToEqualOWB(teCode, products, uow))
                return false;

            // список уникальных упаковываемых артикулов
            var artDistinct = products.Select(i => i.ArtCode_R).Distinct().ToArray();

            var packingProducts = new List<Product>();
            var canceledProducsts = new List<Product>();

            // по каждому артикулу получам конфигурации
            var artConfigs = new Dictionary<string, PMConfig[]>();
            using (var mgr = GetSpecManager<IPMConfigManager>(uow))
                foreach (var art in artDistinct)
                {
                    // получаем все возможные стратегии для данного артикула
                    var confs = mgr.GetPMConfigByParamListByArtCode(art, "OP_PRODUCT_PACKING", null).ToArray();

                    // запоминаем
                    artConfigs.Add(art, confs);
                }

            // блокируем дальнейшие действия, чтобы пользователь мог спокойно в одно окно ввести данные
            lock (_packLock)
            {
                // если что-то распаковали, то прерываем дальнейшее исполнение
                if (UnpackIfNeed(uow, artConfigs, products))
                {
                    isUnpack = true;
                    return false;
                }

                // получим список полей товара
                var productFields = DataFieldHelper.Instance.GetDataFields(typeof(Product), SettingDisplay.Detail);

                // проверим параметры MUST_SET и MUST_COMPARE для каждого артикула
                foreach (var art in artDistinct)
                {
                    // получаем все стратегии для данного артикула
                    var artConfs = artConfigs[art];

                    // упаковываемые текущее sku данного артикула или указанную
                    var artSkuList = startSkuId == null ? products.Where(i => i.ArtCode_R == art).Select(i => i.SKUID).Distinct() : new List<decimal> { startSkuId.Value };
                    //var artSkuList = products.Where(i => i.ArtCode_R == art).Select(i => i.SKUID).Distinct();

                    // для каждого SKU получим товары и проверим/запросим у каждого атрибуты MUST_SET и MUST_COMPARE
                    foreach (var sku in artSkuList)
                    {
                        // получим список товаров по SKU
                        // если указано стартовое sku, то выбирем товар в рамках артикула
                        var productsWithSameSKU = startSkuId == null ? products.Where(i => i.SKUID == sku).ToArray() : products.Where(i => i.ArtCode_R == art).ToArray();

                        // товар с текущим SKU
                        var tmpProduct = productsWithSameSKU[0];

                        // вычислим общее кол-во товара
                        var productCountSKU = startSkuId == null ? productsWithSameSKU.Sum(i => i.ProductCountSKU) : productsWithSameSKU.Where(i => i.SKUID == startSkuId.Value).Sum(i => i.ProductCountSKU);

                        // если указано стартовое sku, то выбирем товар в рамках артикула
                        if (startSkuId != null)
                            productsWithSameSKU = products.Where(i => i.ArtCode_R == art).ToArray();

                        // у каждого товара в списке проверим параметры MUST_SET
                        foreach (var product in productsWithSameSKU)
                        {
                            // если проверки и разнесения не прошли - выходим
                            if (!CheckByProductManager(artConfs, product, productFields, art, packingProducts, canceledProducsts, out askForQuantity))
                                return false;
                        }

                        // если по результатам упаковывать нечего - переходим дальше
                        if (packingProducts.Count == 0)
                            continue;

                        // если пакуем все ТЕ - ничего не спрашиваем и не показываем
                        if (isNeedPackSourceTE)
                        {
                            // передаваемое значение упаковываемого товара значения не имеет, т.к. при упаковке полного ТЕ - мы пакуем все товары в том кол-ве, в каком они есть
                            Pack(uow, teCode, startSkuId, 1, packingProducts, isNeedPackSourceTE);
                        }
                        // если у нас всего 1 продукт - можно не спрашивать кол-во
                        // если работаем по BC и есть признак PIECE_ONLY - можно не спрашивать кол-во
                        else if (productCountSKU == 1 || (isByBarcode && !askForQuantity))
                        {
                            Pack(uow, teCode, startSkuId, 1, packingProducts, isNeedPackSourceTE);
                        }
                        // если пакуем все - можно не спрашивать кол-во
                        else if (isAllPack)
                        {
                            Pack(uow, teCode, startSkuId, productCountSKU, packingProducts, isNeedPackSourceTE);
                        }
                        else
                        {
                            // иначе запрашиваем у пользователя кол-во
                            var countField =
                                productFields.FirstOrDefault(
                                    i => i.Name.EqIgnoreCase(Product.ProductCountSKUPropertyName));
                            if (countField == null)
                                throw new DeveloperException("Unknown property '{0}'",
                                    Product.ProductCountSKUPropertyName);

                            //Если учитываем сохраненное кол-во, то
                            var useStoredProductCountSku = false;
                            if (storedProductCountSku.HasValue && productCountSKU > 0)
                            {
                                useStoredProductCountSku = true;
                                if (storedProductCountSku < productCountSKU)
                                    productCountSKU = storedProductCountSku.Value;
                            }

                            var skuName = tmpProduct.SKUName;
                            var artName = tmpProduct.VArtName;
                            var artDesc = tmpProduct.VArtDesc;

                            // если было указано начальное SKU и оно отличается
                            if (startSkuId.HasValue && tmpProduct.SKUID != startSkuId)
                            {
                                var prod = packingProducts.FirstOrDefault(i => i.SKUID == startSkuId);
                                if (prod != null)
                                    skuName = prod.SKUName;
                                else
                                {
                                    // не нашли SKU в товаре, делаем запрос
                                    using (var mgr = IoC.Instance.Resolve<IBaseManager<SKU>>())
                                    {
                                        var startSku = mgr.Get(startSkuId.Value, FilterHelper.GetAttrEntity<SKU>(new []{SKU.SKUIDPropertyName, SKU.SKUNamePropertyName}));
                                        if(startSku == null)
                                            throw new OperationException("SKU {0} отсутствует в системе.", startSkuId.Value);
                                        skuName = startSku.SKUName;
                                        // т.к. товара с данным SKU небыло, укажем 1
                                        productCountSKU = 1;
                                    }
                                }
                            }
                            
                            // получаем модель по одному из "одинаковых" продуктов
                            var countModel = GetPackDialogModel(countField, productCountSKU, skuName, artName, artDesc, useStoredProductCountSku);

                            // отобразим количество
                            while (true) // делаем это для возврата на форму
                            {
                                if (GetViewService().ShowDialogWindow(countModel, true, false, "40%") == true)
                                {
                                    // если ругнется тут, то выведем ошибку как есть
                                    Decimal enteredValue = 1;

                                    // проверим указанное количество (не должно превышать кол-ва товара по SKU)
                                    var value = countModel[countField.Name];
                                    if (value == null)
                                        throw new OperationException("Не удалось прочитать параметр " +
                                                                     countField.Caption);

                                    enteredValue = Convert.ToDecimal(value);
                                    if (enteredValue == 0)
                                    {
                                        var message = string.Format("0 - недопустимое значение для '{0}'\r\n",
                                            countField.Caption);
                                        ShowWarning(message,
                                            string.Format("Ошибка при упаковке товара артикула '{0}'", art));
                                        continue;
                                    }

                                    // если не пакуем по sku, тогда ругаемся на кол-во
                                    if (startSkuId == null && enteredValue > productCountSKU)
                                    {
                                        var message =
                                            string.Format(
                                                "'{0}' не может быть больше кол-ва суммы товара по SKU = '{1}'\r\n",
                                                countField.Caption, productCountSKU);
                                        ShowWarning(message,
                                            string.Format("Ошибка при упаковке товара артикула '{0}'", art));
                                        continue;
                                    }

                                    //Запоминаем
                                    try
                                    {
                                        if (countModel[UseStoredProductCountSkuFieldName].To(false))
                                        {
                                            storedProductCountSku = enteredValue;
                                        }
                                        else
                                        {
                                            storedProductCountSku = null;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new DeveloperException(string.Format("Не найдено поле '{0}' в моделе countModel.", UseStoredProductCountSkuFieldName), ex);
                                    }

                                    // если есть что паковать
                                    if (packingProducts.Count > 0)
                                        Pack(uow, teCode, startSkuId, enteredValue, packingProducts, isNeedPackSourceTE);

                                    break;
                                }
                                else
                                {
                                    // если упаковываем более одного, то можем отменить как всю работу, так и только этот элемент
                                    if (packingProducts.Count > 1)
                                    {
                                        var message =
                                            string.Format(
                                                "Отменить упаковку текущей группы артикула '{0}' SKU '{1}'?\r\n\r\nДля отмены всего процесса упаковки нажмите 'Нет'.",
                                                art, sku);
                                        var dr = GetViewService().ShowDialog("Отмена упаковки", message,
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question,
                                            MessageBoxResult.Yes);

                                        if (dr == MessageBoxResult.Yes)
                                        {
                                            canceledProducsts.AddRange(productsWithSameSKU);
                                            break;
                                        }
                                    }

                                    return false;
                                }
                            }
                        }
                    }
                }

                // если были отмененные товары, то покажем их
                if (canceledProducsts.Count > 0)
                {
                    var canceledMessage = new StringBuilder("Следующие товары не были упакованы:\r\n");
                    foreach (var canceled in canceledProducsts)
                        canceledMessage.AppendLine(canceled.GetKey().ToString());

                    ShowWarning(canceledMessage.ToString());
                }
                else
                {
                    var lastProd = packingProducts.LastOrDefault();
                    if (lastProd != null)
                    {
                        using (var mgr = GetSpecManager<IBPProcessManager>(uow))
                        {
                            var needCheckWh = mgr.GetCpvValue("MANDANT", lastProd.MandantID.ToString(), "PackManyWHL2");
                            var owbPackStatus = mgr.CheckPackOWB(lastProd.ProductId.Value, needCheckWh);
                            if (Equals(owbPackStatus, NOT_PACKED))
                                return true;
                            var checkPackMessage = string.Format("Весь товар по накладной '{0}'{1} упакован.",
                                lastProd.VOWBName, owbPackStatus == WH_PACKED ? " на данном складе" : "");
                            GetViewService()
                                .ShowDialog("Упаковка товара", checkPackMessage, MessageBoxButton.OK,
                                    MessageBoxImage.Information, MessageBoxResult.OK);
                        }
                    }
                }

                return true;
            }
        }

        public bool ClosePack(TE teBase)
        {
            const string warningCaption = "Закрытие короба";
            IUnitOfWork uow = null;
            var isNeedDisposeUoW = false;

            string teCode = null;

            try
            {
                // Все TE входящие в базовую
                var allTes = new List<TE>();
                
                using (var mgr = GetManager<TE>(uow))
                {
                    var filter = string.Format("{0} in (select wmste.tecode from wmste start with (TECarrierStreakCode = '{1}') connect by prior TECode = TECarrierStreakCode)",
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(Product), TE.TECodePropertyName),
                        teBase.TECode);
                    allTes = mgr.GetFiltered(filter, GetModeEnum.Partial).ToList();
                }

                // Если есть не короба  - ругаемся
                if (allTes.Any(i => TEPackStatus.TE_PKG_NONE.Equals(i.TEPackStatus)))
                {
                    var message = string.Format("В коробе '{0}' находятся TE, не являющиеся коробами", teBase.TECode);
                    ShowWarning(message, warningCaption);
                    return false;
                }

                // Если есть не закрытые и не активированные - ругаемся
                if (allTes.Any(i => !TEPackStatus.TE_PKG_COMPLETED.Equals(i.TEPackStatus) && !TEPackStatus.TE_PKG_ACTIVATED.Equals(i.TEPackStatus)))
                {
                    var message = string.Format("В коробе '{0}' находятся короба, статус которых не позволяет их закрыть", teBase.TECode);
                    ShowWarning(message, warningCaption);
                    return false;
                }

                var teBaseCode = string.IsNullOrEmpty(teBase.TECarrierBaseCode) ? teBase.TECode : teBase.TECarrierBaseCode;
                // TE находящиеся на базовой и не закрытые 
                var tes = allTes.OrderBy(i => i.TECarrierStreakCode == teBaseCode).ThenBy(y => allTes.Any(i => i.TECarrierStreakCode == y.TECode)).ToList().Where(i => !TEPackStatus.TE_PKG_COMPLETED.Equals(i.TEPackStatus)).ToList();
                tes.Add(teBase);

                Product[] allProductList;

                // получим товар, который находится в упаковке
                using (var productMgr = GetManager<Product>(uow))
                {
                    var filter = string.Format("{0} in ('{1}')", 
                        SourceNameHelper.Instance.GetPropertySourceName(typeof (Product), Product.TECodePropertyName), 
                        string.Join("','", tes.Select(i => i.TECode).ToArray()));
                    allProductList = productMgr.GetFiltered(filter, GetModeEnum.Partial).ToArray();
                }

                var result = true;

                foreach (var te in tes)
                {
                    var isEmptyStreak = false;
                    teCode = te.TECode;
                    var productList = allProductList.Where(i => teCode.Equals(i.TECode)).ToArray();

                    // если товара нет, то ругаемся
                    if (productList.Length == 0)
                    {
                        // если нет вложенных ТЕ, то ругаемся
                        if (!allTes.Any(i => teCode.Equals(i.TECarrierStreakCode)))
                        {
                            var message = string.Format("В коробе '{0}' отсутствует товар", teCode);
                            ShowWarning(message, warningCaption);
                            result = false;
                            break;
                        }
                        isEmptyStreak = true;
                    }

                    TE packTE = null;
                    if (!isEmptyStreak)
                    {
                        // если в коробе товар более чем по одной расходной накладной или группе накладных, то не закрывать короб и ругаться
                        if (!CheckProductToEqualOWB(teCode, null, uow))
                        {
                            result = false;
                            break;
                        }

                        // получим атрибуты из настроек менеджера товара
                        var configs = new List<PMConfig>();
                        List<PMConfig> commonConfigs;
                        var distinctArts = productList.Select(i => i.ArtCode_R).Distinct().ToArray();

                        using (var mgr = GetSpecManager<IBPProcessManager>(uow))
                            configs = mgr.GetPMConfigListByArtCodeList(distinctArts, "OP_PACKING_CLOSE", null);
                        
                        var mustSetStr = PMMethods.MUST_SET.ToString();
                        var setNullStr = PMMethods.SET_NULL.ToString();
                        var mustManualCorrectStr = PMMethods.MUST_MANUAL_CORRECT.ToString();
                        var setFromSku2TteStr = PMMethods.SET_FROM_SKU2TTE.ToString();

                        var mustManualCorrectList =
                            configs.Where(i => i.MethodCode_r == mustManualCorrectStr).DistinctBy(i => i.ObjectName_r).ToArray();
                        var setNullList =
                            configs.Where(i => i.MethodCode_r == setNullStr).DistinctBy(i => i.ObjectName_r).ToArray();
                        var mustSetList =
                            configs.Where(i => i.MethodCode_r == mustSetStr).DistinctBy(i => i.ObjectName_r).ToArray();
                        var setFromSku2TteList =
                            configs.Where(i => i.MethodCode_r == setFromSku2TteStr).DistinctBy(i => i.ObjectName_r).ToArray();
                        var skuOnTe = productList.Select(p => p.SKUID).Distinct().ToArray();
                        var needSetFromSku2Tte = setFromSku2TteList.Length > 0 && skuOnTe.Length == 1;
                        if (needSetFromSku2Tte)
                        {
                            packTE = GetTeAndSetAttributes(uow, teCode, setFromSku2TteList, skuOnTe[0]);
                        }

                        if (mustManualCorrectList.Length > 0 || setNullList.Length > 0 || mustSetList.Length > 0)
                        {
                            packTE = GetCorrectedTE(uow, packTE, teCode, mustManualCorrectList, mustSetList, setNullList);
                        }
                    }
                    using (var mgr = GetSpecManager<IBPProcessManager>(uow))
                    {
                        // если в процессе поменяли ТЕ, сохраняем его
                        if (packTE != null && packTE.IsDirty)
                            mgr.UpdateEntity(packTE);

                        mgr.CloseBox("OP_PACKING_CLOSE", teCode);
                    }
                }
                return result;
            }
            catch (UserInterrrupteException)
            {
                // Ничего не делаем - просто завершаем операцию
                _log.Info("Пользователь прервал операцию");
            }
            catch (Exception ex)
            {
                var message = (string.IsNullOrEmpty(teCode) ? "Ошибка при закрытии короба." : "Ошибка при закрытии короба '" + teCode + "'. ") + ExceptionHelper.GetErrorMessage(ex, false);
                _log.Error(message, ex);

                ShowWarning(message);
            }
            return false;
        }

        public bool OpenPack(string teCode, string placeCode, string operation)
        {
            IUnitOfWork uow = null;

            using (var mgr = GetSpecManager<IBPProcessManager>(uow))
                mgr.OpenBox(operation, teCode, placeCode);

            return true;
        }

        public bool UnpackAll(string teCode, string placeCode)
        {
            IUnitOfWork uow = null;

            using (var mgr = GetSpecManager<IBPProcessManager>(uow))
                mgr.UnpackTe(teCode, placeCode);

            return true;
        }

        public bool PackSourceTE(string[] teCodeList, string placeCode)
        {
            if (teCodeList.Length == 0)
                throw new OperationException("Не указано ни одной ТЕ");

            try
            {
                IUnitOfWork uow = null;

                // определяем уникальные ТЕ
                var teList = teCodeList.Distinct().ToArray();

                // проверяем типы ТЕ на предмет упаковываемости
                var nonPackingTETypeFilter = string.Format("tetypecode in (select tetypecode_r from wmste where tecode in ('{0}')) and NVL(pkgCustomParamValue.getCPVValue('TETYPE', tetypecode, 'TETypeIsPackingL2'),'0') != '1' ", string.Join("','", teList));
                using (var mgr = GetManager<TEType>(uow))
                {
                    var teTypes = mgr.GetFiltered(nonPackingTETypeFilter, GetModeEnum.Partial).ToArray();
                    if (teTypes.Length > 0)
                    {
                        var message = string.Format("Тип(ы) ТЕ '{0}' - не упаковка!", string.Join("','", teTypes.Select(i => i.TETypeCode)));
                        ShowWarning(message);
                        return false;
                    }
                }

                // бежим по всем ТЕ и пакуем
                foreach (var teCode in teList)
                {
                    try
                    {
                        // проставляем статус упаковки
                        using (var mgr = GetManager<TE>(uow))
                            ((IStateChange)mgr).ChangeStateByKey(teCode, "OP_PACKING_CREATE_BASEDON");

                        // получаем товар в данном коробе
                        Product[] teProducts = null;
                        using (var mgr = GetManager<Product>(uow))
                        {
                            var teFiledName = SourceNameHelper.Instance.GetPropertySourceName(typeof(Product), Product.TECodePropertyName);
                            var filter = string.Format("{0}='{1}'", teFiledName, teCode);
                            teProducts = mgr.GetFiltered(filter).ToArray();
                        }

                        // пакуем все товары на самой ТЕ
                        bool isUnpack = false;
                        decimal? storedProductCountSku = null;
                        Pack(teProducts, teCode, null, false, true, ref isUnpack, false, ref storedProductCountSku);
                    }
                    catch (Exception ex)
                    {
                        var message = "Ошибка при упаковке ТЕ целиком. " + ExceptionHelper.ExceptionToString(ex);
                        _log.Error(message, ex);

                        // убираем статус упаковки
                        try
                        {
                            using (var mgr = GetManager<TE>(uow))
                                ((IStateChange)mgr).ChangeStateByKey(teCode, "OP_PACKING_CREATE_BASEDON");
                        }
                        catch (Exception ex1)
                        {
                            _log.Error(string.Format("Ошибка при упаковке ТЕ целиком. Ну удалось вернуть статус 'не упаковка' для ТЕ '{0}'. {1}", teCode, ExceptionHelper.ExceptionToString(ex1)), ex1);
                            throw;
                        }

                        ShowWarning(message);
                    }
                }

                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private TE GetTeAndSetAttributes(IUnitOfWork uow, string teCode, PMConfig[] setFromConfigs, decimal skuId)
        {
            if (setFromConfigs == null)
                return null;

            // получаем нашу ТЕ
            TE packTE = null;
            using (var mgr = GetManager<TE>(uow))
                packTE = mgr.Get(teCode);

            if (packTE == null)
                throw new OperationException("Не найдена ТЕ " + teCode);

            // получим список полей ТЕ по атрибутам
            var packFields = DataFieldHelper.Instance.GetDataFields(typeof(TE), SettingDisplay.Detail);
            SKU2TTE sku2tte = null;
            var foundSku2Tte = false;

            foreach (var config in setFromConfigs)
            {
                var field = packFields.FirstOrDefault(i => i.Name.EqIgnoreCase(config.ObjectName_r));
                if (field == null)
                    throw new SettingsException("В менеджере товара задан неизвестный параметр '{0}' для метода SET_FROM_SKU2TTE.", config.ObjectName_r);

                // если поле заполнено - пропускаем, работаем только с пустыми атрибутами ТЕ
                var oldValue = packTE.GetProperty(config.ObjectName_r);
                var fieldType = field.FieldType.GetNonNullableType();
                if (oldValue != null && (fieldType == typeof (decimal) && (decimal)oldValue != 0))
                    continue;
                
                if (!foundSku2Tte)
                    using (var mgr = GetManager<SKU2TTE>(uow))
                    {
                        var sku2tteList = mgr.GetFiltered(string.Format("SKUID_R = {0} and TETYPECODE_R = '{1}'"
                            , skuId, packTE.TETypeCode), GetModeEnum.Partial);
                        if (sku2tteList == null)
                            break;
                        var sku2tteArray = sku2tteList.ToArray();
                        sku2tte = sku2tteArray.Length == 1 ? sku2tteArray[0] : sku2tte;
                        foundSku2Tte = true;
                        if (sku2tte == null)
                            break;
                    }

                var newValue = sku2tte.GetProperty("SKU2TTE" + field.Name.Substring(2, field.Name.Length - 2));
                packTE.SetProperty(field.Name, newValue);
            }

            return packTE;
        }

        private TE GetCorrectedTE(IUnitOfWork uow, TE packTE, string teCode, PMConfig[] mustManualCorrectList, PMConfig[] mustSetList, PMConfig[] setNullList)
        {
            // проверим корректно ли настроен менеджер
            // todo: непонятная проверка, закомментировал
            //var mustSetWithoutCorrect = mustSetList.Where(i => mustManualCorrectList.All(j => j.ObjectName_r != i.ObjectName_r)).ToArray();
            //if (mustSetWithoutCorrect.Length > 0)
            //    throw new SettingsException("Для параметра '{0}' существует атрибут MUST_SET, но отсутствует MUST_MANUAL_CORRECT.",
            //        mustSetWithoutCorrect[0].ObjectName_r);

            //var setNullWithoutCorrect = setNullList.Where(i => mustManualCorrectList.All(j => j.ObjectName_r != i.ObjectName_r)).ToArray();
            //if (setNullWithoutCorrect.Length > 0)
            //    throw new SettingsException("Для параметра '{0}' существует атрибут SET_NULL, но отсутствует MUST_MANUAL_CORRECT.",
            //        setNullWithoutCorrect[0].ObjectName_r);

            // получим список полей ТЕ по атрибутам
            var fieldList = new List<ValueDataField>();
            var packFields = DataFieldHelper.Instance.GetDataFields(typeof(TE), SettingDisplay.Detail);

            // получаем нашу ТЕ
            if (packTE == null)
            {
                using (var mgr = GetManager<TE>(uow))
                    packTE = mgr.Get(teCode);
                if (packTE == null)
                    throw new OperationException("Не найдена ТЕ " + teCode);
            }

            var minmaxattributes = new List<PMConfig>();

            foreach (var m in mustManualCorrectList)
            {
                var field = packFields.FirstOrDefault(i => i.Name.EqIgnoreCase(m.ObjectName_r));
                if (field == null)
                    throw new SettingsException("При указании метода MUST_SET менеджера товара задан неизвестный параметр '{0}'.", m.ObjectName_r);

                var vfield = new ValueDataField(field);
                // признак того, что поле MUST_SET
                vfield.IsRequired = mustSetList.Any(i => i.ObjectName_r.EqIgnoreCase(m.ObjectName_r));
                var nullValue = setNullList.FirstOrDefault(i => i.ObjectName_r.EqIgnoreCase(m.ObjectName_r));
                vfield.Value = nullValue == null ? packTE.GetProperty(m.ObjectName_r) : null;
                fieldList.Add(vfield);

                if (vfield.IsRequired)
                {
                    minmaxattributes.Add(new PMConfig
                    {
                        MethodCode_r = PMConfigValidator.MinMaxValidateMethodCode,
                        ObjectName_r = vfield.Name,
                        ValidatorHandle = fieldValue =>
                        {
                            if (fieldValue == null)
                                return string.Empty;

                            var comparable = fieldValue as IComparable;
                            if (comparable == null)
                                throw new DeveloperException("Type {0} is not implement IComparable", fieldValue);

                            var notNullType = field.FieldType.GetNonNullableType();
                            var rangeTypedValue = SerializationHelper.ConvertToTrueType(0, notNullType);

                            return comparable.CompareTo(rangeTypedValue) > 0 ? string.Empty : "Значение меньше допустимого";
                        }
                    });
                }
            }

            // заполняем аттрибуты
            var attributes = new List<PMConfig>();
            attributes.AddRange(mustSetList);
            attributes.AddRange(mustManualCorrectList);
            attributes.AddRange(minmaxattributes);

            //
            // отображаем форму
            //
            var model = new ExpandoObjectViewModelBase();

            // создадим валидатор
            var pmConfigValidator = new PMConfigValidator(model.Source);
            model.Source.SetValidator(pmConfigValidator);
            pmConfigValidator.Attributes = attributes.ToArray();
            model.Fields = new ObservableCollection<ValueDataField>(fieldList);
            model.PanelCaption = string.Format("Закрытие короба '{0}'", packTE.TECode);

            // генерим уникальный суффикс для возможности настройки
            model.SetSuffix("E5D42496-64A8-4BB8-AD6F-024CCD8D349C");
            model.SuspendDispose = true;
            try
            {
                //INFO: здесь нельзя восстанавливать настройки
                bool needReturn;
                do
                {
                    needReturn = false;

                    var close = GetViewService().ShowDialogWindow(model, false, true, "40%");

                    // если пользователь отказался - выходим
                    if (close != true)
                        throw new UserInterrrupteException();

                    foreach (var f in fieldList)
                    {
                        var value = model[f.Name];
                        packTE.SetProperty(f.Name, value);

                        if (f.Name == TE.TEWeightPropertyName)
                        {
                            var message = CheckTeWeight(uow, packTE.TECode, value.ConvertTo<decimal>());
                            if (!String.IsNullOrEmpty(message))
                            {
                                ShowWarning(message, "Закрытие короба");
                                needReturn = true;
                            }
                        }
                    }
                } while (needReturn);
            }
            finally
            {
                model.SuspendDispose = false;
                model.Dispose();
            }

            return packTE;
        }

        /// <summary>
        /// Проверка вес ТЕ
        /// </summary>
        /// <returns>Текст ошибки, если таковая имелась. В противном случае null</returns>
        private static string CheckTeWeight(IUnitOfWork uow, string key, decimal factWeight)
        {
            decimal calcWeight;
            decimal? calcDev;

            using (var bpmngr = GetSpecManager<IBPProcessManager>(uow))
                bpmngr.GetTEWeightControl(key, out calcWeight, out calcDev);

            if (!calcDev.HasValue)
                return null;

            var checkWeight = (factWeight >= (calcWeight - calcDev)) && (factWeight <= (calcWeight + calcDev));
            if (checkWeight)
                return null;

            var diff = factWeight > (calcWeight + calcDev)
                ? factWeight - (calcWeight + calcDev)
                : (calcWeight - calcDev) - factWeight;

            return string.Format(
                "{0} = {1} гр.\r\nРасчетный вес ТЕ = {2} гр., Погрешность = {3} гр.",
                factWeight > (calcWeight + calcDev) ? "Перевес " : "Недовес ", diff, calcWeight, calcDev);
        }


        private static bool Pack(IUnitOfWork uow, string teCode, decimal? startSkuId, decimal packCount, List<Product> packingProducts, bool isNeedPackSourceTE)
        {
            var skuIdList = packingProducts.Select(i => i.SKUID).Distinct().ToArray();

            // Проверка на существование привязки SKU к Типу ТЕ (и создание в случае отсутствия)
            // http://mp-ts-nwms/issue/wmsMLC-3439
            if (!CheckSKU2TTE(uow, teCode, skuIdList, packCount))
                return false;

            // пакуем
            using (var mgr = GetSpecManager<IBPProcessManager>(uow))
                if (startSkuId == null)
                    // ReSharper disable once PossibleInvalidOperationException
                    mgr.PackProductLst(packingProducts.Select(i => i.ProductId.Value), packingProducts.Where(i => i.IsDirty), teCode, packCount, isNeedPackSourceTE);
                else
                    mgr.PackProductLstBySKU(packingProducts.Select(i => i.ProductId.Value), packingProducts.Where(i => i.IsDirty), teCode, startSkuId.Value, packCount, true);

            // пикаем после успешной упаковки
            SystemSounds.Beep.Play();

            return true;
        }

        private ExpandoObjectViewModelBase GetPackDialogModel(DataField countField, decimal productCountSKU, string skuName, string artName, string artDesc, bool useStoredProductCountSku)
        {
            // поле для ввода количества
            var vCountField = new ValueDataField(countField)
            {
                IsRequired = true,
                LabelPosition = "Left",
                Value = productCountSKU,
                IsLabelFontWeightBold = true,
                Order = 3
            };

            var vArtNameField = new ValueDataField()
            {
                Caption = "Наименование артикула",
                Name = "ArtName",
                FieldType = typeof(string),
                IsEnabled = false,
                LabelPosition = "Left",
                IsRequired = true,
                Order = 0
            };

            var vArtDescField = new ValueDataField()
            {
                Caption = "Описание артикула",
                Name = "ArtDesc",
                FieldType = typeof(string),
                IsEnabled = false,
                LabelPosition = "Left",
                IsRequired = true,
                Order = 1
            };

            var vSKUNameField = new ValueDataField()
            {
                Caption = "Единица учета",
                Name = "SKUName",
                FieldType = typeof(string),
                IsEnabled = false,
                LabelPosition = "Left",
                IsRequired = true,
                Order = 2
            };

            var vUseStoredProductCountSkuFeld = new ValueDataField()
            {
                Caption = "Запомнить количество товара",
                Name = UseStoredProductCountSkuFieldName,
                FieldType = typeof(bool),
                IsEnabled = true,
                LabelPosition = "Left",
                Order = 4
            };


            vSKUNameField.Value = skuName;
            vArtDescField.Value = artDesc;
            vArtNameField.Value = artName;
            vUseStoredProductCountSkuFeld.Value = useStoredProductCountSku;

            // создадим модель
            var countModel = new ExpandoObjectViewModelBase
            {
                Fields = new ObservableCollection<ValueDataField>(new[] { vSKUNameField, vArtDescField, vArtNameField, vCountField, vUseStoredProductCountSkuFeld }),
                PanelCaption = string.Format("Упаковка товара")
            };

            if (FontSize.HasValue && FontSize.Value > 0)
                foreach (var f in countModel.Fields)
                    f.Set(ValueDataFieldConstants.FontSize, FontSize);

            countModel.SetSuffix("7219230F-FE10-4438-8FE5-5E60CA74E620");
            return countModel;
        }

        private static void ShowWarning(string message, string caption = "Упаковка товара")
        {
            GetViewService().ShowDialog(caption, message, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
        }

        private static string LastOkSKU2TTETECode;
        private static List<decimal> LastOkSKU2TTESKUId = new List<decimal>();

        private static bool CheckSKU2TTE(IUnitOfWork uow, string teCode, decimal[] packingSKUIdList, decimal enteredValue)
        {
            var listToCheck = new List<decimal>(packingSKUIdList.Distinct());
            // быстрая проверка, что такую связку мы уже проверяли
            if (LastOkSKU2TTETECode == teCode)
            {
                foreach (var item in packingSKUIdList.Where(item => LastOkSKU2TTESKUId.Contains(item)))
                    listToCheck.Remove(item);
            }

            // если проверять нечего - все ок
            if (listToCheck.Count == 0)
                return true;

            // определяем sku для которых нет связи с типом нашей ТЕ
            var skuIdListStr = string.Join(",", listToCheck);
            var skuFilter = string.Format("skuid in ({0}) and not exists(select 1 from wmssku2tte s join wmste t on s.tetypecode_r = t.tetypecode_r where skuid_r = skuid and t.tecode = '{1}')", skuIdListStr, teCode);
            SKU[] skuLst = null;

            var attrEntity = FilterHelper.GetAttrEntity<SKU>(SKU.SKUIDPropertyName, SKU.SKUNamePropertyName);
            using (var mgr = GetManager<SKU>(uow))
                skuLst = mgr.GetFiltered(skuFilter, attrEntity).ToArray();

            // если не все связи есть - добавляем
            if (skuLst.Length > 0)
            {
                // получаем тип нашей ТЕ
                var teType = new TEType();
                using (var mgr = GetManager<TEType>(uow))
                {
                    var filter = string.Format("tetypecode = (select max(tetypecode_r) from wmste where tecode = '{0}')", teCode);
                    teType = mgr.GetFiltered(filter, GetModeEnum.Partial).FirstOrDefault();
                }

                if (teType == null)
                    throw new OperationException("Для ТЕ {0} не удалось определить тип ТЕ", teCode);

                foreach (var sku in skuLst)
                {
                    var mes =
                        string.Format(
                            "Отсутствует связь SKU '{0}' к Типу ТЕ '{1}'. Добавить связь?\r\n'Да' = добавить автоматически\r\n'Нет' = добавить вручную\r\n'Отмена' = отмена упаковки",
                            sku.SKUName, teType.TETypeName);
                    var mesResult = GetViewService()
                        .ShowDialog("Упаковка товара", mes, MessageBoxButton.YesNoCancel, MessageBoxImage.Question,
                            MessageBoxResult.Yes);

                    // пользователь прерывает упаковку
                    if (mesResult == MessageBoxResult.Cancel)
                        return false;

                    var sku2tte = new SKU2TTE
                    {
                        SKU2TTESKUID = sku.SKUID,
                        TETypeCode = teType.TETypeCode,
                        SKU2TTEQuantity = enteredValue,
                        SKU2TTEQuantityMax = enteredValue,
                        SKU2TTEHeight = teType.Height,
                        SKU2TTELength = teType.Length,
                        SKU2TTEWidth = teType.Width,
                        SKU2TTEMaxWeight = teType.MaxWeight
                    };

                    switch (mesResult)
                    {
                        case MessageBoxResult.Yes:
                            using (var mgr = GetManager<SKU2TTE>(uow))
                                mgr.Insert(ref sku2tte);
                            break;

                        case MessageBoxResult.No:
                            var vm = IoC.Instance.Resolve<IObjectViewModel<SKU2TTE>>();
                            vm.SetSource(sku2tte);
                            var dialogResult = GetViewService()
                                .ShowDialogWindow(vm, true, true, height: "80%", width: "80%", noButtons: true);
                            var saved = (SKU2TTE)vm.GetSource();

                            // если ссылки совпадают, заначит не сохранили - прерываем выполенение
                            if (ReferenceEquals(sku2tte, saved))
                                return false;

                            break;
                    }
                }
            }

            if (LastOkSKU2TTETECode == teCode)
                LastOkSKU2TTESKUId.AddRange(listToCheck);
            else
            {
                LastOkSKU2TTETECode = teCode;
                LastOkSKU2TTESKUId = listToCheck;
            }
            return true;
        }

        private static bool CheckByProductManager(
            PMConfig[] artConfs,
            Product product,
            ObservableCollection<DataField> productFields,
            string art,
            List<Product> packingPropducts,
            List<Product> canceledProducsts,
            out bool askForQuantity)
        {
            // выбираем контролируемые
            var mustSetStr = PMMethods.MUST_SET.ToString();
            var mustCompareStr = PMMethods.MUST_COMPARE.ToString();
            var pieceOnlyStr = PMMethods.PIECE_ONLY.ToString();

            var mustSetList = artConfs.Where(i => mustSetStr.EqIgnoreCase(i.MethodCode_r)).ToArray();
            var mustCompareList = artConfs.Where(i => mustCompareStr.EqIgnoreCase(i.MethodCode_r)).ToArray();
            var rangeList = artConfs.Where(i => !string.IsNullOrEmpty(i.MethodCode_r) && i.MethodCode_r.Contains("RANGE_")).ToArray();

            // если есть хотя бы одна PIECE_ONLY, то не нужно указывать кол-во
            askForQuantity = !artConfs.Any(i => pieceOnlyStr.EqIgnoreCase(i.MethodCode_r));

            var fieldList = new List<ValueDataField>();

            // собирем поля MUST_SET
            foreach (var m in mustSetList)
            {
                //TODO: могут быть проблемы с не nullable полями. Нужно сравнивать c default или сделать метод IsNull() в CustomProperty
                if (product.GetProperty(m.ObjectName_r) != null)
                    continue;

                // получаем контролируемое поле
                var field = productFields.FirstOrDefault(i => i.Name.EqIgnoreCase(m.ObjectName_r));
                if (field == null)
                    throw new DeveloperException("Ошибка в настройках MUST_SET менеджера товара. Задан неизвестный параметр '{0}'.", m.ObjectName_r);

                if (fieldList.Any(i => i.FieldName == field.FieldName))
                    continue;

                // создаем поле для запроса
                var vfield = new ValueDataField(field);
                // признак того, что поле MUST_SET
                vfield.IsRequired = true;
                vfield.IsLabelFontWeightBold = true;
                fieldList.Add(vfield);
            }

            // собирем поля MUST_COMPARE
            foreach (var m in mustCompareList)
            {
                // получаем контролируемое поле
                var field = productFields.FirstOrDefault(i => i.Name.EqIgnoreCase(m.ObjectName_r));
                if (field == null)
                    throw new DeveloperException("Ошибка в настройках MUST_COMPARE менеджера товара. Задан неизвестный параметр '{0}'.", m.ObjectName_r);

                // если есть Range - то COMPARE - ignore
                if (rangeList.FirstOrDefault(i => i.ObjectName_r.Equals(m.ObjectName_r)) != null)
                {
                    var pm = artConfs.FirstOrDefault(i => mustCompareStr.EqIgnoreCase(i.MethodCode_r) && m.ObjectName_r.EqIgnoreCase(i.ObjectName_r));
                    if (pm != null)
                        //artConfs = artConfs.ToList().Remove(pm);
                        artConfs = artConfs.Where(p => p != pm).ToArray();
                }

                if (fieldList.Any(i => i.FieldName == field.FieldName))
                    continue;

                // создаем поле для запроса
                var vfield = new ValueDataField(field);
                // признак того, что поле MUST_COMPARE
                vfield.IsRequired = false;
                fieldList.Add(vfield);
            }

            // собирем поля RANGE
            foreach (var w in rangeList)
            {
                // если нет пары в COMPARE - то ignore
                if (mustCompareList.FirstOrDefault(i => i.ObjectName_r.Equals(w.ObjectName_r)) == null)
                    continue;

                // получаем контролируемое поле
                var field = productFields.FirstOrDefault(i => i.Name.EqIgnoreCase(w.ObjectName_r));
                if (field == null)
                    throw new DeveloperException("Ошибка в настройках RANGE_*** менеджера товара. Задан неизвестный параметр '{0}'.", w.ObjectName_r);

                if (fieldList.Any(i => i.FieldName == field.FieldName))
                    continue;

                // создаем поле для запроса
                var vfield = new ValueDataField(field) {IsRequired = true};
                fieldList.Add(vfield);
            }

            // если спрашивать нечего - безусловно добавляем продукт в итоговый список на упаковку
            if (fieldList.Count == 0)
            {
                packingPropducts.Add(product);
                return true;
            }

            // создаем модель для отображения
            var model = new ExpandoObjectViewModelBase
            {
                Fields = new ObservableCollection<ValueDataField>(fieldList),
                PanelCaption = string.Format("Упаковка товара '{0}' артикул '{1}'", product.GetKey(), art)
            };

            foreach (var field in fieldList)
                model.Source.SetInitialValue(field.FieldName, product.GetProperty(field.FieldName));
            var pmConfigValidator = new PMConfigValidator(model.Source) { Attributes = artConfs.ToArray() };
            model.Source.SetValidator(pmConfigValidator);
            model.SuspendDispose = true;

            // генерим уникальный суффикс для возможности настройки
            model.SetSuffix("2EEF030F-06D1-4121-8E6A-0A8C8DF26D38");

            try
            {
                while (true) // делаем цикл для возврата на форму
                {
                    //INFO: здесь нельзя восстанавливать настройки
                    if (GetViewService().ShowDialogWindow(model, false, true, "30%") == true)
                    {
                        packingPropducts.Add(product);
                        return true;
                    }
                    else
                    {
                        var dr = GetViewService().ShowDialog("Отмена упаковки",
                            "Отменить упаковку текущего товара?\r\n\r\nДля отмены всего процесса упаковки нажмите 'Нет'.",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question,
                            MessageBoxResult.Yes);

                        if (dr == MessageBoxResult.Yes)
                        {
                            canceledProducsts.Add(product);
                            return true;
                        }

                        return false;
                    }
                }
            }
            finally
            {
                model.SuspendDispose = false;
                model.Dispose();
            }
        }

        /// <summary>
        /// Проверяем нужно ли распаковывать товар. Если нужно, то тут же и распаковываем
        /// </summary>
        /// <returns>false - распаковка не нужна, иначе true</returns>
        private static bool UnpackIfNeed(IUnitOfWork uow, Dictionary<string, PMConfig[]> artConfigs, Product[] items)
        {
            var methodStr = PMMethods.CHECK_PIECE.ToString();
            var unpackedItems = artConfigs.Where(i => i.Value.Any(j => methodStr.EqIgnoreCase(j.MethodCode_r))).ToArray();

            // если распаковывать нечего - выходим
            if (unpackedItems.Length == 0)
                return false;

            // список артикулов товара на распаковку
            var arts = unpackedItems.Select(i => i.Key).ToArray();
            var productUnpackMessage = "Будут распакованы товары со следующими артикулами:" + Environment.NewLine + string.Join(Environment.NewLine, arts);

            // отображаем пользователю диалог
            var dr = GetViewService().ShowDialog("Распаковка товара", productUnpackMessage, MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.OK);

            // если пользователь не согласился, то выходим и ничего не делаем. Прерываем процесс упаковки
            if (dr != MessageBoxResult.OK)
                return true;

            // вычисляем товары, которые нужно распаковать
            var unpackProductList = items.Where(i => arts.Any(j => j == i.ArtCode_R));

            bool isError = false;
            //расформируем короб по системе (строка товара преобразуется в строку товара с другой SKU)
            using (var mgr = GetSpecManager<IBPProcessManager>(uow))
            {
                foreach (var product in unpackProductList)
                {
                    var prod = mgr.UnPack(product.ProductId.Value);
                    if (prod != null && product.SKUID == prod.SKUID)
                        isError = true;
                }
            }

            return !isError;
        }

        private static IViewService GetViewService()
        {
            return IoC.Instance.Resolve<IViewService>();
        }

        private static IBaseManager<T> GetManager<T>(IUnitOfWork uow)
            where T : WMSBusinessObject
        {
            var mgr = IoC.Instance.Resolve<IBaseManager<T>>();
            if (uow != null)
                mgr.SetUnitOfWork(uow);
            return mgr;
        }

        private static T GetSpecManager<T>(IUnitOfWork uow)
            where T : ITrueBaseManager
        {
            var mgr = IoC.Instance.Resolve<T>();
            if (uow != null)
                mgr.SetUnitOfWork(uow);
            return mgr;
        }

        private static double? GetFontSize()
        {
            if (_cachedFontSizeLoaded)
                return _cachedFontSize;

            using (var mgr = IoC.Instance.Resolve<IBaseManager<CustomParamValue>>())
            {
                var filter = string.Format("CPV2ENTITY='CLIENT' and CUSTOMPARAMCODE_R='ClientDesignFontSizePackL2' and CPVKEY='{0}'", WMSEnvironment.Instance.ClientCode);
                var cpvArray = mgr.GetFiltered(filter).ToArray();
                if (cpvArray.Length == 1)
                    _cachedFontSize = Convert.ToDouble((cpvArray[0]).CPVValue);
            }
            _cachedFontSizeLoaded = true;
            return _cachedFontSize;
        }
        #endregion

        /// <summary>
        /// Вложить
        /// </summary>
        /// <param name="currentPlaceCode"></param>
        /// <param name="truckCode"></param>
        /// <param name="workerID"></param>
        public void PutIn(string currentPlaceCode, string truckCode, decimal workerID)
        {
            IUnitOfWork uow = null;

            var vTECodeField = new ValueDataField()
            {
                Caption = "Короб",
                Description = "Код ТЕ",
                Name = "TECODE",
                FieldType = typeof(string),
                IsEnabled = true,
                LabelPosition = "Left",
                SetFocus = true,
                IsRequired = true,
                Order = 0,
            };

            var vTECodeBaseField = new ValueDataField()
            {
                Caption = "Грузовое место",
                Description = "Код ТЕ",
                Name = "TEBASECODE",
                FieldType = typeof(string),
                IsEnabled = true,
                LabelPosition = "Left",
                SetFocus = false,
                IsRequired = true,
                CloseDialog = true,
                Order = 1
            };

            // создадим модель
            var model = new PutInViewModel
            {
                Fields = new ObservableCollection<ValueDataField>(new[] {vTECodeField, vTECodeBaseField}),
                FieldNames = new[] { vTECodeField.Name, vTECodeBaseField.Name },
                PropertyChangeHandler = new TimerPropertyChangeHandler()
            };

            model.SetSuffix("C5F59783-FD52-4H3A-AB43-8787F326UUER");

           

            if (FontSize.HasValue && FontSize.Value > 0)
                foreach (var f in model.Fields)
                    f.Set(ValueDataFieldConstants.FontSize, FontSize);

            // отображаем форму
            while (true) 
            {
                if (GetViewService().ShowDialogWindow(model, false, false, "30%") != true)
                    break;

                // проверим, что поля заполненны
                var teCode = model.Get<string>(vTECodeField.Name).ToUpper();
                if (string.IsNullOrEmpty(teCode))
                {
                    ShowWarning("Не указан номер короба", "Ошибка ввода данных");
                    continue;
                }

                var teBaseCode = model.Get<string>(vTECodeBaseField.Name).ToUpper();
                if (string.IsNullOrEmpty(teBaseCode))
                {
                    ShowWarning("Не указано грузовое место", "Ошибка ввода данных");
                    continue;
                }

                TransportTask ttask = null;
                // проверка короба
                if (!CheckTE(teCode, teBaseCode, currentPlaceCode, ref ttask))
                    continue;

                // Проверка ГМ
                if (!CheckPackage(teCode, teBaseCode, currentPlaceCode))
                    continue;

                // вкладываем короб в короб
                if (!CompleteTTask(truckCode, workerID, DateTime.Now - model.StartTime, ttask, teBaseCode))
                    continue;

                model.RefreshStartTime();
                model["TECODE"] = string.Empty;
            }

            model.Dispose();
        }

        private static bool CompleteTTask(string truckCode, decimal workerID, TimeSpan span, TransportTask ttask, string teBaseCode)
        {
            IUnitOfWork uow = null;
            string message = null;

            using (var mgr = GetSpecManager<IBPProcessManager>(uow))
            {
                // квитируем ЗНТ
                var context = new BpContext {Items = new object[] {ttask}};
                context.Set("TRUCKCODE", truckCode);
                context.Set("WORKERCODE", workerID);
                context.Set("TECODE", teBaseCode);
                context.Set("TIMESPAN", span);
                context.Set("ISNOTVIEW", true);
                mgr.Parameters.Add(BpContext.BpContextArgumentName, context);
                var manualEvent = new ManualResetEvent(false);
                mgr.Run("TTCONFIRMATION", ctx =>
                  DispatcherHelper.BeginInvoke(new Action(() =>
                  {
                      manualEvent.Set();
                      message = context.Get<String>("SUCCESSMESSAGE");
                  })));
                // ожидаем когда выполнится
                manualEvent.WaitOne();
            }
            return !string.IsNullOrEmpty(message);
        }

        /// <summary>
        /// Проверка ТЕ, что ее можно вложить в др. ТЕ
        /// исходное место по ЗНТ = конечное место по ЗНТ = текущее место упаковки
        /// (стратегия в ЗНТ = PLACE_TE2TE_CREATE) или ((PLACE_TE2TE или PLACE_OWB_TE2TE) и TTaskTargetTE = ГМ)
        /// </summary>
        /// <param name="teCode"></param>
        /// <param name="placeCode"></param>
        /// <returns></returns>
        private static bool CheckTE(string teCode, string teBaseCode, string placeCode, ref TransportTask ttask)
        {
            List<TransportTask> tTasks;
            IUnitOfWork uow = null;

            using (var mgr = GetManager<TransportTask>(uow))
            {
                var filter = string.Format("TECODE_R = '{0}' and STATUSCODE_R = 'TTASK_CREATED'", teCode);
                tTasks = mgr.GetFiltered(filter, FilterHelper.GetAttrEntity<TransportTask>(TransportTask.TtaskIDPropertyName, TransportTask.TECodePropertyName, TransportTask.TaskCurrentPlacePropertyName, TransportTask.TaskFinishPlacePropertyName, TransportTask.TaskStartPlaceCodePropertyName, TransportTask.TargetTEPropertyName, TransportTask.StrategyPropertyName)).ToList();
            }

            if (tTasks.Count == 1)
            {
                if (!(string.Equals(placeCode, (tTasks[0]).TaskStartPlaceCode) &&
                      string.Equals(placeCode, (tTasks[0]).TaskFinishPlace)))
                {
                    ShowWarning("Место в ЗНТ не соответствует месту упаковки", "Ошибка вложения");
                    return false;
                }
                if (string.Equals("PLACE_TE2TE_CREATE", (tTasks[0]).Strategy) ||
                   (new[] { "PLACE_TE2TE", "PLACE_OWB_TE2TE" }.Contains(tTasks[0].Strategy) && string.Equals(teBaseCode, (tTasks[0]).TargetTE.ToUpper())))
                {
                    ttask = tTasks[0];
                    return true;
                }

                ShowWarning("С указанной стратегией в ЗНТ нельзя вложить ТЕ", "Ошибка вложения");
                return false;
            }

            ShowWarning(tTasks.Count > 1
                    ? string.Format("Для короба '{0}' существует более 2 ЗНТ", teCode)
                    : string.Format("Для короба '{0}' нет ЗНТ", teCode),
                "Ошибка вложения");
            return false;
        }

        /// <summary>
        /// Проверка ТЕ (грузовое место), что в нее можно вложить др. ТЕ
        /// 1) ГМ существует 2) на него нет ЗНТ 3) оно находится на текущем месте упаковки
        /// 4) статус "TE_PKG_CREATED" (тогда переводим в статус "TE_PKG_ACTIVATED") или статус "TE_PKG_ACTIVATED"
        /// </summary>
        /// <param name="teCode"></param>
        /// <param name="placeCode"></param>
        /// <returns></returns>
        private static bool CheckPackage(string teCode, string teBaseCode, string placeCode)
        {
            IUnitOfWork uow = null;

            using (var mgr = GetSpecManager<IBPProcessManager>(uow))
            {
                var existTe = mgr.CheckInstanceEntity("TE", teBaseCode);

                if (existTe == 0)
                {
                    // необхоимо создать новый короб
                    var context = new BpContext();
                    context.Set("CURRENTPLACE", placeCode);
                    context.Set("TECODE", teBaseCode);
                    context.Set("OPTIONALFILTER", string.Format("tetypecode in (select  tetype2tetypemaster from wmstetype2tetype where tetype2tetypeslave in (select tetypecode_r from wmste where tecode ='{0}'))", teCode));
                    mgr.Parameters.Add(BpContext.BpContextArgumentName, context);
                    var manualEvent = new ManualResetEvent(false);
                    TE box = null;
                    mgr.Run("CREATEBOX", ctx =>
                    DispatcherHelper.BeginInvoke(new Action(() =>
                    {
                        manualEvent.Set();
                        box = context.Get<TE>("BOX");
                    })));

                    // ожидаем когда выполнится
                    manualEvent.WaitOne();

                    if (box == null || !string.Equals(box.GetKey(), teBaseCode))
                        return false;
                 
                    box.TEPackStatus = TEPackStatus.TE_PKG_ACTIVATED;
                    mgr.UpdateEntity(box); 
                    return true;
                }
            }

            TE getTE;

            using (var teMgr = GetManager<TE>(uow))
            {
                getTE = teMgr.Get(teBaseCode);
            }

            if (getTE == null)
            {
                ShowWarning(string.Format("Нет прав на ТЕ с кодом {0}", teBaseCode), "Ошибка вложения");
                return false;
            }

            if (getTE.TEPackStatus == TEPackStatus.TE_PKG_CREATED)
            {
                getTE.TEPackStatus = TEPackStatus.TE_PKG_ACTIVATED;
                using (var mgr = GetSpecManager<IBPProcessManager>(uow))
                {
                    mgr.UpdateEntity(getTE);
                }
            }

            if (!string.Equals(placeCode, getTE.CurrentPlace))
            {
                ShowWarning("Грузовое место не на текущем месте упаковки", "Ошибка вложения");
                return false;
            }

            if (getTE.TEPackStatus != TEPackStatus.TE_PKG_ACTIVATED)
            {
                ShowWarning("Грузовое место имеет статус отличный от 'Упаковка наполняется' ", "Ошибка вложения");
                return false;
            }

            using (var mgr = GetManager<TransportTask>(uow))
            {
                var filter = string.Format("TECODE_R = '{0}' and STATUSCODE_R = 'TTASK_CREATED'", teBaseCode);
                var tTasks = mgr.GetFiltered(filter, FilterHelper.GetAttrEntity<TransportTask>(new[] { TransportTask.TtaskIDPropertyName })).ToList();

                if (!tTasks.Any()) 
                    return true;

                ShowWarning(string.Format("Для грузового места '{0}' уже есть ЗНТ", teBaseCode), "Ошибка вложения");
                return false;
            }
        }

        public void Move(TE[] availableTEList, string defaultDestTECode, Product[] movingProducts, string currentPlaceCode)
        {
            var oneProductMode = movingProducts.Length == 1;
            var availableTECodes = availableTEList.Where(i => i.TEPackStatus == TEPackStatus.TE_PKG_ACTIVATED || i.TEPackStatus == TEPackStatus.TE_PKG_CREATED).Select(i => i.TECode);
            var filter = string.Format("TECODE in ('{0}')", string.Join("','", availableTECodes));
            if (filter.Length > 4000)
                filter = string.Format("TECURRENTPLACE = '{0}' AND TEPACKSTATUS in ('TE_PKG_CREATED','TE_PKG_ACTIVATED') AND TETYPECODE_R IN (select tt.tetypecode from wmstetype tt inner join wmscustomparamvalue cpv on cpv.cpvkey = tt.tetypecode and CPV2ENTITY = 'TETYPE' AND CUSTOMPARAMCODE_R = 'TETypeIsPackingL2' AND CPVVALUE = '1')", currentPlaceCode);

            IUnitOfWork uow = null;

            var vBoxCodeField = new ValueDataField()
            {
                Caption = "Короб",
                Name = "PACKCODE",
                FieldType = typeof(string),
                LookupCode = "TE_TECODE",
                LookupFilterExt = filter,
                IsEnabled = true,
                LabelPosition = "Left",
                IsRequired = true,
                Order = 0 
            };

            var vCountField = new ValueDataField()
            {
                Caption = "Кол-во",
                Name = "COUNT",
                FieldType = typeof(decimal),
                IsEnabled = oneProductMode,
                LabelPosition = "Left",
                IsRequired = true,
                Order = 2
            };

            vBoxCodeField.Value = defaultDestTECode;
            vCountField.Value = movingProducts.Sum(i => i.ProductCountSKU);

            // создадим модель
            var model = new ExpandoObjectViewModelBase
            {
                Fields = new ObservableCollection<ValueDataField>(new[] { vBoxCodeField, vCountField }),
                PanelCaption = string.Format("Перемещение")
            };

            if (FontSize.HasValue && FontSize.Value > 0)
                foreach (var f in model.Fields)
                    f.Set(ValueDataFieldConstants.FontSize, FontSize);

            model.SetSuffix("A1D4B481-FA75-4C7D-AE90-8337F556DDED");

            // отображаем форму
            while (true) // делаем это для возврата на форму
            {
                if (GetViewService().ShowDialogWindow(model, true, false, "30%") != true)
                    return;

                var productCountSKU = movingProducts.Sum(i => i.ProductCountSKU);

                // проверим указанное количество (не должно превышать кол-ва товара по SKU)
                var countValue = model.Get<decimal?>(vCountField.Name);
                if (countValue == null)
                    throw new OperationException("Не удалось прочитать параметр " + vCountField.Caption);

                var decCountValue = Convert.ToDecimal(countValue);

                if (decCountValue > productCountSKU)
                {
                    var message = string.Format("'{0}' не может быть больше кол-ва суммы товара = '{1}'\r\n", vCountField.Caption, productCountSKU);
                    ShowWarning(message, "Ошибка при переносе товара в короб");
                    continue;
                }

                var enteredDestTECode = model.Get<string>(vBoxCodeField.Name);
                if (string.IsNullOrEmpty(enteredDestTECode))
                {
                    ShowWarning("Не указан номер короба для переноса", "Ошибка при переносе товара в короб");
                    continue;
                }

                // проверяем привзяку SKU2TTE
                if (!CheckSKU2TTE(uow, enteredDestTECode, movingProducts.Select(i => i.SKUID).Distinct().ToArray(), decCountValue))
                    return;

                // упаковываем
                using (var mgr = GetSpecManager<IBPProcessManager>(uow))
                {
                    // переносим в другой короб
                    var productIdList = movingProducts.Select(i => i.ProductId.Value);

                    if (!string.IsNullOrEmpty(enteredDestTECode))
                        defaultDestTECode = enteredDestTECode;
                    mgr.PackProductLst(productIdList, null, defaultDestTECode, decCountValue, false);
                }

                break;
            }
        }

        private static bool CheckProductToEqualOWB(string teCode, ICollection<Product> packList, IUnitOfWork uow)
        {
            var productOWB = string.Empty;
            if (packList != null)
            {
                //Проверяем наличие не зарезервированного товара
                var unreserveds = packList.Where(p => !p.VOWBId.HasValue)
                    .Select(p => p.GetKey<decimal>()).ToArray();
                if (unreserveds.Length > 0)
                    throw new OperationException("Товар с ID: {0} не зарезервирован.", string.Join(", ", unreserveds.Distinct()));

                var keys = packList.Select(pack => pack.VOWBId).Distinct().ToArray();
                if (keys.Length != 0)
                    productOWB = string.Format("select owbid from wmsowb where OWBID in ({0}) union all ", string.Join(", ", keys));
            }

            // ищем все накладные товар которых уже лежит на ТЕ и те, к которым относится упаковываемый товар.
            OWB[] res;
            string[] owbGroups;
            using (var mgr = GetManager<OWB>(uow))
            {
                var filter = string.Format("owbid in ({0}select owbid from wmsowb where OWBID in (select distinct wmsowbpos.owbid_r from wmsproduct p inner join wmsowbpos on wmsowbpos.owbposid = p.owbposid_r where p.tecode_r = '{1}'))", productOWB, teCode);
                res = mgr.GetFiltered(filter, FilterHelper.GetAttrEntity<OWB>(OWB.OWBIDPropertyName, OWB.OWBGROUPPropertyName)).ToArray();
                owbGroups = res.Select(x => x.Group).Distinct().ToArray();
            }

            // если найдено не более 1 накладной или не более одной группы накладных, то все отлично
            if (res.Length <= 1 || (owbGroups.Length == 1 && !string.IsNullOrEmpty(owbGroups[0])))
                return true;

            // иначе - ошибка
            var groupErr = owbGroups.Length > 1 && owbGroups.Any(x => !string.IsNullOrEmpty(x))
                ? string.Format(" (группы накладных '{0}')", string.Join("', '", owbGroups))
                : "";
            var message = string.Format("В коробе '{0}' находится/окажется товар разных накладных: {1}{2}", teCode, string.Join(", ", res.Select(i => i.OWBID)), groupErr);
            GetViewService().ShowDialog("Ошибка упаковки товара", message, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
            return false;
        }
    }

    public class UserInterrrupteException : BaseException
    {
    }
}