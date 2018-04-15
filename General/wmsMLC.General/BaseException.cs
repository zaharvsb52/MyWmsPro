using System;

namespace wmsMLC.General
{
    /// <summary>
    /// Базовый класс слоя исключений NEK.Framework
    /// NOTE: все наследники должны строиться по принципам, описанным в http://msdn.microsoft.com/en-us/library/ms229064(v=VS.80).aspx 
    /// </summary>
    [Serializable]
    public class BaseException : Exception
    {
        #region  .  Constructors  .
        public BaseException() { }
        public BaseException(string message) : base(message) { }
        public BaseException(string message, Exception innerException) : base(message, innerException) { }
        #endregion
    }

    public abstract class BaseDataException : BaseException
    {
        #region  .  Constructors  .
        public BaseDataException() { }

        public BaseDataException(string code, string message = null, Exception innerException = null) : base(message, innerException)
        {
            Code = code;
        }
        #endregion

        public string Code { get; protected set; }
    }

    public class CustomDataException : BaseDataException
    {
        #region  .  Constructors  .
        public CustomDataException() { }
        public CustomDataException(string code, string message = null, Exception innerException = null) : base(code, message, innerException) { }
        #endregion
    }

    public class SystemDataException : BaseDataException
    {
        #region  .  Constructors  .
        public SystemDataException() { }
        public SystemDataException(string code, string message = null, Exception innerException = null) : base(code, message, innerException) { }
        #endregion
    }

    public abstract class BaseLogicException : BaseException
    {
    }

    public class CustomLogicException : BaseLogicException
    {
    }
}