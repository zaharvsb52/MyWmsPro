using System;
using wmsMLC.Business.Objects;

namespace wmsMLC.Business.Managers
{
    [Obsolete]
    public interface IMandantHandler
    {
        decimal? GetMandantCode(WMSBusinessObject bo);
    }
}
