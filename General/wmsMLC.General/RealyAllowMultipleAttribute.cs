using System;

namespace wmsMLC.General
{
    /// <summary>
    /// Класс для борьбы с "фитчей" от MS. Нельзя повесить на динамический member боле одного аттрибута одного типа
    /// </summary>
    public abstract class RealyAllowMultipleAttribute : Attribute
    {
        /// <summary>
        /// Уникальный идентификатор аттрибута.
        /// <remarks>
        /// В базовом классе он равен GetType(). При использовании такого подхода в TypeDescriptor можно добавть только один аттрибут
        /// каждого типа, что меня совершенно не устраивает.
        /// link: http://social.msdn.microsoft.com/Forums/en-US/winforms/thread/e6bb4146-eb1a-4c1b-a5b1-f3528d8a7864/
        /// </remarks>
        /// </summary>
        public override object TypeId
        {
            get
            {
                return Guid.NewGuid();
            }
        }
    }
}