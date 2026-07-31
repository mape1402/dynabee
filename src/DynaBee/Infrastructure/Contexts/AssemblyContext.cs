namespace DynaBee.Infrastructure.Contexts
{
    using System.Reflection;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    internal class AssemblyContext : IAssemblyContext
    {
        private readonly Dictionary<string, ITypeContext> _typeContexts;
        private readonly Dictionary<string, object> _metadata;

        public AssemblyContext(
            string name,
            Assembly assembly,
            IEnumerable<ITypeContext> typeContexts,
            IDictionary<string, object> metadata = null)
        {
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException(nameof(name)) : name;
            Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            _typeContexts = typeContexts == null ? throw new ArgumentNullException(nameof(typeContexts)) : typeContexts.ToDictionary(x => x.Name);
            _metadata = metadata == null ? new Dictionary<string, object>() : new Dictionary<string, object>(metadata);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Assembly Assembly { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IReadOnlyDictionary<string, object> Metadata => _metadata;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public object GetMetadata(string key)
        {
            if (!_metadata.ContainsKey(key))
                throw new KeyNotFoundException($"Metadata with key '{key}' doesn't exist into dynamic assembly '{Name}'.");

            return _metadata[key];
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool TryGetMetadata(string key, out object value)
            => _metadata.TryGetValue(key, out value);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool TryGetMetadata<T>(BeeMetadataKey<T> key, out T value)
        {
            value = default;

            if (string.IsNullOrWhiteSpace(key.Name))
                return false;

            if (!_metadata.TryGetValue(key.Name, out var rawValue) || rawValue is not T typedValue)
                return false;

            value = typedValue;
            return true;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ITypeContext Find(string name)
        {
            if (!_typeContexts.ContainsKey(name))
                throw new KeyNotFoundException($"Type with name '{name}' doesn't exist into dynamic assembly '{Name}'.");

            return _typeContexts[name];
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IEnumerable<ITypeContext> Find(Func<ITypeContext, bool> predicate)
            => _typeContexts.Values.Where(predicate);
    }
}
