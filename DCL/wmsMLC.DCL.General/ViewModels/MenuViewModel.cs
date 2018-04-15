using System.Collections.ObjectModel;
using System.Linq;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.General.PL.WPF.ViewModels;

namespace wmsMLC.DCL.General.ViewModels
{
    public class MenuViewModel : ViewModelBase, ISettingsNameHandler
    {
        public const string BarsPropertyName = "Bars";
        private ObservableCollection<BarItem> _bars;
        private string _suffix;
        
        public MenuViewModel(string suffix)
        {
            Bars = new ObservableCollection<BarItem>();
            _suffix = suffix;
        }

        /// <summary>
        /// Не использовать глобальный фал вида.
        /// </summary>
        public bool NotUseGlobalLayoutSettings { get; set; }

        public ObservableCollection<BarItem> Bars
        {
            get
            {
                if (_bars == null) 
                    return _bars;
                var ordered = _bars.OrderBy(i => i.Priority).ToArray();
                _bars = null;
                _bars = new ObservableCollection<BarItem>(ordered);
                return _bars;
            }
            set
            {
                _bars = value;
                OnPropertyChanged(BarsPropertyName);
            }
        }

        public BarItem GetOrCreateBarItem(string caption, int? priorityForCreate = null, string name = null)
        {
            var bar = Bars.FirstOrDefault(p => p.Caption == caption);
            if (bar == null)
            {
                bar = new BarItem { Caption = caption };
                Bars.Add(bar);
                bar.Priority = priorityForCreate ?? Bars.Count;
                if (!string.IsNullOrEmpty(name))
                    bar.Name = name;
            }
            return bar;
        }

        public void SetSuffix(string suffix)
        {
            _suffix = suffix;
        }

        public string GetSuffix()
        {
            return _suffix ?? string.Empty;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _bars = null;
        }
    }
}