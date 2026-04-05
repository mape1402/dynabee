namespace DynaBee.Infrastructure.Configurators
{
    using System.Reflection;

    internal sealed class InterfaceMethodConfigurator : IElementConfigurator
    {
        private readonly string _name;
        private readonly BeeType _returnType;
        private readonly IReadOnlyList<(string Name, BeeType Type)> _parameters;
        private readonly MethodAccessModifier _accessModifier;
        private readonly IReadOnlyCollection<BeeAttribute> _attributes;

        public InterfaceMethodConfigurator(
            string name,
            BeeType returnType,
            IReadOnlyList<(string Name, BeeType Type)> parameters,
            MethodAccessModifier accessModifier,
            IReadOnlyCollection<BeeAttribute> attributes)
        {
            _name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException(nameof(name)) : name;
            _returnType = returnType;
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            _accessModifier = accessModifier;
            _attributes = attributes ?? Array.Empty<BeeAttribute>();
        }

        public void Configure(ITypeContextBuilder typeContextBuilder)
        {
            typeContextBuilder.AddElement(_name, ElementType.Method, _ => BuildAction(typeContextBuilder));
        }

        private void BuildAction(ITypeContextBuilder typeContextBuilder)
        {
            var returnType = ResolveType(_returnType, typeContextBuilder);
            var parameterTypes = _parameters.Select(x => ResolveType(x.Type, typeContextBuilder)).ToArray();

            var access = _accessModifier.IsDefault ? MethodAccessModifier.Public : _accessModifier;
            var attributes = access.Attributes | MethodAttributes.Abstract | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot;

            var methodBuilder = typeContextBuilder.TypeBuilder.DefineMethod(_name, attributes, returnType, parameterTypes);
            for (var i = 0; i < _parameters.Count; i++)
                methodBuilder.DefineParameter(i + 1, ParameterAttributes.None, _parameters[i].Name);

            foreach (var attribute in _attributes)
                methodBuilder.SetCustomAttribute(attribute.Build());
        }

        private static Type ResolveType(BeeType beeType, ITypeContextBuilder typeContextBuilder)
        {
            if (!beeType.IsReference)
                return beeType;

            return typeContextBuilder.AssemblyBuilderContext.GetTypeBuilder((string)beeType).TypeBuilder;
        }
    }
}