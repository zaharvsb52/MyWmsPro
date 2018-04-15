using System.Collections.Generic;
using System.Linq;
using wmsMLC.APS.wmsSI.Wrappers;

namespace wmsMLC.APS.wmsSI.Helpers
{
    internal static class MessageHelper
    {
        public const int WarningCode = 117;
        public const int ErrorCode = 1;
        public const int SuccessCode = 0;

        public static void AddMessage(ICollection<string> messages, int errorcode, List<ErrorWrapper> errMessages)
        {
            if (messages == null || messages.Count == 0 || errMessages == null)
                return;

            errMessages.AddRange(messages.Select(p => new ErrorWrapper
            {
                ERRORCODE = errorcode.ToString(),
                ERRORMESSAGE = string.Format("{0}{1}", errorcode == WarningCode ? "Предупреждение. " : null, p)
            }));
        }

        public static void AddMessage(ICollection<string> messages, List<ErrorWrapper> errMessages)
        {
            AddMessage(messages, WarningCode, errMessages);
        }

        public static void RemoveSuccessMessage(List<ErrorWrapper> errMessages)
        {
            if (errMessages == null || errMessages.Count == 0)
                return;

            foreach (var p in errMessages.Where(p => p.ERRORCODE == SuccessCode.ToString()).ToArray())
            {
                errMessages.Remove(p);
            }
        }

        public static int GetErrorCount(List<ErrorWrapper> errMessages)
        {
            return errMessages == null ? 0 : errMessages.Count(p => p.ERRORCODE == ErrorCode.ToString());
        }
    }
}
