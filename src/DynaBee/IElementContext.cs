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

        /// <summary>
        /// Gets metadata attached to this element.
        /// </summary>
        /// <param name="key">Metadata key.</param>
        /// <returns>Metadata value.</returns>
        object GetMetadata(string key);

        /// <summary>
        /// Tries to get metadata attached to this element.
        /// </summary>
        /// <param name="key">Metadata key.</param>
        /// <param name="value">Metadata value when present.</param>
        /// <returns><c>true</c> when metadata exists; otherwise <c>false</c>.</returns>
        bool TryGetMetadata(string key, out object value);

        /// <summary>
        /// Tries to get strongly typed metadata attached to this element.
        /// </summary>
        /// <typeparam name="T">Metadata value type.</typeparam>
        /// <param name="key">Typed metadata key.</param>
        /// <param name="value">Typed metadata value when present.</param>
        /// <returns><c>true</c> when metadata exists and can be cast to <typeparamref name="T"/>.</returns>
        bool TryGetMetadata<T>(BeeMetadataKey<T> key, out T value);
    }
}
