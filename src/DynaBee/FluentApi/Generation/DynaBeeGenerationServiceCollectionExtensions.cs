namespace DynaBee.FluentApi.Generation
{
    using DynaBee.FluentApi.DependencyInjection;
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
    }
}
