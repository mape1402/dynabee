namespace DynaBee.FluentApi.DependencyInjection
{
    /// <summary>
    /// Thread-safe provider for the current immutable assembly context snapshot.
    /// </summary>
    public sealed class AssemblyContextProvider : IAssemblyContextProvider
    {
        private readonly object _sync = new();
        private readonly IAssemblyContextRegistry _registry;
        private IAssemblyContext _current;
        private long _generation;
        private long _observedRegistryRevision;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyContextProvider"/> class.
        /// </summary>
        /// <param name="registry">Mutable registry used to produce new snapshots.</param>
        public AssemblyContextProvider(IAssemblyContextRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _current = _registry.BuildSnapshot();
            _observedRegistryRevision = _registry.Revision;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyContextProvider"/> class with an existing snapshot.
        /// </summary>
        /// <param name="registry">Mutable registry used to produce new snapshots.</param>
        /// <param name="initialSnapshot">Initial immutable snapshot.</param>
        public AssemblyContextProvider(IAssemblyContextRegistry registry, IAssemblyContext initialSnapshot)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _current = initialSnapshot ?? throw new ArgumentNullException(nameof(initialSnapshot));
            _observedRegistryRevision = _registry.Revision;
        }

        /// <inheritdoc />
        public IAssemblyContext Current
        {
            get
            {
                lock (_sync)
                {
                    if (_observedRegistryRevision != _registry.Revision)
                        RebuildCore();

                    return _current;
                }
            }
        }

        /// <inheritdoc />
        public long Generation
        {
            get
            {
                lock (_sync)
                    return _generation;
            }
        }

        /// <inheritdoc />
        public IAssemblyContext Rebuild()
        {
            lock (_sync)
            {
                return RebuildCore();
            }
        }

        private IAssemblyContext RebuildCore()
        {
            _current = _registry.BuildSnapshot();
            _observedRegistryRevision = _registry.Revision;
            _generation++;
            return _current;
        }
    }
}
