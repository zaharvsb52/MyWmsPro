using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MLC.SvcClient.Impl
{
    public class Manager : IManager
    {
        private readonly Dictionary<string, IProvider> _providers;

        public Manager()
        {
            _providers = new Dictionary<string, IProvider>();
        }

        public void AddProvider(IProvider provider)
        {
            var metadata = provider.GetMetadata();
            var actions = metadata.GetActionNames();
            foreach (var action in actions)
            {
                var methods = metadata.GetMethodNames(action);
                foreach (var method in methods)
                {
                    var key = string.Format("{0}.{1}", action, method);
                    _providers.Add(key, provider);
                }
            }
        }

        public void RemoveProvider(IProvider provider)
        {
            foreach (var key in _providers.Where(i => i.Value == provider).Select(i => i.Key).ToArray())
                _providers.Remove(key);
        }

        public object ProcessTransaction(Transaction transaction)
        {
            var provider = GetProvider(transaction);
            ConfigureTransaction(transaction);
            return provider.Execute(transaction);
        }

        public Task<object> ProcessTransactionAsync(Transaction transaction)
        {
            return Task.Factory.StartNew(() => ProcessTransaction(transaction));
        }

        public TResult ProcessTransaction<TResult>(Transaction transaction)
        {
            var provider = GetProvider(transaction);
            ConfigureTransaction(transaction);
            return provider.Execute<TResult>(transaction);
        }

        public Task<TResult> ProcessTransactionAsync<TResult>(Transaction transaction)
        {
            return Task.Factory.StartNew(() => ProcessTransaction<TResult>(transaction));
        }

        public void ProcessOneWayTransaction(Transaction transaction)
        {
            var provider = GetProvider(transaction);
            ConfigureTransaction(transaction);
            provider.ExecuteNonQuery(transaction);
        }

        public Task ProcessOneWayTransactionAsync(Transaction transaction)
        {
            return Task.Factory.StartNew(() => ProcessOneWayTransactionAsync(transaction));
        }

        private IProvider GetProvider(Transaction transaction)
        {
            var key = transaction.Action + "." + transaction.Method;
            IProvider provider;
            if (!_providers.TryGetValue(key, out provider))
                throw new InvalidOperationException(string.Format("Не найден провайдер для использования контракта {0}. Уточнтите правильность указания контракта.", key));
            return provider;
        }

        private void ConfigureTransaction(Transaction transaction)
        {

        }
    }
}