using System;
using System.Windows;
using System.Windows.Controls;
using wmsMLC.General.PL.Model;

namespace wmsMLC.General.PL.WPF.Components.Helpers
{
    public class ColumnTemplateSelector : DataTemplateSelector
    {
        public class CustomColumnEventArgs : EventArgs
        {
            public DataField DataField { get; private set; }
            public DataTemplate ColumnTemplate { get; set; }

            public CustomColumnEventArgs(DataField dataField)
            {
                DataField = dataField;
            }
        }

        public event EventHandler<CustomColumnEventArgs> CustomColumn;

        public DataTemplate OnCustomColumn(DataField df)
        {
            var cc = CustomColumn;
            if (cc == null)
                return null;

            var ea = new CustomColumnEventArgs(df);
            cc(this, ea);
            return ea.ColumnTemplate;
        }

        public bool AllowUseComboBoxAsEditor { get; set; }
        public DataTemplate DateTimeColumnTemplate { get; set; }
        public DataTemplate CheckColumnTemplate { get; set; }
        public DataTemplate DefaultColumnTemplate { get; set; }
        public DataTemplate LookUpEditColumnTemplate { get; set; }
        public DataTemplate ComboBoxColumnTemplate { get; set; }
        public DataTemplate MemoEditColumnTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var df = item as DataField;
            if (df != null)
            {
                var customTemplate = OnCustomColumn(df);
                if (customTemplate != null)
                    return customTemplate;

                if (df.LookupCode != null)
                    return AllowUseComboBoxAsEditor ? ComboBoxColumnTemplate : LookUpEditColumnTemplate;

                var nonNullableDateTime = df.FieldType.GetNonNullableType();

                if (nonNullableDateTime == typeof(DateTime))
                    return DateTimeColumnTemplate;

                if (nonNullableDateTime == typeof(Boolean))
                    return CheckColumnTemplate;

                if (df.IsMemoView)
                    return MemoEditColumnTemplate;
            }

            return DefaultColumnTemplate;
        }
    }
}