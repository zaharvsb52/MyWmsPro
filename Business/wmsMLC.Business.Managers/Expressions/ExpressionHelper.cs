using System;
using System.Collections.Generic;

namespace wmsMLC.Business.Managers.Expressions
{
    public static class ExpressionHelper
    {
        /// <summary>
        /// Вычислить
        /// </summary>
        /// <typeparam name="T">Тип объекта над которым будет производиться вычисление</typeparam>
        /// <param name="initialObject">объект над которым производятся вычисления</param>
        /// <param name="initialContext">начальный конеткс (список формул)</param>
        /// <returns>Список расчитанных занчений</returns>
        public static IEnumerable<T> Process<T>(T initialObject, ExpressionContext initialContext)
            where T : ICloneable
        {
            var res = new List<T>();
            IEnumerable<ExpressionContext> contexts;

            // размножаем условия, если нужно
            if (TryGetMultiply(initialContext, out contexts))
            {
                foreach (var context in contexts)
                {
                    // клонируем новый целевой объект
                    var newObject = (T)initialObject.Clone();

                    // заполняем рассчитанные формулы
                    context.FillObject(newObject);

                    // добавляем в коллекцию
                    res.Add(newObject);
                }
            }
            else
            {
                // заполняем рассчитанные формулы
                var newObject = (T) initialObject.Clone();
                initialContext.FillObject(newObject);
                res.Add(newObject);
            }
            return res;
        }

        private static bool TryGetMultiply(ExpressionContext context, out IEnumerable<ExpressionContext> contexts)
        {
            var items = new List<ExpressionContext>();
            foreach (var expression in context.Expressions)
            {
                // разбираем выражение
                var rangeGroup = expression.Value.GetRanges();
                if (rangeGroup == null)
                    continue;

                // создаем кучку объектов
                foreach (var range in rangeGroup.Ranges)
                {
                    for (int i = range.Start; i <= range.End; i++)
                    {
                        var item = (ExpressionContext)context.Clone();

                        // добавлем в локальные переменные значение текущего счетчика
                        if (!string.IsNullOrEmpty(rangeGroup.Name))
                            item.Variables.Add(rangeGroup.Name, i);

                        // заменяем в новом контексте объявление range-а на текущее значение
                        item.Expressions[expression.Key].SetRangeValue(rangeGroup, i.ToString());
                        var innerItems = new List<ExpressionContext>() as IEnumerable<ExpressionContext>;
                        if (TryGetMultiply(item, out innerItems))
                            items.AddRange(innerItems);
                        else
                            items.Add(item);
                    }
                }

                break;
            }

            contexts = items.Count > 0 ? items : null;

            return items.Count > 0;
        }
    }
}