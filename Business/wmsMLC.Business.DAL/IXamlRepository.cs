using wmsMLC.General.DAL;

namespace wmsMLC.Business.DAL
{
    public interface IXamlRepository<T, in TKey> : IRepository<T, TKey>
    {
        string GetXaml(TKey pKey);

        void SetXaml(TKey pKey, string xaml);
    }
}
