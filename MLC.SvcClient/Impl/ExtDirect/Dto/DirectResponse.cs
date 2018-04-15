using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MLC.SvcClient.Impl.ExtDirect.Dto
{
    public class DirectResponse
    {
        [JsonProperty("action")]
        public String Action;

        [JsonProperty("method")]
        public String Method;

        [JsonProperty("tid")]
        public int Tid;

        [JsonProperty("type")]
        public String Type;

        [JsonProperty("result")]
        public JToken Result;

        [JsonProperty("message")]
        public String Message;

        [JsonProperty("where")]
        public String Where;

        [JsonProperty("error")]
        public JToken Error;

        public DirectResponse()
        {
        }

        public DirectResponse(DirectRequest request)
        {
            Action = request.Action;
            Method = request.Method;
            Tid = request.Tid;
            Type = request.Type;
        }

        public DirectResponse(DirectRequest request, Exception e) : this(request)
        {
            Type = "exception";
            Message = e.Message;
            Where = e.ToString();
        }

        public DirectResponse(DirectRequest request, JToken result) : this(request)
        {
            Result = result;
        }
    }
}