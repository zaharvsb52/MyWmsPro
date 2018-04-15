using Newtonsoft.Json;

namespace MLC.SvcClient.Impl.ExtDirect.Dto
{
    /// <summary>
    /// DTO для передачи на клиент информации об одной ошибке.
    /// </summary>
    public class ErrorItemDescriptor
    {
        [JsonProperty("severity")]
        public int Severity { get; set; }

        [JsonProperty("userMessage")]
        public string UserMessage { get; set; }

        [JsonProperty("technicalMessage")]
        public string TechnicalMessage { get; set; }
    }
}