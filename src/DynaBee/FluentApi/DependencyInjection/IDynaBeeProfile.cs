namespace DynaBee.FluentApi.DependencyInjection
{
    using DynaBee.FluentApi;

    /// <summary>
    /// Defines a reusable profile that configures a dynamic assembly using the fluent API.
    /// </summary>
    public interface IDynaBeeProfile
    {
        /// <summary>
        /// Gets the logical assembly name that owns all types declared by this profile.
        /// </summary>
        string AssemblyName { get; }

        /// <summary>
        /// Applies profile-specific type configuration to the provided assembly builder.
        /// </summary>
        /// <param name="builder">The assembly builder abstraction to configure.</param>
        void Configure(IBeeAssemblyBuilder builder);
    }
}
