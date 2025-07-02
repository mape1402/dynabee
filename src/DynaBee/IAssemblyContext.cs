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
