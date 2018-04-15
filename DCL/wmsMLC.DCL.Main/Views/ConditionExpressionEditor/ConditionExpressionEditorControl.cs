using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.ExpressionEditor;
using DevExpress.Data;

namespace wmsMLC.DCL.Main.Views.ConditionExpressionEditor
{
    internal class ConditionExpressionEditorControl : ExpressionEditorControl
    {
        private TextEdit _expressionTextEdit;
        public event EventHandler ExpressionChanged;

        public ConditionExpressionEditorControl()
        {

        }

        public ConditionExpressionEditorControl(IDataColumnInfo columnInfo)
            : base(columnInfo)
        {

        }

        public string ExpressionText
        {
            get { return (string)GetValue(ExpressionTextProperty); }
            set { SetValue(ExpressionTextProperty, value); }
        }
        public static DependencyProperty ExpressionTextProperty = DependencyProperty.Register("ExpressionText", typeof(string), typeof(ConditionExpressionEditorControl), new PropertyMetadata(OnExpressionTextPropertyChanged));

        private static void OnExpressionTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ConditionExpressionEditorControl)d).OnExpressionTextEditEditValueChanged();
        }

        public ConditionExpressionEditorLogic EditorLogic
        {
            get { return (ConditionExpressionEditorLogic)fEditorLogic; }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            fEditorLogic = new ConditionExpressionEditorLogic(this, ColumnInfo);
            fEditorLogic.Initialize();
            fEditorLogic.OnLoad();

            _expressionTextEdit = (TextEdit)GetTemplateChild("expressionTextEdit");
            if (_expressionTextEdit != null)
            {
                BindingOperations.SetBinding(this, ExpressionTextProperty, new Binding("EditValue") { Source = _expressionTextEdit, Mode = BindingMode.TwoWay });
            }

            //HACK: Баг при выборе функций
            var listOfInputTypes = (ListBox)GetTemplateChild("listOfInputTypes");
            if (listOfInputTypes != null && listOfInputTypes.Items != null && listOfInputTypes.Items.Count > 0)
            {
                listOfInputTypes.SelectedIndex = listOfInputTypes.Items.Count - 1;
                listOfInputTypes.SelectedIndex = 0;
            }

            var listOfInputParameters = (DXListBox)GetTemplateChild("listOfInputParameters");
            if (listOfInputParameters != null)
            {
                listOfInputParameters.PreviewMouseLeftButtonDown += (s, e) =>
                {
                    if (e.ClickCount > 1 || listOfInputParameters.SelectedItem == null) 
                        return;
                    if (_expressionTextEdit != null)
                        _expressionTextEdit.Focus();
                    DragDrop.DoDragDrop(listOfInputParameters, listOfInputParameters.SelectedItem, DragDropEffects.Copy);
                };
            }
        }

        public void SetExpression(string expression)
        {
            EditorLogic.SetExpression(expression);
        }

        private void OnExpressionTextEditEditValueChanged()
        {
            var handler = ExpressionChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}
