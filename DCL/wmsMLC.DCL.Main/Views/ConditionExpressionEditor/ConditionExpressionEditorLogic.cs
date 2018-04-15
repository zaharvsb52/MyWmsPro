using DevExpress.Data.ExpressionEditor;
using DevExpress.Data;

namespace wmsMLC.DCL.Main.Views.ConditionExpressionEditor
{
    internal class ConditionExpressionEditorLogic : ExpressionEditorLogicEx
    {
        public ConditionExpressionEditorLogic(IExpressionEditor editor, IDataColumnInfo columnInfo)
            : base(editor, columnInfo)
        {

        }

        public void SetExpression(string expression)
        {
            ExpressionMemoEdit.Text = expression;
        }
    }
}
