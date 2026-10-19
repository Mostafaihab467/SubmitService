using System;
using System.Collections.Generic;

namespace SubmitServ.Helpers
{
    /// <summary>
    /// Simple service container for dependency injection in WPF application
    /// </summary>
    public static class ServiceContainer
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private static bool _isInitialized = false;

        /// <summary>
        /// Register a service instance
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="service">Service instance</param>
        public static void Register<T>(T service) where T : class
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            _services[typeof(T)] = service;
        }

        /// <summary>
        /// Resolve a service instance
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <returns>Service instance</returns>
        public static T Resolve<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out object service))
            {
                return service as T;
            }
            
            throw new InvalidOperationException($"Service of type {typeof(T).Name} is not registered.");
        }

        /// <summary>
        /// Check if a service is registered
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <returns>True if service is registered</returns>
        public static bool IsRegistered<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Clear all registered services
        /// </summary>
        public static void Clear()
        {
            _services.Clear();
            _isInitialized = false;
        }

        /// <summary>
        /// Initialize default services
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized)
                return;

            // Register default services here if needed
            _isInitialized = true;
        }
    }
}