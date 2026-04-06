namespace DynaBee.Infrastructure.Contexts
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    internal class ElementContext : IElementContext
    {
        private readonly Dictionary<string, object> _metadata;

        public ElementContext(string name, ElementType elementType, IDictionary<string, object> metadata = null)
        {
            Name = string.IsNullOrEmpty(name) ? throw new ArgumentException(nameof(name)) : name;
            ElementType = elementType;
            _metadata = metadata == null ? new Dictionary<string, object>() : new Dictionary<string, object>(metadata);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ElementType ElementType { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public object GetMetadata(string key)
        {
            if (!_metadata.ContainsKey(key))
                throw new KeyNotFoundException($"Metadata with key '{key}' doesn't exist into dynamic element '{Name}'.");

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
