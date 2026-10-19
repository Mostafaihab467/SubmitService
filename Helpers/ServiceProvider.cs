using System;
using System.Linq;
using System.Reflection;
using ETA.eReceipt.IntegrationToolkit;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SubmitServ.Helpers
{
    public static class ServiceProvider
    {
        private static IServiceCollection _services = new ServiceCollection()
            .AddLogging()
            .AddHttpClient()
            .AddMemoryCache();
        private static IServiceProvider _provider;
        private static bool _isBuilt = false;
        public static bool IsBuilt => _isBuilt;

        public static void AddScoped<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            if (_isBuilt)
                throw new InvalidOperationException("Services cannot be added after the service provider is built.");

            _services.AddScoped<TService, TImplementation>();
        }

        public static void AddScoped<TService>(Func<TService> factory) where TService : class
        {
            if (_isBuilt)
                throw new InvalidOperationException("Services cannot be added after the service provider is built.");

            _services.AddScoped(_ => factory());
        }

        public static void Build()
        {
            _provider = _services.BuildServiceProvider();
            _isBuilt = true;
        }

        public static T GetService<T>() where T : class
        {
            if (!_isBuilt)
                throw new InvalidOperationException("Service provider must be built before getting services.");

            var service = _provider.GetService<T>();
            if (service != null)
                return service;

            throw new InvalidOperationException($"Service of type {typeof(T).Name} is not registered.");
        }

        public static bool IsRegistered<T>() where T : class
        {
            return _services.Any(s => s.ServiceType == typeof(T));
        }

        public static void AddMediatR(params Assembly[] assemblies)
        {
            if (_isBuilt)
                throw new InvalidOperationException("Services cannot be added after the service provider is built.");

            _services.AddMediatR(assemblies);
        }

        public static void AddToolkit(IConfiguration configuration, ServiceLifetime databaseServiceScope = ServiceLifetime.Scoped)
        {
            if (_isBuilt)
                throw new InvalidOperationException("Services cannot be added after the service provider is built.");

            try
            {
                _services.AddToolkit(configuration, databaseServiceScope);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddToolkit internally failed: {ex}");
                Console.Error.WriteLine($"AddToolkit failed during startup: {ex.Message}");
                Environment.Exit(1);
            }
        }

        public static void ReplaceServiceLifetimeByName(string serviceTypeName, ServiceLifetime newLifetime)
        {
            if (_isBuilt)
                throw new InvalidOperationException("Services cannot be modified after the service provider is built.");

            var descriptor = _services.FirstOrDefault(s => s.ServiceType.Name == serviceTypeName);
            if (descriptor == null)
                return;

            _services.Remove(descriptor);

            if (descriptor.ImplementationType != null)
            {
                _services.Add(new ServiceDescriptor(descriptor.ServiceType, descriptor.ImplementationType, newLifetime));
                return;
            }

            if (descriptor.ImplementationFactory != null)
            {
                _services.Add(new ServiceDescriptor(descriptor.ServiceType, descriptor.ImplementationFactory, newLifetime));
                return;
            }

            if (descriptor.ImplementationInstance != null)
            {
                _services.Add(new ServiceDescriptor(descriptor.ServiceType, descriptor.ImplementationInstance));
            }
        }

        public static void Clear()
        {
            _services = new ServiceCollection();
            _provider = null;
            _isBuilt = false;
        }

        public static IServiceProvider GetServiceProvider()
        {
            if (!_isBuilt)
                throw new InvalidOperationException("Service provider must be built before getting services.");

            return _provider;
        }
    }
}
