namespace DynaBee.Infrastructure.Configurators
{
    using System.Reflection;

    internal sealed class InterfacePropertyConfigurator : IElementConfigurator
    {
        private readonly string _name;
        private readonly BeeType _type;
        private readonly bool _hasGetter;
        private readonly bool _hasSetter;
        private readonly MethodAccessModifier _getterAccessModifier;
        private readonly MethodAccessModifier _setterAccessModifier;
        private readonly IReadOnlyCollection<BeeAttribute> _attributes;

        public InterfacePropertyConfigurator(
            string name,
            BeeType type,
            bool hasGetter,
            bool hasSetter,
            MethodAccessModifier getterAccessModifier,
            MethodAccessModifier setterAccessModifier,
            IReadOnlyCollection<BeeAttribute> attributes)
        {
            _name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException(nameof(name)) : name;
            _type = type;
            _hasGetter = hasGetter;
            _hasSetter = hasSetter;
            _getterAccessModifier = getterAccessModifier;
            _setterAccessModifier = setterAccessModifier;
            _attributes = attributes ?? Array.Empty<BeeAttribute>();

            if (!_hasGetter && !_hasSetter)
                throw new ArgumentException("An interface property must define at least a getter or a setter.");
        }

        public void Configure(ITypeContextBuilder typeContextBuilder)
        {
            typeContextBuilder.AddElement(_name, ElementType.Property, _ => BuildAction(typeContextBuilder));
        }

        private void BuildAction(ITypeContextBuilder typeContextBuilder)
        {
            var clrType = _type.IsReference
                ? typeContextBuilder.AssemblyBuilderContext.GetTypeBuilder((string)_type).TypeBuilder
                : (Type)_type;

            var propertyBuilder = typeContextBuilder.TypeBuilder.DefineProperty(_name, PropertyAttributes.None, clrType, null);

            foreach (var attribute in _attributes)
                propertyBuilder.SetCustomAttribute(attribute.Build());

            if (_hasGetter)
            {
                var access = _getterAccessModifier.IsDefault ? MethodAccessModifier.Public : _getterAccessModifier;
                var getterAttributes = access.Attributes | MethodAttributes.Abstract | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.NewSlot;
                var getter = typeContextBuilder.TypeBuilder.DefineMethod($"get_{_name}", getterAttributes, clrType, Type.EmptyTypes);
                propertyBuilder.SetGetMethod(getter);
            }

            if (_hasSetter)
            {
                var access = _setterAccessModifier.IsDefault ? MethodAccessModifier.Public : _setterAccessModifier;
                var setterAttributes = access.Attributes | MethodAttributes.Abstract | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.NewSlot;
                var setter = typeContextBuilder.TypeBuilder.DefineMethod($"set_{_name}", setterAttributes, null, new[] { clrType });
                propertyBuilder.SetSetMethod(setter);
            }
        }
    }
}