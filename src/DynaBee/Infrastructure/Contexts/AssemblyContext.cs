namespace DynaBee.Infrastructure.Contexts
{
    using System.Reflection;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    internal class AssemblyContext : IAssemblyContext
    {
        private readonly Dictionary<string, ITypeContext> _typeContexts;

        public AssemblyContext(string name, Assembly assembly, IEnumerable<ITypeContext> typeContexts)
        {
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException(nameof(name)) : name;
            Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            _typeContexts = typeContexts == null ? throw new ArgumentNullException(nameof(typeContexts)) : typeContexts.ToDictionary(x => x.Name);
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
