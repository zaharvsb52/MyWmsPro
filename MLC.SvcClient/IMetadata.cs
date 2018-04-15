using System.Collections.Generic;

namespace MLC.SvcClient
{
    public interface IMetadata
    {
        IEnumerable<string> GetActionNames();
        IEnumerable<string> GetMethodNames(string action);
    }
}