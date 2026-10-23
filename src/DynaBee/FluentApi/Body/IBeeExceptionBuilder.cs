namespace DynaBee.FluentApi.Body
{
    /// <summary>
    /// Configures catch clauses for a generated exception block.
    /// </summary>
    public interface IBeeExceptionBuilder
    {
        /// <summary>
        /// Adds a catch clause for an exception type.
        /// </summary>
        /// <param name="exceptionType">Exception type to catch.</param>
        /// <param name="exceptionName">Generated local variable name for the caught exception.</param>
        /// <param name="body">Catch clause body.</param>
        /// <returns>The same exception builder.</returns>
        IBeeExceptionBuilder Catch(Type exceptionType, string exceptionName, Action<IBeeLocal, IBeeMethodBodyBuilder> body);

        /// <summary>
        /// Adds a typed catch clause.
        /// </summary>
        /// <typeparam name="TException">Exception type to catch.</typeparam>
        /// <param name="exceptionName">Generated local variable name for the caught exception.</param>
        /// <param name="body">Catch clause body.</param>
        /// <returns>The same exception builder.</returns>
        IBeeExceptionBuilder Catch<TException>(string exceptionName, Action<IBeeLocal, IBeeMethodBodyBuilder> body)
            where TException : Exception;
    }
}
