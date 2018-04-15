using System.ComponentModel;
using System.Linq;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.DCL.Content.ViewModels.ArtMassInput
{
    public static class Mapper
    {
        public static T Map<T>(object source, T dest)
        {
            var sourceProperties = TypeDescriptor.GetProperties(source);
            var destProperties = TypeDescriptor.GetProperties(dest);
            foreach (PropertyDescriptor property in sourceProperties)
            {
                if (property.PropertyType.IsGenericType &&
                    property.PropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(WMSBusinessCollection<>)))
                    continue;

                var prop = destProperties.Cast<PropertyDescriptor>().FirstOrDefault(i => i.Name.EqIgnoreCase(property.Name));
                var value = property.GetValue(source);
                prop.SetValue(dest, value);
            }
            return dest;
        }
    }
}