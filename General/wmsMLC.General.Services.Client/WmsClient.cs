using System.ServiceModel;
using System.Threading.Tasks;

namespace wmsMLC.General.Services.Client
{
    public class WmsClient : ClientBase<IWmsService>
    {
        public WmsClient(string endpointConfigurationName) : base(endpointConfigurationName)
        {
        }

        public WmsClient(string endpointConfigurationName, string remoteAddress)
            : base(endpointConfigurationName, remoteAddress)
        {
        }

        public WmsClient(string endpointConfigurationName, EndpointAddress remoteAddress)
            : base(endpointConfigurationName, remoteAddress)
        {
        }

        public WmsClient(System.ServiceModel.Channels.Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        {
        }

        public string StartSession(ClientTypeCode clientType, decimal? clientSessionId)
        {
            return Channel.StartSession(clientType, clientSessionId);
        }

        public void SetClientSession(decimal clientSessionId)
        {
            Channel.SetClientSession(clientSessionId);
        }

        public string TerminateSession()
        {
            return Channel.TerminateSession();
        }

        public Task<byte[]> ProcessTelegramAsync(byte[] telegram)
        {
            //if (IsNet45OrNewer)
            //    return base.Channel.ProcessTelegramTaskBasedAsync(telegram);
            //else
                return Task<byte[]>.Factory.FromAsync((cb, s) => Channel.BeginProcessTelegram(telegram, cb, s), Channel.EndProcessTelegram, null);
        }

        public byte[] ProcessTelegram(byte[] telegram)
        {
            return Channel.ProcessTelegram(telegram);
            //return Task<byte[]>.Factory.FromAsync(BeginProcessTelegram, EndProcessTelegram, telegram);
        }

        //private static bool IsNet45OrNewer = Type.GetType("System.Reflection.ReflectionContext", false) != null;
    }
}
