using System.Collections.Generic;

namespace wmsMLC.Business.Managers.Expressions
{
    /// <summary>
    /// Класс, объединяющий набор диапазонов
    /// </summary>
    public class RangeGroup
    {
        public RangeGroup()
        {
            Ranges = new List<Range>();
        }

        /// <summary>
        /// Имя набора
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Текстовое представление из которого был получен набор
        /// </summary>
        public string ExpressionText { get; set; }

        /// <summary>
        /// Коллекция диапазонов
        /// </summary>
        public List<Range> Ranges { get; private set; }
    }
}