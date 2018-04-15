using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace wmsMLC.DCL.Content.ViewModels.ArtMassInput
{
    public class DirtyCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        private bool _isDirty = false;

        public bool IsDirty
        {
            get { return _isDirty; }
        }

        public void Clean()
        {
            _isDirty = false;
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            _isDirty = true;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddPropertyChanged(e.NewItems);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    RemovePropertyChanged(e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    RemovePropertyChanged(e.OldItems);
                    AddPropertyChanged(e.NewItems);
                    break;
            }

            base.OnCollectionChanged(e);
        }

        private void AddPropertyChanged(IEnumerable items)
        {
            if (items == null) return;
            foreach (var obj in items.OfType<INotifyPropertyChanged>())
            {
                obj.PropertyChanged += OnItemPropertyChanged;
            }
        }

        private void RemovePropertyChanged(IEnumerable items)
        {
            if (items == null) return;
            foreach (var obj in items.OfType<INotifyPropertyChanged>())
            {
                obj.PropertyChanged -= OnItemPropertyChanged;
            }
        }

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _isDirty = true;
        }
    }
}