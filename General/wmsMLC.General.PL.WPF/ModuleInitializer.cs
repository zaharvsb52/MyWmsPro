using System;
using Microsoft.Practices.Unity;
using wmsMLC.General.Modules;

namespace wmsMLC.General.PL.WPF
{
    /// <summary>
    /// Implements the <see cref="IModuleInitializer"/> interface. Handles loading of a module based on a type.
    /// </summary>
    public class ModuleInitializer : IModuleInitializer
    {
        private readonly IUnityContainer _serviceLocator;

        /// <summary>
        /// Initializes a new instance of <see cref="ModuleInitializer"/>.
        /// </summary>
        /// <param name="serviceLocator">The container that will be used to resolve the modules by specifying its type.</param>
        public ModuleInitializer(IUnityContainer serviceLocator)
        {
            if (serviceLocator == null)
            {
                throw new ArgumentNullException("serviceLocator");
            }

            _serviceLocator = serviceLocator;
        }

        /// <summary>
        /// Initializes the specified module.
        /// </summary>
        /// <param name="moduleInfo">The module to initialize</param>
        public void Initialize(ModuleInfo moduleInfo)
        {
            if (moduleInfo == null) 
                throw new ArgumentNullException("moduleInfo");

            IRunableModule moduleInstance = null;

            try
            {
                moduleInstance = CreateModule(moduleInfo);
                moduleInstance.Initialize();
            }
            catch (Exception ex)
            {
                HandleModuleInitializationError(
                    moduleInfo,
                    moduleInstance != null ? moduleInstance.GetType().Assembly.FullName : null,
                    ex);
            }
        }
       
        public virtual void HandleModuleInitializationError(ModuleInfo moduleInfo, string assemblyName, Exception exception)
        {
            if (moduleInfo == null) 
                throw new ArgumentNullException("moduleInfo");
            if (exception == null) 
                throw new ArgumentNullException("exception");

            var moduleException = !string.IsNullOrEmpty(assemblyName)
                ? new DeveloperException(string.Format("{0} for module: '{1}' in assembly: '{2}'", exception.Message, moduleInfo.ModuleName,
                    assemblyName), exception)
                : new DeveloperException(string.Format("{0} for module: '{1}'", exception.Message, moduleInfo.ModuleName), exception);

            throw moduleException;
        }

        /// <summary>
        /// Uses the container to resolve a new <see cref="IRunableModule"/> by specifying its <see cref="Type"/>.
        /// </summary>
        /// <param name="moduleInfo">The module to create.</param>
        /// <returns>A new instance of the module specified by <paramref name="moduleInfo"/>.</returns>
        protected virtual IRunableModule CreateModule(ModuleInfo moduleInfo)
        {
            if (moduleInfo == null) 
                throw new ArgumentNullException("moduleInfo");
            return CreateModule(moduleInfo.ModuleType);
        }

        /// <summary>
        /// Uses the container to resolve a new <see cref="IRunableModule"/> by specifying its <see cref="Type"/>.
        /// </summary>
        /// <param name="typeName">The type name to resolve. This type must implement <see cref="IRunableModule"/>.</param>
        /// <returns>A new instance of <paramref name="typeName"/>.</returns>
        protected virtual IRunableModule CreateModule(string typeName)
        {
            var moduleType = Type.GetType(typeName);
            if (moduleType == null)
                throw new DeveloperException("Unable to locate the module with type '{0}' among the exported modules. Make sure the module name in the module catalog matches that specified on ModuleExportAttribute for the module type.", typeName);

            return (IRunableModule) _serviceLocator.Resolve(moduleType);
        }
    }
}
