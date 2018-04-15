using System;
using System.Activities;
using System.Activities.Expressions;
using System.Activities.Presentation.Model;
using System.Globalization;
using System.Windows.Data;
using Microsoft.VisualBasic.Activities;

namespace wmsMLC.Activities.General
{
    public class ComboBoxItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var modelItem = value as ModelItem;
            if (value != null)
            {
                if (modelItem != null)
                {
                    var inArgument = modelItem.GetCurrentValue() as InArgument<string>;

                    if (inArgument != null)
                    {
                        var expression = inArgument.Expression;
                        var vbexpression = expression as VisualBasicValue<string>;
                        var literal = expression as Literal<string>;

                        if (literal != null)
                            return literal.Value;
                        if (vbexpression != null)
                            return vbexpression.ExpressionText == null ? null : vbexpression.ExpressionText.Substring(1, vbexpression.ExpressionText.Length - 2);
                    }
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;
            if (!string.IsNullOrEmpty(str))
                str = string.Format("\"{0}\"", str);
            var vbArgument = new VisualBasicValue<string>(str);
            var inArgument = new InArgument<string>(vbArgument);
            return inArgument;
        }
    }
}