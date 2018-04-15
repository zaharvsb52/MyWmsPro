using System.Collections.Generic;
using wmsMLC.General;
using wmsMLC.General.PL.WPF.Components.ViewModels;
using wmsMLC.General.PL.WPF.Factory;
using wmsMLC.General.PL.WPF.ViewModels;

namespace wmsMLC.RCL.Content.Factory
{
    public class RclObjectListFactory : IObjectListFactory
    {
        public IViewModel CreateModel<T>(string caption, string filter, IEnumerable<T> source = null, double? fontSize = null, string layoutSuffix = null)
        {
            var model = (IViewModel)IoC.Instance.Resolve<IDialogSourceViewModel<T>>();
            var ds = (IDialogSourceViewModel<T>)model;
            ds.PanelCaption = caption;
            ds.SetEntityFilter(filter);
            if (fontSize != null)
                ds.FontSize = fontSize.Value;
            return model;
        }
    }
}
