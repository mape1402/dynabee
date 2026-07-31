namespace DynaBee.Infrastructure.Configurators
{
    using DynaBee.FluentApi;
    using DynaBee.FluentApi.Body;
    using DynaBee.Infrastructure.ContextBuilders;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class PropertyOverrideConfigurator : IElementConfigurator
    {
        private readonly PropertyInfo _baseProperty;
        private readonly BeePropertyAccessorBuilder _getter;
        private readonly BeePropertyAccessorBuilder _setter;
        private readonly IReadOnlyCollection<BeeAttribute> _attributes;
        private readonly IReadOnlyDictionary<string, object> _metadata;

        public PropertyOverrideConfigurator(
            PropertyInfo baseProperty,
            BeePropertyAccessorBuilder getter,
            BeePropertyAccessorBuilder setter,
            IReadOnlyCollection<BeeAttribute> attributes,
            IReadOnlyDictionary<string, object> metadata)
        {
            _baseProperty = baseProperty ?? throw new ArgumentNullException(nameof(baseProperty));
            _getter = getter;
            _setter = setter;
            _attributes = attributes ?? Array.Empty<BeeAttribute>();
            _metadata = metadata ?? new Dictionary<string, object>();
        }

        public void Configure(ITypeContextBuilder typeContextBuilder)
        {
            typeContextBuilder.AddElement(_baseProperty.Name, ElementType.Property, BuildAction, _metadata);
        }

        private void BuildAction(ITypeContextBuilder typeContextBuilder)
        {
            ValidateProperty(typeContextBuilder);

            var typeBuilder = typeContextBuilder.TypeBuilder;
            var propertyBuilder = typeBuilder.DefineProperty(
                _baseProperty.Name,
                PropertyAttributes.HasDefault,
                _baseProperty.PropertyType,
                null);

            foreach (var attribute in _attributes)
                propertyBuilder.SetCustomAttribute(attribute.Build());

            var baseGetter = _baseProperty.GetMethod;
            var baseSetter = _baseProperty.SetMethod;
            MethodBuilder getterBuilder = null;
            MethodBuilder setterBuilder = null;

            if (baseGetter != null && _getter != null)
            {
                getterBuilder = DefineAccessor(
                    typeContextBuilder,
                    baseGetter,
                    $"get_{_baseProperty.Name}",
                    _baseProperty.PropertyType,
                    Type.EmptyTypes,
                    Array.Empty<(string Name, Type Type, int ArgumentIndex)>(),
                    _getter);
                propertyBuilder.SetGetMethod(getterBuilder);
                typeBuilder.DefineMethodOverride(getterBuilder, baseGetter);
            }

            if (baseSetter != null && _setter != null)
            {
                setterBuilder = DefineAccessor(
                    typeContextBuilder,
                    baseSetter,
                    $"set_{_baseProperty.Name}",
                    typeof(void),
                    new[] { _baseProperty.PropertyType },
                    new[] { ("value", _baseProperty.PropertyType, 1) },
                    _setter);
                propertyBuilder.SetSetMethod(setterBuilder);
                typeBuilder.DefineMethodOverride(setterBuilder, baseSetter);
            }

            if (typeContextBuilder is TypeContextBuilder concreteTypeContextBuilder)
                concreteTypeContextBuilder.RegisterProperty(_baseProperty.Name, _baseProperty.PropertyType, getterBuilder, setterBuilder);
        }

        private MethodBuilder DefineAccessor(
            ITypeContextBuilder typeContextBuilder,
            MethodInfo baseAccessor,
            string name,
            Type returnType,
            Type[] parameterTypes,
            IReadOnlyList<(string Name, Type Type, int ArgumentIndex)> bodyParameters,
            BeePropertyAccessorBuilder accessor)
        {
            var attributes = MethodConfigurator.GetAccessModifier(baseAccessor).Attributes
                | MethodAttributes.SpecialName
                | MethodAttributes.HideBySig
                | MethodAttributes.Virtual;
            var methodBuilder = typeContextBuilder.TypeBuilder.DefineMethod(name, attributes, returnType, parameterTypes);
            var il = methodBuilder.GetILGenerator();

            if (accessor.IlBody != null)
            {
                accessor.IlBody(il);
                return methodBuilder;
            }

            if (accessor.HasConstantValue)
            {
                if (returnType == typeof(void))
                    throw new InvalidOperationException($"Setter override '{name}' cannot return a constant value.");

                var bodyBuilder = new BeeMethodBodyBuilder(
                    il,
                    returnType,
                    bodyParameters,
                    typeContextBuilder.TypeBuilder,
                    typeContextBuilder as TypeContextBuilder);
                bodyBuilder.Return(bodyBuilder.Constant(accessor.ConstantValue, returnType));
                return methodBuilder;
            }

            if (accessor.MethodBody != null)
            {
                var bodyBuilder = new BeeMethodBodyBuilder(
                    il,
                    returnType,
                    bodyParameters,
                    typeContextBuilder.TypeBuilder,
                    typeContextBuilder as TypeContextBuilder);
                accessor.MethodBody(bodyBuilder);

                if (!bodyBuilder.HasReturn)
                    EmitDefaultBody(il, returnType);

                return methodBuilder;
            }

            EmitDefaultBody(il, returnType);
            return methodBuilder;
        }

        private void ValidateProperty(ITypeContextBuilder typeContextBuilder)
        {
            var generatedBaseType = typeContextBuilder.TypeBuilder.BaseType ?? typeof(object);
            var declaringType = _baseProperty.DeclaringType;
            if (declaringType == null || !declaringType.IsAssignableFrom(generatedBaseType))
            {
                throw new InvalidOperationException(
                    $"Property '{_baseProperty.Name}' cannot be overridden because generated type '{typeContextBuilder.TypeBuilder.FullName}' does not inherit '{declaringType?.FullName}'.");
            }

            if (_getter == null && _setter == null)
                throw new InvalidOperationException($"Property override '{_baseProperty.Name}' must configure a getter and/or setter.");

            if (_getter != null)
                ValidateAccessor(_baseProperty.GetMethod, "getter");

            if (_setter != null)
                ValidateAccessor(_baseProperty.SetMethod, "setter");
        }

        private void ValidateAccessor(MethodInfo accessor, string accessorName)
        {
            if (accessor == null)
                throw new InvalidOperationException($"Property '{_baseProperty.Name}' does not define a {accessorName} to override.");

            if (!accessor.IsVirtual || accessor.IsFinal || accessor.IsPrivate)
                throw new InvalidOperationException($"Property '{_baseProperty.Name}' {accessorName} cannot be overridden because it is not virtual or abstract.");
        }

        private static void EmitDefaultBody(ILGenerator il, Type returnType)
        {
            if (returnType == typeof(void))
            {
                il.Emit(OpCodes.Ret);
                return;
            }

            if (returnType.IsValueType)
            {
                var local = il.DeclareLocal(returnType);
                il.Emit(OpCodes.Ldloca_S, local);
                il.Emit(OpCodes.Initobj, returnType);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ret);
                return;
            }

            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ret);
        }
    }
}
