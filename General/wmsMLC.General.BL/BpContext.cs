using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using wmsMLC.General.Resources;

namespace wmsMLC.General.BL
{
    public class BpContext
    {
        public const string BpContextArgumentName = "BpContext";

        public BpContext()
        {
            Properties = new Dictionary<string, object>();
        }

        /// <summary>
        /// Коллекция объектов
        /// </summary>
        public object[] Items { get; set; }

        /// <summary>
        /// Не обновлять список объектов
        /// </summary>
        public bool DoNotRefresh { get; set; }

        /// <summary>
        /// Название бизнес процесса
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Дополнительные свойства БП.
        /// </summary>
        public IDictionary<string, object> Properties { get; private set; }

        public T Get<T>(string name)
        {
            if (string.IsNullOrEmpty(name) || !Properties.ContainsKey(name))
                return default(T);

            try
            {
                return (T)SerializationHelper.ConvertToTrueType(Properties[name], typeof(T));
            }
            catch (Exception ex)
            {
                throw new OperationException(ExceptionResources.TypeConversionError, "BpContext.", name, typeof(T), ex);
            }
        }

        public T[] GetArray<T>(string name)
        {
            if (string.IsNullOrEmpty(name))
                return default(T[]);

            if ("Items".EqIgnoreCase(name))
                return Items.Cast<T>().ToArray();

            if (!Properties.ContainsKey(name))
                return default(T[]);

            try
            {
                return (T[])SerializationHelper.ConvertToTrueType(Properties[name], typeof(T[]));
            }
            catch (Exception ex)
            {
                throw new OperationException(string.Format("Значение поля '{0}{1}' не может быть конвертировано в коллекцию типа '{2}'.", "BpContext.", name, typeof(T)), ex);
            }
        }

        public void Set<T>(string name, T value)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if ("Items".EqIgnoreCase(name))
            {
                var en = value as IEnumerable;
                Items = en == null 
                    ? null
                    : en.Cast<object>().ToArray();
            }
            else
                Properties[name] = value;
        }
    }
}
