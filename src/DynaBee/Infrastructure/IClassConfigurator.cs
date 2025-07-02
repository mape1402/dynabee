namespace DynaBee.Infrastructure
{
    /// <summary>
    /// Defines a contract for configuring a dynamic class,
    /// including its base type and other metadata.
    /// </summary>
    public interface IClassConfigurator
    {
        /// <summary>
        /// Specifies the parent (base) type that the dynamically generated class should inherit from.
        /// </summary>
        /// <param name="parentType">
        /// The base <see cref="Type"/> to inherit.
        /// </param>
        /// <returns>
        /// The current <see cref="IClassConfigurator"/> instance to allow fluent configuration.
        /// </returns>
        IClassConfigurator WithParentType(Type parentType);
    }
}
