namespace DynaBee.Tools
{
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides a base implementation for managing and validating named arguments 
    /// with immutable values and validation logic.
    /// </summary>
    internal abstract class BaseArguments
    {
        /// <summary>
        /// Stores a collection of named arguments that implement <see cref="IValidableArgument"/>.
        /// </summary>
        protected Dictionary<string, IValidableArgument> Arguments { get; } = new();

        /// <summary>
        /// Retrieves the value of a strongly typed immutable argument based on the calling property name.
        /// </summary>
        /// <typeparam name="T">The type of the argument value.</typeparam>
        /// <param name="key">The name of the calling member. Automatically supplied by the compiler.</param>
        /// <returns>The value associated with the specified argument.</returns>
        protected T Get<T>([CallerMemberName] string key = null)
            => ((Immutable<T>)Arguments[key]).Value;

        /// <summary>
        /// Sets the value of a strongly typed immutable argument based on the calling property name.
        /// </summary>
        /// <typeparam name="T">The type of the argument value.</typeparam>
        /// <param name="value">The value to set.</param>
        /// <param name="key">The name of the calling member. Automatically supplied by the compiler.</param>
        protected void Set<T>(T value, [CallerMemberName] string key = null)
            => ((Immutable<T>)Arguments[key]).Set(value);

        /// <summary>
        /// Validates all registered arguments and throws an <see cref="ArgumentException"/> 
        /// if any of them are invalid.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if one or more arguments are invalid.</exception>
        public virtual void ValidateAndThrow()
        {
            foreach (var argument in Arguments)
            {
                if (!argument.Value.IsValid())
                    throw new ArgumentException($"Argument '{argument.Key}' is not valid.");
            }
        }
    }
}
