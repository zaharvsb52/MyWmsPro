using System.Linq;
using System.Net;

namespace wmsMLC.RCL.Launcher.Common
{
    public class NetInfo
    {
        public string GetHostName()
        {
            return Dns.GetHostName();
        }

        public string[] GetIp4()
        {
            var emptyresult = new string[0];

            var hostName = GetHostName();
            if (string.IsNullOrEmpty(hostName))
                return emptyresult;

            var hostEntry = Dns.GetHostEntry(hostName);
            if (hostEntry == null)
                return emptyresult;

            return hostEntry.AddressList.Where(p => p.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .Select(p => p.ToString()).ToArray();
        }
    }
}
