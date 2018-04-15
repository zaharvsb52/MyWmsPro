using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.ServiceModel;
using Oracle.DataAccess.Client;
using wmsMLC.APS.wmsSI.Helpers;
using wmsMLC.APS.wmsSI.Wrappers;
using wmsMLC.Business.Managers.Imports;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;
using wmsMLC.General.DAL;

namespace wmsMLC.APS.wmsSI
{
    public partial class IntegrationService
    {
        private Measure[] _packageMeasures;
        private string[] _missingMeasures;
        private Factory[] _packageFactories;
        private string[] _missingFactories;
        private IsoCountry[] _packageCountries;

        public ErrorWrapper[] ArtPackageLoad(ArtWrapper[] artPackage)
        {
            Contract.Requires(artPackage != null);

            var processId = Guid.NewGuid();

            var startAllTime = DateTime.Now;
            Log.InfoFormat("Start of ArtPackageLoad ({0})", processId);
            Log.DebugFormat("({0}) -> {1}", processId, artPackage.DumpToXML());
            var retMessage = new List<ErrorWrapper>();

            try
            {
                // дополняем данным Id мандантов
                FillMandantIds(artPackage);

                // создаем группы артикулов
                FillArtGroups(artPackage);

                // проверяем наличие заголовков транзитов
                FillArtTransits(artPackage);

                // найдём и запомним все Ед.Изм.
                FindMeasures(artPackage);

                // найдём и запомним все фабрики
                FindFactories(artPackage);

                // найдём и запомним все страны
                FindCountries(artPackage);

                // запускаем параллельную обработку артикулов
                //var processorCount = Environment.ProcessorCount;
                //var parallelOptions = new ParallelOptions
                //{
                //    MaxDegreeOfParallelism = (processorCount <= 1 ? 8 : processorCount)
                //};
                //Parallel.ForEach(artPackage, parallelOptions, item =>
                foreach (var item in artPackage)
                {
                    IUnitOfWork uow = null;

                    try
                    {
                        uow = UnitOfWorkHelper.GetUnit();

                        var startTime = DateTime.Now;
                        Log.DebugFormat("Загружаем артикул '{0}' манданта '{1}'", item.ARTNAME, item.MandantCode);

                        var beforeProcessingWfcode = item.BEFOREPROCESSINGWFCODE;
                        if (!string.IsNullOrEmpty(beforeProcessingWfcode) && item.ARTUPDATE == 1)
                        {
                            Log.DebugFormat("Запуск wf '{0}' перед загрузкой артикула '{1}' манданта '{2}'", beforeProcessingWfcode, item.ARTNAME, item.MandantCode);
                            try
                            {
                                var wfex = ExecuteWorkflowBeforeProcessing(beforeProcessingWfcode, item);
                                if (wfex != null)
                                    throw wfex;
                            }
                            catch (Exception ex)
                            {
                                var wfopex = new OperationException(string.Format("Ошибка при выполнении wf '{0}' перед загрузкой артикула '{1}' манданта '{2}'.",
                                        beforeProcessingWfcode, item.ARTNAME, item.MandantCode), ex);
                                 Log.Error(wfopex);
                                 MessageHelper.AddMessage(new[]
                                 {
                                     string.Format("{0}", wfopex), 
                                 }, retMessage);
                            }
                        }

                        uow.BeginChanges();
                        var errmessages = ArtLoadInternal(item, uow);
                        MessageHelper.AddMessage(errmessages, retMessage);

                        errmessages = ArtLoadHelper.LoadCpv(item, null, null, uow, Log);
                        if (errmessages.Length > 0)
                            MessageHelper.AddMessage(errmessages, retMessage);

                        Log.DebugFormat("Артикул '{0}' загружен за {1}", item.ARTNAME, DateTime.Now - startTime);
                        uow.CommitChanges();

                        if (item.BARCODEONLY == 1)
                        {
                            retMessage.Add(
                                new ErrorWrapper
                                {
                                    ERRORCODE = MessageHelper.SuccessCode.ToString(),
                                    ERRORMESSAGE = string.Format("Загружен ШК для артикула «{0}»", item.ARTNAME)
                                });
                        }
                        else
                        {
                            retMessage.Add(
                                new ErrorWrapper
                                {
                                    ERRORCODE = MessageHelper.SuccessCode.ToString(),
                                    ERRORMESSAGE = string.Format("Загружен артикул «{0}»", item.ARTNAME)
                                });
                        }
                    }
                    catch (IntegrationLogicalException iex)
                    {
                        if (uow != null)
                            uow.RollbackChanges();

                        var message = ExceptionHelper.ExceptionToString(iex);
                        Log.Error(message, iex);

                        var ew = new ErrorWrapper { ERRORCODE = MessageHelper.ErrorCode.ToString(), ERRORMESSAGE = message };
                        retMessage.Add(ew);
                    }
                    catch (Exception ex)
                    {
                        if (uow != null)
                            uow.RollbackChanges();

                        var message = ExceptionHelper.ExceptionToString(ex);
                        Log.Error(message, ex);

                        var ew = new ErrorWrapper { ERRORCODE = MessageHelper.ErrorCode.ToString(), ERRORMESSAGE = "Системная ошибка: " + message };
                        retMessage.Add(ew);
                    }
                    finally
                    {
                        if (uow != null)
                            uow.Dispose();
                    }
                }//);
            }
            catch (Exception ex)
            {
                var message = ExceptionHelper.ExceptionToString(ex);
                Log.Error(message, ex);
                throw new FaultException<string>(message, new FaultReason(message));
            }
            finally
            {
                _packageMeasures = null;
                _missingMeasures = null;
                _packageFactories = null;
                _missingFactories = null;
                _packageCountries = null;

                Log.DebugFormat("Общее время загрузки {0} ({1})", DateTime.Now - startAllTime, processId);
                Log.InfoFormat("End of ArtPackageLoad ({0})", processId);
            }
            return retMessage.ToArray();
        }

        private void FillArtGroups(IEnumerable<ArtWrapper> artPackage)
        {
            // получем все уникальные группы
            //var artsInGroups = artPackage.Where(i => i.GROUP2ARTL != null && i.GROUP2ARTL.Count > 0).ToArray();
            //var artGroupCodes = new List<string>();
            //var artGroupNames = new List<string>();
            //foreach (var a2G in artsInGroups.SelectMany(artWithGroup => artWithGroup.GROUP2ARTL.Where
            //    (a2G => !artGroupCodes.Contains(a2G.ART2GROUPARTGROUPCODE))))
            //{
            //    artGroupCodes.Add(a2G.ART2GROUPARTGROUPCODE);
            //    artGroupNames.Add(a2G.ART2GROUPARTGROUPNAME);
            //}

            var art2Groups =
                artPackage.Where(p => p.GROUP2ARTL != null && p.GROUP2ARTL.Count > 0)
                    .SelectMany(p => p.GROUP2ARTL).Distinct(new Art2GroupWrapperEqualityComparer())
                    .ToArray();
            if (art2Groups.Length == 0)
                return;

            // формируем условие поиска групп
            //var groupValues = "'" + string.Join("','", art2Groups.Select(p => p.ART2GROUPARTGROUPCODE)) + "'";
            //var a2GFilter2 = string.Format("{0} in ({1})",
            //    SourceNameHelper.Instance.GetPropertySourceName(typeof(ArtGroup), ArtGroup.ArtGroupCodePropertyName),
            //    groupValues);

            var a2GFilters = FilterHelper.GetArrayFilterIn(
                SourceNameHelper.Instance.GetPropertySourceName(typeof (ArtGroup), ArtGroup.ArtGroupCodePropertyName),
                art2Groups.Select(p => p.ART2GROUPARTGROUPCODE));

            var wmsArtGroups = new List<ArtGroup>();
            using (var mgrAg = IoC.Instance.Resolve<IBaseManager<ArtGroup>>())
            {
                foreach (var a2GFilter in a2GFilters)
                {
                    var mgrAGs = mgrAg.GetFiltered(a2GFilter, GetModeEnum.Partial).ToArray();
                    if(mgrAGs.Length > 0)
                        wmsArtGroups.AddRange(mgrAGs);
                }

                foreach (var a2G in art2Groups)
                {
                    if (wmsArtGroups.Any(p => p.ArtGroupCode == a2G.ART2GROUPARTGROUPCODE))
                    {
                        Log.DebugFormat("Существует группа «{0}», код «{1}»", a2G.ART2GROUPARTGROUPNAME, a2G.ART2GROUPARTGROUPCODE);
                        continue;
                    }

                    var groupArt = new ArtGroup
                    {
                        ArtGroupName = a2G.ART2GROUPARTGROUPNAME,
                        ArtGroupCode = a2G.ART2GROUPARTGROUPCODE,
                        ARTGROUPHOSTREF = a2G.ART2GROUPHREF,
                        MANDANTID = a2G.MANDANTID
                    };

                    try
                    {
                        mgrAg.Insert(ref groupArt);
                        Log.InfoFormat("Добавлена группа артикулов «{0}», код «{1}»", groupArt.ArtGroupName,
                            groupArt.ArtGroupCode);
                    }
                    catch (Exception ex)
                    {
                        var opex = new OperationException(string.Format("Ошибка при добавлении ArtGroup (код '{0}').", groupArt.ArtGroupCode), ex);
                        Log.Error(opex);

                        var oraex = ex as OracleException ?? ex.InnerException as OracleException;
                        if (oraex == null || oraex.Number != 1)
                            throw opex;
                    }
                }
            }
        }

        private void FillMandantIds(ArtWrapper[] artPackage)
        {
            var mandantCodes = artPackage.Where(i => !i.MANDANTID.HasValue).Select(i => i.MandantCode).Distinct();
            foreach (var mandantCode in mandantCodes)
            {
                var mandantId = CheckMandant(null, mandantCode);
                if (mandantId == null)
                    continue;

                var code = mandantCode;
                var items = artPackage.Where(i => !i.MANDANTID.HasValue && i.MandantCode == code);
                foreach (var item in items)
                    item.MANDANTID = mandantId;
            }
        }

        private void FillPartnerColor(ArtWrapper artPackage, IUnitOfWork uow)
        {
            if(artPackage == null || string.IsNullOrEmpty(artPackage.ARTCOLOR))
                return;

            var typePartnerColor = typeof(PartnerColor);
            var filter = string.Format("{0} = {1} and upper({2}) = '{3}'",
                SourceNameHelper.Instance.GetPropertySourceName(typePartnerColor, PartnerColor.MANDANTIDPropertyName), 
                artPackage.MANDANTID,
                SourceNameHelper.Instance.GetPropertySourceName(typePartnerColor, PartnerColor.PARTNERCOLORNAMEPropertyName),
                artPackage.ARTCOLOR.ToUpper());

            using (var mgr = IoC.Instance.Resolve<IBaseManager<PartnerColor>>())
            {
                if (uow != null)
                    mgr.SetUnitOfWork(uow);

                var colors = mgr.GetFiltered(filter, GetModeEnum.Partial).ToArray();
                if (colors.Length == 0)
                {
                    var colorcode = string.Format("SI_NEW_COLOR_CODE_{0}", Guid.NewGuid());
                    var partnerColor = new PartnerColor
                    {
                        MANDANTID = artPackage.MANDANTID,
                        PARTNERCOLORCODE = colorcode,
                        PARTNERCOLORNAME = artPackage.ARTCOLOR
                    };
                    mgr.Insert(ref partnerColor);

                    //Меняем код цвета
                    var key = partnerColor.GetKey();
                    var color = mgr.Get(key, GetModeEnum.Partial);
                    color.PARTNERCOLORCODE = string.Format("Code_{0}", key);
                    mgr.Update(color);
                    artPackage.ARTCOLOR = color.PARTNERCOLORCODE;
                }
                else
                {
                    artPackage.ARTCOLOR = colors[0].PARTNERCOLORCODE;
                }
            }
        }

        private string[] ArtLoadInternal(ArtWrapper item, IUnitOfWork uow)
        {
            var messages = new List<string>();

            // проверяем, что у нас есть все ЕИ, которые потребуются 
            FillMeasure(item, _packageMeasures, _missingMeasures, uow);

            var isbarcodeonly = item.BARCODEONLY == 1;
            if (isbarcodeonly)
                Log.InfoFormat("Режим загрузки ШК для артикула '{0}'", item.ARTNAME);

            if (!isbarcodeonly && !string.IsNullOrEmpty(item.ARTFACTORY))
                FillFactory(item, uow);

            if (!isbarcodeonly)
                FillPartnerColor(item, uow);

            if (!isbarcodeonly && !string.IsNullOrEmpty(item.ARTMANUFACTURERCODE))
            {
                var errmessages = ArtLoadHelper.FillArtManufacturer(item, uow, Log, PartnerNameFieldMaxLength);
                if (errmessages.Length > 0)
                    messages.AddRange(errmessages);
            }

            if (!isbarcodeonly && !string.IsNullOrEmpty(item.COUNTRYCODE_R))
                FillCountry(item, uow);

            var art = new Art();
            var isNewArt = false;

            var arts = ArtLoadHelper.FindArtByArtName(item.MANDANTID, item.ARTNAME, GetModeEnum.Full, uow);

            if (arts.Length > 1)
            {
                throw new IntegrationLogicalException("Найдено более одного артикула с наименованием '{0}'",
                    item.ARTNAME);
            }

            using (var artMgr = IoC.Instance.Resolve<IBaseManager<Art>>())
            {
                if (uow != null)
                    artMgr.SetUnitOfWork(uow);

                if (arts.Length == 0) // добавляем
                {
                    if (isbarcodeonly)
                        throw new IntegrationLogicalException("Режим загрузки штрихкодов. Не найден артикул '{0}'", item.ARTNAME);

                    art = MapTo(item, art);
                    var errmessages = ArtLoadHelper.FillArtAbcd(art, item.ARTABCD, item.ARTNAME, Log);
                    if (errmessages.Length > 0)
                        messages.AddRange(errmessages);
                    SetXmlIgnore(art, false);
                    artMgr.Insert(ref art);
                    item.ARTCODE = art.ArtCode;
                    Log.InfoFormat("Добавлен артикул '{0}'", item.ARTNAME);
                    isNewArt = true;
                }
                else
                {
                    art = arts[0];
                    Log.InfoFormat("Код артикула '{0}'", art.ArtCode);
                    item.ARTCODE = art.ArtCode;

                    if (!isbarcodeonly)
                    {
                        if (item.ARTUPDATE != 1) // 1 - разрешен UPDATE артикула
                        {
                            Log.DebugFormat("Обновление артикула '{0}' не разрешено", item.ARTNAME);
                        }
                        else
                        {
                            //art.ARTABCD = item.ARTABCD;
                            var errmessages = ArtLoadHelper.FillArtAbcd(art, item.ARTABCD, item.ARTNAME, Log);
                            if (errmessages.Length > 0)
                                messages.AddRange(errmessages);

                            art.MANDANTID = item.MANDANTID;
                            art.ArtInputDateMethod = item.ARTINPUTDATEMETHOD;
                            art.ArtDesc = item.ARTDESC;

                            if (art.ArtPickOrder == 0)
                                art.ArtPickOrder = item.ARTPICKORDER;

                            if (art.ArtCommercTime == 0)
                            {
                                art.ArtCommercTime = item.ARTCOMMERCTIME;
                                art.ArtCommercTimeMeasure = item.ARTCOMMERCTIMEMEASURE;
                            }

                            if (art.ArtLifeTime == 0 || item.ARTLIFETIMEREFRESH == 1)
                            {
                                art.ArtLifeTime = item.ARTLIFETIME;
                                art.ArtLifeTimeMeasure = item.ARTLIFETIMEMEASURE;
                            }

                            if (item.ARTTEMPMAX.HasValue)
                                if (!art.ArtTempMax.HasValue)
                                    art.ArtTempMax = item.ARTTEMPMAX;
                                else if (art.ArtTempMax != item.ARTTEMPMAX)
                                    throw new IntegrationLogicalException(
                                        "Для артикула '{0}' существуют другие условия хранения: ArtTempMax = {1}",
                                        item.ARTNAME, art.ArtTempMax);

                            if (item.ARTTEMPMIN.HasValue)
                                if (!art.ArtTempMin.HasValue)
                                    art.ArtTempMin = item.ARTTEMPMIN;
                                else if (art.ArtTempMin != item.ARTTEMPMIN)
                                    throw new IntegrationLogicalException(
                                        "Для артикула '{0}' существуют другие условия хранения: ArtTempMin = {1}",
                                        item.ARTNAME, art.ArtTempMin);

                            if (!string.IsNullOrEmpty(item.ARTBRAND))
                                art.SetProperty(Art.ARTBRANDPropertyName, item.ARTBRAND);

                            if (!string.IsNullOrEmpty(item.ARTMARK))
                                art.SetProperty(Art.ARTMARKPropertyName, item.ARTMARK);

                            if (!string.IsNullOrEmpty(item.ARTMODEL))
                                art.SetProperty(Art.ARTMODELPropertyName, item.ARTMODEL);

                            if (!string.IsNullOrEmpty(item.ARTTYPE))
                                art.SetProperty(Art.ARTTYPEPropertyName, item.ARTTYPE);

                            if (!string.IsNullOrEmpty(item.ARTIWBTYPE))
                                art.SetProperty(Art.ARTIWBTYPEPropertyName, item.ARTIWBTYPE);

                            if (!string.IsNullOrEmpty(item.ARTDANGERLEVEL))
                                art.SetProperty(Art.ARTDANGERLEVELPropertyName, item.ARTDANGERLEVEL);

                            if (!string.IsNullOrEmpty(item.ARTDANGERSUBLEVEL))
                                art.SetProperty(Art.ARTDANGERSUBLEVELPropertyName, item.ARTDANGERSUBLEVEL);

                            if (!string.IsNullOrEmpty(item.ARTDESCEXT))
                                art.SetProperty(Art.ARTDESCEXTPropertyName, item.ARTDESCEXT);

                            if (!string.IsNullOrEmpty(item.ARTCOLOR))
                                art.SetProperty(Art.ARTCOLORPropertyName, item.ARTCOLOR);

                            // проверим перед записью есть ли изменения
                            if (art.IsDirty)
                            {
                                SetXmlIgnore(art, true);
                                artMgr.Update(art);
                                Log.DebugFormat("Обновлен артикул '{0}'", item.ARTNAME);
                            }
                            else
                            {
                                Log.DebugFormat("Обновление артикула '{0}' не требуется", item.ARTNAME);
                            }
                        }
                    }
                }
            }

            // обрабатываем SKU артикула
            if (!isbarcodeonly)
            {
                if (!ProcessArtSku(item, art, isNewArt, uow))
                    Log.DebugFormat("Для артикула '{0}' не требуется загрузка SKU", item.ARTNAME);
            }

            // обрабатываем привязки артикула к группам
            if (!isbarcodeonly)
            {
                if (!isNewArt && item.NOTNEEEDEDUPDATEARTGROUP == 1)
                {
                    Log.DebugFormat(
                        "Для артикула '{0}' не требуется привязка к группам (артикул существуеи и задан параметр NOTNEEEDEDUPDATEARTGROUP '{1}').",
                        item.ARTNAME, item.NOTNEEEDEDUPDATEARTGROUP);
                }
                else
                {
                    if (!ProcessArtGroup(item, uow))
                        Log.DebugFormat("Для артикула '{0}' не требуется привязка к группам", item.ARTNAME);
                }
            }

            // обрабатываем транзитные данные артикула
            if (!isbarcodeonly)
            {
                if (!LoadArtTransit(item, uow))
                    Log.DebugFormat("Для артикула '{0}' не требуется загрузка транзитных данных", item.ARTNAME);
            }

            //Загружаем штрихкоды
            if (isbarcodeonly)
            {
                if (item.SKUL == null)
                {
                    var message = string.Format("Режим загрузки ШК для артикула '{0}'. Не определена коллекция SKUL.",
                        item.ARTNAME);
                    messages.Add(message);
                    Log.Error(message);
                    return messages.ToArray();
                }

                if (art == null)
                {
                    var message = string.Format("Режим загрузки ШК для артикула '{0}'. Не найден артикул.",
                        item.ARTNAME);
                    messages.Add(message);
                    Log.Error(message);
                    return messages.ToArray();
                }

                if (art.SKUL == null)
                {
                    var message = string.Format("Режим загрузки ШК для артикула '{0}'. У найденного артикула не определена коллекция SKUL .",
                        item.ARTNAME);
                    messages.Add(message);
                    Log.Error(message);
                    return messages.ToArray();
                }

                foreach (var skuWrapper in item.SKUL)
                {
                    var existsSku = new SKU[] {};
                    if (art.SKUL != null && art.SKUL.Count > 0)
                        existsSku = art.SKUL.Where(i => string.Equals(i.MeasureCode, skuWrapper.MEASURECODE_R)).ToArray();
                    if (existsSku.Length == 0)
                        throw new OperationException("Режим загрузки ШК. Для артикула '{0}' не найдена ЕУ (SKU) по ЕИ '{1}'.", item.ARTNAME, skuWrapper.MEASURECODE_R);
                    foreach (var esku in existsSku)
                    {
                        try
                        {
                            skuWrapper.SKUID = esku.GetKey<decimal>();
                            LoadBarcodes(uow, skuWrapper, false, true);
                        }
                        catch (Exception ex)
                        {
                            var opex = new OperationException(string.Format("Режим загрузки ШК для артикула '{0}', skuid = '{1}'.", item.ARTNAME, skuWrapper.SKUID), ex);
                            messages.Add(string.Format("{0}", opex));
                            Log.Error(opex);
                        }
                    }
                }
            }

            return messages.ToArray();
        }

        private static void FillMeasure(ArtWrapper item, Measure[] packageMeasures, string[] missingMeasures, IUnitOfWork uow)
        {
            if (item.SKUL == null || item.SKUL.Count == 0)
                return;

            if (packageMeasures == null)
                throw new ArgumentNullException("packageMeasures");
            if (missingMeasures == null)
                throw new ArgumentNullException("missingMeasures");

            // если среди SKU артикула есть те, Ед.Изм. которых мы не смогли найти - весь артикул пропускается
            if (missingMeasures.Length > 0 && item.SKUL.Any(sku => missingMeasures.Contains(sku.MEASURECODE_R)))
                throw new IntegrationLogicalException("Не найдены единицы измерения «{0}»",
                    string.Join("», «", item.SKUL.Where(sku => missingMeasures.Contains(sku.MEASURECODE_R)).Select(sku => sku.MEASURECODE_R)));

            foreach (var skuWrapper in item.SKUL)
            {
                Measure neededMeasure;
                // если все Ед.Изм. удалось найти пред кешированием = ищем необходимую для SKU среди найденных
                if (packageMeasures.Length > 0)
                    neededMeasure = packageMeasures.FirstOrDefault(i => skuWrapper.MEASURECODE_R.EqIgnoreCase(i.MeasureCode) ||
                                                                skuWrapper.MEASURECODE_R.EqIgnoreCase(i.MeasureShortName));
                // иначе ищем каждую в БД отдельно. избыточно, зато отказоустойчиво.
                else
                {
                    var measuresFilter = string.Format("upper({0}) = '{1}' or upper({2}) = '{1}'",
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(Measure), Measure.MeasureCodePropertyName), skuWrapper.MEASURECODE_R.ToUpper(),
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(Measure), Measure.MeasureShortNamePropertyName));

                    using (var measureMgr = IoC.Instance.Resolve<IBaseManager<Measure>>())
                    {
                        if (uow != null)
                            measureMgr.SetUnitOfWork(uow);
                        var measures = measureMgr.GetFiltered(measuresFilter, GetModeEnum.Partial).ToArray();
                        neededMeasure = measures.FirstOrDefault();
                    }
                }
                if (neededMeasure == null)
                    throw new IntegrationLogicalException("Не найдена единица измерения «{0}»", skuWrapper.MEASURECODE_R);

                skuWrapper.MEASURECODE_R = neededMeasure.MeasureCode;
            }
        }

        private void FillFactory(ArtWrapper item, IUnitOfWork uow)
        {
            // если фабрика есть среди тех, которые мы не смогли найти - весь артикул пропускается
            if (_missingFactories.Length > 0 && _missingMeasures.Contains(item.ARTFACTORY))
                throw new IntegrationLogicalException("Не найдена фабрика с кодом «{0}» манданта ID={1}", item.ARTFACTORY, item.MANDANTID);

            Factory neededFactory;
            // если все фабрики удалось найти пред кешированием = ищем необходимую среди найденных
            if (_packageFactories.Length > 0)
                neededFactory = _packageFactories.FirstOrDefault(i => i.FactoryCode.EqIgnoreCase(item.ARTFACTORY));
            // иначе ищем каждую в БД отдельно. избыточно, зато отказоустойчиво.
            else
            {
                var factoryFilter = string.Format("upper({0}) = upper('{1}') and {2} = {3}",
                    SourceNameHelper.Instance.GetPropertySourceName(typeof(Factory), Factory.FactoryCodePropertyName), item.ARTFACTORY,
                    SourceNameHelper.Instance.GetPropertySourceName(typeof(Factory), Factory.PARTNERID_RPropertyName), item.MANDANTID);

                using (var factoryMgr = IoC.Instance.Resolve<IBaseManager<Factory>>())
                {
                    if (uow != null)
                        factoryMgr.SetUnitOfWork(uow);
                    var factories = factoryMgr.GetFiltered(factoryFilter).ToArray();

                    if (factories.Length == 0)
                        throw new IntegrationLogicalException("Не найдена фабрика с кодом «{0}» манданта ID={1}", item.ARTFACTORY, item.MANDANTID);

                    if (factories.Length > 1)
                        throw new IntegrationLogicalException("Существует несколько значений для кода фабрики манданта ID={1}", item.ARTFACTORY, item.MANDANTID);

                    neededFactory = factories[0];
                }
            }

            if (neededFactory == null)
                throw new IntegrationLogicalException("Не найдена фабрика с кодом «{0}» манданта ID={1}", item.ARTFACTORY, item.MANDANTID);

            item.FACTORYID_R = neededFactory.FactoryID;
            Log.DebugFormat("Фабрика = {0}", item.FACTORYID_R);
        }

        private void FillCountry(ArtWrapper item, IUnitOfWork uow)
        {
            IsoCountry neededCountry = null;
            // если все страны удалось найти пред кешированием = ищем необходимую среди найденных
            if (_packageCountries.Length > 0)
            {
                neededCountry = _packageCountries.FirstOrDefault(i =>
                    i.CountryCode.EqIgnoreCase(item.COUNTRYCODE_R) ||
                    i.CountryAlpha2.EqIgnoreCase(item.COUNTRYCODE_R) ||
                    i.COUNTRYNAMERUS.EqIgnoreCase(item.COUNTRYCODE_R));
            }
            if (neededCountry == null)
                throw new IntegrationLogicalException(
                    "Не найдена страна происхождения (COUNTRYCODE_R) '{0}' для артикула '{1}'.", item.COUNTRYCODE_R,
                    item.ARTNAME);

            item.COUNTRYCODE_R = neededCountry.CountryCode;
        }

        private bool ProcessArtGroup(ArtWrapper artWrapper, IUnitOfWork uow)
        {
            if (artWrapper.GROUP2ARTL == null || artWrapper.GROUP2ARTL.Count == 0)
                return false;

            //var groupPriority = artWrapper.GROUP2ARTL[0].ART2GROUPPRIORITY;

            using (var mgrArt2Group = IoC.Instance.Resolve<IBaseManager<Art2Group>>())
            {
                if (uow != null)
                    mgrArt2Group.SetUnitOfWork(uow);

                var art2GroupCodes = artWrapper.GROUP2ARTL.Select(i => i.ART2GROUPARTGROUPCODE).Distinct();
                var art2GroupPriority = artWrapper.GROUP2ARTL.Select(i => i.ART2GROUPPRIORITY).Distinct();
                var art2GroupFilterValue = "'" + string.Join("','", art2GroupCodes) + "'";
                var groupPriority = string.Join(",", art2GroupPriority);

                var art2GroupFilter = string.Format("{0} = '{1}' and ({2} in ({3}) or {4} in ({5}))",
                    SourceNameHelper.Instance.GetPropertySourceName(typeof(Art2Group), Art2Group.Art2GroupArtCodePropertyName), artWrapper.ARTCODE,
                    SourceNameHelper.Instance.GetPropertySourceName(typeof(Art2Group), Art2Group.Art2GroupArtGroupCodePropertyName), art2GroupFilterValue,
                    SourceNameHelper.Instance.GetPropertySourceName(typeof(Art2Group), Art2Group.Art2GroupPriorityPropertyName), groupPriority);
                Log.InfoFormat("art2GroupFilter = '{0}'", art2GroupFilter);
                var art2GroupItems = mgrArt2Group.GetFiltered(art2GroupFilter).ToArray();

                foreach (var art2GroupWrapper in artWrapper.GROUP2ARTL)
                {
                    var needDeleteGroup = art2GroupWrapper.ART2GROUPDELETE.HasValue &&
                                          art2GroupWrapper.ART2GROUPDELETE.Value == 1;

                    Art2Group art2Group = null;
                    if (art2GroupItems.Length > 0)
                        art2Group = art2GroupItems.FirstOrDefault(i => i.Art2GroupArtGroupCode == art2GroupWrapper.ART2GROUPARTGROUPCODE);
                    if (art2Group != null)
                    {
                        if (needDeleteGroup)
                        {
                            mgrArt2Group.Delete(art2Group);
                            Log.InfoFormat("Удалена привязка артикула «{0}» к группе «{1}»", artWrapper.ARTNAME, art2Group.Art2GroupArtGroupCode);
                        }
                        else
                        {
                            Log.InfoFormat("Привязка артикула «{0}» к группе «{1}» уже существует",
                                artWrapper.ARTNAME, art2Group.Art2GroupArtGroupCode);
                        }
                    }
                    else
                    {
                        var existPriority = art2GroupItems.FirstOrDefault(i => i.Art2GroupPriority == art2GroupWrapper.ART2GROUPPRIORITY);
                        if (existPriority == null)
                        {
                            art2Group = new Art2Group
                            {
                                Art2GroupArtCode = artWrapper.ARTCODE,
                                Art2GroupArtGroupCode = art2GroupWrapper.ART2GROUPARTGROUPCODE,
                                Art2GroupPriority = art2GroupWrapper.ART2GROUPPRIORITY
                            };
                            mgrArt2Group.Insert(ref art2Group);
                            Log.InfoFormat("Добавлена привязка артикула «{0}» к группе «{1}» (приоритет = {2})",
                                artWrapper.ARTNAME, art2GroupWrapper.ART2GROUPARTGROUPCODE, art2GroupWrapper.ART2GROUPPRIORITY);
                        }
                        else
                            if (art2GroupWrapper.ART2GROUPREBINDING == 1)
                            {
                                mgrArt2Group.Delete(existPriority);
                                Log.InfoFormat("Удалена привязка артикула «{0}» к группе «{1}» (rebinding)",
                                    artWrapper.ARTNAME, existPriority.Art2GroupArtGroupCode);

                                art2Group = new Art2Group
                                {
                                    Art2GroupArtCode = artWrapper.ARTCODE,
                                    Art2GroupArtGroupCode = art2GroupWrapper.ART2GROUPARTGROUPCODE,
                                    Art2GroupPriority = art2GroupWrapper.ART2GROUPPRIORITY
                                };
                                mgrArt2Group.Insert(ref art2Group);
                                Log.InfoFormat("Артикул «{0}» перепривязан к группе «{1}» (приоритет = {2})",
                                    artWrapper.ARTNAME, art2GroupWrapper.ART2GROUPARTGROUPCODE, art2GroupWrapper.ART2GROUPPRIORITY);
                            }
                            else
                                throw new IntegrationLogicalException("Существует привязка артикула «{0}» с приоритетом «{1}» (группа «{2}»)",
                                                    artWrapper.ARTNAME, art2GroupWrapper.ART2GROUPPRIORITY, existPriority.Art2GroupArtGroupCode);
                    }
                }
            }
            return true;
        }

        private bool ProcessArtSku(ArtWrapper item, Art art, bool isNewArt, IUnitOfWork uow)
        {
            if (item.SKUL == null || item.SKUL.Count == 0)
                return false;

            if (isNewArt)
                art.SKUL = new WMSBusinessCollection<SKU>();

            using (var skuMgr = IoC.Instance.Resolve<IBaseManager<SKU>>())
            {
                if (uow != null)
                    skuMgr.SetUnitOfWork(uow);

                Log.Debug("Ищем базовую SKU для артикула " + item.ARTCODE);

                var primaries = item.SKUL.Where(i => i.SKUPRIMARY).ToArray();
                if (primaries.Length == 0)
                    throw new IntegrationLogicalException("Не передана ни одна базовая SKU");
                if (primaries.Length > 1)
                    throw new IntegrationLogicalException("Передано более одной базовой SKU");
                var primarySku = primaries[0];

                var existsPrimary = new SKU[] {};
                if (art.SKUL != null && art.SKUL.Count > 0)
                {
                    existsPrimary = art.SKUL.Where(i => i.SKUPrimary).ToArray();
                    if (existsPrimary.Length > 1)
                        throw new IntegrationLogicalException("Существует более одной базовой SKU");
                }

                if (existsPrimary.Length == 1)
                {
                    primarySku.SKUID = existsPrimary[0].GetKey<decimal>();
                    Log.InfoFormat("Существует базовая SKU (ID = {0})", primarySku.SKUID);
                }
                else
                {
                    var sku = new SKU { ArtCode = item.ARTCODE };
                    sku = MapTo(primarySku, sku);
                    SetXmlIgnore(sku, false);
                    skuMgr.Insert(ref sku);
                    primarySku.SKUID = sku.GetKey<decimal>();
                    Log.InfoFormat("Загружена базовая SKU (ID = {0})", primarySku.SKUID);
                }

                // загружаем привязку этикетки (только для базовой SKU)
                if (!string.IsNullOrEmpty(item.ARTLABELCODE))
                    LoadArtLabels(uow, primarySku, isNewArt, item.ARTLABELCODE);
                else
                    Log.DebugFormat("Для SKU {0} не требуется загрузка LabelUse", primarySku.SKUNAME);

                foreach (var skuWrapper in item.SKUL)
                {
                    if (!skuWrapper.SKUPRIMARY)
                    {
                        skuWrapper.SKUPARENT = primarySku.SKUID;

                        if (!skuWrapper.SKUCOUNT.HasValue)
                            throw new IntegrationLogicalException("Для SKU '{0}' не указано кол-во", skuWrapper.SKUID);

                        var skuCount = Convert.ToDouble(skuWrapper.SKUCOUNT.Value);
                        var existsSku = new SKU[] {};
                        if (art.SKUL != null && art.SKUL.Count > 0)
                        {
                            existsSku = art.SKUL.Where(i => /*i.SKUClient &&*/
                                string.Equals(i.MeasureCode, skuWrapper.MEASURECODE_R) &&
                                i.SKUCount.Equals(skuCount)).ToArray();
                        }

                        if (existsSku.Length == 0)
                        {
                            var sku = new SKU { ArtCode = item.ARTCODE };
                            sku = MapTo(skuWrapper, sku);
                            SetXmlIgnore(sku, false);
                            skuMgr.Insert(ref sku);
                            skuWrapper.SKUID = sku.GetKey<decimal>();
                            Log.InfoFormat("Загружена SKU (ID = {0})", skuWrapper.SKUID);
                        }
                        else
                        {
                            Log.InfoFormat("Найдена SKU (ЕИ = {0}, Кол-во = {1})", skuWrapper.MEASURECODE_R, skuWrapper.SKUCOUNT);
                            skuWrapper.SKUID = existsSku[0].GetKey<decimal>();
                        }
                    }

                    // загружаем цены
                    if (!LoadArtPrices(uow, skuWrapper, isNewArt))
                        Log.DebugFormat("Для SKU {0} не требуется загрузка ArtPrice", skuWrapper.SKUNAME);

                    // загружаем ШК
                    if (!LoadBarcodes(uow, skuWrapper, isNewArt, false))
                        Log.DebugFormat("Для SKU {0} не требуется загрузка Barcode", skuWrapper.SKUNAME);

                    // загружаем TEType2SKU
                    if (!LoadSku2Tte(uow, skuWrapper, isNewArt))
                        Log.DebugFormat("Для SKU {0} не требуется загрузка SKU2TTE", skuWrapper.SKUNAME);
                }
            }
            return true;
        }

        private bool LoadArtPrices(IUnitOfWork uow, SKUWrapper skuWrapper, bool isNewArt)
        {
            if (skuWrapper.ArtPriceList == null || skuWrapper.ArtPriceList.Count == 0)
                return false;

            using (var mgrArtPrice = IoC.Instance.Resolve<IBaseManager<ArtPrice>>())
            {
                if (uow != null)
                    mgrArtPrice.SetUnitOfWork(uow);

                IEnumerable<ArtPrice> artPrices;
                // если артикул только создали - у него не может быть привязки к ценам
                if (isNewArt)
                    artPrices = new List<ArtPrice>();
                else
                {
                    var artPriceFilter = string.Format("{0} = {1}",
                            SourceNameHelper.Instance.GetPropertySourceName(typeof(ArtPrice),
                            ArtPrice.ArtPriceSKUIDPropertyName), skuWrapper.SKUID);

                    artPrices = mgrArtPrice.GetFiltered(artPriceFilter, GetModeEnum.Partial).ToArray();
                }

                foreach (var artPriceWrapper in skuWrapper.ArtPriceList)
                {
                    if (artPrices.Any(i => i.ArtPriceValue == artPriceWrapper.ARTPRICEVALUE))
                    {
                        Log.DebugFormat("Для SKU '{0}' существует цена со значением '{1}'", skuWrapper.SKUNAME,
                            artPriceWrapper.ARTPRICEVALUE);
                    }
                    else
                    {
                        artPriceWrapper.SKUID_R = skuWrapper.SKUID;
                        artPriceWrapper.MANDANTID = skuWrapper.MANDANTID;
                        var ap = new ArtPrice();
                        ap = MapTo(artPriceWrapper, ap);
                        SetXmlIgnore(ap, false);
                        mgrArtPrice.Insert(ref ap);
                        artPriceWrapper.ARTPRICEID = ap.ArtPriceID;
                        Log.DebugFormat("Для SKU '{0}' добавлена цена со значением '{1}'", skuWrapper.SKUNAME, artPriceWrapper.ARTPRICEVALUE);
                    }
                }
            }

            return true;
        }

        private bool LoadBarcodes(IUnitOfWork uow, SKUWrapper skuWrapper, bool isNewArt, bool isbarcodeonly)
        {
            if (skuWrapper.BarcodeList == null || skuWrapper.BarcodeList.Count == 0)
                return false;

            using (var mgrBarcode = IoC.Instance.Resolve<IBaseManager<Barcode>>())
            {
                if (uow != null)
                    mgrBarcode.SetUnitOfWork(uow);

                var values = "'" + string.Join("','", skuWrapper.BarcodeList.Select(i => i.BARCODEVALUE)) + "'";

                IEnumerable<Barcode> brcds;
                // если артикул только создали - у него не может быть привязки к ШК
                if (isNewArt)
                {
                    brcds = new List<Barcode>();
                }
                else
                {
                    // Получаем все ШК с указанными кодами для сущностей ART и SKU. Предполагаем, что кол-во ограничено
                    var brFilter = string.Format("{0} in ({1}) and {2} in ('ART', 'SKU')",
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(Barcode), Barcode.BarcodeValuePropertyName), values,
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(Barcode), Barcode.BarcodeEntityPropertyName));

                    brcds = mgrBarcode.GetFiltered(brFilter).ToArray();
                }

                foreach (var barcode in skuWrapper.BarcodeList)
                {
                    if (barcode.BARCODE2ENTITY == "ART")
                    {
                        // Согласно ТЗ (от Бытачевского) при обнаружении привязки к артикулу (по артикулу) отличному от нашего необходимо прерывать процесс и выводить сообщение об ошибке
                        var wrongBc = brcds.FirstOrDefault(i => i.BarcodeEntity == "ART" && i.BarcodeValue == barcode.BARCODEVALUE && i.BarcodeKey != skuWrapper.ARTCODE_R);
                        if (wrongBc != null)
                            throw new IntegrationLogicalException("Для штрих-кода '{0}' существует привязка к артикулу '{1}'. Ожидается единственная привязка к артикулу '{2}'",
                                barcode.BARCODEVALUE, wrongBc.BarcodeValue, skuWrapper.ARTCODE_R);

                        // ищем уже привязанный ШК
                        var existBc = brcds.FirstOrDefault(i => i.BarcodeEntity == "ART" && i.BarcodeValue == barcode.BARCODEVALUE && i.BarcodeKey == skuWrapper.ARTCODE_R);
                        if (existBc != null)
                        {
                            Log.DebugFormat("Для артикула '{0}' уже существует ШК с кодом '{1}'", skuWrapper.ARTCODE_R, barcode.BARCODEVALUE);
                        }
                        else
                        {
                            var bar = new Barcode { BarcodeKey = skuWrapper.ARTCODE_R };
                            bar = MapTo(barcode, bar);
                            SetXmlIgnore(bar, false);
                            mgrBarcode.Insert(ref bar);
                            barcode.BARCODEID = bar.BarcodeID;
                            Log.DebugFormat("Для артикула '{0}' добавлен ШК с кодом '{1}'", skuWrapper.ARTCODE_R, barcode.BARCODEVALUE);
                        }
                    }
                    else
                    {
                        try
                        {
                            var skuId = skuWrapper.SKUID.ToString();
                            var existBc = brcds.FirstOrDefault(i => i.BarcodeEntity == "SKU" && i.BarcodeValue == barcode.BARCODEVALUE && i.BarcodeKey == skuId);
                            if (existBc != null)
                            {
                                Log.DebugFormat("Для SKU '{0}' уже существует ШК с кодом '{1}'", skuId, barcode.BARCODEVALUE);
                            }
                            else
                            {
                                var bar = new Barcode { BarcodeKey = skuId };
                                bar = MapTo(barcode, bar);
                                SetXmlIgnore(bar, false);
                                mgrBarcode.Insert(ref bar);
                                barcode.BARCODEID = bar.BarcodeID;
                                Log.DebugFormat("Для SKU '{0}' добавлен ШК с кодом '{1}'", skuId, barcode.BARCODEVALUE);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (isbarcodeonly)
                            {
                                var opex = new OperationException(string.Format("Режим загрузки ШК для артикула, skuid = '{0}', ШК = '{1}'.", skuWrapper.SKUID, barcode.BARCODEVALUE), ex);
                                Log.Error(opex);
                                continue;    
                            }
                            throw;
                        } 
                    }
                }
            }
            return true;
        }

        private bool LoadSku2Tte(IUnitOfWork uow, SKUWrapper skuWrapper, bool isNewArt)
        {
            if (skuWrapper.TETYPE2SKU == null || skuWrapper.TETYPE2SKU.Count == 0)
                return false;

            using (var mgrS2T = IoC.Instance.Resolve<IBaseManager<SKU2TTE>>())
            {
                if (uow != null)
                    mgrS2T.SetUnitOfWork(uow);

                IEnumerable<SKU2TTE> sku2Ttes;
                // если артикул только создали - у него не может быть привязки к TTE
                if (isNewArt)
                {
                    sku2Ttes = new List<SKU2TTE>();
                }
                else
                {
                    var sku2TteFilter = string.Format("{0} = {1}",
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(SKU2TTE), SKU2TTE.SKU2TTESKUIDPropertyName),
                        skuWrapper.SKUID);
                    sku2Ttes = mgrS2T.GetFiltered(sku2TteFilter).ToArray();
                }

                foreach (var sku2TteItem in skuWrapper.TETYPE2SKU)
                {
                    var items = sku2Ttes.Where(i => i.TETypeCode == sku2TteItem.SKU2TTETETYPECODE).ToArray();
                    if (items.Length > 0)
                        continue;

                    //TODO: можно добавить проверку, на то, что мы пытаемся добавить привязку "по-умолчанию" в то время, как она уже есть

                    var sku2Tte = new SKU2TTE();
                    sku2TteItem.SKU2TTESKUID = skuWrapper.SKUID;
                    sku2Tte = MapTo(sku2TteItem, sku2Tte);
                    SetXmlIgnore(sku2Tte, false);
                    mgrS2T.Insert(ref sku2Tte);
                    sku2TteItem.SKU2TTEID = sku2Tte.SKU2TTEID;

                    Log.InfoFormat("Привязка SKU к ТЕ {0}(ID = {1})",
                        sku2TteItem.SKU2TTEDEFAULT ? "по умолчанию " : string.Empty,
                        sku2TteItem.SKU2TTEID);
                }
            }
            return true;
        }

        private void LoadArtLabels(IUnitOfWork uow, SKUWrapper skuWrapper, bool isNewArt, string labelCode)
        {
            using (var mgrArtLabel = IoC.Instance.Resolve<IBaseManager<Label>>())
            {
                if (uow != null)
                    mgrArtLabel.SetUnitOfWork(uow);

                var artLabelFilter = string.Format("{0} = '{1}'",
                            SourceNameHelper.Instance.GetPropertySourceName(typeof(Label),
                            Label.LabelCodePropertyName), labelCode);
                var artLabels = mgrArtLabel.GetFiltered(artLabelFilter).ToArray();

                if (artLabels.Length == 0)
                    throw new IntegrationLogicalException("Не найдена этикетка «{0}»", labelCode);

                using (var mgrArtLabelUse = IoC.Instance.Resolve<IBaseManager<LabelUse>>())
                {
                    mgrArtLabelUse.SetUnitOfWork(uow);

                    if (isNewArt)
                    {
                        var aluObj = new LabelUse
                        {
                            LabelCode_R = labelCode,
                            SKUID_R = skuWrapper.SKUID,
                            ArtCode_R = skuWrapper.ARTCODE_R
                        };
                        SetXmlIgnore(aluObj, false);
                        mgrArtLabelUse.Insert(ref aluObj);
                        Log.InfoFormat("Создана привязка базовой SKU артикула {0} к этикетке {1}", skuWrapper.ARTCODE_R, labelCode);
                    }
                    else
                    {
                        var artLabelUseFilter = string.Format("{0} = '{1}' and {2} = {3} and {4} = '{5}'",
                            SourceNameHelper.Instance.GetPropertySourceName(typeof(LabelUse),
                            LabelUse.LabelCode_RPropertyName), labelCode,
                            SourceNameHelper.Instance.GetPropertySourceName(typeof(LabelUse),
                            LabelUse.SKUID_RPropertyName), skuWrapper.SKUID,
                            SourceNameHelper.Instance.GetPropertySourceName(typeof(LabelUse),
                            LabelUse.ArtCode_RPropertyName), skuWrapper.ARTCODE_R);
                        var artLabelUses = mgrArtLabelUse.GetFiltered(artLabelUseFilter).ToArray();

                        if (artLabelUses.Length == 0)
                        {
                            var aluObj = new LabelUse
                            {
                                LabelCode_R = labelCode,
                                SKUID_R = skuWrapper.SKUID,
                                ArtCode_R = skuWrapper.ARTCODE_R
                            };
                            SetXmlIgnore(aluObj, false);
                            mgrArtLabelUse.Insert(ref aluObj);
                            Log.InfoFormat("Создана привязка базовой SKU артикула {0} к этикетке {1}", skuWrapper.ARTCODE_R, labelCode);
                        }
                        else
                            Log.InfoFormat("Существует привязка SKU артикула {0} к этикетке {1}", skuWrapper.ARTCODE_R, labelCode);
                    }
                }
            }
        }

        private void FillArtTransits(ArtWrapper[] artPackage)
        {
            // получем все уникальные заголовки транзитов
            //var artsInTransits = artPackage.Where(i => i.TRANSITDATAL != null).ToArray();
            //var artTransits = new List<string>();
            //foreach (var aTr in artsInTransits.SelectMany(artWithTransit => artWithTransit.TRANSITDATAL.Where
            //                                                (aTr => !artTransits.Contains(aTr.TRANSITNAME))).Distinct())
            //{
            //    artTransits.Add(aTr.TRANSITNAME);
            //}

            var artsInTransits = artPackage.Where(p => p.TRANSITDATAL != null && p.TRANSITDATAL.Count > 0).ToArray();
            if (artsInTransits.Length == 0)
                return;

            var transitData = artsInTransits
                .SelectMany(p => p.TRANSITDATAL).Distinct(new TransitDataWrapperEqualityComparer())
                .ToArray();
            if (transitData.Length == 0)
                return;

            // формируем условие поиска заголовков транзитов
            //var transitValues = "'" + string.Join("','", artTransits) + "'";
            //var aTrFilter = string.Format("{0} in ({1}) and {2} = {3} and {4} = 'ART'",
            //    SourceNameHelper.Instance.GetPropertySourceName(typeof(Transit), Transit.TransitNamePropertyName),
            //    transitValues,
            //    Transit.MANDANTIDPropertyName, artPackage[0].MANDANTID,
            //    SourceNameHelper.Instance.GetPropertySourceName(typeof(Transit), Transit.TransitEntityPropertyName));

            var aTrFilterEx = string.Format(" and {0} = {1} and {2} = 'ART'",
                Transit.MANDANTIDPropertyName, artPackage[0].MANDANTID,
                SourceNameHelper.Instance.GetPropertySourceName(typeof (Transit), Transit.TransitEntityPropertyName));

            var aTrFilters =
                FilterHelper.GetArrayFilterIn(
                    SourceNameHelper.Instance.GetPropertySourceName(typeof (Transit), Transit.TransitNamePropertyName),
                    transitData.Select(p => p.TRANSITNAME), aTrFilterEx);

            var transits = new List<Transit>();
            using (var mgrTr = IoC.Instance.Resolve<IBaseManager<Transit>>())
            {
                foreach (var aTrFilter in aTrFilters)
                {
                    var transs = mgrTr.GetFiltered(aTrFilter).ToArray();
                    if (transs.Length != 0)
                        transits.AddRange(transs);
                }
            }

            foreach (var artTransit in artsInTransits)
            {
                foreach (var artTr in artTransit.TRANSITDATAL)
                {
                    var existTrn = transits.FirstOrDefault(p => artTr.TRANSITNAME.Equals(p.TransitName));
                    if (existTrn != null)
                    {
                        artTr.TRANSITID = existTrn.TransitID;
                        Log.DebugFormat("Существует заголовок транзита «{0}»", artTr.TRANSITNAME);
                        continue;
                    }

                    throw new IntegrationLogicalException("Не найден заголовок транзита «{0}».", artTr.TRANSITNAME);
                }
            }
        }

        private bool LoadArtTransit(ArtWrapper item, IUnitOfWork uow)
        {
            if (item.TRANSITDATAL == null || item.TRANSITDATAL.Count == 0)
                return false;

            var transitDataValues = item.TRANSITDATAL.Select(i => i.TRANSITDATAVALUE).Distinct();
            var transitDataFilter = "'" + string.Join("','", transitDataValues) + "'";
            var transitFilter = string.Format("{0} in ({1}) and {2} = '{3}'",
                    SourceNameHelper.Instance.GetPropertySourceName(typeof(TransitData), TransitData.TransitDataValuePropertyName), transitDataFilter,
                    SourceNameHelper.Instance.GetPropertySourceName(typeof(TransitData), TransitData.TransitDataKeyPropertyName), item.ARTCODE);

            using (var transitDataMgr = IoC.Instance.Resolve<IBaseManager<TransitData>>())
            {
                if (uow != null)
                    transitDataMgr.SetUnitOfWork(uow);

                var transites = transitDataMgr.GetFiltered(transitFilter).ToArray();
                foreach (var tdw in item.TRANSITDATAL)
                {
                    if (!String.IsNullOrEmpty(tdw.TRANSITDATAVALUE) && tdw.TRANSITDATAVALUE != "")
                    {
                        if (transites.All(i => i.TransitDataValue != tdw.TRANSITDATAVALUE))
                        {
                            var trDataObj = new TransitData
                            {
                                TransitDataKey = item.ARTCODE,
                                TransitDataValue = tdw.TRANSITDATAVALUE,
                                TransitID = tdw.TRANSITID
                            };
                            SetXmlIgnore(trDataObj, false);
                            Log.Debug(trDataObj.DumpToXML());
                            transitDataMgr.Insert(ref trDataObj);
                            Log.InfoFormat("Созданы транзитные данные артикула «{0}» = «{1}»", item.ARTCODE,
                                tdw.TRANSITDATAVALUE);
                        }
                        else
                        {
                            Log.InfoFormat("Транзитные данные артикула «{0}» = «{1}» существуют", item.ARTCODE,
                                tdw.TRANSITDATAVALUE);
                        }
                    }
                }
            }
            return true;
        }

        public Exception ExecuteWorkflowBeforeProcessing(string wfcode, ArtWrapper artWrapper)
        {
            if (string.IsNullOrEmpty(wfcode) || artWrapper == null || artWrapper.ARTUPDATE != 1 ||
                string.IsNullOrEmpty(artWrapper.MandantCode) ||
                artWrapper.CUSTOMPARAMVAL == null || artWrapper.CUSTOMPARAMVAL.Count == 0)
                return null;

            //Получим wf
            var wfxaml = GetWorkflowXaml(wfcode);
            if (string.IsNullOrEmpty(wfxaml))
                return null;

            var art = new Art();
            MapTo(artWrapper, art);
            art.CUSTOMPARAMVAL = new WMSBusinessCollection<ArtCpv>();
            foreach (var cpvw in artWrapper.CUSTOMPARAMVAL)
            {
                var cpv = new ArtCpv();
                MapTo(cpvw, cpv);
                art.CUSTOMPARAMVAL.Add(cpv);
            }

            var imgr = new ImportManager();
            var bpContext = new BpContext
            {
                Items = new object[] { art }
            };
            bpContext.Set("MANDANTCODE", artWrapper.MandantCode);
            var result = imgr.ExecuteWfByCode(wfxaml, new Dictionary<string, object>
            {
                {BpContext.BpContextArgumentName, bpContext}
            }, TimeSpan.FromMinutes(5));

            return (result != null && result.Exception != null) ? result.Exception : null;
        }

        #region .  Find and cache Mandants, Factories, Countries, ArtGroups...

        private void FindMeasures(ArtWrapper[] artPackage)
        {
            _packageMeasures = new Measure[0];
            _missingMeasures = new string[0];

            var artsWithSkuList = artPackage.Where(art => !(art.SKUL == null || art.SKUL.Count == 0));
            var skuList = artsWithSkuList.SelectMany(i => i.SKUL);
            var measureCodes = skuList.Select(i => i.MEASURECODE_R.ToUpper()).Distinct().ToArray();
            var measuresFilter = "'" + string.Join("','", measureCodes) + "'";
            measuresFilter = string.Format("upper({0}) in ({1}) or upper({2}) in ({3})",
                SourceNameHelper.Instance.GetPropertySourceName(typeof(Measure), Measure.MeasureCodePropertyName), measuresFilter,
                SourceNameHelper.Instance.GetPropertySourceName(typeof(Measure), Measure.MeasureShortNamePropertyName), measuresFilter);

            using (var measureMgr = IoC.Instance.Resolve<IBaseManager<Measure>>())
            {
                try
                {
                    var measures = measureMgr.GetFiltered(measuresFilter, GetModeEnum.Partial).ToArray();
                    _packageMeasures = measures;
                    _missingMeasures = measureCodes.Except(measures.Select(m => m.MeasureCode.ToUpper()))
                        .Except(measures.Select(m => m.MeasureShortName.ToUpper())).ToArray();
                }
                catch (Exception ex)
                {
                    Log.Error(string.Format("Can't find all measures from file. Filter = '{0}'.", measuresFilter), ex);
                }
            }
        }

        private void FindFactories(ArtWrapper[] artPackage)
        {
            _packageFactories = new Factory[0];
            _missingFactories = new string[0];

            var factoryCodes = artPackage.Where(art => !string.IsNullOrEmpty(art.ARTFACTORY)).Select(art => art.ARTFACTORY.ToUpper()).Distinct().ToArray();
            if (factoryCodes.Length == 0)
                return;

            var factoryFilter = "'" + string.Join("','", factoryCodes) + "'";
            var mandantId = artPackage.FirstOrDefault().MANDANTID;
            factoryFilter = string.Format("upper({0}) in ({1}) and {2} = {3}",
                SourceNameHelper.Instance.GetPropertySourceName(typeof(Factory), Factory.FactoryCodePropertyName), factoryFilter,
                SourceNameHelper.Instance.GetPropertySourceName(typeof(Factory), Factory.PARTNERID_RPropertyName), mandantId);

            using (var factoryMgr = IoC.Instance.Resolve<IBaseManager<Factory>>())
            {
                try
                {
                    var factories = factoryMgr.GetFiltered(factoryFilter, GetModeEnum.Partial).ToArray();
                    _packageFactories = factories;
                    _missingFactories = factoryCodes.Except(factories.Select(m => m.FactoryCode)).ToArray();
                }
                catch (Exception ex)
                {
                    Log.Error(string.Format("Can't find all factories from file. Filter = '{0}'.", factoryFilter), ex);
                }
            }
        }

        private void FindCountries(ArtWrapper[] artPackage)
        {
            _packageCountries = new IsoCountry[0];
            const string emty2 = "  ";
            var emty2Length = emty2.Length;

            //Подготавливаем данные к поиску
            foreach (var artw in artPackage.Where(p => !string.IsNullOrEmpty(p.COUNTRYCODE_R)))
            {
                artw.COUNTRYCODE_R = artw.COUNTRYCODE_R.Trim();
                if (artw.COUNTRYCODE_R.Length > emty2Length)
                {
                    while (artw.COUNTRYCODE_R.Contains(emty2))
                    {
                        artw.COUNTRYCODE_R = artw.COUNTRYCODE_R.Replace(emty2, " ");    
                    }
                }
            }

            var countryCodes = artPackage.Where(art => !string.IsNullOrEmpty(art.COUNTRYCODE_R)).Select(art => art.COUNTRYCODE_R.ToUpper()).Distinct().ToArray();
            if (countryCodes.Length == 0)
                return;
            
            var type = typeof (IsoCountry);
            var filters = new List<string>();

            //COUNTRYALPHA2
            var calfa2Codes = countryCodes.Where(p => p.Length == 2).ToArray();
            if (calfa2Codes.Length > 0)
            {
                var filter = FilterHelper.GetArrayFilterIn(
                    string.Format("UPPER({0})",
                        SourceNameHelper.Instance.GetPropertySourceName(type, IsoCountry.COUNTRYALPHA2PropertyName)),
                    calfa2Codes);
                if (filter.Length > 0)
                    filters.AddRange(filter);
            }

            var ccodes = countryCodes.Where(p => p.Length > 2).ToArray();
            if (ccodes.Length > 0)
            {
                //COUNTRYCODE
                var filter = FilterHelper.GetArrayFilterIn(
                    string.Format("UPPER({0})",
                        SourceNameHelper.Instance.GetPropertySourceName(type, IsoCountry.COUNTRYCODEPropertyName)),
                    ccodes);
                if (filter.Length > 0)
                    filters.AddRange(filter);

                //COUNTRYNAMERUS
                filter = FilterHelper.GetArrayFilterIn(
                    string.Format("UPPER({0})",
                        SourceNameHelper.Instance.GetPropertySourceName(type, IsoCountry.COUNTRYNAMERUSPropertyName)),
                    ccodes);
                if (filter.Length > 0)
                    filters.AddRange(filter);
            }

            if (filters.Count == 0)
                return;

            var isocountries = new List<IsoCountry>();
            using (var countryMgr = IoC.Instance.Resolve<IBaseManager<IsoCountry>>())
            {
                foreach (var filter in filters)
                {
                    try
                    {
                        var countries = countryMgr.GetFiltered(filter, GetModeEnum.Partial).ToArray();
                        if (countries.Length > 0)
                            isocountries.AddRange(countries);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(string.Format("Can't find all countries from file. Filter = '{0}'.", filter),
                            ex);
                    }
                }
            }

            if (isocountries.Count > 0)
                _packageCountries = isocountries.ToArray();
        }
        
        #endregion .  Find and cache Mandants, Factories, Countries, ArtGroups...
    }
}
