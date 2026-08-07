namespace DynaBee.Testing.DependencyInjection;

using DynaBee.FluentApi.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection extensions for DynaBee testing.
/// </summary>
public static class DynaBeeTestingServiceCollectionExtensions
{
    /// <summary>
    /// Registers DynaBee testing services.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddDynaBeeTesting(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services.AddSingleton<IDynaBeeAssemblyBuilderFactory, DynaBeeAssemblyBuilderFactory>();
        services.AddSingleton<IDynaBeeTestGenerator, DynaBeeTestGenerator>();
        return services;
    }

    /// <summary>
    /// Registers generated types from an assembly context in dependency injection.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="assembly">Generated assembly context.</param>
    /// <param name="lifetime">Generated service lifetime.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddGeneratedAssembly(
        this IServiceCollection services,
        IAssemblyContext assembly,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (assembly == null)
            throw new ArgumentNullException(nameof(assembly));

        return services.AddDynaBee(assembly, lifetime);
    }
}
