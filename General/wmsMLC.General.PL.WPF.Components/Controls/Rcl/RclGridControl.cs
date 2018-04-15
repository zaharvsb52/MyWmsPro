using System.IO;
using System.Windows;
using System.Windows.Data;
using DevExpress.Utils;
using DevExpress.Xpf.Grid;

namespace wmsMLC.General.PL.WPF.Components.Controls.Rcl
{
    public class RclGridControl : CustomGridControl
    {
        #region .  Fields  .
        private bool _isLoaded;
        private bool _isColumnsSourceChanged;
        private string _layoutString;
        #endregion .  Fields  .

        #region .  Properties  .
        public bool VerifyColumnsSourceChanged
        {
            get { return (bool)GetValue(VerifyColumnsSourceChangedProperty); }
            set { SetValue(VerifyColumnsSourceChangedProperty, value); }
        }
        public static readonly DependencyProperty VerifyColumnsSourceChangedProperty = DependencyProperty.Register("VerifyColumnsSourceChanged", typeof(bool), typeof(RclGridControl));

        private static readonly DependencyProperty ColumnsSourceBaseProperty = DependencyProperty.Register("ColumnsSourceBase", typeof(object), typeof(RclGridControl), new PropertyMetadata(OnColumnsSourceChanged));

        public bool IsWfDesignMode { get; set; }

        public bool IsRestoringLayoutFromXml { get; protected set; }
        #endregion .  Properties  .

        #region .  Methods  .

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            BindingOperations.SetBinding(this, ColumnsSourceBaseProperty,
               new Binding("ColumnsSource")
               {
                   Source = this,
                   Mode = BindingMode.TwoWay,
                   UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
               });
        }

        protected override bool GetAddNewColumns()
        {
            if (IsWfDesignMode)
                return true;

            return base.GetAddNewColumns();
        }


        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            base.OnLoaded(sender, e);

            if (_isLoaded)
                return;
            _isLoaded = true;

            if (_layoutString != null && (!VerifyColumnsSourceChanged || VerifyColumnsSourceChanged && _isColumnsSourceChanged))
            {
                RestoreLayoutFromStringBase(_layoutString);
                _layoutString = null;
            }
        }

        private static void OnColumnsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RclGridControl)d).OnColumnsSourceChanged();
        }

        private void OnColumnsSourceChanged()
        {
            _isColumnsSourceChanged = true;

            if (VerifyColumnsSourceChanged && _layoutString != null)
            {
                RestoreLayoutFromStringBase(_layoutString);
                _layoutString = null;
            }

            if (Columns != null && !IsWfDesignMode)
            {
                foreach (var column in Columns)
                {
                    column.AllowColumnFiltering = DefaultBoolean.False;
                }
            }
        }

        public void RestoreLayoutFromString(string layout)
        {
            if (string.IsNullOrEmpty(layout))
                return;

            if (!_isLoaded || (VerifyColumnsSourceChanged && !_isColumnsSourceChanged))
            {
                _layoutString = layout;
            }
            else
            {
                RestoreLayoutFromStringBase(layout);
            }
        }

        public string SaveLayoutToString()
        {
            using (var ms = new MemoryStream())
            {
                SaveLayoutToStream(ms);
                ms.Position = 0;
                using (var sr = new StreamReader(ms))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        protected virtual void RestoreLayoutFromStringBase(string layout)
        {
            if (string.IsNullOrEmpty(layout))
                return;

            try
            {
                IsRestoringLayoutFromXml = true;
                using (var ms = new MemoryStream())
                {
                    using (var sw = new StreamWriter(ms))
                    {
                        sw.AutoFlush = true;
                        sw.Write(layout);
                        ms.Position = 0;
                        RestoreLayoutFromStream(ms);
                    }
                }
            }
            finally
            {
                IsRestoringLayoutFromXml = false;
            }

            OnRestoredLayoutFromXml();
        }

        public void BestFitColumn(GridColumn column)
        {
            if (column == null)
                return;

            var view = View as TableView;
            if (view == null)
                return;

            view.BestFitColumn(column);
        }

        public void BestFitColumns()
        {
            var view = View as TableView;
            if (view == null)
                return;

            view.BestFitColumns();
        }
        #endregion .  Methods  .
    }
}
