using DynaBee.Infrastructure.Contexts;

namespace DynaBee.Infrastructure.ContextBuilders
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    internal class ElementContextBuilder : IElementContextBuilder
    {
        private readonly ElementBuilderAction _buildAction;

        public ElementContextBuilder(string name, ElementType elementType, ElementBuilderAction buildAction, ITypeContextBuilder typeContextBuilder)
        {
            Name = string.IsNullOrEmpty(name) ? throw new ArgumentException(nameof(name)) : name;
            ElementType = elementType;
            _buildAction = buildAction ?? throw new ArgumentNullException(nameof(buildAction));
            TypeContextBuilder = typeContextBuilder ?? throw new ArgumentNullException(nameof(typeContextBuilder));
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
        public IElementContext Build()
        {
            //TODO: Get Element Metadata
            _buildAction(TypeContextBuilder);
            return new ElementContext(Name, ElementType);
        }
    }
}
