using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DevExpress.Charts.Native;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Charts;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.PivotGrid;
using DevExpress.Xpf.PivotGrid.Printing;
using DevExpress.Xpf.Printing;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels;

namespace wmsMLC.DCL.Main.Views
{
    public partial class PreviewPivotGridView : BaseDialogWindow, IHelpHandler
    {
        public PreviewPivotGridView()
        {
            InitializeComponent();

            //Свойство изменено (было PrintLayoutMode.Auto) из-за ошибки: Page size is too small to contain fixed panes and data cells. Please increase the page size or set the PivotGridControl.PrintLayoutMode property to SinglePageLayout. 
            pivotGridControl.PrintLayoutMode = PrintLayoutMode.SinglePageLayout;

            Loaded += OnLoaded;
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            DataContextChanged -= OnDataContextChanged;

            var vm = DataContext as IListViewModel;
            if (vm == null)
                return;

            Field.Clear();

            foreach (var field in vm.Fields)
            {
                var newField = new PivotGridField(field.FieldName, FieldArea.FilterArea)
                {
                    Caption = field.Caption,
                    Visible = false
                };
                Field.Items.Add(newField.Caption);
                if (!string.IsNullOrEmpty(field.DisplayFormat))
                {
                    newField.CellFormat = field.DisplayFormat;
                    //newField.ValueFormat = field.DisplayFormat;
                }
                else
                {
                    newField.CellFormat = "{0}";
                    newField.ValueFormat = "{0}";
                }
                pivotGridControl.Fields.Add(newField);
            }
            pivotGridControl.BeginUpdate();
            pivotGridControl.AllowCrossGroupVariation = false;
            pivotGridControl.EndUpdate();

            var arr = EnumHelper.GetValues(typeof(FieldSummaryType));
            foreach (FieldSummaryType type in arr)
                SummaryType.Items.Add(type);
            Field.SelectedIndex = 0;

            var arr1 = EnumHelper.GetValues(typeof(FieldSummaryDisplayType));
            foreach (FieldSummaryDisplayType type in arr1)
                SummaryDisplayType.Items.Add(type.ToString());
            SummaryDisplayType.SelectedIndex = 1;

        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var screenWidth = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
            var screenHeight = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;

            SizeToContent = SizeToContent.Manual;
            Width = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
            Height = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;

            var windowWidth = Width;
            var windowHeight = Height;
            Left = (screenWidth - windowWidth) / 2;
            Top = (screenHeight - windowHeight) / 2;

            ChartFactory.InitComboBox(cbChartType, null);
        }

        private void Item_PrintPivorGrid(object sender, ItemClickEventArgs e)
        {
            OnPrintPivotGrid();
        }
     
        private void Button_PrintPivorGrid(object sender, RoutedEventArgs e)
        {
            OnPrintPivotGrid();
        }

        private void OnPrintPivotGrid()
        {
            pivotGridControl.ShowPrintPreview(this);
        }

        private void sl_CreateDetail(object sender, CreateAreaEventArgs e)
        {
            var brush = new VisualBrush(chartControl);
            var visual = new DrawingVisual();
            var context = visual.RenderOpen();

            context.DrawRectangle(brush, null, new Rect(0, 0, chartControl.ActualWidth, chartControl.ActualHeight));
            context.Close();

            var bmp = new RenderTargetBitmap((int)chartControl.ActualWidth, (int)chartControl.ActualHeight, 96, 96, PixelFormats.Pbgra32);

            bmp.Render(visual);
            e.Data = bmp;
        }
       
        private void Button_PrintChart(object sender, RoutedEventArgs e)
        {
            var sl = new SimpleLink {DetailCount = 1, DetailTemplate = (DataTemplate) Resources["Data"]};
            sl.CreateDetail += sl_CreateDetail;
            sl.CreateDocument(true);
            //sl.PrintingSystem.Document.AutoFitToPagesWidth = 1; //возникает ошибка System.NotSupportedException' in DevExpress.Xpf.Printing.v15.2.dll
            sl.ShowPrintPreviewDialog(this);
        }

        private void Field_OnSelectedIndexChanged(object sender, RoutedEventArgs e)
        {
            foreach (var field in pivotGridControl.Fields.Where(field => field.Caption == Field.SelectedItem.ToString()))
                SummaryType.SelectedIndex = SummaryType.Items.IndexOf(field.SummaryType);
        }

        private void SummaryType_OnSelectedIndexChanged(object sender, RoutedEventArgs e)
        {
            foreach (var field in pivotGridControl.Fields.Where(field => field.Caption == Field.SelectedItem.ToString()))
                field.SummaryType = (FieldSummaryType)SummaryType.SelectedItem;
        }

        private void OnPivotGridCustomSummary(object sender, PivotCustomSummaryEventArgs e)
        {
            e.CustomValue = e.SummaryValue.Summary;
        }

        private void SummaryDisplayType_OnSelectedIndexChanged(object sender, RoutedEventArgs e)
        {
            var value = SummaryDisplayType.SelectedItem.ToString();
            foreach (PivotGridField field in pivotGridControl.Fields.Where(field => field.Caption == Field.SelectedItem.ToString()))
                field.SummaryDisplayType = (FieldSummaryDisplayType) Enum.Parse(typeof (FieldSummaryDisplayType), value, false);
        }

        private void PivotGridControl_OnFieldAreaChanged(object sender, PivotFieldEventArgs e)
        {
            e.Field.ShowSummaryTypeName = e.Field.Area == FieldArea.DataArea;
        }

        private void cbChartType_SelectedIndexChanged(object sender, RoutedEventArgs e)
        {
            if (cbChartType.SelectedIndex < 0)
                return;
            chartControl.Diagram = ChartFactory.GenerateDiagram((Type)((ComboBoxEditItem)cbChartType.SelectedItem).Tag, ceShowPointsLabels.IsChecked);
            pivotGridControl.ChartProvideEmptyCells = IsProvideEmptyCells();
        }
        private void ceShowPointsLabels_Checked(object sender, RoutedEventArgs e)
        {
            chartControl.Diagram.SeriesTemplate.LabelsVisibility = object.Equals(ceShowPointsLabels.IsChecked, true);
            chartControl.CrosshairEnabled = object.Equals(ceShowPointsLabels.IsChecked, false);
        }

        private void oncrChartDataVerticalSelectedIndexChanged(object sender, RoutedEventArgs e)
        {
            pivotGridControl.ChartProvideDataByColumns = crChartDataVertical.SelectedIndex == 1;
        }

        void chartControl_BoundDataChanged(object sender, RoutedEventArgs e)
        {
            if (chartControl.Diagram is SimpleDiagram2D)
                ConfigurePie();
            if (chartControl.Diagram is SimpleDiagram3D)
                ConfigurePie();
        }

        void ConfigurePie()
        {
            var counts = new Dictionary<PieSeries, int>();
            foreach (PieSeries series in chartControl.Diagram.Series)
            {
                counts.Add(series, GetPointsCount(series));
                series.Titles.Add(new Title() { Content = series.DisplayName, Dock = Dock.Bottom, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, FontSize = 12, VerticalAlignment = System.Windows.VerticalAlignment.Center });
                series.ShowInLegend = false;
            }

            var max = 0;
            PieSeries maxSeries = null;
            foreach (var pair in counts)
                if (max < pair.Value)
                {
                    max = pair.Value;
                    maxSeries = pair.Key;
                }

            if (maxSeries == null)
                return;
            var values = maxSeries.Points.Select(point => point.Argument).ToList();

            maxSeries.ShowInLegend = true;

            if (!(chartControl.Diagram is SimpleDiagram2D)) 
                return;

            foreach (PieSeries series in chartControl.Diagram.Series)
            {
                foreach (var point in maxSeries.Points.Where(point => !values.Contains(point.Argument)))
                {
                    series.ShowInLegend = true;
                    values.Add(point.Argument);
                }
            }
        }

        int GetPointsCount(PieSeries series)
        {
            return series.Points.Count(t => !double.IsNaN(t.Value));
        }

        bool IsProvideEmptyCells()
        {
            return (chartControl.Diagram is SimpleDiagram2D) || (chartControl.Diagram is SimpleDiagram3D);
        }

        private void PivotGridControl_OnCustomChartDataSourceData(object sender, PivotCustomChartDataSourceDataEventArgs e)
        {
            if (IsProvideEmptyCells())
            {
                if (e.ItemDataMember == PivotChartItemDataMember.Value && e.Value == DBNull.Value)
                    e.Value = 0;
            }
        }

        #region . IHelpHandler .
        string IHelpHandler.GetHelpLink()
        {
            return "wmsMLC";
        }

        string IHelpHandler.GetHelpEntity()
        {
            return "PivotGrid";
        }
        #endregion
    }
    
    internal static class ChartFactory
    {
        static readonly Type XYDiagram2DType = typeof(XYDiagram2D);
        static readonly Type XYDiagram3DType = typeof(XYDiagram3D);
        static readonly Type SimpleDiagram3DType = typeof(SimpleDiagram3D);
        static readonly Type SimpleDiagram2DType = typeof(SimpleDiagram2D);
        static readonly Type DefaultSeriesType = typeof(BarStackedSeries2D);

        static Dictionary<Type, SeriesTypeDescriptor> seriesTypes;

        public static Dictionary<Type, SeriesTypeDescriptor> SeriesTypes
        {
            get { return seriesTypes ?? (seriesTypes = CreateSeriesTypes()); }
        }

        static Dictionary<Type, SeriesTypeDescriptor> CreateSeriesTypes()
        {
            var seriesTypes = new Dictionary<Type, SeriesTypeDescriptor>();
            seriesTypes.Add(typeof(AreaFullStackedSeries2D), new SeriesTypeDescriptor { DiagramType = XYDiagram2DType, DisplayText = "Area Full-Stacked Series 2D" });
            seriesTypes.Add(typeof(AreaSeries2D), new SeriesTypeDescriptor { DiagramType = XYDiagram2DType, DisplayText = "Area Series 2D" });
            seriesTypes.Add(typeof(AreaStackedSeries2D), new SeriesTypeDescriptor { DiagramType = XYDiagram2DType, DisplayText = "Area Stacked Series 2D" });
            seriesTypes.Add(typeof(BarFullStackedSeries2D), new SeriesTypeDescriptor { DiagramType = XYDiagram2DType, DisplayText = "Bar Full-Stacked Series 2D" });
            seriesTypes.Add(typeof(BarStackedSeries2D), new SeriesTypeDescriptor { DiagramType = XYDiagram2DType, DisplayText = "Bar Stacked Series 2D" });
            seriesTypes.Add(typeof(LineSeries2D), new SeriesTypeDescriptor { DiagramType = XYDiagram2DType, DisplayText = "Line Series 2D" });
            seriesTypes.Add(typeof(PointSeries2D), new SeriesTypeDescriptor { DiagramType = XYDiagram2DType, DisplayText = "Point Series 2D" });
            seriesTypes.Add(typeof(AreaSeries3D), new SeriesTypeDescriptor { DiagramType = XYDiagram3DType, DisplayText = "Area Series 3D" });
            seriesTypes.Add(typeof(AreaStackedSeries3D), new SeriesTypeDescriptor { DiagramType = XYDiagram3DType, DisplayText = "Area Stacked Series 3D" });
            seriesTypes.Add(typeof(AreaFullStackedSeries3D), new SeriesTypeDescriptor { DiagramType = XYDiagram3DType, DisplayText = "Area Full-Stacked Series 3D" });
            seriesTypes.Add(typeof(BarSeries3D), new SeriesTypeDescriptor { DiagramType = XYDiagram3DType, DisplayText = "Bar Series 3D" });
            seriesTypes.Add(typeof(PointSeries3D), new SeriesTypeDescriptor { DiagramType = XYDiagram3DType, DisplayText = "Point Series 3D" });
            seriesTypes.Add(typeof(PieSeries3D), new SeriesTypeDescriptor { DiagramType = SimpleDiagram3DType, DisplayText = "Pie Series 3D" });
            seriesTypes.Add(typeof(PieSeries2D), new SeriesTypeDescriptor { DiagramType = SimpleDiagram2DType, DisplayText = "Pie Series 2D" });
            return seriesTypes;
        }

        public class SeriesTypeDescriptor
        {
            public Type DiagramType { get; set; }
            public string DisplayText { get; set; }
        }

        public static int CompareComboItemsByStringContent(ComboBoxEditItem first, ComboBoxEditItem second)
        {
            var firstStr = first.Content as string;
            return firstStr == null ? -1 : String.Compare(firstStr, second.Content as string, StringComparison.Ordinal);
        }
        public static void InitComboBox(ComboBoxEdit comboBox, Type[] diagramFilter)
        {
            var itemsList = new List<ComboBoxEditItem>();
            ComboBoxEditItem item, selectedItem = null;
            foreach (var seriesType in SeriesTypes.Keys)
            {
                var sd = SeriesTypes[seriesType];
                if (diagramFilter != null && Array.IndexOf(diagramFilter, sd.DiagramType) < 0) continue;
                item = new ComboBoxEditItem {Content = sd.DisplayText, Tag = seriesType};
                itemsList.Add(item);
                if (seriesType == DefaultSeriesType)
                    selectedItem = item;
            }
            itemsList.Sort(CompareComboItemsByStringContent);
            comboBox.Items.AddRange(itemsList.ToArray());
            comboBox.SelectedItem = selectedItem;
        }
        public static Diagram GenerateDiagram(Type seriesType, bool? showPointsLabels)
        {
            var seriesTemplate = CreateSeriesInstance(seriesType);
            var diagram = CreateDiagramBySeriesType(seriesType);
            if (diagram is XYDiagram2D)
                PrepareXYDiagram2D(diagram as XYDiagram2D);
            if (diagram is XYDiagram3D)
                PrepareXYDiagram3D(diagram as XYDiagram3D);
            var d = diagram as Diagram3D;
            if (d != null)
                d.RuntimeRotation = true;
            diagram.SeriesDataMember = "Series";
            seriesTemplate.ArgumentDataMember = "Arguments";
            seriesTemplate.ValueDataMember = "Values";
            if (seriesTemplate.Label == null)
                seriesTemplate.Label = new SeriesLabel();
            seriesTemplate.LabelsVisibility = showPointsLabels == true;
            if (seriesTemplate is PieSeries2D || seriesTemplate is PieSeries3D)
            {
                if (seriesTemplate.LegendPointOptions == null)
                    seriesTemplate.LegendPointOptions = new PointOptions();
                seriesTemplate.LegendPointOptions.PointView = PointView.Argument;
                seriesTemplate.PointOptions = new PointOptions();
                seriesTemplate.PointOptions.PointView = PointView.ArgumentAndValues;
                seriesTemplate.PointOptions.ValueNumericOptions = new NumericOptions();
                seriesTemplate.PointOptions.ValueNumericOptions.Format = NumericFormat.Percent;
                seriesTemplate.PointOptions.ValueNumericOptions.Precision = 0;
            }
            else
            {
                if (seriesTemplate.LegendPointOptions == null)
                    seriesTemplate.LegendPointOptions = new PointOptions();
                seriesTemplate.LegendPointOptions.PointView = PointView.ArgumentAndValues;
                seriesTemplate.PointOptions = null;
                seriesTemplate.ShowInLegend = true;
            }
            diagram.SeriesTemplate = seriesTemplate;
            return diagram;
        }
        static void PrepareXYDiagram2D(XYDiagram2D diagram)
        {
            if (diagram == null) return;
            diagram.AxisX = new AxisX2D {Label = new AxisLabel {Staggered = true}};
        }
        static void PrepareXYDiagram3D(XYDiagram3D diagram)
        {
            if (diagram == null) return;
            diagram.AxisX = new AxisX3D {Label = new AxisLabel {Visible = false}};
        }
        public static Series CreateSeriesInstance(Type seriesType)
        {
            var series = (Series)Activator.CreateInstance(seriesType);
            var supportTransparency = series as ISupportTransparency;
            if (supportTransparency == null) 
                return series;

            var flag = series is AreaSeries2D;
            flag = flag || series is AreaSeries3D;
            supportTransparency.Transparency = flag ? 0.4 : 0;
            return series;
        }

        static Diagram CreateDiagramBySeriesType(Type seriesType)
        {
            return (Diagram)Activator.CreateInstance(SeriesTypes[seriesType].DiagramType);
        }
    }
}


