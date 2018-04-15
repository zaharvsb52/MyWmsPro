namespace wmsMLC.General
{
    public interface ISqlExpressionHelper
    {
        /// <summary>
        /// Получение sql выражения по выражению фильтра
        /// </summary>
        /// <param name="filterExpression">выражение в формате FilterControl</param>
        /// <returns>sql выражение</returns>
        string GetSqlExpression(string filterExpression);

        /// <summary>
        /// Получение sql выражения по выражению фильтра
        /// </summary>
        /// <param name="filterIn">выражение в формате FilterControl</param>
        /// <param name="filterOut">sql выражение</param>
        /// <returns>валидность входного фильтра</returns>
        bool TryGetSqlExpression(string filterIn, out string filterOut);

        /// <summary>
        /// Получение sql выражения равенства поля значению
        /// Поддерживает is Null, in (...), =
        /// </summary>
        /// <param name="fieldName">имя поля</param>
        /// <param name="value">значение (м.б. null, IEnumerable)</param>
        /// <returns>sql выражение в формате {fieldName}{operator}{value} </returns>
        string ConstructEqualsWithValue(string fieldName, object value);

        /// <summary>
        /// Получение sql выражения неравенства поля значению
        /// Поддерживает is Null, not in (...) 
        /// </summary>
        /// <param name="fieldName">имя поля</param>
        /// <param name="value">значение (м.б. null, IEnumerable)</param>
        /// <returns>sql выражение в формате {fieldName}{operator}{value} </returns>
        string ConstructNonEqualsWithValue(string fieldName, object value);
    }
}