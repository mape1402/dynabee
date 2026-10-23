namespace DynaBee.FluentApi.Body
{
    /// <summary>
    /// Configures branches for a generated switch statement.
    /// </summary>
    public interface IBeeSwitchBuilder
    {
        /// <summary>
        /// Adds a branch for a constant case value.
        /// </summary>
        /// <param name="value">Case constant.</param>
        /// <param name="body">Case body.</param>
        /// <returns>The same switch builder.</returns>
        IBeeSwitchBuilder Case(object value, Action<IBeeMethodBodyBuilder> body);

        /// <summary>
        /// Adds a typed branch for a constant case value.
        /// </summary>
        /// <typeparam name="T">Case constant type.</typeparam>
        /// <param name="value">Case constant.</param>
        /// <param name="body">Case body.</param>
        /// <returns>The same switch builder.</returns>
        IBeeSwitchBuilder Case<T>(T value, Action<IBeeMethodBodyBuilder> body);

        /// <summary>
        /// Adds the fallback branch.
        /// </summary>
        /// <param name="body">Fallback body.</param>
        /// <returns>The same switch builder.</returns>
        IBeeSwitchBuilder Default(Action<IBeeMethodBodyBuilder> body);
    }
}
