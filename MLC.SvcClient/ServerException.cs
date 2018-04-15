using System;
using System.Linq;
using System.Runtime.Serialization;
using MLC.SvcClient.Impl.ExtDirect.Dto;

namespace MLC.SvcClient
{
    [Serializable]
    public class ServerException : Exception
    {
        private readonly ErrorDescriptor _errorDescriptor;

        public ServerException()
        {
        }

        public ServerException(string message) : base(message)
        {
        }

        public ServerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ServerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ServerException(ErrorDescriptor errorDescriptor) : this(GetMessageByErrorDescriptor(errorDescriptor))
        {
            _errorDescriptor = errorDescriptor;
        }

        private static string GetMessageByErrorDescriptor(ErrorDescriptor errorDescriptor)
        {
            return String.Format("{0}{1}{2}", errorDescriptor.Title,
                                 Environment.NewLine,
                                 string.Join(Environment.NewLine, errorDescriptor.Items.Select(i => i.UserMessage)));
        }
    }
}