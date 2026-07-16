namespace DynaBee.FluentApi.Invocation
{
    /// <summary>
    /// Invokes a generated method bound to a specific target instance.
    /// </summary>
    public interface IDynaBeeBoundMethodInvoker
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
        /// Invokes the generated method on the bound target instance.
        /// </summary>
        /// <param name="arguments">Method arguments.</param>
        /// <returns>The method return value, or <c>null</c> for void methods.</returns>
        object Invoke(IReadOnlyList<object> arguments);
    }
}
