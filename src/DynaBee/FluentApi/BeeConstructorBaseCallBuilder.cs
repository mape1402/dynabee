namespace DynaBee.FluentApi
{
    /// <summary>
    /// Fluent builder for selecting generated constructor arguments passed to a base constructor.
    /// </summary>
    public sealed class BeeConstructorBaseCallBuilder
    {
        private readonly List<string> _argumentNames = new();

        /// <summary>
        /// Adds one generated constructor argument by parameter name.
        /// </summary>
        /// <param name="parameterName">Generated constructor parameter name to pass to the base constructor.</param>
        /// <returns>The same base-call builder.</returns>
        public BeeConstructorBaseCallBuilder Argument(string parameterName)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
                throw new ArgumentException(nameof(parameterName));

            _argumentNames.Add(parameterName);
            return this;
        }

        internal IReadOnlyList<string> ArgumentNames => _argumentNames;
    }
}
