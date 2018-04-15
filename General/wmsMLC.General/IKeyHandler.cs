namespace wmsMLC.General
{
    public interface IKeyHandler
    {
        string GetPrimaryKeyPropertyName();
        object GetKey();
        TKey GetKey<TKey>();
        void SetKey(object o);
        bool HasPrimaryKey();
    }
}