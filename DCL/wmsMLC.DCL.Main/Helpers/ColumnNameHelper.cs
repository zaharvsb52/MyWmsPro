using System.Windows;
using DevExpress.Xpf.Grid;

namespace wmsMLC.DCL.Main.Helpers
{
    /// <summary>
    /// HACK: от devexpress. Этот класс нужен для возможности сохранять/загружать колонки из настроек
    /// Подробности тут: http://www.devexpress.com/Support/Center/Question/Details/Q457197
    /// </summary>
    public class ColumnNameHelper
    {
        public static readonly DependencyProperty NameProperty = DependencyProperty.RegisterAttached("Name", typeof(string), typeof(ColumnNameHelper), new PropertyMetadata(OnNameChanged));

        public static string GetName(GridColumn target)
        {
            return (string)target.GetValue(NameProperty);
        }

        public static void SetName(GridColumn target, string value)
        {
            target.SetValue(NameProperty, value);
        }

        private static void OnNameChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var column = (GridColumn)o;
            //column.Name = (string)e.NewValue;
            //HACK: для вложенных объектов
            // MultiSelectionViewModel
            var str = e.NewValue.ToString();
            var pos = str.IndexOf('.');
            column.Name = (pos > -1) ? str.Substring(pos + 1) : str;
        }
    }
}