using System;

namespace wmsMLC.APS.wmsRS
{
    internal static class Global
    {
        public const string ScheduleId = "SCHEDULEID";

        internal static bool ValidateDateRange(DateTime now, DateTime? begin, DateTime? end)
        {
            if (begin.HasValue && end.HasValue && begin.Value > end.Value)
                return false;

            var result = true;
            if (begin.HasValue)
                result = now >= begin.Value;

            if (!result)
                return false;

            if (end.HasValue)
                result = now < end.Value;
            return result;
        }
    }
}
