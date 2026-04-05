namespace DynaBee.Infrastructure.Configurators
{
    using DynaBee.FluentApi;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class DependencyConstructorConfigurator : IElementConfigurator
    {
        private readonly IReadOnlyList<(string PropertyName, Type PropertyType, string ParameterName)> _dependencies;

        public DependencyConstructorConfigurator(IReadOnlyList<(string PropertyName, Type PropertyType, string ParameterName)> dependencies)
        {
            _dependencies = dependencies ?? throw new ArgumentNullException(nameof(dependencies));
        }

        public void Configure(ITypeContextBuilder typeContextBuilder)
        {
            if (typeContextBuilder == null)
                throw new ArgumentNullException(nameof(typeContextBuilder));

            typeContextBuilder.AddElement($".ctor:inject:{_dependencies.Count}", ElementType.Method, x => BuildAction(x));
        }

        private void BuildAction(ITypeContextBuilder typeContextBuilder)
        {
            var parameterTypes = _dependencies.Select(x => x.PropertyType).ToArray();

            var constructorBuilder = typeContextBuilder.TypeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                parameterTypes);

            for (var i = 0; i < _dependencies.Count; i++)
                constructorBuilder.DefineParameter(i + 1, ParameterAttributes.None, _dependencies[i].ParameterName);

            var il = constructorBuilder.GetILGenerator();
            var baseConstructor = (typeContextBuilder.TypeBuilder.BaseType ?? typeof(object)).GetConstructor(Type.EmptyTypes)
                ?? throw new InvalidOperationException(
                    "Base type must expose a parameterless constructor when using synthesized dependency constructor.");

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, baseConstructor);

            for (var i = 0; i < _dependencies.Count; i++)
            {
                var dependency = _dependencies[i];

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldstr, dependency.PropertyName);
                il.Emit(OpCodes.Ldarg, i + 1);
                if (dependency.PropertyType.IsValueType)
                    il.Emit(OpCodes.Box, dependency.PropertyType);

                var setPropertyMethod = typeof(DynamicAccess).GetMethod(nameof(DynamicAccess.SetProperty))
                    ?? throw new InvalidOperationException("DynamicAccess.SetProperty was not found.");
                il.Emit(OpCodes.Call, setPropertyMethod);
            }

            il.Emit(OpCodes.Ret);
        }
    }
}
