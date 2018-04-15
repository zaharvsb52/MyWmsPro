using System;
using wmsMLC.General;

namespace wmsMLC.Business.Managers.Processes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class BPAttribute : RealyAllowMultipleAttribute
    {
    }
}
