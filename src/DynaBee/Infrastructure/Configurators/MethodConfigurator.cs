namespace DynaBee.Infrastructure.Configurators
{
    using DynaBee.FluentApi.Body;
    using DynaBee.Infrastructure.ContextBuilders;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class MethodConfigurator : IElementConfigurator
    {
        private readonly string _name;
        private readonly BeeType _returnType;
        private readonly IReadOnlyList<(string Name, BeeType Type)> _parameters;
        private readonly Action<ILGenerator> _ilBody;
        private readonly Action<IBeeMethodBodyBuilder> _methodBody;
        private readonly Delegate _lambdaBody;
        private readonly LambdaExpression _expressionBody;
        private readonly bool _isStatic;
        private readonly MethodAccessModifier _accessModifier;
        private readonly IReadOnlyCollection<BeeAttribute> _attributes;
        private readonly IReadOnlyDictionary<string, object> _metadata;
        private readonly MethodInfo _overrideMethod;

        public MethodConfigurator(
            string name,
            BeeType returnType,
            IReadOnlyList<(string Name, BeeType Type)> parameters,
            Action<ILGenerator> ilBody,
            Action<IBeeMethodBodyBuilder> methodBody,
            Delegate lambdaBody,
            LambdaExpression expressionBody,
            bool isStatic,
            MethodAccessModifier accessModifier,
            IReadOnlyCollection<BeeAttribute> attributes,
            IReadOnlyDictionary<string, object> metadata = null,
            MethodInfo overrideMethod = null)
        {
            _name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException(nameof(name)) : name;
            _returnType = returnType;
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            _ilBody = ilBody;
            _methodBody = methodBody;
            _lambdaBody = lambdaBody;
            _expressionBody = expressionBody;
            _isStatic = isStatic;
            _accessModifier = accessModifier;
            _attributes = attributes ?? Array.Empty<BeeAttribute>();
            _metadata = metadata ?? new Dictionary<string, object>();
            _overrideMethod = overrideMethod;
        }

        public void Configure(ITypeContextBuilder typeContextBuilder)
        {
            if (typeContextBuilder == null)
                throw new ArgumentNullException(nameof(typeContextBuilder));

            typeContextBuilder.AddElement(_name, ElementType.Method, x => BuildAction(x), _metadata);
        }

        private void BuildAction(ITypeContextBuilder typeContextBuilder)
        {
            var returnType = ResolveType(_returnType, typeContextBuilder);
            var parameterTypes = _parameters.Select(x => ResolveType(x.Type, typeContextBuilder)).ToArray();
            ValidateOverride(typeContextBuilder, returnType, parameterTypes);

            var access = ResolveAccessModifier();
            var attributes = access.Attributes | MethodAttributes.HideBySig;
            if (_isStatic)
            {
                attributes |= MethodAttributes.Static;
            }
            else if (_overrideMethod != null)
            {
                attributes |= MethodAttributes.Virtual;
            }
            else if ((attributes & MethodAttributes.MemberAccessMask) != MethodAttributes.Private)
            {
                attributes |= MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.NewSlot;
            }

            var methodBuilder = typeContextBuilder.TypeBuilder.DefineMethod(_name, attributes, returnType, parameterTypes);
            if (_overrideMethod != null)
                typeContextBuilder.TypeBuilder.DefineMethodOverride(methodBuilder, _overrideMethod);

            if (typeContextBuilder is TypeContextBuilder concreteTypeContextBuilder)
                concreteTypeContextBuilder.RegisterMethod(_name, parameterTypes, methodBuilder);

            for (var i = 0; i < _parameters.Count; i++)
                methodBuilder.DefineParameter(i + 1, ParameterAttributes.None, _parameters[i].Name);

            foreach (var attribute in _attributes)
                methodBuilder.SetCustomAttribute(attribute.Build());

            var il = methodBuilder.GetILGenerator();

            if (_expressionBody != null)
            {
                ExpressionIlEmitter.Emit(_expressionBody, il, parameterTypes, returnType, _isStatic);
                return;
            }

            if (_lambdaBody != null)
            {
                EmitLambdaForwarder(il, parameterTypes, returnType);
                return;
            }

            if (_methodBody != null)
            {
                var bodyParameters = _parameters
                    .Select((x, index) => (x.Name, Type: parameterTypes[index], ArgumentIndex: _isStatic ? index : index + 1))
                    .ToArray();

                var bodyBuilder = new BeeMethodBodyBuilder(
                    il,
                    returnType,
                    bodyParameters,
                    _isStatic ? null : typeContextBuilder.TypeBuilder,
                    typeContextBuilder as TypeContextBuilder);
                _methodBody(bodyBuilder);

                if (!bodyBuilder.HasReturn)
                    EmitDefaultBody(il, returnType);

                return;
            }

            if (_ilBody == null)
            {
                EmitDefaultBody(il, returnType);
                return;
            }

            _ilBody(il);
        }

        private void ValidateOverride(ITypeContextBuilder typeContextBuilder, Type returnType, Type[] parameterTypes)
        {
            if (_overrideMethod == null)
                return;

            if (_isStatic)
                throw new InvalidOperationException($"Method '{_overrideMethod.Name}' cannot be overridden by a static method.");

            if (!_overrideMethod.IsVirtual || _overrideMethod.IsFinal || _overrideMethod.IsPrivate)
                throw new InvalidOperationException($"Method '{_overrideMethod.DeclaringType?.FullName}.{_overrideMethod.Name}' cannot be overridden because it is not an overridable virtual or abstract method.");

            var generatedBaseType = typeContextBuilder.TypeBuilder.BaseType ?? typeof(object);
            var declaringType = _overrideMethod.DeclaringType;
            if (declaringType == null || !declaringType.IsAssignableFrom(generatedBaseType))
            {
                throw new InvalidOperationException(
                    $"Method '{_overrideMethod.Name}' cannot be overridden because generated type '{typeContextBuilder.TypeBuilder.FullName}' does not inherit '{declaringType?.FullName}'.");
            }

            if (_overrideMethod.ReturnType != returnType)
                throw new InvalidOperationException($"Override method '{_overrideMethod.Name}' must return '{_overrideMethod.ReturnType.FullName}', not '{returnType.FullName}'.");

            var baseParameters = _overrideMethod.GetParameters();
            if (baseParameters.Length != parameterTypes.Length)
                throw new InvalidOperationException($"Override method '{_overrideMethod.Name}' must define {baseParameters.Length} parameter(s).");

            for (var i = 0; i < baseParameters.Length; i++)
            {
                if (baseParameters[i].ParameterType != parameterTypes[i])
                    throw new InvalidOperationException($"Override method '{_overrideMethod.Name}' parameter {i} must be '{baseParameters[i].ParameterType.FullName}', not '{parameterTypes[i].FullName}'.");
            }

            if (!_accessModifier.IsDefault && _accessModifier.Attributes != GetAccessModifier(_overrideMethod).Attributes)
                throw new InvalidOperationException($"Override method '{_overrideMethod.Name}' must preserve base method access level '{GetAccessModifier(_overrideMethod)}'.");
        }

        private MethodAccessModifier ResolveAccessModifier()
        {
            if (!_accessModifier.IsDefault)
                return _accessModifier;

            if (_overrideMethod != null)
                return GetAccessModifier(_overrideMethod);

            return MethodAccessModifier.Public;
        }

        internal static MethodAccessModifier GetAccessModifier(MethodBase method)
        {
            if (method.IsPublic)
                return MethodAccessModifier.Public;

            if (method.IsFamily)
                return MethodAccessModifier.Protected;

            if (method.IsAssembly)
                return MethodAccessModifier.Internal;

            if (method.IsFamilyOrAssembly)
                return MethodAccessModifier.ProtectedInternal;

            if (method.IsFamilyAndAssembly)
                return MethodAccessModifier.PrivateProtected;

            return MethodAccessModifier.Private;
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

        private void EmitLambdaForwarder(ILGenerator il, Type[] parameterTypes, Type returnType)
        {
            var lambdaId = LambdaMethodRegistry.Register(_lambdaBody);
            il.Emit(OpCodes.Ldc_I4, lambdaId);

            if (_isStatic)
                il.Emit(OpCodes.Ldnull);
            else
                il.Emit(OpCodes.Ldarg_0);

            il.Emit(OpCodes.Ldc_I4, _parameters.Count);
            il.Emit(OpCodes.Newarr, typeof(object));

            for (var i = 0; i < _parameters.Count; i++)
            {
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldarg, i + 1);

                var parameterType = parameterTypes[i];
                if (parameterType.IsValueType)
                    il.Emit(OpCodes.Box, parameterType);

                il.Emit(OpCodes.Stelem_Ref);
            }

            var invokeMethod = typeof(LambdaMethodRegistry).GetMethod(nameof(LambdaMethodRegistry.Invoke), BindingFlags.Public | BindingFlags.Static);
            il.Emit(OpCodes.Call, invokeMethod);

            if (returnType == typeof(void))
            {
                il.Emit(OpCodes.Pop);
                il.Emit(OpCodes.Ret);
                return;
            }

            if (returnType.IsValueType)
                il.Emit(OpCodes.Unbox_Any, returnType);
            else
                il.Emit(OpCodes.Castclass, returnType);

            il.Emit(OpCodes.Ret);
        }

        private static Type ResolveType(BeeType beeType, ITypeContextBuilder typeContextBuilder)
        {
            if (!beeType.IsReference)
                return beeType;

            if (typeContextBuilder == null)
                throw new InvalidOperationException("A reference BeeType requires a type context builder.");

            return typeContextBuilder.AssemblyBuilderContext.GetTypeBuilder((string)beeType).TypeBuilder;
        }
    }
}
