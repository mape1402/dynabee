namespace DynaBee.Infrastructure.Configurators
{
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    internal class PropertyConfigurator : IElementConfigurator
    {
        private readonly string _name;
        private readonly BeeType _beeType;
        private readonly bool _hasGetter;
        private readonly bool _hasSetter;
        private readonly FieldAccessModifier _fieldAccessModifier;
        private readonly MethodAccessModifier _getterAccessModifier;
        private readonly MethodAccessModifier _setterAccessModifier;
        private readonly IReadOnlyCollection<BeeAttribute> _attributes;
        private readonly IReadOnlyDictionary<string, object> _metadata;

        public PropertyConfigurator(
            string name,
            BeeType type,
            bool hasGetter = true,
            bool hasSetter = true,
            FieldAccessModifier fieldAccessModifier = default,
            MethodAccessModifier getterAccessModifier = default,
            MethodAccessModifier setterAccessModifier = default,
            IReadOnlyCollection<BeeAttribute> attributes = null,
            IReadOnlyDictionary<string, object> metadata = null)
        {
            _name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException(nameof(name)) : name;
            _beeType = type;
            _hasGetter = hasGetter;
            _hasSetter = hasSetter;
            _fieldAccessModifier = fieldAccessModifier;
            _getterAccessModifier = getterAccessModifier;
            _setterAccessModifier = setterAccessModifier;
            _attributes = attributes ?? Array.Empty<BeeAttribute>();
            _metadata = metadata ?? new Dictionary<string, object>();

            if (!_hasGetter && !_hasSetter)
                throw new ArgumentException("A property must define at least a getter or a setter.");
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Configure(ITypeContextBuilder typeContextBuilder)
        {
            typeContextBuilder.AddElement(_name, ElementType.Property, BuildAction, _metadata);
        }

        private void BuildAction(ITypeContextBuilder typeContextBuilder)
        {
            var typeBuilder = typeContextBuilder.TypeBuilder;

            var type = _beeType.IsReference ? typeContextBuilder.AssemblyBuilderContext.GetTypeBuilder(_beeType).TypeBuilder : _beeType;

            var fieldAccess = _fieldAccessModifier.IsDefault ? FieldAccessModifier.Private : _fieldAccessModifier;
            var fieldBuilder = typeBuilder.DefineField($"_{_name}", type, fieldAccess.Attributes);

            var propertyBuilder = typeBuilder.DefineProperty(
                                                                _name,
                                                                PropertyAttributes.HasDefault,
                                                                type,
                                                                null
                                                            );

            foreach (var attribute in _attributes)
                propertyBuilder.SetCustomAttribute(attribute.Build());

            if (_hasGetter)
            {
                var getMethodBuilder = typeBuilder.DefineMethod(
                    $"get_{_name}",
                    BuildAccessorAttributes(_getterAccessModifier),
                    type,
                    Type.EmptyTypes
                );

                var getIL = getMethodBuilder.GetILGenerator();
                getIL.Emit(OpCodes.Ldarg_0);
                getIL.Emit(OpCodes.Ldfld, fieldBuilder);
                getIL.Emit(OpCodes.Ret);

                propertyBuilder.SetGetMethod(getMethodBuilder);
            }

            if (_hasSetter)
            {
                var setMethodBuilder = typeBuilder.DefineMethod(
                    $"set_{_name}",
                    BuildAccessorAttributes(_setterAccessModifier),
                    null,
                    new Type[] { type }
                );

                var setIL = setMethodBuilder.GetILGenerator();
                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldarg_1);
                setIL.Emit(OpCodes.Stfld, fieldBuilder);
                setIL.Emit(OpCodes.Ret);

                propertyBuilder.SetSetMethod(setMethodBuilder);
            }
        }

        private static MethodAttributes BuildAccessorAttributes(MethodAccessModifier accessModifier)
        {
            var access = accessModifier.IsDefault ? MethodAccessModifier.Public : accessModifier;
            var attributes = access.Attributes | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

            if ((attributes & MethodAttributes.MemberAccessMask) != MethodAttributes.Private)
                attributes |= MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.NewSlot;

            return attributes;
        }
    }
}
