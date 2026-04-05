namespace DynaBee.Infrastructure.Configurators
{
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class EnumConfigurator : ITypeConfigurator
    {
        private readonly Dictionary<string, object> _values = new(StringComparer.Ordinal);
        private readonly List<BeeAttribute> _attributes = new();
        private readonly string _name;
        private readonly Type _underlyingType;
        private readonly ClassAccessModifier _accessModifier;

        public EnumConfigurator(string name, Type underlyingType, ClassAccessModifier accessModifier)
        {
            _name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException(nameof(name)) : name;
            _underlyingType = underlyingType ?? throw new ArgumentNullException(nameof(underlyingType));
            _accessModifier = accessModifier;

            if (!IsValidUnderlyingType(_underlyingType))
                throw new ArgumentException("Enum underlying type must be a valid integral type.", nameof(underlyingType));
        }

        public ITypeConfigurator AddElementBuilder(IElementConfigurator elementConfigurator)
            => this;

        public EnumConfigurator AddValue(string name, object value)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var coercedValue = Convert.ChangeType(value, _underlyingType);
            _values[name] = coercedValue;
            return this;
        }

        public EnumConfigurator AddAttribute(BeeAttribute attribute)
        {
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));

            _attributes.Add(attribute);
            return this;
        }

        public void Configure(IAssemblyContextBuilder assemblyContextBuilder)
        {
            var access = _accessModifier.IsDefault ? ClassAccessModifier.Public : _accessModifier;
            var typeAttributes = (TypeAttributes)access | TypeAttributes.Sealed;
            var typeBuilder = assemblyContextBuilder.ModuleBuilder.DefineType(_name, typeAttributes, typeof(Enum));

            foreach (var attribute in _attributes)
                typeBuilder.SetCustomAttribute(attribute.Build());

            typeBuilder.DefineField(
                "value__",
                _underlyingType,
                FieldAttributes.Private | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName);

            foreach (var pair in _values)
            {
                var field = typeBuilder.DefineField(
                    pair.Key,
                    typeBuilder,
                    FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal);

                field.SetConstant(pair.Value);
            }

            assemblyContextBuilder.AddTypeBuilder(_name, typeBuilder);
        }

        private static bool IsValidUnderlyingType(Type type)
            => type == typeof(byte)
            || type == typeof(sbyte)
            || type == typeof(short)
            || type == typeof(ushort)
            || type == typeof(int)
            || type == typeof(uint)
            || type == typeof(long)
            || type == typeof(ulong);
    }
}
