using System;
using wmsMLC.Business.General;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Business.Managers.Statemachine
{
    public class TransportTaskStatusStatemachine : IStatusStatemachine
    {
        public object GetNextState(object state, string action)
        {
            TTaskStates currentState;
            var nextState = TTaskStates.TTASK_NONE;
            if (!Enum.TryParse(state.ToString(), out currentState))
                throw new DeveloperException("Unknown state {0}.", state);

            switch (currentState)
            {
                case TTaskStates.TTASK_NONE:
                    if (!(action.EqIgnoreCase(WMSBusinessObjectManager<WMSBusinessObject, int>.InsertMethodName) ||
                          action.EqIgnoreCase(WMSBusinessObjectManager<WMSBusinessObject, int>.UpdateMethodName)))
                        nextState = TTaskStates.TTASK_NONE;
                    else
                        nextState = TTaskStates.TTASK_CREATED;
                    break;

                case TTaskStates.TTASK_CREATED:
                    if (action.EqIgnoreCase(WMSBusinessObjectManager<WMSBusinessObject, int>.InsertMethodName) ||
                        action.EqIgnoreCase(WMSBusinessObjectManager<WMSBusinessObject, int>.UpdateMethodName))
                    {
                        nextState = TTaskStates.TTASK_CREATED;
                    }
                    else if (action.EqIgnoreCase("Cancel"))
                    {
                        nextState = TTaskStates.TTASK_CANCELED;
                    }
                    else if (action.EqIgnoreCase("Activate"))
                    {
                        nextState = TTaskStates.TTASK_ACTIVATED;
                    }
                    else
                    {
                        nextState = TTaskStates.TTASK_NONE;
                    }
                    break;

                case TTaskStates.TTASK_ACTIVATED:
                    if (action.EqIgnoreCase("Complete"))
                    {
                        nextState = TTaskStates.TTASK_COMPLETED;
                    }
                    else if (action.EqIgnoreCase("Cancel"))
                    {
                        nextState = TTaskStates.TTASK_CANCELED;
                    }
                    else
                    {
                        nextState = TTaskStates.TTASK_NONE;
                    }
                    break;
            }

            //if (nextState == IWBStates.IWB_ERROR)
            //    throw new DeveloperException("Действие {0} не доступно в состоянии {1}", action, state);

            return nextState;
        }
    }
}
