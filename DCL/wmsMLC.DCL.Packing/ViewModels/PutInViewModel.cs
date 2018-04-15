using System;
using System.Linq;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Packing.Views;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Packing.ViewModels
{
    [View(typeof (PutInView))]
    public class PutInViewModel : ExpandoObjectViewModelBase, IPitInViewModel
    {
        private string _timeText;
        private readonly System.Timers.Timer _timer;
        

        public PutInViewModel()
        {
            RefreshStartTime();
            TimeText = TimeSpan.Zero.ToString(@"mm\:ss");

            _timer = new System.Timers.Timer(1000) { AutoReset = true };
            _timer.Elapsed += delegate
            {
                TimeText = (DateTime.Now - StartTime).ToString(@"mm\:ss");
            };
            _timer.Start();
        }


        #region .  Properties  .
        
        public string[] FieldNames;
        public DateTime StartTime { get; private set; }
        public bool ByFill
        {
            get
            {
                if (FieldNames == null || FieldNames.Length == 0)
                    return false;

                return (FieldNames.All(name => this[name] != null));
            }
        }

        public string TimeText
        {
            get { return _timeText; }
            set
            {
                _timeText = value;
                OnPropertyChanged("TimeText");
            }
        }

        #endregion

        public void RefreshStartTime()
        {
            StartTime = DateTime.Now;
        }

        protected override void Dispose(bool disposing)
        {
            if (_timer != null)
                _timer.Dispose();

            base.Dispose(disposing);
        }
    }
}