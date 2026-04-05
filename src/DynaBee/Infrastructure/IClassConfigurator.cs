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

        /// <summary>
        /// Adds an interface that the dynamic class must implement.
        /// </summary>
        /// <param name="interfaceType">Interface type to implement.</param>
        /// <returns>The current configurator for fluent chaining.</returns>
        IClassConfigurator Implements(Type interfaceType);

        /// <summary>
        /// Adds a custom attribute to the dynamic class.
        /// </summary>
        /// <param name="attribute">Attribute descriptor.</param>
        /// <returns>The current configurator for fluent chaining.</returns>
        IClassConfigurator AddAttribute(BeeAttribute attribute);
    }
}
