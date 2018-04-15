using System;
using System.Collections;
using System.Linq;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Managers.Validation;
using wmsMLC.Business.Managers.Validation.Attributes;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Business
{
    public static class ValidateStrategiesHelper
    {
        public static void Initialize()
        {
            // Добавляем стратегию валидации обязательности при создании
            WMSValidateAttribute.RegisterStrategy("requirement", context =>
            {
                // если значение есть - нечего и проверять
                var isNull = IsNullValueCheck(context.Property.GetValue(context.Obj));
                if (!isNull)
                    return false;

                // читаем параметры
                if (string.IsNullOrEmpty(context.Parameters))
                    throw new DeveloperException("Ожидались параметры валидации, а их нет");

                var pCreate = context.Parameters.ToLower().Contains("create");
                var pEdit = context.Parameters.ToLower().Contains("edit");
                if (!pCreate && !pEdit)
                    throw new DeveloperException("Ошибка в параметрах валидации");

                if (pCreate && pEdit)
                    return true;

                // смотрим, может ли объекты сказать о себе (новый или редактируемый)
                var isNew = context.Obj as IIsNew;
                if (isNew == null)
                    throw new DeveloperException(
                        "Ожидается, что объекты валидации обязательности должны поддерживать интерфейс IIsNew");

                if (pCreate && isNew.IsNew)
                    return true;

                if (pEdit && !isNew.IsNew)
                    return true;

                return false;
            });

            // Добавляем стратегию проверки наличия всех обязательных полей для globalparam
            WMSValidateAttribute.RegisterStrategy("gpv.requirement", context =>
            {
                // вычитываем параметры для данного типа
                var mngGlobalParam = IoC.Instance.Resolve<IGlobalParamManager>();
                var globalParData = mngGlobalParam.GetByEntity(context.Obj.GetType());

                // отбираем обязательные
                var requireGlobalParData = globalParData.Where(p => p.GlobalParamMustSet).ToArray();

                // обязательных нет - ошибки тоже нет
                if (requireGlobalParData.Length == 0)
                    return false;

                // получаем список введенных параметров
                var temp = context.Property.GetValue(context.Obj) as IList;
                var collect = temp?.Cast<GlobalParamValue>().ToArray();

                // бежим по обязательным и смотрим ввели ли уже такие
                var errorList = string.Empty;
                foreach (var p in requireGlobalParData)
                {
                    // если есть - идем дальше
                    if (collect != null && collect.Any(i => i.GlobalParamCode_R.EqIgnoreCase(p.GlobalParamCode)))
                        continue;

                    errorList += string.Format("{0} ({1}){2}", p.GlobalParamName, p.GlobalParamCode, Environment.NewLine);
                }

                if (string.IsNullOrEmpty(errorList))
                    return false;

                context.ErrorMessage += string.Format(":{0}{1}", Environment.NewLine, errorList);
                return true;
            });

            // Добавляем стратегию проверки максимального кол-ва параметров globalparamvalue
            WMSValidateAttribute.RegisterStrategy("gpv.maxcount", context =>
            {
                var temp = context.Property.GetValue(context.Obj) as IList;
                var collect = temp == null ? null : temp.Cast<GlobalParamValue>().ToArray();

                // коллекция пустая - превышать нечего
                if (collect == null || collect.Length == 0)
                    return false;

                // вычитываем параметры для данного типа
                var mngGlobalParam = IoC.Instance.Resolve<IGlobalParamManager>();
                var globalParData = mngGlobalParam.GetByEntity(context.Obj.GetType());

                var errorList = string.Empty;
                foreach (var p in globalParData)
                {
                    var count = collect.Count(i => i.GlobalParamCode_R.EqIgnoreCase(p.GlobalParamCode));
                    if (count > p.GlobalParamCount)
                        errorList += string.Format("{0} ({1}){2}", p.GlobalParamName, p.GlobalParamCode, Environment.NewLine);
                }

                if (string.IsNullOrEmpty(errorList))
                    return false;

                context.ErrorMessage += string.Format(":{0}{1}", Environment.NewLine, errorList);
                return true;
            });

            // Добавляем стратегию проверки максимального кол-ва параметров customparamvalue
            WMSValidateAttribute.RegisterStrategy("cpv.maxcount", context =>
            {
                var temp = context.Property.GetValue(context.Obj) as IList;
                var collect = temp == null ? null : temp.Cast<CustomParamValue>().ToArray();

                // коллекция пустая - превышать нечего
                if (collect == null || collect.Length == 0)
                    return false;

                // вычитываем параметры для данного типа
                var mgrCustomParam = IoC.Instance.Resolve<ICustomParamManager>();
                var customParData = mgrCustomParam.GetByEntity(context.Obj.GetType());

                var errorList = string.Empty;
                foreach (var p in customParData)
                {
                    var pk = p.GetKey().To<string>();
                    var count = collect.Count(i => i.CustomParamCode.EqIgnoreCase(pk));
                    if (count > p.CustomParamCount)
                        errorList += string.Format("{0} ({1}){2}", p.CustomParamName, pk, Environment.NewLine);
                }

                if (errorList.Length == 0)
                    return false;

                context.ErrorMessage += string.Format(":{0}{1}", Environment.NewLine, errorList);
                return true;
            });

            // Добавляем стратегию проверки наличия всех обязательных полей для customparam
            WMSValidateAttribute.RegisterStrategy("cpv.requirement", context =>
            {
                // вычитываем параметры для данного типа
                var mgrCustomParam = IoC.Instance.Resolve<ICustomParamManager>();

                var obj = context.Obj as IHasParent;
                if (obj != null && string.IsNullOrEmpty(obj.SourceTypeName) && string.IsNullOrEmpty(obj.TargetTypeName))
                    return false;

                var customParData = obj == null ? mgrCustomParam.GetByEntity(context.Obj.GetType())
                                                 : mgrCustomParam.GetByEntity(context.Obj.GetType(), obj.SourceTypeName, obj.TargetTypeName);

                // отбираем обязательные
                var requireCustomParData = customParData.Where(p => p.CustomParamMustHave).ToArray();

                // обязательных нет - ошибки тоже нет
                if (requireCustomParData.Length == 0)
                    return false;

                var pCreate = context.Parameters.ToLower().Contains("create");
                var pEdit = context.Parameters.ToLower().Contains("edit");
                if (!pCreate && !pEdit)
                    throw new DeveloperException("Ошибка в параметрах валидации");

                // смотрим, может ли объекты сказать о себе (новый или редактируемый)
                var isNew = context.Obj as IIsNew;
                if (isNew == null)
                    throw new DeveloperException(
                        "Ожидается, что объекты валидации обязательности должны поддерживать интерфейс IIsNew");

                if (isNew.IsNew && pEdit)
                    return false;

                if (!isNew.IsNew && pCreate)
                    return false;

                // получаем список введенных параметров
                var temp = context.Property.GetValue(context.Obj) as IList;
                var collect = temp == null ? null : temp.Cast<CustomParamValue>().ToArray();

                // бежим по обязательным и смотрим ввели ли уже такие
                var errorList = string.Empty;
                foreach (var p in requireCustomParData)
                {
                    var pk = p.GetKey().To<string>();
                    // если есть - идем дальше
                    if (collect != null && collect.Any(i => i.CustomParamCode.EqIgnoreCase(pk)))
                        continue;

                    errorList += string.Format("{0} ({1}){2}", p.CustomParamName, pk, Environment.NewLine);
                }

                if (errorList.Length == 0)
                    return false;

                context.ErrorMessage += string.Format(":{0}{1}", Environment.NewLine, errorList);
                return true;
            });

            // Добавляем стратегию проверки минимального граничного значения
            WMSValidateAttribute.RegisterStrategy("range.min", context =>
            {
                var value = context.Property.GetValue(context.Obj);
                if (value == null)
                    return false;

                var notNullType = context.Property.PropertyType.GetNonNullableType();
                var rangeTypedValue = SerializationHelper.ConvertToTrueType(context.Value, notNullType);
                var comparable = rangeTypedValue as IComparable;
                if (comparable == null)
                    throw new DeveloperException("Type {0} is not implement IComparable", rangeTypedValue);

                return comparable.CompareTo(value) > 0;
            });

            // Добавляем стратегию проверки максимального граничного значения
            WMSValidateAttribute.RegisterStrategy("range.max", context =>
            {
                var value = context.Property.GetValue(context.Obj);
                if (value == null)
                    return false;

                var notNullType = context.Property.PropertyType.GetNonNullableType();
                var rangeTypedValue = SerializationHelper.ConvertToTrueType(context.Value, notNullType);
                var comparable = rangeTypedValue as IComparable;
                if (comparable == null)
                    throw new DeveloperException("Type {0} is not implement IComparable", rangeTypedValue);

                return comparable.CompareTo(value) < 0;
            });

            // Добавляем стратегию проверки наличия значения по умолчанию
            WMSValidateAttribute.RegisterStrategy(DefaultValueSetter.DefaultStrategyName, context =>
            {
                var eo = context.Obj as EditableBusinessObject;
                // если объект не новый, то не надо валидировать
                if (eo != null && !eo.IsNew)
                    return false;
                var value = context.Property.GetValue(context.Obj);
                var errorList = string.Empty;
                if ((value == null) || (value is DateTime && value.Equals(DateTime.MinValue)) ||
                    (value is Guid && value.Equals(Guid.Empty)) ||
                    (value is string && string.IsNullOrEmpty((string)value)))
                {
                    var obj = (BusinessObject)context.Obj;
                    errorList += obj.GetPropertyDefaultValue(context.Property.Name) ?? "(пустое значение)";
                }

                if (string.IsNullOrEmpty(errorList))
                    return false;

                context.ErrorMessage += ": " + errorList;
                return true;
            });
        }

        private static bool IsNullValueCheck(object value)
        {
            if (value == null)
                return true;

            if ((value is DateTime && value.Equals(DateTime.MinValue)) ||
                (value is Guid && value.Equals(Guid.Empty)) ||
                (value is string && string.IsNullOrEmpty((string)value)) ||
                (value is IList && ((IList)value).Count == 0))
                return true;

            return false;
        }
    }
}
