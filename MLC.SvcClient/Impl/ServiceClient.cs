using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Threading.Tasks;

namespace MLC.SvcClient.Impl
{
    public class ServiceClient : IServiceClient
    {
        private readonly IManager _manager;

        private class DynamicServiceClient : DynamicObject
        {
            private readonly ServiceClient _serviceClient;
            private readonly string _path;

            public DynamicServiceClient(ServiceClient serviceClient)
            {
                Contract.Requires(serviceClient != null);
                _serviceClient = serviceClient;
            }

            private DynamicServiceClient(ServiceClient serviceClient, string path)
            {
                Contract.Requires(serviceClient != null);
                Contract.Requires(path != null);

                _serviceClient = serviceClient;
                _path = path;
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                var path = _path == null ? binder.Name : string.Format("{0}.{1}", _path, binder.Name);
                result = new DynamicServiceClient(_serviceClient, path);
                return true;
           }

            public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
            {
                var parameters = new List<Parameter>();
                for (int i = 0; i < args.Length; i++)
                {
                    var name = binder.CallInfo.ArgumentNames == null
                                   ? i.ToString()
                                   : binder.CallInfo.ArgumentNames[i];
                    var value = args[i];
                    parameters.Add(new Parameter(name, value, typeof(object)));
                }
                var transaction = GetTransaction(_path, binder.Name, parameters.ToArray(), binder.ReturnType);
                result = _serviceClient._manager.ProcessTransaction(transaction);
                return true;
            }
        }

        public ServiceClient(IManager manager)
        {
            Contract.Requires(manager != null);

            _manager = manager;
        }

        #region .  IServiceClient  .
        public void Exec(string action, string method, Parameter[] parameters)
        {
            var transaction = GetTransaction(action, method, parameters);
            _manager.ProcessOneWayTransaction(transaction);
        }

        public Task ExecAsync(string action, string method, Parameter[] parameters)
        {
            return Task.Factory.StartNew(() => Exec(action, method, parameters));
        }

        public TResult Exec<TResult>(string action, string method, Parameter[] parameters)
        {
            var transaction = GetTransaction(action, method, parameters, typeof(TResult));
            return _manager.ProcessTransaction<TResult>(transaction);
        }

        public Task<TResult> ExecAsync<TResult>(string action, string method, Parameter[] parameters)
        {
            var transaction = GetTransaction(action, method, parameters, typeof(TResult));
            return _manager.ProcessTransactionAsync<TResult>(transaction);
        }

        public object Exec(string action, string method, Parameter[] parameters, Type resultType)
        {
            var transaction = GetTransaction(action, method, parameters, resultType);
            return _manager.ProcessTransaction(transaction);
        }

        public Task<object> ExecAsync(string action, string method, Parameter[] parameters, Type resultType)
        {
            var transaction = GetTransaction(action, method, parameters, resultType);
            return _manager.ProcessTransactionAsync(transaction);
        }

        public dynamic AsDynamic()
        {
            return new DynamicServiceClient(this);
        }
        #endregion

        private static Transaction GetTransaction(string action, string method, Parameter[] parameters, Type resultType = null)
        {
            return new Transaction(action, method, parameters, resultType);
        }
    }
}