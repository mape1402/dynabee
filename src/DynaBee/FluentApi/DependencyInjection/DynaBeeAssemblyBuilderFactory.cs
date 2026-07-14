namespace DynaBee.FluentApi.DependencyInjection
{
    /// <summary>
    /// Default implementation for creating fluent DynaBee assembly builders.
    /// </summary>
    public sealed class DynaBeeAssemblyBuilderFactory : IDynaBeeAssemblyBuilderFactory
    {
        /// <inheritdoc />
        public BeeAssemblyBuilder Create(string assemblyName)
            => DynaBeeBuilder.CreateAssembly(assemblyName);
    }
}
