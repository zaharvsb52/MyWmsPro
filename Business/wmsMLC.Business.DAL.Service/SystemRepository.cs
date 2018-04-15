using System;
using wmsMLC.General.DAL.Service;

namespace wmsMLC.Business.DAL.Service
{
    public class SystemRepository : BaseRepository, ISystemRepository
    {
        private TimeSpan _pingSpan;

        public void Ping()
        {
            throw new NotImplementedException("Obsolete");
        }

        public int GetLastPingTime()
        {
            return _pingSpan.Milliseconds;
        }

        public void SendMessage(string subject, string message)
        {
            throw new NotImplementedException("Obsolete");
        }
    }
}