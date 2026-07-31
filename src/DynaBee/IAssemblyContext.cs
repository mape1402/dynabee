namespace DynaBee
{
    using System.Reflection;

    /// <summary>
    /// Represents an immutable context that provides access to dynamically created types within a specific <see cref="Assembly"/>.
    /// </summary>
    public interface IAssemblyContext
    {
        /// <summary>
        /// Gets the underlying <see cref="Assembly"/> that contains all dynamically generated types.
        /// </summary>
        Assembly Assembly { get; }

        /// <summary>
        /// Gets the unique name assigned to this assembly context, which can be used for identification.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets metadata attached to this generated assembly context.
        /// </summary>
        IReadOnlyDictionary<string, object> Metadata { get; }

        /// <summary>
        /// Gets metadata attached to this generated assembly context.
        /// </summary>
        /// <param name="key">Metadata key.</param>
        /// <returns>Metadata value.</returns>
        object GetMetadata(string key);

        /// <summary>
        /// Tries to get metadata attached to this generated assembly context.
        /// </summary>
        /// <param name="key">Metadata key.</param>
        /// <param name="value">Metadata value when present.</param>
        /// <returns><c>true</c> when metadata exists; otherwise <c>false</c>.</returns>
        bool TryGetMetadata(string key, out object value);

        /// <summary>
        /// Tries to get strongly typed metadata attached to this generated assembly context.
        /// </summary>
        /// <typeparam name="T">Metadata value type.</typeparam>
        /// <param name="key">Typed metadata key.</param>
        /// <param name="value">Typed metadata value when present.</param>
        /// <returns><c>true</c> when metadata exists and can be cast to <typeparamref name="T"/>.</returns>
        bool TryGetMetadata<T>(BeeMetadataKey<T> key, out T value);

        /// <summary>
        /// Finds a single <see cref="ITypeContext"/> by its unique name.
        /// </summary>
        /// <param name="name">The unique name of the type to find.</param>
        /// <returns>The matching <see cref="ITypeContext"/>, or null if no match is found.</returns>
        ITypeContext Find(string name);

        /// <summary>
        /// Finds all <see cref="ITypeContext"/> instances that match the specified predicate expression.
        /// </summary>
        /// <param name="predicate">The filter expression used to select matching type contexts.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing all matching <see cref="ITypeContext"/> instances.</returns>
        IEnumerable<ITypeContext> Find(Func<ITypeContext, bool> predicate);
    }
}
