using System.Collections.Generic;
using wmsMLC.General.Modules;

namespace wmsMLC.General.PL.WPF
{
    public class ModuleRegistrator : IModuleRegistrator
    {
        private readonly List<IRunableModule> _modules;

        public ModuleRegistrator()
        {
            _modules = new List<IRunableModule>();
        }

        public void Register(IRunableModule module)
        {
            _modules.Add(module);
        }

        public IEnumerable<IRunableModule> GetModules()
        {
            return _modules;
        }
    }
}