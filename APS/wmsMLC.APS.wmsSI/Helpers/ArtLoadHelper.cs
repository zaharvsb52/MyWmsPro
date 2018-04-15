using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using wmsMLC.APS.wmsSI.Wrappers;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.DAL;

namespace wmsMLC.APS.wmsSI.Helpers
{
    public static class ArtLoadHelper
    {
        private const string CpEntity = "ART";
        private const string CpCodeNameArtPartnerHostRef = "ARTPartnerHostRef";
        private const string CpCodeNameArtPartner = "ARTPartner";
        public const string DefaultArtAbcd = "C";

        private static Dictionary<string, bool> _artAbcd;

        /// <summary>
        /// Добавление CPV. 
        /// </summary>
        public static string[] LoadCpv(ArtWrapper item, string cpSource, string cpTarget, IUnitOfWork uow, ILog log)
        {
            //Внимание! Загрузка CPV в предположении, что уровень вложенности - 0, одна строка с ИП - один CPV.
            log.Debug("Загрузка CPV для артикула.");

            if (item == null)
                throw new ArgumentNullException("item");

            var messages = new List<string>();
            var loadentityname = item.ARTNAME;

            if (item.CUSTOMPARAMVAL == null || item.CUSTOMPARAMVAL.Count == 0)
            {
                log.DebugFormat("Артикул '{0}' не содержит CPV", loadentityname);
                return messages.ToArray();
            }

            if (string.IsNullOrEmpty(item.ARTCODE))
                throw new DeveloperException("Свойство Artcode артикула '{0}' неопределено. Загрузка CPV невозможна.", loadentityname);

            //Проверяем cpv
            var cpvwraps = item.CUSTOMPARAMVAL.Where(p => !string.IsNullOrEmpty(p.CUSTOMPARAMCODE_R_ARTCPV)).ToArray();
            if (cpvwraps.Length == 0)
            {
                var message = string.Format("Свойство CUSTOMPARAMCODE не определено в коллекции CPV артикула '{0}'.", loadentityname);
                messages.Add(message);
                log.Error(message);
                return messages.ToArray();
            }

            //Получаем список cpv
            var cpvHelper = new CpvHelper<ArtCpv>(CpEntity, item.ARTCODE);
            var wmscpvs = cpvHelper.GetAllCpv(uow);
            if (wmscpvs.Length == 0)
            {
                var message = string.Format("Для сущности '{0}' CP не определены.", CpEntity);
                messages.Add(message);
                log.Error(message);
                return messages.ToArray();
            }

            Func<string, List<ArtCpv>> findCpvHandler = code =>
            {
                var findcpvs = wmscpvs.Where(p => p.CustomParamCode.EqIgnoreCase(code)).ToList();
                if (findcpvs.Count == 0)
                {
                    var message = string.Format("Не найден cpv '{0}' для артикула '{1}'.", code, loadentityname);
                    throw new OperationException(message);
                }
                return findcpvs;
            };

            foreach (var cpvw in cpvwraps)
            {
                var customParamCode = cpvw.CUSTOMPARAMCODE_R_ARTCPV;
                
                //Ищем в wmscpvs параметр
                var wmscpvl = findCpvHandler(customParamCode);
                var cpvvalue = cpvw.CPVVALUE_ARTCPV;
                var isCpCodeArtPartner = CpCodeNameArtPartner.EqIgnoreCase(customParamCode);

                //HACK: проверяем CPV ARTPartner или ARTPartnerHostRef
                if (isCpCodeArtPartner || CpCodeNameArtPartnerHostRef.EqIgnoreCase(customParamCode))
                {
                    if (!string.IsNullOrEmpty(cpvvalue))
                    {
                        var partners = GetPartnerByMandantByNamesByHostRef(mandantid: item.MANDANTID,
                            partnerName: cpvvalue, partnerHostRef: cpvvalue, partnerCode: null, partnerFullName: null,
                            uow: uow);
                        if (partners.Length == 0)
                        {
                            var message = string.Format("Не найден Поставщик по значению '{0}' в cpv '{1}' для артикула '{2}'.", cpvvalue, customParamCode, loadentityname);
                            throw new IntegrationService.IntegrationLogicalException(message);
                            //messages.Add(message);
                        }

                        string spartnerid;
                        if (partners.Length == 1)
                        {
                            spartnerid = partners[0].GetKey<decimal>().ToString(CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            var maxPartnerId = partners.Max(p => p.GetKey<decimal>());
                            spartnerid = partners.Select(p => p.GetKey<decimal>()).First(p => p == maxPartnerId).ToString(CultureInfo.InvariantCulture);

                            //var message = string.Format("Найдено несколько Поставщиков по значению '{0}' в cpv '{1}' для артикула '{2}'", cpvvalue, customParamCode, loadentityname);
                            //throw new IntegrationService.IntegrationLogicalException(message);
                            ////messages.Add(message);
                        }

                        //Добавляем ид. партнера в cpv 'ARTPartner'
                        if (isCpCodeArtPartner)
                        {
                            cpvvalue = spartnerid;
                        }
                        else
                        {
                            var cpvArtPartnerl = findCpvHandler(CpCodeNameArtPartner);
                            cpvArtPartnerl.ForEach(p => p.CPVValue = spartnerid);
                        }
                    }
                }

                //Обновляем значение
                wmscpvl.ForEach(p => p.CPVValue = cpvvalue);
            }

            //Сохраняем параметр
            if (wmscpvs.Any())
            {
                cpvHelper.Save(source: wmscpvs, allowUpdate: item.ARTUPDATE == 1, includeCpvWithDafaultValue: false,
                    verify: false, uow: uow);
                log.DebugFormat("Для артикула '{0}' загружены CPV.", loadentityname);
            }

            return messages.ToArray();
        }

        public static string[] FillArtManufacturer(ArtWrapper item, IUnitOfWork uow, ILog log, int partnerNameFieldMaxLength)
        {
            var messages = new List<string>();

            var value = item.ARTMANUFACTURERCODE;
            if (string.IsNullOrEmpty(value))
                return messages.ToArray();

            var partners = GetPartnerByMandantByNamesByHostRef(mandantid: item.MANDANTID, partnerName: value,
                partnerHostRef: value, partnerCode: null, partnerFullName: value, uow: uow);

            if (partners.Length == 0)
            {
                if (item.CREATEMANUFACTURER.HasValue)
                {
                    //Создаём производителя
                    var partnerObj = new Partner
                    {
                        MandantId = item.MANDANTID,
                        PartnerFullName = value
                    };
                    var name = value;
                    if (name.Length > partnerNameFieldMaxLength)
                        name = name.Substring(0, partnerNameFieldMaxLength);
                    partnerObj.PartnerName = name;

                    using (var partMgr = IoC.Instance.Resolve<IBaseManager<Partner>>())
                    {
                        if (uow != null)
                            partMgr.SetUnitOfWork(uow);
                        partMgr.Insert(ref partnerObj);
                    }

                    partners = new[] {partnerObj};
                }
                else
                {
                    var message =
                        string.Format(
                            "Не найден Производитель по значению '{0}' в ARTMANUFACTURERCODE для артикула '{1}'.", value,
                            item.ARTNAME);
                    messages.Add(message);
                    log.Error(message);
                }
            }

            if (partners.Length > 1)
            {
                var message = string.Format("Найдено несколько Производителей по значению '{0}' в ARTMANUFACTURERCODE для артикула '{1}'.", value, item.ARTNAME);
                messages.Add(message);
                log.Error(message);
            }

            item.ARTMANUFACTURER = partners.Select(p => p.GetKey<decimal?>()).FirstOrDefault();

            return messages.ToArray();
        }

        public static Partner[] GetPartnerByMandantByNamesByHostRef(decimal? mandantid, string partnerName, string partnerHostRef, string partnerCode, string partnerFullName, IUnitOfWork uow)
        {
            var filterOrs = new List<string>();
            var partnerType = typeof(Partner);
            if (!string.IsNullOrEmpty(partnerName))
                filterOrs.Add(string.Format("Upper({0}) = '{1}'",
                    SourceNameHelper.Instance.GetPropertySourceName(partnerType, Partner.PARTNERNAMEPropertyName),
                    partnerName.ToUpper()));
            if (!string.IsNullOrEmpty(partnerHostRef))
                filterOrs.Add(string.Format("Upper({0}) = '{1}'",
                    SourceNameHelper.Instance.GetPropertySourceName(partnerType, Partner.PARTNERHOSTREFPropertyName),
                    partnerHostRef.ToUpper()));
            if (!string.IsNullOrEmpty(partnerCode))
                filterOrs.Add(string.Format("Upper({0}) = '{1}'",
                    SourceNameHelper.Instance.GetPropertySourceName(partnerType, Partner.PARTNERCODEPropertyName),
                    partnerCode.ToUpper()));
            if (!string.IsNullOrEmpty(partnerFullName))
                filterOrs.Add(string.Format("Upper({0}) = '{1}'",
                    SourceNameHelper.Instance.GetPropertySourceName(partnerType, Partner.PARTNERFULLNAMEPropertyName),
                    partnerFullName.ToUpper()));

            if (filterOrs.Count == 0)
                throw new ArgumentNullException("All arguments in GetPartnerByMandantByNameByHostRef are null.");

            var filter = string.Format("{0} = {1} AND ({2})",
                SourceNameHelper.Instance.GetPropertySourceName(partnerType, Partner.MANDANTIDPropertyName),
                mandantid,
                string.Join(" OR ", filterOrs));  

            using (var mgr = IoC.Instance.Resolve<IBaseManager<Partner>>())
            {
                if (uow != null)
                    mgr.SetUnitOfWork(uow);

                return mgr.GetFiltered(filter, GetModeEnum.Partial).ToArray();
            }
        }

        public static string[] FillCountry(ArtWrapper item, IUnitOfWork uow, ILog log)
        {
            var messages = new List<string>();
            var value = item.COUNTRYCODE_R;
            if (string.IsNullOrEmpty(value))
                return messages.ToArray();
            
            var iserror = false;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<IsoCountry>>())
            {
                if (uow != null)
                    mgr.SetUnitOfWork(uow);

                switch (value.Length)
                {
                    case 2: //COUNTRYALPHA2
                        var filter = string.Format("{0} = '{1}'",
                            SourceNameHelper.Instance.GetPropertySourceName(typeof(IsoCountry), IsoCountry.COUNTRYALPHA2PropertyName),
                            value);
                        var counries = mgr.GetFiltered(filter, GetModeEnum.Partial).ToArray();
                        if (counries.Length == 0)
                        {
                            iserror = true;
                            break;
                        }
                        item.COUNTRYCODE_R = counries[0].GetKey<string>();
                        break;
                    default:
                        var country = mgr.Get(value, GetModeEnum.Partial);
                        if (country == null)
                            iserror = true;
                        break;
                }
            }

            if (iserror)
            {
                item.COUNTRYCODE_R = null;
                var message = string.Format("Не найдена страна происхождения (COUNTRYCODE_R) '{0}' для артикула '{1}'.", value, item.ARTNAME);
                messages.Add(message);
                log.Error(message);
            }

            return messages.ToArray();
        }

        /// <summary>
        /// Правило заполнения свойства ARTABCD. 
        /// см. http://mp-ts-nwms/issue/wmsMLC-10817.
        /// </summary>
        public static string[] FillArtAbcd(Art art, string wrapperArtAbcd, string artName, ILog log)
        {
            if (art == null)
                throw new ArgumentNullException("art");

            var messages = new List<string>();

            if (string.IsNullOrEmpty(wrapperArtAbcd))
            {
                if (string.IsNullOrEmpty(art.ARTABCD))
                    art.ARTABCD = DefaultArtAbcd;
            }
            else
            {
                //Проверяем, значение wrapperArtAbcd
                if (ExistsArtAbcd(wrapperArtAbcd))
                {
                    art.ARTABCD = wrapperArtAbcd;
                }
                else
                {
                    var message = string.Format("Не найден ABCD-критерий '{0}' для артикула '{1}'.", wrapperArtAbcd, artName);    
                    if (string.IsNullOrEmpty(art.ARTABCD))
                    {
                        art.ARTABCD = DefaultArtAbcd;
                        message = string.Format("{0} Свойство ABCD-критерий артикула (ARTABCD) будет установлено в значение по-умолчанию '{1}'.", message, DefaultArtAbcd);    
                    }
                    else
                    {
                        message = string.Format("{0} Значение свойства ABCD-критерий артикула (ARTABCD) '{1}' не будет изменено.", message, art.ARTABCD);    
                    }
                    
                    messages.Add(message);
                    log.Error(message);
                }
            }

            return messages.ToArray();
        }

        private static bool ExistsArtAbcd(string wrapperArtAbcd)
        {
            if (_artAbcd == null)
            {
                _artAbcd = new Dictionary<string, bool>();

                var type = typeof(SysEnum);
                var filter = string.Format("{0} = 'ART' AND {1} = 'ABCD'",
                    SourceNameHelper.Instance.GetPropertySourceName(type, SysEnum.ENUMGROUPPropertyName),
                    SourceNameHelper.Instance.GetPropertySourceName(type, SysEnum.ENUMKEYPropertyName));

                SysEnum[] enums;
                using (var mgr = IoC.Instance.Resolve<IBaseManager<SysEnum>>())
                {
                    enums = mgr.GetFiltered(filter, GetModeEnum.Partial).ToArray();
                }

                foreach (var p in enums)
                {
                    _artAbcd[p.SysEnumValue] = true;
                }
            }

            return _artAbcd.ContainsKey(wrapperArtAbcd);
        }

        public static Art[] FindArtByArtName(decimal? mandantid, string artname, GetModeEnum mode, IUnitOfWork uow)
        {
            if (!mandantid.HasValue || string.IsNullOrEmpty(artname))
                return null;

            var type = typeof (Art);
            var filter = string.Format("UPPER({0}) = '{1}' and {2} = {3}",
                SourceNameHelper.Instance.GetPropertySourceName(type, Art.ArtNamePropertyName),
                string.IsNullOrEmpty(artname) ? artname : artname.ToUpper(),
                SourceNameHelper.Instance.GetPropertySourceName(type, Art.MANDANTIDPropertyName), mandantid);

            Art[] result;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Art>>())
            {
                if (uow != null)
                    mgr.SetUnitOfWork(uow);
                result = mgr.GetFiltered(filter, mode).ToArray();
            }

            if (result.Length > 1)
                result = result.Where(p => p.ArtName == artname).ToArray();

            return result;
        }
    }
}
