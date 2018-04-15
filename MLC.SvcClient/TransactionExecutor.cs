using System.Threading.Tasks;

namespace MLC.SvcClient
{
    public class TransactionExecutor : ITransactionExecutor
    {
        private readonly Transaction _transaction;
        private readonly IManager _manager;

        public TransactionExecutor(IManager manager, string action, string method)
        {
            _manager = manager;
            _transaction = new Transaction(action, method);
        }

        public ITransactionExecutor AddParameter<TParamType>(string name, TParamType value)
        {
            _transaction.Parameters.Add(new Parameter(name, value, typeof(TParamType)));
            return this;
        }

        public TResult Process<TResult>()
        {
            return _manager.ProcessTransaction<TResult>(_transaction);
        }

        public object Process()
        {
            return _manager.ProcessTransaction(_transaction);
        }

        public Task<TResult> ProcessAsync<TResult>()
        {
            return _manager.ProcessTransactionAsync<TResult>(_transaction);
        }

        public Task ProcessAsync()
        {
            return _manager.ProcessTransactionAsync(_transaction);
        }
    }
}