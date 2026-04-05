namespace DynaBee.Infrastructure.Configurators
{
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class ConstructorConfigurator : IElementConfigurator
    {
        private readonly IReadOnlyList<(string Name, BeeType Type)> _parameters;
        private readonly Action<ILGenerator> _body;

        public ConstructorConfigurator(
            IReadOnlyList<(string Name, BeeType Type)> parameters,
            Action<ILGenerator> body)
        {
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            _body = body;
        }

        public void Configure(ITypeContextBuilder typeContextBuilder)
        {
            if (typeContextBuilder == null)
                throw new ArgumentNullException(nameof(typeContextBuilder));

            typeContextBuilder.AddElement($".ctor:{_parameters.Count}", ElementType.Method, x => BuildAction(x));
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

            var baseType = typeContextBuilder.TypeBuilder.BaseType ?? typeof(object);
            var baseConstructor = baseType.GetConstructor(Type.EmptyTypes);
            if (baseConstructor == null)
                throw new InvalidOperationException(
                    $"Type '{baseType.FullName}' does not have a parameterless constructor. Add a constructor body that calls a valid base constructor.");

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, baseConstructor);
            il.Emit(OpCodes.Ret);
        }

        private static Type ResolveType(BeeType beeType, ITypeContextBuilder typeContextBuilder)
        {
            if (!beeType.IsReference)
                return beeType;

            return typeContextBuilder.AssemblyBuilderContext.GetTypeBuilder((string)beeType).TypeBuilder;
        }
    }
}