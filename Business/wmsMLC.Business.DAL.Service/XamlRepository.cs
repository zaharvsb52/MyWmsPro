using wmsMLC.General.DAL.Service;
using wmsMLC.General.DAL.Service.Telegrams;

namespace wmsMLC.Business.DAL.Service
{
    public abstract class XamlRepository<T, TKey> : BaseHistoryRepository<T, TKey>, IXamlRepository<T, TKey> where T : class, new()
    {
        public virtual string GetXaml(TKey pKey)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(string), IsOut = true };
            var pKeyParam = new TransmitterParam { Name = "pKey", Type = typeof(TKey), IsOut = false, Value = pKey };
            var telegram = new RepoQueryTelegramWrapper(typeof(T).Name, "GetXaml", new[] { resultParam, pKeyParam });
            ProcessTelegramm(telegram);
            return resultParam.Value.ToString();
        }

        public virtual void SetXaml(TKey pKey, string xaml)
        {
            var pKeyParam = new TransmitterParam { Name = "pKey", Type = typeof(TKey), IsOut = false, Value = pKey };
            var xamlParam = new TransmitterParam { Name = "xaml", Type = typeof(string), IsOut = false, Value = xaml };
            var telegram = new RepoQueryTelegramWrapper(typeof(T).Name, "SetXaml", new[] { pKeyParam, xamlParam });
            ProcessTelegramm(telegram);
        }
    }
}