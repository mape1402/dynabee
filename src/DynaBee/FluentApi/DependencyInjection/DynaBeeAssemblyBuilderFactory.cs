namespace DynaBee.FluentApi.DependencyInjection
{
    /// <summary>
    /// Default implementation for creating fluent DynaBee assembly builders.
    /// </summary>
    public sealed class DynaBeeAssemblyBuilderFactory : IDynaBeeAssemblyBuilderFactory
    {
        /// <inheritdoc />
        public IBeeAssemblyBuilder Create(string assemblyName)
            => new BeeAssemblyBuilder(assemblyName);
    }
}
