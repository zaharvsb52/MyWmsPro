using System;

namespace wmsMLC.General.Modules
{
    /// <summary>
    /// Defines the metadata that describes a module.
    /// </summary>
    [Serializable]
    public class ModuleInfo
    {
        /// <summary>
        /// Initializes a new empty instance of <see cref="ModuleInfo"/>.
        /// </summary>
        public ModuleInfo()
            : this(null, null)
        {
        }

        public ModuleInfo(string name, string type)
        {
            ModuleName = name;
            ModuleType = type;
        }

        /// <summary>
        /// Gets or sets the name of the module.
        /// </summary>
        /// <value>The name of the module.</value>
        public string ModuleName { get; set; }

        /// <summary>
        /// Gets or sets the module <see cref="Type"/>'s AssemblyQualifiedName.
        /// </summary>
        /// <value>The type of the module.</value>
        public string ModuleType { get; set; }

        /// <summary>
        /// Reference to the location of the module assembly.
        /// <example>The following are examples of valid <see cref="ModuleInfo.Ref"/> values:
        /// file://c:/MyProject/Modules/MyModule.dll for a loose DLL in WPF.
        /// </example>
        /// </summary>
        public string Ref { get; set; }

        /// <summary>
        /// Gets or sets the state of the <see cref="ModuleInfo"/> with regards to the module loading and initialization process.
        /// </summary>
        public ModuleState State { get; set; }
    }

    /// <summary>
    /// Defines the states a <see cref="ModuleInfo"/> can be in, with regards to the module loading and initialization process. 
    /// </summary>
    public enum ModuleState
    {
        /// <summary>
        /// Initial state for <see cref="ModuleInfo"/>s. The <see cref="ModuleInfo"/> is defined, 
        /// but it has not been loaded, retrieved or initialized yet. 
        /// </summary>
        NotStarted,

        /// <summary>
        /// The assembly that contains the type of the module is currently being loaded by an instance of a
        /// <see cref="IModuleTypeLoader"/>. 
        /// </summary>
        LoadingTypes,

        /// <summary>
        /// The assembly that holds the Module is present. This means the type of the <see cref="IModule"/> can be instantiated and initialized. 
        /// </summary>
        ReadyForInitialization,

        /// <summary>
        /// The module is currently Initializing, by the <see cref="IModuleInitializer"/>
        /// </summary>
        Initializing,

        /// <summary>
        /// The module is initialized and ready to be used. 
        /// </summary>
        Initialized
    }
}
