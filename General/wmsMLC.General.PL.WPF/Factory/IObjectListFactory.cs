using System.Collections.Generic;
using wmsMLC.General.PL.WPF.ViewModels;

namespace wmsMLC.General.PL.WPF.Factory
{
    public interface IObjectListFactory
    {
        IViewModel CreateModel<T>(string caption, string filter, IEnumerable<T> source = null, double? fontSize = null, string layoutSuffix = null);
    }
}
