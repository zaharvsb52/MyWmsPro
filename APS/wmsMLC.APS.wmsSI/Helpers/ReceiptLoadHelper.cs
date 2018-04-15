using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using wmsMLC.APS.wmsSI.Wrappers;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;

namespace wmsMLC.APS.wmsSI.Helpers
{
    internal static class ReceiptLoadHelper
    {
        public const string Cpv2EntityIwbPosName = "IWBPOS";

        public static void FindCountries(PurchaseInvoiceWrapper item, ILog log)
        {
            if (item == null || item.IWBPOSL == null || item.IWBPOSL.Count == 0)
                return;

            var countryCodes =
                item.IWBPOSL.Where(iwbpos => !string.IsNullOrEmpty(iwbpos.COUNTRYCODE_R))
                    .Select(iwbpos => iwbpos.COUNTRYCODE_R.ToUpper())
                    .Distinct()
                    .ToArray();
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
                        log.Error(string.Format("Can't find all countries from file. Filter = '{0}'.", filter),
                            ex);
                    }
                }
            }

            if (isocountries.Count == 0)
                return;

            foreach (var iwbpos in item.IWBPOSL.Where(p => !string.IsNullOrEmpty(p.COUNTRYCODE_R)))
            {
                var neededCountry = isocountries.FirstOrDefault(p =>
                    p.CountryCode.EqIgnoreCase(iwbpos.COUNTRYCODE_R) ||
                    p.CountryAlpha2.EqIgnoreCase(iwbpos.COUNTRYCODE_R));

                if (neededCountry == null)
                {
                    log.DebugFormat("Не найдена страна для кода '{0}' в позиции '{1}' приходной накладной '{2}'.",
                        iwbpos.COUNTRYCODE_R, iwbpos.IWBPOSNUMBER, item.IWBNAME);
                    iwbpos.COUNTRYCODE_R = null;
                }
                else
                {
                    iwbpos.COUNTRYCODE_R = neededCountry.CountryCode;
                }
            }
        }
    }
}
