namespace DynaBee
{
    /// <summary>
    /// Defines a builder context for configuring a specific element (such as a property, method, field, or constant)
    /// within a dynamic type.
    /// </summary>
    public interface IElementContextBuilder
    {
        /// <summary>
        /// Gets the parent <see cref="ITypeContextBuilder"/> that owns this element context.
        /// </summary>
        ITypeContextBuilder TypeContextBuilder { get; }

        /// <summary>
        /// Gets the unique name assigned to the element being built.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the type of element being built (e.g., property, method, field, or constant).
        /// </summary>
        ElementType ElementType { get; }

        /// <summary>
        /// Finalizes the element context construction and returns an immutable <see cref="IElementContext"/>
        /// representing the completed element definition.
        /// </summary>
        /// <returns>An <see cref="IElementContext"/> instance containing the finalized metadata for the element.</returns>
        IElementContext Build();
    }
}
