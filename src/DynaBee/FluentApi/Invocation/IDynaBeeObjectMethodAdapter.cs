namespace DynaBee.FluentApi.Invocation
{
    /// <summary>
    /// Represents an object-based adapter for a generated method.
    /// </summary>
    public interface IDynaBeeObjectMethodAdapter
    {
        /// <summary>
        /// Gets the generated method parameter types accepted by this adapter.
        /// </summary>
        IReadOnlyList<Type> ParameterTypes { get; }

        /// <summary>
        /// Gets the generated method return type.
        /// </summary>
        Type ReturnType { get; }

        /// <summary>
        /// Invokes the generated method with object-based arguments.
        /// </summary>
        /// <param name="arguments">Arguments to pass to the generated method.</param>
        /// <returns>The boxed generated method result, or <see langword="null"/> for void methods.</returns>
        object Invoke(IReadOnlyList<object> arguments);
    }
}
