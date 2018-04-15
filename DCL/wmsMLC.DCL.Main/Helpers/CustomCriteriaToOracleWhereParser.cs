using System;
using System.Linq;
using DevExpress.Data.Db;
using DevExpress.Data.Filtering;
using DevExpress.Data.Filtering.Helpers;

namespace wmsMLC.DCL.Main.Helpers
{
    /// <summary>
    /// Класс в замен DevExpress - CriteriaToWhereClauseHelper.GetOracleWhere
    /// <remarks>
    /// Explicit реализация позволяет сделать неявное наследование с переопеделением некоторых методов базового класса.
    /// Часть методов IClientCriteriaVisitor<string> реализована в базовом классе
    /// </remarks>
    /// </summary>
    internal class CustomCriteriaToOracleWhereParser : BaseWhereGenerator, IClientCriteriaVisitor<string>
    {
        private bool _isNeedUpper;

        protected override string FormatOperandProperty(OperandProperty operand)
        {
            return operand.PropertyName;
        }

        string ICriteriaVisitor<string>.Visit(FunctionOperator theOperator)
        {
            try
            {
                _isNeedUpper = theOperator.Operands.Any(IsNeedUpper);

                var str = OracleFormatterHelper.FormatFunction(obj => Process((CriteriaOperator)obj), theOperator.OperatorType, theOperator.Operands.ToArray());
                if (!string.IsNullOrEmpty(str))
                    return str;

                var operands = new string[theOperator.Operands.Count];
                for (var i = 0; i < theOperator.Operands.Count; i++)
                    operands[i] = Process(theOperator.Operands[i]);
                str = OracleFormatterHelper.FormatFunction(theOperator.OperatorType, operands);
                if (!string.IsNullOrEmpty(str))
                    return str;

                return base.VisitInternal(theOperator);
            }
            finally
            {
                _isNeedUpper = false;
            }
        }

        string ICriteriaVisitor<string>.Visit(OperandValue theOperand)
        {
            var c = theOperand as ConstantValue;
            var str = ReferenceEquals(c, null)
                ? theOperand.LegacyToString()
                : SqlExpressionHelper.GetCorrectStringValue(c.Value);

            if (string.IsNullOrEmpty(str))
                return str;

            return _isNeedUpper ? string.Format("upper({0})", str) : str;
        }

        string ICriteriaVisitor<string>.Visit(InOperator theOperator)
        {
            return string.Format("{0} in ({1})", Process(theOperator.LeftOperand),
                string.Join(", ", theOperator.Operands.Select(Process).Distinct().ToArray()));
        }

        string ICriteriaVisitor<string>.Visit(UnaryOperator theOperator)
        {
            return BaseFormatterHelper.DefaultFormatUnary(theOperator.OperatorType, Process(theOperator.Operand));
        }

        string ICriteriaVisitor<string>.Visit(BinaryOperator theOperator)
        {
            // если левый или правый оператор нужно приводить к верхнему регистру, то приводим оба
            _isNeedUpper = IsNeedUpper(theOperator.LeftOperand) || IsNeedUpper(theOperator.RightOperand);
            try
            {
                string leftOperand = Process(theOperator.LeftOperand);
                string rightOperand = Process(theOperator.RightOperand);
                return OracleFormatterHelper.FormatBinary(theOperator.OperatorType, leftOperand, rightOperand); 
            }
            finally
            {
                _isNeedUpper = false;
            }
        }

        string ICriteriaVisitor<string>.Visit(BetweenOperator theOperator)
        {
            return string.Format("{0} between {1} and {2}", Process(theOperator.TestExpression), Process(theOperator.BeginExpression), Process(theOperator.EndExpression));
        }

        string IClientCriteriaVisitor<string>.Visit(OperandProperty theOperand)
        {
            return _isNeedUpper ? string.Format("upper({0})", theOperand.PropertyName) : theOperand.PropertyName;
        }

        string IClientCriteriaVisitor<string>.Visit(AggregateOperand theOperand)
        {
            throw new NotImplementedException();
        }

        string IClientCriteriaVisitor<string>.Visit(JoinOperand theOperand)
        {
            throw new NotImplementedException();
        }

        private static bool IsNeedUpper(CriteriaOperator op)
        {
            var c = op as ConstantValue;
            if (ReferenceEquals(c, null))
                return false;

            return c.Value is string;
        }
    }
}