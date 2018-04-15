namespace wmsMLC.General.BL
{
    public interface IStateChange
    {
        void ChangeState(object entity, string operationName);
        void ChangeStateByKey(object entityKey, string operationName);
    }
}
