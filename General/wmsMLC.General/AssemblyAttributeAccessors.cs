using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace wmsMLC.General
{
    /// <summary>
    /// Класс-helper чтения методанных сборок
    /// </summary>
    public class AssemblyAttributeAccessors
    {
        public static string AssemblyTitle
        {
            get
            {
                // Get all Title attributes on this assembly
                var attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                // If there is at least one Title attribute
                if (attributes.Length > 0)
                {
                    // Select the first one
                    var titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    // If it is not an empty string, return it
                    if (!string.IsNullOrEmpty(titleAttribute.Title))
                        return titleAttribute.Title;
                }
                // If there was no Title attribute, or if the Title attribute was the empty string, return the .exe name
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().CodeBase);
            }
        }
        public static string AssemblyFileVersion
        {
            get
            {
                var attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
                if (attributes.Length > 0)
                    return ((AssemblyFileVersionAttribute)attributes[0]).Version;

                return "0.0.0.0";
            }
        }
        public static Version AssemblyVersion
        {
            get
            {
                return Assembly.GetEntryAssembly().GetName().Version;
            }
        }
        public static string AssemblyDescription
        {
            get
            {
                // Get all Description attributes on this assembly
                var attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                // If there aren't any Description attributes, return an empty string
                if (attributes.Length == 0)
                    return string.Empty;
                // If there is a Description attribute, return its value
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }
        public static string AssemblyProduct
        {
            get
            {
                // Get all Product attributes on this assembly
                var attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                // If there aren't any Product attributes, return an empty string
                if (attributes.Length == 0)
                    return string.Empty;
                // If there is a Product attribute, return its value
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }
        public static string AssemblyCopyright
        {
            get
            {
                // Get all Copyright attributes on this assembly
                var attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                // If there aren't any Copyright attributes, return an empty string
                if (attributes.Length == 0)
                    return string.Empty;
                // If there is a Copyright attribute, return its value
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }
        public static string AssemblyCompany
        {
            get
            {
                // Get all Company attributes on this assembly
                var attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                // If there aren't any Company attributes, return an empty string
                if (attributes.Length == 0)
                    return string.Empty;
                // If there is a Company attribute, return its value
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        public static string GetAssemblyFileVersion(Assembly a)
        {
            if (a == null)
                return string.Empty;

            var attributes = a.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);

            return attributes.Length == 0
                ? string.Empty
                : ((AssemblyFileVersionAttribute)attributes[0]).Version;
        }

        public static string AssemblyAll
        {
            get
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                var nullVersion = new Version(0, 0, 0, 0);
                var result = new List<string>();

                foreach (var a in assemblies.Where(a => !a.GetName().Version.Equals(nullVersion)))
                {
                    var name = a.GetName().Name.ToUpper();
                    if (name.IndexOf("DEVEXPRESS.", StringComparison.InvariantCultureIgnoreCase) > -1 ||
                        name.IndexOf("WMS", StringComparison.InvariantCultureIgnoreCase) > -1)
                    {
                        result.Add(string.Format("{0} - v.{1}/v.{2}", a.GetName().Name, GetAssemblyFileVersion(a),
                            a.GetName().Version));
                    }
                }

                if (!result.Any())
                    return string.Empty;

                return string.Join(Environment.NewLine, result.OrderBy(p => p));
            }
        }

    }
}
