using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

namespace wmsMLC.General
{
    /// <summary>
    /// Proxy нужен, чтобы скрыть внутренности Unity
    /// (чтобы не нужно было везде тащить using Microsoft.Practices.Unity)
    /// </summary>
    public sealed class IoC
    {
        #region .  Singleton  .

        // ReSharper disable once InconsistentNaming
        private static readonly Lazy<IoC> _instance = new Lazy<IoC>(() => new IoC());

        private IoC() { }

        public static IoC Instance
        {
            get { return _instance.Value; }
        }

        #endregion

        private IUnityContainer _internalContainer;
        private IUnityContainer InternalContainer
        {
            get
            {
                if (_internalContainer == null)
                    throw new DeveloperException("IoC was not configure");
                return _internalContainer;
            }
        }

        public void Configure(IoCConfigurationContext context)
        {
            if (context == null)
            {
                _internalContainer = new UnityContainer();
                return;
            }

            _internalContainer = context.ExternalContainer ?? new UnityContainer();

            if (!string.IsNullOrEmpty(context.ConfigFileName))
            {
                var map = new ExeConfigurationFileMap {ExeConfigFilename = context.ConfigFileName};
                var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
                var section = (UnityConfigurationSection)config.GetSection("unity");
                section.Configure(InternalContainer);
            }
        }

        public T Resolve<T>()
        {
            return InternalContainer.Resolve<T>();
        }
        public T Resolve<T>(string name)
        {
            return InternalContainer.Resolve<T>(name);
        }
        public object Resolve(Type t)
        {
            return InternalContainer.Resolve(t);
        }
        public bool TryResolve(Type t, out object resolved)
        {
            resolved = null;
            try
            {
                resolved = Resolve(t);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool TryResolve<T>(out T resolved) where T : class
        {
            resolved = null;
            try
            {
                resolved = Resolve<T>();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public object Resolve(Type t, string name)
        {
            return InternalContainer.Resolve(t, name);
        }

        public Type ResolveType<T>()
        {
            return ResolveType(typeof(T));
        }

        public Type ResolveType(Type type)
        {
            var regType = InternalContainer.Registrations.FirstOrDefault(i => i.RegisteredType == type);
            if (regType == null)
                return null;
            return regType.MappedToType;
        }

        public void Register<TFrom, TTo>(LifeTime lifeTime = LifeTime.None) where TTo : TFrom
        {
            Register(typeof(TFrom), typeof(TTo), lifeTime);
        }
        public void Register<TFrom, TTo>(string name) where TTo : TFrom
        {
            InternalContainer.RegisterType<TFrom, TTo>(name);
        }      
        public void Register(Type typeFrom, Type typeTo, LifeTime lifeTime = LifeTime.None)
        {
            var lifeTimeMgr = GetLifetimeManager(lifeTime);
            if (lifeTimeMgr == null)
                InternalContainer.RegisterType(typeFrom, typeTo);
            else
                InternalContainer.RegisterType(typeFrom, typeTo, lifeTimeMgr);
        }
        public void Register(Type typeFrom, LifeTime lifeTime = LifeTime.None, params object[] injectionMembers)
        {
            var lifeTimeMgr = GetLifetimeManager(lifeTime);
            var ims = injectionMembers?.Cast<InjectionMember>().ToArray();
            InternalContainer.RegisterType(typeFrom, lifeTimeMgr, ims);
        }
        public void Register<TFrom>(LifeTime lifeTime = LifeTime.None, params object[] injectionMembers)
        {
            Register(typeof(TFrom), lifeTime, injectionMembers);
        }
        public void RegisterInstance(Type typeFrom, object instance, LifeTime lifeTime = LifeTime.None, string name = null)
        {
            var lifeTimeMgr = GetLifetimeManager(lifeTime);
            //if (lifeTimeMgr == null)
            //    _internalContainer.RegisterInstance(typeFrom, instance);
            //else
            //    _internalContainer.RegisterInstance(typeFrom, instance, lifeTimeMgr);

            _internalContainer.RegisterInstance(typeFrom, name, instance, lifeTimeMgr);
        }

        private static LifetimeManager GetLifetimeManager(LifeTime lifeTime)
        {
            switch (lifeTime)
            {
                case LifeTime.None:
                    return null;

                case LifeTime.PerThread:
                    return new PerThreadLifetimeManager();

                case LifeTime.Singleton:
                    return new ContainerControlledLifetimeManager();

                default:
                    throw new DeveloperException(string.Format("Unknown enum value {0} for {1}", lifeTime, typeof(LifeTime).Name));
            }
        }
    }

    public class IoCConfigurationContext
    {
        public IUnityContainer ExternalContainer { get; set; }
        public string ConfigFileName { get; set; }
    }

    public enum LifeTime
    {
        /// <summary> Один на запрос </summary>
        None,

        /// <summary> Один на поток </summary>
        PerThread,

        /// <summary> Один на приложение </summary>
        Singleton,
    }
}