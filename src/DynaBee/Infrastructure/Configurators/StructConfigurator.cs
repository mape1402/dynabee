namespace DynaBee.Infrastructure.Configurators
{
    using System.Reflection;

    internal sealed class StructConfigurator : ITypeConfigurator
    {
        private readonly List<IElementConfigurator> _elementConfigurators = new();
        private readonly List<Type> _interfaces = new();
        private readonly List<BeeAttribute> _attributes = new();
        private readonly string _name;
        private readonly ClassAccessModifier _accessModifier;

        public StructConfigurator(string name, ClassAccessModifier accessModifier)
        {
            _name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException(nameof(name)) : name;
            _accessModifier = accessModifier;
        }

        public ITypeConfigurator AddElementBuilder(IElementConfigurator elementConfigurator)
        {
            if (elementConfigurator == null)
                throw new ArgumentNullException(nameof(elementConfigurator));

            _elementConfigurators.Add(elementConfigurator);
            return this;
        }

        public StructConfigurator Implements(Type interfaceType)
        {
            if (interfaceType == null)
                throw new ArgumentNullException(nameof(interfaceType));

            if (!interfaceType.IsInterface)
                throw new ArgumentException("The provided type must be an interface.", nameof(interfaceType));

            if (!_interfaces.Contains(interfaceType))
                _interfaces.Add(interfaceType);

            return this;
        }

        public StructConfigurator AddAttribute(BeeAttribute attribute)
        {
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));

            _attributes.Add(attribute);
            return this;
        }

        public void Configure(IAssemblyContextBuilder assemblyContextBuilder)
        {
            var access = _accessModifier.IsDefault ? ClassAccessModifier.Public : _accessModifier;
            var typeAttributes = (TypeAttributes)access
                | TypeAttributes.Sealed
                | TypeAttributes.SequentialLayout
                | TypeAttributes.AnsiClass
                | TypeAttributes.BeforeFieldInit;

            var typeBuilder = assemblyContextBuilder.ModuleBuilder.DefineType(
                _name,
                typeAttributes,
                typeof(ValueType),
                _interfaces.ToArray());

            foreach (var attribute in _attributes)
                typeBuilder.SetCustomAttribute(attribute.Build());

            var typeBuilderContext = assemblyContextBuilder.AddTypeBuilder(_name, typeBuilder);

            foreach (var elementConfigurator in _elementConfigurators)
                elementConfigurator.Configure(typeBuilderContext);
        }
    }
}