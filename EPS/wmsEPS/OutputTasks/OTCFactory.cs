using System;
using wmsMLC.Business.Objects;

namespace wmsMLC.EPS.wmsEPS.OutputTasks
{
    public class OTCFactory
    {
        public IEpsOutputTask Create(EpsTaskType type)
        {
            switch (type)
            {
                case EpsTaskType.OTC_PRINT:
                    return new EpsOtcPrint();
                case EpsTaskType.OTC_MAIL:
                    return new EpsOtcMail();
                case EpsTaskType.OTC_SHARE:
                    return new EpsOtcShare();
                case EpsTaskType.OTC_FTP:
                    return new EpsOtcFtp();
                case EpsTaskType.OTC_DCL:
                    return new EpsOtcDcl();
                default:

                    throw new ArgumentException("type"); // нет такой задачи
            }
        }
    }
}