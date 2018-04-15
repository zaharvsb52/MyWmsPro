using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using wmsMLC.Business.Objects;
using System.Linq;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectTreeViewCustom))]
    public class MotionAreaGroupTreeViewModel : ObjectTreeViewModelBase<MotionAreaGroup>, ITreeViewModelM2M
    {
        public const string ParentItemsPropertyName = "ParentItems";

        public IEnumerable ParentItems
        {
            get
            {
                var ret = new WMSBusinessCollection<MotionAreaGroup>();
                if (Source != null)
                    foreach (var item in Source)
                    {
                        ret.Add(item);
                    }
                return ret;
            }
        }

        protected override void OnSourceChanged()
        {
            base.OnSourceChanged();
            OnPropertyChanged(ParentItemsPropertyName);
        }

        protected override void SourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.SourceCollectionChanged(sender, e);
            OnPropertyChanged(ParentItemsPropertyName);
        }

        public IEnumerable GetChild(object parent)
        {
            var ret = new WMSBusinessCollection<MotionAreaGroup>();
            foreach (var item in Source)
            {
                 if (string.IsNullOrEmpty(item.MotionAreaGroupParent)) continue;

                 if (((MotionAreaGroup) parent).GetKey().To<string>() == item.MotionAreaGroupParent)
                     ret.Add(item);
            }
            return ret;
        }
    }

}