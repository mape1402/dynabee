namespace DynaBee.FluentApi.DependencyInjection
{
    using DynaBee.FluentApi;

    /// <summary>
    /// Base class for reusable profile definitions.
    /// </summary>
    public abstract class DynaBeeProfile : IDynaBeeProfile
    {
        /// <summary>
        /// Initializes a new profile and pins it to a single logical assembly name.
        /// </summary>
        /// <param name="assemblyName">The dynamic assembly name that owns this profile.</param>
        protected DynaBeeProfile(string assemblyName)
        {
            AssemblyName = string.IsNullOrWhiteSpace(assemblyName)
                ? throw new ArgumentException(nameof(assemblyName))
                : assemblyName;
        }

        /// <inheritdoc />
        public string AssemblyName { get; }

        /// <summary>
        /// Applies profile-specific type configuration to the provided assembly builder.
        /// </summary>
        /// <param name="builder">The assembly builder abstraction to configure.</param>
        public abstract void Configure(IBeeAssemblyBuilder builder);
    }
}
