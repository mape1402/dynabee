namespace DynaBee.Infrastructure.ContextBuilders
{
    using DynaBee.Infrastructure.Contexts;
    using System.Reflection.Emit;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    internal sealed class AssemblyContextBuilder : IAssemblyContextBuilder
    {
        private readonly Dictionary<string, ITypeContextBuilder> _typeBuilderContexts = new();
        private readonly Dictionary<string, object> _metadata = new();
        private readonly string _name;
        private readonly AssemblyBuilder _assemblyBuilder;

        public AssemblyContextBuilder(string name, AssemblyBuilder assemblyBuilder, ModuleBuilder moduleBuilder)
        {
            _name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException(nameof(name)) : name;
            _assemblyBuilder = assemblyBuilder ?? throw new ArgumentNullException(nameof(assemblyBuilder));
            ModuleBuilder = moduleBuilder ?? throw new ArgumentNullException(nameof(moduleBuilder));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ModuleBuilder ModuleBuilder { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ITypeContextBuilder AddTypeBuilder(string name, TypeBuilder typeBuilder)
        {
            if (_typeBuilderContexts.ContainsKey(name))
                throw new InvalidOperationException($"Dynamic Type with _name '{name}' already exists.");

            var typeBuilderContext = new TypeContextBuilder(name, typeBuilder, this);

            _typeBuilderContexts.Add(name, typeBuilderContext);

            return typeBuilderContext;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void SetMetadata(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));

            _metadata[key] = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ITypeContextBuilder GetTypeBuilder(string name)
        {
            if (!_typeBuilderContexts.ContainsKey(name))
                throw new InvalidOperationException($"Dynamic Type with _name '{name}' doesn't exist.");

            return _typeBuilderContexts[name];
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IAssemblyContext Build()
        {
            var typeContexts = _typeBuilderContexts.Values.Select(x => x.Build());
            return new AssemblyContext(_name, _assemblyBuilder, typeContexts, _metadata);
        }
    }
}
