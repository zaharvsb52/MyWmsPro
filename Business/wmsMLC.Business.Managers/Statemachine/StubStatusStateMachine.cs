using wmsMLC.Business.General;

namespace wmsMLC.Business.Managers.Statemachine
{
    public class StubStatusStateMachine : IStatusStatemachine
    {
        public object GetNextState(object state, string action)
        {
            return state;
        }
    }
}
