using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using MLC.SvcClient.Impl.ExtDirect.Dto;

namespace MLC.SvcClient.Impl.ExtDirect
{
    public class ExtDirectServiceMetadata : IMetadata
    {
        private readonly ExtDirectServiceConfig _config;

        public ExtDirectServiceMetadata(ExtDirectServiceConfig config)
        {
            Contract.Requires(config != null);
            _config = config;
        }

        public string GetNamespace()
        {
            return _config.Namespace;
        }

        public IEnumerable<string> GetActionNames()
        {
            return _config.Actions.Keys;
        }

        public IEnumerable<string> GetMethodNames(string actionName)
        {
            return _config.Actions[actionName].Select(i => i.Name).ToArray();
        }

        public Type GetActionType(string actionName, string methodName)
        {
            throw new NotImplementedException();
        }

        public MethodInfo GetMethodInfo(string actionName, string methodName)
        {
            throw new NotImplementedException();
        }

        public int GetNumberOfParameters(string actionName, string methodName)
        {
            throw new NotImplementedException();
        }

        public bool IsFormHandler(string actionName, string methodName)
        {
            throw new NotImplementedException();
        }

        public bool UsePositionalArguments(string actionName, string methodName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetArgumentNames(string actionName, string methodName)
        {
            throw new NotImplementedException();
        }
    }
}