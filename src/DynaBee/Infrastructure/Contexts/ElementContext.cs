namespace DynaBee.Infrastructure.Contexts
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    internal class ElementContext : IElementContext
    {

        public ElementContext(string name, ElementType elementType)
        {
            Name = string.IsNullOrEmpty(name) ? throw new ArgumentException(nameof(name)) : name;
            ElementType = elementType;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ElementType ElementType { get; }
    }
}
