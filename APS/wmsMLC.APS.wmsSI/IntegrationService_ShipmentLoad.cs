using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using MLC.WebClient;
using Oracle.DataAccess.Client;
using wmsMLC.APS.wmsSI.Helpers;
using wmsMLC.APS.wmsSI.Wrappers;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;
using wmsMLC.General.DAL;

namespace wmsMLC.APS.wmsSI
{
    public partial class IntegrationService
    {
        //Определяем режим обновленя cpv (UPDATEEXISTCPV) - обновление: false  или добавление: true параметра. По-умолчанию - обновление.
        private readonly Dictionary<string, bool> _cpAddedOrUpdated = new Dictionary<string, bool>
        {
            {"OWBClientDoc", true },
            {"OWBClientDocName", true },
            {"OWBClientDocDate", true }
        };

        public ErrorWrapper[] ShipmentLoad(SalesInvoiceWrapper item)
        {
            Contract.Requires(item != null);

            var processId = Guid.NewGuid();

            var retMessage = new List<ErrorWrapper>();
            Log.InfoFormat("Start of ShipmentLoad ({0})", processId);
            Log.DebugFormat("({0}) -> {1}", processId, item.DumpToXML());
            var startAllTime = DateTime.Now;
            var hostRefAndName = false;
            var updateexistcpv = item != null && item.UPDATEEXISTCPV > 0;

            try
            {
                IUnitOfWork uow = null;
                try
                {
                    uow = UnitOfWorkHelper.GetUnit();

                    item.MANDANTID = CheckMandant(item.MANDANTID, item.MandantCode, uow, false);
                    if (item.MANDANTID == null)
                        throw new NullReferenceException("Не указан MandantCode");
                    Log.DebugFormat("Мандант = {0}", item.MandantCode);

                    //Не для режима обновления CPV
                    if (!updateexistcpv)
                    {
                        if (item.OWB_HOSTREF_NAME == 1)
                            hostRefAndName = true;

                        item.OWBRECIPIENT = CheckPartnerHostRefOrName(item.MANDANTID.Value, item.OWBRECIPIENT_CODE,
                            item.OWBRECIPIENT_NAME, uow, hostRefAndName);
                            if (!item.OWBRECIPIENT.HasValue && (!item.OWBCREATE_RECIPIENT.HasValue || item.OWBCREATE_RECIPIENT == 0) && !hostRefAndName)
                            throw new IntegrationLogicalException("Неизвестный код получателя «{0}»!",
                                (string.IsNullOrEmpty(item.OWBRECIPIENT_NAME))
                                    ? item.OWBRECIPIENT_CODE
                                    : item.OWBRECIPIENT_NAME);

                        item.OWBPAYER = CheckPartnerHostRefOrName(item.MANDANTID.Value, item.OWBPAYER_CODE,
                            item.OWBPAYER_NAME, uow, hostRefAndName);
                        if (!item.OWBPAYER.HasValue && (!item.OWBCREATE_PAYER.HasValue || item.OWBCREATE_PAYER == 0) &&
                            !hostRefAndName)
                        {
                            //throw new IntegrationLogicalException("Неизвестный код плательщика «{0}»!",
                            //    (string.IsNullOrEmpty(item.OWBPAYER_NAME))
                            //        ? item.OWBPAYER_CODE
                            //        : item.OWBPAYER_NAME);
                            var message = string.Format("Неизвестный код плательщика «{0}»!",
                                string.IsNullOrEmpty(item.OWBPAYER_NAME)
                                    ? item.OWBPAYER_CODE
                                    : item.OWBPAYER_NAME);
                            Log.Debug(message);
                            var ew = new ErrorWrapper { ERRORCODE = MessageHelper.WarningCode.ToString(), ERRORMESSAGE = message };
                            retMessage.Add(ew);
                        }

                        if (!string.IsNullOrEmpty(item.OWBCARRIERCODE))
                        {
                            var errmessages = ShipmentLoadHelper.FillCarrier(item, uow, Log);
                            if (errmessages.Length > 0)
                                MessageHelper.AddMessage(errmessages, retMessage);
                        }
                    }

                    uow.BeginChanges();
                    OwbLoadInternal(item, retMessage, uow);

                    //Проверяем существавание ид. накладной
                    if (!item.OWBID.HasValue)
                        throw new DeveloperException("({0}) -> Не определен ид. у расходной накладной '{1}'.", processId, item.OWBNAME);

                    uow.CommitChanges();

                    //Не для режима обновления CPV
                    if (!updateexistcpv)
                    {
                        //Маршрутизация
                        try
                        {
                            if (CheckCpv(item.MANDANTID, uow))
                            {
                                Log.DebugFormat("({0}) -> MandantUseRouteL2 = 1", processId);
                                RoutingOwb(item.OWBID.Value, uow);
                                Log.DebugFormat("({0}) -> Накладная '{1}' (ид. '{2}') маршрутизирована.", processId, item.OWBNAME, item.OWBID);
                            }
                        }
                        catch (Exception ex)
                        {
                            var message = string.Format("({0}) -> Ошибки маршрутизации (MandantUseRouteL2 = 1) накладной '{1}' (ид. '{2}'). {3}",
                                processId, item.OWBNAME, item.OWBID, ExceptionHelper.ExceptionToString(ex));
                            Log.Error(message, ex);
                        }

                        //Резервирование

                        try
                        {
                            if (item.OWBAUTORES > 0)
                            {
                                Log.DebugFormat("({0}) -> Резервирование", processId);
                                var api = IoC.Instance.Resolve<WmsAPI>();
                                api.SetOwbReserve((int) item.OWBID.Value);
                                Log.DebugFormat(
                                    "({0}) -> Накладная '{1}' (ид. '{2}') поставлена в очеред на резервирование.",
                                    processId, item.OWBNAME, item.OWBID);
                            }
                        }
                        catch (Exception ex)
                        {
                            var message =
                                string.Format("({0}) -> Ошибки резервирования накладной '{1}' (ид. '{2}'). {3}",
                                    processId, item.OWBNAME, item.OWBID, ExceptionHelper.ExceptionToString(ex));
                            Log.Error(message, ex);
                        }
                    }
                }
                catch (IntegrationLogicalException iex)
                {
                    if (uow != null)
                        uow.RollbackChanges();

                    var message = ExceptionHelper.ExceptionToString(iex);
                    Log.Error(message, iex);

                    var ew = new ErrorWrapper {ERRORCODE = MessageHelper.ErrorCode.ToString(), ERRORMESSAGE = message};
                    retMessage.Add(ew);
                    MessageHelper.RemoveSuccessMessage(retMessage);
                }
                catch (Exception ex)
                {
                    if (uow != null)
                        uow.RollbackChanges();

                    var message = ExceptionHelper.ExceptionToString(ex);
                    Log.Error(message, ex);

                    var ew = new ErrorWrapper
                    {
                        ERRORCODE = MessageHelper.ErrorCode.ToString(),
                        ERRORMESSAGE = "Системная ошибка: " + message
                    };
                    retMessage.Add(ew);
                    MessageHelper.RemoveSuccessMessage(retMessage);
                }
                finally
                {
                    if (uow != null)
                        uow.Dispose();
                }
            }
            catch (Exception ex)
            {
                var message = ExceptionHelper.ExceptionToString(ex);
                Log.Error(message, ex);
                throw new FaultException<string>(message, new FaultReason(message));
            }
            finally
            {
                if (retMessage.Count > 0)
                    Log.DebugFormat("Messages ({0}): {1}", processId, string.Join(";", retMessage.Select(p => string.Format("Code: {0}, Message: {1}", p.ERRORCODE, p.ERRORMESSAGE))));
                Log.DebugFormat("Общее время загрузки {0} ({1})", DateTime.Now - startAllTime, processId);
                Log.InfoFormat("End of ShipmentLoad ({0})", processId);
            }
            return retMessage.ToArray();
        }

        private void FillFactoryOwb(SalesInvoiceWrapper item, IUnitOfWork uow)
        {
            using (var factoryMgr = IoC.Instance.Resolve<IBaseManager<Factory>>())
            {
                var factoryFilter = string.Format("upper({0}) = upper('{1}') and {2} = {3}",
                    SourceNameHelper.Instance.GetPropertySourceName(typeof (Factory), Factory.FactoryCodePropertyName),
                    item.OWBFACTORY,
                    SourceNameHelper.Instance.GetPropertySourceName(typeof (Factory), Factory.PARTNERID_RPropertyName),
                    item.MANDANTID);

                if (uow != null)
                    factoryMgr.SetUnitOfWork(uow);
                var factorys = factoryMgr.GetFiltered(factoryFilter).ToArray();

                if (factorys.Length == 0)
                    throw new IntegrationLogicalException("Не найден код фабрики «{0}» (накладная «{1}»)",
                        item.OWBFACTORY, item.OWBNAME);

                if (factorys.Length > 1)
                    throw new IntegrationLogicalException(
                        "Существует несколько значений для кода фабрики «{0}» (накладная «{1}»)", item.OWBFACTORY,
                        item.OWBNAME);

                item.FACTORYID_R = factorys[0].FactoryID;
                Log.DebugFormat("Фабрика = {0}", item.FACTORYID_R);
            }
        }

        private static void FillArtOwb(SalesInvoiceWrapper item, List<ErrorWrapper> retMessage, IUnitOfWork uow)
        {
            var errCountOld = MessageHelper.GetErrorCount(retMessage);
            var errmessage = string.Format("Ошибка при проверке артикулов (накладная «{0}»)", item.OWBNAME);
            var artNames =
                item.OWBPOSL.Where(p => !string.IsNullOrEmpty(p.OWBPOSARTNAME))
                    .Select(i => i.OWBPOSARTNAME.ToUpper())
                    .Distinct()
                    .ToArray();
            if (artNames.Length == 0)
            {
                var ew = new ErrorWrapper
                {
                    ERRORCODE = MessageHelper.ErrorCode.ToString(),
                    ERRORMESSAGE = string.Format("Отсутствуют артикула в позициях накладной '{0}'.", item.OWBNAME)
                };
                retMessage.Add(ew);
                throw new IntegrationLogicalException(errmessage);
            }

            var arts = new List<Art>();
            var typeart = typeof(Art);
            var artsList =
                FilterHelper.GetArrayFilterIn(
                    string.Format("UPPER({0})",
                        SourceNameHelper.Instance.GetPropertySourceName(typeart, Art.ArtNamePropertyName)), artNames,
                    string.Format(" and {0} = {1}",
                        SourceNameHelper.Instance.GetPropertySourceName(typeart,
                            Art.MANDANTIDPropertyName),
                        item.MANDANTID));

            using (var mgrArt = IoC.Instance.Resolve<IBaseManager<Art>>())
            {
                if (uow != null)
                    mgrArt.SetUnitOfWork(uow);

                foreach (var artFilter in artsList)
                {
                    var artspart = mgrArt.GetFiltered(artFilter).ToArray();
                    if (artspart.Length > 0)
                        arts.AddRange(artspart);
                }

                foreach (var pos in item.OWBPOSL)
                {
                    var existsArt = arts.Where(i => pos.OWBPOSARTNAME.EqIgnoreCase(i.ArtName)).ToArray();
                    if (existsArt.Length > 1 && (existsArt = arts.Where(i => pos.OWBPOSARTNAME == i.ArtName).ToArray()).Length > 1)
                    {
                        var ew = new ErrorWrapper
                        {
                            ERRORCODE = MessageHelper.ErrorCode.ToString(),
                            ERRORMESSAGE = string.Format("Существует несколько значений для артикула «{0}»",
                                pos.OWBPOSARTNAME)
                        };
                        retMessage.Add(ew);
                        continue;
                    }
                    if (existsArt.Length == 0)
                    {
                        var ew = new ErrorWrapper
                        {
                            ERRORCODE = MessageHelper.ErrorCode.ToString(),
                            ERRORMESSAGE = string.Format("Не найден артикул «{0}»",
                                pos.OWBPOSARTNAME)
                        };
                        retMessage.Add(ew);
                        continue;
                    }
                    pos.OWBPOSARTCODE = existsArt[0].ArtCode;
                }
                if (MessageHelper.GetErrorCount(retMessage) > errCountOld)
                    throw new IntegrationLogicalException(errmessage);
            }
        }

        private static bool FillArtGroupOwb(SalesInvoiceWrapper item, IUnitOfWork uow)
        {
            if (item.OWBPOSL.All(p => string.IsNullOrEmpty(p.OWBPOSGROUPCHECK)))
                return false;

            var artCheckCodes = item.OWBPOSL.Where(p => !string.IsNullOrEmpty(p.OWBPOSARTCODE)).Select(p => p.OWBPOSARTCODE).Distinct().ToArray();
            var artCheckFilterValues = "'" + string.Join("','", artCheckCodes) + "'";

            using (var a2GMgr = IoC.Instance.Resolve<IBaseManager<Art2Group>>())
            {
                if (uow != null)
                    a2GMgr.SetUnitOfWork(uow);

                var art2GroupFilter = string.Format("{0} in ({1})",
                    SourceNameHelper.Instance.GetPropertySourceName(typeof (Art2Group),
                        Art2Group.Art2GroupArtCodePropertyName), artCheckFilterValues);
                var art2Groups = a2GMgr.GetFiltered(art2GroupFilter).ToArray();

                foreach (var pos in item.OWBPOSL.Where(p => !string.IsNullOrEmpty(p.OWBPOSGROUPCHECK)))
                {
                    var existsArtGroup = art2Groups.Where(p => string.Equals(pos.OWBPOSARTCODE, p.Art2GroupArtCode)).ToArray();

                    foreach (var artGroup in existsArtGroup)
                    {
                        //if (pos.OWBPOSGROUPCHECK.IndexOf(',') == -1) 
                        //{
                        //    if (artGroup.Art2GroupArtGroupCode != pos.OWBPOSGROUPCHECK)
                        //        pos.OWBPOSBATCH = null;
                        //}
                        //else
                        //{
                        //    var splitGroup = pos.OWBPOSGROUPCHECK.Split(',');
                        //    if (splitGroup.Any(strGroup => artGroup.Art2GroupArtGroupCode == strGroup))
                        //    {
                        //        return true;
                        //    }
                        //}

                        //Изменения по http://mp-ts-nwms/issue/wmsMLC-12529
                        if (pos.OWBPOSGROUPCHECK.IndexOf(',') >= 0)
                        {
                            var splitGroup = pos.OWBPOSGROUPCHECK.Split(" , ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            if (splitGroup.Any(strGroup => artGroup.Art2GroupArtGroupCode == strGroup))
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }

        private void FillSkuforOwb(SalesInvoiceWrapper item, IEnumerable<string> artCodes, List<ErrorWrapper> retMessage,
            IUnitOfWork uow)
        {
            if (item.OWBPOSL.Count == 0)
                return;

            var errCountOld = MessageHelper.GetErrorCount(retMessage);
            var isNotMeasures = string.IsNullOrEmpty(item.OWBPOSL[0].OWBPOSMEASURE);

            using (var skuMgr = IoC.Instance.Resolve<IBaseManager<SKU>>())
            {
                string[] skuList;
                if (!isNotMeasures)
                {
                    var measureCodes = item.OWBPOSL.Select(i => i.OWBPOSMEASURE.ToUpper()).Distinct();
                    var measuresFilterValue = "'" + string.Join("','", measureCodes) + "'";

                    skuList = FilterHelper.GetArrayFilterIn("ARTCODE_R", artCodes,
                        string.Format(" and {0} in ({1}) and ({2} = 1 or {3} = 1)",
                            SourceNameHelper.Instance.GetPropertySourceName(typeof (SKU), SKU.MeasureCodePropertyName),
                            measuresFilterValue,
                            SourceNameHelper.Instance.GetPropertySourceName(typeof (SKU), SKU.SKUClientPropertyName),
                            SourceNameHelper.Instance.GetPropertySourceName(typeof (SKU), SKU.SKUPrimaryPropertyName)));
                }
                else
                {
                    skuList = FilterHelper.GetArrayFilterIn("ARTCODE_R", artCodes,
                        string.Format(" and ({0} = 1 or {1} = 1)",
                            SourceNameHelper.Instance.GetPropertySourceName(typeof (SKU), SKU.SKUClientPropertyName),
                            SourceNameHelper.Instance.GetPropertySourceName(typeof (SKU), SKU.SKUPrimaryPropertyName)));
                }
                var skuDefList = FilterHelper.GetArrayFilterIn("ARTCODE_R", artCodes,
                    string.Format(" and {0} = 1",
                        SourceNameHelper.Instance.GetPropertySourceName(typeof (SKU), SKU.SKUDefaultPropertyName)));

                if (uow != null)
                    skuMgr.SetUnitOfWork(uow);

                var skus = new List<SKU>();
                var skusDef = new List<SKU>();
                foreach (var skuFilter in skuList)
                {
                    skus.AddRange(skuMgr.GetFiltered(skuFilter).ToArray());
                }

                //if (item.OWBCHECKMULTIPLE == 1)
                if (item.OWBCHECKMULTIPLE == 1 && item.OWBPOSL.Any(i => i.OWBPOSCHECKMULTIPLE == 1))
                {
                    foreach (var skuDefFilter in skuDefList)
                    {
                        skusDef.AddRange(skuMgr.GetFiltered(skuDefFilter).ToArray());
                    }
                }

                foreach (var owbPosWrapper in item.OWBPOSL)
                {
                    SKU existSku;
                    if (!isNotMeasures)
                        existSku = skus.FirstOrDefault(i => owbPosWrapper.OWBPOSARTCODE.Equals(i.ArtCode) &&
                                                            owbPosWrapper.OWBPOSMEASURE.Equals(i.MeasureCode) &&
                                                            i.SKUClient);
                    else
                        existSku = skus.FirstOrDefault(i => owbPosWrapper.OWBPOSARTCODE.Equals(i.ArtCode) &&
                                                            i.SKUClient);

                    if (existSku == null)
                        if (item.OWBALLOWBASE == 0)
                        {
                            if (!isNotMeasures)
                            {
                                var ew = new ErrorWrapper
                                {
                                    ERRORCODE = MessageHelper.ErrorCode.ToString(),
                                    ERRORMESSAGE =
                                        string.Format("Не существует клиентской SKU для артикула «{0}» с ЕИ «{1}»",
                                            owbPosWrapper.OWBPOSARTNAME, owbPosWrapper.OWBPOSMEASURE)
                                };
                                retMessage.Add(ew);
                                continue;
                            }
                            else
                            {
                                var ew = new ErrorWrapper
                                {
                                    ERRORCODE = MessageHelper.ErrorCode.ToString(),
                                    ERRORMESSAGE = string.Format("Не существует клиентской SKU для артикула «{0}»",
                                        owbPosWrapper.OWBPOSARTNAME)
                                };
                                retMessage.Add(ew);
                                continue;
                            }
                        }
                        else
                        {
                            existSku =
                                skus.FirstOrDefault(i => owbPosWrapper.OWBPOSARTCODE.Equals(i.ArtCode) && i.SKUPrimary);
                            if (existSku == null)
                            {
                                var ew = new ErrorWrapper
                                {
                                    ERRORCODE = MessageHelper.ErrorCode.ToString(),
                                    ERRORMESSAGE = string.Format("Не существует даже базовой SKU для артикула «{0}»",
                                        owbPosWrapper.OWBPOSARTNAME)
                                };
                                retMessage.Add(ew);
                                continue;
                            }
                            owbPosWrapper.SKUID_R = existSku.SKUID;
                        }
                    else
                    {
                        owbPosWrapper.SKUID_R = existSku.SKUID;
                    }

                    if (item.OWBCHECKMULTIPLE == 1 && owbPosWrapper.OWBPOSCHECKMULTIPLE == 1)
                    {
                        //int res;
                        var existSkuDefault =
                            skusDef.FirstOrDefault(i => owbPosWrapper.OWBPOSARTCODE.Equals(i.ArtCode) && i.SKUDefault);
                        if (existSkuDefault == null)
                        {
                            var ew = new ErrorWrapper
                            {
                                ERRORCODE = MessageHelper.ErrorCode.ToString(),
                                ERRORMESSAGE =
                                    string.Format("Не существует SKU по умолчанию для артикула «{0}»",
                                        owbPosWrapper.OWBPOSARTNAME)
                            };
                            retMessage.Add(ew);
                            continue;
                        }

                        //if (!int.TryParse(Convert.ToString(owbPosWrapper.OWBPOSCOUNT.To<double>() / existSkuDefault.SKUCount), out res))
                        //{
                        //    var ew = new ErrorWrapper
                        //    {
                        //        ERRORCODE = MessageHelper.ErrorCode.ToString(),
                        //        ERRORMESSAGE = string.Format("У артикула «{0}» количество «{1}» не кратно коробу («{2}»)", owbPosWrapper.OWBPOSARTNAME,
                        //                                    owbPosWrapper.OWBPOSCOUNT, existSkuDefault.SKUCount)
                        //    };
                        //    retMessage.Add(ew);
                        //    continue;
                        //}
                        //Log.DebugFormat("Артикул «{0}» -> кратность = {1}", owbPosWrapper.OWBPOSARTNAME, res);

                        var skuratio = Convert.ToDouble(owbPosWrapper.OWBPOSCOUNT)/existSkuDefault.SKUCount;
                        var skuratioceil = Convert.ToInt64(Math.Floor(skuratio));
                        var skurem = skuratio - skuratioceil;

                        if (skuratioceil == 0 || skurem > 0)
                        {
                            var ew = new ErrorWrapper
                            {
                                ERRORCODE = MessageHelper.ErrorCode.ToString(),
                                ERRORMESSAGE =
                                    string.Format("У артикула «{0}» количество «{1}» не кратно коробу («{2}»)",
                                        owbPosWrapper.OWBPOSARTNAME,
                                        owbPosWrapper.OWBPOSCOUNT, existSkuDefault.SKUCount)
                            };
                            retMessage.Add(ew);
                            continue;
                        }

                        Log.DebugFormat("Артикул «{0}» -> кратность = {1}", owbPosWrapper.OWBPOSARTNAME, skuratioceil);
                    }

                    if (!String.IsNullOrEmpty(owbPosWrapper.OWBPOSKITCODE) && owbPosWrapper.OWBPOSKITCODE != "")
                        owbPosWrapper.OWBPOSARTNAME = owbPosWrapper.OWBPOSKITCODE;
                }

                if (MessageHelper.GetErrorCount(retMessage) > errCountOld)
                    throw new IntegrationLogicalException("Ошибка при поиске SKU (накладная «{0}»)", item.OWBNAME);
            }
        }

        private void LoadBarcodesOwb(OWBPosWrapper pw, List<ErrorWrapper> retMessage, IUnitOfWork uow)
        {
            if (pw.BarcodeList == null)
                return;

            using (var mgrBarcode = IoC.Instance.Resolve<IBaseManager<Barcode>>())
            {
                if (uow != null)
                    mgrBarcode.SetUnitOfWork(uow);

                foreach (var barcode in pw.BarcodeList)
                {
                    if (barcode.BARCODE2ENTITY == "ART")
                    {
                        var brFilter = string.Format("{0} = '{1}' and {2} = 'ART'",
                            SourceNameHelper.Instance.GetPropertySourceName(typeof (Barcode),
                                Barcode.BarcodeValuePropertyName),
                            barcode.BARCODEVALUE,
                            SourceNameHelper.Instance.GetPropertySourceName(typeof (Barcode),
                                Barcode.BarcodeEntityPropertyName));
                        var barcodes = mgrBarcode.GetFiltered(brFilter).ToArray();

                        if (barcodes.Any(brcd => brcd.BarcodeKey != pw.OWBPOSARTCODE))
                        {
                            var ew = new ErrorWrapper
                            {
                                ERRORCODE = MessageHelper.ErrorCode.ToString(),
                                ERRORMESSAGE =
                                    string.Format("Штрих-код «{0}» (Артикул «{1}») уже существует для артикула «{2}»",
                                        barcode.BARCODEVALUE, pw.OWBPOSARTNAME, barcodes[0].BarcodeKey)
                            };
                            retMessage.Add(ew);
                            return;
                        }
                        var bar = new Barcode {BarcodeKey = pw.OWBPOSARTCODE};
                        bar = MapTo(barcode, bar);
                        SetXmlIgnore(bar, false);
                        mgrBarcode.Insert(ref bar);
                    }
                    else
                    {
                        var barcodeFilter = string.Format("{0} = '{1}' and {2} = '{3}' and {4} = 'SKU'",
                            SourceNameHelper.Instance.GetPropertySourceName(typeof (Barcode),
                                Barcode.BarcodeKeyPropertyName),
                            pw.SKUID_R,
                            SourceNameHelper.Instance.GetPropertySourceName(typeof (Barcode),
                                Barcode.BarcodeValuePropertyName),
                            barcode.BARCODEVALUE,
                            SourceNameHelper.Instance.GetPropertySourceName(typeof (Barcode),
                                Barcode.BarcodeEntityPropertyName));
                        var barcodes = mgrBarcode.GetFiltered(barcodeFilter);

                        if (barcodes.Any()) continue;

                        var bar = new Barcode {BarcodeKey = pw.SKUID_R.ToString()};
                        bar = MapTo(barcode, bar);
                        SetXmlIgnore(bar, false);
                        mgrBarcode.Insert(ref bar);
                    }
                }
            }
        }

        /// <summary>
        /// Загрузка Cpv для OWB.
        /// </summary>
        private string[] LoadCpvOwb(SalesInvoiceWrapper item, bool addCpv, IUnitOfWork uow)
        {
            var updateexistcpv = item != null && item.UPDATEEXISTCPV > 0;
            var messages = new List<string>();
            Log.Debug("Загрузка CPV для расходной накладной.");

            var loadentityname = item.OWBNAME;

            if (item.CUSTOMPARAMVAL == null || item.CUSTOMPARAMVAL.Count == 0)
            {
                var message = string.Format("Расходная накладная '{0}' не содержит CPV", loadentityname);
                //messages.Add(message);
                Log.Debug(message);
                return messages.ToArray();
            }

            const string cpEntity = "OWB";
            var cpvkey = item.OWBID.ToString();
            var wmscpvs = new List<OWBCpv>();
            var cpvid = CpvHelper.GetMinCpvId(item.CUSTOMPARAMVAL.Select(p => p.CPVID_OWBCPV).ToArray());
            var cpvHelper = new CpvHelper<OWBCpv>(cpEntity, cpvkey);

            //Получим все cpv
            var allWmsCpvs = cpvHelper.GetAllCpv(uow);
            if (allWmsCpvs.Length == 0)
            {
                var message = string.Format("Для сущности '{0}' CP не определены.", cpEntity);
                messages.Add(message);
                Log.Error(message);
                return messages.ToArray();
            }

            Func<string, OWBCpv> findCpvHandler = code =>
            {
                var findcpvs = allWmsCpvs.Where(p => p.CustomParamCode.EqIgnoreCase(code)).ToArray();
                if (findcpvs.Length == 0)
                {
                    var message = string.Format("Не найден cpv '{0}' для расходной накладной '{1}'.", code, loadentityname);
                    messages.Add(message);
                    return null;
                }

                if (findcpvs.Length > 1)
                {
                    var maxcpvid = findcpvs.Max(p => p.GetKey<decimal>());
                    return findcpvs.First(p => p.GetKey<decimal>() == maxcpvid);
                }

                return findcpvs[0];
            };

            Func<decimal?, bool> hasChildrenHandler = cpvwid =>
            {
                return cpvwid.HasValue && item.CUSTOMPARAMVAL.Any(p => p.CPVPARENT_OWBCPV == cpvwid);
            };

            foreach (var cw in item.CUSTOMPARAMVAL)
            {
                var customParamCode = cw.CUSTOMPARAMCODE_R_OWBCPV;

                //Проверяем существование такого cpv
                var wcpv = findCpvHandler(customParamCode);
                if (wcpv == null)
                    continue;

                List<string> errmessages;
                var isvalid = ShipmentLoadHelper.ValidateCpv(item, cw, uow, Log, out errmessages);
                if (errmessages.Count > 0)
                    messages.AddRange(errmessages);

                if (!isvalid)
                {
                    //Если не прошли проверку очищаем значение
                    cw.CPVVALUE_OWBCPV = null;
                }

                //Проверяем, если значение == null и нет детей, то не добавляем такой CPV
                if (string.IsNullOrEmpty(cw.CPVVALUE_OWBCPV) && !hasChildrenHandler(cw.CPVID_OWBCPV))
                    continue;

                //Ищем среди существующих
                if (updateexistcpv)
                {
                    //Проверяем режим обновления
                    var isadded = _cpAddedOrUpdated.ContainsKey(customParamCode) && _cpAddedOrUpdated[customParamCode] &&
                        wcpv.Cp != null && wcpv.Cp.CustomParamCount > 1;

                    var wcpvkey = wcpv.GetKey<decimal>();
                    if (wcpvkey > 0 && !isadded) //Существует такой cpv
                    {
                        var key = cw.CPVID_OWBCPV;
                        cw.CPVID_OWBCPV = wcpvkey;
                        var childs = item.CUSTOMPARAMVAL.Where(i => i.CPVPARENT_OWBCPV == key).ToArray();
                        foreach (var cpvw in childs)
                        {
                            cpvw.CPVPARENT_OWBCPV = wcpvkey;
                        }
                    }
                }

                var cpv = new OWBCpv();
                cpv = MapTo(cw, cpv);
                cpv.Cp = wcpv.Cp;

                //HACK: Не заполненный CPVParent
                if (cpv.CPVID < 0 && cpv.CPVID != -1 && !cpv.CPVParent.HasValue)
                    cpv.CPVParent = -1;
                if (!cpv.CPVID.HasValue)
                    cpv.CPVID = --cpvid;
                cpv.CPV2Entity = cpEntity;
                cpv.CPVKey = cpvkey;

                wmscpvs.Add(cpv);
            }

            if (wmscpvs.Any())
            {
                //Удаление ссылок на несуществкющих родителей
                cpvHelper.ClearBadParent(wmscpvs);
                cpvHelper.Save(source: wmscpvs.ToArray(), allowUpdate: true, includeCpvWithDafaultValue: false,
                    verify: false, uow: uow);
                Log.DebugFormat("Для расходной накладной '{0}' загружены CPV.", loadentityname);
            }

            if (!addCpv)
                return messages.ToArray();

            using (var cpvMgr = IoC.Instance.Resolve<IBaseManager<OWBCpv>>())
            {
                var wmscpv = new OWBCpv
                {
                    CPVKey = cpvkey,
                    CPV2Entity = cpEntity,
                    CustomParamCode = "OWBTagL1",
                    CPVValue = "PACK"
                };
                cpvMgr.Insert(ref wmscpv);
                Log.InfoFormat("Загружен дополнительный CPV «OWBTagL1» (ID = {0}) для расходной накладной '{1}'", wmscpv.CPVID, loadentityname);
            }

            return messages.ToArray();
        }

        /// <summary>
        /// Загрузка Cpv для OwbPos. Перед загрузкой все Cpv должны быть удалены.
        /// </summary>
        private string[] LoadOwbPosCpv(OWBPosWrapper item, string owbname, IUnitOfWork uow)
        {
            Log.Debug("Загрузка CPV для позиций расходной накладной.");
            var loadentityname = item.OWBPOSNUMBER;
            List<string> messages;
            var wmscpvs = CreateOwbPosCpv(item, owbname, uow, out messages);
            if (wmscpvs != null && wmscpvs.Any())
            {
                var cpvkey = item.OWBPOSID.HasValue ? item.OWBPOSID.ToString() : null;
                var cpvHelper = new CpvHelper<OWBPosCpv>(ShipmentLoadHelper.Cpv2EntityOwbPosName, cpvkey);

                cpvHelper.Save(source: wmscpvs.ToArray(), allowUpdate: true, includeCpvWithDafaultValue: false,
                    verify: false,
                    uow: uow);
                Log.DebugFormat("Для позиции '{0}' расходной накладной '{1}' загружены CPV.", loadentityname, owbname);
            }

            return messages.ToArray();
        }

        private OWBPosCpv[] CreateOwbPosCpv(OWBPosWrapper item, string owbname, IUnitOfWork uow, out List<string> messages)
        {
            messages = new List<string>();
            var loadentityname = item.OWBPOSNUMBER;
            if (item.CUSTOMPARAMVAL == null || item.CUSTOMPARAMVAL.Count == 0)
            {
                //var message = string.Format("Позиция '{0}' расходной накладной '{1}' не содержит CPV.", loadentityname,
                //    owbname);
                //Log.Debug(message);
                return null;
            }

            var cpvkey = item.OWBPOSID.HasValue ? item.OWBPOSID.ToString() : null;
            var wmscpvs = new List<OWBPosCpv>();
            var cpvid = CpvHelper.GetMinCpvId(item.CUSTOMPARAMVAL.Select(p => p.CPVID_OWBPOSCPV).ToArray());
            var cpvHelper = new CpvHelper<OWBPosCpv>(ShipmentLoadHelper.Cpv2EntityOwbPosName, cpvkey);

            //Получаем все cp
            var allcp = cpvHelper.GetAllCp(null);
            if (allcp.Length == 0)
            {
                var message = string.Format("Для сущности '{0}' CP не определены.", ShipmentLoadHelper.Cpv2EntityOwbPosName);
                messages.Add(message);
                Log.Error(message);
                return null;
            }

            Func<string, List<string>, CustomParam> findCpHandler = (code, errmessages) =>
            {
                var wmscp = allcp.FirstOrDefault(p => p.GetKey<string>().EqIgnoreCase(code) && p.CustomParam2Entity == ShipmentLoadHelper.Cpv2EntityOwbPosName);
                if (wmscp == null)
                {
                    var message =
                        string.Format("Не найден CP но коду '{0}' (сущность '{1}') для позиции '{2}' расходной накладной '{3}'.", code,
                            ShipmentLoadHelper.Cpv2EntityOwbPosName, loadentityname, owbname);
                    errmessages.Add(message);
                }
                return wmscp;
            };

            Func<decimal?, bool> hasChildrenHandler = cpvwid =>
            {
                return cpvwid.HasValue && item.CUSTOMPARAMVAL.Any(p => p.CPVPARENT_OWBPOSCPV == cpvwid);
            };

            foreach (var cw in item.CUSTOMPARAMVAL)
            {
                var customParamCode = cw.CUSTOMPARAMCODE_R_OWBPOSCPV;

                //Проверяем существование такого cp
                var cp = findCpHandler(customParamCode, messages);
                if (cp == null)
                    continue;

                //Нет валидации

                //Проверяем, если значение == null и нет детей, то не добавляем такой CPV
                if (string.IsNullOrEmpty(cw.CPVVALUE_OWBPOSCPV) && !hasChildrenHandler(cw.CPVID_OWBPOSCPV))
                    continue;

                var cpv = new OWBPosCpv();
                cpv = MapTo(cw, cpv);

                //HACK: Не заполненный CPVParent
                if (cpv.CPVID != -1 && !cpv.CPVParent.HasValue)
                    cpv.CPVParent = -1;
                if (!cpv.CPVID.HasValue)
                    cpv.CPVID = --cpvid;
                cpv.CPV2Entity = ShipmentLoadHelper.Cpv2EntityOwbPosName;
                cpv.CPVKey = cpvkey;

                wmscpvs.Add(cpv);
            }

            return wmscpvs.ToArray();
        }

        private void DeleteCpvOwb(decimal owbId, IUnitOfWork uow)
        {
            using (var mgr = IoC.Instance.Resolve<IBaseManager<OWBCpv>>())
            {
                if (uow != null)
                    mgr.SetUnitOfWork(uow);
                var filterCpvDel = string.Format("{0} = '{1}'",
                    SourceNameHelper.Instance.GetPropertySourceName(typeof (OWBCpv), OWBCpv.CPVKeyPropertyName), owbId);
                var cpvDel = mgr.GetFiltered(filterCpvDel).ToArray();
                if (cpvDel.Length > 0)
                {
                    mgr.Delete(cpvDel);
                    Log.InfoFormat("Удалены CPV по накладной (ID = '{0}')", owbId);
                }
            }
        }

        private static void ConvertSku2SkuOwb(SalesInvoiceWrapper item, IEnumerable<string> artCodes, decimal currentNum, bool isIncludedGroup, List<ErrorWrapper> retMessage, IUnitOfWork uow)
        { 
            if (item.OWBPOSL == null || item.OWBPOSL.Count == 0)
                return;

            var errCountOld = MessageHelper.GetErrorCount(retMessage);
            using (var skuMgr = IoC.Instance.Resolve<IBaseManager<SKU>>())
            {
                var skuList = FilterHelper.GetArrayFilterIn("ARTCODE_R", artCodes,
                    string.Format(" and ({0} = 1 or {1} = 1)",
                        SourceNameHelper.Instance.GetPropertySourceName(typeof (SKU), SKU.SKUClientPropertyName),
                        SourceNameHelper.Instance.GetPropertySourceName(typeof (SKU), SKU.SKUDefaultPropertyName)));

                if (uow != null)
                    skuMgr.SetUnitOfWork(uow);

                var skus = new List<SKU>();
                foreach (var skuFilter in skuList)
                {
                    skus.AddRange(skuMgr.GetFiltered(skuFilter).ToArray());
                }
                var newOwbPos = new WMSBusinessCollection<OWBPosWrapper>();

                foreach (var owbPosWrapper in item.OWBPOSL)
                {
                    var existSkuDefault =
                        skus.FirstOrDefault(i => owbPosWrapper.OWBPOSARTCODE.Equals(i.ArtCode) && i.SKUDefault);
                    if (existSkuDefault == null)
                    {
                        var ew = new ErrorWrapper
                        {
                            ERRORCODE = MessageHelper.ErrorCode.ToString(),
                            ERRORMESSAGE =
                                string.Format("Не существует SKU по умолчанию для артикула «{0}»",
                                    owbPosWrapper.OWBPOSARTNAME)
                        };
                        retMessage.Add(ew);
                        continue;
                    }

                    if (!isIncludedGroup)
                    {
                        var existSkuClient =
                            skus.FirstOrDefault(i => owbPosWrapper.OWBPOSARTCODE.Equals(i.ArtCode) && i.SKUClient);
                        if (existSkuClient == null)
                        {
                            var ew = new ErrorWrapper
                            {
                                ERRORCODE = MessageHelper.ErrorCode.ToString(),
                                ERRORMESSAGE =
                                    string.Format("Не существует клиентской SKU для артикула «{0}»",
                                        owbPosWrapper.OWBPOSARTNAME)
                            };
                            retMessage.Add(ew);
                            continue;
                        }

                        using (var mgrBpProc = IoC.Instance.Resolve<BPProcessManager>())
                        {
                            mgrBpProc.SetUnitOfWork(uow);
                            int res;
                            var originalQty = owbPosWrapper.OWBPOSCOUNT.To<decimal>();
                            var retQty = mgrBpProc.ConvertSKUtoSKU(existSkuClient.SKUID, existSkuDefault.SKUID, 1,
                                owbPosWrapper.OWBPOSCOUNT);

                            if (int.TryParse(Convert.ToString(retQty), out res))
                            {
                                owbPosWrapper.SKUID_R = existSkuDefault.SKUID;
                                owbPosWrapper.OWBPOSCOUNT2SKU = originalQty.To<double>()/retQty.To<double>();
                                owbPosWrapper.OWBPOSCOUNT = retQty;
                            }
                            else
                            {
                                var resultQty = Math.Floor(retQty);
                                if (resultQty > 0)
                                {
                                    owbPosWrapper.SKUID_R = existSkuDefault.SKUID;
                                    owbPosWrapper.OWBPOSCOUNT2SKU = existSkuDefault.SKUCount;
                                    owbPosWrapper.OWBPOSCOUNT = resultQty;

                                    var owbPosAddWrapper = new OWBPosWrapper
                                    {
                                        SKUID_R = existSkuDefault.SKUID,
                                        OWBPOSARTCODE = existSkuDefault.ArtCode,
                                        OWBPOSARTNAME = owbPosWrapper.OWBPOSARTNAME,
                                        OWBPOSCOUNT = 1,
                                        OWBPOSCOUNT2SKU = originalQty.To<double>() -
                                                          (resultQty.To<double>()*existSkuDefault.SKUCount),
                                        OWBPOSNUMBER = ++currentNum,
                                        OWBPOSBATCH = owbPosWrapper.OWBPOSBATCH,
                                        OWBPOSLOT = owbPosWrapper.OWBPOSLOT
                                    };
                                    newOwbPos.Add(owbPosAddWrapper);
                                }
                                else
                                {
                                    owbPosWrapper.SKUID_R = existSkuDefault.SKUID;
                                    owbPosWrapper.OWBPOSCOUNT2SKU = originalQty.To<double>() -
                                                                    (resultQty.To<double>()*existSkuDefault.SKUCount);
                                    owbPosWrapper.OWBPOSCOUNT = 1;
                                }
                            }
                        }
                    }
                    else
                    {
                        var originalQty = owbPosWrapper.OWBPOSCOUNT.To<decimal>();
                        owbPosWrapper.SKUID_R = existSkuDefault.SKUID;
                        owbPosWrapper.OWBPOSCOUNT2SKU = existSkuDefault.SKUCount;
                        owbPosWrapper.OWBPOSCOUNT = Math.Ceiling(originalQty/existSkuDefault.SKUCount.To<decimal>());
                    }
                }
                if (newOwbPos.Count > 0)
                    foreach (var newPos in newOwbPos)
                    {
                        item.OWBPOSL.Add(newPos);
                    }
            }
            if (MessageHelper.GetErrorCount(retMessage) > errCountOld)
                throw new IntegrationLogicalException("Ошибка при поиске и пересчете SKU (накладная «{0}»)",
                    item.OWBNAME);
        }

        private void OwbLoadInternal(SalesInvoiceWrapper item, List<ErrorWrapper> retMessage, IUnitOfWork uow)
        {
            var updateexistcpv = item != null && item.UPDATEEXISTCPV > 0;

            //Не для режима обновления CPV
            if (!updateexistcpv)
            {
                if ((!item.OWBCREATE_RECIPIENT.HasValue || item.OWBCREATE_RECIPIENT == 0) && !item.OWBRECIPIENT.HasValue)
                    throw new IntegrationLogicalException("Неизвестный код получателя «{0}»!",
                        (string.IsNullOrEmpty(item.OWBRECIPIENT_NAME)) ? item.OWBRECIPIENT_CODE : item.OWBRECIPIENT_NAME);

                if ((!item.OWBCREATE_PAYER.HasValue || item.OWBCREATE_PAYER == 0) && !item.OWBPAYER.HasValue)
                {
                    //throw new IntegrationLogicalException("Неизвестный код плательщика «{0}»!",
                    //    (string.IsNullOrEmpty(item.OWBPAYER_NAME)) ? item.OWBPAYER_CODE : item.OWBPAYER_NAME);
                    var message = string.Format("Неизвестный код плательщика «{0}»!",
                              string.IsNullOrEmpty(item.OWBPAYER_NAME)
                                  ? item.OWBPAYER_CODE
                                  : item.OWBPAYER_NAME);
                    Log.Debug(message);
                    var ew = new ErrorWrapper { ERRORCODE = MessageHelper.WarningCode.ToString(), ERRORMESSAGE = message };
                    retMessage.Add(ew);
                }
            }

            var nowyear = DateTime.Now.Year;
            using (var owbMgr = IoC.Instance.Resolve<IBaseManager<OWB>>())
            {
                if (uow != null)
                    owbMgr.SetUnitOfWork(uow);

                var isUpdate = false;
                var isIncludedGroup = false;
                var filter =
                    string.Format("{0} = '{1}' and {2} = {3}",
                        SourceNameHelper.Instance.GetPropertySourceName(typeof (OWB), OWB.OWBNAMEPropertyName),
                        item.OWBNAME,
                        SourceNameHelper.Instance.GetPropertySourceName(typeof (OWB), OWB.MANDANTIDPropertyName),
                        item.MANDANTID);
                decimal owbId = 0;

                var owbObjs = owbMgr.GetFiltered(filter, GetModeEnum.Partial).ToArray();

                OWB owbObj = null;
                if (owbObjs.Length == 1)
                {
                    owbObj = owbObjs[0];
                }
                else if (owbObjs.Length > 1)
                {
                    //Берем последнюю накладную
                    var maxdate = owbObjs.Max(p => p.DateIns);
                    if (maxdate.HasValue)
                    {
                        var owbmaxes = owbObjs.Where(p => p.DateIns == maxdate).ToArray();
                        if (owbmaxes.Length == 1)
                        {
                            owbObj = owbmaxes[0];
                        }
                        else
                        {
                            var maxid = owbmaxes.Max(p => p.OWBID);
                            owbObj = owbmaxes.Single(p => p.OWBID == maxid);
                        }
                    }
                    else
                    {
                        //Если какая-то беда с maxdate, то берем первый элемент
                        owbObj = owbObjs[0];
                    }
                }

                //Не для режима обновления CPV
                if (!updateexistcpv && item.OWBREPEAT == 1)
                {
                    //filter = string.Format("{0} = '{1}' and {2} = {3} and {4} <> 'OWB_CREATED'",
                    //    SourceNameHelper.Instance.GetPropertySourceName(typeof (OWB), OWB.OWBNAMEPropertyName),
                    //    item.OWBNAME,
                    //    SourceNameHelper.Instance.GetPropertySourceName(typeof (OWB), OWB.MANDANTIDPropertyName),
                    //    item.MANDANTID,
                    //    SourceNameHelper.Instance.GetPropertySourceName(typeof (OWB), OWB.StatusCodePropertyName));
                    //owbObj = owbMgr.GetFiltered(filter, GetModeEnum.Partial).FirstOrDefault();

                    //Если накладная существует
                    if (owbObj != null)
                    {
                        if (owbObj.StatusCode == OWBStates.OWB_CANCELED.ToString())
                        {
                            //Если накладная в статусе: отменена 
                            //Добавляем как новую накладную
                            owbObj = null;
                        }
                        else if (owbObj.StatusCode != OWBStates.OWB_CREATED.ToString()) //Если накладная не в статусе: создана, отменена
                        {
                            var throwexception = false;
                            if (item.OWBNAME.StartsWith("R"))
                            {
                                throwexception = item.OWBHOSTREFDATE.HasValue &&
                                    item.OWBHOSTREFDATE.Value.Year == nowyear;
                            }
                            else
                            {
                                throwexception = owbObj.DateIns.HasValue &&
                                    owbObj.DateIns.Value.Year == nowyear;
                            }

                            if (throwexception)
                                throw new IntegrationLogicalException(
                                    "Расходная накладная «{0}» с текущей датой (ГОД) существует.", item.OWBNAME);

                            //Добавляем как новую накладную
                            owbObj = null;
                        }
                    }
                }

                if (owbObj == null)
                {
                    //Накладная не существует
                    if (updateexistcpv)
                        throw new IntegrationLogicalException(
                            "Режим обновления параметров. Расходная накладная «{0}» не найдена.",
                            item.OWBNAME);
                }
                else
                {
                    //Накладная существует
                    owbId = owbObj.OWBID;
                    item.OWBID = owbId;
                    var owbCreatedInCurrentYear = owbObj.DateIns.HasValue &&
                        owbObj.DateIns.Value.Year == nowyear;
                    isUpdate = true;

                    //Не для режима обновления CPV
                    if (!updateexistcpv)
                    {
                        if (!item.OWBRECREATE.HasValue || item.OWBRECREATE == 0)
                        {
                            if (owbCreatedInCurrentYear)
                            {
                                throw new IntegrationLogicalException("Расходная накладная «{0}» существует.",
                                    item.OWBNAME);
                            }
                        }
                        else if (owbObj.StatusCode == OWBStates.OWB_CREATED.ToString())
                        {
                            var filterPosDel = string.Format("{0} = {1} and {2} = {3}",
                                SourceNameHelper.Instance.GetPropertySourceName(typeof (OWBPos),
                                    OWBPos.OWBID_RPropertyName), owbId,
                                SourceNameHelper.Instance.GetPropertySourceName(typeof (OWBPos),
                                    OWBPos.MANDANTIDPropertyName), item.MANDANTID);

                            using (var mgrPosDel = IoC.Instance.Resolve<IBaseManager<OWBPos>>())
                            {
                                mgrPosDel.SetUnitOfWork(uow);
                                var posDel = mgrPosDel.GetFiltered(filterPosDel, GetModeEnum.Partial).ToArray();
                                mgrPosDel.Delete(posDel);
                            }

                            DeleteCpvOwb(owbId, uow);
                        }
                        else 
                        {
                            throw new IntegrationLogicalException("Расходная накладная «{0}» в работе.",
                                item.OWBNAME);
                        }
                    }
                }

                #region owbUpdateHandler
                Action<OWB, IBaseManager<OWB>, bool> owbUpdateHandler = (owb, mngOwb, owbNeedeUpdate) =>
                {
                    if (owbNeedeUpdate)
                    {
                        ErrorWrapper ew;
                        if (updateexistcpv)
                        {
                            Log.InfoFormat("Обновлены параметры расходной накладной «{0}» (ID = {1})", item.OWBNAME, item.OWBID);
                            ew = new ErrorWrapper
                            {
                                ERRORCODE = MessageHelper.SuccessCode.ToString(),
                                ERRORMESSAGE = string.Format("Обновлены параметры расходной накладной «{0}»", item.OWBNAME)
                            };
                        }
                        else
                        {
                            SetXmlIgnore(owb, true);
                            owb.StatusCode = "OWB_CREATED";
                            owb.OWBID = owbId;
                            mngOwb.Update(owb);
                            Log.InfoFormat("Обновлена расходная накладная «{0}» (ID = {1})", item.OWBNAME, item.OWBID);
                            ew = new ErrorWrapper
                            {
                                ERRORCODE = MessageHelper.SuccessCode.ToString(),
                                ERRORMESSAGE = string.Format("Обновлена расходная накладная «{0}»", item.OWBNAME)
                            };
                        }
                        retMessage.Insert(0, ew);
                    }
                    else
                    {
                        SetXmlIgnore(owb, false);

                        var owbPosL = owb.OWBPosL == null ? null : owb.OWBPosL.ToArray();
                        if (owb.OWBPosL != null)
                            owb.OWBPosL.Clear();

                        mngOwb.Insert(ref owb);
                        item.OWBID = owb.OWBID;

                        //Сохраняем позиции и CPV для OWBPos
                        if (owbPosL != null)
                        {
                            var cpvhelper = new CpvHelper<OWBPosCpv>(ShipmentLoadHelper.Cpv2EntityOwbPosName, "-1");
                            using (var owbPosMgr = IoC.Instance.Resolve<IBaseManager<OWBPos>>())
                            {
                                if (uow != null)
                                    owbPosMgr.SetUnitOfWork(uow);
                                foreach (var pos in owbPosL)
                                {
                                    pos.OWBID_R = owb.OWBID;
                                    var poswms = pos;
                                    var cpvs = pos.CustomParamVal == null ? null : pos.CustomParamVal.ToArray();
                                    if (pos.CustomParamVal != null)
                                        pos.CustomParamVal.Clear();
                                    owbPosMgr.Insert(ref poswms);

                                    if (cpvs != null)
                                    {
                                        var posid = poswms.GetKey<decimal>();
                                        foreach (var cpv in cpvs)
                                        {
                                            cpv.CPVKey = posid.ToString(CultureInfo.InvariantCulture);
                                        }
                                        cpvhelper.Save(source: cpvs, allowUpdate: true, includeCpvWithDafaultValue: false, verify: false, uow: uow);
                                    }
                                }
                            }
                        }

                        if (owb.OWBPosL != null)
                        {
                            for (var i = 0; i < owb.OWBPosL.Count && i < item.OWBPOSL.Count; i++)
                            {
                                item.OWBPOSL[i].OWBPOSID = owb.OWBPosL[i].GetKey<decimal?>();
                            }
                        }

                        Log.InfoFormat("Загружена расходная накладная «{0}» (ID = {1})", item.OWBNAME, item.OWBID);
                        var ewr = new ErrorWrapper
                        {
                            ERRORCODE = MessageHelper.SuccessCode.ToString(),
                            ERRORMESSAGE = string.Format("Загружена расходная накладная «{0}»", item.OWBNAME)
                        };
                        retMessage.Insert(0, ewr);
                    }
                };
                #endregion owbUpdateHandler

                var addCpv = false;
                //Не для режима обновления CPV
                if (!updateexistcpv)
                { 
                    if (item.OWBCREATE_RECIPIENT == 1)
                    {
                        var messages = CreateOrUpdateRecipient(item, uow);
                        MessageHelper.AddMessage(messages, retMessage);
                    }

                    if (item.OWBCREATE_PAYER == 1)
                    {
                        var messages = CreateOrUpdatePayer(item, uow);
                        MessageHelper.AddMessage(messages, retMessage);
                    }

                    if (!string.IsNullOrEmpty(item.OWBFACTORY))
                        FillFactoryOwb(item, uow);

                    owbObj = owbMgr.New();
                    owbObj = MapTo(item, owbObj);

                    var currentNum = item.OWBCOUNTPOS.HasValue ? item.OWBCOUNTPOS.Value : 0;
                    if (item.OWBPOSL == null || item.OWBPOSL.Count == 0)
                        throw new IntegrationLogicalException("Нет позиций по накладной «{0}»", item.OWBNAME);

                    if (!string.IsNullOrEmpty(item.OWBPARTNERGROUP))
                    {
                        isIncludedGroup = FindPartnerGroupOwb(item.OWBPARTNERGROUP, item.OWBRECIPIENT);
                    }

                    if (item.OWBBOXRESERVE == 1)
                    {
                        var owbposL = ShipmentLoadHelper.OwbBoxReserve(isUpdate ? owbId : (decimal?) null, item);
                        if (owbposL == null || owbposL.Length == 0)
                            throw new IntegrationLogicalException(
                                "Неизвестная ошибка при обработке расходной накладной «{0}» в режиме OWBBOXRESERVE.",
                                item.OWBNAME);

                        if (isUpdate)
                        {
                            IEnumerable<OWBPos> owbposl = new List<OWBPos>(owbposL);
                            using (var owbPosMgr = IoC.Instance.Resolve<IBaseManager<OWBPos>>())
                            {
                                if (uow != null)
                                    owbPosMgr.SetUnitOfWork(uow);
                                owbPosMgr.Insert(ref owbposl);
                            }
                            Log.InfoFormat("Добавлены позиции ('{0}') в расходную накладную «{1}» (ID = {2})",
                                owbposL.Length, item.OWBNAME, owbId);
                        }
                        else
                        {
                            owbObj.OWBPosL = new WMSBusinessCollection<OWBPos>(owbposL);
                        }

                        owbUpdateHandler(owbObj, owbMgr, isUpdate);
                        return;
                    }

                    if (owbObj.OWBPosL == null)
                        owbObj.OWBPosL = new WMSBusinessCollection<OWBPos>();

                    FillArtOwb(item, retMessage, uow);
                    addCpv = FillArtGroupOwb(item, uow);

                    var artCodes = item.OWBPOSL.Select(i => i.OWBPOSARTCODE).Distinct();
                    // ??? а если две позиции с одинаковыми комплектами!!!

                    #region . CreateKitPos .

                    var mgrKit = IoC.Instance.Resolve<IBaseManager<Kit>>();
                    mgrKit.SetUnitOfWork(uow);
                    var kitFilterList =
                        FilterHelper.GetArrayFilterIn(SourceNameHelper.Instance.GetPropertySourceName(typeof (Kit),
                            Kit.ArtCodeRPropertyName), artCodes,
                            string.Format(" and {0} = {1}",
                                SourceNameHelper.Instance.GetPropertySourceName(typeof (Art),
                                    Kit.MANDANTIDPropertyName), item.MANDANTID));
                    foreach (var kitFilter in kitFilterList)
                    {
                        var kits = mgrKit.GetFiltered(kitFilter).ToArray();

                        if (kits.Length > 0 && item.OWBREPLACEKITS == 1)
                        {
                            foreach (var kit in kits)
                            {
                                using (var kpMgr = IoC.Instance.Resolve<IBaseManager<KitPos>>())
                                {
                                    var kitposFilter = string.Format("{0} = '{1}'",
                                        SourceNameHelper.Instance.GetPropertySourceName(typeof (KitPos),
                                            KitPos.KitPosCodeRPropertyName), kit.KitCode);
                                    var ktps = kpMgr.GetFiltered(kitposFilter).ToArray();

                                    if (ktps.Length == 0)
                                    {
                                        var ew = new ErrorWrapper
                                        {
                                            ERRORCODE = MessageHelper.ErrorCode.ToString(),
                                            ERRORMESSAGE = string.Format("У комплекта «{0}» нет комплектующих",
                                                kit.ArtCodeR)
                                        };
                                        retMessage.Add(ew);
                                        continue;
                                    }

                                    var firstOrDefault =
                                        item.OWBPOSL.FirstOrDefault(i => i.OWBPOSARTCODE.Equals(kit.ArtCodeR));

                                    if (firstOrDefault == null) continue;

                                    var kitCount = firstOrDefault.OWBPOSCOUNT;
                                    var kitNumber = firstOrDefault.OWBPOSNUMBER;

                                    var itemToRemove = item.OWBPOSL.SingleOrDefault(r => r.OWBPOSNUMBER == kitNumber);
                                    item.OWBPOSL.Remove(itemToRemove);

                                    foreach (var pos in ktps.Select(kitpos => new OWBPos
                                    {
                                        SKUID = kitpos.KitPosSKUIDR.To<decimal>(),
                                        OWBPosCount = kitCount.To<decimal>()*kitpos.KitPosCount,
                                        OWBPosArtName = (kitpos.KitPosCodeR.StartsWith("KIT"))
                                            ? kitpos.KitPosCodeR.Substring(item.MandantCode.Length + 3)
                                            : kitpos.KitPosCodeR.Substring(item.MandantCode.Length),
                                        OWBPosNumber = ++currentNum,
                                        MandantID = item.MANDANTID
                                    }))
                                    {
                                        owbObj.OWBPosL.Add(pos);
                                    }
                                }
                            }
                        }
                    }

                    #endregion . CreateKitPos .

                    if (item.OWBCONVERTSKU == 0 && !isIncludedGroup)
                        FillSkuforOwb(item, artCodes, retMessage, uow);
                    else
                        ConvertSku2SkuOwb(item, artCodes, currentNum, isIncludedGroup, retMessage, uow);

                    foreach (var pos in item.OWBPOSL)
                    {
                        if (pos.CHECKBARCODE.HasValue && pos.CHECKBARCODE.Value > 0)
                        {
                            if (!BarcodeHelper.ValidateBarcode(pos.SKUID_R, pos.BARCODE, uow))
                            {
                                //Проверка ШК
                                var exmessage = string.Format(BarcodeHelper.CheckBarcodeMessage +
                                                                ". ШК '{0}' не найден для позиции расходной накладной '{1}' (артикул '{2}').",
                                    pos.BARCODE, item.OWBNAME, pos.OWBPOSARTNAME);
                                var errorWrapper = new ErrorWrapper
                                {
                                    ERRORCODE = MessageHelper.WarningCode.ToString(),
                                    ERRORMESSAGE = exmessage
                                };

                                switch (pos.CHECKBARCODE.Value)
                                {
                                    case 2: //warning
                                        retMessage.Add(errorWrapper);
                                        break;
                                    case 3:
                                        if (string.IsNullOrEmpty(pos.BARCODE))
                                        {
                                            retMessage.Add(errorWrapper);
                                            break;
                                        }
                                        throw new IntegrationLogicalException(exmessage);
                                    default: //error
                                        throw new IntegrationLogicalException(exmessage);
                                }
                            }
                        }

                        var p = new OWBPos();
                        if (!string.IsNullOrEmpty(item.OWBFACTORY))
                            pos.FACTORYID_R = item.FACTORYID_R.To<decimal>();

                        MapTo(pos, p);
                        SetXmlIgnore(p, true);

                        if (isUpdate)
                        {
                            using (var owbPosMgr = IoC.Instance.Resolve<IBaseManager<OWBPos>>())
                            {
                                owbPosMgr.SetUnitOfWork(uow);
                                p.OWBID_R = owbId;
                                owbPosMgr.Insert(ref p);
                            }
                            Log.InfoFormat("Добавлена позиция в расходную накладную «{0}» (ID = {1})", item.OWBNAME,
                                p.OWBPosID);
                            pos.OWBPOSID = p.GetKey<decimal>();

                            //Загружаем CPV для OWBPos
                            var errposmessages = LoadOwbPosCpv(pos, item.OWBNAME, uow);
                            MessageHelper.AddMessage(errposmessages, retMessage);
                        }
                        else
                        {
                            List<string> messageL;
                            var wmscpvs = CreateOwbPosCpv(pos, item.OWBNAME, uow, out messageL);
                            MessageHelper.AddMessage(messageL, retMessage);
                            if (wmscpvs != null && wmscpvs.Any())
                            {
                                if (p.CustomParamVal == null)
                                    p.CustomParamVal = new WMSBusinessCollection<OWBPosCpv>();
                                p.CustomParamVal.AddRange(wmscpvs);
                            }
                            owbObj.OWBPosL.Add(p);
                        }

                        LoadBarcodesOwb(pos, retMessage, uow);
                    }

                    //handle EcomClient information
                    if (item.OWBCLIENTRECIPIENT != null)
                    {
                        owbObj.OWBClientRecipient = UpdateOrCreateEcomClient(item.OWBCLIENTRECIPIENT, item.MANDANTID,
                            retMessage);

                        var addressBook = new AddressBook();
                        var recipientAddress =
                            item.Address.FirstOrDefault(
                                i => i.ADDRESSBOOKTYPECODE == AddressBookType.ADR_CLIENTRECIPIENT);

                        if (recipientAddress != null)
                        {
                            if (owbObj.OWBClientRecipient != null)
                            {
                                using (var ecomClientMgr = IoC.Instance.Resolve<IBaseManager<EcomClient>>())
                                {
                                    recipientAddress.ADDRESSBOOKTYPECODE = AddressBookType.ADR_PHYSICAL;
                                    var ecomAdrs = ecomClientMgr.Get(owbObj.OWBClientRecipient);
                                    if (ecomAdrs != null)
                                    {
                                        addressBook = MapTo(recipientAddress, addressBook);
                                        var existRecipAddr = AddressHelper.FindAddressInCollection(ecomAdrs.Address, addressBook);

                                        if (existRecipAddr == null)
                                        {
                                            addressBook.ADDRESSBOOKTYPECODE = AddressBookType.ADR_LEGAL.ToString();
                                            existRecipAddr = AddressHelper.FindAddressInCollection(ecomAdrs.Address, addressBook);
                                        }

                                        if (existRecipAddr != null && existRecipAddr.ADDRESSBOOKID != null)
                                            owbObj.OWBClientRecipientAddr = existRecipAddr.ADDRESSBOOKID;
                                    }
                                }
                            }
                        }
                    }

                    if (item.OWBCLIENTPAYER != null)
                    {
                        owbObj.OWBClientPayer = UpdateOrCreateEcomClient(item.OWBCLIENTPAYER, item.MANDANTID, retMessage);
                    }

                    owbUpdateHandler(owbObj, owbMgr, isUpdate);
                    LoadTransitOwb(item, retMessage, uow);

                    if (item.OWBPOSL.Count(i => i.TRANSITDATAL != null) > 0)
                        LoadTransitOwbPos(item, retMessage, uow);
                }

                //Загружаем CPV для OWB
                var errmessages = LoadCpvOwb(item: item, addCpv: addCpv, uow: uow);
                MessageHelper.AddMessage(errmessages, retMessage);

                if (updateexistcpv) //Режим обновления CPV
                    owbUpdateHandler(owbObj, owbMgr, true);
            }
        }

        private decimal UpdateOrCreateEcomClient(EcomClientWrapper ecomClientWrapper, decimal? mandantId, List<ErrorWrapper> messages)
        {
            var excount = 0;
            using (var mgrEcomClient = IoC.Instance.Resolve<IBaseManager<EcomClient>>())
            {
                var ecomType = typeof (EcomClient);
                var ecomFilter = string.IsNullOrEmpty(ecomClientWrapper.ClientHostRef)
                    ? string.Format(
                        "Upper({0}) = Upper('{1}') and Upper({2}) = upper('{3}') and Upper({4}) = upper('{5}')",
                        SourceNameHelper.Instance.GetPropertySourceName(ecomType, EcomClient.ClientlastnamePropertyName),
                        ecomClientWrapper.ClientLastName,
                        SourceNameHelper.Instance.GetPropertySourceName(ecomType,
                            EcomClient.ClientmiddlenamePropertyName), ecomClientWrapper.ClientMiddleName,
                        SourceNameHelper.Instance.GetPropertySourceName(ecomType, EcomClient.ClientnamePropertyName),
                        ecomClientWrapper.ClientName)
                    : string.Format("{0} = '{1}'",
                        SourceNameHelper.Instance.GetPropertySourceName(ecomType, EcomClient.ClienthostrefPropertyName),
                        ecomClientWrapper.ClientHostRef);

                var ecomClient = mgrEcomClient.GetFiltered(ecomFilter).FirstOrDefault();
            oexlabel:
                if (ecomClient != null)
                {
                    //map wrapper to existEcomClient
                    ecomClient = MapTo(ecomClientWrapper, ecomClient);

                    //update address for EcomClient
                    FillClientAdress(ecomClient, ecomClientWrapper.AddressL);
                    if (ecomClient.IsDirty)
                    {
                        try
                        {
                            mgrEcomClient.Update(ecomClient);
                        }
                        catch (DALCustomException dcex)
                        {
                            var ecomClientKey = ecomClient.GetKey<decimal>();
                            MessageHelper.AddMessage(
                                new[]
                                {
                                    string.Format("Ошибка при обновлении E-commerce клиента (ID = '{0}'). {1}",
                                        ecomClientKey, dcex.Message)
                                }, messages);
                            var oex = dcex.InnerException as OracleException;
                            if (oex != null && oex.Number == 20987 && excount < 3) //ORA-20987: Объект изменен пользователем
                            {
                                excount++;
                                ecomClient = mgrEcomClient.Get(ecomClientKey, GetModeEnum.Partial);
                                if (ecomClient != null)
                                    goto oexlabel;
                            }
                            throw;
                        }
                    }
                }
                else
                {
                    //create new EcomClient
                    ecomClient = mgrEcomClient.New();
                    ecomClient = MapTo(ecomClientWrapper, ecomClient);
                    if (mandantId != null) 
                        ecomClient.MandantID = mandantId.Value;
                    FillClientAdress(ecomClient, ecomClientWrapper.AddressL);
                    mgrEcomClient.Insert(ref ecomClient);
                }

                return ecomClient.ClientID;
            }
        }

        private void FillClientAdress(EcomClient ecomClient, List<AddressBookWrapper> ecomClientAddressBooks)
        {
            if (ecomClientAddressBooks == null) 
                return;

            foreach (var addressBookWrapper in ecomClientAddressBooks)
            {
                var addressBook = new AddressBook();
                addressBook = MapTo(addressBookWrapper, addressBook);
                AddressBook clientAddrExists = null;

                if (ecomClient.Address != null)
                    clientAddrExists = AddressHelper.FindAddressInCollection(ecomClient.Address, addressBook);

                //if address already exists in db
                if (clientAddrExists != null)
                    continue;

                if (ecomClient.Address == null)
                    ecomClient.Address = new WMSBusinessCollection<AddressBook>();

                //Add new address only
                ecomClient.Address.Add(addressBook);
            }
        }

        private string[] CreateOrUpdateRecipient(SalesInvoiceWrapper item, IUnitOfWork uow = null)
        {
            var messages = new List<string>();
            var donotupdatelegaladdress = item.DONOTUPDATELEGALADDRESS > 0;

            //Получим не пустые адреса, исключая ADR_CLIENTRECIPIENT
            var addresstypes = AddressHelper.GetAddressTypes(AddressBookType.ADR_CLIENTRECIPIENT);
            var wrapperaddrs = AddressHelper.GetNotEmptyAddressBookByTypes(item.Address, addresstypes);

            //Преобразуем в wms-адреса
            var wmsaddresscol = new List<AddressBook>();
            if (wrapperaddrs != null && wrapperaddrs.Length > 0)
            {
                foreach (var ad in wrapperaddrs)
                {
                    var wmsadr = new AddressBook();
                    MapTo(ad, wmsadr);
                    if (!AddressHelper.IsAddressEmpty(wmsadr))
                        wmsaddresscol.Add(wmsadr);
                }
            }

            Partner wmspartner;

            if (item.OWBRECIPIENT.HasValue)
            {
                #region Получатель (партнер) существует
                //Если получатель (партнер) существует - берем его из БД
                var excount = 0;
                using (var partMgr = IoC.Instance.Resolve<IBaseManager<Partner>>())
                {
                    if (uow != null)
                        partMgr.SetUnitOfWork(uow);

                    Log.DebugFormat("ID получателя = '{0}'", item.OWBRECIPIENT);

                oexlabel:
                    wmspartner = partMgr.Get(item.OWBRECIPIENT);

                    //Добавляем адрес
                    AddOrUpdatePartnerAddress(wmspartner, wmsaddresscol, donotupdatelegaladdress);

                    //Если были изменения - сохраняем
                    if (wmspartner.IsDirty)
                    {
                        SetXmlIgnore(wmspartner, true);

                        try
                        {
                            partMgr.Update(wmspartner);
                            wmspartner = partMgr.Get(item.OWBRECIPIENT);
                        }
                        catch (DALCustomException dcex)
                        {
                            messages.Add(string.Format("Ошибка при обновлении получателя (ID = '{0}'). {1}",
                                item.OWBRECIPIENT, dcex.Message));
                            var oex = dcex.InnerException as OracleException;
                            if (oex != null && oex.Number == 20987 && excount < 3) //ORA-20987: Объект изменен пользователем
                            {
                                excount++;
                                goto oexlabel;
                            }
                            throw;
                        }
                    }
                }

                var typePartnerGpv = typeof(PartnerGpv);
                var gpvFilter = string.Format("{0} = '{1}' and {2} = 'PARTNER' and {3} = 'IsRecipient'",
                    SourceNameHelper.Instance.GetPropertySourceName(typePartnerGpv,
                        PartnerGpv.GParamValKeyPropertyName), item.OWBRECIPIENT,
                    SourceNameHelper.Instance.GetPropertySourceName(typePartnerGpv,
                        PartnerGpv.GParamVal2EntityPropertyName),
                    SourceNameHelper.Instance.GetPropertySourceName(typePartnerGpv,
                        PartnerGpv.GlobalParamCodePropertyName));

                using (var gpvMgr = IoC.Instance.Resolve<IBaseManager<PartnerGpv>>())
                {
                    gpvMgr.SetUnitOfWork(uow);
                    var gpvs = gpvMgr.GetFiltered(gpvFilter).ToArray();

                    if (!gpvs.Any())
                    {
                        var gpv = new PartnerGpv
                        {
                            GlobalParamCode_R = "IsRecipient",
                            GparamValValue = "1",
                            GParamVal2Entity = "PARTNER",
                            GParamValKey = item.OWBRECIPIENT.Value
                        };
                        gpvMgr.Insert(ref gpv);
                        Log.InfoFormat("Загружена GPV (ID = {0})", gpv.GetKey());
                    }

                    if (gpvs.Length == 1)
                    {
                        var gpvExist = gpvs[0];
                        if (!string.IsNullOrEmpty(gpvExist.GparamValValue) && gpvExist.GparamValValue != "1")
                        {
                            gpvExist.GparamValValue = "1";
                            gpvMgr.Update(gpvExist);
                        }
                    }
                }
                #endregion Получатель (партнер) существует
            }
            else
            {
                #region Получатель (партнер) не существует
                //Если получателя (партнера) не существует - добавляем
                wmspartner = new Partner
                {
                    PartnerName = item.OWBRECIPIENT_NAME,
                    PartnerFullName = item.OWBRECIPIENT_NAME,
                    PartnerHostRef = item.OWBRECIPIENT_CODE,
                    PartnerINN = item.OWBRECIPIENT_INN,
                    PartnerOKPO = item.OWBRECIPIENT_OKPO,
                    PartnerPhone = item.OWBRECIPIENT_PHONE
                };

                if (!wmspartner.MandantId.HasValue) 
                    wmspartner.MandantId = item.MANDANTID;

                //Добавляем адрес
                AddOrUpdatePartnerAddress(wmspartner, wmsaddresscol, donotupdatelegaladdress);

                //Валидация партнера
                if (string.IsNullOrEmpty(wmspartner.PartnerName) && wmspartner.Address != null && wmspartner.Address.Count > 0)
                {
                    var address = wmspartner.Address[0];
                    var name = address.ADDRESSBOOKRAW;
                    if (string.IsNullOrEmpty(name))
                    {
                        name = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}", address.ADDRESSBOOKCOUNTRY,
                            address.ADDRESSBOOKINDEX, address.ADDRESSBOOKREGION,
                            address.ADDRESSBOOKCITY, address.ADDRESSBOOKDISTRICT, address.ADDRESSBOOKSTREET,
                            address.ADDRESSBOOKBUILDING,
                            address.ADDRESSBOOKAPARTMENT);
                    }
                    if (!string.IsNullOrEmpty(name))
                    {
                        name = name.Trim();
                        if (name.Length > 0)
                        {
                            name = name.Replace(" ", string.Empty);
                            if (name.Length > PartnerNameFieldMaxLength)
                                name = name.Substring(0, PartnerNameFieldMaxLength);

                            if (string.IsNullOrEmpty(wmspartner.PartnerName))
                                wmspartner.PartnerName = name;
                            if (string.IsNullOrEmpty(wmspartner.PartnerFullName))
                                wmspartner.PartnerFullName = name;
                        }
                    }
                }

                if (string.IsNullOrEmpty(wmspartner.PartnerName))
                {
                    const string message = "Получатель не создан. Наименование получателя не может быть пустым.";
                    messages.Add(message);
                    Log.Info(message);
                    return messages.ToArray();
                }

                using (var partMgr = IoC.Instance.Resolve<IBaseManager<Partner>>())
                {
                    if (uow != null)
                        partMgr.SetUnitOfWork(uow);
                    SetXmlIgnore(wmspartner, false);
                    partMgr.Insert(ref wmspartner);
                    Log.DebugFormat("Партнер «{0}» загружен (ID = {1})",
                        string.IsNullOrEmpty(item.OWBRECIPIENT_NAME) ? item.OWBRECIPIENT_CODE : item.OWBRECIPIENT_NAME,
                        wmspartner.PartnerId);
                    item.OWBRECIPIENT = wmspartner.PartnerId;

                    using (var gpvMgr = IoC.Instance.Resolve<IBaseManager<PartnerGpv>>())
                    {
                        gpvMgr.SetUnitOfWork(uow);
                        var gpv = new PartnerGpv
                        {
                            GparamValValue = "1",
                            GlobalParamCode_R = "IsRecipient",
                            GParamVal2Entity = "PARTNER",
                            GParamValKey = wmspartner.GetKey<decimal>()
                        };
                        gpvMgr.Insert(ref gpv);
                        Log.InfoFormat("Загружена GPV (ID = {0})", gpv.GetKey());
                    }
                }
                #endregion Получатель (партнер) не существует
            }

            //И снова адрес, но для item.ADDRESSBOOKID_R
            if (wmspartner.Address != null)
            {
                if (wmsaddresscol.Count > 0)
                {
                    // Пытаемся найти переданный физ. или юр. адреса
                    var adrcollection = new List<AddressBook>();
                    foreach (var code in new[] {AddressBookType.ADR_PHYSICAL.ToString(), AddressBookType.ADR_LEGAL.ToString()})
                    {
                        var adr = wmsaddresscol.FirstOrDefault(p => p.ADDRESSBOOKTYPECODE == code);
                        if (adr != null)
                            adrcollection.Add(adr);
                    }

                    foreach (var adr in adrcollection)
                    {
                        var findadr = AddressHelper.FindAddressInCollection(wmspartner.Address, adr);
                        if (findadr != null)
                        {
                            item.ADDRESSBOOKID_R = findadr.GetKey<decimal?>();
                            if (item.ADDRESSBOOKID_R.HasValue)
                                break;
                        }
                    }
                }

                if (!item.ADDRESSBOOKID_R.HasValue)
                {
                    var notemptyadrs = wmspartner.Address.Where(p => !AddressHelper.IsAddressEmpty(p)).ToArray();
                    //Берем макс. ид. непустого физ. адреса
                    var existsadr = AddressHelper.GetAddressWithMaxIdByType(notemptyadrs, AddressBookType.ADR_PHYSICAL.ToString());
                    if (existsadr != null)
                        item.ADDRESSBOOKID_R = existsadr.GetKey<decimal?>();

                    if (!item.ADDRESSBOOKID_R.HasValue) //Берем макс. ид. непустого юр. адреса
                    {
                        existsadr = AddressHelper.GetAddressWithMaxIdByType(notemptyadrs,
                            AddressBookType.ADR_LEGAL.ToString());
                        if (existsadr != null)
                            item.ADDRESSBOOKID_R = existsadr.GetKey<decimal?>();
                    }
                }
            }

            return messages.ToArray();
        }

        private string[] CreateOrUpdatePayer(SalesInvoiceWrapper item, IUnitOfWork uow = null)
        {
            var messages = new List<string>();
            Partner wmspartner;

            if (item.OWBPAYER.HasValue)
            {
                #region Плательщик (партнер) существует
                //Если плательщик (партнер) существует - берем его из БД
                var excount = 0;
                using (var partMgr = IoC.Instance.Resolve<IBaseManager<Partner>>())
                {
                    if (uow != null)
                        partMgr.SetUnitOfWork(uow);

                    Log.DebugFormat("ID плательщика = '{0}'", item.OWBPAYER);

                    oexlabel:
                    wmspartner = partMgr.Get(item.OWBPAYER);

                    //Если были изменения - сохраняем
                    if (wmspartner.IsDirty)
                    {
                        SetXmlIgnore(wmspartner, true);

                        try
                        {
                            partMgr.Update(wmspartner);
                        }
                        catch (DALCustomException dcex)
                        {
                            messages.Add(string.Format("Ошибка при обновлении плательщика (ID = '{0}'). {1}",
                                item.OWBPAYER, dcex.Message));
                            var oex = dcex.InnerException as OracleException;
                            if (oex != null && oex.Number == 20987 && excount < 3) //ORA-20987: Объект изменен пользователем
                            {
                                excount++;
                                goto oexlabel;
                            }
                            throw;
                        }
                    }
                }

                var typePartnerGpv = typeof (PartnerGpv);
                var gpvFilter = string.Format("{0} = '{1}' and {2} = 'PARTNER' and {3} = 'IsPayer'",
                    SourceNameHelper.Instance.GetPropertySourceName(typePartnerGpv,
                        PartnerGpv.GParamValKeyPropertyName), item.OWBPAYER,
                    SourceNameHelper.Instance.GetPropertySourceName(typePartnerGpv,
                        PartnerGpv.GParamVal2EntityPropertyName),
                    SourceNameHelper.Instance.GetPropertySourceName(typePartnerGpv,
                        PartnerGpv.GlobalParamCodePropertyName));

                using (var gpvMgr = IoC.Instance.Resolve<IBaseManager<PartnerGpv>>())
                {
                    gpvMgr.SetUnitOfWork(uow);
                    var gpvs = gpvMgr.GetFiltered(gpvFilter).ToArray();

                    if (!gpvs.Any())
                    {
                        var gpv = new PartnerGpv
                        {
                            GlobalParamCode_R = "IsPayer",
                            GparamValValue = "1",
                            GParamVal2Entity = "PARTNER",
                            GParamValKey = item.OWBPAYER.Value
                        };
                        gpvMgr.Insert(ref gpv);
                        Log.InfoFormat("Загружена GPV (ID = {0})", gpv.GetKey());
                    }

                    if (gpvs.Length == 1)
                    {
                        var gpvExist = gpvs[0];
                        if (!string.IsNullOrEmpty(gpvExist.GparamValValue) && gpvExist.GparamValValue != "1")
                        {
                            gpvExist.GparamValValue = "1";
                            gpvMgr.Update(gpvExist);
                        }
                    }
                }
                #endregion Плательщик (партнер) существует
            }
            else
            {
                #region Плательщик (партнер) не существует
                //Если плательщика (партнера) не существует - добавляем
                wmspartner = new Partner
                {
                    PartnerName = item.OWBPAYER_NAME,
                    PartnerFullName = item.OWBPAYER_NAME,
                    PartnerHostRef = item.OWBPAYER_CODE,
                    PartnerINN = item.OWBPAYER_INN,
                    PartnerOKPO = item.OWBPAYER_OKPO,
                    PartnerPhone = item.OWBPAYER_PHONE
                };

                if (!wmspartner.MandantId.HasValue)
                    wmspartner.MandantId = item.MANDANTID;

                //Валидация партнера
                if (string.IsNullOrEmpty(wmspartner.PartnerName) && wmspartner.Address != null && wmspartner.Address.Count > 0)
                {
                    var address = wmspartner.Address[0];
                    var name = address.ADDRESSBOOKRAW;
                    if (string.IsNullOrEmpty(name))
                    {
                        name = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}", address.ADDRESSBOOKCOUNTRY,
                            address.ADDRESSBOOKINDEX, address.ADDRESSBOOKREGION,
                            address.ADDRESSBOOKCITY, address.ADDRESSBOOKDISTRICT, address.ADDRESSBOOKSTREET,
                            address.ADDRESSBOOKBUILDING,
                            address.ADDRESSBOOKAPARTMENT);
                    }
                    if (!string.IsNullOrEmpty(name))
                    {
                        name = name.Trim();
                        if (name.Length > 0)
                        {
                            name = name.Replace(" ", string.Empty);
                            if (name.Length > PartnerNameFieldMaxLength)
                                name = name.Substring(0, PartnerNameFieldMaxLength);

                            if (string.IsNullOrEmpty(wmspartner.PartnerName))
                                wmspartner.PartnerName = name;
                            if (string.IsNullOrEmpty(wmspartner.PartnerFullName))
                                wmspartner.PartnerFullName = name;
                        }
                    }
                }

                if (string.IsNullOrEmpty(wmspartner.PartnerName))
                {
                    const string message = "Плательщик не создан. Наименование плательщика не может быть пустым.";
                    messages.Add(message);
                    Log.Info(message);
                    return messages.ToArray();
                }

                using (var partMgr = IoC.Instance.Resolve<IBaseManager<Partner>>())
                {
                    if (uow != null)
                        partMgr.SetUnitOfWork(uow);
                    SetXmlIgnore(wmspartner, false);
                    partMgr.Insert(ref wmspartner);
                    Log.DebugFormat("Партнер «{0}» загружен (ID = {1})",
                        string.IsNullOrEmpty(item.OWBRECIPIENT_NAME) ? item.OWBRECIPIENT_CODE : item.OWBRECIPIENT_NAME,
                        wmspartner.PartnerId);
                    item.OWBPAYER = wmspartner.PartnerId;

                    using (var gpvMgr = IoC.Instance.Resolve<IBaseManager<PartnerGpv>>())
                    {
                        gpvMgr.SetUnitOfWork(uow);
                        var gpv = new PartnerGpv
                        {
                            GparamValValue = "1",
                            GlobalParamCode_R = "IsPayer",
                            GParamVal2Entity = "PARTNER",
                            GParamValKey = wmspartner.GetKey<decimal>()
                        };
                        gpvMgr.Insert(ref gpv);
                        Log.InfoFormat("Загружена GPV (ID = {0})", gpv.GetKey());
                    }
                }
                #endregion Плательщик (партнер) не существует
            }

            return messages.ToArray();
        }

        private void RoutingOwb(decimal owbId, IUnitOfWork uow)
        {
            using (var mgrBpProc = IoC.Instance.Resolve<BPProcessManager>())
            {
                if (uow != null)
                    mgrBpProc.SetUnitOfWork(uow);

                var listOwb = new List<decimal> {owbId};
                if (mgrBpProc == null)
                    return;
                var retListStrings = mgrBpProc.ChangeOWBRoute(listOwb, null);

                foreach (var mess in retListStrings)
                {
                    Log.DebugFormat("Результат маршрутизации накладной (ID = {0}): {1}", owbId, mess);
                }
            }
        }

        private void LoadTransitOwb(SalesInvoiceWrapper item, List<ErrorWrapper> retMessage, IUnitOfWork uow)
        {
            if (item.TRANSITDATAL == null || item.TRANSITDATAL.Count == 0)
                return;

            var transitNames = item.TRANSITDATAL.Select(i => i.TRANSITNAME.ToUpper()).Distinct();
            var transitFilterValue = "'" + string.Join("','", transitNames) + "'";
            var transitFilter = string.Format("{0} in ({1}) and {2} = {3} and {4} = 'OWB'",
                SourceNameHelper.Instance.GetPropertySourceName(typeof (Transit), Transit.TransitNamePropertyName),
                transitFilterValue,
                Transit.MANDANTIDPropertyName, item.MANDANTID,
                SourceNameHelper.Instance.GetPropertySourceName(typeof (Transit), Transit.TransitEntityPropertyName));

            using (var transitMgr = IoC.Instance.Resolve<IBaseManager<Transit>>())
            {
                if (uow != null)
                    transitMgr.SetUnitOfWork(uow);

                var transites = transitMgr.GetFiltered(transitFilter).ToArray();

                foreach (var tdw in item.TRANSITDATAL)
                {
                    var existTrn = transites.FirstOrDefault(i => tdw.TRANSITNAME.Equals(i.TransitName));
                    if (existTrn == null)
                    {
                        var ew = new ErrorWrapper
                        {
                            ERRORCODE = MessageHelper.ErrorCode.ToString(),
                            ERRORMESSAGE =
                                string.Format("Не существует заголовок транзита «{0}»", tdw.TRANSITNAME)
                        };
                        retMessage.Add(ew);
                    }
                    else
                    {
                        var trDataMgr = IoC.Instance.Resolve<IBaseManager<TransitData>>();
                        trDataMgr.SetUnitOfWork(uow);
                        var trDataObj = new TransitData
                        {
                            TransitDataKey = item.OWBID.ToString(),
                            TransitDataValue = tdw.TRANSITDATAVALUE,
                            TransitID = existTrn.TransitID
                        };
                        SetXmlIgnore(trDataObj, false);
                        trDataMgr.Insert(ref trDataObj);
                        Log.InfoFormat("Созданы транзитные данные накладной: «{0}» = «{1}»", tdw.TRANSITNAME,
                            tdw.TRANSITDATAVALUE);
                    }
                }
                if (retMessage.Any(mess => mess.ERRORCODE.Equals(MessageHelper.ErrorCode.ToString())))
                    throw new IntegrationLogicalException("Ошибка при создании транзитных данных накладной «{0}»",
                        item.OWBNAME);
            }
        }

        private void LoadTransitOwbPos(SalesInvoiceWrapper item, List<ErrorWrapper> retMessage, IUnitOfWork uow)
        {
            var owbPosFilter = string.Format("{0} = {1}",
                SourceNameHelper.Instance.GetPropertySourceName(typeof (OWBPos), OWBPos.OWBID_RPropertyName), item.OWBID);
            var poses = new List<OWBPos>();
            using (var posMgr = IoC.Instance.Resolve<IBaseManager<OWBPos>>())
            {
                if (uow != null)
                    posMgr.SetUnitOfWork(uow);
                poses.AddRange(posMgr.GetFiltered(owbPosFilter).ToArray());
            }

            var transitInPos = item.OWBPOSL.Where(i => i.TRANSITDATAL != null).ToArray();
            var transitNames = new List<string>();
            foreach (var tdWr in transitInPos.SelectMany(posWithTransit => posWithTransit.TRANSITDATAL.Where
                (tdWr => !transitNames.Contains(tdWr.TRANSITNAME))))
            {
                transitNames.Add(tdWr.TRANSITNAME);
            }

            var transitPosFilterValue = "'" + string.Join("','", transitNames) + "'";
            var transitPosFilter = string.Format("{0} in ({1}) and {2} = {3} and {4} = 'OWBPOS'",
                SourceNameHelper.Instance.GetPropertySourceName(typeof (Transit), Transit.TransitNamePropertyName),
                transitPosFilterValue,
                Transit.MANDANTIDPropertyName, item.MANDANTID,
                SourceNameHelper.Instance.GetPropertySourceName(typeof (Transit), Transit.TransitEntityPropertyName));

            using (var transitPosMgr = IoC.Instance.Resolve<IBaseManager<Transit>>())
            {
                if (uow != null)
                    transitPosMgr.SetUnitOfWork(uow);

                var transites = transitPosMgr.GetFiltered(transitPosFilter).ToArray();
                foreach (var trInPos in transitInPos)
                {
                    foreach (var tdw in trInPos.TRANSITDATAL)
                    {
                        var existTrn = transites.FirstOrDefault(i => tdw.TRANSITNAME.Equals(i.TransitName));
                        if (existTrn == null)
                        {
                            var ew = new ErrorWrapper
                            {
                                ERRORCODE = MessageHelper.ErrorCode.ToString(),
                                ERRORMESSAGE =
                                    string.Format("Не существует заголовок транзита «{0}»", tdw.TRANSITNAME)
                            };
                            retMessage.Add(ew);
                        }
                        else
                        {
                            tdw.TRANSITID = existTrn.TransitID;
                        }
                    }
                }
            }
            if (retMessage.Any(mess => mess.ERRORCODE.Equals(MessageHelper.ErrorCode.ToString())))
                throw new IntegrationLogicalException("Ошибка при создании транзитных данных позиций накладной «{0}»",
                    item.OWBNAME);

            foreach (var wrPos in item.OWBPOSL)
            {
                var firstOrDefault = poses.FirstOrDefault(i => i.OWBPosNumber.Equals(wrPos.OWBPOSNUMBER));
                if (firstOrDefault != null)
                {
                    var posId = firstOrDefault.OWBPosID;

                    if (wrPos.TRANSITDATAL == null) continue;

                    foreach (var wrTdata in wrPos.TRANSITDATAL)
                    {
                        var trDataMgr = IoC.Instance.Resolve<IBaseManager<TransitData>>();
                        trDataMgr.SetUnitOfWork(uow);

                        if (!String.IsNullOrEmpty(wrTdata.TRANSITDATAVALUE) && wrTdata.TRANSITDATAVALUE != "")
                        {
                            var trDataObj = new TransitData
                            {
                                TransitDataKey = posId.ToString(),
                                TransitDataValue = wrTdata.TRANSITDATAVALUE,
                                TransitID = wrTdata.TRANSITID
                            };
                            SetXmlIgnore(trDataObj, false);
                            trDataMgr.Insert(ref trDataObj);
                            Log.InfoFormat("Созданы транзитные данные {0} позиции: «{1}» = «{2}»",
                                wrPos.OWBPOSNUMBER, wrTdata.TRANSITNAME, wrTdata.TRANSITDATAVALUE);
                        }
                    }
                }
            }
        }

        private bool FindPartnerGroupOwb(string groupName, decimal? partnerId)
        {
            var gpFilter = string.Format("{0} = '{1}'",
                SourceNameHelper.Instance.GetPropertySourceName(typeof (PartnerGroup),
                    PartnerGroup.PARTNERGROUPNAMEPropertyName),
                groupName);
            var gpMgr = IoC.Instance.Resolve<IBaseManager<PartnerGroup>>();
            var groupPartner = gpMgr.GetFiltered(gpFilter).FirstOrDefault();
            if (groupPartner == null)
                throw new IntegrationLogicalException("Не существует группа компаний «{0}»", groupName);

            var g2PFilter = string.Format("{0} = {1} and {2} = {3}",
                SourceNameHelper.Instance.GetPropertySourceName(typeof (Partner2Group),
                    Partner2Group.PARTNERGROUPIDPropertyName),
                groupPartner.PartnerGroupId,
                SourceNameHelper.Instance.GetPropertySourceName(typeof (Partner2Group),
                    Partner2Group.PARTNERIDPropertyName),
                partnerId);
            var g2PMgr = IoC.Instance.Resolve<IBaseManager<Partner2Group>>();
            var group2Partner = g2PMgr.GetFiltered(g2PFilter).FirstOrDefault();
            if (group2Partner == null)
                return false;

            Log.InfoFormat("Партнер привязан к группе «{0}»", groupName);
            return true;
        }
    }
}