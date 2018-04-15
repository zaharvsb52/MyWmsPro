using System;

namespace wmsMLC.General
{
    public interface ISysDbInfo : IDisposable
    {
        DbSysInfo DbSystemInfo { get; }
    }
}
