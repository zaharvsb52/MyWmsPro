using Newtonsoft.Json;

namespace MLC.SvcClient.Impl.ExtDirect.Dto
{
    /// <summary>
    /// DTO для передачи информации об ошибках на клиента.
    /// </summary>
    public class ErrorDescriptor
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("items")]
        public ErrorItemDescriptor[] Items { get; set; }
    }
}