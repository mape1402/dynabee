namespace DynaBee
{
    using System.Reflection.Emit;

    /// <summary>
    /// Defines a context that encapsulates metadata and access to a specific dynamic <see cref="TypeBuilder"/>.
    /// </summary>
    public interface ITypeContextBuilder
    {
        /// <summary>
        /// Gets the parent <see cref="IAssemblyContextBuilder"/> that owns this type context.
        /// </summary>
        IAssemblyContextBuilder AssemblyBuilderContext { get; }

        /// <summary>
        /// Gets the unique name assigned to the type being built.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the underlying <see cref="TypeBuilder"/> used to define the dynamic type.
        /// </summary>
        TypeBuilder TypeBuilder { get; }

        /// <summary>
        /// Adds a new element with the specified name and type to the dynamic type definition
        /// by applying the given <see cref="ElementBuilderAction"/> to the underlying <see cref="ITypeContextBuilder"/>.
        /// </summary>
        /// <param name="name">
        /// The unique name of the element to add.
        /// </param>
        /// <param name="elementType">
        /// The kind of element being added (e.g., property, method, field, or constant).
        /// </param>
        /// <param name="buildAction">
        /// The action that defines how to configure or implement the element.
        /// </param>
        /// <param name="metadata">
        /// Optional metadata attached to the element context.
        /// </param>
        /// <returns>
        /// An <see cref="IElementContextBuilder"/> for further configuration of the added element.
        /// </returns>
        IElementContextBuilder AddElement(
            string name,
            ElementType elementType,
            ElementBuilderAction buildAction,
            IReadOnlyDictionary<string, object> metadata = null);

        /// <summary>
        /// Stores metadata in the current type builder context.
        /// </summary>
        /// <param name="key">Metadata key.</param>
        /// <param name="value">Metadata value.</param>
        void SetMetadata(string key, object value);

        /// <summary>
        /// Finalizes the type context construction and returns an immutable <see cref="ITypeContext"/>
        /// representing the completed dynamic type definition.
        /// </summary>
        /// <returns>An <see cref="ITypeContext"/> instance containing the finalized metadata and structure.</returns>
        ITypeContext Build();
    }
}
