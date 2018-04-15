namespace wmsMLC.APS.wmsSI.Wrappers
{
    /// <summary>
    /// Интерфейс для записи пустого поля
    /// </summary>
    public interface IBaseWrapper
    {
        /// <summary>
        /// Если нужно записать в поле значение null
        /// </summary>
        bool? UpdateNullValues { get; set; }
    }
}
