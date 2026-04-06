namespace DynaBee.FluentApi.DependencyInjection
{
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Extensions to register generated DynaBee types in DI.
    /// </summary>
    public static class DynaBeeServiceCollectionExtensions
    {
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

            var builder = DynaBeeBuilder.CreateAssembly(assemblyName).WithVersion(version);
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

            return services;
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
