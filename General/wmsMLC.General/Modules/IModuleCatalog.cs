using System.Collections.Generic;

namespace wmsMLC.General.Modules
{
    public interface IModuleCatalog
    {
        IEnumerable<ModuleInfo> Modules { get; }

        void Initialize();

        void AddModule(ModuleInfo moduleInfo);
    }
}
