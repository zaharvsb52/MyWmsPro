namespace wmsMLC.Business.General
{
    public interface IStatusStatemachine
    {
        object GetNextState(object state, string action);
    }
}
