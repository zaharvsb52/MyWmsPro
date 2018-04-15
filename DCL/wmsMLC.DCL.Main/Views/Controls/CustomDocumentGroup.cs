using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using DevExpress.Xpf.Docking;
using DevExpress.Xpf.Docking.Base;
using wmsMLC.DCL.Main.Views.Templates;

namespace wmsMLC.DCL.Main.Views.Controls
{
    public class CustomDocumentGroup : DocumentGroup
    {
        private readonly Dictionary<CustomLayoutPanel, CustomLayoutPanel> _previousPanels = new Dictionary<CustomLayoutPanel, CustomLayoutPanel>();

        public CustomDocumentGroup()
        {
            SelectedItemChanged += DocumentGroupOnSelectedItemChanged;
            Items.CollectionChanged += ItemsOnCollectionChanged;
        }

        private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.Action != NotifyCollectionChangedAction.Remove)
                return;

            if (notifyCollectionChangedEventArgs.OldItems.Count != 1)
                return;

            var item = notifyCollectionChangedEventArgs.OldItems[0];
            var layoutPanel = item as CustomLayoutPanel;
            if (layoutPanel == null)
                return;

            if (!_previousPanels.ContainsKey(layoutPanel))
                return;

            CustomLayoutPanel oldPanel;
            _previousPanels.TryGetValue(layoutPanel, out oldPanel);
            if (_previousPanels.ContainsKey(layoutPanel))
                _previousPanels.Remove(layoutPanel);

            if (oldPanel == null)
                return;

            if ((SelectedTabIndex != notifyCollectionChangedEventArgs.OldStartingIndex &&
                 notifyCollectionChangedEventArgs.OldStartingIndex != Items.Count && SelectedTabIndex == Items.Count - 1) ||
                (SelectedTabIndex == notifyCollectionChangedEventArgs.OldStartingIndex &&
                 notifyCollectionChangedEventArgs.OldStartingIndex == Items.Count - 1))
            {
                return;
            }

            var index = Items.IndexOf(oldPanel);
            if (index < 0)
                return;

            SelectedTabIndex = index;
        }
        
        private void DocumentGroupOnSelectedItemChanged(object sender, SelectedItemChangedEventArgs selectedItemChangedEventArgs)
        {
            if (selectedItemChangedEventArgs.OldItem == null)
                return;

            var layoutPanel = selectedItemChangedEventArgs.Item as CustomLayoutPanel;
            if (layoutPanel == null)
                return;

            if (_previousPanels.ContainsKey(layoutPanel))
                _previousPanels.Remove(layoutPanel);

            var oldLayoutPanel = selectedItemChangedEventArgs.OldItem as CustomLayoutPanel;
            if (oldLayoutPanel == null)
                return;

            if (layoutPanel.Caption != null && oldLayoutPanel.Caption != null)
                _previousPanels.Add(layoutPanel, oldLayoutPanel);
        }
    }
}