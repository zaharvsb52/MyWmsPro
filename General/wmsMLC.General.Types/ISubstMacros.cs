namespace wmsMLC.General.Types
{

    /// <summary>
    /// Интерфейс для классов, используемых в подстановке макросов
    /// </summary>
    interface ISubstMacros
    {
        ///<remarks>Доступ к списку макросов</remarks>
        System.Collections.Generic.SortedList<string, string> Macroses { get; set; }
        ///<remarks>Добавить макрос</remarks>
        void SetMacro(string name, string value);
        ///<remarks>Добавить макрос</remarks>
        void SetMacro(string name, object value);
        ///<remarks>Возвращает результат подстановки макросов в исходную строку</remarks>
        string Substitute(string inputString);
        ///<remarks>Возвращает результат подстановки макросов в исходную строку типа StringBuilder</remarks>
        System.Text.StringBuilder Substitute(System.Text.StringBuilder inputSb);
        ///<remarks>Удалить макрос</remarks>
        void UnsetMacro(string name);
    }
}
