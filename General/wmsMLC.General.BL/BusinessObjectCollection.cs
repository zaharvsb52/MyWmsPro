using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace wmsMLC.General.BL
{
    /// <summary>
    /// Базовый класс для типизированных коллекций
    /// <remarks>ITypedList реализует проброс информации о динамических св-вах элементов. </remarks>
    /// </summary>
    /// <typeparam name="T">Тип элемента коллекции</typeparam>
    public abstract class BusinessObjectCollection<T> : BaseObservableCollection<T>, ITypedList
    {
        #region .  Constructors  .
        protected BusinessObjectCollection() { }
        protected BusinessObjectCollection(IEnumerable<T> collection) : base(collection) { }
        #endregion

        #region .  ITypedList  .
        public string GetListName(PropertyDescriptor[] listAccessors)
        {
            return "List of " + typeof(T).Name;
        }

        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            // если объект может сам о себе рассказать - пусть рассказывает
            if (typeof (ICustomTypeDescriptor).IsAssignableFrom(typeof (T)))
            {
                var simpleItem = (ICustomTypeDescriptor)Activator.CreateInstance<T>();
                return simpleItem.GetProperties();
            }
            
            // так будет "по умолчанию"
            return TypeDescriptor.GetProperties(typeof(T));
        } 
        #endregion
    }
}