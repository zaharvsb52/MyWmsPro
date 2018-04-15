namespace wmsMLC.General
{
    public interface IGetLookupInfo
    {
        /// <summary>
        /// Получить инфо по лукапу.
        /// </summary>
        LookupInfo GetLookupInfo(string lookUpCodeEditor);
    }
}
