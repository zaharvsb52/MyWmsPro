using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace wmsMLC.General.BL.Helpers
{
    public static class FilterHelper
    {
        /// <summary>
        /// Максимальная длина фильтра.
        /// </summary>
        public const int MaxLength = 4000;

        /// <summary>
        /// Разделитель ';'.
        /// </summary>
        public const string Semicolon = ";";

        private static readonly Lazy<ISqlExpressionHelper> _sqlExpressionHelper = new Lazy<ISqlExpressionHelper>(() => IoC.Instance.Resolve<ISqlExpressionHelper>());

        /// <summary>
        /// Формирует SQL выражение вхождения IN (...) элементов
        /// </summary>
        /// <param name="modelType">Тип объектов в коллекции</param>
        /// <param name="items">Коллекция объектов</param>
        /// <returns>Строка выражения 'xxx IN (...)'</returns>
        public static string GetFilterIn(Type modelType, IEnumerable<object> items)
        {
            var itemsArray = items as object[] ?? items.ToArray();
            var anyitemkh = itemsArray.FirstOrDefault() as IKeyHandler;
            if (anyitemkh == null)
                throw new DeveloperException("Фильтр можно получить только от элементов, для которых определен IKeyHandler");
            var sourceName = SourceNameHelper.Instance.GetPropertySourceName(modelType, anyitemkh.GetPrimaryKeyPropertyName());
            return GetFilterIn(sourceName, itemsArray.Cast<IKeyHandler>().Select(i => i.GetKey()));
        }

        /// <summary>
        /// Формирует SQL выражение вхождения IN (...) элементов
        /// </summary>
        /// <param name="keyName">Имя параметра</param>
        /// <param name="keys">Коллекция ключей</param>
        /// <returns>Строка выражения 'xxx IN (...)'</returns>
        public static string GetFilterIn(string keyName, IEnumerable<object> keys)
        {
            var itemsArray = keys as object[] ?? keys.ToArray();
            if (itemsArray.Length < 1)
                return null;

            // если не можем определить что нам передали
            var notNullItem = itemsArray.FirstOrDefault(i => i != null);
            if (notNullItem == null)
                return null;

            var type = notNullItem.GetType();
            var formatSql = (type == typeof(string) || type == typeof(Guid) ? "'{0}'" : "{0}");
            var sb = new StringBuilder(keyName + " IN (");
            for (int i = 0; i < itemsArray.Length; i++)
            {
                var key = itemsArray[i];
                var f1 = string.Format(formatSql, Normalize(key));
                if (i == 0)
                    sb.Append(f1);
                else
                    sb.Append(",").Append(f1);
            }
            return sb + ")";
        }

        /// <summary>
        /// Формирует коллекцию SQL выражений вхождения IN (...) элементов длиной до 4000 символов
        /// </summary>
        /// <param name="modelType">Тип объектов коллекции</param>
        /// <param name="items">Коллекция объектов</param>
        /// <param name="filterExt">Дополнительный фильтр</param>
        /// <returns>Коллекция строк выражений 'xxx IN (...) + доп. фильтр'</returns>
        public static string[] GetArrayFilterIn(Type modelType, IEnumerable<object> items, string filterExt = null)
        {
            var itemsArray = items as object[] ?? items.ToArray();
            var anyitemkh = itemsArray.FirstOrDefault() as IKeyHandler;
            if (anyitemkh == null)
                throw new DeveloperException("Фильтр можно получить только от элементов, для которых определен IKeyHandler");
            var sourceName = SourceNameHelper.Instance.GetPropertySourceName(modelType, anyitemkh.GetPrimaryKeyPropertyName());
            return GetArrayFilterIn(sourceName, itemsArray.Cast<IKeyHandler>().Select(i => i.GetKey()), filterExt);
        }

        /// <summary>
        /// Формирует коллекцию SQL выражений вхождения IN (...) элементов длиной до 4000 символов
        /// </summary>
        /// <param name="keyName">Имя параметра</param>
        /// <param name="keys">Коллекция ключей</param>
        /// <param name="filterExt">Дополнительный фильтр</param>
        /// <returns>Коллекция строк выражений 'xxx IN (...) + доп. фильтр'</returns>
        public static string[] GetArrayFilterIn(string keyName, IEnumerable<object> keys, string filterExt = null)
        {
            var result = new List<string>();
            var totalFilter = GetFilterIn(keyName, keys) + filterExt;
            if (string.IsNullOrEmpty(totalFilter))
                return null;
            if (totalFilter.Length <= MaxLength)
                result.Add(totalFilter);
            else
            {
                // сколько можно за раз
                var amount = Math.Abs(MaxLength * keys.Count() / totalFilter.Length);
                var tmp = totalFilter;
                while (tmp.Length > MaxLength && amount > 0)
                {
                    tmp = GetFilterIn(keyName, keys.Take(amount)) + filterExt;
                    if (tmp.Length > MaxLength)
                        amount--;
                }
                if (!string.IsNullOrEmpty(tmp))
                    result.Add(tmp);
                var half = keys.Skip(amount);
                if (half.Any())
                {
                    var filters = GetArrayFilterIn(keyName, half, filterExt);
                    if (filters.Length > 0)
                        result.AddRange(filters);
                }
            }
            return result.ToArray();
        }

        public static string GetFetchCountFilter(int fetchedRowsCount)
        {
            if (fetchedRowsCount < 0)
                throw new OperationException("Нельзя использовать отрицательные значения в ограничениях фильтра");

            return string.Format("ROWNUM<={0}", fetchedRowsCount);
        }

        public static string SurroundWithBrackets(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return string.Format("({0})", value);
        }

        public static string And(string left, params string[] right)
        {
            string filter = left;
            foreach (var s in right)
            {
                if (string.IsNullOrEmpty(s))
                    continue;

                filter += string.IsNullOrEmpty(filter) ? string.Empty : " AND ";
                filter += s;
            }

            return filter;
        }

        public static string Or(string left, params string[] right)
        {
            var operands = string.IsNullOrEmpty(left) ? 0 : 1;
            var filter = left;
            foreach (var s in right)
            {
                if (string.IsNullOrEmpty(s))
                    continue;

                filter += string.IsNullOrEmpty(filter) ? string.Empty : " OR ";
                filter += s;
                operands++;
            }

            return string.Format(operands > 1 ? "({0})" : "{0}", filter);
        }

        public static string ConvertGridFilter(string gridFilter, Type type = null)
        {
            var filter = gridFilter;

            // если мы знаем с какого типа получен фильтр - пытаемся заменить именя полей на SourceName
            if (type != null)
                filter = ReplaceOnSourceNames(gridFilter, type);

            return _sqlExpressionHelper.Value.GetSqlExpression(filter);
        }

        public static string ReplaceOnSourceNames(string filter, Type type)
        {
            // отбираем св-ва у которых не совпадают sourceName и displayName
            var properties = TypeDescriptor.GetProperties(type)
                .Cast<DynamicPropertyDescriptor>()
                .Where(i => i.SourceName != null && !i.Name.EqIgnoreCase(i.SourceName));

            var res = filter;
            foreach (var property in properties)
            {
                var propFilter = string.Format("[{0}]", property.Name.ToUpper());
                var sourceFilter = string.Format("[{0}]", property.SourceName.ToUpper());
                res = res.Replace(propFilter, sourceFilter);
            }
            return res;
        }

        public static string ConstructEquals(string fieldName, object value)
        {
            return _sqlExpressionHelper.Value.ConstructEqualsWithValue(fieldName, value);
        }

        public static string ConstructNonEquals(string fieldName, object value)
        {
            return _sqlExpressionHelper.Value.ConstructNonEqualsWithValue(fieldName, value);
        }

        public static object Normalize(object value)
        {
            var str = value as string;
            if (!string.IsNullOrEmpty(str) && str.Contains("'"))
                return str.Replace("'", "''");
            return value;
        }

        public static string GetAttrEntity<T>(params string[] propertiesNames)
        {
            return GetAttrEntity(typeof(T), propertiesNames);
        }

        public static string GetAttrEntity(Type type, params string[] propertiesNames)
        {
            var sb = new StringBuilder();

            // вычисляем корень
            var rootName = SerializationHelper.GetXmlRootName(type);
            sb.Append(string.Format("<{0}>", rootName));

            // получаем св-ва
            var properties = TypeDescriptor.GetProperties(type);

            // если хотим полный объект
            if (propertiesNames == null || propertiesNames.Length == 0)
            {
                foreach (DynamicPropertyDescriptor desc in properties)
                    sb.Append(GetAttrEntityItemValue(desc));
            }
            else
            {
                // а если передали список св-в, то берем только их
                foreach (var name in propertiesNames)
                {
                    var desc = properties.Find(name, true) as DynamicPropertyDescriptor;
                    if (desc == null)
                        throw new DeveloperException("Can't create AttrEntity with property '{0}'. Property with that name not found in type '{1}' or it's not a {2}",
                            name, type.Name, typeof (DynamicPropertyDescriptor).Name);

                    sb.Append(GetAttrEntityItemValue(desc));
                }
            }
            sb.Append(string.Format("</{0}>", rootName));

            // обязательно возвращаем в верхнем регистре - иначе ничего не работает
            return sb.ToString().ToUpper();
        }

        private static string GetAttrEntityItemValue(DynamicPropertyDescriptor desc)
        {
            if (typeof (ICollection).IsAssignableFrom(desc.PropertyType))
            {
                var itemType = desc.PropertyType.GetCollectionElementType();
                if (itemType == null)
                    throw new DeveloperException("Can't get element type from collection with type " + desc.PropertyType);

                var innerAttrEntity = GetAttrEntity(itemType);
                return string.Format("<{0}>{1}</{0}>", desc.SourceName, innerAttrEntity);
            }

            return string.Format("<{0} />", desc.SourceName);
        }

        public static string ValidateFilterString(string text)
        {
            const char apch = '\'';

            if (string.IsNullOrEmpty(text) || !text.Contains(apch))
                return text;

            var res = new Dictionary<int, int>();
            var count = 0;
            var index = 0;
            for (var i = 0; i < text.Length; i++)
            {
                if (text[i] == apch)
                {
                    if (count == 0)
                        index = i;
                    res[index] = ++count;
                }
                else
                {
                    count = 0;
                }
            }

            var offset = 0;
            var result = new List<char>(text.ToCharArray());
            foreach (var p in res.Where(p => p.Value % 2 != 0))
            {
                result.Insert(p.Key + offset++, apch);
            }

            return string.Join(string.Empty, result);
        }
    }
}
