using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using wmsMLC.Business.General;
using wmsMLC.Business.Managers;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;
using wmsMLC.General.PL.WPF.Attributes;

namespace wmsMLC.General.PL.WPF.Helpers
{
    public static class LookupHelper
    {
        public const string FilterValueAny = "*";
        public const string FilterValueNull = "NULL";
        private const string Regexset = "[a-zA-Zа-яА-Я0-9,'_$%*()= {}!.;<>]";

        private static readonly Lazy<IGetLookupInfo> LookUpManager =
            new Lazy<IGetLookupInfo>(() => IoC.Instance.Resolve<IGetLookupInfo>());

        /// <summary>
        /// Получить по коду LookupInfo.
        /// </summary>
        public static LookupInfo GetLookupInfo(string lookUpCodeEditor)
        {
            var result = LookUpManager.Value.GetLookupInfo(lookUpCodeEditor);
            if (result.ItemType == null)
                result.ItemType = GetItemSourceType(result);
            return result;
        }

        public static IBaseManager GetItemSourceManager(LookupInfo lookupInfo)
        {
            if (lookupInfo.ItemType == null)
                lookupInfo.ItemType = GetItemSourceType(lookupInfo);
            var type = lookupInfo.ItemType;
            var mto = IoC.Instance.Resolve<IManagerForObject>(); 
            var mgrType = mto.GetManagerByTypeName(type.Name);
            if (mgrType == null)
                throw new DeveloperException(string.Format("Unknown source type '{0}'.", type.Name));

            var managerInstance = IoC.Instance.Resolve(mgrType, null) as IBaseManager;
            if (managerInstance == null)
                throw new DeveloperException(string.Format("Can't resolve IBaseManager by '{0}'.", mgrType.Name));
            return managerInstance;
        }

        public static Type GetItemSourceType(LookupInfo lookupInfo)
        {
            if (lookupInfo == null)
                throw new ArgumentNullException("lookupInfo");

            if (lookupInfo.ItemType != null)
                return lookupInfo.ItemType;

            using (var som = IoC.Instance.Resolve<ISysObjectManager>())
            {
                var type = som.GetTypeByName(lookupInfo.ItemSource);
                if (type == null)
                    throw new DeveloperException("Unknown source type '{0}'.", lookupInfo.ItemSource);

                return type;
            }
        }

        public static object[] GetComboItemsSource(IEnumerable<object> data, string displayMember)
        {
            var result = new List<object>();
            // ReSharper disable PossibleMultipleEnumeration
            if (data != null && data.Any() && !string.IsNullOrEmpty(displayMember))
            {
                var prdesc = TypeDescriptor.GetProperties(data.First());
                var prop = prdesc.Find(displayMember, true);
                if (prop != null) result.AddRange(data.Select(prop.GetValue));
            }
            // ReSharper restore PossibleMultipleEnumeration
            return result.ToArray();
        }

        /// <summary>
        /// Метод приводит текстовое выражение к одному регистру. 
        /// </summary>
        public static string GetStringValue(string txt)
        {
            return string.IsNullOrEmpty(txt) ? txt : txt.ToUpper();
        }

        /// <summary>
        /// Инициализация VarFilter'а.
        /// </summary>
        /// <param name="filtertxt">значение фильтра из БД</param>
        /// <param name="filter">out фильтр по умолчанию</param>
        /// <param name="varFilter">out словарь, определяющий VarFilter</param>
        /// <param name="skipvarfilter">не определять VarFilter</param>
        public static void InitializeVarFilter(string filtertxt, out string filter, out Dictionary<string, Dictionary<string, string>> varFilter, bool skipvarfilter = false)
        {
            filter = filtertxt;
            varFilter = null;
            if (string.IsNullOrEmpty(filtertxt)) 
                return;
            var regexOptions = RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;
            //var regex = string.Format(@"\[\s*({0}*)\s*\]", Regexset);
            var regex = @"\[\s*(.[^\]]*)\s*\]";
            var expressions = Regex.Matches(filtertxt, regex, regexOptions);
            if (expressions.Count == 0) 
                return;

            var varfilters = new List<string>();
            var clearvarfilters = new List<string>();
            foreach (Match match in expressions)
            {
                if (match.Groups.Count <= 1) 
                    throw new DeveloperException(string.Format("Format of the filter '{0}' does not match regex expression '{1}'.", filtertxt, regex));
                varfilters.Add(match.Groups[0].Value);
                clearvarfilters.Add(match.Groups[1].Value);
            }

            filter = filter.Replace("\r", string.Empty).Replace("\n", string.Empty);
            filter = varfilters.Aggregate(filter, (current, p) => current.Replace(p, string.Empty));
            filter = filter.GetTrim();
            if (filter == string.Empty) 
                filter = null;
            if (filter != null)
            {
                //INFO: это странный код (gleb)
                filter = Regex.Replace(filter, ",", string.Empty);
                //Меняем ; на , в функциях 
                filter = filter.Replace(";", ",");
            }
            if (skipvarfilter) 
                return;

            regex = @"\$(\w*),";
            var regex2 = string.Format(@"\s*\(\s*({0}*)\s*,\s*({0}*)\s*\)", Regexset.Replace(",", string.Empty));
            varFilter = new Dictionary<string, Dictionary<string, string>>();
            foreach (var p in clearvarfilters)
            {
                expressions = Regex.Matches(p, regex, regexOptions);
                if (expressions.Count == 0 || expressions[0].Groups.Count <= 1) 
                    throw new DeveloperException(string.Format("Format of the subfilter '{0}' does not match regex expression '{1}'.", p, regex));
                var expressions2 = Regex.Matches(p, regex2, regexOptions);
                if (expressions2.Count == 0 || expressions2[0].Groups.Count <= 2) 
                    throw new DeveloperException(string.Format("Format of the subfilter '{0}' does not match regex expression '{1}'.", p, regex2));

                var value = new Dictionary<string, string>();
                foreach (Match match in expressions2)
                {
                    var valuekey = match.Groups[1].Value.GetTrim();
                    if (string.IsNullOrEmpty(valuekey)) 
                        valuekey = FilterValueNull;
                    value[GetStringValue(valuekey)] = match.Groups[2].Value.GetTrim().Replace(";", ","); //Меняем ; на , в функциях
                }

                var key = expressions[0].Groups[1].Value.GetTrim();
                if (string.IsNullOrEmpty(key)) 
                    throw new DeveloperException(string.Format("Property name is not define in subfilter '{0}'.", p));
                varFilter[GetStringValue(key)] = value;
            }

            if (!varFilter.Keys.Any())
                varFilter = null;
        }

        public static void InitializeVarFilter(string filtertxt, out string filter)
        {
            Dictionary<string, Dictionary<string, string>> varFilter;
            InitializeVarFilter(filtertxt, out filter, out varFilter, true);
        }

        public static string GetVirtualFieldName(Type sourceType, string propertyName)
        {
            var properties = TypeDescriptor.GetProperties(sourceType).Cast<PropertyDescriptor>();
            var property = properties.FirstOrDefault(i => i.Name.EqIgnoreCase(propertyName));
            if (property == null)
                return null;
            var virtualAttr = property.Attributes[typeof(LinkToVirtualFieldAttribute)] as LinkToVirtualFieldAttribute;
            return virtualAttr != null ? virtualAttr.VirtualFieldName : null;
        }

        public static string GetDispalyMemberValue(string lookUpCodeEditor, object editValue)
        {
            if (editValue == null)
                return null;

            var lookupInfo = GetLookupInfo(lookUpCodeEditor);

            //определяем тип лукапа
            var lookuptype = GetItemSourceType(lookupInfo);

            //определяем тип ObjectLookupPkey
            var prdesc = TypeDescriptor.GetProperties(lookuptype);
            var vmproperty = prdesc.Find(lookupInfo.ValueMember, true);
            if (vmproperty == null)
                return null;

            var dmproperty = prdesc.Find(lookupInfo.DisplayMember, true);
            if (dmproperty == null)
                return null;

            var comma = vmproperty.PropertyType == typeof(string) ? "'" : string.Empty;
            var vmsourcename = SourceNameHelper.Instance.GetPropertySourceName(vmproperty.PropertyType, vmproperty.Name);

            string filter = null;
            if (!string.IsNullOrEmpty(lookupInfo.Filter))
                InitializeVarFilter(lookupInfo.Filter, out filter);

            object[] lkObjects;
            using (var mng = GetItemSourceManager(lookupInfo))
            {
                lkObjects =
                    mng.GetFiltered(string.Format("{0} = {1}{2}{1}{3}", vmsourcename, comma, editValue,
                        string.IsNullOrEmpty(filter) ? null : " AND " + filter),
                        GetModeEnum.Partial).ToArray();
            }

            if (lkObjects.Length == 0)
                return null;

            var result = dmproperty.GetValue(lkObjects[0]);
            return result == null ? null : result.ToString();
        }

        /// <summary>
        /// Создание ограниченного шаблона для получения данных (только нужные поля).
        /// </summary>
        public static string GetAttrEntity(LookupInfo lookupInfo)
        {
            if (lookupInfo == null)
                throw new ArgumentNullException("lookupInfo");

            if (lookupInfo.ItemType == null)
                lookupInfo.ItemType = GetItemSourceType(lookupInfo);

            // если уже получали
            if (!string.IsNullOrEmpty(lookupInfo.AttrEntity))
                return lookupInfo.AttrEntity;

            // строим шаблон
            lookupInfo.AttrEntity = string.IsNullOrEmpty(lookupInfo.DisplayMember)
                ? FilterHelper.GetAttrEntity(lookupInfo.ItemType, lookupInfo.ValueMember)
                : FilterHelper.GetAttrEntity(lookupInfo.ItemType, lookupInfo.ValueMember, lookupInfo.DisplayMember);

            return lookupInfo.AttrEntity;
        }
    }
}
