namespace DynaBee.Infrastructure.Configurators
{
    using DynaBee.Infrastructure.ContextBuilders;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class EventConfigurator : IElementConfigurator
    {
        private readonly string _name;
        private readonly BeeType _handlerType;
        private readonly MethodAccessModifier _accessModifier;
        private readonly IReadOnlyCollection<BeeAttribute> _attributes;
        private readonly IReadOnlyDictionary<string, object> _metadata;

        public EventConfigurator(
            string name,
            BeeType handlerType,
            MethodAccessModifier accessModifier,
            IReadOnlyCollection<BeeAttribute> attributes,
            IReadOnlyDictionary<string, object> metadata)
        {
            _name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException(nameof(name)) : name;
            _handlerType = handlerType;
            _accessModifier = accessModifier;
            _attributes = attributes ?? Array.Empty<BeeAttribute>();
            _metadata = metadata ?? new Dictionary<string, object>();
        }

        public void Configure(ITypeContextBuilder typeContextBuilder)
            => typeContextBuilder.AddElement(_name, ElementType.Event, BuildAction, _metadata);

        private void BuildAction(ITypeContextBuilder typeContextBuilder)
        {
            var handlerType = _handlerType.Resolve(typeContextBuilder);
            if (!typeof(MulticastDelegate).IsAssignableFrom(handlerType.BaseType))
                throw new ArgumentException($"Event handler type '{handlerType}' must be a delegate.");

            var typeBuilder = typeContextBuilder.TypeBuilder;
            var field = typeBuilder.DefineField($"_{_name}", handlerType, FieldAttributes.Private);
            var eventBuilder = typeBuilder.DefineEvent(_name, EventAttributes.None, handlerType);
            foreach (var attribute in _attributes)
                eventBuilder.SetCustomAttribute(attribute.Build());

            var access = _accessModifier.IsDefault ? MethodAccessModifier.Public : _accessModifier;
            var methodAttributes = access.Attributes | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.NewSlot;
            var add = typeBuilder.DefineMethod($"add_{_name}", methodAttributes, null, new[] { handlerType });
            var remove = typeBuilder.DefineMethod($"remove_{_name}", methodAttributes, null, new[] { handlerType });
            var combine = typeof(Delegate).GetMethod(nameof(Delegate.Combine), new[] { typeof(Delegate), typeof(Delegate) })!;
            var removeDelegate = typeof(Delegate).GetMethod(nameof(Delegate.Remove), new[] { typeof(Delegate), typeof(Delegate) })!;

            EmitUpdate(add.GetILGenerator(), field, handlerType, combine);
            EmitUpdate(remove.GetILGenerator(), field, handlerType, removeDelegate);
            eventBuilder.SetAddOnMethod(add);
            eventBuilder.SetRemoveOnMethod(remove);
        }

        private static void EmitUpdate(ILGenerator il, FieldBuilder field, Type handlerType, MethodInfo operation)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, field);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, operation);
            il.Emit(OpCodes.Castclass, handlerType);
            il.Emit(OpCodes.Stfld, field);
            il.Emit(OpCodes.Ret);
        }
    }
}
