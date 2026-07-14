namespace DynaBee.FluentApi.DependencyInjection
{
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Extensions to register generated DynaBee types in DI.
    /// </summary>
    public static class DynaBeeServiceCollectionExtensions
    {
        /// <summary>
        /// Automatically discovers profiles, groups them by assembly name, and registers all generated types in DI.
        /// </summary>
        /// <param name="services">DI service collection.</param>
        /// <param name="lifetime">Service lifetime for generated types.</param>
        /// <param name="assemblies">Assemblies to scan. When omitted, currently loaded app-domain assemblies are scanned.</param>
        /// <returns>The same service collection.</returns>
        public static IServiceCollection AddDynaBeeProfiles(
            this IServiceCollection services,
            ServiceLifetime lifetime = ServiceLifetime.Transient,
            params Assembly[] assemblies)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            var builderFactory = new DynaBeeAssemblyBuilderFactory();
            services.AddSingleton<IDynaBeeAssemblyBuilderFactory>(builderFactory);

            var profiles = (assemblies == null || assemblies.Length == 0)
                ? DynaBeeProfileDiscoveryExtensions.DiscoverProfilesFromCurrentAppDomain()
                : DynaBeeProfileDiscoveryExtensions.DiscoverProfilesFromAssemblies(assemblies);

            var groupedProfiles = profiles
                .GroupBy(x => x.AssemblyName, StringComparer.Ordinal)
                .OrderBy(x => x.Key, StringComparer.Ordinal)
                .ToArray();

            var registries = new Dictionary<string, IAssemblyContextRegistry>(StringComparer.Ordinal);
            var providers = new Dictionary<string, IAssemblyContextProvider>(StringComparer.Ordinal);

            foreach (var group in groupedProfiles)
            {
                var registry = new AssemblyContextRegistry(group.Key, builderFactory);
                foreach (var profile in group)
                    registry.AddProfile(profile);

                var snapshot = registry.BuildSnapshot();
                var provider = new AssemblyContextProvider(registry, snapshot);

                registries.Add(group.Key, registry);
                providers.Add(group.Key, provider);

                services.AddSingleton<IAssemblyContextRegistry>(registry);
                services.AddSingleton<IAssemblyContextProvider>(provider);
                RegisterGeneratedTypes(services, snapshot, lifetime);
            }

            services.AddSingleton<IDynaBeeAssemblyCatalog>(new DynaBeeAssemblyCatalog(registries, providers));

            return services;
        }

        /// <summary>
        /// Registers a mutable registry and a rebuildable assembly context provider for DynaBee.
        /// </summary>
        /// <param name="services">DI service collection.</param>
        /// <param name="assemblyName">Logical dynamic assembly name.</param>
        /// <param name="configureRegistry">Optional registry bootstrap callback.</param>
        /// <param name="lifetime">Service lifetime for generated types.</param>
        /// <returns>The same service collection.</returns>
        public static IServiceCollection AddDynaBeeRegistry(
            this IServiceCollection services,
            string assemblyName,
            Action<IAssemblyContextRegistry> configureRegistry = null,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            var builderFactory = new DynaBeeAssemblyBuilderFactory();
            services.AddSingleton<IDynaBeeAssemblyBuilderFactory>(builderFactory);

            var registry = new AssemblyContextRegistry(assemblyName, builderFactory);
            configureRegistry?.Invoke(registry);
            var initialSnapshot = registry.BuildSnapshot();

            services.AddSingleton<IAssemblyContextRegistry>(registry);
            services.AddSingleton<IAssemblyContextProvider>(_ => new AssemblyContextProvider(registry, initialSnapshot));
            services.AddTransient(sp => sp.GetRequiredService<IAssemblyContextProvider>().Current);

            RegisterGeneratedTypes(services, initialSnapshot, lifetime);

            return services;
        }

        /// <summary>
        /// Builds a dynamic assembly and registers generated concrete types in DI.
        /// </summary>
        public static IServiceCollection AddDynaBee(
            this IServiceCollection services,
            string assemblyName,
            Action<BeeAssemblyBuilder> configure,
            string version = "latest",
            ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var builderFactory = new DynaBeeAssemblyBuilderFactory();
            services.AddSingleton<IDynaBeeAssemblyBuilderFactory>(builderFactory);

            var builder = new BeeAssemblyBuilder(assemblyName).WithVersion(version);
            configure(builder);
            var context = builder.Build();

            return services.AddDynaBee(context, lifetime);
        }

        /// <summary>
        /// Registers generated concrete types from an existing assembly context in DI.
        /// </summary>
        public static IServiceCollection AddDynaBee(
            this IServiceCollection services,
            IAssemblyContext context,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            services.AddSingleton(context);
            RegisterGeneratedTypes(services, context, lifetime);

            return services;
        }

        /// <summary>
        /// Registers generated concrete types from the current provider snapshot in DI.
        /// </summary>
        /// <param name="services">DI service collection.</param>
        /// <param name="provider">Assembly context provider.</param>
        /// <param name="lifetime">Service lifetime for generated registrations.</param>
        /// <returns>The same service collection.</returns>
        public static IServiceCollection AddDynaBee(
            this IServiceCollection services,
            IAssemblyContextProvider provider,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            return services.AddDynaBee(provider.Current, lifetime);
        }

        private static void RegisterGeneratedTypes(
            IServiceCollection services,
            IAssemblyContext context,
            ServiceLifetime lifetime)
        {
            var concreteTypes = context
                .Find(_ => true)
                .Where(x => x.ClrType.IsClass && !x.ClrType.IsAbstract && !x.ClrType.IsGenericTypeDefinition)
                .ToArray();

            foreach (var typeContext in concreteTypes)
            {
                var implementationType = typeContext.ClrType;
                var registerConcreteType = ResolveRegisterConcreteType(typeContext);
                if (registerConcreteType)
                    services.Add(new ServiceDescriptor(implementationType, implementationType, lifetime));

                var interfaceRegistrations = ResolveInterfaceRegistrations(typeContext, implementationType);
                foreach (var serviceType in interfaceRegistrations)
                    services.Add(new ServiceDescriptor(serviceType, implementationType, lifetime));
            }
        }

        private static bool ResolveRegisterConcreteType(ITypeContext typeContext)
        {
            if (typeContext.TryGetMetadata(BeeDiMetadataKeys.RegisterAsConcrete, out var value) && value is bool registerAsConcrete)
                return registerAsConcrete;

            return true;
        }

        private static IEnumerable<Type> ResolveInterfaceRegistrations(ITypeContext typeContext, Type implementationType)
        {
            if (typeContext.TryGetMetadata(BeeDiMetadataKeys.InterfaceRegistrations, out var value) &&
                value is Dictionary<Type, bool> interfaceRegistrations)
            {
                return interfaceRegistrations.Where(x => x.Value).Select(x => x.Key);
            }

            return implementationType.GetInterfaces();
        }
    }
}
