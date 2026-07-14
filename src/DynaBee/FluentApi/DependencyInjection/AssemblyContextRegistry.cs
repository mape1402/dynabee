namespace DynaBee.FluentApi.DependencyInjection
{
    using DynaBee.FluentApi;

    /// <summary>
    /// Default mutable registry that stores dynamic type definitions and produces immutable snapshots on demand.
    /// </summary>
    public sealed class AssemblyContextRegistry : IAssemblyContextRegistry
    {
        private readonly object _sync = new();
        private readonly IDynaBeeAssemblyBuilderFactory _builderFactory;
        private readonly List<Action<IBeeAssemblyBuilder>> _configurations = new();
        private readonly HashSet<Type> _registeredProfileTypes = new();
        private long _revision;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyContextRegistry"/> class.
        /// </summary>
        /// <param name="assemblyName">Logical assembly name for generated snapshots.</param>
        public AssemblyContextRegistry(string assemblyName)
            : this(assemblyName, new DynaBeeAssemblyBuilderFactory())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyContextRegistry"/> class.
        /// </summary>
        /// <param name="assemblyName">Logical assembly name for generated snapshots.</param>
        /// <param name="builderFactory">Factory used to create assembly builders.</param>
        public AssemblyContextRegistry(string assemblyName, IDynaBeeAssemblyBuilderFactory builderFactory)
        {
            AssemblyName = string.IsNullOrWhiteSpace(assemblyName)
                ? throw new ArgumentException(nameof(assemblyName))
                : assemblyName;

            _builderFactory = builderFactory ?? throw new ArgumentNullException(nameof(builderFactory));
        }

        /// <inheritdoc />
        public string AssemblyName { get; }

        /// <inheritdoc />
        public long Revision
        {
            get
            {
                lock (_sync)
                    return _revision;
            }
        }

        /// <inheritdoc />
        public void Configure(Action<IBeeAssemblyBuilder> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            // Every configuration append changes the registry state.
            lock (_sync)
            {
                _configurations.Add(configure);
                _revision++;
            }
        }

        /// <inheritdoc />
        public bool AddProfile(IDynaBeeProfile profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            if (!string.Equals(profile.AssemblyName, AssemblyName, StringComparison.Ordinal))
                throw new InvalidOperationException(
                    $"Profile '{profile.GetType().FullName}' targets assembly '{profile.AssemblyName}', but this registry targets '{AssemblyName}'.");

            lock (_sync)
            {
                var profileType = profile.GetType();
                if (!_registeredProfileTypes.Add(profileType))
                    return false;

                _configurations.Add(profile.Configure);
                _revision++;
                return true;
            }
        }

        /// <inheritdoc />
        public IAssemblyContext BuildSnapshot()
        {
            Action<IBeeAssemblyBuilder>[] snapshot;
            lock (_sync)
                snapshot = _configurations.ToArray();

            var builder = _builderFactory
                .Create(AssemblyName)
                .DisableCache();

            foreach (var configure in snapshot)
                configure(builder);

            return builder.Build();
        }
    }
}
