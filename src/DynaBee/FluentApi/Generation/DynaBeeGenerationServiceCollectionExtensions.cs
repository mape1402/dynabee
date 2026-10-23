namespace DynaBee.FluentApi.Generation
{
    using DynaBee.FluentApi.DependencyInjection;
    using DynaBee.FluentApi.Extensions;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Registers plan-generation services with a dependency injection container.
    /// </summary>
    public static class DynaBeeGenerationServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the plan executor and its in-memory cache.
        /// </summary>
        /// <param name="services">Service collection to update.</param>
        /// <returns>The same service collection.</returns>
        public static IServiceCollection AddDynaBeeGeneration(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSingleton<IDynaBeeAssemblyBuilderFactory, DynaBeeAssemblyBuilderFactory>();
            services.AddSingleton<IDynaBeeGenerationPlanCache, DynaBeeGenerationPlanCache>();
            services.AddSingleton<IDynaBeeGenerationPlanExecutor, DynaBeeGenerationPlanExecutor>();
            return services;
        }

        /// <summary>
        /// Registers a generation extension as a singleton service.
        /// </summary>
        /// <typeparam name="TExtension">Extension implementation type.</typeparam>
        /// <param name="services">Service collection to update.</param>
        /// <returns>The same service collection.</returns>
        public static IServiceCollection AddDynaBeeExtension<TExtension>(this IServiceCollection services)
            where TExtension : class, IDynaBeeGenerationExtension
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSingleton<IDynaBeeGenerationExtension, TExtension>();
            return services;
        }

        /// <summary>
        /// Registers a typed metadata schema as a singleton service.
        /// </summary>
        /// <typeparam name="T">Metadata value type.</typeparam>
        /// <param name="services">Service collection to update.</param>
        /// <param name="schema">Schema instance.</param>
        /// <returns>The same service collection.</returns>
        public static IServiceCollection AddDynaBeeMetadataSchema<T>(this IServiceCollection services, DynaBeeMetadataSchema<T> schema)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (schema == null)
                throw new ArgumentNullException(nameof(schema));

            services.AddSingleton<IDynaBeeMetadataSchema>(schema);
            return services;
        }

        /// <summary>
        /// Discovers and registers concrete generation extensions from assemblies.
        /// </summary>
        /// <param name="services">Service collection to update.</param>
        /// <param name="assemblies">Assemblies to scan.</param>
        /// <returns>The same service collection.</returns>
        public static IServiceCollection AddDynaBeeExtensionsFromAssemblies(this IServiceCollection services, params System.Reflection.Assembly[] assemblies)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            foreach (var assembly in assemblies ?? Array.Empty<System.Reflection.Assembly>())
            {
                if (assembly == null)
                    continue;

                foreach (var type in assembly.GetTypes().Where(x =>
                    !x.IsAbstract &&
                    !x.IsInterface &&
                    typeof(IDynaBeeGenerationExtension).IsAssignableFrom(x)))
                {
                    services.AddSingleton(typeof(IDynaBeeGenerationExtension), type);
                }
            }

            return services;
        }
    }
}
