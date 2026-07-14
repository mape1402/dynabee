namespace DynaBee.FluentApi.DependencyInjection
{
    using DynaBee.FluentApi;

    /// <summary>
    /// Represents a mutable registry of dynamic type definitions that can produce immutable assembly snapshots.
    /// </summary>
    public interface IAssemblyContextRegistry
    {
        /// <summary>
        /// Gets the logical name used for generated dynamic assemblies.
        /// </summary>
        string AssemblyName { get; }

        /// <summary>
        /// Gets the internal mutable revision number of the registry.
        /// </summary>
        long Revision { get; }

        /// <summary>
        /// Adds an inline configuration action to the registry.
        /// </summary>
        /// <param name="configure">Configuration callback.</param>
        void Configure(Action<IBeeAssemblyBuilder> configure);

        /// <summary>
        /// Adds a reusable profile to the registry.
        /// </summary>
        /// <param name="profile">Profile instance.</param>
        /// <returns><c>true</c> when the profile was added; otherwise <c>false</c> when it was already registered.</returns>
        bool AddProfile(IDynaBeeProfile profile);

        /// <summary>
        /// Builds a new immutable assembly snapshot from all current registry entries.
        /// </summary>
        /// <returns>The built assembly context.</returns>
        IAssemblyContext BuildSnapshot();
    }
}
