using System;
using System.Collections.Concurrent;
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
        //Использовать только в методе PartnerLoad
        private static readonly ConcurrentDictionary<decimal, object> LockPartnerLoadPerMandant = new ConcurrentDictionary<decimal, object>();

        public ErrorWrapper[] PartnerLoad(PartnerWrapper item)
        {
            Contract.Requires(item != null);

            var retMessage = new List<ErrorWrapper>();
            Log.InfoFormat("Start of PartnerLoad");
            Log.Debug(item.DumpToXML());
            var startAllTime = DateTime.Now;

            try
            {
                IUnitOfWork uow = null;
                try
                {
                    uow = UnitOfWorkHelper.GetUnit();

                    item.MANDANTID = CheckMandant(item.MANDANTID, item.MandantCode, uow);
                    if (item.MANDANTID == null) 
                        throw new NullReferenceException("MandantCode");
                    Log.DebugFormat("Мандант = {0}", item.MandantCode);

                    var isPermit = item.PARTNERLOCKED;
                    if (isPermit) 
                        item.PARTNERLOCKED = false;

                    var lockobj = LockPartnerLoadPerMandant.GetOrAdd(item.MANDANTID.Value, new object());

                    lock (lockobj)
                    {
                        Log.InfoFormat("Process PartnerLoad locked = {0:hh:mm:ss.fff}", DateTime.Now);

                        uow.BeginChanges();

                        //var partner = new Partner();
                        var partnerType = typeof(Partner);
                        var partnername = FilterHelper.ValidateFilterString(item.PARTNERNAME);
                        var partnerFilter = string.Format("{0} = {1} AND (upper({2}) = '{3}' or upper({4}) = '{5}')",
                            SourceNameHelper.Instance.GetPropertySourceName(partnerType, Partner.MANDANTIDPropertyName),
                            item.MANDANTID,
                            SourceNameHelper.Instance.GetPropertySourceName(partnerType, Partner.PARTNERNAMEPropertyName),
                            string.IsNullOrEmpty(partnername) ? null : partnername.ToUpper(),
                            SourceNameHelper.Instance.GetPropertySourceName(partnerType, Partner.PARTNERHOSTREFPropertyName),
                            string.IsNullOrEmpty(item.PARTNERHOSTREF) ? null : item.PARTNERHOSTREF.ToUpper());

                        //Получим не пустые адреса, исключая ADR_CLIENTRECIPIENT
                        var addresstypes = AddressHelper.GetAddressTypes(AddressBookType.ADR_CLIENTRECIPIENT);
                        var wrapperaddrs = AddressHelper.GetNotEmptyAddressBookByTypes(item.ADDRESS, addresstypes);

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

                        using (var mgr = IoC.Instance.Resolve<IBaseManager<Partner>>())
                        {
                            mgr.SetUnitOfWork(uow);

                            var partners = mgr.GetFiltered(partnerFilter).ToArray();
                            decimal? partnerId;
                            if (partners.Length == 0)
                                partnerId = PartnerLoadInternal(item, wmsaddresscol, retMessage, mgr, uow);
                            else
                                partnerId = PartnerUpdateInternal(item, partners, wmsaddresscol, isPermit, retMessage, mgr, uow);

                            if (partnerId.HasValue)
                            {
                                if (!ProcessPartnerGroup(item, partnerId.Value, uow))
                                    Log.DebugFormat("Для партнера «{0}» не требуется привязка к группе",
                                        item.PARTNERNAME);
                            }
                        }
                        uow.CommitChanges();
                        Log.InfoFormat("Process PartnerLoad unlocked = {0:hh:mm:ss.fff}", DateTime.Now);
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
                    throw;
                }
                catch (Exception ex)
                {
                    if (uow != null)
                        uow.RollbackChanges();

                    var message = ExceptionHelper.ExceptionToString(ex);
                    Log.Error(message, ex);

                    var ew = new ErrorWrapper { ERRORCODE = MessageHelper.ErrorCode.ToString(), ERRORMESSAGE = "Системная ошибка: " + message };
                    retMessage.Add(ew);
                    throw;
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
                Log.DebugFormat("PartnerLoad - общее время загрузки {0}", (DateTime.Now - startAllTime));
                Log.InfoFormat("End of PartnerLoad");
            }
            return retMessage.ToArray();
        }

        private decimal? PartnerLoadInternal(PartnerWrapper item, ICollection<AddressBook> wmsaddresscol, List<ErrorWrapper> retMessage, IBaseManager<Partner> mgr, IUnitOfWork uow)
        {
            var partner = new Partner();
            partner = MapTo(item, partner);

            if (string.IsNullOrEmpty(partner.PartnerName) && string.IsNullOrEmpty(partner.PartnerFullName))
            {
                const string message = "Партнер не создан. Наименование партнера не может быть пустым.";
                retMessage.Add(new ErrorWrapper
                {
                    ERRORCODE = MessageHelper.WarningCode.ToString(),
                    ERRORMESSAGE = message
                });
                Log.Info(message);
                return null;
            }

            AddOrUpdatePartnerAddress(partner, wmsaddresscol, item.DONOTUPDATELEGALADDRESS > 0);

            SetXmlIgnore(partner, false);
            mgr.Insert(ref partner);
            Log.InfoFormat("Загружен партнер «{0}» с ID = {1}", item.PARTNERNAME, partner.PartnerId);

            var ewr = new ErrorWrapper
            {
                ERRORCODE = MessageHelper.SuccessCode.ToString(),
                ERRORMESSAGE = string.Format("Загружен партнер «{0}»", item.PARTNERNAME)
            };
            retMessage.Add(ewr);

            if (item.GPVs != null)
            {
                using (var gpvMgr = IoC.Instance.Resolve<IBaseManager<PartnerGpv>>())
                {
                    gpvMgr.SetUnitOfWork(uow);
                    foreach (var gpvObj in item.GPVs)
                    {
                        var gpv = new PartnerGpv();
                        gpv = MapTo(gpvObj, gpv);
                        gpv.GParamValKey = partner.PartnerId.To<decimal>();
                        SetXmlIgnore(gpv, false);
                        gpvMgr.Insert(ref gpv);
                        Log.InfoFormat("Загружена GPV {0} (ID = {1})", gpv.GlobalParamCode_R, gpv.GParamID);
                    }
                }
            }

            return partner.PartnerId;
        }

        private decimal? PartnerUpdateInternal(PartnerWrapper item, Partner[] partners, ICollection<AddressBook> wmsaddresscol, bool isPermit, List<ErrorWrapper> retMessage, IBaseManager<Partner> mgr, IUnitOfWork uow)
        {
            var partner = new Partner();
            decimal partnerId = 0;
            var isHostRef = false;
            var donotupdatelegaladdress = item.DONOTUPDATELEGALADDRESS > 0;

            if (item.PARTNERHOSTREFNAME == 1)
            {
                var pHostRefName = partners.Where(i => i.PartnerHostRef.EqIgnoreCase(item.PARTNERHOSTREF) && i.PartnerName.EqIgnoreCase(item.PARTNERNAME)).ToArray();
                if (pHostRefName.Length > 0)
                {
                    partner = pHostRefName[0];
                    if (item.PARTNERCREATEBYHOSTREF > 1)
                        isHostRef = true;
                }
                else
                {
                    if (item.PARTNERCREATEBYHOSTREF > 1)
                    {
                        var pHostRefs = partners.Where(i => i.PartnerHostRef.EqIgnoreCase(item.PARTNERHOSTREF)).ToArray();
                        if (pHostRefs.Length > 0)
                        {
                            if (item.PARTNERCREATEBYHOSTREF == 2)
                            {
                                partner = pHostRefs[0];
                                isHostRef = true;
                            }
                            if (item.PARTNERCREATEBYHOSTREF == 3)
                                isPermit = true;                                 
                        }
                    }
                }
            }
            else
            {
                var pHostRefs = partners.Where(i => i.PartnerHostRef.EqIgnoreCase(item.PARTNERHOSTREF)).ToArray();
                if (pHostRefs.Length > 0)
                {
                    partner = pHostRefs[0];
                    isHostRef = true;
                }

                if (pHostRefs.Length == 0)
                {
                    if (!item.PARTNERCREATEBYHOSTREF.HasValue || item.PARTNERCREATEBYHOSTREF == 0 || item.PARTNERCREATEBYHOSTREF == 3)
                    {
                        var pNames = partners.Where(i => i.PartnerName.ToUpper() == item.PARTNERNAME.ToUpper()).ToArray();
                        if (pNames.Length > 0)
                            partner = pNames[0];
                        Log.InfoFormat("Найден партнер «{0}» с ID = {1}", item.PARTNERNAME, partner.PartnerId);
                    }
                }
            }

            if (!isPermit)
            {
                if (isHostRef || !item.PARTNERCREATEBYHOSTREF.HasValue || item.PARTNERCREATEBYHOSTREF == 0)
                {
                    if (!string.IsNullOrEmpty(item.PARTNERNAME))
                        partner = MapTo(item, partner);

                    AddOrUpdatePartnerAddress(partner, wmsaddresscol, donotupdatelegaladdress);

                    if (partner.IsDirty)
                    {
                        SetXmlIgnore(partner, true);
                        mgr.Update(partner);

                        Log.InfoFormat("Обновлен партнер «{0}» {1}", item.PARTNERNAME, DateTime.Now.ToString("hh:mm:ss.fff"));
                        var ewr = new ErrorWrapper
                        {
                            ERRORCODE = MessageHelper.SuccessCode.ToString(),
                            ERRORMESSAGE = string.Format("Обновлен партнер «{0}»", item.PARTNERNAME)
                        };
                        retMessage.Add(ewr);
                    }
                    else
                    {
                        Log.DebugFormat("Обновление партнера «{0}» не требуется", item.PARTNERNAME);
                        var ewr = new ErrorWrapper
                        {
                            ERRORCODE = MessageHelper.SuccessCode.ToString(),
                            ERRORMESSAGE = string.Format("Обновление партнера «{0}» не требуется", item.PARTNERNAME)
                        };
                        retMessage.Add(ewr);
                    }
                    partnerId = partner.PartnerId.To<decimal>();
                }
                else
                {
                    partner = MapTo(item, partner);
                    AddOrUpdatePartnerAddress(partner, wmsaddresscol, donotupdatelegaladdress);
                    SetXmlIgnore(partner, false);
                    mgr.Insert(ref partner);
                    partnerId = partner.PartnerId.To<decimal>();
                    Log.InfoFormat("Загружен партнер «{0}» (код «{1}») с ID = {2}", item.PARTNERNAME, item.PARTNERHOSTREF, partnerId);
                    var ewr = new ErrorWrapper
                    {
                        ERRORCODE = MessageHelper.SuccessCode.ToString(),
                        ERRORMESSAGE = string.Format("Загружен партнер «{0}» (код «{1}»)", item.PARTNERNAME, item.PARTNERHOSTREF)
                    };
                    retMessage.Add(ewr);
                }
            }
            else
            {
                Log.DebugFormat("Партнер «{0}» существует", item.PARTNERNAME);
                var ewr = new ErrorWrapper
                {
                    ERRORCODE = MessageHelper.SuccessCode.ToString(),
                    ERRORMESSAGE = string.Format("Партнер «{0}» существует", item.PARTNERNAME)
                };
                retMessage.Add(ewr);
            }

            if (item.GPVs != null)
            {
                ProcessLoadGpv(item, partnerId, uow);
            }

            return partner.PartnerId;
        }

        private void ProcessLoadGpv(PartnerWrapper item, decimal partnerId, IUnitOfWork uow)
        {
            using (var gpvMgr = IoC.Instance.Resolve<IBaseManager<PartnerGpv>>())
            {
                gpvMgr.SetUnitOfWork(uow);

                var valuesGpv = "'" + string.Join("','", item.GPVs.Select(i => i.GLOBALPARAMCODE_R_PARTNERGPV)) + "'";

                var gpvFilter = string.Format("{0} = '{1}' and {2} = 'PARTNER' and {3} in ({4})",
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(PartnerGpv), PartnerGpv.GParamValKeyPropertyName), partnerId,
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(PartnerGpv), PartnerGpv.GParamVal2EntityPropertyName),
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(PartnerGpv), PartnerGpv.GlobalParamCodePropertyName), valuesGpv);
                var gpvs = gpvMgr.GetFiltered(gpvFilter).ToArray();

                foreach (var gpvWrapper in item.GPVs)
                {
                    if (gpvWrapper.GPARAMVALVALUE_PARTNERGPV == "1")
                    {
                        if (gpvs.Any(i => i.GlobalParamCode_R == gpvWrapper.GLOBALPARAMCODE_R_PARTNERGPV && i.GparamValValue == "1"))
                        {
                            Log.DebugFormat("Существует требуемый GPV «{0}»", gpvWrapper.GLOBALPARAMCODE_R_PARTNERGPV);
                        }
                        else
                        {
                            var existGpv = gpvs.FirstOrDefault(i => i.GlobalParamCode_R == gpvWrapper.GLOBALPARAMCODE_R_PARTNERGPV);
                            var gpv = new PartnerGpv();
                            if (existGpv != null)
                            {
                                gpv = existGpv;
                                gpv.GparamValValue = "1";
                                SetXmlIgnore(gpv, true);
                                gpvMgr.Update(gpv);
                                Log.InfoFormat("Обновлен GPV «{0}» (ID = {1})", gpv.GlobalParamCode_R, gpv.GParamID);
                            }
                            else
                            {
                                gpv = MapTo(gpvWrapper, gpv);
                                gpv.GParamValKey = partnerId;
                                SetXmlIgnore(gpv, false);
                                gpvMgr.Insert(ref gpv);
                                Log.InfoFormat("Загружена GPV «{0}» (ID = {1})", gpv.GlobalParamCode_R, gpv.GParamID);
                            }
                        }
                    }
                    else
                    {
                        if (gpvs.Any(i => i.GlobalParamCode_R == gpvWrapper.GLOBALPARAMCODE_R_PARTNERGPV))
                        {
                            Log.DebugFormat("Существует требуемый GPV «{0}»", gpvWrapper.GLOBALPARAMCODE_R_PARTNERGPV);
                        }
                        else
                        {
                            var gpv = new PartnerGpv();
                            gpv = MapTo(gpvWrapper, gpv);
                            gpv.GParamValKey = partnerId;
                            SetXmlIgnore(gpv, false);
                            gpvMgr.Insert(ref gpv);
                            Log.InfoFormat("Загружена GPV «{0}» (ID = {1})", gpv.GlobalParamCode_R, gpv.GParamID);
                        }
                    }
                }
            }
        }

        private bool ProcessPartnerGroup(PartnerWrapper item, decimal partnerId, IUnitOfWork uow)
        {
            if (string.IsNullOrEmpty(item.PARTNERGROUPCODE) && string.IsNullOrEmpty(item.PARTNERGROUPHOSTREF))
                return false;

            var partnergrouphostrefisnotempty = !string.IsNullOrEmpty(item.PARTNERGROUPHOSTREF);
            var partnerGroupType = typeof (PartnerGroup);
            var pgFilter = string.Format("{0} = '{1}' or {2} = '{3}'",
                SourceNameHelper.Instance.GetPropertySourceName(partnerGroupType,
                    PartnerGroup.PARTNERGROUPNAMEPropertyName), item.PARTNERGROUPCODE,
                SourceNameHelper.Instance.GetPropertySourceName(partnerGroupType,
                    PartnerGroup.PARTNERGROUPHOSTREFPropertyName), item.PARTNERGROUPHOSTREF);

            PartnerGroup[] pgs;
            using (var pgMgr = IoC.Instance.Resolve<IBaseManager<PartnerGroup>>())
            {
                if (uow != null)
                    pgMgr.SetUnitOfWork(uow);
                pgs = pgMgr.GetFiltered(pgFilter, GetModeEnum.Partial).ToArray();
            }

            if (pgs.Length == 0)
            {
                if (partnergrouphostrefisnotempty)
                    throw new IntegrationLogicalException(
                        string.Format("Группа партнёров с хост-идентификатором '{0}' не найдена.", item.PARTNERGROUPHOSTREF));

                Log.DebugFormat("Не существует группа партнеров с PARTNERGROUPCODE '{0}'.", item.PARTNERGROUPCODE);
            }
            else
            {
                if (pgs.Length > 1 && partnergrouphostrefisnotempty)
                    throw new IntegrationLogicalException(
                        string.Format("Найдено несколько групп партнёров с хост-идентификатором '{0}'.", item.PARTNERGROUPHOSTREF));

                var partner2GroupType = typeof (Partner2Group);
                var p2GFilter = string.Format("{0} = {1} and {2} = {3}",
                 SourceNameHelper.Instance.GetPropertySourceName(partner2GroupType,
                     Partner2Group.PARTNERGROUPIDPropertyName), pgs[0].PartnerGroupId,
                 SourceNameHelper.Instance.GetPropertySourceName(partner2GroupType,
                     Partner2Group.PARTNERIDPropertyName), partnerId);

                using (var p2GMgr = IoC.Instance.Resolve<IBaseManager<Partner2Group>>())
                {
                    if (uow != null)
                        p2GMgr.SetUnitOfWork(uow);
             
                    var p2Gs = p2GMgr.GetFiltered(p2GFilter, GetModeEnum.Partial).ToArray();

                    if (p2Gs.Length == 0)
                    {
                        var p2GObj = new Partner2Group
                        {
                            PartnerGroupId = pgs[0].PartnerGroupId,
                            PartnerId = partnerId
                        };

                        SetXmlIgnore(p2GObj, true);
                        p2GMgr.Insert(ref p2GObj);

                        Log.InfoFormat("Создана привязка к группе партнеров ид. '{0}'", p2GObj.PartnerGroupId);
                    }
                    else
                    {
                        Log.DebugFormat("Существует привязка к группе партнеров {0}", item.PARTNERGROUPCODE);
                    }

                    if (partnergrouphostrefisnotempty)
                    {
                        //Проверяем, связан ли партнёр с другой группой, имеющей не пустой хостидентификатор
                        p2GFilter = string.Format(
                            "wmspartner2group.PARTNERGROUPID_R in (select distinct(g.PARTNERGROUPID) from wmspartnergroup g where G.PARTNERGROUPHOSTREF is not null" +
                                " and G.PARTNERGROUPHOSTREF <> '{0}')" +
                                    " and WMSPARTNER2GROUP.PARTNERID_R = {1}", item.PARTNERGROUPHOSTREF, partnerId);

                        p2Gs = p2GMgr.GetFiltered(p2GFilter, GetModeEnum.Partial).ToArray();
                        if (p2Gs.Length > 0)
                        {
                            p2GMgr.Delete(p2Gs);
                            Log.DebugFormat("Удалены следующие привязки партнера '{0}' к группе: {1}", partnerId,
                                string.Join(",", p2Gs.Select(p => string.Format("'{0}'", p.PartnerGroupId))));
                        }
                    }
                }
            }

            return true;
        }
    }
}
