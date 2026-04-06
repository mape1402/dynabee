namespace DynaBee
{
    /// <summary>
    /// Represents an immutable context that provides metadata and access to a dynamically generated type
    /// and its defined elements.
    /// </summary>
    public interface ITypeContext
    {
        /// <summary>
        /// Gets the unique name assigned to the dynamic type.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the runtime CLR <see cref="Type"/> that was dynamically created.
        /// </summary>
        Type ClrType { get; }

        /// <summary>
        /// Finds a single element within the type by its unique name.
        /// </summary>
        /// <param name="name">The unique name of the element to find.</param>
        /// <returns>
        /// The matching <see cref="IElementContext"/> if found; otherwise, null.
        /// </returns>
        IElementContext FindOne(string name);

        /// <summary>
        /// Finds all elements within the type that match the specified predicate.
        /// </summary>
        /// <param name="predicate">
        /// A function used to filter elements based on custom conditions.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> containing all matching <see cref="IElementContext"/> instances.
        /// </returns>
        IEnumerable<IElementContext> Find(Func<IElementContext, bool> predicate);

        /// <summary>
        /// Gets metadata attached to this generated type.
        /// </summary>
        /// <param name="key">Metadata key.</param>
        /// <returns>Metadata value.</returns>
        object GetMetadata(string key);

        /// <summary>
        /// Tries to get metadata attached to this generated type.
        /// </summary>
        /// <param name="key">Metadata key.</param>
        /// <param name="value">Metadata value when present.</param>
        /// <returns><c>true</c> when metadata exists; otherwise <c>false</c>.</returns>
        bool TryGetMetadata(string key, out object value);
    }
}
