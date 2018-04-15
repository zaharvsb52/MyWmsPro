using System;
using System.Linq;
using System.Text.RegularExpressions;
using wmsMLC.General;

namespace wmsMLC.Business.Managers.Expressions
{
    /// <summary>
    /// Выражение, которое может быть использовано для вычисления значения поля
    /// </summary>
    public class Expression
    {
        /// <summary>
        /// Статус
        /// </summary>
        public enum ProcessingState
        {
            None,
            Processing,
            Processed
        }

        static readonly Regex RangeRegexp = new Regex(@"\#\{((?<name>.*);)?(?<ranges>[^\}]+)\}");
        static readonly Regex VariableRegexp = new Regex(@"\$\{(?<name>[^\}]+)*\}");

        public Expression(string text)
        {
            Text = text;
        }

        /// <summary>
        /// Текст формулы
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Статус текущего вычисления (используется для оптимизации прохода по тереву условий)
        /// </summary>
        public ProcessingState Processed { get; set; }

        public RangeGroup GetRanges()
        {
            if (string.IsNullOrEmpty(Text) || !Text.Contains("#{"))
                return null;

            var matches = RangeRegexp.Matches(Text);
            if (matches.Count == 0)
                return null;

            if (matches.Count > 1)
                throw new DeveloperException("В одном поле не должно быть более одного 'range'");

            var match = matches[0];

            var res = new RangeGroup();
            res.ExpressionText = match.Value;

            var nameMatch = match.Groups["name"];
            if (nameMatch.Success)
                res.Name = nameMatch.Value;

            var rangesMatch = match.Groups["ranges"];
            if (!rangesMatch.Success)
                throw new DeveloperException(string.Format("Невозможно прочитать диапазоны в выражении '{0}'", Text));

            // вычисляем диапазоны
            var values = rangesMatch.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var ranges = new Range[values.Length];

            try
            {
                for (int i = 0; i < values.Length; i++)
                {
                    var rangeIdx = values[i].IndexOf('-');
                    if (rangeIdx == -1)
                    {
                        var idx = int.Parse(values[i]);
                        ranges[i] = new Range(idx, idx);
                    }
                    else
                    {
                        var left = int.Parse(values[i].Substring(0, rangeIdx));
                        var right = int.Parse(values[i].Substring(rangeIdx + 1));
                        ranges[i] = new Range(left, right);
                    }
                }
            }
            catch (FormatException ex)
            {
                throw new Exception(string.Format("Неправильный формат в формуле '{0}'.", res.ExpressionText), ex);
            }

            res.Ranges.AddRange(ranges);
            return res;
        }

        /// <summary>
        /// Метод возвращает строку, в которой объявления переменных заменены их значениями.
        /// <remarks>Если какую-либо переменную не удается определить, то она учавствует в выражении в том виде, в котором была объявлена</remarks>
        /// </summary>
        /// <param name="variableResolver">Делегат, который будет вызван для разрешения переменной</param>
        public string ProcessVariables(Func<string, object> variableResolver)
        {
            // быстрый выход
            if (string.IsNullOrEmpty(Text) || !Text.Contains("$") || variableResolver == null)
                return Text;

            var expression = Text;
            var parameters = VariableRegexp.Matches(expression).Cast<Match>();
            foreach (var parameter in parameters)
            {
                var variableName = parameter.Groups["name"];
                var variableValue = variableResolver(variableName.Value);

                // если параметр нашли - заменяем в исходной строке
                if (variableValue != null)
                    expression = expression.Replace(parameter.Value, variableValue.ToString());
            }

            return expression;
        }

        public void SetRangeValue(RangeGroup rangeGroup, string value)
        {
            Text = Text.Replace(rangeGroup.ExpressionText, value);
        }
    }
}