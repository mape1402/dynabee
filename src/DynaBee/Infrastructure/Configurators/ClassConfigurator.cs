namespace DynaBee.Infrastructure.Configurators
{
    using DynaBee.FluentApi.DependencyInjection;
    using DynaBee.Infrastructure;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    internal class ClassConfigurator : ITypeConfigurator, IClassConfigurator
    {
        private readonly List<IElementConfigurator> _elementConfigurator = new();
        private readonly List<Type> _interfaces = new();
        private readonly Dictionary<Type, bool> _interfaceRegistrations = new();
        private readonly Dictionary<Type, bool> _serviceRegistrations = new();
        private readonly List<BeeAttribute> _attributes = new();
        private readonly Dictionary<string, object> _metadata = new();
        private readonly ClassArguments _arguments = new();
        private Type _parentType;
        private bool _registerAsConcrete = true;

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

            var parentType = _parentType ?? typeof(object);
            var typeBuilder = assemblyContextBuilder.ModuleBuilder.DefineType(
                _arguments.Name,
                _arguments.AccessModifier,
                parentType,
                _interfaces.ToArray());

            foreach (var attribute in _attributes)
                typeBuilder.SetCustomAttribute(attribute.Build());

            var typeBuilderContext = assemblyContextBuilder.AddTypeBuilder(_arguments.Name, typeBuilder);
            foreach (var metadata in _metadata)
                typeBuilderContext.SetMetadata(metadata.Key, metadata.Value);

            typeBuilderContext.SetMetadata(BeeDiMetadataKeys.RegisterAsConcrete, _registerAsConcrete);
            typeBuilderContext.SetMetadata(BeeDiMetadataKeys.InterfaceRegistrations, new Dictionary<Type, bool>(_interfaceRegistrations));
            typeBuilderContext.SetMetadata(BeeDiMetadataKeys.ServiceRegistrations, new Dictionary<Type, bool>(_serviceRegistrations));

            foreach (var elementConfigurator in _elementConfigurator)
                elementConfigurator.Configure(typeBuilderContext);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IClassConfigurator WithParentType(Type parentType)
        {
            if (parentType == null)
                throw new ArgumentNullException(nameof(parentType));

            _parentType = parentType;
            return this;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IClassConfigurator Implements(Type interfaceType, bool registerInDi = true)
        {
            if (interfaceType == null)
                throw new ArgumentNullException(nameof(interfaceType));

            if (!interfaceType.IsInterface)
                throw new ArgumentException("The provided type must be an interface.", nameof(interfaceType));

            if (_interfaces.Contains(interfaceType))
            {
                _interfaceRegistrations[interfaceType] = registerInDi;
                return this;
            }

            _interfaces.Add(interfaceType);
            _interfaceRegistrations[interfaceType] = registerInDi;
            return this;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IClassConfigurator RegisterAsConcrete(bool register = true)
        {
            _registerAsConcrete = register;
            return this;
        }

        public IClassConfigurator RegisterAs(Type serviceType, bool registerInDi = true)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            _serviceRegistrations[serviceType] = registerInDi;
            return this;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IClassConfigurator WithMetadata(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));

            _metadata[key] = value ?? throw new ArgumentNullException(nameof(value));
            return this;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IClassConfigurator AddAttribute(BeeAttribute attribute)
        {
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));

            _attributes.Add(attribute);
            return this;
        }
    }
}
