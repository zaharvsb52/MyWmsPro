using System;
using System.Activities.Presentation.Model;
using System.Globalization;
using System.Runtime;
using System.Text;
using System.Windows.Data;

namespace wmsMLC.Activities.Dialogs.Views
{
    public sealed class GenericTypeParameterConverter : IValueConverter
    {
        // Fields
        private readonly bool _displayFullName;

        // Methods
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public GenericTypeParameterConverter() : this(false)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public GenericTypeParameterConverter(bool displayFullName)
        {
            _displayFullName = displayFullName;
        }

        private Type BuildTargetType(Type[] argumentTypes, Type containerType)
        {
            if (containerType.IsGenericType)
            {
                return containerType.MakeGenericType(argumentTypes);
            }
            return containerType;
        }

        private Type GetContainerType(object value)
        {
            if (value != null)
            {
                if (value is ModelItem)
                {
                    value = ((ModelItem) value).GetCurrentValue();
                }
                if (value is Type)
                {
                    return (Type) value;
                }
                if (value is string)
                {
                    return Type.GetType((string) value, false, true);
                }
            }
            return null;
        }

        private Type[] GetGenericTypeArguments(object value)
        {
            var valueinternal = value;
            if (valueinternal is ModelItem)
                valueinternal = ((ModelItem) valueinternal).GetCurrentValue();

            if (!(valueinternal is Type))
                return null;

            var type = (Type) valueinternal;
            if (type.IsGenericType)
                return type.GetGenericArguments();

            return new[] {type};
        }

        private object HandleConversion(Type resultType, Type convertToType)
        {
            if (typeof (string) == convertToType)
            {
                if (resultType.IsGenericType)
                {
                    var builder = new StringBuilder();
                    builder.Append(_displayFullName
                        ? resultType.FullName.Substring(0, resultType.FullName.IndexOf('`'))
                        : resultType.Name.Substring(0, resultType.Name.IndexOf('`')));
                    builder.Append("<");
                    var flag = false;
                    foreach (var type in resultType.GetGenericArguments())
                    {
                        builder.Append(flag ? "," : string.Empty);
                        if (type.IsGenericType)
                        {
                            builder.Append(HandleConversion(type, convertToType));
                        }
                        else
                        {
                            builder.Append(_displayFullName ? type.FullName : type.Name);
                        }
                        flag = true;
                    }
                    builder.Append(">");
                    return builder.ToString();
                }
                if (!_displayFullName)
                    return resultType.Name;
                return resultType.FullName;
            }
            return typeof (Type) == convertToType ? resultType : null;
        }

        object IValueConverter.Convert(object value, Type convertToType, object containerParameter, CultureInfo culture)
        {
            var genericTypeArguments = GetGenericTypeArguments(value);
            var containerType = GetContainerType(containerParameter);
            if ((genericTypeArguments != null) && (containerType != null))
            {
                var resultType = BuildTargetType(genericTypeArguments, containerType);
                return HandleConversion(resultType, convertToType);
            }
            containerType = GetContainerType(value);

            return containerType != null ? HandleConversion(containerType, convertToType) : null;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //public class StringToTypeConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return value != null ? ((Type)value).Name : null;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        var svalue = value.To<string>();
    //        var type = string.IsNullOrEmpty(svalue) ? null : Type.GetType(svalue);

    //        if (type == null)
    //        {
    //            if (svalue.EqIgnoreCase("string"))
    //                type = typeof(string);
    //            else if (svalue.EqIgnoreCase("decimal"))
    //                type = typeof(decimal);
    //            else if (svalue.EqIgnoreCase("System.Windows.Controls.Button") || svalue.EqIgnoreCase("Button"))
    //                type = typeof(Button);
    //        }

    //        return type;
    //    }
    //}
}
