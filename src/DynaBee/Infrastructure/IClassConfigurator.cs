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
        /// <param name="registerInDi">True to register this interface in DI; otherwise false.</param>
        /// <returns>The current configurator for fluent chaining.</returns>
        IClassConfigurator Implements(Type interfaceType, bool registerInDi = true);

        /// <summary>
        /// Sets whether this dynamic class should be registered as its own concrete type in DI.
        /// </summary>
        /// <param name="register">True to register concrete type; false to skip it.</param>
        /// <returns>The current configurator for fluent chaining.</returns>
        IClassConfigurator RegisterAsConcrete(bool register = true);

        /// <summary>
        /// Stores metadata for the generated class context.
        /// </summary>
        /// <param name="key">Metadata key.</param>
        /// <param name="value">Metadata value.</param>
        /// <returns>The current configurator for fluent chaining.</returns>
        IClassConfigurator WithMetadata(string key, object value);

        /// <summary>
        /// Adds a custom attribute to the dynamic class.
        /// </summary>
        /// <param name="attribute">Attribute descriptor.</param>
        /// <returns>The current configurator for fluent chaining.</returns>
        IClassConfigurator AddAttribute(BeeAttribute attribute);
    }
}
