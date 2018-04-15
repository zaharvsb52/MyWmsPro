using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;

namespace wmsMLC.General.Modules
{
    /// <summary>
    /// A catalog built from a configuration file.
    /// </summary>
    public class ConfigurationModuleCatalog : IModuleCatalog
    {
        private readonly ModuleCatalogItemCollection _items;
        private bool _isLoaded;

        public ConfigurationModuleCatalog()
        {
            _items = new ModuleCatalogItemCollection();
            _items.CollectionChanged += ItemsCollectionChanged;
        }

        public ConfigurationModuleCatalog(IEnumerable<ModuleInfo> modules)
            : this()
        {
            if (modules == null)
                throw new ArgumentNullException("modules");
            foreach (var moduleInfo in modules)
            {
                _items.Add(moduleInfo);
            }
        }

        public IEnumerable<ModuleInfo> Modules
        {
            get { return _items; }
        }

        private bool Validated { get; set; }

        public void Load()
        {
            _isLoaded = true;
            EnsureModulesDiscovered();
        }
        private static string GetFileAbsoluteUri(string filePath)
        {
            var uriBuilder = new UriBuilder
            {
                Host = String.Empty,
                Scheme = Uri.UriSchemeFile,
                Path = Path.GetFullPath(filePath)
            };

            var fileUri = uriBuilder.Uri;
            return fileUri.ToString();
        }

        private void EnsureModulesDiscovered()
        {
            var section = ConfigurationManager.GetSection("modules") as ModulesConfigurationSection;

            if (section != null)
            {
                foreach (ModuleConfigurationElement element in section.Modules)
                {
                    var moduleInfo = new ModuleInfo(element.ModuleName, element.ModuleType)
                    {
                        Ref = GetFileAbsoluteUri(element.AssemblyFile),
                    };
                    AddModule(moduleInfo);
                }
            }
        }

        public void Validate()
        {
            ValidateUniqueModules();

            Validated = true;
        }

        public void AddModule(ModuleInfo moduleInfo)
        {
            _items.Add(moduleInfo);
        }

        public void Initialize()
        {
            if (!_isLoaded)
                Load();

            Validate();
        }

        private void ValidateUniqueModules()
        {
            var moduleNames = Modules.Select(m => m.ModuleName).ToList();

            string duplicateModule = moduleNames.FirstOrDefault(
                m => moduleNames.Count(m2 => m2 == m) > 1);

            if (duplicateModule != null)
                throw new DeveloperException("A duplicated module with name {0} has been found by the loader.", duplicateModule);
        }

        /// <summary>
        /// Ensures that the catalog is validated.
        /// </summary>
        private void EnsureCatalogValidated()
        {
            if (!Validated)
            {
                Validate();
            }
        }

        private void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Validated)
            {
                EnsureCatalogValidated();
            }
        }

        private class ModuleCatalogItemCollection : Collection<ModuleInfo>, INotifyCollectionChanged
        {
            public event NotifyCollectionChangedEventHandler CollectionChanged;

            protected override void InsertItem(int index, ModuleInfo item)
            {
                base.InsertItem(index, item);

                OnNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            }

            private void OnNotifyCollectionChanged(NotifyCollectionChangedEventArgs eventArgs)
            {
                if (CollectionChanged != null)
                {
                    CollectionChanged(this, eventArgs);
                }
            }
        }
    }
}
