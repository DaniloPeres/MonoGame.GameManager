using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Services
{
    /// <summary>The lifetime of a service registered with a factory.</summary>
    public enum ServiceLifetime
    {
        /// <summary>The factory is called once and the instance is reused.</summary>
        Singleton,

        /// <summary>The factory is called on every resolution.</summary>
        Transient
    }

    /// <summary>
    /// A small service container (Registry pattern): services are registered by interface and can be replaced,
    /// so the game can provide its own implementations (Dependency Inversion).
    /// </summary>
    public sealed class ServiceRegistry
    {
        private readonly Dictionary<Type, Registration> registrations = new Dictionary<Type, Registration>();
        private readonly object syncRoot = new object();

        /// <summary>Registers (or replaces) a service instance.</summary>
        public void Register<TService>(TService instance) where TService : class
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            lock (syncRoot)
                registrations[typeof(TService)] = Registration.ForInstance(instance);
        }

        /// <summary>Registers (or replaces) a service created on demand by a factory.</summary>
        public void Register<TService>(Func<TService> factory, ServiceLifetime lifetime = ServiceLifetime.Singleton) where TService : class
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            lock (syncRoot)
                registrations[typeof(TService)] = Registration.ForFactory(() => factory(), lifetime);
        }

        /// <summary>Registers a service only if the type is not registered yet. Returns true if it was registered.</summary>
        public bool TryRegister<TService>(Func<TService> factory, ServiceLifetime lifetime = ServiceLifetime.Singleton) where TService : class
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            lock (syncRoot)
            {
                if (registrations.ContainsKey(typeof(TService)))
                    return false;
                registrations[typeof(TService)] = Registration.ForFactory(() => factory(), lifetime);
                return true;
            }
        }

        /// <summary>Replaces a service instance (same as <see cref="Register{TService}(TService)"/>).</summary>
        public void Replace<TService>(TService instance) where TService : class => Register(instance);

        /// <summary>Returns a service or throws when it is not registered.</summary>
        public TService Resolve<TService>() where TService : class
        {
            if (TryResolve(out TService service))
                return service;

            throw new InvalidOperationException(
                $"The service '{typeof(TService).Name}' is not registered. Services are available after the ScreenManager is created, or register your own with ServiceProvider.Register.");
        }

        public bool TryResolve<TService>(out TService service) where TService : class
        {
            Registration registration;
            lock (syncRoot)
            {
                if (!registrations.TryGetValue(typeof(TService), out registration))
                {
                    service = null;
                    return false;
                }
            }

            service = (TService)registration.GetInstance();
            return true;
        }

        public bool IsRegistered<TService>() where TService : class
        {
            lock (syncRoot)
                return registrations.ContainsKey(typeof(TService));
        }

        public bool Remove<TService>() where TService : class
        {
            lock (syncRoot)
                return registrations.Remove(typeof(TService));
        }

        /// <summary>Removes every registration.</summary>
        public void Clear()
        {
            lock (syncRoot)
                registrations.Clear();
        }

        private sealed class Registration
        {
            private readonly Func<object> factory;
            private readonly ServiceLifetime lifetime;
            private readonly object syncRoot = new object();
            private object instance;
            private bool hasInstance;

            private Registration(Func<object> factory, ServiceLifetime lifetime, object instance, bool hasInstance)
            {
                this.factory = factory;
                this.lifetime = lifetime;
                this.instance = instance;
                this.hasInstance = hasInstance;
            }

            public static Registration ForInstance(object instance) => new Registration(null, ServiceLifetime.Singleton, instance, true);

            public static Registration ForFactory(Func<object> factory, ServiceLifetime lifetime) => new Registration(factory, lifetime, null, false);

            public object GetInstance()
            {
                if (lifetime == ServiceLifetime.Transient && factory != null)
                    return factory();

                lock (syncRoot)
                {
                    if (!hasInstance)
                    {
                        instance = factory();
                        hasInstance = true;
                    }
                    return instance;
                }
            }
        }
    }
}
