using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using wmsMLC.General;

namespace wmsMLC.Business.Managers.Expressions
{
    public class ExpressionContext : ICloneable
    {
        private CalcEngine.CalcEngine _calEngine;
        #region .  Constructors  .
        public ExpressionContext()
        {
            Expressions = new Dictionary<string, Expression>();
            Variables = new Dictionary<string, object>();
            _calEngine = new CalcEngine.CalcEngine();
        }

        public ExpressionContext(IEnumerable<KeyValuePair<string, object>> eo)
            : this()
        {
            foreach (var i in eo)
            {
                if (i.Value == null) 
                    continue;
                var value = i.Value.ToString();
                if (!string.IsNullOrEmpty(value))
                    Expressions.Add(i.Key, new Expression(value));
            }
        }
        #endregion

        /// <summary>
        /// Коллекция выражений, которые нужно разобрать
        /// </summary>
        public Dictionary<string, Expression> Expressions { get; private set; }

        /// <summary>
        /// Коллекция локальных переменных, которые могут быть подставлены в выражения
        /// </summary>
        public Dictionary<string, object> Variables { get; private set; }

        public void FillObject(object obj)
        {
            foreach (var ex in Expressions)
                ProcessExpression(ex.Key, obj);
        }

        private void ProcessExpression(string name, Object obj)
        {
            var expression = Expressions[name];

            // понимаем, что мы вошли в бесконечный цикл
            if (expression.Processed == Expression.ProcessingState.Processing)
                throw new CyclicDependencyException();

            // если уже обработали - выходим (нужно проверить нет ли каких нюансов)
            if (expression.Processed == Expression.ProcessingState.Processed)
                return;

            // стартуем
            expression.Processed = Expression.ProcessingState.Processing;

            // заполняем св-во объекта
            var preperties = TypeDescriptor.GetProperties(obj);
            var value = expression.ProcessVariables(s =>
                {
                    // ищем в локальных переменных
                    var localVar = Variables.FirstOrDefault(i => Extensions.EqIgnoreCase(i.Key, s)).Value;
                    if (localVar != null)
                        return localVar;

                    // если запрашиваем другую формулу, то сначала считаем ее
                    var dependedExpr = Expressions.Keys.FirstOrDefault(i => i.EqIgnoreCase(s));
                    if (dependedExpr != null)
                        ProcessExpression(dependedExpr, obj);

                    // ищем среди полей объекта
                    var property = preperties.Find(s, true);
                    if (property != null)
                        return property.GetValue(obj);

                    // ищем по DisplayName
                    property = preperties.Cast<PropertyDescriptor>().FirstOrDefault(i => s.EqIgnoreCase(i.DisplayName));

                    if (property != null)
                    {
                        var propertyExpr = Expressions.Keys.FirstOrDefault(i => i.EqIgnoreCase(property.Name));
                        if (propertyExpr != null)
                            ProcessExpression(propertyExpr, obj);
                    }

                    return property == null ? null : property.GetValue(obj);
                });

            try
            {
                var formatted = _calEngine.TryEvaluate(value);
                preperties[name].SetValue(obj, formatted);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Неправильный формат в формуле '{0}'.", value), ex);
            }

            // помечаем, как обработанный
            expression.Processed = Expression.ProcessingState.Processed;
        }

        public object Clone()
        {
            var res = new ExpressionContext();
            foreach (var e in Expressions)
                res.Expressions.Add(e.Key, new Expression(e.Value.Text));
            foreach (var v in Variables)
                res.Variables.Add(v.Key, v.Value);
            return res;
        }
    }
}