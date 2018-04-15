using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace MLC.SvcClient
{
    /// <summary>
    /// ќпераци€, описывающа€ требуеме действи€ на сервере
    /// </summary>
    public class Transaction
    {
        public const int DefaultTimeoutInMs = 30000;
        public const int DefaultMaxRetry = 1;

        public Transaction(string action, string method, IEnumerable<Parameter> parameters = null, Type resultType = null)
        {
            Contract.Requires(action != null);
            Contract.Requires(method != null);

            Action = action;
            Method = method;
            Parameters = parameters == null ? new List<Parameter>() : new List<Parameter>(parameters);
            ResultType = resultType ?? typeof(object);

            Timeout = DefaultTimeoutInMs;
            MaxRetry = DefaultMaxRetry;

            ExtraParameters = new Dictionary<string, string>();
        }

        public string Action { get; private set; }
        public string Method { get; private set; }
        public List<Parameter> Parameters { get; private set; }

        public Type ResultType { get; private set; }

        public int Timeout { get; set; }
        public int MaxRetry { get; set; }

        public Dictionary<string, string> ExtraParameters { get; private set; }
    }
}