using System.Collections.Generic;
using Newtonsoft.Json;

namespace MLC.SvcClient.Impl.ExtDirect.Dto
{
    public class ExtDirectServiceConfig
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("namespace")]
        public string Namespace { get; set; }

        [JsonProperty("actions")]
        public Dictionary<string, List<ExtDirectActionMethod>> Actions { get; set; }

        public class ExtDirectActionMethod
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("params")]
            public List<string> Params { get; set; }

            [JsonProperty("len")]
            public int Len { get; set; }
        }
    }
}