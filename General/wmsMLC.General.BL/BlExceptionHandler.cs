using System;

namespace wmsMLC.General.BL
{
    public static class BlExceptionHandler
    {
        //TODO: Сделать Enum.
        public const string BusinessLogicPolicyName = "BusinessLogicPolicy";

        public static bool HandleException(ref Exception ex)
        {
            var result = ExceptionPolicy.Instance.HandleException(ex, BusinessLogicPolicyName);
            if (ex is PassThroughException || ex is AggregateException)
                return result;

//            var dalException = ex as DALException;
//            if (dalException != null)
//                ex = ProcessDALException(dalException);

            return result;
        }
    }
}
