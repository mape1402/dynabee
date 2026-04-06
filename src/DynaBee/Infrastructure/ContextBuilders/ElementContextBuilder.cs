using DynaBee.Infrastructure.Contexts;

namespace DynaBee.Infrastructure.ContextBuilders
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    internal class ElementContextBuilder : IElementContextBuilder
    {
        private readonly ElementBuilderAction _buildAction;
        private readonly Dictionary<string, object> _metadata;

        public ElementContextBuilder(
            string name,
            ElementType elementType,
            ElementBuilderAction buildAction,
            ITypeContextBuilder typeContextBuilder,
            IReadOnlyDictionary<string, object> metadata = null)
        {
            Name = string.IsNullOrEmpty(name) ? throw new ArgumentException(nameof(name)) : name;
            ElementType = elementType;
            _buildAction = buildAction ?? throw new ArgumentNullException(nameof(buildAction));
            TypeContextBuilder = typeContextBuilder ?? throw new ArgumentNullException(nameof(typeContextBuilder));
            _metadata = metadata == null ? new Dictionary<string, object>() : new Dictionary<string, object>(metadata);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ITypeContextBuilder TypeContextBuilder { get; }

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
        public void SetMetadata(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));

            _metadata[key] = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IElementContext Build()
        {
            _buildAction(TypeContextBuilder);
            return new ElementContext(Name, ElementType, _metadata);
        }
    }
}
