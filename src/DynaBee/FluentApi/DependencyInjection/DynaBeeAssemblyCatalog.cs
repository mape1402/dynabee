namespace DynaBee.FluentApi.DependencyInjection
{
    /// <summary>
    /// Default in-memory catalog for dynamic assembly registrations.
    /// </summary>
    public sealed class DynaBeeAssemblyCatalog : IDynaBeeAssemblyCatalog
    {
        private readonly Dictionary<string, IAssemblyContextRegistry> _registries;
        private readonly Dictionary<string, IAssemblyContextProvider> _providers;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynaBeeAssemblyCatalog"/> class.
        /// </summary>
        /// <param name="registries">Registry registrations grouped by assembly name.</param>
        /// <param name="providers">Provider registrations grouped by assembly name.</param>
        public DynaBeeAssemblyCatalog(
            IDictionary<string, IAssemblyContextRegistry> registries,
            IDictionary<string, IAssemblyContextProvider> providers)
        {
            if (registries == null)
                throw new ArgumentNullException(nameof(registries));

            if (providers == null)
                throw new ArgumentNullException(nameof(providers));

            _registries = new Dictionary<string, IAssemblyContextRegistry>(registries, StringComparer.Ordinal);
            _providers = new Dictionary<string, IAssemblyContextProvider>(providers, StringComparer.Ordinal);

            if (_registries.Count != _providers.Count || _registries.Keys.Except(_providers.Keys, StringComparer.Ordinal).Any())
                throw new InvalidOperationException("Catalog registries and providers must contain the same assembly names.");
        }

        /// <inheritdoc />
        public IReadOnlyCollection<string> AssemblyNames => _registries.Keys.ToArray();

        /// <inheritdoc />
        public IReadOnlyCollection<IAssemblyContextRegistry> Registries => _registries.Values.ToArray();

        /// <inheritdoc />
        public IReadOnlyCollection<IAssemblyContextProvider> Providers => _providers.Values.ToArray();

        /// <inheritdoc />
        public IAssemblyContextRegistry GetRegistry(string assemblyName)
        {
            if (string.IsNullOrWhiteSpace(assemblyName))
                throw new ArgumentException(nameof(assemblyName));

            if (!_registries.TryGetValue(assemblyName, out var registry))
                throw new KeyNotFoundException($"Dynamic assembly registry '{assemblyName}' was not found.");

            return registry;
        }

        /// <inheritdoc />
        public IAssemblyContextProvider GetProvider(string assemblyName)
        {
            if (string.IsNullOrWhiteSpace(assemblyName))
                throw new ArgumentException(nameof(assemblyName));

            if (!_providers.TryGetValue(assemblyName, out var provider))
                throw new KeyNotFoundException($"Dynamic assembly provider '{assemblyName}' was not found.");

            return provider;
        }

        /// <inheritdoc />
        public IAssemblyContext GetContext(string assemblyName)
            => GetProvider(assemblyName).Current;
    }
}
