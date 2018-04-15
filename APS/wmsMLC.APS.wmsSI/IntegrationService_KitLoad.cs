using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.ServiceModel;
using wmsMLC.APS.wmsSI.Helpers;
using wmsMLC.APS.wmsSI.Wrappers;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;
using wmsMLC.General.DAL;

namespace wmsMLC.APS.wmsSI
{
    public partial class IntegrationService
    {
        public ErrorWrapper[] KitLoad(KitWrapper[] item)
        {
            Contract.Requires(item != null);
            Log.InfoFormat("Start of KitLoad");
            var startAllTime = DateTime.Now;
            Log.Debug(item.DumpToXML());
            var retMessage = new List<ErrorWrapper>();

            if (item[0].MandantCode == null)
                throw new Exception("Не указан MandantCode");
            Log.DebugFormat("Мандант = {0}", item[0].MandantCode);

            try
            {
                foreach (var kit in item)
                {
                    IUnitOfWork uow = null;
                    try
                    {
                        uow = UnitOfWorkHelper.GetUnit();

                        var startTime = DateTime.Now;
                        Log.DebugFormat("Загружаем комплект '{0}' манданта '{1}'", kit.ARTNAME, kit.MandantCode);

                        uow.BeginChanges();

                        KitLoadInternal(kit, retMessage, uow);

                        Log.DebugFormat("Комплект '{0}' загружен за {1}", kit.ARTNAME, DateTime.Now - startTime);
                        uow.CommitChanges();
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
                Log.DebugFormat("Общее время загрузки {0}", DateTime.Now - startAllTime);
                Log.InfoFormat("End of KitLoad");
            }
            return retMessage.ToArray();
        }

        private void KitLoadInternal(KitWrapper kit, List<ErrorWrapper> retMessage, IUnitOfWork uow)
        {
            using (var kitMgr = IoC.Instance.Resolve<IBaseManager<Kit>>())
            {
                if (uow != null)
                    kitMgr.SetUnitOfWork(uow);

                kit.MANDANTID = CheckMandant(kit.MANDANTID, kit.MandantCode, uow);

                FillArtToKit(kit, uow);

                var kitTypeCode = FillKitType(kit, uow);

                var kitCode = FillKit(kit, kitTypeCode, uow);

                KitPosLoadInternal(kit, kitCode, retMessage, uow);

            }
        }

        private void FillArtToKit(KitWrapper kit, IUnitOfWork uow)
        {
            var arts = ArtLoadHelper.FindArtByArtName(kit.MANDANTID, kit.ARTNAME, GetModeEnum.Full, uow);

            using (var artMgr = IoC.Instance.Resolve<IBaseManager<Art>>())
            {
                if (uow != null)
                    artMgr.SetUnitOfWork(uow);
         
                // если его нет, то создаем артикул и SKU
                if (!arts.Any())
                {
                    if (kit.KITINSART == 1)
                    {
                        var art = new Art
                        {
                            ArtName = kit.ARTNAME,
                            ArtDesc = kit.ARTDESC,
                            ArtCommercDay = 0,
                            ArtPickOrder = 1,
                            ARTABCD = kit.ARTABCD,
                            MANDANTID = kit.MANDANTID
                        };
                        SetXmlIgnore(art, false);
                        artMgr.Insert(ref art);
                        kit.ARTCODE_R = art.ArtCode;
                        Log.DebugFormat("Создан артикул комплекта '{0}'", kit.ARTCODE_R);
                    }
                    else
                        throw new IntegrationLogicalException("Не существует артикул комплекта = '{0}'", kit.ARTNAME);

                    // есть ли тип ЕИ
                    var mtFilter = string.Format("{0} = '{1}'",
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(MeasureType), MeasureType.MeasureTypeNamePropertyName),
                        kit.KITMEASURE);
                    var mtMgr = IoC.Instance.Resolve<IBaseManager<MeasureType>>();
                    mtMgr.SetUnitOfWork(uow);
                    var measureTypes = mtMgr.GetFiltered(mtFilter).ToArray();
                    string measureTypeCode;
                    if (!measureTypes.Any())
                    {
                        var measureType = new MeasureType
                        {
                            MeasureTypeCode = "U" + kit.KITMEASURE,
                            MeasureTypeName = kit.KITMEASURE
                        };
                        SetXmlIgnore(measureType, false);
                        mtMgr.Insert(ref measureType);
                        measureTypeCode = measureType.MeasureTypeCode;
                        Log.DebugFormat("Создан тип ЕИ (ID = {0})", measureTypeCode);
                    }
                    else
                    {
                        measureTypeCode = measureTypes[0].MeasureTypeCode;
                        Log.DebugFormat("Найден тип ЕИ (ID = {0})", measureTypeCode);
                    }

                    // есть ли ЕИ
                    var measureFilter = string.Format("{0} = '{1}'",
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(Measure), Measure.MeasureNamePropertyName),
                        kit.KITMEASURE);
                    var measureMgr = IoC.Instance.Resolve<IBaseManager<Measure>>();
                    measureMgr.SetUnitOfWork(uow);
                    var measures = measureMgr.GetFiltered(measureFilter).ToArray();
                    string measureCode;
                    if (!measures.Any())
                    {
                        var measure = new Measure
                        {
                            MeasureTypeCodeR = measureTypeCode,
                            MeasureFactor = 1,
                            MeasureName = kit.KITMEASURE,
                            MeasureCode = kit.KITMEASURE
                        };
                        SetXmlIgnore(measure, false);
                        measureMgr.Insert(ref measure);
                        measureCode = measure.MeasureCode;
                        Log.DebugFormat("Создана ЕИ (ID = {0})", measureCode);
                    }
                    else
                    {
                        measureCode = measures[0].MeasureCode;
                        Log.DebugFormat("Найдена ЕИ (ID = {0})", measureCode);
                    }

                    var skuMgr = IoC.Instance.Resolve<IBaseManager<SKU>>();
                    skuMgr.SetUnitOfWork(uow);
                    var sku = new SKU
                    {
                        ArtCode = kit.ARTCODE_R,
                        MeasureCode = measureCode,
                        SKUCount = (double)SerializationHelper.ConvertToTrueType(kit.KITCOUNT, typeof(double)),
                        SKUPrimary = true,
                        SKUName = kit.ARTNAME + "_" + kit.KITMEASURE + "_1"
                    };
                    SetXmlIgnore(sku, false);
                    skuMgr.Insert(ref sku);
                    Log.DebugFormat("Создана SKU шапки комплекта (ID = {0})", sku.SKUID);
                }
                else
                {
                    var firstOrDefault = arts.FirstOrDefault();
                    if (firstOrDefault != null)
                        kit.ARTCODE_R = firstOrDefault.ArtCode;
                }
            }
        }

        private string FillKitType(KitWrapper kit, IUnitOfWork uow)
        {
            // есть ли тип комплекта
            var kittypeFilter = string.Format("{0} = '{1}'",
                                SourceNameHelper.Instance.GetPropertySourceName(typeof(KitType), KitType.KitTypeNamePropertyName),
                                kit.KITTYPE.KITYPENAME);
            using (var kittypeMgr = IoC.Instance.Resolve<IBaseManager<KitType>>())
            {
                if (uow != null)
                    kittypeMgr.SetUnitOfWork(uow);

                var kittypes = kittypeMgr.GetFiltered(kittypeFilter).ToArray();
                if (!kittypes.Any())
                {
                    var kittype = new KitType
                    {
                        KitTypeCode = kit.KITTYPE.KITYPENAME,
                        KitTypeName = kit.KITTYPE.KITYPENAME
                    };
                    SetXmlIgnore(kittype, false);
                    kittypeMgr.Insert(ref kittype);
                    Log.DebugFormat("Создан тип комплекта (ID = {0})", kittype.KitTypeCode);
                    return kittype.KitTypeCode;
                }
                Log.DebugFormat("Найден тип комплекта (ID = {0})", kittypes[0].KitTypeCode);
                return kittypes[0].KitTypeCode;
            }
        }

        private string FillKit(KitWrapper kit, string kitTypeCode, IUnitOfWork uow)
        {
            // есть ли такой комплект
            var kitFilter = string.Format("{0} = '{1}' and {2} = '{3}'",
                                SourceNameHelper.Instance.GetPropertySourceName(typeof(Kit), Kit.ArtCodeRPropertyName),
                                kit.ARTCODE_R,
                                SourceNameHelper.Instance.GetPropertySourceName(typeof(Kit), Kit.KitTypeCodeRPropertyName),
                                kitTypeCode);

            using (var kitMgr = IoC.Instance.Resolve<IBaseManager<Kit>>())
            {
                if (uow != null)
                    kitMgr.SetUnitOfWork(uow);

                var kits = kitMgr.GetFiltered(kitFilter).ToArray();
                if (!kits.Any())
                {
                    var kitObj = new Kit
                    {
                        ArtCodeR = kit.ARTCODE_R,
                        KitCode = kitTypeCode + kit.ARTCODE_R,
                        KitTypeCodeR = kitTypeCode,
                        KitPriorityIn = kit.KITPRIORITYIN,
                        KitPriorityOut = kit.KITPRIORITYOUT
                    };
                    SetXmlIgnore(kitObj, false);
                    kitMgr.Insert(ref kitObj);
                    Log.DebugFormat("Создан комплект (ID = {0})", kitObj.KitCode);
                    return kitObj.KitCode;
                }
                if (kit.KITUPDATE == 0)
                    throw new IntegrationLogicalException("Попытка изменить существующий комплект: '{0}'", kitTypeCode + kit.ARTCODE_R);

                Log.DebugFormat("Существует комплект (ID = {0})", kits[0].KitCode);
                return kits[0].KitCode;
            }
        }

        private void KitPosLoadInternal(KitWrapper kit, string kitCode, List<ErrorWrapper> retMessage, IUnitOfWork uow)
        {
            var errCountOld = MessageHelper.GetErrorCount(retMessage);
            var errmessage = string.Format("Ошибка при загрузке комплектующих (комплект «{0}»)", kit.ARTNAME);
            var artNames = kit.KITPOS.Where(p => !string.IsNullOrEmpty(p.KITPOSARTNAME)).Select(i => i.KITPOSARTNAME.ToUpper()).Distinct().ToArray();
            if (artNames.Length == 0)
            {
                var ew = new ErrorWrapper
                {
                    ERRORCODE = MessageHelper.ErrorCode.ToString(),
                    ERRORMESSAGE =
                        string.Format("Нет артикулов в элементах комплектующей '{0}'.", kit.ARTNAME)
                };
                retMessage.Add(ew);
                throw new IntegrationLogicalException(errmessage);
            }

            var typeart = typeof(Art);
            var artsList =
              FilterHelper.GetArrayFilterIn(string.Format("UPPER({0})", SourceNameHelper.Instance.GetPropertySourceName(typeart, Art.ArtNamePropertyName)), artNames,
                  string.Format(" and {0} = {1}",
                      SourceNameHelper.Instance.GetPropertySourceName(typeart, Art.MANDANTIDPropertyName),
                      kit.MANDANTID));

            var artcomps = new List<Art>();

            using (var artMgr = IoC.Instance.Resolve<IBaseManager<Art>>())
            {
                if (uow != null)
                    artMgr.SetUnitOfWork(uow);

                foreach (var artFilter in artsList)
                {
                    var artspart = artMgr.GetFiltered(artFilter).ToArray();
                    if (artspart.Length > 0)
                        artcomps.AddRange(artspart);
                }

                foreach (var pos in kit.KITPOS)
                {
                    var existArt = artcomps.FirstOrDefault(i => pos.KITPOSARTNAME.EqIgnoreCase(i.ArtName));
                    string artCode;

                    if (existArt == null)
                    {
                        if (pos.KITPOSINSART == 1) // НЕ создается SKU!!! (потому как НЕТ ЕИ!) - кто придумал?
                        {
                            var art = new Art
                            {
                                ArtName = pos.KITPOSARTNAME,
                                ArtCommercDay = 0,
                                ArtPickOrder = 1,
                                //ARTABCD = kit.ARTABCD,
                                MANDANTID = kit.MANDANTID
                            };
                            var errmessages = ArtLoadHelper.FillArtAbcd(art, kit.ARTABCD, kit.ARTNAME, Log);
                            if (errmessages.Length > 0)
                                MessageHelper.AddMessage(errmessages, retMessage);

                            SetXmlIgnore(art, false);
                            artMgr.Insert(ref art);
                            artCode = art.ArtCode;
                            Log.DebugFormat("Создан артикул комплектующей «{0}»", pos.KITPOSARTNAME);
                        }
                        else
                        {
                            var ew = new ErrorWrapper
                            {
                                ERRORCODE = MessageHelper.ErrorCode.ToString(),
                                ERRORMESSAGE =
                                    string.Format("Не существует артикул комплектующей «{0}»", pos.KITPOSARTNAME)
                            };
                            retMessage.Add(ew);
                            continue;
                        }
                    }
                    else artCode = existArt.ArtCode;

                    var skuFilter = string.Format("{0} = '{1}' and {2} = 1",
                            SourceNameHelper.Instance.GetPropertySourceName(typeof(SKU), SKU.ArtCodePropertyName),
                            artCode,
                            SourceNameHelper.Instance.GetPropertySourceName(typeof(SKU), SKU.SKUClientPropertyName));
                    var skuMgr = IoC.Instance.Resolve<IBaseManager<SKU>>();
                    skuMgr.SetUnitOfWork(uow);
                    var skus = skuMgr.GetFiltered(skuFilter).ToArray();
                    if (!skus.Any())
                    {
                        var ew = new ErrorWrapper
                        {
                            ERRORCODE = MessageHelper.ErrorCode.ToString(),
                            ERRORMESSAGE =
                                string.Format("Не существует SKU комплектующей «{0}»", pos.KITPOSARTNAME)
                        };
                        retMessage.Add(ew);
                        continue;
                    }

                    var kitPosMgr = IoC.Instance.Resolve<IBaseManager<KitPos>>();
                    kitPosMgr.SetUnitOfWork(uow);
                    var kitPosObj = new KitPos();
                    kitPosObj = MapTo(pos, kitPosObj);
                    kitPosObj.KitPosSKUIDR = skus[0].SKUID;
                    kitPosObj.KitPosCodeR = kitCode;
                    kitPosObj.KitPosPriority = 500;
                    SetXmlIgnore(kitPosObj, false);
                    kitPosMgr.Insert(ref kitPosObj);
                }

                if (MessageHelper.GetErrorCount(retMessage) > errCountOld)
                    throw new IntegrationLogicalException(errmessage);
            }
        }
    }
}
