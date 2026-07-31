namespace DynaBee.Infrastructure.Configurators
{
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class ConstructorConfigurator : IElementConfigurator
    {
        private readonly IReadOnlyList<(string Name, BeeType Type)> _parameters;
        private readonly Action<ILGenerator> _body;
        private readonly IReadOnlyDictionary<string, object> _metadata;
        private readonly ConstructorInfo _baseConstructor;
        private readonly IReadOnlyList<string> _baseConstructorArgumentNames;

        public ConstructorConfigurator(
            IReadOnlyList<(string Name, BeeType Type)> parameters,
            Action<ILGenerator> body,
            IReadOnlyDictionary<string, object> metadata = null,
            ConstructorInfo baseConstructor = null,
            IReadOnlyList<string> baseConstructorArgumentNames = null)
        {
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            _body = body;
            _metadata = metadata ?? new Dictionary<string, object>();
            _baseConstructor = baseConstructor;
            _baseConstructorArgumentNames = baseConstructorArgumentNames ?? Array.Empty<string>();
        }

        public void Configure(ITypeContextBuilder typeContextBuilder)
        {
            if (typeContextBuilder == null)
                throw new ArgumentNullException(nameof(typeContextBuilder));

            typeContextBuilder.AddElement($".ctor:{_parameters.Count}", ElementType.Method, x => BuildAction(x), _metadata);
        }

        private void BuildAction(ITypeContextBuilder typeContextBuilder)
        {
            var parameterTypes = _parameters.Select(x => ResolveType(x.Type, typeContextBuilder)).ToArray();
            var constructorBuilder = typeContextBuilder.TypeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                parameterTypes);

            for (var i = 0; i < _parameters.Count; i++)
                constructorBuilder.DefineParameter(i + 1, ParameterAttributes.None, _parameters[i].Name);

            var il = constructorBuilder.GetILGenerator();
            if (_body != null)
            {
                _body(il);
                return;
            }

            if (_baseConstructor != null)
            {
                EmitBaseConstructorCall(typeContextBuilder, il, parameterTypes);
                return;
            }

            var baseType = typeContextBuilder.TypeBuilder.BaseType ?? typeof(object);
            var baseConstructor = baseType.GetConstructor(Type.EmptyTypes);
            if (baseConstructor == null)
                throw new InvalidOperationException(
                    $"Type '{baseType.FullName}' does not have a parameterless constructor. Add a constructor body that calls a valid base constructor.");

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, baseConstructor);
            il.Emit(OpCodes.Ret);
        }

        private void EmitBaseConstructorCall(ITypeContextBuilder typeContextBuilder, ILGenerator il, Type[] parameterTypes)
        {
            var generatedBaseType = typeContextBuilder.TypeBuilder.BaseType ?? typeof(object);
            var baseDeclaringType = _baseConstructor.DeclaringType
                ?? throw new InvalidOperationException("The selected base constructor does not have a declaring type.");

            if (!baseDeclaringType.IsAssignableFrom(generatedBaseType))
            {
                throw new InvalidOperationException(
                    $"Constructor '{_baseConstructor}' cannot be called because generated type '{typeContextBuilder.TypeBuilder.FullName}' inherits '{generatedBaseType.FullName}'.");
            }

            var baseParameters = _baseConstructor.GetParameters();
            if (baseParameters.Length != _baseConstructorArgumentNames.Count)
            {
                throw new InvalidOperationException(
                    $"Base constructor '{_baseConstructor}' expects {baseParameters.Length} argument(s), but {_baseConstructorArgumentNames.Count} were configured.");
            }

            il.Emit(OpCodes.Ldarg_0);
            for (var i = 0; i < _baseConstructorArgumentNames.Count; i++)
            {
                var argumentName = _baseConstructorArgumentNames[i];
                var parameterIndex = FindParameterIndex(argumentName);
                var generatedParameterType = parameterTypes[parameterIndex];
                var baseParameterType = baseParameters[i].ParameterType;

                if (!baseParameterType.IsAssignableFrom(generatedParameterType))
                {
                    throw new InvalidOperationException(
                        $"Generated constructor parameter '{argumentName}' of type '{generatedParameterType.FullName}' cannot be passed to base constructor parameter '{baseParameters[i].Name}' of type '{baseParameterType.FullName}'.");
                }

                il.Emit(OpCodes.Ldarg, parameterIndex + 1);
            }

            il.Emit(OpCodes.Call, _baseConstructor);
            il.Emit(OpCodes.Ret);
        }

        private int FindParameterIndex(string parameterName)
        {
            for (var i = 0; i < _parameters.Count; i++)
            {
                if (string.Equals(_parameters[i].Name, parameterName, StringComparison.Ordinal))
                    return i;
            }

            throw new InvalidOperationException($"Generated constructor parameter '{parameterName}' was not found.");
        }

        private static Type ResolveType(BeeType beeType, ITypeContextBuilder typeContextBuilder)
        {
            if (!beeType.IsReference)
                return beeType;

            return typeContextBuilder.AssemblyBuilderContext.GetTypeBuilder((string)beeType).TypeBuilder;
        }
    }
}
