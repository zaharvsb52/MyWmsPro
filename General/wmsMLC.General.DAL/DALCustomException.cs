using System;

namespace wmsMLC.General.DAL
{
    public class DALCustomException : DALException, ICustomException
    {
        #region  .  Constructors  .
        public DALCustomException() { }
        public DALCustomException(string message) : base(message) { }
        public DALCustomException(string message, Exception innerException) : base(message, innerException) { }
        #endregion
    }
}