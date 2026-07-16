namespace DynaBee.FluentApi.Invocation
{
    /// <summary>
    /// Invokes a generated method without using reflection invocation in the hot path.
    /// </summary>
    public interface IDynaBeeMethodInvoker
    {
        /// <summary>
        /// Gets the generated method return type.
        /// </summary>
        Type ReturnType { get; }

        /// <summary>
        /// Gets the generated method parameter types.
        /// </summary>
        IReadOnlyList<Type> ParameterTypes { get; }

        /// <summary>
        /// Invokes the generated method on the specified instance.
        /// </summary>
        /// <param name="instance">Target object instance.</param>
        /// <param name="arguments">Method arguments.</param>
        /// <returns>The method return value, or <c>null</c> for void methods.</returns>
        object Invoke(object instance, IReadOnlyList<object> arguments);
    }
}
