using System;
using wmsMLC.General.Services;

namespace wmsMLC.General.Types
{
    public class SocketMessageHelper
    {
        public SocketMessageTemplate GetMessage(byte[] buffer)
        {
            return (buffer == null || buffer.Length == 0)
                ? new SocketMessageTemplate { Action = (int)SocketMessageTemplate.ActionType.None }
                : GSerialize.DeserializeBytes<SocketMessageTemplate>(buffer);
        }

        public SocketMessageTemplate GetMessage(byte[] buffer, int bytestoread)
        {
            var buff = new byte[bytestoread];
            /*for (var i = 0; i < bytestoread; i++) 
            {
                buff[i] = buffer[i];
            }*/
            Array.Copy(buffer, 0, buff, 0, bytestoread);
            return GSerialize.DeserializeBytes<SocketMessageTemplate>(buff);
        }

        public byte[] SetMessage<T>(SocketMessageTemplate.ActionType action, T result)
        {
            return SetMessage(new SocketMessageTemplate
                {
                    Action = (int)action,
                    Result = typeof(T) == typeof(byte[]) ? result as byte[] : GSerialize.SerializeBytes(result),
                });
        }

        public byte[] SetMessage(SocketMessageTemplate message)
        {
            return message == null ? null : GSerialize.SerializeBytes(message);
        }
    }
}
