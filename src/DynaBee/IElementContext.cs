namespace DynaBee
{
    /// <summary>
    /// Represents an immutable context containing metadata for a specific element
    /// (such as a property, method, field, or constant) within a dynamically generated type.
    /// </summary>
    public interface IElementContext
    {
        /// <summary>
        /// Gets the unique name assigned to the element.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the type of element (e.g., property, method, field, or constant).
        /// </summary>
        ElementType ElementType { get; }
    }
}
