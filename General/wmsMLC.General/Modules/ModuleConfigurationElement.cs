using System.Configuration;

namespace wmsMLC.General.Modules
{
    /// <summary>
    /// A configuration element to declare module metadata.
    /// </summary>
    public class ModuleConfigurationElement : ConfigurationElement
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ModuleConfigurationElement"/>.
        /// </summary>
        public ModuleConfigurationElement()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ModuleConfigurationElement"/>.
        /// </summary>
        /// <param name="assemblyFile">The assembly file where the module is located.</param>
        /// <param name="moduleType">The type of the module.</param>
        /// <param name="moduleName">The name of the module.</param>
        public ModuleConfigurationElement(string assemblyFile, string moduleType, string moduleName)
        {
            base["assemblyFile"] = assemblyFile;
            base["moduleType"] = moduleType;
            base["moduleName"] = moduleName;
        }

        /// <summary>
        /// Gets or sets the assembly file.
        /// </summary>
        /// <value>The assembly file.</value>
        [ConfigurationProperty("assemblyFile", IsRequired = true)]
        public string AssemblyFile
        {
            get { return (string)base["assemblyFile"]; }
            set { base["assemblyFile"] = value; }
        }

        /// <summary>
        /// Gets or sets the module type.
        /// </summary>
        /// <value>The module's type.</value>
        [ConfigurationProperty("moduleType", IsRequired = true)]
        public string ModuleType
        {
            get { return (string)base["moduleType"]; }
            set { base["moduleType"] = value; }
        }

        /// <summary>
        /// Gets or sets the module name.
        /// </summary>
        /// <value>The module's name.</value>
        [ConfigurationProperty("moduleName", IsRequired = true)]
        public string ModuleName
        {
            get { return (string)base["moduleName"]; }
            set { base["moduleName"] = value; }
        }
    }
}
