using System.Collections.Specialized;
using wmsMLC.General.DAL.WebAPI;
using wmsMLC.General.Services;

namespace wmsMLC.Business.DAL.WebAPI
{
    public abstract class BpProcessRepositorySvc : Service.BPProcessRepository
    {
        public const string BpController = "Bp";

        public override string GetSdclEndPoint(string clientCode, ref string prevServiceCode)
        {
            var helper = new WebAPIHelper();
            var parameters = new NameValueCollection(2)
            {
                {"clientcode", clientCode},
                {"prevsdclcode", prevServiceCode}
            };
            var res = helper.Get<SdclConnectInfo>(BpController, "getsdclinfo", parameters);
            prevServiceCode = res.Code;
            return res.Endpoint;
        }
    }
}