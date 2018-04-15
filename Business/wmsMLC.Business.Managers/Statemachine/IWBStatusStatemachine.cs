using System;
using wmsMLC.Business.General;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Business.Managers.Statemachine
{
    public class IWBStatusStatemachine : IStatusStatemachine
    {
        public object GetNextState(object state, string action)
        {
            IWBStates currentState;
            var nextState = IWBStates.IWB_NONE;
            if (!Enum.TryParse(state.ToString(), out currentState))
                throw new DeveloperException("Unknown state {0}", state);

            switch (currentState)
            {
                case IWBStates.IWB_NONE:
                    if (!(action.EqIgnoreCase(WMSBusinessObjectManager<WMSBusinessObject, int>.InsertMethodName) ||
                          action.EqIgnoreCase(WMSBusinessObjectManager<WMSBusinessObject, int>.UpdateMethodName)))
                        nextState = IWBStates.IWB_ERROR;
                    else
                        nextState = IWBStates.IWB_CREATED;
                    break;

                case IWBStates.IWB_CREATED:
                    if (action.EqIgnoreCase(WMSBusinessObjectManager<WMSBusinessObject, int>.InsertMethodName) ||
                        action.EqIgnoreCase(WMSBusinessObjectManager<WMSBusinessObject, int>.UpdateMethodName))
                    {
                        nextState = IWBStates.IWB_CREATED;
                    }
                    else if (action.EqIgnoreCase("Cancel"))
                    {
                        nextState = IWBStates.IWB_CANCELED;
                    }
                    else if (action.EqIgnoreCase("Activate"))
                    {
                        nextState = IWBStates.IWB_ACTIVATED;
                    }
                    else
                    {
                        nextState = IWBStates.IWB_ERROR;
                    }
                    break;

                case IWBStates.IWB_ACTIVATED:
                    if (action.EqIgnoreCase("Complete"))
                    {
                        nextState = IWBStates.IWB_COMPLETED;
                    }
                    else if (action.EqIgnoreCase("Cancel"))
                    {
                        nextState = IWBStates.IWB_CANCELED;
                    }
                    else
                    {
                        nextState = IWBStates.IWB_ERROR;
                    }
                    break;
            }

            if (nextState == IWBStates.IWB_ERROR)
                throw new DeveloperException("Действие {0} не доступно в состоянии {1}", action, state);

            return nextState;
        }
    }
}
