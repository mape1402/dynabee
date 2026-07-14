namespace DynaBee.FluentApi.DependencyInjection
{
    /// <summary>
    /// Factory abstraction for creating fluent assembly builders through DI.
    /// </summary>
    public interface IDynaBeeAssemblyBuilderFactory
    {
        /// <summary>
        /// Creates a new fluent assembly builder for the specified logical assembly name.
        /// </summary>
        /// <param name="assemblyName">Logical assembly name.</param>
        /// <returns>A new <see cref="BeeAssemblyBuilder"/> instance.</returns>
        BeeAssemblyBuilder Create(string assemblyName);
    }
}
