namespace DynaBee
{
    /// <summary>
    /// Defines a contract for configuring an entire dynamic assembly,
    /// including its types and elements.
    /// </summary>
    public interface IAssemblyConfigurator
    {
        /// <summary>
        /// Adds a type configurator to the assembly configuration.
        /// </summary>
        /// <param name="typeConfigurator">
        /// The <see cref="ITypeConfigurator"/> that defines how to build a specific dynamic type.
        /// </param>
        /// <returns>
        /// The current <see cref="IAssemblyConfigurator"/> instance to allow fluent configuration.
        /// </returns>
        IAssemblyConfigurator AddTypeBuilder(ITypeConfigurator typeConfigurator);

        /// <summary>
        /// Stores metadata for the generated assembly context.
        /// </summary>
        /// <param name="key">Metadata key.</param>
        /// <param name="value">Metadata value.</param>
        /// <returns>The current configurator for fluent chaining.</returns>
        IAssemblyConfigurator WithMetadata(string key, object value);

        /// <summary>
        /// Applies the assembly configuration and returns a builder context
        /// that can be used to generate the final dynamic assembly and its types.
        /// </summary>
        /// <returns>
        /// An <see cref="IAssemblyContextBuilder"/> for building the dynamic assembly and its types.
        /// </returns>
        IAssemblyContextBuilder Configure();
    }
}
