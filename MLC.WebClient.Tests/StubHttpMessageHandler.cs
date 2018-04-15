using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MLC.WebClient.Tests
{
    public class StubHttpMessageHandler : HttpClientHandler
    {
        public HttpRequestMessage RequestMessage { get; private set; }
        public HttpResponseMessage ResponseMessage { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestMessage = request;
            if (ResponseMessage == null)
                ResponseMessage = base.SendAsync(request, cancellationToken).Result;
            return Task.FromResult(ResponseMessage);
        }
    }
}