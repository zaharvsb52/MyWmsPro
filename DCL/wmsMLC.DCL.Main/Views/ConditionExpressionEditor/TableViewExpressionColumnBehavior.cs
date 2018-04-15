using System;
using System.Linq;
using System.Windows.Interactivity;
using DevExpress.Xpf.Grid;
using System.Windows;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core.ConditionalFormatting;
using wmsMLC.DCL.Main.Views.Controls;
using wmsMLC.General;
using wmsMLC.General.PL.WPF;

namespace wmsMLC.DCL.Main.Views.ConditionExpressionEditor
{
    /// <summary>
    /// http://www.devexpress.com/Support/Center/Example/Details/E4272
    /// TableView.ShowFormatConditionDialog
    /// </summary>
    public class TableViewExpressionColumnBehavior : Behavior<TableView>
    {
        private bool _isViewLoaded;

        public TableViewExpressionColumnBehavior()
        {
            StylesCollection = new StyleOptionCollection();
        }

        #region . Properties .

        public StyleOptionCollection StylesCollection
        {
            get { return (StyleOptionCollection)GetValue(StylesCollectionProperty); }
            set { SetValue(StylesCollectionProperty, value); }
        }
        public static readonly DependencyProperty StylesCollectionProperty = DependencyProperty.Register("StylesCollection", typeof(StyleOptionCollection), typeof(TableViewExpressionColumnBehavior));

        /// <summary>
        /// Условие добавление в контекстное меню столбца меню вызова формы "Редактора формата условий".
        /// </summary>
        public bool AddFormatConditionsEditorMenuItem { get; set; }

        public bool ShowFormatConditionsEditorWindow
        {
            get { return (bool)GetValue(ShowFormatConditionsEditorWindowProperty); }
            set { SetValue(ShowFormatConditionsEditorWindowProperty, value); }
        }
        public static readonly DependencyProperty ShowFormatConditionsEditorWindowProperty = DependencyProperty.Register("ShowFormatConditionsEditorWindow", typeof(bool), typeof(TableViewExpressionColumnBehavior), new PropertyMetadata(OnShowFormatConditionsEditorWindowPropertyChanged));

        private static void OnShowFormatConditionsEditorWindowPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                ((TableViewExpressionColumnBehavior) d).ShowExpressionWindow();
        }

        private TableView View { get; set; }
        private BarButtonItem ExprButton { get; set; }

        #endregion . Properties .

        protected override void OnAttached()
        {
            base.OnAttached();
            View = AssociatedObject;

            //Совместимость с 14.1
            //View.UseLightweightTemplates = UseLightweightTemplates.None;
            //View.AllowConditionalFormattingMenu = true;

            View.Loaded += OnViewLoaded;
            
            // подписываемся на изменения стилей отображения
            var cgc = View.Grid as CustomGridControl;
            if (cgc != null)
                cgc.ExpressionStyleOptionsChanged += ExpressionStyleOptionsChanged;

            if (AddFormatConditionsEditorMenuItem)
            {
                EditorBarButtonBuilding();
                AssociatedObject.ColumnMenuCustomizations.Add(ExprButton);
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            View.Loaded -= OnViewLoaded;
            ExprButton.ItemClick -= OnExprButtonClick;
            ExprButton = null;
            if (StylesCollection != null)
            {
                StylesCollection.Clear();
                StylesCollection = null;
            }

            View.Grid.ItemsSourceChanged -= OnGridItemsSourceChanged;
            var cgc = View.Grid as CustomGridControl;
            if (cgc != null)
                cgc.ExpressionStyleOptionsChanged -= ExpressionStyleOptionsChanged;
        }

        private void ExpressionStyleOptionsChanged(object sender, EventArgs eventArgs)
        {
            FillStylesCollection();
        }

        private void OnViewLoaded(object sender, RoutedEventArgs e)
        {
            if (_isViewLoaded)
                return;
            _isViewLoaded = true;

            FillStylesCollection();
            View.Grid.ItemsSourceChanged -= OnGridItemsSourceChanged;
            View.Grid.ItemsSourceChanged += OnGridItemsSourceChanged;

            SetColumnInfo();
        }

        private void CollectionModify(StyleOption styleOption)
        {
            if (styleOption == null)
                return;

            FormatConditionBase formatCondition = null;

            Func<TopBottomRule, FormatConditionBase> createTopBottomRuleFormatConditionHandler = rule =>
            {
                var result = new TopBottomRuleFormatCondition();
                var format = CreateFormat(styleOption);
                if (format != null)
                    result.Format = format;
            
                result.Rule = rule;
                result.Threshold = styleOption.Threshold;


                if (styleOption.ApplyToRow)
                    result.Expression = string.Format("[{0}]", styleOption.FieldName);
                else
                    result.FieldName = styleOption.FieldName;

                return result;
            };

            Action<FormatConditionBase> setAnimatedFormatConditionHandler = fc =>
            {
                formatCondition = fc;
                formatCondition.FieldName = styleOption.FieldName;
                formatCondition.PredefinedFormatName = styleOption.PredefinedFormatName;
            };

            switch (styleOption.FormatConditionType)
            {
                case FormatConditionType.Default:
                    var condition = new FormatCondition();
                    var format = CreateFormat(styleOption);
                    if (format != null)
                        condition.Format = format;

                    formatCondition = condition;
                    formatCondition.FieldName = styleOption.ApplyToRow ? null : styleOption.FieldName;
                    formatCondition.Expression = styleOption.ExpressionString;
                    break;

                case FormatConditionType.TopItemsRule:
                    formatCondition = createTopBottomRuleFormatConditionHandler(TopBottomRule.TopItems);
                    break;
                case FormatConditionType.TopPersentRule:
                    formatCondition = createTopBottomRuleFormatConditionHandler(TopBottomRule.TopPercent);
                    break;
                case FormatConditionType.BottomItemsRule:
                    formatCondition = createTopBottomRuleFormatConditionHandler(TopBottomRule.BottomItems);
                    break;
                case FormatConditionType.BottomPercentRule:
                    formatCondition = createTopBottomRuleFormatConditionHandler(TopBottomRule.BottomPercent);
                    break;
                case FormatConditionType.AboveAverageRule:
                    //Не работает
                    styleOption.ApplyToRow = false;
                    formatCondition = createTopBottomRuleFormatConditionHandler(TopBottomRule.AboveAverage);
                    break;
                case FormatConditionType.BelowAverageRule:
                    //Не работает
                    styleOption.ApplyToRow = false;
                    formatCondition = createTopBottomRuleFormatConditionHandler(TopBottomRule.BelowAverage);
                    break;
                case FormatConditionType.DataBar:
                    var dataBarCondition = new DataBarFormatCondition();
                    //Необходимо специальное форматирование для этого свойства.
                    setAnimatedFormatConditionHandler(dataBarCondition);
                    break;
                case FormatConditionType.ColorScale:
                    var colorScaleCondition = new ColorScaleFormatCondition();
                    //Необходимо специальное форматирование для этого свойства.
                    setAnimatedFormatConditionHandler(colorScaleCondition);
                    break;
                case FormatConditionType.IconSet:
                    var iconSetCondition = new IconSetFormatCondition();
                    //Необходимо специальное форматирование для этого свойства.
                    setAnimatedFormatConditionHandler(iconSetCondition);
                    break;

                default:
                    throw new DeveloperException("Undefined FormatConditionType '{0}'.", styleOption.FormatConditionType);
            }

            if (formatCondition != null)
                View.AddFormatCondition(formatCondition);
        }

        private void CollectionModify()
        {
            try
            {
                View.FormatConditions.BeginUpdate();

                foreach (var t in StylesCollection.Where(p => p != null))
                {
                    CollectionModify(t);
                }
            }
            finally
            {
                View.FormatConditions.EndUpdate();
            }
        }

        private Format CreateFormat(StyleOption styleOption)
        {
            if (styleOption == null)
                return null;

            Format result = null;
            Func<Format> formatHandler = () => result ?? (result = new Format());
            
            if (styleOption.Foreground != null)
                formatHandler().Foreground = styleOption.Foreground;
            if (styleOption.Background != null)
                formatHandler().Background = styleOption.Background;
            if (styleOption.FontFamily != null)
                formatHandler().FontFamily = styleOption.FontFamily;
            if (styleOption.FontStyle.HasValue)
                formatHandler().FontStyle = styleOption.FontStyle.Value;
            if (styleOption.FontSize > 0)
                formatHandler().FontSize = styleOption.FontSize;
            if (styleOption.FontWeight.HasValue)
                formatHandler().FontWeight = styleOption.FontWeight.Value;

            return result;
        }

        private void OnGridItemsSourceChanged(object sender, ItemsSourceChangedEventArgs itemsSourceChangedEventArgs)
        {
            SetColumnInfo();
        }

        private void FillStylesCollection()
        {
            if (!_isViewLoaded)
                return;

            var grid = View.Grid as CustomGridControl;
            if (grid == null) 
                return;

            foreach (var p in StylesCollection.Where(p => !p.IsReadOnly).ToArray())
            {
                StylesCollection.Remove(p);
            }

            foreach (var o in grid.ExpressionStyleOptions.Options)
            {
                StylesCollection.Add(new StyleOption(o));
            }

            CollectionModify();
        }

        private void EditorBarButtonBuilding()
        {
            ExprButton = new BarButtonItem { Name = "barButtonItem1", Content = Resources.StringResources.ConditionalFormattingWindowTitle };
            ExprButton.ItemClick += OnExprButtonClick;
        }

        private void OnExprButtonClick(object sender, ItemClickEventArgs e)
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

                var window = new ConditionalFormattingWindow(View, StylesCollection)
                {
                    Owner = Application.Current.MainWindow.IsActive ? Application.Current.MainWindow : null
                };
                if (window.ShowDialog() == true)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        if (View.Grid is CustomGridControl)
                        {
                            var grid = (CustomGridControl) View.Grid;
                            grid.ExpressionStyleOptions.Clear();

                            //Конвертируем название столбцов в название свойств.
                            foreach (var p in StylesCollection.Where(p => p != null && !string.IsNullOrEmpty(p.ExpressionString)))
                            {
                                p.ExpressionString = p.Parent.ConvertToFields(p.ExpressionString);
                            }
                            grid.ExpressionStyleOptions.Options.AddRange(
                                StylesCollection.Where(p => p != null && !p.IsReadOnly));
                        }

                        View.ClearFormatConditionsFromAllColumns();
                        CollectionModify();
                    }));
                }
            }
            finally
            {
                ShowFormatConditionsEditorWindow = false;
            }
        }

        private void SetColumnInfo()
        {
            GridColumn column = null;
            if (View != null && View.Grid != null)
            {
                column = View.Grid.Columns != null && View.Grid.Columns.Count > 0 ? View.Grid.Columns[0] : null;
            }
            StylesCollection.ColumnInfo = column == null ? null : new ExpressionColumnInfo(column);
        }
    }
}
