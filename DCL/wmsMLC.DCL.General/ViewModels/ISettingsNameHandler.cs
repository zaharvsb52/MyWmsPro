namespace wmsMLC.DCL.General.ViewModels
{
    /// <summary>
    /// Интерфейс позволяющий влиять на имя настройки (может использоваться как во View, так и во ViewModel).
    /// </summary>
    public interface ISettingsNameHandler
    {
        string GetSuffix();
    }
}
