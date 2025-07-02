namespace DynaBee
{
    using System.Reflection.Emit;

    /// <summary>
    /// Defines a context for managing dynamic type creation within a specific <see cref="ModuleBuilder"/>.
    /// </summary>
    public interface IAssemblyContextBuilder
    {
        /// <summary>
        /// Gets the underlying <see cref="ModuleBuilder"/> used to define dynamic types.
        /// </summary>
        ModuleBuilder ModuleBuilder { get; }

        /// <summary>
        /// Retrieves a previously registered <see cref="ITypeContextBuilder"/> by its name.
        /// </summary>
        /// <param name="name">The unique name of the type builder to retrieve.</param>
        /// <returns>The corresponding <see cref="ITypeContextBuilder"/>, or null if not found.</returns>
        ITypeContextBuilder GetTypeBuilder(string name);

        /// <summary>
        /// Registers a new <see cref="TypeBuilder"/> in the current context under the specified name.
        /// </summary>
        /// <param name="name">The unique name for the type builder.</param>
        /// <param name="typeBuilder">The <see cref="TypeBuilder"/> to associate with the name.</param>
        /// <returns>The created <see cref="ITypeContextBuilder"/> instance for the registered type.</returns>
        ITypeContextBuilder AddTypeBuilder(string name, TypeBuilder typeBuilder);

        /// <summary>
        /// Finalizes the assembly context construction and returns an immutable <see cref="IAssemblyContext"/>
        /// containing all registered type definitions.
        /// </summary>
        /// <returns>An <see cref="IAssemblyContext"/> instance representing the completed dynamic type context.</returns>
        IAssemblyContext Build();
    }
}
