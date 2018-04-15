using System;
using System.Collections.Generic;
using System.Linq;

namespace wmsMLC.General.Services.Service
{
    public class ServiceContext
    {
        private readonly IDictionary<string, string> _args;

        public string Name { get; set; }

        public ServiceContext(string name, IDictionary<string, string> args)
        {
            Name = name;
            _args = args;
        }

        public string Get(string paramName)
        {
            var key = _args.Keys.FirstOrDefault(i => i.Equals(paramName, StringComparison.InvariantCultureIgnoreCase));
            return key == null ? null : _args[key];
        }
    }
}
