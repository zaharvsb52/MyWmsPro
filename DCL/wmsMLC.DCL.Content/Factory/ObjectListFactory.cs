using System.Collections.Generic;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Factory;
using wmsMLC.General.PL.WPF.ViewModels;

namespace wmsMLC.DCL.Content.Factory
{
    public class ObjectListFactory : IObjectListFactory
    {
        public IViewModel CreateModel<T>(string caption, string filter, IEnumerable<T> source = null, double? fontSize = null, string layoutSuffix = null)
        {
            var modelType = typeof(ListViewModelBase<>).MakeGenericType(typeof(T));
            var model = (IListViewModel<T>)IoC.Instance.Resolve(modelType);
            model.PanelCaption = caption;
            var modelBase = (ListViewModelBase<T>)model;
            modelBase.IsMainMenuEnable = false;
            if (!string.IsNullOrEmpty(layoutSuffix))
                modelBase.SetSuffix(layoutSuffix);
            modelBase.IsCustomizeBarEnabled = true;
            model.IsCloseDoubleClick = true;

            if (source != null)
                model.SetSource(new EditableBusinessObjectCollection<T>(source));
            else
                model.ApplyFilter(filter);

            var items = model.GetSource() as IList<T>;
            if (items != null && items.Count > 0)
                model.SelectedItems.Add(items[0]);
            //INFO: это нужно, чтобы модель жила после закрытия диалога
            modelBase.SuspendDispose = true;
            return model;
        }
    }
}
