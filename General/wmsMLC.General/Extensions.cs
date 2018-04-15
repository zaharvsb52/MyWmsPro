using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace wmsMLC.General
{
    public static class Extensions
    {
        // кэш для метода IsNullable (с ним работает гораздо быстрее)
        private static readonly ConcurrentDictionary<Type, bool> IsNullableCache = new ConcurrentDictionary<Type, bool>();

        // кэш для метода GetNonNullableType (с ним работает гораздо быстрее)
        private static readonly ConcurrentDictionary<Type, Type> GetNonNullableTypeCache = new ConcurrentDictionary<Type, Type>();

        public static bool IsNullOrEmptyAfterTrim(this string source)
        {
            return (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(source.Trim()));
        }

        public static string GetTrim(this string source)
        {
            return (string.IsNullOrEmpty(source) ? string.Empty : source.Trim());
        }

        public static bool EqIgnoreCase(this string a, string b, bool usetrim = false, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            if (usetrim)
                return string.Equals(a.GetTrim(), b.GetTrim(), stringComparison);
            return string.Equals(a, b, stringComparison);
        }

        /// <summary>
        /// Возвращает текст заданной длины.
        /// </summary>
        public static string Left(this string text, int lenght, bool istrim = false)
        {
            if (lenght <= 0) throw new ArgumentException("Argument 'lenght' should be more than zero.");
            if (string.IsNullOrEmpty(text)) return text;
            var result = (istrim ? text.Trim() : text);
            return (result.Length > lenght ? result.Substring(0, lenght) : result);
        }

        /// <summary>
        /// Возвращает имя свойства данного класса из выражения типа: () => Property.
        /// </summary>
        public static string ExtractPropertyName<T>(this Type assignableFrom, Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException(@"propertyExpression");
            }

            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null)
            {
                const string message = "The expression is not a member access expression.";
                throw new ArgumentException(message, "propertyExpression");
            }

            var property = memberExpression.Member as PropertyInfo;
            if (property == null)
            {
                const string message = "The member access expression does not access a property.";
                throw new ArgumentException(message, "propertyExpression");
            }

            if (assignableFrom != null && property.DeclaringType != null && !property.DeclaringType.IsAssignableFrom(assignableFrom))
            {
                const string message = "The referenced property belongs to a different type.";
                throw new ArgumentException(message, "propertyExpression");
            }

            var getMethod = property.GetGetMethod(true);
            if (getMethod == null)
            {
                const string message = "The referenced property does not have a get method.";
                throw new ArgumentException(message, "propertyExpression");
            }

            if (getMethod.IsStatic)
            {
                const string message = "The referenced property is a static property.";
                throw new ArgumentException(message, "propertyExpression");
            }

            return memberExpression.Member.Name;
        }

        #region To
        [DebuggerNonUserCode]
        public static TDestType To<TSourceType, TDestType>(TSourceType obj)
            where TSourceType : IConvertible
            where TDestType : IConvertible
        {
            return obj.To(default(TDestType));
        }

        [DebuggerNonUserCode]
        public static TDestType To<TDestType>(this object obj) where TDestType : IConvertible
        {
            return obj.To(default(TDestType));
        }

        [DebuggerNonUserCode]
        public static TDestType To<TDestType>(this object obj, TDestType defaultValue, bool ignoreCase = true) where TDestType : IConvertible
        {
            if (typeof(TDestType) == Convert.DBNull.GetType())
            {
                return default(TDestType);
            }

            if (typeof(TDestType).BaseType == typeof(Enum))
            {
                try
                {
                    if (obj != null)
                    {
                        var result = (TDestType)Enum.Parse(typeof(TDestType), obj.ToString(), ignoreCase);
                        if (Enum.IsDefined(typeof(TDestType), result))
                        {
                            return result;
                        }
                    }
                    return defaultValue;
                }
                catch (Exception)
                {
                    return defaultValue;
                }
            }

            try
            {
                var objinternal = obj;
                if (typeof(TDestType) == typeof(string))
                    objinternal = obj == null ? defaultValue : (object)Convert.ToString(obj);

                var result = (TDestType)Convert.ChangeType(objinternal, typeof(TDestType), System.Threading.Thread.CurrentThread.CurrentCulture);
                return (Equals(result, null) ? defaultValue : result);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
        #endregion To

        /// <summary>
        /// Add a range of items to a collection.
        /// </summary>
        /// <typeparam name="T">Type of objects within the collection.</typeparam>
        /// <param name="collection">The collection to add items to.</param>
        /// <param name="items">The items to add to the collection.</param>
        /// <returns>The collection.</returns>
        /// <exception cref="System.ArgumentNullException">An <see cref="System.ArgumentNullException"/> is thrown if <paramref name="collection"/> or <paramref name="items"/> is <see langword="null"/>.</exception>
        public static Collection<T> AddRange<T>(this Collection<T> collection, IEnumerable<T> items)
        {
            if (collection == null) 
                throw new System.ArgumentNullException("collection");
            if (items == null) 
                throw new System.ArgumentNullException("items");

            foreach (var each in items)
            {
                collection.Add(each);
            }

            return collection;
        }

        // INFO: если у проперти есть атрибуты DisplayName, то он не клонируется.
        // TODO: надо разобраться!!!
        public static T Clone<T>(this T obj, Type type = null, bool skipReadOnly = true) where T : class
        {
            if (obj == null)
                return null;

            // если объект умеет сам себя клонировать - пусть действует
            var clonable = obj as ICloneable;
            if (clonable != null)
                return (T)clonable.Clone();

            var result = type == null ? Activator.CreateInstance(obj.GetType()) : Activator.CreateInstance(type);

            // если перед нами коллекция
            var cln = obj as IList;
            if (cln != null)
            {
                foreach (var item in cln)
                    ((IList) result).Add(item.Clone(item.GetType()));
            }

            // копируем св-ва
            var propDescrs = TypeDescriptor.GetProperties(obj);
            foreach (PropertyDescriptor property in propDescrs)
            {
                if (skipReadOnly && property.IsReadOnly)
                    continue;

                var value = property.GetValue(obj);
                if (property.PropertyType.IsValueType || value == null)
                    property.SetValue(result, value);
                else
                    property.SetValue(result, value);
            }
            return (T) result;
        }

        #region .  TypeEx  .
        public static string GetDisplayName(this Type type)
        {
            if (type == null) 
                return null;
            var attributes = TypeDescriptor.GetAttributes(type);
            var att = attributes[typeof(DisplayNameAttribute)] as DisplayNameAttribute;
            var result = att == null ? type.Name : att.DisplayName;
            return result.IsNullOrEmptyAfterTrim() ? type.Name : result;
        }

        public static Type GetNonNullableType(this Type type)
        {
            return GetNonNullableTypeCache.GetOrAdd(type, t => !t.IsNullable() 
                ? t 
                : !t.IsGenericType 
                    ? t
                    : type.GetGenericArguments()[0]);
        }

        public static bool IsNullable(this Type type)
        {
            return IsNullableCache.GetOrAdd(type, t => !t.IsValueType || (t.IsGenericType && Nullable.GetUnderlyingType(t) != null));
            //return type.IsGenericType && type.Name.Star.tsWith("Nullable");// Nullable.GetUnderlyingType(type) != null;
            //return type.IsGenericType && Nullable.GetUnderlyingType(type) != null;
            //return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool IsFloatType(this Type type)
        {
            var notNullableType = type.GetNonNullableType();
            return notNullableType == typeof (float) ||
                   notNullableType == typeof (double) ||
                   notNullableType == typeof (decimal);
        }

        public static bool IsNumeric(this Type type)
        {
            var typeCode = Type.GetTypeCode(type);

            switch (typeCode)
            {
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
            }

            return false;
        }

        public static bool IsNullAssignable(this Type type)
        {
            return !type.IsValueType || type.IsNullable();
        }

        public static Type GetNullAssignableType(this Type type)
        {
            if (!type.IsNullAssignable())
                return typeof(Nullable<>).MakeGenericType(type);

            return type;
        }

        public static Type GetGenericTypeFormInheritanceNode(this Type type, Type genericType)
        {
            if (genericType == null)
                throw new ArgumentNullException("genericType");

            if (type.IsGenericType && type.GetGenericTypeDefinition() == genericType)
                return type;

            if (type.BaseType == null)
                return null;

            return GetGenericTypeFormInheritanceNode(type.BaseType, genericType);
        }

        public static string GetFullNameWithoutVersion(this Type type)
        {
            if (type == null)
                return null;

            var genericArgs = new List<string>();
            if (type.IsGenericType)
            {
                foreach (var p in type.GetGenericArguments())
                {
                    genericArgs.Add(p.GetFullNameWithoutVersion());
                }
            }

            return string.Format("{0}.{1}{2}", type.Namespace, type.Name,
                genericArgs.Count > 0 ? string.Format("<{0}>", string.Join(",", genericArgs.ToArray())) : null);
        }

        #endregion .  TypeEx  .

        public static Guid FlipEndian(this Guid guid)
        {
            var newBytes = new byte[16];
            var oldBytes = guid.ToByteArray();

            for (var i = 8; i < 16; i++)
                newBytes[i] = oldBytes[i];

            newBytes[3] = oldBytes[0];
            newBytes[2] = oldBytes[1];
            newBytes[1] = oldBytes[2];
            newBytes[0] = oldBytes[3];
            newBytes[5] = oldBytes[4];
            newBytes[4] = oldBytes[5];
            newBytes[6] = oldBytes[7];
            newBytes[7] = oldBytes[6];

            return new Guid(newBytes);
        }
    }

    public static class ConcurrentDictionaryEx
    {
        public static V GetOrAddSafe<T, V>(this ConcurrentDictionary<T, Lazy<V>> dictionary, T key, Func<T, V> valueFactory)
        {
            Lazy<V> lazy = dictionary.GetOrAdd(key, new Lazy<V>(() => valueFactory(key)));
            return lazy.Value;
        }

        public static V AddOrUpdateSafe<T, V>(this ConcurrentDictionary<T, Lazy<V>> dictionary, T key, Func<T, V> addValueFactory, Func<T, V, V> updateValueFactory)
        {
            Lazy<V> lazy = dictionary.AddOrUpdate(key,
                new Lazy<V>(() => addValueFactory(key)),
                (k, oldValue) => new Lazy<V>(() => updateValueFactory(k, oldValue.Value)));
            return lazy.Value;
        }
    }

    public static class DebugDumps
    {
        public static string DumpToXML(this object obj)
        {
            var xmlSerializer = new XmlSerializer(obj.GetType());

            using (var memStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memStream))
                {
                    xmlSerializer.Serialize(streamWriter, obj);
                    memStream.Position = 0;
                    using (var streamReader = new StreamReader(memStream))
                    {
                        var serializedXml = new XmlDocument();
                        serializedXml.Load(streamReader);
                        return serializedXml.InnerXml;
                    }
                }
            }
        }
    }

    public static class MemberInfoEx
    {
        public static T[] GetCustomAttributes<T>(this MemberInfo methodInfo, bool inherit)  where T : class
        {
            return methodInfo.GetCustomAttributes(typeof (T), inherit).Cast<T>().ToArray();
        }

        public static T GetOneCustomAttributes<T>(this MemberInfo methodInfo, bool throwExceptionOnNotExists = false, bool inherit = true) where T : class
        {
            var items = methodInfo.GetCustomAttributes(typeof(T), inherit).Cast<T>().ToArray();
            if (items.Length == 0 || (!throwExceptionOnNotExists && items.Length > 1))
                return null;

            if (items.Length > 1)
                throw new DeveloperException("More than one attribute {0} for member {1}", typeof (T).Name, methodInfo.Name);

            return items[0];
        }
    }

    public static class TypeEx
    {
        public static Type GetCollectionElementType(this Type type)
        {
            var interfaces = type.GetInterfaces();
            foreach (var ifc in interfaces.Where(i => i.IsGenericType))
            {
                if (ifc.GetGenericTypeDefinition() == typeof (ICollection<>)
                    || ifc.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                    || ifc.GetGenericTypeDefinition() == typeof(IList<>))
                    return ifc.GetGenericArguments()[0];
            }
            return null;
        }
    }
}
