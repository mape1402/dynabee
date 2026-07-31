namespace DynaBee.Infrastructure.Contexts
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    internal class TypeContext : ITypeContext
    {
        private readonly IReadOnlyList<IElementContext> _elementContexts;
        private readonly Dictionary<string, object> _metadata;

        public TypeContext(
            string name,
            Type clrType,
            IEnumerable<IElementContext> elementContexts,
            IDictionary<string, object> metadata = null)
        {
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException(nameof(name)) : name;
            ClrType = clrType ?? throw new ArgumentNullException(nameof(clrType));
            _elementContexts = elementContexts == null ? throw new ArgumentNullException(nameof(elementContexts)) : elementContexts.ToArray();
            _metadata = metadata == null ? new Dictionary<string, object>() : new Dictionary<string, object>(metadata);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Type ClrType { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IReadOnlyDictionary<string, object> Metadata => _metadata;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IElementContext FindOne(string name)
        {
            var matches = _elementContexts.Where(x => string.Equals(x.Name, name, StringComparison.Ordinal)).ToArray();

            if (matches.Length == 0)
                throw new KeyNotFoundException($"Element with name '{name}' doesn't exist into dynamic type '{Name}'.");

            if (matches.Length > 1)
                throw new InvalidOperationException($"More than one element with name '{name}' exists into dynamic type '{Name}'.");

            return matches[0];
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IEnumerable<IElementContext> Find(Func<IElementContext, bool> predicate)
            => _elementContexts.Where(predicate);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public object GetMetadata(string key)
        {
            if (!_metadata.ContainsKey(key))
                throw new KeyNotFoundException($"Metadata with key '{key}' doesn't exist into dynamic type '{Name}'.");

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
    }
}
