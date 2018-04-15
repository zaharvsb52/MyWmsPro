using System;
using System.Collections.Generic;

namespace wmsMLC.DCL.WorkflowDesigner
{
    public interface IActivityLoader
    {
        void LoadActivities(bool clear = true);

        IDictionary<string, Type> GetActivities();
    }
}
