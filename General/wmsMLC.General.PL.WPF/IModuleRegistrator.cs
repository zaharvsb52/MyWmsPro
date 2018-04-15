using System.Collections.Generic;
using wmsMLC.General.Modules;

namespace wmsMLC.General.PL.WPF
{
    public interface IModuleRegistrator
    {
        void Register(IRunableModule module);
        IEnumerable<IRunableModule> GetModules();
    }
}