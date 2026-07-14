namespace DynaBee.FluentApi.DependencyInjection
{
    /// <summary>
    /// Provides access to dynamic assembly registrations grouped by logical assembly name.
    /// </summary>
    public interface IDynaBeeAssemblyCatalog
    {
        /// <summary>
        /// Gets all registered dynamic assembly names.
        /// </summary>
        IReadOnlyCollection<string> AssemblyNames { get; }

        /// <summary>
        /// Gets all registry registrations.
        /// </summary>
        IReadOnlyCollection<IAssemblyContextRegistry> Registries { get; }

        /// <summary>
        /// Gets all provider registrations.
        /// </summary>
        IReadOnlyCollection<IAssemblyContextProvider> Providers { get; }

        /// <summary>
        /// Resolves a registry by assembly name.
        /// </summary>
        IAssemblyContextRegistry GetRegistry(string assemblyName);

        /// <summary>
        /// Resolves a provider by assembly name.
        /// </summary>
        IAssemblyContextProvider GetProvider(string assemblyName);

        /// <summary>
        /// Resolves the current context snapshot by assembly name.
        /// </summary>
        IAssemblyContext GetContext(string assemblyName);
    }
}
