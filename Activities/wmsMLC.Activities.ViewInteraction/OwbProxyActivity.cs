using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.Activities.ViewInteraction.ViewModels;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.Activities.ViewInteraction
{
    public class OwbProxyActivity : NativeActivity<int>
    {
        [DisplayName(@"Груз расходной накладной")]
        public InArgument<CargoOWB> Source { get; set; }

        [DisplayName(@"Фильтр по накладным")]
        public InArgument<string> Filter { get; set; }

        public OwbProxyActivity()
        {
            DisplayName = "Указать доверенность";
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, Source, type.ExtractPropertyName(() => Source));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Filter, type.ExtractPropertyName(() => Filter));

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var cargoOwb = Source.Get(context);
            if (cargoOwb == null)
                throw new NullReferenceException("Source");

            var vs = IoC.Instance.Resolve<IViewService>();

            var model = new OwbProxyViewModel();
            model.PanelCaption = string.Format("Указать доверенность по грузу '{0}'", cargoOwb.GetKey());
            var owbList = GetOwbList(context).ToArray();
            if (!owbList.Any())
            {
                vs.ShowDialog("Ошибка",
                    string.Format("Груз '{0}' не имеет привязки к расходным накладным!", cargoOwb.GetKey()),
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning,
                    System.Windows.MessageBoxResult.OK);
                Result.Set(context, -1);
                return;
            }

            var owbProxyList = new List<OwbProxy>();
            foreach (var o in owbList)
            {
                var proxy = new OwbProxy(o.GetKey<decimal>())
                {
                    OwbName = o.GetProperty<string>(OWB.OWBNAMEPropertyName),
                    Traffic = o.GetProperty<string>(OWB.VEXTERNALTRAFFICPropertyName),
                    Driver = o.GetProperty<string>(OWB.VEXTERNALTRAFFICDRIVERFIOPropertyName),
                    ProxyCode = o.GetProperty<string>(OWB.OWBPROXYNUMBERPropertyName),
                    ProxyDate = o.GetProperty<DateTime?>(OWB.OWBPROXYDATEPropertyName)
                };
                owbProxyList.Add(proxy);
            }
            model.Source = new ObservableCollection<OwbProxy>(owbProxyList);

            var result = vs.ShowDialogWindow(model, true, false, "50%", "50%");

            // сохраняем
            if (!result.HasValue || !result.Value)
            {
                Result.Set(context, -1);
            }
            else
            {
                SaveCheckedOwbList(model.Source, owbList, model.ProxyCode, model.ProxyDate);
                Result.Set(context, 0);
            }
        }

        private IEnumerable<OWB> GetOwbList(NativeActivityContext context)
        {
            using (var mgr = IoC.Instance.Resolve<IBaseManager<OWB>>())
            {
                var filter = Filter.Get(context);
                    //string.Format("OWBID IN (SELECT O2C.OWBID_R FROM WMSOWB2CARGO O2C WHERE CARGOOWBID_R = {0})",
                    //    cargo.GetKey());
                return mgr.GetFiltered(filter);
            }
        }

        private void SaveCheckedOwbList(IEnumerable<OwbProxy> source, IEnumerable<OWB> target, string proxyCode, DateTime? proxyDate)
        {
            var list = new List<OWB>();
            var trg = target.ToArray();
            foreach (var s in source)
            {
                if (!s.Checked) 
                    continue;
                var t = trg.First(i => i.GetKey().Equals(s.GetId()));
                t.SetProperty(OWB.OWBPROXYNUMBERPropertyName, proxyCode);
                t.SetProperty(OWB.OWBPROXYDATEPropertyName, proxyDate);
                list.Add(t);
            }
            using (var mgr = IoC.Instance.Resolve<IBaseManager<OWB>>())
                mgr.Update(list);
        }
    }
}
