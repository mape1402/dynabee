namespace DynaBee.Infrastructure.Contexts
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    internal class TypeContext : ITypeContext
    {
        private readonly Dictionary<string, IElementContext> _elementContexts;

        public TypeContext(string name, Type clrType, IEnumerable<IElementContext> elementContexts)
        {
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException(nameof(name)) : name;
            ClrType = clrType ?? throw new ArgumentNullException(nameof(clrType));
            _elementContexts = elementContexts == null ? throw new ArgumentNullException(nameof(elementContexts)) : elementContexts.ToDictionary(x => x.Name);
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
        public IElementContext FindOne(string name)
        {
            if (!_elementContexts.ContainsKey(name))
                throw new KeyNotFoundException($"Element with name '{name}' doesn't exist into dynamic type '{Name}'.");

            return _elementContexts[name];
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IEnumerable<IElementContext> Find(Func<IElementContext, bool> predicate)
            => _elementContexts.Values.Where(predicate);
    }
}
