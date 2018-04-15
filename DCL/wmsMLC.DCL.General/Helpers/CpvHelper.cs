using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Converters;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.DCL.General.Helpers
{
    public static class CpvHelper
    {
        public static string GetFormattedValue(CustomParamValue cpv)
        {
            if (cpv == null)
                return null;
            var cp = cpv.Cp;
            if (cp == null)
                return null;

            var value = cpv.CPVValue;

            if (!string.IsNullOrEmpty(cp.ObjectlookupCode_R))
                return LookupHelper.GetDispalyMemberValue(cp.ObjectlookupCode_R, value);

            var fieldType = GetValueType(cpv);
            if (fieldType == null)
                return value;
            var type = fieldType.GetNonNullableType();
            var existsFormat = !string.IsNullOrEmpty(cp.CustomParamFormat);

            if (type == typeof (DateTime))
            {
                var dtvalue = new StringToDateTimeConverter().Convert(value, null, GetDateTimeFormat(),
                    Thread.CurrentThread.CurrentCulture) as DateTime?;
                if (dtvalue.HasValue)
                {
                    return existsFormat
                        ? dtvalue.Value.ToString(cp.CustomParamFormat)
                        : dtvalue.Value.ToShortDateString();
                }
                return null;
            }

            if (type.IsPrimitive || type == typeof(decimal))
            {
                var nvalue = new StringToNumericConverter().Convert(value, null, fieldType, Thread.CurrentThread.CurrentCulture);
                if (nvalue != null)
                    return existsFormat ? string.Format("{0}:" + cp.CustomParamFormat, nvalue) : nvalue.ToString();
                return null;
            }

            return value;
        }

        public static string[] GetDateTimeFormat()
        {
            return new[] {SerializationHelper.DefaultDateTimeStringFormat, "d"};
        }

        public static Type GetValueType(CustomParamValue cpv)
        {
            if (cpv == null)
                return null;
            var cp = cpv.Cp;
            if (cp == null)
                return null;

            if (cp.ValueType == null)
            {
                if (string.IsNullOrEmpty(cp.ObjectlookupCode_R))
                {
                    cp.ValueType =
                        IoC.Instance.Resolve<ISysObjectManager>().GetTypeBySysObjectId((int) cp.CustomParamDataType);
                }
                else //Если лукап определяем тип ValueMember
                {
                    var lookupInfo = LookupHelper.GetLookupInfo(cp.ObjectlookupCode_R);
                    var itemType = LookupHelper.GetItemSourceType(lookupInfo);
                    var td = TypeDescriptor.GetProperties(itemType);
                    var vmt = td.Find(lookupInfo.ValueMember, true);
                    if (vmt != null)
                        cp.ValueType = vmt.PropertyType;
                }
                
            }
            return cp.ValueType;
        }

        public static string GetPropertyName(Type type, string propertyName)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName");

            if (!typeof(CustomParamValue).IsAssignableFrom(type))
                throw new DeveloperException("Type 'CustomParamValue' is not assignable from '{0}'.", type);

            var cpv = (CustomParamValue) Activator.CreateInstance(type);
            return cpv.ChangePropertyName(propertyName);
        }

        public static T[] GetChildsCpvByParentCpv<T>(IEnumerable<CustomParamValue> cpvs, CustomParamValue parentcpv, bool addToResultParentcpv) where T : CustomParamValue
        {
            if (parentcpv == null || cpvs == null)
                return new T[0];

            var result = new List<T>();

            foreach (var p in cpvs.Where(p => p.CPVParent == parentcpv.CPVID))
            {
                result.Add((T)p);
                var childs = GetChildsCpvByParentCpv<T>(cpvs, p, false);
                if (childs.Length > 0)
                    result.AddRange(childs);
            }

            // Чтобы Parent был в конце списка
            if (addToResultParentcpv)
                result.Add((T)parentcpv);

            return result.ToArray();
        }
    }
}
