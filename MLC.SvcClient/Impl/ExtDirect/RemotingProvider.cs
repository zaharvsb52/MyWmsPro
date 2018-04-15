/*
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace WebClient.ExtDirectClient.Ext
{
    public class RemotingProvider : Provider
    {
        private ConcurrentDictionary<string, Transaction> _transactions;
        private ConcurrentQueue<Transaction> _transactionsQueue;
        private DateTime _timeToBeExecuted;
        private object _sendingLock = new object();
        private Timer _timer;

        public RemotingProvider()
        {
            MaxRetries = 1;
            EnabeleBuffer = 10;
            _transactions = new ConcurrentDictionary<string, Transaction>();
            _transactionsQueue = new ConcurrentQueue<Transaction>();
        }

        /// <summary>
        /// If a number is specified this is the amount of time in milliseconds
        /// to wait before sending a batched request.
        ///
        /// Calls which are received within the specified timeframe will be
        /// concatenated together and sent in a single request, optimizing the
        /// application by reducing the amount of round trips that have to be made
        /// to the server.To cancel buffering for some particular invocations, pass
        /// `timeout` parameter in `options` object for that method call.
        /// </summary>
        public int EnabeleBuffer { get; protected set; }
        public string Url { get; private set; }

        public void ConfigureRequest(string action, string name, IDictionary<string, object> parameters)
        {
            var configuredTransaction = ConfigureTransaction(action, name, parameters);
            QueueTransaction(configuredTransaction);
        }

        private Transaction ConfigureTransaction(string action, string name, IDictionary<string, object> parameters)
        {
            throw new NotImplementedException();
        }

        public override void QueueTransaction(Transaction transaction)
        {
            if (transaction.Form)
            {
                SendFormRequest(transaction);
                return;
            }

            if (EnabeleBuffer == 0 || !transaction.Timeout.HasValue)
            {
                SendRequest(new[] { transaction });
                return;
            }

            _transactionsQueue.Enqueue(transaction);

            if (EnabeleBuffer > 0)
            {
                lock (_sendingLock)
                {
                    if (_timer == null)
                        _timer = new Timer(state => CombineAndSend());
                    _timer.Change(_transactionsQueue.Count * EnabeleBuffer, Timeout.Infinite);
                }
            }
            else
            {
                CombineAndSend();
            }

            base.QueueTransaction(transaction);
        }

        private void SendRequest(IEnumerable<Transaction> transactions)
        {
            //var actionName = GetActionName();
            //var data = new Dictionary<string, object>();
            //for (int i = 0; i < parameters.Length; i++)
            //{
            //    data.Add(i.ToString(), parameters[i]);
            //}
            //var request = new DirectRequest()
            //{
            //    Action = actionName,
            //    Method = name,
            //    Type = "rpc",
            //    FormData = data
            //};
            //return new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        }

        private string GetActionName()
        {
            var actionIdx = Url.LastIndexOf('/');
            if (actionIdx == -1)
                throw new ArgumentException(string.Format("Url {0} does not contains action name", Url));

            return Url.Substring(actionIdx + 1);
        }

        private void SendFormRequest(Transaction transaction)
        {
            throw new NotImplementedException();
        }

        private void CombineAndSend()
        {
            if (_transactionsQueue.IsEmpty)
                return;

            lock (_sendingLock)
            {
                if (_transactionsQueue.IsEmpty)
                    return;

                // stop timer
                _timer.Change(0, Timeout.Infinite);

                // create sending buffer
                var buffer = new List<Transaction>();
                Transaction outTransaction;
                while(_transactionsQueue.TryDequeue(out outTransaction))
                    buffer.Add(outTransaction);

                SendRequest(buffer);
            }
        }

        private void DoConnect()
        {
            throw new NotImplementedException();
        }

        private string GetNamespace()
        {
            throw new NotImplementedException();
        }

        private void InitApi()
        {
        }

        private void CreateHandler()
        {
        }

        protected override void Connect()
        {
            base.Connect();
        }

    }
}
*/