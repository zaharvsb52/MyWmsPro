using System.Linq;
using System.Windows;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectView))]
    public class IWBPosViewModel : ObjectViewModelBase<IWBPos>
    {
        protected override void SourceObjectPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.SourceObjectPropertyChanged(sender, e);

            if (Source == null)
                return;

            var editable = Source as IEditable;
            if (editable.IsInRejectChanges)
                return;

            if (!e.PropertyName.EqIgnoreCase(IWBPos.IWBID_RPropertyName) &&
                !e.PropertyName.EqIgnoreCase(IWBPos.SKUIDPropertyName))
                return;
            
            var mgr = (IPosManager) GetManager();

            if (e.PropertyName.EqIgnoreCase(IWBPos.IWBID_RPropertyName))
                mgr.FillMandantAndFactory(Source);
            else
                mgr.FillFromSku(Source);

            var parentModelSource = ((IModelHandler)this).ParentViewModelSource as IWB;
            if (parentModelSource != null && e.PropertyName.EqIgnoreCase(IWBPos.SKUIDPropertyName) && Source.SKUID > 0)
            {
                var artMgr = IoC.Instance.Resolve<IBaseManager<Art>>();
                var art = artMgr.GetFiltered(string.Format("exists(select 1 from wmsSKU s where s.ArtCode_r = ArtCode and s.SKUID = {0})", Source.SKUID));
                if (art.First().MANDANTID != parentModelSource.MandantID)
                {
                    GetViewService().ShowDialog("Создание позиции - Выбор SKU", "Мандант выбранной SKU отличается от манданта накладной!", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                    Source.SKUID = (decimal)Source.GetPropertyDefaultValue(IWBPos.SKUIDPropertyName);
                }
            }
        }

        protected override System.Collections.ObjectModel.ObservableCollection<wmsMLC.General.PL.Model.DataField> GetFields(wmsMLC.General.PL.SettingDisplay displaySetting)
        {
            var fields = base.GetFields(displaySetting);
            var f = fields.FirstOrDefault(i => i.Name.EqIgnoreCase(IWBPos.SKUIDPropertyName));
            if (f != null)
            {
                f.LookupVarFilterExt = "[$FACTORYID_R, (null, 1=1), (*, ARTCODE_R in (select a.ARTCODE from wmsart a where a.FACTORYID_R = {0}))]";
            }
            return fields;
        }

        protected override void OnSourceChanged()
        {
            base.OnSourceChanged();
            // выставим фабрику
            if (Mode != ObjectViewModelMode.Object && Source.IsNew)
            {
                var parentModelSource = ((IModelHandler)this).ParentViewModelSource as IWB;
                if (parentModelSource != null)
                {
                    Source.FactoryID_R = parentModelSource.FactoryID_R;
                }
            }
        }
    }
}