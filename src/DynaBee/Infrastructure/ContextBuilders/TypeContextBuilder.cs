namespace DynaBee.Infrastructure.ContextBuilders
{
    using DynaBee.Infrastructure.Contexts;
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    internal sealed class TypeContextBuilder : ITypeContextBuilder
    {
        private readonly Dictionary<string, IElementContextBuilder> _elementContextBuilders = new();

        public TypeContextBuilder(string name, TypeBuilder typeBuilder, IAssemblyContextBuilder assemblyBuilderContext)
        {
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentNullException(nameof(name)) : name;
            TypeBuilder = typeBuilder ?? throw new ArgumentNullException(nameof(typeBuilder));
            AssemblyBuilderContext = assemblyBuilderContext ?? throw new ArgumentNullException(nameof(assemblyBuilderContext));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IAssemblyContextBuilder AssemblyBuilderContext { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public TypeBuilder TypeBuilder {  get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IElementContextBuilder AddElement(string name, ElementType elementType, ElementBuilderAction buildAction)
        {
            if (_elementContextBuilders.ContainsKey(name))
                throw new InvalidOperationException($"Element with name '{name}' already exists in dynamic type '{TypeBuilder.Name}'.");

            var elementContextBuilder = new ElementContextBuilder(name, elementType, buildAction, this);
            _elementContextBuilders.Add(name, elementContextBuilder);

            return elementContextBuilder;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ITypeContext Build()
        {
            var orderedBuilders = _elementContextBuilders.Values
                .OrderBy(x => x.Name.StartsWith(".ctor", StringComparison.Ordinal) ? 1 : 0)
                .ToArray();

            var elementContexts = orderedBuilders.Select(x => x.Build()).ToArray();
            var clrType = TypeBuilder.CreateTypeInfo()?.AsType() ?? (Type)TypeBuilder;
            return new TypeContext(Name, clrType, elementContexts);
        }
    }
}
