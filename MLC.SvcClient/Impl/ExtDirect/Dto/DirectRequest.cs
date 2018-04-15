using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MLC.SvcClient.Impl.ExtDirect.Dto
{
    public class DirectRequest
    {
        [JsonProperty("action")]
        public String Action;

        [JsonProperty("method")]
        public String Method;

        [JsonProperty("type")]
        public String Type;

        [JsonProperty("tid")]
        public int Tid;

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public JToken JsonData = new JArray();

        public IDictionary<string, object> FormData = new Dictionary<string, object>();

        public bool Upload;
    }
}