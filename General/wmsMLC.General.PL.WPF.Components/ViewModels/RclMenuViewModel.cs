using System.Collections.ObjectModel;
using System.Linq;
using wmsMLC.General.PL.WPF.Components.ViewModels.RclMenu;
using wmsMLC.General.PL.WPF.ViewModels;

namespace wmsMLC.General.PL.WPF.Components.ViewModels
{
    public class RclMenuViewModel : ViewModelBase
    {
        public const string BarsPropertyName = "Bars";
        private ObservableCollection<BarItem> _bars;

        public RclMenuViewModel()
        {
            Bars = new ObservableCollection<BarItem>();
        }

        public ObservableCollection<BarItem> Bars
        {
            get
            {
                var ordered = _bars.OrderBy(i => i.Priority).ToArray();
                _bars.Clear();
                foreach (var item in ordered)
                    _bars.Add(item);
                return _bars;
            }
            set
            {
                _bars = value;
                OnPropertyChanged(BarsPropertyName);
            }
        }

        public BarItem GetOrCreateBarItem(string caption, int? priorityForCreate = null)
        {
            var bar = Bars.FirstOrDefault(p => p.Caption == caption);
            if (bar == null)
            {
                bar = new BarItem { Caption = caption, Priority = priorityForCreate.HasValue ? priorityForCreate.Value : Bars.Count };
                Bars.Add(bar);
            }
            return bar;
        }
    }
}