using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;
using BLToolkit.Data;
using Oracle.DataAccess.Client;

namespace wmsMLC.General.DAL.Oracle
{
    public abstract class BaseXmlRepository<T, TKey> : BaseDataAccessor<T>
    {
        // ReSharper disable once StaticFieldInGenericType
        // ReSharper disable once InconsistentNaming
        protected static double _lastQueryExecutionTime;

        /// <summary>
        /// Время выполнения последнего запроса.
        /// </summary>
        public double LastQueryExecutionTime { get { return _lastQueryExecutionTime; } }

        public void ClearStatistics()
        {
            _lastQueryExecutionTime = 0;
        }

        #region .  DB Calls  .

        protected virtual List<XmlDocument> XmlGetList(XmlDocument attrentity, string filter, string functionName = null)
        {
            ClearStatistics();

            // В фильтре могут быть переданы разу несколько (для обхода ограничения на 4000 символов). Разделитель - ";"
            string[] filters = null;
            if (!string.IsNullOrEmpty(filter))
            {
                filters = filter.Split(';');

                // проверяем каждый
                foreach (var f in filters)
                    CheckFilter(f);
            }

            return RunManualDbOperation(db =>
            {
                var funcName = string.IsNullOrEmpty(functionName)
                    ? GetSpName(typeof (T), "XmlGetList")
                    : functionName;

                var stm = string.Format("select SYS.XMLTYPE.GETCLOBVAL(COLUMN_VALUE) from TABLE({0}({1},{2}))"
                    , funcName
                    , attrentity == null ? "NULL" : ":pAttrEntity"
                    , string.IsNullOrEmpty(filter) ? "NULL" : ":pFilter");

                var now = DateTime.Now;

                var parameters = new List<IDbDataParameter>();
                IDbDataParameter pFilter = null;
                if (!string.IsNullOrEmpty(filter))
                {
                    pFilter = db.InputParameter("pFilter", filter);
                    parameters.Add(pFilter);
                }

                if (attrentity != null)
                {
                    var pAttrEntity = db.InputParameter("pAttrEntity", attrentity);
                    parameters.Add(pAttrEntity);
                }

                var resultCln = new List<XmlDocument>();
                if (pFilter != null && filters != null && filters.Length > 1)
                {
                    foreach (var f in filters)
                    {
                        pFilter.Value = f;
                        var res = parameters.Count == 0
                            ? db.SetCommand(stm).ExecuteScalarList<XmlDocument>()
                            : db.SetCommand(stm, parameters.ToArray()).ExecuteScalarList<XmlDocument>();
                        resultCln.AddRange(res);
                    }
                }
                else
                {
                    var res = parameters.Count == 0
                        ? db.SetCommand(stm).ExecuteScalarList<XmlDocument>()
                        : db.SetCommand(stm, parameters.ToArray()).ExecuteScalarList<XmlDocument>();
                    resultCln.AddRange(res);
                }

                _lastQueryExecutionTime = (DateTime.Now - now).TotalSeconds;
                return resultCln;
            });
        }

        protected virtual List<T> GetListTFromFunction(string filter)
        {
            ClearStatistics();

            CheckFilter(filter);

            return RunManualDbOperation(db =>
            {
                var stm = string.Format("select * from TABLE({0})", filter);
                var now = DateTime.Now;
                var res = db.SetCommand(stm).ExecuteList<T>();
                _lastQueryExecutionTime = (DateTime.Now - now).TotalSeconds;
                return res;
            });
        }

        /// <summary>
        /// Метод фиксирует логику работы с DbManager-ом. Чтобы везде не писать ручками сделана такая обертка.
        /// Внимание!!! Все "ручные" запросы в БД делать через этот метод
        /// </summary>
        /// <typeparam name="TRes">Тип возвращаемого значения</typeparam>
        /// <param name="action">Собственно делегат, который будет исполняться</param>
        /// <param name="useTransaction">Признак необходимости выполнения action-а в транзакции</param>
        /// <returns>Результат работы делегата</returns>
        protected TRes RunManualDbOperation<TRes>(Func<DbManager, TRes> action, bool useTransaction = false)
        {
            var db = GetDbManager();
            var dispose = DisposeDbManager;
            var transactionHandler = db.Transaction == null;
            try
            {
                // запомним указанного DbManager-а для всех вложенных операций
                SetDbManager(db, false);

                if (useTransaction && transactionHandler)
                    db.BeginTransaction();

                var res = action(db);

                if (useTransaction && transactionHandler)
                    db.CommitTransaction();

                return res;
            }
            // ловим все ошибки - для отката транзакции
            catch (Exception ex)
            {
                if (useTransaction && transactionHandler)
                    db.RollbackTransaction();

                // отлавливаем все ошибки, чтобы не "светить" наружу специфическими ошибками БД
                var newEx = DALExceptionHandler.ProcessException(ex);
                if (newEx == ex)
                    throw;

                throw newEx;
            }
            finally
            {
                DisposeDbManager = dispose;
                Dispose(db);
            }
        }

        protected virtual XmlDocument XmlGet(TKey key, XmlDocument attrentity = null)
        {
            return RunManualDbOperation(db =>
            {
                var func = GetSpName(typeof(T), "XmlGet");
                var pKey = db.InputParameter("pKey", key);

                var stm = string.Format("SELECT SYS.XMLTYPE.GETCLOBVAL({0}(:{1}, {2})) FROM DUAL",
                    func,
                    pKey.ParameterName,
                    attrentity == null ? "NULL" : ":pAttrEntity");

                if (attrentity == null)
                    return db.SetCommand(stm, pKey).ExecuteScalar<XmlDocument>();

                var pEntXml = new OracleParameter("pAttrEntity", OracleDbType.XmlType, attrentity, ParameterDirection.Input);
                return db.SetCommand(stm, pKey, pEntXml).ExecuteScalar<XmlDocument>();
            });
        }

        protected virtual void XmlUpdate(XmlDocument entxml)
        {
            RunManualDbOperation<object>(db =>
            {
                var sql = GetSpName(typeof(T), "XmlUpdate");
                var pEntXml = db.InputParameter("pEntXml", entxml);
                db.SetCommand(CommandType.StoredProcedure, sql, pEntXml).ExecuteNonQuery();
                return null;
            });
        }

        protected virtual void XmlDelete(TKey key)
        {
            RunManualDbOperation<object>(db =>
            {
                var sql = GetSpName(typeof(T), "XmlDelete");
                var pKey = db.InputParameter("pKey", key);
                db.SetSpCommand(sql, pKey).ExecuteNonQuery();
                return null;
            });
        }

        protected virtual void XmlInsert(XmlDocument entxml, out TKey key)
        {
            key = RunManualDbOperation(db =>
            {
                var sql = GetSpName(typeof(T), "XmlInsert");
                var innerKey = default(TKey);

                var pEntXml = db.InputParameter("pEntXml", entxml);
                var pKey = db.OutputParameter("pKey", innerKey);

                //Если перед нами строка или Guid, то резервируем максимальный размер для VARCHAR2 = 32767
                if (typeof (TKey) == typeof (string) ||
                    typeof(TKey) == typeof(Guid))
                    pKey.Size = 32767;

                db.SetCommand(CommandType.StoredProcedure, sql, new [] { pEntXml, pKey }).ExecuteNonQuery();
                return (TKey)db.DataProvider.MappingSchema.ConvertChangeType(pKey.Value, typeof (TKey));
            });
        }

        [Obsolete("Do not use this method. BLToolkit already free resources")]
        protected virtual void DisposeCommand(IDbCommand command)
        {
            if (command == null)
                return;

            // прибиваем параметры
            if (command.Parameters != null)
            {
                foreach (IDataParameter p in command.Parameters)
                {
                    var oraP = p as OracleParameter;
                    if (oraP != null && !string.IsNullOrEmpty(oraP.UdtTypeName))
                    {
                        //                        var method = typeof (OracleParameter).GetMethod("ResetUDTInd", BindingFlags.Instance | BindingFlags.NonPublic);
                        //                        if (method == null)
                        //                            throw new DeveloperException("HACK больше не работет. Пропал метод ResetUDTInd");
                        //                        method.Invoke(oraP, null);

                        //                        var m_udtDescriptorField = typeof(OracleParameter).GetField("m_udtDescriptor", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
                        //                        var value = m_udtDescriptorField.GetValue(oraP);
                        //                        if (value != null)
                        //                            m_udtDescriptorField.SetValue(oraP, null);
                    }

                    var disp = p.Value as IDisposable;
                    if (disp != null)
                        disp.Dispose();
                }
            }
            // прибиваем комманду
            command.Dispose();
        }
        #endregion

        #region .  Helpers  .
        protected override string GetSpName(Type type, string actionName)
        {
            var pkg = GetPakageName(type);
            if (!string.IsNullOrEmpty(pkg))
                pkg = pkg + ".";

            return pkg + GetEssenceName(type, actionName);
        }

        protected virtual string GetObjectName()
        {
            return SourceNameHelper.Instance.GetSourceName(typeof(T));
        }

        protected virtual string GetPakageName(Type type)
        {

            return "pkg" + GetObjectName();
        }

        protected virtual string GetSuffix(string actionName)
        {
            switch (actionName)
            {
                case "XmlUpdate": return "Upd";
                case "XmlDelete": return "Del";
                case "XmlInsert": return "Ins";
                case "XmlGet": return "Get";
                case "XmlGetList": return "Get";
                default:
                    return string.Empty;
            }
        }

        protected virtual string GetPostfix(string actionName)
        {
            switch (actionName)
            {
                case "XmlUpdate": return string.Empty;
                case "XmlDelete": return string.Empty;
                case "XmlInsert": return string.Empty;
                case "XmlGet": return string.Empty;
                case "XmlGetList": return "Lst";
                default:
                    return string.Empty;
            }
        }

        protected virtual string GetEssenceName(Type type, string actionName)
        {
            return GetSuffix(actionName) + GetObjectName() + GetPostfix(actionName);
        }

        protected static string GetPluaralForm(string name)
        {
            // TODO: доделать правила множественного числа
            // NOTE: не хочется использовать PluralizationService, т.к. он подтянет EntityFramework
            var res = name;
            if (res[res.Length - 1] == 'y')
                res = res.Substring(0, res.Length - 1) + "ies";
            else if ("o,s".Contains(res[res.Length - 1]))
                res += "es";
            else
                res += "s";
            return res;
        }

        /// <summary>
        /// Проверяем ограничения на фильтр, передаваемый в БД
        /// </summary>
        /// <param name="filter">строка фильтра</param>
        // ReSharper disable once UnusedParameter.Local
        private static void CheckFilter(string filter)
        {
            if (!string.IsNullOrEmpty(filter) && filter.Length > 4000)
                throw new DALCustomException(ExceptionResources.FilterLengthMoreThan4000);
        }

        #endregion
    }
}