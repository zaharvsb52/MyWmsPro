using System;

namespace wmsMLC.Business.Managers
{
    public interface IClientSessionManager : IDisposable
    {
        string GetClientInputPlaceValue();
        bool CloseRclSession(string clientCode, string truckCode);
        void UpdateWorker(decimal clientSessionId, decimal? workerId);
    }
}