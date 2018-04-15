namespace wmsMLC.General.BL
{
    /// <summary>
    /// Интерфейс объявления метода принудительного вызова уведомления об изменении св-ва
    /// </summary>
    public interface IRaisePropertyChanged
    {
        void RaisePropertyChanged(string propertyName);
    }
}