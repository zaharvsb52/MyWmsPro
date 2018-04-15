using System;

namespace wmsMLC.Business.General
{
    public interface IManagerForObject
    {
        void Register(Type objectType, Type managerType);
        Type GetManagerByTypeName(string objectTypeName, bool ignoreCase = false);
        Type GetTypeByName(string objectTypeName);
        Type GetTypeByTENTName(string objectTENTTypeName);
    }
}