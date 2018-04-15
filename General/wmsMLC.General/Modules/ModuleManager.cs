using System;
using System.Collections.Generic;
using System.Linq;

namespace wmsMLC.General.Modules
{
    /// <summary>
    /// Component responsible for coordinating the modules' type loading and module initialization process. 
    /// </summary>
    public class ModuleManager : IModuleManager, IDisposable
    {
        private readonly IModuleInitializer _moduleInitializer;
        private readonly IModuleCatalog _moduleCatalog;
        private IEnumerable<IModuleTypeLoader> _typeLoaders;
        private HashSet<IModuleTypeLoader> _subscribedToModuleTypeLoaders = new HashSet<IModuleTypeLoader>();
     
        public ModuleManager(IModuleInitializer moduleInitializer, IModuleCatalog moduleCatalog)
        {
            if (moduleInitializer == null)
            {
                throw new ArgumentNullException("moduleInitializer");
            }

            if (moduleCatalog == null)
            {
                throw new ArgumentNullException("moduleCatalog");
            }

            _moduleInitializer = moduleInitializer;
            _moduleCatalog = moduleCatalog;
        }

        /// <summary>
        /// The module catalog specified in the constructor.
        /// </summary>
        protected IModuleCatalog ModuleCatalog
        {
            get { return _moduleCatalog; }
        }

        /// <summary>
        /// Raised when a module is loaded or fails to load.
        /// </summary>
        public event EventHandler<LoadModuleCompletedEventArgs> LoadModuleCompleted;

        private void RaiseLoadModuleCompleted(ModuleInfo moduleInfo, Exception error)
        {
            RaiseLoadModuleCompleted(new LoadModuleCompletedEventArgs(moduleInfo, error));
        }

        private void RaiseLoadModuleCompleted(LoadModuleCompletedEventArgs e)
        {
            var handler = LoadModuleCompleted;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void Run()
        {
            _moduleCatalog.Initialize();
            LoadModuleTypes(_moduleCatalog.Modules);
        }

        public void LoadModule(string moduleName)
        {
            var module = _moduleCatalog.Modules.Where(m => m.ModuleName == moduleName).ToArray();
            if (module == null)
                throw new DeveloperException("Module '{0}' was not found in the catalog.", moduleName);

            if (module.Count() != 1)
                throw new DeveloperException("Module '{0}' was added more than once.", moduleName);

            LoadModuleTypes(module);
        }

        /// <summary>
        /// Checks if the module needs to be retrieved before it's initialized.
        /// </summary>
        /// <param name="moduleInfo">Module that is being checked if needs retrieval.</param>
        /// <returns></returns>
        protected virtual bool ModuleNeedsRetrieval(ModuleInfo moduleInfo)
        {
            if (moduleInfo == null) 
                throw new ArgumentNullException("moduleInfo");

            if (moduleInfo.State == ModuleState.NotStarted)
            {
                // If we can instantiate the type, that means the module's assembly is already loaded into 
                // the AppDomain and we don't need to retrieve it. 
                var isAvailable = Type.GetType(moduleInfo.ModuleType) != null;
                if (isAvailable)
                {
                    moduleInfo.State = ModuleState.ReadyForInitialization;
                }

                return !isAvailable;
            }

            return false;
        }

        private void LoadModuleTypes(IEnumerable<ModuleInfo> moduleInfos)
        {
            if (moduleInfos == null)
            {
                return;
            }

            foreach (var moduleInfo in moduleInfos)
            {
                if (moduleInfo.State == ModuleState.NotStarted)
                {
                    if (ModuleNeedsRetrieval(moduleInfo))
                    {
                        BeginRetrievingModule(moduleInfo);
                    }
                    else
                    {
                        moduleInfo.State = ModuleState.ReadyForInitialization;
                    }
                }
            }

            LoadModulesThatAreReadyForLoad();
        }

        /// <summary>
        /// Loads the modules that are not intialized and have their dependencies loaded.
        /// </summary>
        private void LoadModulesThatAreReadyForLoad()
        {
            var keepLoading = true;
            while (keepLoading)
            {
                keepLoading = false;
                var availableModules = _moduleCatalog.Modules.Where(m => m.State == ModuleState.ReadyForInitialization);

                foreach (var moduleInfo in availableModules)
                {
                    if (moduleInfo.State != ModuleState.Initialized)
                    {
                        moduleInfo.State = ModuleState.Initializing;
                        InitializeModule(moduleInfo);
                        keepLoading = true;
                        break;
                    }
                }
            }
        }

        private void BeginRetrievingModule(ModuleInfo moduleInfo)
        {
            var moduleInfoToLoadType = moduleInfo;
            var moduleTypeLoader = GetTypeLoaderForModule(moduleInfoToLoadType);
            moduleInfoToLoadType.State = ModuleState.LoadingTypes;

            // Delegate += works differently betweem SL and WPF.
            // We only want to subscribe to each instance once.
            if (!_subscribedToModuleTypeLoaders.Contains(moduleTypeLoader))
            {
                moduleTypeLoader.LoadModuleCompleted += IModuleTypeLoader_LoadModuleCompleted;
                _subscribedToModuleTypeLoaders.Add(moduleTypeLoader);
            }

            moduleTypeLoader.LoadModuleType(moduleInfo);
        }

        private void IModuleTypeLoader_LoadModuleCompleted(object sender, LoadModuleCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if ((e.ModuleInfo.State != ModuleState.Initializing) && (e.ModuleInfo.State != ModuleState.Initialized))
                {
                    e.ModuleInfo.State = ModuleState.ReadyForInitialization;
                }

                // This callback may call back on the UI thread, but we are not guaranteeing it.
                // If you were to add a custom retriever that retrieved in the background, you
                // would need to consider dispatching to the UI thread.
                LoadModulesThatAreReadyForLoad();
            }
            else
            {
                RaiseLoadModuleCompleted(e);

                // If the error is not handled then I log it and raise an exception.
                if (!e.IsErrorHandled)
                {
                    HandleModuleTypeLoadingError(e.ModuleInfo, e.Error);
                }
            }
        }
       
        private void HandleModuleTypeLoadingError(ModuleInfo moduleInfo, Exception exception)
        {
            if (moduleInfo == null) 
                throw new ArgumentNullException("moduleInfo");

            throw new DeveloperException(string.Format("{0} for module: '{1}'", exception.Message, moduleInfo.ModuleName), exception);
        }

        private IModuleTypeLoader GetTypeLoaderForModule(ModuleInfo moduleInfo)
        {
            foreach (var typeLoader in ModuleTypeLoaders)
            {
                if (typeLoader.CanLoadModuleType(moduleInfo))
                {
                    return typeLoader;
                }
            }

            throw new DeveloperException("There is currently no moduleTypeLoader in the ModuleManager that can retrieve the specified module '{0}'.", moduleInfo.ModuleName);
        }

        private void InitializeModule(ModuleInfo moduleInfo)
        {
            if (moduleInfo.State == ModuleState.Initializing)
            {
                _moduleInitializer.Initialize(moduleInfo);
                moduleInfo.State = ModuleState.Initialized;
                RaiseLoadModuleCompleted(moduleInfo, null);
            }
        }

        /// <summary>
        /// Returns the list of registered <see cref="IModuleTypeLoader"/> instances that will be 
        /// used to load the types of modules. 
        /// </summary>
        /// <value>The module type loaders.</value>
        public IEnumerable<IModuleTypeLoader> ModuleTypeLoaders
        {
            get
            {
                if (_typeLoaders == null)
                {
                    _typeLoaders = new List<IModuleTypeLoader>
                    {
                        new FileModuleTypeLoader()
                    };
                }

                return _typeLoaders;
            }

            set
            {
                _typeLoaders = value;
            }
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>Calls <see cref="Dispose(bool)"/></remarks>.
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the associated <see cref="IModuleTypeLoader"/>s.
        /// </summary>
        /// <param name="disposing">When <see langword="true"/>, it is being called from the Dispose method.</param>
        protected virtual void Dispose(bool disposing)
        {
            foreach (IModuleTypeLoader typeLoader in ModuleTypeLoaders)
            {
                var disposableTypeLoader = typeLoader as IDisposable;
                if (disposableTypeLoader != null)
                {
                    disposableTypeLoader.Dispose();
                }
            }
        }

        #endregion
    }

    public interface IModuleManager
    {
        void Run();

        void LoadModule(string moduleName);

        event EventHandler<LoadModuleCompletedEventArgs> LoadModuleCompleted;
    }

    public interface IModuleInitializer
    {
        void Initialize(ModuleInfo moduleInfo);
    }
}
