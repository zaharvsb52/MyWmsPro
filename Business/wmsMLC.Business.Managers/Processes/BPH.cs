using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;

namespace wmsMLC.Business.Managers.Processes
{
    // ReSharper disable once InconsistentNaming
    public static class BPH
    {
        public static T GetKey<T>(this WMSBusinessObject wmsBusinessObject)
        {
            return wmsBusinessObject == null ? default(T) : (T)wmsBusinessObject.GetKey();
        }

        public static T Get<T>(this WMSBusinessObject wmsBusinessObject, string propertyname)
        {
            return (wmsBusinessObject == null || string.IsNullOrEmpty(propertyname)) ? default(T) : wmsBusinessObject.GetProperty<T>(propertyname);
        }

        public static T ConvertTo<T>(this IDictionary<string, object> dictionary, string key, bool isrequiredfield = false)
        {
            if (dictionary == null)
                throw new ArgumentNullException("dictionary");
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            //if (dictionary == null || string.IsNullOrEmpty(key))
            //    return default(T);
            if (!dictionary.ContainsKey(key))
            {
                if (isrequiredfield)
                    throw new OperationException(string.Format("Поле '{0}' является обязательным.", key));
                return default(T);
            }

            try
            {
                return dictionary[key].ConvertTo<T>();
            }
            catch (Exception ex)
            {
                throw new OperationException(string.Format("Значение поля '{0}' не может быть конвертировано в тип '{1}'.", key, typeof(T)), ex);
            }
        }

        public static T ConvertTo<T>(this object value)
        {
            if (value == null)
                return default(T);

            var type = typeof(T);
            if (type == typeof(DateTime?) && value.To<string>() == string.Empty)
                return default(T);

            var result = SerializationHelper.ConvertToTrueType(value, type);

            if (result == null && !type.IsNullable())
                return default(T);

            return (T)result;
        }

        public static string GetInnerException(Exception e)
        {
            if (e == null)
                return null;
            return ExceptionHelper.GetErrorMessage(e);
        }

        public static T[] GetDistinctPropertyByName<T>(IEnumerable<T> obj, string propertyName)
        {
            var result = obj.Cast<WMSBusinessObject>().DistinctBy(i => i.GetProperty(propertyName));
            return result.Cast<T>().ToArray();
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
        (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            return source.Where(element => seenKeys.Add(keySelector(element)));
        }

        public static DateTime GetSystemDate()
        {
            using (var manager = IoC.Instance.Resolve<IBPProcessManager>())
            {
                return manager.GetSystemDate();
            }
        }

        #region .  Business  .

        /// <summary>
        /// Получение манданта у бизнес объекта
        /// </summary>
        /// <param name="obj">бизнес объект</param>
        /// <returns>код манданта</returns>
        public static decimal? GetMandantId(WMSBusinessObject obj)
        {
            var properties = TypeDescriptor.GetProperties(obj);
            //HACK: переделать на нормальную схему
            var property = properties["MANDANTID"];
            if (property == null)
                return null;
            return (decimal?)property.GetValue(obj);
        }

        /// <summary>
        /// Получение информационного сообщения расхождений заявленного и принятого товара по грузу с группировкой по SKU
        /// формат: Накл. '111' SKU 'xxxx' Заяв. х Прин. y Расх. z
        /// </summary>
        /// <param name="cargoIwbId">идентификатор груза</param>
        /// <returns>массив с информацией по SKU</returns>
        [Obsolete("Теперь достаточно вычесть IWBPOS.IWBPOSPRODUCTCOUNT - IWBPOS.IWBPOSCOUNT")]
        public static string[] GetSKUVarianceForIWBInfo(decimal cargoIwbId)
        {
            using (var iwbMgr = IoC.Instance.Resolve<IBaseManager<IWB>>())
            {
                var filter = string.Format("WMSIWB.STATUSCODE_R not in ('IWB_COMPLETED') and  WMSIWB.IWBID in (select WC.IWBID_R from WMSIWB2CARGO wc where WMSIWB.IWBID=WC.IWBID_R AND WC.CARGOIWBID_R={0} union select p.IWBID_R from wmscargoiwbpos p where WMSIWB.IWBID=p.IWBID_R AND P.CARGOIWBID_R={0})", cargoIwbId);
                var iwbList = iwbMgr.GetFiltered(filter).ToArray();
                return GetSKUVarianceForIWBInfo(iwbList.Select(i => i.GetKey<decimal>()).ToArray());
            }
        }

        /// <summary>
        /// Получение информационного сообщения расхождений заявленного и принятого товара по всем позициям с группировкой по SKU
        /// формат: Накл. '111' SKU 'xxxx' Заяв. х Прин. y Расх. z
        /// </summary>
        /// <param name="iwbIdList">идентификатор накладной</param>
        /// <returns>массив с информацией по SKU</returns>
        [Obsolete("Теперь достаточно вычесть IWBPOS.IWBPOSPRODUCTCOUNT - IWBPOS.IWBPOSCOUNT")]
        public static string[] GetSKUVarianceForIWBInfo(decimal[] iwbIdList)
        {
            var result = new List<string>();
            if (iwbIdList == null)
                return null;
            foreach (var iwbId in iwbIdList)
            {
                var posInputList = GetIWBPosInputForIWB(iwbId);
                if (!posInputList.Any())
                    continue;
                var posBySkuList = GetDistinctPropertyByName<IWBPosInput>(posInputList, "SKUID");
                foreach (var posSku in posBySkuList)
                {
                    var sku = posSku;
                    var posSkuGroup = posInputList.Where(i => i.SKUID == sku.SKUID).ToArray();
                    var reqCnt = posSkuGroup.Sum(i => i.IWBPosCount);
                    var prodCnt = posSkuGroup.Sum(i => i.ProductCountSKU);
                    if (reqCnt != prodCnt)
                        result.Add(string.Format("Накладная '{0}' SKU '{1}'{5}Заявлено {2}{5}Принято {3}{5}Расхождения {4}", iwbId, sku.SKUNAME, reqCnt, prodCnt, Math.Abs(reqCnt - prodCnt), System.Environment.NewLine));
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// Получение расхождений/соответствий в приходных накладных заявленного и принятого товара по всем позициям
        /// </summary>
        /// <param name="iwbIdList">список накладных</param>
        /// <param name="variance">true = искать расхождения, false = без расхождений</param>
        /// <returns>список расходящихся/соответствующих накладных</returns>
        [Obsolete("Теперь достаточно вычесть IWBPOS.IWBPOSPRODUCTCOUNT - IWBPOS.IWBPOSCOUNT")]
        public static decimal[] GetSKUVarianceForIWB(decimal[] iwbIdList, bool variance = true)
        {
            var goodList = new List<decimal>();
            var badList = new List<decimal>();
            if (iwbIdList == null)
                return null;
            foreach (var iwbId in iwbIdList)
            {
                var posInputList = GetIWBPosInputForIWB(iwbId);
                if (!posInputList.Any())
                {
                    goodList.Add(iwbId);
                    continue;
                }
                var badIwb = posInputList.AsParallel().FirstOrDefault(i => i.IWBPosCount - i.ProductCountSKU != 0);
                if (badIwb != null)
                    badList.Add(iwbId);
                else
                    goodList.Add(iwbId);
            }
            return variance ? badList.ToArray() : goodList.ToArray();
        }

        /// <summary>
        /// Получение расхождений/соответствий в приходных накладных заявленного и принятого товара по всем позициям
        /// </summary>
        /// <param name="iwbIdList">список накладных</param>
        /// <param name="variance">true = искать расхождения, false = без расхождений</param>
        /// <returns>список расходящихся/соответствующих накладных</returns>
        public static decimal[] GetSKUVarianceForIWB(IEnumerable<IWB> iwbIdList, bool variance = true)
        {
            if (iwbIdList == null)
                return null;

            var goodList = new List<decimal>();
            var badList = new List<decimal>();

            IWBPos[] iwbList;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<IWBPos>>())
            {
                var filter = string.Format("IWBID_R in ({0})", string.Join(",", iwbIdList.Select(i => i.GetKey<decimal>()).ToArray()));
                iwbList = mgr.GetFiltered(filter).ToArray();
            }

            foreach (var iwbId in iwbIdList.Select(i => i.GetKey<decimal>()).ToArray())
            {
                if (!iwbList.Any())
                {
                    goodList.Add(iwbId);
                    continue;
                }

                var badIwb = iwbList.AsParallel().FirstOrDefault(i => i.IWBID_R == iwbId && (i.IWBPosCount - (decimal)i.IWBPosProductCount != 0));
                if (badIwb != null)
                    badList.Add(iwbId);
                else
                    goodList.Add(iwbId);

            }
            return variance ? badList.ToArray() : goodList.ToArray();
        }

        /// <summary>
        /// Получение виртуальных позиций по накладной
        /// </summary>
        /// <param name="iwbId">идентификатор накладной</param>
        /// <returns>массив виртуальных позиций</returns>
        private static IWBPosInput[] GetIWBPosInputForIWB(decimal iwbId)
        {
            using (var posMgr = IoC.Instance.Resolve<IBaseManager<IWBPos>>())
            {
                var posList = posMgr.GetFiltered(string.Format("IWBID_R = {0}", iwbId)).ToArray();
                using (var bpMgr = IoC.Instance.Resolve<IBPProcessManager>())
                    return bpMgr.GetIWBPosInputLst(posList).ToArray();
            }
        }

        /// <summary>
        /// Определение типа ТЕ по коду ТЕ
        /// </summary>
        /// <param name="teCode">код ТЕ</param>
        /// <param name="filter">дополнительный фильтр по типам ТЕ</param>
        /// <returns>код типа ТЕ</returns>
        public static string DetermineTeTypeCodeByTeCode(string teCode, string filter = null)
        {
            var result = string.Empty;
            if (string.IsNullOrEmpty(teCode))
                return result;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<TEType>>())
            {
                var typeList = mgr.GetFiltered(filter, FilterHelper.GetAttrEntity<TEType>(TEType.CodePropertyName, TEType.TETYPENUMBERPREFIXPropertyName));
                var teType = typeList.Where(x => !string.IsNullOrEmpty(x.TETYPENUMBERPREFIX) && teCode.StartsWith(x.TETYPENUMBERPREFIX)).OrderByDescending(x => x.TETYPENUMBERPREFIX.Length).FirstOrDefault();
                if (teType != null)
                    result = teType.GetKey().To<string>();
            }
            return result;
        }

        /// <summary>
        /// Получение кода места погрузчика.
        /// </summary>
        /// <param name="truckCode">код погрузчика</param>
        /// <returns>код места</returns>
        public static string GetTruckPlaceCode(string truckCode)
        {
            var result = string.Empty;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Truck>>())
            {
                var truck = mgr.Get(truckCode);
                if (truck == null)
                    throw new OperationException("Погрузчик с кодом '{0}' не найден", truckCode);
                result = truck.PLACECODE_R;
            }
            return result;
        }

        /// <summary>
        /// Получение списка складов, к которым привязан сотрудник.
        /// </summary>
        public static string[] GetWarehouseCodesByWorkerId(decimal workerid)
        {
            var filter = string.Format("WORKERID_R = {0}", workerid);
            var attr = FilterHelper.GetAttrEntity(typeof(Worker2Warehouse), Worker2Warehouse.WORKER2WAREHOUSEWAREHOUSECODEPropertyName);

            Worker2Warehouse[] w2Whs;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Worker2Warehouse>>())
            {
                w2Whs = mgr.GetFiltered(filter, attr).ToArray();
            }

            if (w2Whs.Length == 0)
                return new string[0];

            return w2Whs.Select(p => p.Get<string>(Worker2Warehouse.WORKER2WAREHOUSEWAREHOUSECODEPropertyName)).Distinct().ToArray();
        }

        /// <summary>
        /// Получение списка складов из списков пикинга.
        /// </summary>
        public static string[] GetWarehouseCodesByPlIds(decimal[] plids)
        {
            if (plids == null || plids.Length == 0)
                return null;

            var sqlformat = "select distinct a.warehousecode_r from wmsplpos pp" +
                      " join wmsplace p on p.placecode = pp.placecode_r" +
                      " join wmssegment s on s.segmentcode = p.segmentcode_r" +
                      " join wmsarea a on a.areacode = s.areacode_r " +
                      " where {0} and a.warehousecode_r is not null";

            var result = new List<string>();
            var sqlfilter = FilterHelper.GetArrayFilterIn("pp.plid_r", plids.Cast<object>());
            if (sqlfilter == null || sqlfilter.Length == 0)
                return null;

            using (var manager = IoC.Instance.Resolve<IBPProcessManager>())
            {
                foreach (var filter in sqlfilter)
                {
                    var restable = manager.ExecuteDataTable(string.Format(sqlformat, filter));
                    if (restable != null)
                    {
                        var rows = restable.Rows.Cast<DataRow>().ToArray();
                        var whs = rows.Select(p => p["warehousecode_r"].To<string>()).Where(p => !string.IsNullOrEmpty(p)).ToArray();
                        if (whs.Length > 0)
                            result.AddRange(whs);
                    }
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Поиск работы для сущности по коду операции.
        /// </summary>
        /// <param name="entity">имя сущности</param>
        /// <param name="key">код экземпляра сущности</param>
        /// <param name="operation">код операции</param>
        /// <param name="mode">режим получения объекта</param>
        /// <returns>работа</returns>
        public static Work GetWorkByEntity(string entity, object key, BillOperationCode operation, GetModeEnum mode = GetModeEnum.Full)
        {
            var filter =
                string.Format(
                    "workid in (select max(w2e.workid_r) from wmswork2entity w2e inner join wmswork w on w.workid = w2e.workid_r where w2e.work2entityentity = '{0}' and w2e.work2entitykey = '{1}' and w.operationcode_r = '{2}')",
                    entity, key, operation);
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Work>>())
            {
                var works = mgr.GetFiltered(filter, mode).ToArray();
                if (works.Length > 0)
                    return works[0];
            }
            return null;
        }

        public static T MapTo<T>(object source, T dest, bool modeError = false)
        {
            var sourceProperties = TypeDescriptor.GetProperties(source);
            var destProperties = TypeDescriptor.GetProperties(dest);
            foreach (PropertyDescriptor property in sourceProperties)
            {
                var prop = destProperties.Cast<PropertyDescriptor>().FirstOrDefault(i => i.Name.EqIgnoreCase(property.Name));
                if (prop == null)
                    continue;
                var value = property.GetValue(source);
                if (value == null)
                    continue;
                prop.SetValue(dest, value);
            }
            return dest;
        }

        public static DataTable GetOpenWorkingsByWorkerId(decimal workerId, string filter)
        {
            var sql = "select w.workid, wng.workingid, o.operationname, w.operationcode_r from wmsworking wng" +
                      " join wmswork w on wng.workid_r = w.workid" +
                      " join billoperation o on w.operationcode_r = o.operationcode" +
                      string.Format(" where (wng.workingtill is null or wng.workingtill > sysdate) and wng.workerid_r = {0}{1}", workerId, filter) +
                      " order by wng.workingfrom";

            using (var manager = IoC.Instance.Resolve<IBPProcessManager>())
            {
                return manager.ExecuteDataTable(sql);
            }
        }
        #endregion  .  Business  .

        #region . Info .
        /// <summary>
        //// Получаем информацию по товарам, расположенным на ТЕ, сгруппированным по SKU.
        /// </summary>
        public static DataTable GetProductInfoByTe(string tecode)
        {
            var sql = string.Format("SELECT SUM(P.PRODUCTCOUNTSKU) AS PRODUCTCOUNTSKU, SUM(DECODE(P.STATUSCODE_R," +
                                    "'PRODUCT_BUSY', P.PRODUCTCOUNTSKU, 0)) AS RESCOUNTSKU" +
                                    ",MAX(A.ARTNAME) AS ARTNAME, MAX(A.ARTDESC) AS ARTDESC" +
                                    ",MAX(M.MEASURENAME) AS MEASURENAME," +
                                    "MAX(P.TECODE_R) AS TECODE, MAX(SKU.ARTCODE_R) AS ARTCODE, P.SKUID_R, Q.QLFNAME" +
                                    " FROM WMSPRODUCT P" +
                                    " JOIN WMSSKU SKU  ON P.SKUID_R = SKU.SKUID" +
                                    " JOIN WMSART A ON SKU.ARTCODE_R = A.ARTCODE" +
                                    " JOIN WMSMEASURE M ON SKU.MEASURECODE_R = M.MEASURECODE" +
                                    " JOIN WMSQLF Q ON P.QLFCODE_R = Q.QLFCODE" +
                                    " WHERE P.TECODE_R = '{0}'" +
                                    " GROUP BY P.SKUID_R, Q.QLFNAME" +
                                    " ORDER BY ARTNAME", tecode);

            using (var manager = IoC.Instance.Resolve<IBPProcessManager>())
            {
                return manager.ExecuteDataTable(sql);
            }
        }

        /// <summary>
        /// Получаем информацию по товарам, с артикулом Art, сгруппированным по текущему месту ТЕ, ТЕ и SKU.
        /// </summary>
        /// <param name="artcode"></param>
        public static DataTable GetProductInfoByArt(string artcode)
        {
            var sql = string.Format("SELECT MAX(P.TECODE_R) AS TECODE" +
                                    ",MAX(TE.TECURRENTPLACE) AS TECURRENTPLACE, MAX(PL.PLACENAME) AS TECURRENTPLACE_NAME" +
                                    ",SUM(P.PRODUCTCOUNTSKU) AS PRODUCTCOUNTSKU, SUM(DECODE(P.STATUSCODE_R, 'PRODUCT_BUSY', P.PRODUCTCOUNTSKU, 0)) AS RESCOUNTSKU" +
                                    ",MAX(SKU.SKUNAME) AS SKUNAME, Q.QLFNAME" +
                                    " FROM WMSPRODUCT P" +
                                    " JOIN WMSSKU SKU ON P.SKUID_R = SKU.SKUID" +
                                    " JOIN WMSTE TE ON P.TECODE_R = TE.TECODE " +
                                    " JOIN WMSPLACE PL ON TE.TECURRENTPLACE = PL.PLACECODE" +
                                    " JOIN WMSQLF Q ON P.QLFCODE_R = Q.QLFCODE" +
                                    " WHERE SKU.ARTCODE_R = '{0}'" +
                                    " GROUP BY TE.TECURRENTPLACE, P.TECODE_R, P.SKUID_R, Q.QLFNAME" +
                                    " ORDER BY TECURRENTPLACE_NAME", artcode);

            using (var manager = IoC.Instance.Resolve<IBPProcessManager>())
            {
                return manager.ExecuteDataTable(sql);
            }
        }

        /// <summary>
        /// Получаем информацию о выполнении работ для активной сессии
        /// </summary>
        /// <param name="ClientSessionID"></param>
        public static DataTable GetWorkingForSession(decimal? ClientSessionID)
        {
            if (ClientSessionID == null)
                return null;

            var sql =
                string.Format(
                    "SELECT rownum id, w.workingid, w.workerid_r, worker.workerlastname || ' ' || worker.workername || ' ' || worker.workermiddlename as FIO, ww.operationcode_r, " +
                    "oper.operationname as OPERATIONNAME, w2e.work2entityentity, ext.objectextcaption, w2e.work2entitykey, w.workingfrom, w.workingtill " +
                    "from sysclientsession scs " +
                    "join wmsworking w on scs.clientsessionbegin <= w.workingfrom " +
                    "join wmswork ww on ww.workid = w.workid_r " +
                    "left join wmswork2entity w2e on w2e.workid_r = w.workid_r " +
                    "left join wmsworker worker on worker.workerid = w.workerid_r " +
                    "left join billoperation oper on oper.operationcode = ww.operationcode_r " +
                    "left join sysobjectext ext on ext.objectextentity = w2e.work2entityentity and ext.objectextentity = ext.objectname_r " +
                    "where scs.clientsessionid = {0} " +
                    "and w.workerid_r in (select wmsworking.workerid_r from wmsworking where wmsworking.userins = scs.usercode_r and scs.clientsessionbegin <= wmsworking.workingfrom) " +
                    //"and w.workingtill is NULL " +
                    "order by w.workingid", ClientSessionID);
            using (var manager = IoC.Instance.Resolve<IBPProcessManager>())
            {
                return manager.ExecuteDataTable(sql);
            }
        }


        #endregion . Info .

        #region Printer
        public static IEnumerable<KeyValuePair<string, string[]>> GetPrinters(IEnumerable<string> reports, IEnumerable<string> installedPrinters, IEnumerable<decimal> mandantList)
        {
            if (reports == null)
                throw new ArgumentNullException("reports");
            if (installedPrinters == null)
                throw new ArgumentNullException("installedPrinters");
            if (mandantList == null)
                throw new ArgumentNullException("mandantList");

            var printersFilter = string.Join(",", installedPrinters.Select(i => "'" + i.Replace("'", "''") + "'"));
            var reportsFilter = string.Join(",", reports.Select(i => "'" + i + "'"));
            var mandantsFilter = string.Join(",", mandantList);
            var sql = string.Format("SELECT lp.LOGICALPRINTER as LOGICALPRINTER, lp.LOGICALPRINTERDESC as LOGICALPRINTERDESC, ppd.PHYSICALPRINTERDESC as PHYSICALPRINTERDESC " +
                                    "FROM epsprinterlogical lp " +
                                    "INNER JOIN epsprintstreamconfig sc " +
                                    "ON sc.LOGICALPRINTER_R = lp.LOGICALPRINTER " +
                                    "INNER JOIN epsprinterphysical ppd " +
                                    "ON ppd.PHYSICALPRINTER = lp.PHYSICALPRINTER_R " +
                                    "LEFT JOIN epsprinterphysical pp " +
                                    "ON pp.PHYSICALPRINTER = lp.PHYSICALPRINTER_R " +
                                    "AND pp.PHYSICALPRINTER IN ({0}) " +
                                    "WHERE " +
                                    "lp.LOGICALPRINTERLOCKED = 0 " +
                                    "AND NVL(sc.HOST_R, '{1}') = '{1}' " +
                                    "AND NVL(sc.LOGIN_R, '{2}') = '{2}' " +
                                    "AND NVL(sc.PARTNERID_R, 0) IN (0,{3}) " +
                                    "AND NVL(sc.REPORT_R, 'NONE') IN ('NONE', {4}) " +
                                    "AND NVL(sc.PERIODBEGIN, {5}) <= {5} " +
                                    "AND NVL(sc.PERIODEND, {5})   >= {5} " +
                                    "GROUP BY lp.LOGICALPRINTER, lp.LOGICALPRINTERDESC, pp.PHYSICALPRINTER, ppd.PHYSICALPRINTERDESC " +
                                    "HAVING SUM (nvl2(sc.report_r, 1, 999999)) >= {6} " +
                                    "ORDER BY pp.PHYSICALPRINTER nulls last",
                                    string.IsNullOrEmpty(printersFilter) ? "NULL" : printersFilter,
                                    WMSEnvironment.Instance.ClientCode, WMSEnvironment.Instance.AuthenticatedUser.GetSignature(),
                                    string.IsNullOrEmpty(mandantsFilter) ? "0" : mandantsFilter, string.IsNullOrEmpty(reportsFilter) ? "'NONE'" : reportsFilter, "sysdate", reports.Count());

            using (var manager = IoC.Instance.Resolve<IBPProcessManager>())
                return manager.ExecuteDataTable(sql).Rows.Cast<DataRow>().Select(i => new KeyValuePair<string, string[]>(i["LOGICALPRINTER"].ToString(), new[] { i["LOGICALPRINTERDESC"].ToString(), i["PHYSICALPRINTERDESC"].ToString() }));
        }
        #endregion Printer

        #region . Math .
        /// <summary>
        /// Округление элементов по заданным параметрам.
        /// </summary>
        public static IDictionary<string, double> CorrectSumByLastElement(IDictionary<string, double> elements, int rounddigit, double? defaultvalue = null, MidpointRounding mode = MidpointRounding.AwayFromZero)
        {
            var result = new Dictionary<string, double>();
            foreach (var pair in elements)
            {
                var roundel = Math.Round(pair.Value, rounddigit, mode);
                if (roundel == 0 && defaultvalue.HasValue)
                    roundel = defaultvalue.Value;
                result[pair.Key] = roundel;
            }

            return result;
        }
        #endregion . Math .
    }
}
