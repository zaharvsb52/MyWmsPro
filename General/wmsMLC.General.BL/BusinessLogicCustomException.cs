using System;

namespace wmsMLC.General.BL
{
    public class BusinessLogicCustomException : BaseException, ICustomException
    {
         #region  .  Constructors  .
        public BusinessLogicCustomException() { }
        public BusinessLogicCustomException(string message) : base(message) { }
        public BusinessLogicCustomException(string message, Exception innerException) : base(message, innerException) { }
        #endregion  .  Constructors  .
    }
}
