using System;
using System.ComponentModel;
using wmsMLC.Business.Objects;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Managers
{
    public interface ISysObjectManager : IBaseManager<SysObject, decimal>
    {
        Type GetTypeByName(string typeName);
        Type GetTypeBySysObjectId(decimal id);

        void RegisterTypeName(string typeName, Type type);

        ICustomTypeDescriptor GetTypeDescriptor(Type type);

        void LiteClearCache();
    }
}