using System;
using System.Linq;
using System.Windows.Interactivity;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using System.Windows;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid.Themes;
using System.Collections.Specialized;
using wmsMLC.DCL.Main.Views.Controls;
using wmsMLC.General.PL.WPF;

namespace wmsMLC.DCL.Main.Views.ConditionExpressionEditor
{
    //Optimized Mode Styles and Templates https://documentation.devexpress.com/#WPF/CustomDocument17139 
    //WPF Pivot Grid & TreeList - Conditional Formatting (Coming soon in v15.1) https://community.devexpress.com/blogs/thinking/archive/2015/05/19/wpf-pivot-grid-amp-treelist-conditional-formatting-coming-soon-in-v15-1.aspx
    public class TreeListViewExpressionColumnBehavior : Behavior<TreeListView>
    {
        private bool _isViewLoaded;

        public TreeListViewExpressionColumnBehavior()
        {
            StylesCollection = new StyleOptionCollection();
            StylesCollection.CollectionChanged += StylesCollection_CollectionChanged;
            IsLoading = true;
        }

        #region StylesCollection
        public StyleOptionCollection StylesCollection
        {
            get { return (StyleOptionCollection)GetValue(StylesCollectionProperty); }
            set { SetValue(StylesCollectionProperty, value); }
        }
        public static readonly DependencyProperty StylesCollectionProperty = DependencyProperty.Register("StylesCollection", typeof(StyleOptionCollection), typeof(TreeListViewExpressionColumnBehavior));

        private void StylesCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!IsLoading)
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    CollectionModify();
                }
                else if (e.Action == NotifyCollectionChangedAction.Reset)
                {
                    RowStyleBuilding();
                    View.RowStyle = RowStyle;
                    CellStyleBuilding();
                    foreach (var column in ((TreeListControl)View.DataControl).Columns)
                    {
                        if (column.CellStyle != null)
                            column.CellStyle = CellStyle;
                    }
                }
            }
        }

        private void CollectionModify()
        {
            foreach (var t in StylesCollection.Where(p => p != null && !string.IsNullOrEmpty(p.ExpressionString)))
            {
                t.GenerateSetters();
                if (!t.ApplyToRow && !string.IsNullOrEmpty(t.FieldName))
                {
                    CellTriggersAdding(t);
                    var column = ((TreeListControl)View.DataControl).Columns[t.FieldName];
                    if (column != null)
                        column.CellStyle = CellStyle;
                }
                else if (t.ApplyToRow)
                {
                    RowTriggersAdding(t);
                    View.RowStyle = RowStyle;
                }
            }
        }

        #endregion StylesCollection

        /// <summary>
        /// Условие добавление в контекстное меню столбца меню вызова формы "Редактора формата условий".
        /// </summary>
        public bool AddFormatConditionsEditorMenuItem { get; set; }

        public bool ShowFormatConditionsEditorWindow
        {
            get { return (bool)GetValue(ShowFormatConditionsEditorWindowProperty); }
            set { SetValue(ShowFormatConditionsEditorWindowProperty, value); }
        }
        public static readonly DependencyProperty ShowFormatConditionsEditorWindowProperty = DependencyProperty.Register("ShowFormatConditionsEditorWindow", typeof(bool), typeof(TreeListViewExpressionColumnBehavior), new PropertyMetadata(OnShowFormatConditionsEditorWindowPropertyChanged));

        private static void OnShowFormatConditionsEditorWindowPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                ((TreeListViewExpressionColumnBehavior)d).ShowExpressionWindow();
        }

        private TreeListView View { get; set; }
        private Style RowStyle { get; set; }
        private Style CellStyle { get; set; }
        private BarButtonItem ExprButton { get; set; }
        private bool IsLoading { get; set; }

        protected override void OnAttached()
        {
            base.OnAttached();
            View = AssociatedObject;
            View.Loaded += View_Loaded;

            // подписываемся на изменения стилей отображения
            var cgc = View.DataControl as CustomTreeListControl;
            if (cgc != null)
                cgc.ExpressionStyleOptionsChanged += ExpressionStyleOptionsChanged;

            if (AddFormatConditionsEditorMenuItem)
            {
                EditorBarButtonBuilding();
                AssociatedObject.ColumnMenuCustomizations.Add(ExprButton);
            }
            RowStyleBuilding();
        }

        private void ExpressionStyleOptionsChanged(object sender, EventArgs eventArgs)
        {
            FillStylesCollection();
        }

        private void View_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isViewLoaded)
                return;
            _isViewLoaded = true;

            FillStylesCollection();

            View.DataControl.ItemsSourceChanged -= OnItemsSourceChanged;
            View.DataControl.ItemsSourceChanged += OnItemsSourceChanged;

            SetColumnInfo();
            IsLoading = false;
            CollectionModify();
        }

        private void OnItemsSourceChanged(object sender, ItemsSourceChangedEventArgs itemsSourceChangedEventArgs)
        {
            SetColumnInfo();
        }

        private void FillStylesCollection()
        {
            if (!_isViewLoaded)
                return;

            var tree = View.DataControl as CustomTreeListControl;
            if (tree == null)
                return;

            var stylesCollection = StylesCollection.Where(p => p.IsReadOnly).ToArray();
            StylesCollection.Clear();

            foreach (var p in stylesCollection)
            {
                StylesCollection.Add(p);
            }

            foreach (var o in tree.ExpressionStyleOptions.Options)
            {
                StylesCollection.Add(new StyleOption(o));
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            ExprButton.ItemClick -= BarButtonItem_ItemClick;

            View.Loaded -= View_Loaded;

            StylesCollection.CollectionChanged -= StylesCollection_CollectionChanged;
            StylesCollection.Clear();

            View.DataControl.ItemsSourceChanged -= OnItemsSourceChanged;
            var cgc = View.DataControl as CustomTreeListControl;
            if (cgc != null)
                cgc.ExpressionStyleOptionsChanged -= ExpressionStyleOptionsChanged;
        }

        private void EditorBarButtonBuilding()
        {
            ExprButton = new BarButtonItem { Name = "barButtonItem1", Content = Resources.StringResources.ConditionalFormattingWindowTitle };
            ExprButton.ItemClick += BarButtonItem_ItemClick;
        }

        private void RowStyleBuilding()
        {
            //var gridRowThemeKey = new GridRowThemeKeyExtension
            //{
            //    ThemeName = ThemeManager.ApplicationThemeName,
            //    ResourceKey = GridRowThemeKeys.RowStyle
            //};
            //RowStyle = new Style(typeof(GridRowContent))
            //{
            //    BasedOn = AssociatedObject.TryFindResource(gridRowThemeKey) as Style
            //};

            //RowStyle = new Style(typeof(RowControl))
            //{
            //    BasedOn = AssociatedObject.TryFindResource(gridRowThemeKey) as Style
            //};

            //ver. 14.1
            //var gridRowThemeKey = new GridRowThemeKeyExtension
            //{
            //    ThemeName = ThemeManager.ApplicationThemeName,
            //    ResourceKey = GridRowThemeKeys.RowTemplate
            //};
            RowStyle = new Style(typeof (RowControl));
            //RowStyle.Setters.Add(new Setter(Control.TemplateProperty, AssociatedObject.TryFindResource(gridRowThemeKey)));
        }

        private void CellStyleBuilding()
        {
            //var gridCellThemeKey = new GridRowThemeKeyExtension
            //{
            //    ThemeName = ThemeManager.ApplicationThemeName,
            //    ResourceKey = GridRowThemeKeys.CellStyle
            //};

            //CellStyle = new Style(typeof(CellContentPresenter))
            //{
            //    BasedOn = AssociatedObject.TryFindResource(gridCellThemeKey) as Style
            //};

            //ver. 14.1
            var gridCellThemeKey = new GridRowThemeKeyExtension
            {
                ThemeName = ThemeManager.ApplicationThemeName,
                ResourceKey = GridRowThemeKeys.LightweightCellStyle
            };

            CellStyle = new Style(typeof(LightweightCellEditor))
            {
                BasedOn = AssociatedObject.TryFindResource(gridCellThemeKey) as Style
            };
        }

        private void CellTriggersAdding(StyleOption option)
        {
            if (option == null || string.IsNullOrEmpty(option.ExpressionString) || string.IsNullOrEmpty(option.FieldName))
                return;
            var column = ((TreeListControl) View.DataControl).Columns[option.FieldName];
            if (column == null)
                return;
            if (column.CellStyle == null)
            {
                CellStyleBuilding();
                CellStyle.Triggers.Add(option.StyleTrigger);
            }
            else
            {
                //var newCellStyle = new Style(typeof(CellContentPresenter)) { BasedOn = CellStyle.BasedOn };
                //ver/14.1
                var newCellStyle = new Style(typeof(LightweightCellEditor)) { BasedOn = CellStyle.BasedOn };
                
                foreach (var tg in CellStyle.Triggers)
                {
                    newCellStyle.Triggers.Add(tg);
                }
                newCellStyle.Triggers.Add(option.StyleTrigger);
                CellStyle = newCellStyle;
            }
        }

        private void RowTriggersAdding(StyleOption option)
        {
            if (option == null || !option.ApplyToRow)
                return;
            //var newRowStyle = new Style(typeof(GridRowContent)) { BasedOn = RowStyle.BasedOn };
            //ver. 14.1
            var newRowStyle = new Style(typeof(RowControl)) { BasedOn = RowStyle.BasedOn };
            foreach (var tg in RowStyle.Triggers)
            {
                newRowStyle.Triggers.Add(tg);
            }
            newRowStyle.Triggers.Add(option.StyleTrigger);
            RowStyle = newRowStyle;
        }

        private void BarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            ShowExpressionWindow();
        }

        private void ShowExpressionWindow()
        {
            try
            {
                SetColumnInfo();
                //Конвертируем название свойств в название столбцов.
                foreach (var p in StylesCollection.Where(p => p != null))
                {
                    if (p.ExpressionString == null)
                        p.ExpressionString = string.Empty;
                    else if (p.ExpressionString != string.Empty)
                        p.ExpressionString = p.Parent.ConvertToCaptions(p.ExpressionString);

                    if (p.FormatConditionType == FormatConditionType.Default && string.IsNullOrEmpty(p.Name))
                        p.Name = p.ExpressionString;
                }

                var window = new ConditionalFormattingWindow(View, StylesCollection, true)
                {
                    Owner = Application.Current.MainWindow.IsActive ? Application.Current.MainWindow : null
                };
                if (window.ShowDialog() == true)
                {
                    if (View != null && View.DataControl is CustomTreeListControl)
                    {
                        var tree = (CustomTreeListControl) View.DataControl;
                        tree.ExpressionStyleOptions.Clear();
                        //Конвертируем название столбцов в название свойств.
                        foreach (var p in StylesCollection)
                        {
                            p.ExpressionString = p.Parent.ConvertToFields(p.ExpressionString);
                        }
                        tree.ExpressionStyleOptions.Options.AddRange(StylesCollection.Where(p => p != null && !p.IsReadOnly));
                    }
                }
            }
            finally
            {
                ShowFormatConditionsEditorWindow = false;
            }
        }

        private void SetColumnInfo()
        {
            TreeListColumn column = null;
            if (View != null && View.DataControl != null)
            {
                var columns = ((TreeListControl) View.DataControl).Columns;
                column = columns != null && columns.Count > 0 ? columns[0] : null;
            }
            StylesCollection.ColumnInfo = column == null ? null : new ExpressionColumnInfo(column);
        }
    }
}
