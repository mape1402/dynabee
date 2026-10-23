namespace DynaBee.Infrastructure.Configurators
{
    using DynaBee.Infrastructure.ContextBuilders;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class IndexerConfigurator : IElementConfigurator
    {
        private readonly string _name;
        private readonly BeeType _indexType;
        private readonly BeeType _valueType;
        private readonly bool _hasGetter;
        private readonly bool _hasSetter;
        private readonly MethodAccessModifier _getterAccessModifier;
        private readonly MethodAccessModifier _setterAccessModifier;
        private readonly IReadOnlyCollection<BeeAttribute> _attributes;
        private readonly IReadOnlyDictionary<string, object> _metadata;

        public IndexerConfigurator(
            string name,
            BeeType indexType,
            BeeType valueType,
            bool hasGetter,
            bool hasSetter,
            MethodAccessModifier getterAccessModifier,
            MethodAccessModifier setterAccessModifier,
            IReadOnlyCollection<BeeAttribute> attributes,
            IReadOnlyDictionary<string, object> metadata)
        {
            _name = name;
            _indexType = indexType;
            _valueType = valueType;
            _hasGetter = hasGetter;
            _hasSetter = hasSetter;
            _getterAccessModifier = getterAccessModifier;
            _setterAccessModifier = setterAccessModifier;
            _attributes = attributes ?? Array.Empty<BeeAttribute>();
            _metadata = metadata ?? new Dictionary<string, object>();
        }

        public void Configure(ITypeContextBuilder typeContextBuilder)
            => typeContextBuilder.AddElement(_name, ElementType.Property, BuildAction, _metadata);

        private void BuildAction(ITypeContextBuilder typeContextBuilder)
        {
            var indexType = _indexType.Resolve(typeContextBuilder);
            var valueType = _valueType.Resolve(typeContextBuilder);
            var dictionaryType = typeof(Dictionary<,>).MakeGenericType(indexType, valueType);
            var typeBuilder = typeContextBuilder.TypeBuilder;
            var store = typeBuilder.DefineField($"_{_name}Store", dictionaryType, FieldAttributes.Private);
            var property = typeBuilder.DefineProperty(_name, PropertyAttributes.HasDefault, valueType, new[] { indexType });
            foreach (var attribute in _attributes)
                property.SetCustomAttribute(attribute.Build());

            if (_hasGetter)
            {
                var getter = typeBuilder.DefineMethod(
                    $"get_{_name}",
                    BuildAccessorAttributes(_getterAccessModifier),
                    valueType,
                    new[] { indexType });
                var il = getter.GetILGenerator();
                EmitLoadStore(il, store, dictionaryType);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Callvirt, dictionaryType.GetProperty("Item")!.GetGetMethod()!);
                il.Emit(OpCodes.Ret);
                property.SetGetMethod(getter);
            }

            if (_hasSetter)
            {
                var setter = typeBuilder.DefineMethod(
                    $"set_{_name}",
                    BuildAccessorAttributes(_setterAccessModifier),
                    null,
                    new[] { indexType, valueType });
                var il = setter.GetILGenerator();
                EmitLoadStore(il, store, dictionaryType);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Callvirt, dictionaryType.GetProperty("Item")!.GetSetMethod()!);
                il.Emit(OpCodes.Ret);
                property.SetSetMethod(setter);
            }
        }

        private static void EmitLoadStore(ILGenerator il, FieldBuilder store, Type dictionaryType)
        {
            var initialized = il.DefineLabel();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, store);
            il.Emit(OpCodes.Brtrue, initialized);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Newobj, dictionaryType.GetConstructor(Type.EmptyTypes)!);
            il.Emit(OpCodes.Stfld, store);
            il.MarkLabel(initialized);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, store);
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
