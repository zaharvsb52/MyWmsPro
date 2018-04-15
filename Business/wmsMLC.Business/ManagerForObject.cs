using System;
using System.Collections.Generic;
using System.Linq;
using wmsMLC.Business.General;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Business
{
    public class ManagerForObject : IManagerForObject
    {
        private readonly Dictionary<Type, Type> _internalContainer = new Dictionary<Type, Type>();

        public void Register(Type objectType, Type managerType)
        {
            _internalContainer.Add(objectType, managerType);
        }

        public Type GetManagerByTypeName(string objectTypeName, bool ignoreCase)
        {
            var key = _internalContainer.Keys.FirstOrDefault(i => (ignoreCase? i.Name.Equals(objectTypeName, StringComparison.InvariantCultureIgnoreCase) : i.Name == objectTypeName));
            if (key == null)
                return null;

            return _internalContainer[key];
        }

        public Type GetTypeByName(string objectTypeName)
        {
            return _internalContainer.Keys.FirstOrDefault(i => i.Name.EqIgnoreCase(objectTypeName));
        }

        public Type GetTypeByTENTName(string objectTENTTypeName)
        {
            return _internalContainer.Keys.FirstOrDefault(i => objectTENTTypeName.EqIgnoreCase(SerializationHelper.RootNamePrefix + i.Name));
        }

        public Type GetTypeByType(Type objectType)
        {
            return _internalContainer.Keys.FirstOrDefault(i => i.Equals(objectType));
        }
    }
}