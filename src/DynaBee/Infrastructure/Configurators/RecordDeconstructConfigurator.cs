namespace DynaBee.Infrastructure.Configurators
{
    using DynaBee.FluentApi;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class RecordDeconstructConfigurator : IElementConfigurator
    {
        private readonly IReadOnlyList<(string Name, BeeType Type)> _components;

        public RecordDeconstructConfigurator(IReadOnlyList<(string Name, BeeType Type)> components)
        {
            _components = components ?? throw new ArgumentNullException(nameof(components));
        }

        public void Configure(ITypeContextBuilder typeContextBuilder)
        {
            if (typeContextBuilder == null)
                throw new ArgumentNullException(nameof(typeContextBuilder));

            typeContextBuilder.AddElement("Deconstruct", ElementType.Method, _ => BuildAction(typeContextBuilder));
        }

        private void BuildAction(ITypeContextBuilder typeContextBuilder)
        {
            if (_components.Count == 0)
                return;

            var getPropertyGenericMethod = typeof(DynamicAccess)
                .GetMethod(nameof(DynamicAccess.GetProperty), BindingFlags.Public | BindingFlags.Static)
                ?? throw new InvalidOperationException("DynamicAccess.GetProperty method was not found.");

            var parameterTypes = _components
                .Select(x => ResolveType(x.Type, typeContextBuilder).MakeByRefType())
                .ToArray();

            var method = typeContextBuilder.TypeBuilder.DefineMethod(
                "Deconstruct",
                MethodAttributes.Public | MethodAttributes.HideBySig,
                typeof(void),
                parameterTypes);

            for (var i = 0; i < _components.Count; i++)
                method.DefineParameter(i + 1, ParameterAttributes.Out, _components[i].Name);

            var il = method.GetILGenerator();

            for (var i = 0; i < _components.Count; i++)
            {
                var componentName = _components[i].Name;
                var componentType = ResolveType(_components[i].Type, typeContextBuilder);
                var getPropertyMethod = getPropertyGenericMethod.MakeGenericMethod(componentType);

                il.Emit(OpCodes.Ldarg, i + 1);
                il.Emit(OpCodes.Ldarg_0);

                if (typeContextBuilder.TypeBuilder.IsValueType)
                {
                    il.Emit(OpCodes.Ldobj, typeContextBuilder.TypeBuilder);
                    il.Emit(OpCodes.Box, typeContextBuilder.TypeBuilder);
                }

                il.Emit(OpCodes.Ldstr, componentName);
                il.Emit(OpCodes.Call, getPropertyMethod);

                if (componentType.IsValueType)
                    il.Emit(OpCodes.Stobj, componentType);
                else
                    il.Emit(OpCodes.Stind_Ref);
            }

            il.Emit(OpCodes.Ret);
        }

        private static Type ResolveType(BeeType type, ITypeContextBuilder context)
        {
            if (!type.IsReference)
                return type;

            return context.AssemblyBuilderContext.GetTypeBuilder((string)type).TypeBuilder;
        }
    }
}
