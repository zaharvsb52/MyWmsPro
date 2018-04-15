using System.ComponentModel;

namespace wmsMLC.Business.Managers.Validation.Attributes
{
    public class ValidateStrategyContext
    {
        public ValidateStrategyContext(object obj, PropertyDescriptor property, string parameters, string value)
        {
            Parameters = parameters;
            Property = property;
            Obj = obj;
            Value = value;
        }

        /// <summary>
        /// объект над которым осуществляется проверка
        /// </summary>
        public object Obj { get; private set; }

        /// <summary>
        /// Свойство объекта, которое проверяется
        /// </summary>
        public PropertyDescriptor Property { get; private set; }

        /// <summary>
        /// Сообщение об ошибке. Может быть переопределено из стратегии
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Параметры проверки
        /// </summary>
        public string Parameters { get; private set; }

        /// <summary>
        /// Значение для проверки
        /// </summary>
        public string Value { get; private set; }
    }
}