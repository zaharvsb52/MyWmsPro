namespace wmsMLC.General.BL
{
    public interface IEditable
    {
        bool IsDirty { get; }

        /// <summary>
        /// Принять изменения объекта
        /// </summary>
        /// <param name="isNew">по умолчанию выставляет объект как не новый</param>
        void AcceptChanges(bool isNew = false);
        /// <summary>
        /// Метод вызывать если есть дозагрузка объекта
        /// </summary>
        /// <param name="propertyName"></param>
        void AcceptChanges(string propertyName);
        void RejectChanges();

        bool IsInRejectChanges { get; }

        bool GetPropertyIsDirty(string propertyName);
    }
}
