using System;

namespace wmsMLC.General.DAL
{
    public class DALException : BaseException
    {
        #region  .  Constructors  .
        public DALException() { }
        public DALException(string message) : base(message) { }

        public DALException(string message, int? number)
            : base(message)
        {
            Number = number;
        }
        public DALException(string message, Exception innerException) : base(message, innerException) { }
        public DALException(string message, int? number, Exception innerException)
            : base(message, innerException)
        {
            Number = number;
        }
        #endregion

        public int? Number { get; private set; }
    }
}