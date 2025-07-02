namespace DynaBee
{
    /// <summary>
    /// Defines a contract for configuring a dynamic type and its elements within an assembly context.
    /// </summary>
    public interface ITypeConfigurator
    {
        /// <summary>
        /// Adds an <see cref="IElementConfigurator"/> to the type configuration.
        /// </summary>
        /// <param name="elementConfigurator">
        /// The element configurator that defines how to build a specific element
        /// (such as a property, method, field, or constant).
        /// </param>
        /// <returns>
        /// The current <see cref="ITypeConfigurator"/> instance to allow fluent configuration.
        /// </returns>
        ITypeConfigurator AddElementBuilder(IElementConfigurator elementConfigurator);

        /// <summary>
        /// Applies the type configuration to the specified <see cref="IAssemblyContextBuilder"/>.
        /// This method should define the type and its elements in the dynamic assembly.
        /// </summary>
        /// <param name="assemblyContextBuilder">
        /// The assembly context builder to which the type definition and its elements will be added.
        /// </param>
        void Configure(IAssemblyContextBuilder assemblyContextBuilder);
    }

}
