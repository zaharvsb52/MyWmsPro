namespace wmsMLC.General
{
    /// <summary>
    /// Интерфейс проверки прав пользователя
    /// </summary>
    public interface ISecurityChecker
    {
        /// <summary>
        /// Проверка права пользователя на действие
        /// </summary>
        /// <param name="actionName">Имя действия</param>
        /// <returns>Разрешено</returns>
        bool Check(string actionName);

        /// <summary>
        /// Проверка права пользователя на действие
        /// </summary>
        /// <param name="actionName">Имя действия</param>
        /// <param name="userName">Имя пользователя</param>
        /// <returns>Разрешено</returns>
        bool Check(string actionName, string userName);
    }
}
