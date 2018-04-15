using System.Collections.Generic;
using System.Linq;
using log4net;
using wmsMLC.APS.wmsSI.Wrappers;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;
using wmsMLC.General.DAL;

namespace wmsMLC.APS.wmsSI.Helpers
{
    internal static class ShipmentLoadHelper
    {
        public const string Cpv2EntityOwbPosName = "OWBPOS";

        public static OWBPos[] OwbBoxReserve(decimal? owbid, SalesInvoiceWrapper owbwrapper)
        {
            if (owbwrapper == null || owbwrapper.OWBBOXRESERVE != 1 || !owbwrapper.MANDANTID.HasValue)
                return new OWBPos[0];

            var mandantid = owbwrapper.MANDANTID.Value;

            var boxNumbers =
                owbwrapper.OWBPOSL.Where(p => !string.IsNullOrEmpty(p.OWBPOSBOXNUMBER))
                    .Select(p => p.OWBPOSBOXNUMBER)
                    .Distinct()
                    .ToArray();

            if (boxNumbers.Length == 0)
                throw new IntegrationService.IntegrationLogicalException("Номера коробов не определены у всех позиций расходной накладной «{0}» в режиме OWBBOXRESERVE.", owbwrapper.OWBNAME);

            var filterConst = string.Format(" AND MANDANTID = {0} AND STATUSCODE_R = '{1}'", mandantid, ProductStates.PRODUCT_FREE);

            var filterArray = FilterHelper.GetArrayFilterIn("PRODUCTBOXNUMBER", boxNumbers, filterConst);

            var products = new List<Product>();
            using (var mngProduct = IoC.Instance.Resolve<IBaseManager<Product>>())
            {
                foreach (var filter in filterArray)
                {
                    var res = mngProduct.GetFiltered(filter, GetModeEnum.Partial).ToArray();
                    if (res.Length > 0)
                        products.AddRange(res);
                }
            }

            if (!products.Any())
                throw new IntegrationService.IntegrationLogicalException("Отсутствует свободный товар для формирования позиций расходной накладной «{0}» в режиме OWBBOXRESERVE.", owbwrapper.OWBNAME);

            var grProducts = products.GroupBy(k => new
            {
                k.ProductBoxNumber,

                k.SKUID,
                k.ProductCount,
                k.PRODUCTOWNER,
                k.PRODUCTKITARTNAME,
                k.QLFCode_r,
                k.QLFDETAILCODE_R,
                k.ProductExpiryDate,
                k.ProductBatch,
                k.FactoryID_R,
                k.ProductColor,
                k.ProductSize,
                k.PRODUCTDATE,
                k.ProductSerialNumber,
                k.PRODUCTLOT,
                k.ProductTone,
            });

            var owbposL = new WMSBusinessCollection<OWBPos>();
            var posnumber = 0;
            foreach (var grp in grProducts)
            {
                var owbpos = new OWBPos
                {
                    OWBID_R = owbid,
                    OWBPOSBOXNUMBER = grp.Key.ProductBoxNumber,

                    MandantID = mandantid,
                    OWBPosOwner = grp.Key.PRODUCTOWNER,
                    OWBPosNumber = ++posnumber,
                    Status = OWBPosStates.OWBPOS_CREATED,

                    SKUID = grp.Key.SKUID,
                    OWBPosCount2SKU = grp.Key.ProductCount,
                    OWBPOSKITARTNAME = grp.Key.PRODUCTKITARTNAME,

                    QLFCODE_R = grp.Key.QLFCode_r,
                    QLFDETAILCODE_R = grp.Key.QLFDETAILCODE_R,
                    OWBPOSEXPIRYDATE = grp.Key.ProductExpiryDate,
                    OWBPosBatch = grp.Key.ProductBatch,
                    FactoryID_R = grp.Key.FactoryID_R,
                    OWBPOSCOLOR = grp.Key.ProductColor,
                    OWBPOSSIZE = grp.Key.ProductSize,
                    OWBPosCount = grp.Sum(p => p.ProductCountSKU),
                    OWBPOSPRODUCTDATE = grp.Key.PRODUCTDATE,
                    OWBPOSSERIALNUMBER = grp.Key.ProductSerialNumber,
                    OWBPOSLOT = grp.Key.PRODUCTLOT,
                    OWBPOSTONE = grp.Key.ProductTone
                };

                owbposL.Add(owbpos);
            }

            return owbposL.ToArray();
        }

        public static bool ValidateCpv(SalesInvoiceWrapper item, OWBCpvWrapper cpvw, IUnitOfWork uow, ILog log, out List<string> messages)
        {
            messages = new List<string>();
            if (item == null || cpvw == null)
                return false;

            var loadentityname = item.OWBNAME;
            var customParamCode = cpvw.CUSTOMPARAMCODE_R_OWBCPV;
            var cpvvalue = cpvw.CPVVALUE_OWBCPV;

            //HACK: OWBClientFinalRecipientL2 (бывший isShop)
            //if ("isShop".EqIgnoreCase(cpvw.CPVKEY_OWBCPV))
            if ("OWBClientFinalRecipientL2".EqIgnoreCase(customParamCode))
            {
                var type = typeof(Partner);
                var filterPartner = string.Format("{0} = '{1}' and {2} = {3}",
                   SourceNameHelper.Instance.GetPropertySourceName(type,
                       Partner.PARTNERNAMEPropertyName), cpvvalue,
                   SourceNameHelper.Instance.GetPropertySourceName(type,
                       Partner.MANDANTIDPropertyName), item.MANDANTID);

                Partner[] partners;
                using (var mgrPartner = IoC.Instance.Resolve<IBaseManager<Partner>>())
                {
                    if (uow != null)
                        mgrPartner.SetUnitOfWork(uow);
                    partners = mgrPartner.GetFiltered(filterPartner, GetModeEnum.Partial).ToArray();
                }

                if (partners.Length > 0)
                {
                    var partnerid = string.Format("{0}", partners[0].PartnerId);
                    log.InfoFormat("Грузополучатель (магазин) «{0}» (ID = {1}).", cpvvalue, partnerid);
                    cpvw.CPVVALUE_OWBCPV = partnerid;
                    return true;
                }

                var message = string.Format("Грузополучатель (магазин) с наименованием «{0}» не найден.",
                    cpvvalue);
                log.InfoFormat(message);
                messages.Add(message);
                return false;
            }

            //HACK: OWBClSvcArtCode
            if ("OWBClSvcArtCode".EqIgnoreCase(customParamCode))
            {
                var arts = ArtLoadHelper.FindArtByArtName(item.MANDANTID, cpvvalue, GetModeEnum.Partial, uow);
                if (arts == null || arts.Length == 0)
                {
                    var message = string.Format("Не найден артикул по коду '{0}' в cpv '{1}' для расходной накладной '{2}'.", cpvvalue, customParamCode, loadentityname);
                    throw new IntegrationService.IntegrationLogicalException(message);
                }
                if (arts.Length > 1)
                {
                    var message = string.Format("Найдено несколько артикулов по коду '{0}' в cpv '{1}' для расходной накладной '{2}'.", cpvvalue, customParamCode, loadentityname);
                    throw new IntegrationService.IntegrationLogicalException(message);
                }

                cpvw.CPVVALUE_OWBCPV = arts[0].GetKey<string>();
                return true;
            }

            //HACK: OWBClSvcCurrency
            if ("OWBClSvcCurrency".EqIgnoreCase(customParamCode))
            {
                IsoCurrency cur;
                using (var mgrCurrency = IoC.Instance.Resolve<IBaseManager<IsoCurrency>>())
                {
                    if (uow != null)
                        mgrCurrency.SetUnitOfWork(uow);
                    cur = mgrCurrency.Get(cpvvalue, GetModeEnum.Partial);
                }

                if (cur == null)
                {
                    var message = string.Format("Не найдена валюта по коду '{0}' в cpv '{1}' для расходной накладной '{2}'.", cpvvalue, customParamCode, loadentityname);
                    messages.Add(message);
                    return false;
                }

                cpvw.CPVVALUE_OWBCPV = cur.GetKey<string>();
                return true;
            }

            return true;
        }

        public static string[] FillCarrier(SalesInvoiceWrapper item, IUnitOfWork uow, ILog log)
        {
            var messages = new List<string>();
            var loadentityname = item.OWBNAME;

            if (string.IsNullOrEmpty(item.OWBCARRIERCODE))
            {
                log.DebugFormat("Поле перевозчик (OWBCARRIERCODE) не заполнено у расходной накладной '{0}'.", loadentityname);
                return messages.ToArray();
            }

            var partnerType = typeof (Partner);
            var crriercode = item.OWBCARRIERCODE.ToUpper();
            var filter =
                string.Format(
                    "{0} = {1} and upper(GLOBALPARAMVAL.GLOBALPARAMCODE_R) = 'ISCARRIER' and (upper({2}) = '{3}' or upper({4}) = '{5}')",
                    SourceNameHelper.Instance.GetPropertySourceName(partnerType, Partner.MANDANTIDPropertyName),
                    item.MANDANTID,
                    SourceNameHelper.Instance.GetPropertySourceName(partnerType, Partner.PARTNERNAMEPropertyName),
                    crriercode,
                    SourceNameHelper.Instance.GetPropertySourceName(partnerType, Partner.PARTNERHOSTREFPropertyName),
                    crriercode);

            Partner[] result;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Partner>>())
            {
                if (uow != null)
                    mgr.SetUnitOfWork(uow);
                result = mgr.GetFiltered(filter, GetModeEnum.Partial).ToArray();
            }

            if (result.Length == 0)
            {
                var message = string.Format("Не найден перевозчик ('{0}') у расходной накладной '{1}'.",
                    item.OWBCARRIERCODE, loadentityname);
                log.Debug(message);
                messages.Add(message);
            }
            else if (result.Length == 1)
            {
                item.OWBCARRIER = result[0].GetKey<decimal>();
            }
            else
            {
                var message = string.Format("найдено несколько ('{0}') перевозчиков по '{1}' у расходной накладной '{2}'.",
                    result.Length, item.OWBCARRIERCODE, loadentityname);
                log.Debug(message);
                messages.Add(message);
                var res = result.FirstOrDefault(p => item.OWBCARRIERCODE.EqIgnoreCase(p.PartnerHostRef));
                item.OWBCARRIER = res == null ? result[0].GetKey<decimal>() : res.GetKey<decimal>();
            }

            return messages.ToArray();
        }
    }
}
