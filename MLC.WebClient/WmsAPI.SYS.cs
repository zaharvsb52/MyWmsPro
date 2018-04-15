using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wmsMLC.General;
using wmsMLC.General.Services;

namespace MLC.WebClient
{
    public partial class WmsAPI
    {
        //public Task<SdclConnectInfo> GetSdclEndPointAsync(string clientCode, string lastEndPoint)
        //{
        //    var apiresult = WithTransaction("getSdclEndPoint")
        //        .AddParameter("clientCode", clientCode)
        //        .AddParameter("prevServiceCode", lastEndPoint)
        //        .AddParameter("timeout", (int?)null)
        //        .ProcessAsync<dynamic>();

        //    return apiresult.ContinueWith(p =>
        //        p.Result == null
        //            ? null
        //            : new SdclConnectInfo
        //            {
        //                Endpoint = Convert.ToString(p.Result.Endpoint),
        //                Code = Convert.ToString(p.Result.Code)
        //            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        //}

        public SdclConnectInfo GetSdclEndPoint(string clientCode, string lastEndPoint)
        {
            var apiresult = WithTransaction("getSdclEndPoint")
                .AddParameter("clientCode", clientCode)
                .AddParameter("prevServiceCode", lastEndPoint)
                .AddParameter("timeout", (int?) null)
                .Process<dynamic>();

            var result = apiresult == null
                ? null
                : new SdclConnectInfo
                {
                    Endpoint = Convert.ToString(apiresult.Endpoint),
                    Code = Convert.ToString(apiresult.Code)
                };
            return result;
        }

        public Task<Dictionary<string, string>> GetWebEntityByWmsEntityAsync(string wmsEntityName)
        {
            var apiresult = WithTransaction("getEntityMappingByWmsEntity")
                .AddParameter("wmsEntityName", wmsEntityName)
                .ProcessAsync<dynamic>();

            return apiresult.ContinueWith(p =>
                p.Result == null
                    ? new Dictionary<string, string>
                    {
                        {wmsEntityName, "NONE"}
                    }
                    : new Dictionary<string, string>
                    {
                        {wmsEntityName, Convert.ToString(p.Result.WebEntity)},
                        {string.Format("{0}.PK", wmsEntityName), Convert.ToString(p.Result.WebPk)}
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        public string GetWorkflow(string package, string name, string version)
        {
            var apiresult = WithTransaction("getWorkflow")
              .AddParameter("package", package)
              .AddParameter("name", name)
              .AddParameter("version", version)
              .Process<dynamic>();

            return (string)apiresult;
        }

        public void SetWorkflow(string package, string name, string version, string workflowBody)
        {
            throw new NotImplementedException();
        }

        public void RegEventPl(int partnerId, string operation, string eventKindCode, int plId, int plposId, string placeCode)
        {
            WithTransaction("regEventPl")
                .AddParameter("partnerId", partnerId)
                .AddParameter("operation", operation)
                .AddParameter("eventKindCode", eventKindCode)
                .AddParameter("clientSessionId", WMSEnvironment.Instance.SessionId)
                .AddParameter("clientTypeCode", WMSEnvironment.Instance.ClientType.ToString())
                .AddParameter("workId", (int?) null)
                .AddParameter("workingId", (int?) null)
                .AddParameter("plId", plId)
                .AddParameter("plposId", plposId)
                .AddParameter("placeCode", placeCode)
                .AddParameter("timeout", (int?) null)
                .Process<dynamic>();
        }

        public bool ValidatePlace(string placeCode, string teCode, bool raiseError, bool checkCapacity)
        {
            var apiresult = WithTransaction("validatePlace")
                .AddParameter("placeCode", placeCode)
                .AddParameter("teCode", teCode)
                .AddParameter("raiseError", raiseError)
                .AddParameter("checkCapacity", checkCapacity)
                .AddParameter("timeout", (int?)null)
                .Process<dynamic>();

            return (bool) apiresult;
        }
    }
}
