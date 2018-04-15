using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using wmsMLC.APS.wmsSI.Helpers;
using wmsMLC.APS.wmsSI.Wrappers;
using wmsMLC.Business.Objects;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;
using wmsMLC.General.DAL;

namespace wmsMLC.APS.wmsSI
{
    public partial class IntegrationService
    {
        public ErrorWrapper[] ReceiptLoad(PurchaseInvoiceWrapper item)
        {
            Contract.Requires(item != null);

            var retMessage = new List<ErrorWrapper>();
            Log.InfoFormat("Start of ReceiptLoad");
            var startAllTime = DateTime.Now;
            var updateSenderName = false;
            Log.Debug(item.DumpToXML());

            try
            {
                IUnitOfWork uow = null;
                try
                {
                    uow = UnitOfWorkHelper.GetUnit();

                    item.MANDANTID = CheckMandant(item.MANDANTID, item.MandantCode);
                    if (item.MANDANTID == null) 
                        throw new NullReferenceException("MandantCode");
                    Log.DebugFormat("Мандант = {0}", item.MandantCode);

                    item.IWBRECIPIENT = CheckPartnerHostRefOrName(item.MANDANTID.To<decimal>(), item.IWBRECIPIENT_CODE, item.IWBRECIPIENT_NAME, uow);
                    item.IWBSENDER = (!item.IWBUPDATE_SENDER.HasValue || item.IWBUPDATE_SENDER == 0)
                        ? CheckPartnerHostRefOrName(item.MANDANTID.Value, item.IWBSENDER_CODE, item.IWBSENDER_NAME, uow)
                        : CheckPartnerHostRef(item.MANDANTID.Value, item.IWBSENDER_CODE, item.IWBSENDER_NAME, out updateSenderName, uow);

                    ReceiptLoadHelper.FindCountries(item, Log);

                    uow.BeginChanges();
                    if (item.IWBSENDER == null && item.IWBCREATE_SENDER == 1)
                        item.IWBSENDER = CreateSender(item, uow);

                    if (item.IWBSENDER != null && updateSenderName)
                        UpdateSender(item.IWBSENDER, item.IWBSENDER_NAME, uow);

                    IwbLoadInternal(item, retMessage, uow);
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
            catch (Exception ex)
            {
                var message = ExceptionHelper.ExceptionToString(ex);
                Log.Error(message, ex);
                throw new FaultException<string>(message, new FaultReason(message));
            }
            finally
            {
                Log.DebugFormat("Общее время загрузки {0}", DateTime.Now - startAllTime);
                Log.InfoFormat("End of ReceiptLoad");
            }
            return retMessage.ToArray();
        }

        private decimal? CreateSender(PurchaseInvoiceWrapper item, IUnitOfWork uow = null)
        {
            using (var partnerMgr = IoC.Instance.Resolve<IBaseManager<Partner>>())
            {
                if (uow != null)
                    partnerMgr.SetUnitOfWork(uow);

                var partner = new Partner { PartnerName = item.IWBSENDER_NAME, MandantId = item.MANDANTID };

                if (!string.IsNullOrEmpty(item.IWBSENDER_CODE))
                    partner.PartnerHostRef = item.IWBSENDER_CODE;

                if (partner.GlobalParamVal == null)
                    partner.GlobalParamVal = new WMSBusinessCollection<PartnerGpv>();

                var gpv = new PartnerGpv
                {
                    GparamValValue = "1",
                    GlobalParamCode_R = "IsSender",
                    GParamVal2Entity = "PARTNER"
                };
                partner.GlobalParamVal.Add(gpv);

                SetXmlIgnore(partner, false);
                partnerMgr.Insert(ref partner);
                Log.DebugFormat("Партнер (отправитель) «{0}» загружен (ID = {1})", item.IWBSENDER_NAME, partner.PartnerId);

                return partner.PartnerId;
            }
        }

        private void UpdateSender(decimal? senderId, string senderName, IUnitOfWork uow = null)
        {
            if (string.IsNullOrEmpty(senderName)) 
                return;

            var partnerFilter = string.Format("{0} = {1}",
                SourceNameHelper.Instance.GetPropertySourceName(typeof(Partner), Partner.PARTNERIDPropertyName), senderId);

            using (var mgr = IoC.Instance.Resolve<IBaseManager<Partner>>())
            {
                if (uow != null)
                    mgr.SetUnitOfWork(uow);

                var partners = mgr.GetFiltered(partnerFilter).ToArray();

                if (partners.Where(i => i.PartnerName == senderName).ToArray().Length == 0)
                {
                    partners[0].PartnerName = senderName;
                    SetXmlIgnore(partners[0], true);
                    mgr.Update(partners[0]);
                    Log.DebugFormat("Обновлен партнер (отправитель) «{0}» (ID = {1})", senderName, partners[0].PartnerId);
                }
            }
        }

        private void IwbLoadInternal(PurchaseInvoiceWrapper item, List<ErrorWrapper> retMessage, IUnitOfWork uow)
        {
            var errCountOld = MessageHelper.GetErrorCount(retMessage);
            using (var iwbMgr = IoC.Instance.Resolve<IBaseManager<IWB>>())
            {
                if (uow != null)
                    iwbMgr.SetUnitOfWork(uow);

                var isUpdate = false;
                string filter;
                decimal iwbId = 0;

                if (item.IWBID != null)
                {
                    filter = string.Format("{0} = {1}",
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(IWB), IWB.IWBIDPropertyName), item.IWBID);
                    var iwBs = iwbMgr.GetFiltered(filter).ToArray();
                    if (iwBs.Length == 0)
                        throw new IntegrationLogicalException("Приходная накладная с ID = {0} не существует!", item.IWBID);
                    if (iwBs[0].IWBPosL != null)
                        throw new IntegrationLogicalException("В накладной (ID = {0}) существуют позиции!", item.IWBID);
                    isUpdate = true;
                }
                else
                {
                    if (string.IsNullOrEmpty(item.IWBNAME))
                        throw new IntegrationLogicalException("Не заполнено обязательное поле «Номер накладной»");

                    filter = string.Format("{0} = '{1}' and {2} = {3}",
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(IWB), IWB.IWBNAMEPropertyName), item.IWBNAME,
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(IWB), IWB.MANDANTIDPropertyName), item.MANDANTID);
                    var iwBs = iwbMgr.GetFiltered(filter).ToArray();
                    if (iwBs.Length > 0)
                    {
                        iwbId = iwBs[0].IWBID.Value;
                        if (item.IWBRECREATE == 0)
                        {
                            if (iwBs[0].DateIns.HasValue && iwBs[0].DateIns.Value.Year == DateTime.Now.Year)
                                throw new IntegrationLogicalException("Приходная накладная «{0}» существует!", item.IWBNAME);
                        }
                        else
                            if (iwBs[0].StatusCode == "IWB_CREATED")
                            {
                                var filterPosDel = string.Format("{0} = {1} and {2} = {3}",
                                    SourceNameHelper.Instance.GetPropertySourceName(typeof(IWBPos), IWBPos.IWBID_RPropertyName), iwbId,
                                    SourceNameHelper.Instance.GetPropertySourceName(typeof(IWBPos), IWBPos.MANDANTIDPropertyName), item.MANDANTID);
                                var mgrPosDel = IoC.Instance.Resolve<IBaseManager<IWBPos>>();
                                mgrPosDel.SetUnitOfWork(uow);
                                var posDels = mgrPosDel.GetFiltered(filterPosDel).ToArray();

                                if (item.IWBPOSL.Count(i => i.TRANSITDATAL != null && i.TRANSITDATAL.Any()) > 0)
                                    DelTransitIwbPos(item, posDels, uow);

                                mgrPosDel.Delete(posDels);
                                isUpdate = true;
                            }
                            else
                                if (iwBs[0].DateIns.HasValue && iwBs[0].DateIns.Value.Year == DateTime.Now.Year)
                                    throw new IntegrationLogicalException("Приходная накладная «{0}» в работе!", item.IWBNAME);
                    }
                }
                var iwbObj = iwbMgr.New();

                if (!string.IsNullOrEmpty(item.IWBFACTORY))
                    FillFactoryIwb(item, uow);

                iwbObj = MapTo(item, iwbObj);
                var currentNum = item.IWBCOUNTPOS.To<int>();

                if (item.IWBPOSL == null || item.IWBPOSL.Count == 0)
                    throw new IntegrationLogicalException("Нет позиций по накладной «{0}»", item.IWBNAME);

                if (iwbObj.IWBPosL == null)
                    iwbObj.IWBPosL = new WMSBusinessCollection<IWBPos>();

                FillArtIwb(item, retMessage, uow);

                FillArtGroupIwb(item, retMessage, uow);

                var artCodes = item.IWBPOSL.Select(i => i.IWBPOSARTCODE).Distinct();    // ??? если две позиции с одинаковыми комплектами!!!

                # region . CreateKitPos .
                var kitFilterList = FilterHelper.GetArrayFilterIn(SourceNameHelper.Instance.GetPropertySourceName(typeof(Kit),
                                                         Kit.ArtCodeRPropertyName), artCodes,
                                                         string.Format(" and {0} = {1}",
                                                         SourceNameHelper.Instance.GetPropertySourceName(typeof(Art),
                                                         Kit.MANDANTIDPropertyName), item.MANDANTID));
                foreach (var kitFilter in kitFilterList)
                {
                    var mgrKit = IoC.Instance.Resolve<IBaseManager<Kit>>();
                    mgrKit.SetUnitOfWork(uow);
                    //var kitFilter = artCodesFilterValues.Replace("ARTCODE", "ARTCODE_R");

                    var kits = mgrKit.GetFiltered(kitFilter).ToArray();

                    if (kits.Length > 0 && item.IWBREPLACEKITS == 1)
                    {
                        foreach (var kit in kits)
                        {
                            using (var kpMgr = IoC.Instance.Resolve<IBaseManager<KitPos>>())
                            {
                                var kitposFilter = string.Format("{0} = '{1}'",
                                    SourceNameHelper.Instance.GetPropertySourceName(typeof(KitPos), KitPos.KitPosCodeRPropertyName), kit.KitCode);
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

                                var firstOrDefault = item.IWBPOSL.FirstOrDefault(i => i.IWBPOSARTCODE.Equals(kit.ArtCodeR));

                                if (firstOrDefault == null)
                                    continue;

                                var kitCount = firstOrDefault.IWBPOSCOUNT;
                                var kitNumber = firstOrDefault.IWBPOSNUMBER;

                                var itemToRemove = item.IWBPOSL.SingleOrDefault(r => r.IWBPOSNUMBER == kitNumber);
                                item.IWBPOSL.Remove(itemToRemove);

                                foreach (var pos in ktps.Select(kitpos => new IWBPos
                                {
                                    SKUID = kitpos.KitPosSKUIDR.To<decimal>(),
                                    IWBPosCount = kitCount.To<decimal>() * kitpos.KitPosCount,
                                    IWBPosArtName = (kitpos.KitPosCodeR.StartsWith("KIT"))
                                        ? kitpos.KitPosCodeR.Substring(item.MandantCode.Length + 3)
                                        : kitpos.KitPosCodeR.Substring(item.MandantCode.Length),
                                    IWBPosNumber = ++currentNum
                                }))
                                {
                                    iwbObj.IWBPosL.Add(pos);
                                }
                            }
                        }

                        if (MessageHelper.GetErrorCount(retMessage) > errCountOld)
                            throw new IntegrationLogicalException(
                                "Ошибка при проверке комплектующих (накладная «{0}»)", item.IWBNAME);
                    }
                }
                #endregion . CreateKitPos .

                FillMeasureIwb(item, retMessage, uow);

                if (!string.IsNullOrEmpty(item.IWBDELGROUP) && item.IWBDELGROUP != "")
                    DelArt2GroupIwb(artCodes, item.IWBDELGROUP, uow);

                if (item.IWBPOSL.Count(i => i.GROUP2ARTL != null) > 0)
                    LoadArt2GroupIwb(item, retMessage, uow);

                if (item.IWBCONVERTSKU == 0)
                    FillSkUforIwb(item, artCodes, retMessage, uow);
                else
                    ConvertSku2SkuIwb(item, artCodes, currentNum, retMessage, uow);

                foreach (var pos in item.IWBPOSL)
                {
                    if (pos.CHECKBARCODE.HasValue && pos.CHECKBARCODE.Value > 0)
                    {
                        //Проверка ШК
                        if (!BarcodeHelper.ValidateBarcode(pos.SKUID_R, pos.BARCODE, uow))
                        {
                            var exmessage = string.Format(BarcodeHelper.CheckBarcodeMessage +
                                ". ШК '{0}' не найден для позиции приходной накладной '{1}' (артикул '{2}').",
                                pos.BARCODE, item.IWBNAME, pos.IWBPOSARTNAME);

                            Func<ErrorWrapper> messageHandler = () => new ErrorWrapper
                            {
                                ERRORCODE = MessageHelper.WarningCode.ToString(),
                                ERRORMESSAGE = exmessage
                            };

                            switch (pos.CHECKBARCODE.Value)
                            {
                                case 2: //warning
                                    retMessage.Add(messageHandler());
                                    break;
                                case 3: 
                                    if (string.IsNullOrEmpty(pos.BARCODE))
                                    {
                                        retMessage.Add(messageHandler());
                                        break;
                                    }
                                    throw new IntegrationLogicalException(exmessage);
                                case 4: //Add barcode
                                    BarcodeHelper.AddBarcode(pos.SKUID_R, pos.BARCODE, uow);
                                    break;
                                default: //error
                                    throw new IntegrationLogicalException(exmessage);
                            }
                        }
                    }

                    if (item.IWBCHECKATTR == 1)
                        LoadLastAttr(pos, uow);

                    var p = new IWBPos();
                    if (!string.IsNullOrEmpty(item.IWBFACTORY))
                        pos.FACTORYID_R = item.FACTORYID_R.To<decimal>();
                    MapTo(pos, p);
                    SetXmlIgnore(p, true);

                    if (isUpdate)
                    {
                        using (var iwbPosMgr = IoC.Instance.Resolve<IBaseManager<IWBPos>>())
                        {
                            iwbPosMgr.SetUnitOfWork(uow);
                            p.IWBID_R = iwbId;
                            iwbPosMgr.Insert(ref p);
                            Log.InfoFormat("Добавлена позиция в приходную накладную «{0}» (ID = {1})", item.IWBNAME,
                                p.IWBPosID);
                        }

                        pos.IWBPOSID = p.GetKey<decimal>();

                        //Загружаем CPV для IWBPos
                        var errposmessages = LoadIwbPosCpv(pos, item.IWBNAME, uow);
                        MessageHelper.AddMessage(errposmessages, retMessage);
                    }
                    else
                    {
                        List<string> messageL;
                        var wmscpvs = CreateIwbPosCpv(pos, item.IWBNAME, uow, out messageL);
                        MessageHelper.AddMessage(messageL, retMessage);
                        if (wmscpvs != null && wmscpvs.Any())
                        {
                            if (p.CustomParamVal == null)
                                p.CustomParamVal = new WMSBusinessCollection<IWBPosCpv>();
                            p.CustomParamVal.AddRange(wmscpvs);
                        }
                        iwbObj.IWBPosL.Add(p);
                    }
                }

                if (!isUpdate)
                {
                    SetXmlIgnore(iwbObj, false);

                    var iwbObjPosL = iwbObj.IWBPosL == null ? null : iwbObj.IWBPosL.ToArray();
                    if (iwbObj.IWBPosL != null)
                        iwbObj.IWBPosL.Clear();

                    iwbMgr.Insert(ref iwbObj);
                    item.IWBID = iwbObj.IWBID;

                    //Сохраняем позиции и CPV для IWBPos
                    if (iwbObjPosL != null)
                    {
                        var cpvhelper = new CpvHelper<IWBPosCpv>(ReceiptLoadHelper.Cpv2EntityIwbPosName, "-1");
                        using (var iwbPosMgr = IoC.Instance.Resolve<IBaseManager<IWBPos>>())
                        {
                            if (uow != null)
                                iwbPosMgr.SetUnitOfWork(uow);
                            foreach (var pos in iwbObjPosL)
                            {
                                pos.IWBID_R = iwbObj.IWBID;
                                var poswms = pos;
                                var cpvs = pos.CustomParamVal == null ? null : pos.CustomParamVal.ToArray();
                                if (pos.CustomParamVal != null)
                                    pos.CustomParamVal.Clear();
                                iwbPosMgr.Insert(ref poswms);

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

                    if (iwbObj.IWBPosL != null)
                    {
                        for (var i = 0; i < iwbObj.IWBPosL.Count && i < item.IWBPOSL.Count; i++)
                        {
                            item.IWBPOSL[i].IWBPOSID = iwbObj.IWBPosL[i].GetKey<decimal?>();
                        }
                    }

                    Log.InfoFormat("Загружена приходная накладная «{0}» (ID = {1})", iwbObj.IWBName, iwbObj.IWBID);
                    var ew = new ErrorWrapper
                    {
                        ERRORCODE = MessageHelper.SuccessCode.ToString(),
                        ERRORMESSAGE = string.Format("Загружена приходная накладная «{0}»", iwbObj.IWBName)
                    };
                    retMessage.Add(ew);
                }
                else
                {
                    SetXmlIgnore(iwbObj, true);
                    iwbObj.IWBID = iwbId;
                    item.IWBID = iwbId;
                    iwbMgr.Update(iwbObj);
                    Log.InfoFormat("Обновлена приходная накладная «{0}» (ID = {1})", iwbObj.IWBName, iwbObj.IWBID);
                    var ew = new ErrorWrapper
                    {
                        ERRORCODE = MessageHelper.SuccessCode.ToString(),
                        ERRORMESSAGE = string.Format("Обновлена приходная накладная «{0}»", iwbObj.IWBName)
                    };
                    retMessage.Add(ew);
                }

                LoadTransitIwb(item, retMessage, uow);

                if (item.IWBPOSL.Count(i => i.TRANSITDATAL != null) > 0)
                    LoadTransitIwbPos(item, retMessage, uow);

                if (item.CUSTOMPARAMVAL == null || item.CUSTOMPARAMVAL.Count == 0) 
                    return;

                iwbObj.CustomParamVal = new WMSBusinessCollection<IWBCpv>();

                LoadCpvIwb(item, uow);
            }
        }

        private static void FillArtIwb(PurchaseInvoiceWrapper item, List<ErrorWrapper> retMessage, IUnitOfWork uow)
        {
            var errCountOld = MessageHelper.GetErrorCount(retMessage);
            var errmessage = string.Format("Ошибка при проверке артикулов (накладная «{0}»)", item.IWBNAME);

            var artNames = item.IWBPOSL.Where(p => !string.IsNullOrEmpty(p.IWBPOSARTNAME)).Select(p => p.IWBPOSARTNAME.ToUpper()).Distinct().ToArray();
            if (artNames.Length == 0)
            {
                var ew = new ErrorWrapper
                {
                    ERRORCODE = MessageHelper.ErrorCode.ToString(),
                    ERRORMESSAGE = string.Format("Отсутствуют артикула в позициях накладной '{0}'.", item.IWBNAME)
                };
                retMessage.Add(ew);
                throw new IntegrationLogicalException(errmessage);
            }

            var typeart = typeof (Art);
            var arts = new List<Art>();
            var artsList =
                FilterHelper.GetArrayFilterIn(string.Format("UPPER({0})", SourceNameHelper.Instance.GetPropertySourceName(typeart, Art.ArtNamePropertyName)), artNames,
                    string.Format(" and {0} = {1}",
                        SourceNameHelper.Instance.GetPropertySourceName(typeart, Art.MANDANTIDPropertyName),
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

                foreach (var pos in item.IWBPOSL)
                {
                    var existsArt = arts.Where(i => pos.IWBPOSARTNAME.EqIgnoreCase(i.ArtName)).ToArray();
                    if (existsArt.Length > 1 && (existsArt = arts.Where(i => pos.IWBPOSARTNAME == i.ArtName).ToArray()).Length > 1)
                    {
                        var ew = new ErrorWrapper
                        {
                            ERRORCODE = MessageHelper.ErrorCode.ToString(),
                            ERRORMESSAGE = string.Format("Существует несколько значений для артикула «{0}»",
                                pos.IWBPOSARTNAME)
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
                            pos.IWBPOSARTNAME)
                        };
                        retMessage.Add(ew);
                        continue;
                    }
                    pos.IWBPOSARTCODE = existsArt[0].ArtCode;
                }

                if (MessageHelper.GetErrorCount(retMessage) > errCountOld)
                    throw new IntegrationLogicalException(errmessage);
            }
        }

        private static void FillArtGroupIwb(PurchaseInvoiceWrapper item, List<ErrorWrapper> retMessage, IUnitOfWork uow)
        {
            if (string.IsNullOrEmpty(item.IWBPOSL[0].IWBPOSGROUPCHECK) || item.IWBPOSL[0].IWBPOSGROUPCHECK == "")
                return;

            var errCountOld = MessageHelper.GetErrorCount(retMessage);
            var artCheckNames = item.IWBPOSL.Where(i => (!String.IsNullOrEmpty(i.IWBPOSGROUPCHECK) &&
                                                         i.IWBPOSGROUPCHECK != "")).Select(i => i.IWBPOSARTCODE);
            var artCheckFilterValues = "'" + string.Join("','", artCheckNames) + "'";
            using (var a2GMgr = IoC.Instance.Resolve<IBaseManager<Art2Group>>())
            {
                if (uow != null)
                    a2GMgr.SetUnitOfWork(uow);
                var art2GroupFilter = string.Format("{0} in ({1})",
                     SourceNameHelper.Instance.GetPropertySourceName(typeof(Art2Group), Art2Group.Art2GroupArtCodePropertyName), artCheckFilterValues);
                var art2Groups = a2GMgr.GetFiltered(art2GroupFilter).ToArray();

                foreach (var pos in item.IWBPOSL)
                {
                    var existsArtGroup = art2Groups.Where(i => pos.IWBPOSARTCODE.Equals(i.Art2GroupArtCode)).ToArray();
                    if (String.IsNullOrEmpty(pos.IWBPOSBATCH))
                    {
                        ErrorWrapper ew;
                        if (existsArtGroup.Length != 1)
                        {
                            ew = new ErrorWrapper
                            {
                                ERRORCODE = MessageHelper.ErrorCode.ToString(),
                                ERRORMESSAGE = string.Format("Неверное количество групп = «{0}» для артикула без № партии",
                                existsArtGroup.Length)
                            };
                            retMessage.Add(ew);
                            continue;
                        }

                        if (existsArtGroup[0].Art2GroupArtGroupCode != pos.IWBPOSGROUPCHECK) 
                            continue;
                        ew = new ErrorWrapper
                        {
                            ERRORCODE = MessageHelper.ErrorCode.ToString(),
                            ERRORMESSAGE = string.Format("Не соответствует группа «{0}» для артикула без № партии",
                            existsArtGroup[0].Art2GroupArtGroupCode)
                        };
                        retMessage.Add(ew);
                    }
                    else
                        if (existsArtGroup.Length > 0 && existsArtGroup[0].Art2GroupArtGroupCode != pos.IWBPOSGROUPCHECK)
                            pos.IWBPOSBATCH = null;
                }
                if (MessageHelper.GetErrorCount(retMessage) > errCountOld)
                    throw new IntegrationLogicalException("Ошибка при проверке групп артикулов (накладная «{0}»)", item.IWBNAME);
            }
        }

        private static void FillMeasureIwb(PurchaseInvoiceWrapper item, List<ErrorWrapper> retMessage, IUnitOfWork uow)
        {
            if (item.IWBPOSL.Count == 0 || string.IsNullOrEmpty(item.IWBPOSL[0].IWBPOSMEASURE))
                return;

            var errCountOld = MessageHelper.GetErrorCount(retMessage);
            var measureCodes = item.IWBPOSL.Select(i => i.IWBPOSMEASURE.ToUpper()).Distinct();
            var measuresFilterValue = "'" + string.Join("','", measureCodes) + "'";

            using (var measureMgr = IoC.Instance.Resolve<IBaseManager<Measure>>())
            {
                var measureFilter = string.Format("upper({0}) in ({1}) or upper({2}) in ({3})",
                    SourceNameHelper.Instance.GetPropertySourceName(typeof(Measure), Measure.MeasureCodePropertyName), measuresFilterValue,
                    SourceNameHelper.Instance.GetPropertySourceName(typeof(Measure), Measure.MeasureShortNamePropertyName), measuresFilterValue);

                if (uow != null)
                    measureMgr.SetUnitOfWork(uow);
                var measures = measureMgr.GetFiltered(measureFilter).ToArray();

                foreach (var iwbPosWrapper in item.IWBPOSL)
                {
                    var existMeasure = measures.FirstOrDefault(i => iwbPosWrapper.IWBPOSMEASURE.EqIgnoreCase(i.MeasureCode) ||
                                                                    iwbPosWrapper.IWBPOSMEASURE.EqIgnoreCase(i.MeasureShortName));
                    if (existMeasure == null)
                    {
                        var ew = new ErrorWrapper
                        {
                            ERRORCODE = MessageHelper.ErrorCode.ToString(),
                            ERRORMESSAGE = string.Format("Не найдена единица измерения «{0}»",
                            iwbPosWrapper.IWBPOSMEASURE)
                        };
                        retMessage.Add(ew);
                        continue;
                    }
                    iwbPosWrapper.IWBPOSMEASURE = existMeasure.MeasureCode;
                }
                if (MessageHelper.GetErrorCount(retMessage) > errCountOld)
                    throw new IntegrationLogicalException("Ошибка при проверке единиц измерения (накладная «{0}»)", item.IWBNAME);
            }
        }

        private void FillFactoryIwb(PurchaseInvoiceWrapper item, IUnitOfWork uow)
        {
            using (var factoryMgr = IoC.Instance.Resolve<IBaseManager<Factory>>())
            {
                var factoryFilter = string.Format("upper({0}) = upper('{1}') and {2} = {3}",
                    SourceNameHelper.Instance.GetPropertySourceName(typeof(Factory), Factory.FactoryCodePropertyName), item.IWBFACTORY,
                    SourceNameHelper.Instance.GetPropertySourceName(typeof(Factory), Factory.PARTNERID_RPropertyName), item.MANDANTID);

                if (uow != null)
                    factoryMgr.SetUnitOfWork(uow);
                var factorys = factoryMgr.GetFiltered(factoryFilter).ToArray();

                if (factorys.Length == 0)
                    throw new IntegrationLogicalException("Не найден код фабрики «{0}» (накладная «{1}»)", item.IWBFACTORY, item.IWBNAME);

                if (factorys.Length > 1)
                    throw new IntegrationLogicalException("Существует несколько значений для кода фабрики «{0}» (накладная «{1}»)", item.IWBFACTORY, item.IWBNAME);

                item.FACTORYID_R = factorys[0].FactoryID;
                Log.DebugFormat("Фабрика = {0}", item.FACTORYID_R);
            }
        }

        private static void FillSkUforIwb(PurchaseInvoiceWrapper item, IEnumerable<string> artCodes, List<ErrorWrapper> retMessage, IUnitOfWork uow)
        {
            if (item.IWBPOSL.Count == 0)
                return;

            var errCountOld = MessageHelper.GetErrorCount(retMessage);
            var isNotMeasures = string.IsNullOrEmpty(item.IWBPOSL[0].IWBPOSMEASURE);

            using (var skuMgr = IoC.Instance.Resolve<IBaseManager<SKU>>())
            {
                string[] skuList;
                if (!isNotMeasures)
                {
                    var measureCodes = item.IWBPOSL.Select(i => i.IWBPOSMEASURE.ToUpper()).Distinct();
                    var measuresFilterValue = "'" + string.Join("','", measureCodes) + "'";
                    skuList = FilterHelper.GetArrayFilterIn("ARTCODE_R", artCodes,
                        string.Format(" and {0} in ({1}) and ({2} = 1 or {3} = 1)",
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(SKU), SKU.MeasureCodePropertyName), measuresFilterValue,
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(SKU), SKU.SKUClientPropertyName),
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(SKU), SKU.SKUPrimaryPropertyName)));
                }
                else
                {
                    skuList = FilterHelper.GetArrayFilterIn("ARTCODE_R", artCodes,
                        string.Format(" and ({0} = 1 or {1} = 1)",
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(SKU), SKU.SKUClientPropertyName),
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(SKU), SKU.SKUPrimaryPropertyName)));
                }
                var skuDefList = FilterHelper.GetArrayFilterIn("ARTCODE_R", artCodes,
                    string.Format(" and {0} = 1",
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(SKU), SKU.SKUDefaultPropertyName)));

                if (uow != null)
                    skuMgr.SetUnitOfWork(uow);

                var skus = new List<SKU>();
                var skusDef = new List<SKU>();
                foreach (var skuFilter in skuList)
                {
                    skus.AddRange(skuMgr.GetFiltered(skuFilter).ToArray());
                }

                if (item.IWBCHECKMULTIPLE == 1)
                {
                    foreach (var skuDefFilter in skuDefList)
                    {
                        skusDef.AddRange(skuMgr.GetFiltered(skuDefFilter).ToArray());
                    }
                }

                foreach (var iwbPosWrapper in item.IWBPOSL)
                {
                    SKU existSku;
                    if (!isNotMeasures)
                        existSku = skus.FirstOrDefault(i => iwbPosWrapper.IWBPOSARTCODE.Equals(i.ArtCode) &&
                                                                iwbPosWrapper.IWBPOSMEASURE.Equals(i.MeasureCode) &&
                                                                i.SKUClient);
                    else
                        existSku = skus.FirstOrDefault(i => iwbPosWrapper.IWBPOSARTCODE.Equals(i.ArtCode) &&
                                                                i.SKUClient);

                    ErrorWrapper ew;
                    if (existSku == null)
                        if (item.IWBALLOWBASE == 0)
                        {
                            if (!isNotMeasures)
                            {
                                ew = new ErrorWrapper
                                {
                                    ERRORCODE = MessageHelper.ErrorCode.ToString(),
                                    ERRORMESSAGE = string.Format("Не существует клиентской SKU для артикула «{0}» с ЕИ «{1}»",
                                        iwbPosWrapper.IWBPOSARTNAME, iwbPosWrapper.IWBPOSMEASURE)
                                };
                                retMessage.Add(ew);
                                continue;
                            }
                            ew = new ErrorWrapper
                            {
                                ERRORCODE = MessageHelper.ErrorCode.ToString(),
                                ERRORMESSAGE = string.Format("Не существует клиентской SKU для артикула «{0}»",
                                    iwbPosWrapper.IWBPOSARTNAME)
                            };
                            retMessage.Add(ew);
                            continue;
                        }
                        else
                        {
                            existSku = skus.FirstOrDefault(i => iwbPosWrapper.IWBPOSARTCODE.Equals(i.ArtCode) && i.SKUPrimary);
                            if (existSku == null)
                            {
                                ew = new ErrorWrapper
                                {
                                    ERRORCODE = MessageHelper.ErrorCode.ToString(),
                                    ERRORMESSAGE = string.Format("Не существует даже базовой SKU для артикула «{0}»",
                                        iwbPosWrapper.IWBPOSARTNAME)
                                };
                                retMessage.Add(ew);
                                continue;
                            }
                            iwbPosWrapper.SKUID_R = existSku.SKUID;
                        }
                    else
                    {
                        iwbPosWrapper.SKUID_R = existSku.SKUID;
                    }

                    if (item.IWBCHECKMULTIPLE != 1 || iwbPosWrapper.IWBPOSCHECKMULTIPLE != 1)
                        continue;

                    var existSkuDefault = skusDef.FirstOrDefault(i => iwbPosWrapper.IWBPOSARTCODE.Equals(i.ArtCode) && i.SKUDefault);
                    if (existSkuDefault == null)
                    {
                        ew = new ErrorWrapper
                        {
                            ERRORCODE = MessageHelper.ErrorCode.ToString(),
                            ERRORMESSAGE = string.Format("Не существует SKU по умолчанию для артикула «{0}»", iwbPosWrapper.IWBPOSARTNAME)
                        };
                        retMessage.Add(ew);
                        continue;
                    }

                    //if (!int.TryParse(Convert.ToString(iwbPosWrapper.IWBPOSCOUNT.To<double>()/existSkuDefault.SKUCount), out res))
                    //    retMessage.Add(ew);

                    var skuratio = Convert.ToDouble(iwbPosWrapper.IWBPOSCOUNT) / existSkuDefault.SKUCount;
                    var skuratioceil = Convert.ToInt64(Math.Floor(skuratio));
                    var skurem = skuratio - skuratioceil;

                    if (skuratioceil == 0 || skurem > 0)
                    {
                        ew = new ErrorWrapper
                        {
                            ERRORCODE = MessageHelper.ErrorCode.ToString(),
                            ERRORMESSAGE = string.Format("У артикула «{0}» количество не кратно коробу", iwbPosWrapper.IWBPOSARTNAME)
                        };
                        retMessage.Add(ew);
                    }
                }

                if (MessageHelper.GetErrorCount(retMessage) > errCountOld)
                    throw new IntegrationLogicalException("Ошибка при поиске SKU (накладная «{0}»)", item.IWBNAME);
            }
        }

        private static void ConvertSku2SkuIwb(PurchaseInvoiceWrapper item, IEnumerable<string> artCodes, int currentNum,
            List<ErrorWrapper> retMessage, IUnitOfWork uow)
        {
            if (item.IWBPOSL.Count == 0)
                return;

            var errCountOld = MessageHelper.GetErrorCount(retMessage);

            using (var skuMgr = IoC.Instance.Resolve<IBaseManager<SKU>>())
            {
                var skuList = FilterHelper.GetArrayFilterIn("ARTCODE_R", artCodes,
                        string.Format(" and ({0} = 1 or {1} = 1)",
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(SKU), SKU.SKUClientPropertyName),
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(SKU), SKU.SKUDefaultPropertyName)));

                if (uow != null)
                    skuMgr.SetUnitOfWork(uow);
                var skus = new List<SKU>();
                foreach (var skuFilter in skuList)
                {
                    skus.AddRange(skuMgr.GetFiltered(skuFilter).ToArray());
                }
                var newIwbPos = new WMSBusinessCollection<IWBPosWrapper>();

                foreach (var iwbPosWrapper in item.IWBPOSL)
                {
                    var existSkuClient = skus.FirstOrDefault(i => iwbPosWrapper.IWBPOSARTCODE.Equals(i.ArtCode) && i.SKUClient);
                    if (existSkuClient == null)
                    {
                        var ew = new ErrorWrapper
                        {
                            ERRORCODE = MessageHelper.ErrorCode.ToString(),
                            ERRORMESSAGE = string.Format("Не существует клиентской SKU для артикула «{0}»", iwbPosWrapper.IWBPOSARTNAME)
                        };
                        retMessage.Add(ew);
                        continue;
                    }

                    var existSkuDefault = skus.FirstOrDefault(i => iwbPosWrapper.IWBPOSARTCODE.Equals(i.ArtCode) && i.SKUDefault);
                    if (existSkuDefault == null)
                    {
                        var ew = new ErrorWrapper
                        {
                            ERRORCODE = MessageHelper.ErrorCode.ToString(),
                            ERRORMESSAGE = string.Format("Не существует SKU по умолчанию для артикула «{0}»", iwbPosWrapper.IWBPOSARTNAME)
                        };
                        retMessage.Add(ew);
                        continue;
                    }

                    using (var mgrBpProc = IoC.Instance.Resolve<BPProcessManager>())
                    {
                        mgrBpProc.SetUnitOfWork(uow);
                        int res;
                        var originalQty = iwbPosWrapper.IWBPOSCOUNT.To<decimal>();
                        var retQty = mgrBpProc.ConvertSKUtoSKU(existSkuClient.SKUID, existSkuDefault.SKUID, 1, iwbPosWrapper.IWBPOSCOUNT);

                        if (int.TryParse(Convert.ToString(retQty), out res))
                        {
                            iwbPosWrapper.SKUID_R = existSkuDefault.SKUID;
                            iwbPosWrapper.IWBPOSCOUNT2SKU = originalQty.To<double>() / retQty.To<double>();
                            iwbPosWrapper.IWBPOSCOUNT = retQty;
                        }
                        else
                        {
                            var resultQty = Math.Floor(retQty);
                            if (resultQty > 0)
                            {
                                iwbPosWrapper.SKUID_R = existSkuDefault.SKUID;
                                iwbPosWrapper.IWBPOSCOUNT2SKU = existSkuDefault.SKUCount;
                                iwbPosWrapper.IWBPOSCOUNT = resultQty;

                                var iwbPosAddWrapper = new IWBPosWrapper
                                {
                                    SKUID_R = existSkuDefault.SKUID,
                                    IWBPOSARTCODE = existSkuDefault.ArtCode,
                                    IWBPOSARTNAME = iwbPosWrapper.IWBPOSARTNAME,
                                    IWBPOSCOUNT = 1,
                                    IWBPOSCOUNT2SKU = originalQty.To<double>() -
                                                    (resultQty.To<double>() * existSkuDefault.SKUCount),
                                    IWBPOSNUMBER = ++currentNum,
                                    IWBPOSMANUAL = false,
                                    IWBPOSBATCH = iwbPosWrapper.IWBPOSBATCH,
                                    IWBPOSLOT = iwbPosWrapper.IWBPOSLOT
                                };
                                newIwbPos.Add(iwbPosAddWrapper);
                            }
                            else
                            {
                                iwbPosWrapper.SKUID_R = existSkuDefault.SKUID;
                                iwbPosWrapper.IWBPOSCOUNT2SKU = originalQty.To<double>() -
                                                    (resultQty.To<double>() * existSkuDefault.SKUCount);
                                iwbPosWrapper.IWBPOSCOUNT = 1;
                            }
                        }
                    }
                }
                if (newIwbPos.Count > 0)
                    foreach (var newPos in newIwbPos)
                    {
                        item.IWBPOSL.Add(newPos);
                    }
            }
            if (MessageHelper.GetErrorCount(retMessage) > errCountOld)
                throw new IntegrationLogicalException("Ошибка при поиске и пересчете SKU (накладная «{0}»)", item.IWBNAME);
        }

        private void LoadCpvIwb(PurchaseInvoiceWrapper item, IUnitOfWork uow)
        {
            if (item.CUSTOMPARAMVAL == null) 
                return;

            var cpvParent = 0;

            using (var cpvMgr = IoC.Instance.Resolve<IBaseManager<IWBCpv>>())
            {
                if (uow != null)
                    cpvMgr.SetUnitOfWork(uow);

                foreach (var cw in item.CUSTOMPARAMVAL)
                {
                    cpvMgr.SetUnitOfWork(uow);
                    var c = new IWBCpv();
                    if (cw.CPVID_IWBCPV == -1)
                    {
                        cw.CPVID_IWBCPV = null;
                        c = MapTo(cw, c);
                        c.CPVKey = item.IWBID.ToString();
                        cpvMgr.Insert(ref c);
                        cpvParent = c.CPVID.To<int>();
                        Log.InfoFormat("Загружена CPV '{0}' (ID = {1})", cw.CUSTOMPARAMCODE_R_IWBCPV, c.CPVID);
                    }
                    else
                    {
                        c = MapTo(cw, c);
                        c.CPVKey = item.IWBID.ToString();
                        if (cpvParent != 0)
                            c.CPVParent = cpvParent;
                        cpvMgr.Insert(ref c);
                        Log.InfoFormat("Загружена CPV '{0}' (ID = {1})", cw.CUSTOMPARAMCODE_R_IWBCPV, c.CPVID);
                    }
                }
            }
        }

        private void LoadTransitIwb(PurchaseInvoiceWrapper item, List<ErrorWrapper> retMessage, IUnitOfWork uow)
        {
            if (item.TRANSITDATAL == null) 
                return;

            var errCountOld = MessageHelper.GetErrorCount(retMessage);

            var transitNames = item.TRANSITDATAL.Select(i => i.TRANSITNAME.ToUpper()).Distinct();
            var transitFilterValue = "'" + string.Join("','", transitNames) + "'";
            var transitFilter = string.Format("{0} in ({1}) and {2} = {3} and {4} = 'IWB'",
                    SourceNameHelper.Instance.GetPropertySourceName(typeof(Transit), Transit.TransitNamePropertyName), transitFilterValue,
                    Transit.MANDANTIDPropertyName, item.MANDANTID,
                    SourceNameHelper.Instance.GetPropertySourceName(typeof(Transit), Transit.TransitEntityPropertyName));

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
                            TransitDataKey = item.IWBID.ToString(),
                            TransitDataValue = tdw.TRANSITDATAVALUE,
                            TransitID = existTrn.TransitID
                        };
                        SetXmlIgnore(trDataObj, false);
                        trDataMgr.Insert(ref trDataObj);
                        Log.InfoFormat("Созданы транзитные данные накладной: «{0}» = «{1}»", tdw.TRANSITNAME, tdw.TRANSITDATAVALUE);
                    }
                }

                if (MessageHelper.GetErrorCount(retMessage) > errCountOld)
                    throw new IntegrationLogicalException("Ошибка при создании транзитных данных накладной «{0}»",
                        item.IWBNAME);
            }
        }

        private void LoadTransitIwbPos(PurchaseInvoiceWrapper item, List<ErrorWrapper> retMessage, IUnitOfWork uow)
        {
            var iwbPosFilter = string.Format("{0} = {1}",
                    SourceNameHelper.Instance.GetPropertySourceName(typeof(IWBPos), IWBPos.IWBID_RPropertyName), item.IWBID);
            var poses = new List<IWBPos>();
            using (var posMgr = IoC.Instance.Resolve<IBaseManager<IWBPos>>())
            {
                if (uow != null)
                    posMgr.SetUnitOfWork(uow);
                poses.AddRange(posMgr.GetFiltered(iwbPosFilter).ToArray());
            }

            var transitInPos = item.IWBPOSL.Where(i => i.TRANSITDATAL != null).ToArray();
            var transitNames = new List<string>();
            foreach (var tdWr in transitInPos.SelectMany(posWithTransit => posWithTransit.TRANSITDATAL.Where
                                                            (tdWr => !transitNames.Contains(tdWr.TRANSITNAME))))
            {
                transitNames.Add(tdWr.TRANSITNAME);
            }

            var transitPosFilterValue = "'" + string.Join("','", transitNames) + "'";
            var transitPosFilter = string.Format("{0} in ({1}) and {2} = {3} and {4} = 'IWBPOS'",
                    SourceNameHelper.Instance.GetPropertySourceName(typeof(Transit), Transit.TransitNamePropertyName), transitPosFilterValue,
                    Transit.MANDANTIDPropertyName, item.MANDANTID,
                    SourceNameHelper.Instance.GetPropertySourceName(typeof(Transit), Transit.TransitEntityPropertyName));

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
                                    string.Format("Не существует заголовок транзита «{0}» (позиция «{1}», артикул «{2}»)", 
                                                  tdw.TRANSITNAME, trInPos.IWBPOSNUMBER, trInPos.IWBPOSARTNAME)
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
                throw new IntegrationLogicalException("Ошибка при создании транзитных данных позиций накладной «{0}»", item.IWBNAME);

            foreach (var wrPos in item.IWBPOSL)
            {
                var firstOrDefault = poses.FirstOrDefault(i => i.IWBPosNumber.Equals(wrPos.IWBPOSNUMBER));
                if (firstOrDefault != null)
                {
                    var posId = firstOrDefault.IWBPosID;

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
                            SetXmlIgnore(trDataObj, true);
                            trDataMgr.Insert(ref trDataObj);
                            Log.InfoFormat("Созданы транзитные данные {0} позиции: «{1}» = «{2}»",
                                wrPos.IWBPOSNUMBER, wrTdata.TRANSITNAME, wrTdata.TRANSITDATAVALUE);
                        }
                    }
                }
            }
        }

        private void DelTransitIwbPos(PurchaseInvoiceWrapper item, IEnumerable<IWBPos> iwbPos, IUnitOfWork uow)
        {
            var posIds = iwbPos.Select(i => i.IWBPosID.ToString());
            var transitName = item.IWBPOSL[0].TRANSITDATAL[0].TRANSITNAME;
            var transitId = 0;

            var trFilter = string.Format("{0} = {1} and {2} = '{3}'",
                    Transit.MANDANTIDPropertyName, item.MANDANTID,
                    SourceNameHelper.Instance.GetPropertySourceName(typeof(Transit), Transit.TransitNamePropertyName), transitName);
            using (var trMgr = IoC.Instance.Resolve<IBaseManager<Transit>>())
            {
                if (uow != null)
                    trMgr.SetUnitOfWork(uow);
                var trs = trMgr.GetFiltered(trFilter).ToArray();
                if (trs.Length > 0)
                    transitId = trs[0].TransitID.To<int>();
            }

            if (transitId > 0)
            {
                var trDataDel = new List<TransitData>();
                var tranDataList =
                    FilterHelper.GetArrayFilterIn(SourceNameHelper.Instance.GetPropertySourceName(typeof (TransitData),
                    TransitData.TransitDataKeyPropertyName), posIds,
                    string.Format(" and {0} = {1}",
                    SourceNameHelper.Instance.GetPropertySourceName(typeof(TransitData),
                    TransitData.TransitIDPropertyName), transitId));
                var trDataMgr = IoC.Instance.Resolve<IBaseManager<TransitData>>();
                trDataMgr.SetUnitOfWork(uow);
                foreach (var artFilter in tranDataList)
                {
                    trDataDel.AddRange(trDataMgr.GetFiltered(artFilter).ToArray());
                }
                trDataMgr.Delete(trDataDel);
                Log.InfoFormat("Удалены транзитные данные «{0}» позиций накладной «{1}»", transitName, item.IWBNAME);
            }
        }

        private void LoadLastAttr(IWBPosWrapper iwbPosWr, IUnitOfWork uow)
        {
            using (var mgrBpProc = IoC.Instance.Resolve<BPProcessManager>())
            {
                if (uow != null)
                    mgrBpProc.SetUnitOfWork(uow);

                var retListStrings = mgrBpProc.GetLastProductAttr(iwbPosWr.SKUID_R.To<decimal>());

                var wrapperProperties = TypeDescriptor.GetProperties(typeof(IWBPosWrapper));
                foreach (var mess in retListStrings)
                {
                    var idx = mess.IndexOf('=');
                    if (idx == -1)
                        continue;

                    var propertyName = mess.Substring(0, idx);
                    var value = mess.Substring(idx + 1);
                    Log.DebugFormat("Параметр = {0}, Значение = {1}", propertyName, value);

                    var property = wrapperProperties.Find(propertyName, true);
                    if (property == null) continue;
                    var typedValue = SerializationHelper.ConvertToTrueType(value, property.PropertyType);
                    property.SetValue(iwbPosWr, typedValue);
                }
            }
        }

        private void LoadArt2GroupIwb(PurchaseInvoiceWrapper item, List<ErrorWrapper> retMessage, IUnitOfWork uow)
        {
            var errCountOld = MessageHelper.GetErrorCount(retMessage);
            var groupInPos = item.IWBPOSL.Where(i => i.GROUP2ARTL != null).ToArray();
            var artGroupCodes = new List<string>();

            foreach (var a2GWr in groupInPos.SelectMany(posWithGroup => posWithGroup.GROUP2ARTL.Where
                                                            (a2GWr => !artGroupCodes.Contains(a2GWr.ART2GROUPARTGROUPCODE))))
            {
                artGroupCodes.Add(a2GWr.ART2GROUPARTGROUPCODE);
            }

            var artGroupFilterValue = "'" + string.Join("','", artGroupCodes) + "'";
            var artGroupFilter = string.Format("{0} in ({1})",
                    SourceNameHelper.Instance.GetPropertySourceName(typeof(Art2Group), Art2Group.Art2GroupArtGroupCodePropertyName),
                    artGroupFilterValue);

            using (var a2GMgr = IoC.Instance.Resolve<IBaseManager<Art2Group>>())
            {
                if (uow != null)
                    a2GMgr.SetUnitOfWork(uow);

                foreach (var pos in item.IWBPOSL)
                {
                    var art2Groups = a2GMgr.GetFiltered(artGroupFilter).ToArray();

                    foreach (var a2GWr in pos.GROUP2ARTL)
                    {
                        var existAg = art2Groups.FirstOrDefault(i => a2GWr.ART2GROUPARTGROUPCODE.Equals(i.Art2GroupArtGroupCode) &&
                                                                     pos.IWBPOSARTCODE.Equals(i.Art2GroupArtCode));
                        if (existAg != null)
                        {
                            Log.InfoFormat("Привязка артикула «{0}» к группе «{1}» уже существует",
                                pos.IWBPOSARTNAME, a2GWr.ART2GROUPARTGROUPCODE);
                        }
                        else
                        {
                            var existAgPriority = art2Groups.FirstOrDefault(i => i.Art2GroupPriority == a2GWr.ART2GROUPPRIORITY &&
                                                                     pos.IWBPOSARTCODE.Equals(i.Art2GroupArtCode));
                            if (existAgPriority == null)
                            {
                                var art2Group = new Art2Group
                                {
                                    Art2GroupArtCode = pos.IWBPOSARTCODE,
                                    Art2GroupArtGroupCode = a2GWr.ART2GROUPARTGROUPCODE,
                                    Art2GroupPriority = a2GWr.ART2GROUPPRIORITY
                                };
                                a2GMgr.Insert(ref art2Group);
                                Log.InfoFormat("Добавлена привязка артикула «{0}» к группе «{1}» (приоритет = {2})",
                                    pos.IWBPOSARTNAME, a2GWr.ART2GROUPARTGROUPCODE, a2GWr.ART2GROUPPRIORITY);
                            }
                            else
                            {
                                var ew = new ErrorWrapper
                                {
                                    ERRORCODE = MessageHelper.ErrorCode.ToString(),
                                    ERRORMESSAGE = string.Format("Существует привязка артикула «{0}» с приоритетом «{1}» (группа «{2}»)",
                                    pos.IWBPOSARTNAME, a2GWr.ART2GROUPPRIORITY, a2GWr.ART2GROUPARTGROUPCODE)
                                };
                                retMessage.Add(ew);
                            }
                        }
                    }
                }
                if (MessageHelper.GetErrorCount(retMessage) > errCountOld)
                    throw new IntegrationLogicalException("Ошибка привязки позиций к группам артикулов (накладная «{0}»)", item.IWBNAME);
            }
        }

        private void DelArt2GroupIwb(IEnumerable<string> artCodes, string delArt2Group, IUnitOfWork uow)
        {
            var art2GroupList = FilterHelper.GetArrayFilterIn("ARTCODE_R", artCodes,
                string.Format(" and ({0} in ({1}))",
                    SourceNameHelper.Instance.GetPropertySourceName(typeof(Art2Group),
                        Art2Group.Art2GroupPriorityPropertyName),
                    delArt2Group));
            var a2Gs = new List<Art2Group>();

            using (var a2GMgr = IoC.Instance.Resolve<IBaseManager<Art2Group>>())
            {
                if (uow != null)
                    a2GMgr.SetUnitOfWork(uow);

                foreach (var art2GroupFilter in art2GroupList)
                {
                    a2Gs.AddRange(a2GMgr.GetFiltered(art2GroupFilter).ToArray());
                }
                a2GMgr.Delete(a2Gs);
            }
        }

        /// <summary>
        /// Загрузка Cpv для IwbPos. Перед загрузкой все Cpv должны быть удалены.
        /// </summary>
        private string[] LoadIwbPosCpv(IWBPosWrapper item, string iwbname, IUnitOfWork uow)
        {
            Log.Debug("Загрузка CPV для позиций приходной накладной.");
            var loadentityname = item.IWBPOSNUMBER;
            List<string> messages;
            var wmscpvs = CreateIwbPosCpv(item, iwbname, uow, out messages);
            if (wmscpvs != null && wmscpvs.Any())
            {
                var cpvkey = item.IWBPOSID.HasValue ? item.IWBPOSID.ToString() : null;
                var cpvHelper = new CpvHelper<IWBPosCpv>(ReceiptLoadHelper.Cpv2EntityIwbPosName, cpvkey);

                cpvHelper.Save(source: wmscpvs.ToArray(), allowUpdate: true, includeCpvWithDafaultValue: false,
                    verify: false,
                    uow: uow);
                Log.DebugFormat("Для позиции '{0}' приходной накладной '{1}' загружены CPV.", loadentityname, iwbname);
            }

            return messages.ToArray();
        }

        private IWBPosCpv[] CreateIwbPosCpv(IWBPosWrapper item, string iwbname, IUnitOfWork uow, out List<string> messages)
        {
            messages = new List<string>();
            var loadentityname = item.IWBPOSNUMBER;
            if (item.CUSTOMPARAMVAL == null || item.CUSTOMPARAMVAL.Count == 0)
            {
                //var message = string.Format("Позиция '{0}' приходной накладной '{1}' не содержит CPV.", loadentityname,
                //    iwbname);
                //Log.Debug(message);
                return null;
            }

            var cpvkey = item.IWBPOSID.HasValue ? item.IWBPOSID.ToString() : null;
            var wmscpvs = new List<IWBPosCpv>();
            var cpvid = CpvHelper.GetMinCpvId(item.CUSTOMPARAMVAL.Select(p => p.CPVID_IWBPOSCPV).ToArray());
            var cpvHelper = new CpvHelper<IWBPosCpv>(ReceiptLoadHelper.Cpv2EntityIwbPosName, cpvkey);

            //Получаем все cp
            var allcp = cpvHelper.GetAllCp(null);
            if (allcp.Length == 0)
            {
                var message = string.Format("Для сущности '{0}' CP не определены.", ReceiptLoadHelper.Cpv2EntityIwbPosName);
                messages.Add(message);
                Log.Error(message);
                return null;
            }

            Func<string, List<string>, CustomParam> findCpHandler = (code, errmessages) =>
            {
                var wmscp = allcp.FirstOrDefault(p => p.GetKey<string>().EqIgnoreCase(code) && p.CustomParam2Entity == ReceiptLoadHelper.Cpv2EntityIwbPosName);
                if (wmscp == null)
                {
                    var message =
                        string.Format("Не найден CP но коду '{0}' (сущность '{1}') для позиции '{2}' расходной накладной '{3}'.", code,
                            ReceiptLoadHelper.Cpv2EntityIwbPosName, loadentityname, iwbname);
                    errmessages.Add(message);
                }
                return wmscp;
            };

            Func<decimal?, bool> hasChildrenHandler = cpvwid =>
            {
                return cpvwid.HasValue && item.CUSTOMPARAMVAL.Any(p => p.CPVPARENT_IWBPOSCPV == cpvwid);
            };

            foreach (var cw in item.CUSTOMPARAMVAL)
            {
                var customParamCode = cw.CUSTOMPARAMCODE_R_IWBPOSCPV;

                //Проверяем существование такого cp
                var cp = findCpHandler(customParamCode, messages);
                if (cp == null)
                    continue;

                //Нет валидации

                //Проверяем, если значение == null и нет детей, то не добавляем такой CPV
                if (string.IsNullOrEmpty(cw.CPVVALUE_IWBPOSCPV) && !hasChildrenHandler(cw.CPVID_IWBPOSCPV))
                    continue;

                var cpv = new IWBPosCpv();
                cpv = MapTo(cw, cpv);

                //HACK: Не заполненный CPVParent
                if (cpv.CPVID != -1 && !cpv.CPVParent.HasValue)
                    cpv.CPVParent = -1;
                if (!cpv.CPVID.HasValue)
                    cpv.CPVID = --cpvid;
                cpv.CPV2Entity = ReceiptLoadHelper.Cpv2EntityIwbPosName;
                cpv.CPVKey = cpvkey;

                wmscpvs.Add(cpv);
            }

            return wmscpvs.ToArray();
        }
    }
}