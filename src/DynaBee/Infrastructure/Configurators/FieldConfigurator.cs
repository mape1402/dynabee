namespace DynaBee.Infrastructure.Configurators
{
    using DynaBee.Infrastructure.ContextBuilders;
    using System.Reflection;

    internal sealed class FieldConfigurator : IElementConfigurator
    {
        private readonly string _name;
        private readonly BeeType _type;
        private readonly FieldAccessModifier _accessModifier;

        public FieldConfigurator(string name, BeeType type, FieldAccessModifier accessModifier)
        {
            _name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException(nameof(name)) : name;
            _type = type;
            _accessModifier = accessModifier;
        }

        public void Configure(ITypeContextBuilder typeContextBuilder)
        {
            if (typeContextBuilder == null)
                throw new ArgumentNullException(nameof(typeContextBuilder));

            typeContextBuilder.AddElement(_name, ElementType.Field, _ => BuildAction(typeContextBuilder));
        }

        private void BuildAction(ITypeContextBuilder typeContextBuilder)
        {
            var access = _accessModifier.IsDefault ? FieldAccessModifier.Private : _accessModifier;
            var clrType = _type.IsReference
                ? typeContextBuilder.AssemblyBuilderContext.GetTypeBuilder((string)_type).TypeBuilder
                : (Type)_type;

            var fieldBuilder = typeContextBuilder.TypeBuilder.DefineField(_name, clrType, access.Attributes);

            if (typeContextBuilder is TypeContextBuilder concreteTypeContextBuilder)
                concreteTypeContextBuilder.RegisterField(_name, fieldBuilder);
        }
    }
}
