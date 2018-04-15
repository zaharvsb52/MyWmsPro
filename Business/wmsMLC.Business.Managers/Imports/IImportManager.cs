using System;
using System.Collections.Generic;
using wmsMLC.Business.General;
using wmsMLC.Business.Managers.Processes;

namespace wmsMLC.Business.Managers.Imports
{
    public interface IImportManager
    {
        ImportObject ProcessImport(string xmlString);
        void SetApiUri(string uri);
        void SetCredentials(string userName, string pwd);
        CompleteContext ExecuteWfByCode(string wfcode, Dictionary<string, object> parameters, TimeSpan? timeout);
    }
}