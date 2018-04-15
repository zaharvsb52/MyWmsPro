using System;
using DevExpress.Xpf.Editors;

namespace wmsMLC.DCL.Configurator.Views.Controls
{
    public class ConfiguratorListBoxEdit : ListBoxEdit
    {
        public event EventHandler ItemsSourceChanged;
        protected override void OnItemsSourceChanged(object itemsSource)
        {
            base.OnItemsSourceChanged(itemsSource);
            var handler = ItemsSourceChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}
