namespace DynaBee.Infrastructure.Configurators
{
    using DynaBee.Infrastructure;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    internal class ClassConfigurator : ITypeConfigurator, IClassConfigurator
    {
        private readonly List<IElementConfigurator> _elementConfigurator = new();
        private readonly ClassArguments _arguments = new();

        public ClassConfigurator(string name, ClassAccessModifier accessModifier)
        {
            _arguments.Name = name;
            _arguments.AccessModifier = accessModifier;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ITypeConfigurator AddElementBuilder(IElementConfigurator elementConfigurator)
        {
            if (elementConfigurator == null)
                throw new ArgumentNullException(nameof(elementConfigurator));

            _elementConfigurator.Add(elementConfigurator);
            return this;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Configure(IAssemblyContextBuilder assemblyContextBuilder)
        {
            _arguments.ValidateAndThrow();

            var typeBuilder = assemblyContextBuilder.ModuleBuilder.DefineType(_arguments.Name, _arguments.AccessModifier);
            var typeBuilderContext = assemblyContextBuilder.AddTypeBuilder(_arguments.Name, typeBuilder);

            foreach (var elementConfigurator in _elementConfigurator)
                elementConfigurator.Configure(typeBuilderContext);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IClassConfigurator WithParentType(Type parentType)
        {
            _arguments.ParentType = parentType;
            return this;
        }
    }
}
