using System;
using System.Windows;
using DevExpress.Xpf.Bars;

namespace wmsMLC.DCL.Main.Views.Controls
{
    public class CustomBarSubItem : BarSubItem
    {
        public CustomBarSubItem()
        {
            Popup += OnPopup;
            CloseUp += OnCloseUp;
        }

        private void OnPopup(object sender, EventArgs eventArgs)
        {
            IsEnableItems = true;
        }

        private void OnCloseUp(object sender, EventArgs eventArgs)
        {
            IsEnableItems = false;
        }

        public bool IsEnableItems
        {
            get { return (bool)GetValue(IsEnableItemsNameProperty); }
            set { SetValue(IsEnableItemsNameProperty, value); }
        }
        public static readonly DependencyProperty IsEnableItemsNameProperty = DependencyProperty.Register("IsEnableItems", typeof(bool), typeof(CustomBarSubItem), new PropertyMetadata(default(bool)));
    }
}