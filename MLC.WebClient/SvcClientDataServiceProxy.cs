using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using MLC.Ext.Common.Change;
using MLC.Ext.Common.Data;
using MLC.Ext.Common.Data.Impl;
using MLC.Ext.Common.Model.ContextModel;
using MLC.SvcClient;
using WebClient.Common.Client.Protocol.DataTransferObjects.Metadata;

namespace MLC.WebClient
{
    /// <summary>
    /// Взаимодействует с Server-ным DataService
    /// Может создавать DataContext
    /// </summary>
    public class SvcClientDataServiceProxy : IDataServiceProxy
    {
        #region .  Fields & Consts  .

        public const string DefaultActionName = "";
        public const string DefaultGetMetadataName = "getMetadata";

        public const string ChangeSetParameterNameInCommitMethod = "jsChangeSet";

        private readonly IManager _manager;
        private readonly IDataContextModelBuilder _dataContextModelBuilder;
        private readonly IDataContextDataBuilder _dataContextDataBuilder;
        private readonly IDataContextModelStore _dataContextModelStore;
        private readonly IChangeSetCollector _changeSetCollector;
        private IEnumerable<string> _serverMethods;

        #endregion

        public SvcClientDataServiceProxy(IManager manager,
            IDataContextModelBuilder dataContextModelBuilder,
            IDataContextDataBuilder dataContextDataBuilder,
            IDataContextModelStore dataContextModelStore,
            IChangeSetCollector changeSetCollector)
        {
            Contract.Requires(manager != null);
            Contract.Requires(dataContextModelBuilder != null);
            Contract.Requires(dataContextDataBuilder != null);
            Contract.Requires(dataContextModelStore != null);
            Contract.Requires(changeSetCollector != null);

            _manager = manager;
            _dataContextModelBuilder = dataContextModelBuilder;
            _dataContextDataBuilder = dataContextDataBuilder;
            _dataContextModelStore = dataContextModelStore;
            _changeSetCollector = changeSetCollector;

            ActionName = DefaultActionName;
            GetMetadataName = DefaultGetMetadataName;
        }

        #region .  Properties  .

        public string ActionName { get; set; }

        public string GetMetadataName { get; set; }

        /// <summary>
        /// Ext.direct Action - набор серверных методов доступа к данным и метаданным
        /// </summary>
        public IEnumerable<string> ServerMethods
        {
            get { return GetServerMethods(); }
        }

        #endregion

        public IDataContext CreateDataContext(string entityType)
        {
            var jsData = _dataContextModelStore.GetOrLoad(entityType, LoadDataContextModel);
            return CreateDataContextByModel(jsData);
        }

        public void Commit(IDataContext dataContext)
        {
            var changeSet = _changeSetCollector.Collect(dataContext, null);
            var parameters = new Dictionary<string, object>
            {
                {ChangeSetParameterNameInCommitMethod, changeSet},
                {"entityType", dataContext.Model.Structures.Cast<MLC.Ext.Common.Model.ClientRecordStructure>().First().EntityType }
            };

            var commitMethod = GetCommitMethod(dataContext);
            Invoke(commitMethod, parameters);
        }

        private string GetCommitMethod(IDataContext dataContext)
        {
            var res = dataContext.Model.CommitMethod;
            if (string.IsNullOrEmpty(res))
                throw new Exception("Дата сервис не указал в DataContextModel имя commitMethod-а. Дата сервис: " + ActionName);

            if (GetServerMethods().All(i => i != res))
                throw new Exception("Дата сервис указал в DataContextModel имя commitMethod-а которое не существует в классе дата сервиса. Дата сервис: " + ActionName);

            return res;
        }

        private JsDataContextModel LoadDataContextModel(string entityType)
        {
            if (ServerMethods.FirstOrDefault(i => i == GetMetadataName) == null)
                throw new Exception(
                    $"В DataServiceProxy было передано имя несуществующего метода серверного класса, либо имя по-умолчанию указывает на несуществующий метод. ActionName: {ActionName}. GetModelMethodName: {GetMetadataName}");

            var transaction = new Transaction(ActionName, GetMetadataName,
                new[] { new Parameter("entityType", entityType) },
                typeof(JsDataContextModel));

            return _manager.ProcessTransaction<JsDataContextModel>(transaction);
        }

        public object Invoke(string methodName, IDictionary<string, object> parameters)
        {
            var transaction = new Transaction(ActionName, methodName,
                parameters.Select(i => new Parameter(i.Key, i.Value)));

            return _manager.ProcessTransaction(transaction);
        }

        public TResult Invoke<TResult>(string methodName, IDictionary<string, object> parameters)
        {
            var transaction = new Transaction(ActionName, methodName,
                parameters.Select(i => new Parameter(i.Key, i.Value)),
                typeof(TResult));

            return _manager.ProcessTransaction<TResult>(transaction);
        }

        public Task<TResult> InvokeAsync<TResult>(string methodName, IDictionary<string, object> parameters)
        {
            var transaction = new Transaction(ActionName, methodName,
                parameters.Select(i => new Parameter(i.Key, i.Value)),
                typeof(TResult));

            return _manager.ProcessTransactionAsync<TResult>(transaction);
        }

        private IEnumerable<string> GetServerMethods()
        {
            return _serverMethods ?? (_serverMethods = _manager.GetActionMethods(ActionName));
        }

        private IDataContext CreateDataContextByModel(JsDataContextModel jsDataContextModel)
        {
            var model = _dataContextModelBuilder.Build(jsDataContextModel);
            var data = _dataContextDataBuilder.Build(model, this, null);
            return new DataContext(model, data);
        }
    }
}