using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using DevExpress.Xpf.Bars;

namespace wmsMLC.DCL.Main.Views.Controls
{
    public class CustomBar : Bar
    {
        public CustomBar()
        {
            ItemLinks.CollectionChanged += OnItemLinksCollectionChanged;
        }

        public string SerializationName
        {
            get { return (string) GetValue(SerializationNameProperty); }
            set { SetValue(SerializationNameProperty, value); }
        }
        public static readonly DependencyProperty SerializationNameProperty = DependencyProperty.Register("SerializationName", typeof(string), typeof(CustomBar), new PropertyMetadata(OnSerializationNameChanged));

        private static void OnSerializationNameChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
             ((CustomBar)o).OnSerializationNameChanged();   
        }

        private void OnSerializationNameChanged()
        {
            if (string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(SerializationName))
                Name = SerializationName;
        }

        //При изменении Scope, например, панель в окно, не даем изменяться свойствам элементов тулбара
        private void OnItemLinksCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add)
                return;

            var manager = Manager as CustomBarManager;
            if (manager == null)
                return;

            foreach (var barItemLink in e.NewItems.OfType<BarItemLink>())
            {
                if (barItemLink.BarItemDisplayMode != manager.BarItemDisplayMode)
                    barItemLink.BarItemDisplayMode = manager.BarItemDisplayMode;
                if (barItemLink.UserGlyphAlignment != manager.UserGlyphAlignment)
                    barItemLink.UserGlyphAlignment = manager.UserGlyphAlignment;
            }
        }
    }
}
