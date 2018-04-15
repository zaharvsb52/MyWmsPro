using System.Threading.Tasks;
using MLC.SvcClient;

namespace MLC.WebClient
{
    public partial class WmsAPI
    {
        public const string ActionName = "WMS.DataServices.Api.DataService";

        private readonly IManager _manager;

        public WmsAPI(IManager manager)
        {
            _manager = manager;
        }

        public string Echo(string sourceMessage)
        {
            return WithTransaction("echo")
                .AddParameter("sourceMessage", sourceMessage)
                .Process<string>();
        }

        public Task<string> EchoAsync(string sourceMessage)
        {
            return WithTransaction("echo")
                .AddParameter("sourceMessage", sourceMessage)
                .ProcessAsync<string>();
        }

        protected ITransactionExecutor WithTransaction(string name)
        {
            return new TransactionExecutor(_manager, ActionName, name);
        }
    }
}
