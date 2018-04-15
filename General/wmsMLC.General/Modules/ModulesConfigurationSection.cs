using System.Configuration;

namespace wmsMLC.General.Modules
{
    /// <summary>
    /// A <see cref="ConfigurationSection"/> for module configuration.
    /// </summary>
    public class ModulesConfigurationSection : ConfigurationSection
    {
        /// <summary>
        /// Gets or sets the collection of modules configuration.
        /// </summary>
        /// <value>A <seealso cref="ModuleConfigurationElementCollection"/> of <seealso cref="ModuleConfigurationElement"/>.</value>
        [ConfigurationProperty("", IsDefaultCollection = true, IsKey = false)]
        public ModuleConfigurationElementCollection Modules
        {
            get { return (ModuleConfigurationElementCollection)base[""]; }
            set { base[""] = value; }
        }
    }
}
