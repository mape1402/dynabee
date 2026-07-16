namespace DynaBee.FluentApi.Invocation
{
    using System.Collections.Concurrent;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Creates cached method invokers for generated assembly contexts.
    /// </summary>
    public static class AssemblyContextInvokerExtensions
    {
        private static readonly ConditionalWeakTable<IAssemblyContext, ConcurrentDictionary<InvokerKey, IDynaBeeMethodInvoker>> Cache = new();

        /// <summary>
        /// Creates or retrieves a cached unbound invoker for a generated method.
        /// </summary>
        /// <param name="context">Generated assembly context.</param>
        /// <param name="typeName">Generated type name.</param>
        /// <param name="methodName">Method name.</param>
        /// <param name="parameterTypes">Requested method parameter types.</param>
        /// <returns>A cached method invoker.</returns>
        public static IDynaBeeMethodInvoker CreateMethodInvoker(
            this IAssemblyContext context,
            string typeName,
            string methodName,
            IReadOnlyList<Type> parameterTypes)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var key = InvokerKey.Create(typeName, methodName, parameterTypes);
            var contextCache = Cache.GetValue(context, _ => new ConcurrentDictionary<InvokerKey, IDynaBeeMethodInvoker>());

            return contextCache.GetOrAdd(key, _ => CreateUnboundInvoker(context, typeName, methodName, parameterTypes));
        }

        /// <summary>
        /// Creates a bound invoker for a generated method and target instance.
        /// </summary>
        /// <param name="context">Generated assembly context.</param>
        /// <param name="typeName">Generated type name.</param>
        /// <param name="instance">Target object instance.</param>
        /// <param name="methodName">Method name.</param>
        /// <param name="parameterTypes">Requested method parameter types.</param>
        /// <returns>A bound method invoker.</returns>
        public static IDynaBeeBoundMethodInvoker CreateBoundMethodInvoker(
            this IAssemblyContext context,
            string typeName,
            object instance,
            string methodName,
            IReadOnlyList<Type> parameterTypes)
        {
            var invoker = context.CreateMethodInvoker(typeName, methodName, parameterTypes);
            return new DynaBeeBoundMethodInvoker(typeName, methodName, invoker, instance);
        }

        /// <summary>
        /// Creates a typed bound invoker delegate for a generated method with one argument.
        /// </summary>
        /// <typeparam name="T1">First argument type.</typeparam>
        /// <typeparam name="TResult">Return type.</typeparam>
        /// <param name="context">Generated assembly context.</param>
        /// <param name="typeName">Generated type name.</param>
        /// <param name="instance">Target object instance.</param>
        /// <param name="methodName">Method name.</param>
        /// <returns>A typed delegate bound to the target instance.</returns>
        public static Func<T1, TResult> CreateBoundMethodInvoker<T1, TResult>(
            this IAssemblyContext context,
            string typeName,
            object instance,
            string methodName)
        {
            var invoker = context.CreateBoundMethodInvoker(typeName, instance, methodName, new[] { typeof(T1) });
            return arg1 => (TResult)invoker.Invoke(new object[] { arg1 });
        }

        /// <summary>
        /// Creates a typed bound invoker delegate for a generated method with two arguments.
        /// </summary>
        /// <typeparam name="T1">First argument type.</typeparam>
        /// <typeparam name="T2">Second argument type.</typeparam>
        /// <typeparam name="TResult">Return type.</typeparam>
        /// <param name="context">Generated assembly context.</param>
        /// <param name="typeName">Generated type name.</param>
        /// <param name="instance">Target object instance.</param>
        /// <param name="methodName">Method name.</param>
        /// <returns>A typed delegate bound to the target instance.</returns>
        public static Func<T1, T2, TResult> CreateBoundMethodInvoker<T1, T2, TResult>(
            this IAssemblyContext context,
            string typeName,
            object instance,
            string methodName)
        {
            var invoker = context.CreateBoundMethodInvoker(typeName, instance, methodName, new[] { typeof(T1), typeof(T2) });
            return (arg1, arg2) => (TResult)invoker.Invoke(new object[] { arg1, arg2 });
        }

        /// <summary>
        /// Creates a typed bound invoker delegate for a generated method with three arguments.
        /// </summary>
        /// <typeparam name="T1">First argument type.</typeparam>
        /// <typeparam name="T2">Second argument type.</typeparam>
        /// <typeparam name="T3">Third argument type.</typeparam>
        /// <typeparam name="TResult">Return type.</typeparam>
        /// <param name="context">Generated assembly context.</param>
        /// <param name="typeName">Generated type name.</param>
        /// <param name="instance">Target object instance.</param>
        /// <param name="methodName">Method name.</param>
        /// <returns>A typed delegate bound to the target instance.</returns>
        public static Func<T1, T2, T3, TResult> CreateBoundMethodInvoker<T1, T2, T3, TResult>(
            this IAssemblyContext context,
            string typeName,
            object instance,
            string methodName)
        {
            var invoker = context.CreateBoundMethodInvoker(typeName, instance, methodName, new[] { typeof(T1), typeof(T2), typeof(T3) });
            return (arg1, arg2, arg3) => (TResult)invoker.Invoke(new object[] { arg1, arg2, arg3 });
        }

        private static IDynaBeeMethodInvoker CreateUnboundInvoker(
            IAssemblyContext context,
            string typeName,
            string methodName,
            IReadOnlyList<Type> parameterTypes)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentException(nameof(typeName));

            if (string.IsNullOrWhiteSpace(methodName))
                throw new ArgumentException(nameof(methodName));

            if (parameterTypes == null)
                throw new ArgumentNullException(nameof(parameterTypes));

            var typeContext = ResolveTypeContext(context, typeName, methodName, parameterTypes);
            var method = ResolveMethod(context, typeContext.ClrType, typeName, methodName, parameterTypes);
            var dispatch = DynaBeeMethodDispatchFactory.Create(typeContext.ClrType, method);

            return new DynaBeeMethodInvoker(context.Name, typeName, methodName, typeContext.ClrType, method.ReturnType, parameterTypes.ToArray(), dispatch);
        }

        private static ITypeContext ResolveTypeContext(
            IAssemblyContext context,
            string typeName,
            string methodName,
            IReadOnlyList<Type> parameterTypes)
        {
            try
            {
                return context.Find(typeName);
            }
            catch (Exception ex)
            {
                throw CreateResolutionException(context.Name, typeName, methodName, parameterTypes, $"Generated type '{typeName}' was not found.", ex);
            }
        }

        private static MethodInfo ResolveMethod(
            IAssemblyContext context,
            Type generatedType,
            string typeName,
            string methodName,
            IReadOnlyList<Type> parameterTypes)
        {
            var namedMethods = generatedType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => string.Equals(x.Name, methodName, StringComparison.Ordinal))
                .ToArray();

            if (namedMethods.Length == 0)
                throw CreateResolutionException(context.Name, typeName, methodName, parameterTypes, $"Method '{methodName}' was not found.");

            var matches = namedMethods
                .Where(method => ParametersMatch(method.GetParameters(), parameterTypes))
                .ToArray();

            if (matches.Length == 0)
                throw CreateResolutionException(context.Name, typeName, methodName, parameterTypes, "No overload matches the requested parameter types.");

            if (matches.Length > 1)
                throw CreateResolutionException(context.Name, typeName, methodName, parameterTypes, "More than one overload matches the requested parameter types.");

            return matches[0];
        }

        private static bool ParametersMatch(ParameterInfo[] actualParameters, IReadOnlyList<Type> requestedParameterTypes)
        {
            if (actualParameters.Length != requestedParameterTypes.Count)
                return false;

            for (var i = 0; i < actualParameters.Length; i++)
            {
                if (actualParameters[i].ParameterType != requestedParameterTypes[i])
                    return false;
            }

            return true;
        }

        internal static InvalidOperationException CreateResolutionException(
            string assemblyName,
            string typeName,
            string methodName,
            IReadOnlyList<Type> parameterTypes,
            string reason,
            Exception innerException = null)
            => new(
                $"Cannot create DynaBee method invoker. Assembly='{assemblyName}', Type='{typeName}', Method='{methodName}', Parameters=[{FormatTypes(parameterTypes)}]. {reason}",
                innerException);

        internal static string FormatTypes(IReadOnlyList<Type> parameterTypes)
            => parameterTypes == null ? string.Empty : string.Join(", ", parameterTypes.Select(x => x?.FullName ?? "<null>"));
    }

    internal sealed class DynaBeeMethodInvoker : IDynaBeeMethodInvoker
    {
        private readonly string _assemblyName;
        private readonly string _typeName;
        private readonly string _methodName;
        private readonly Type _targetType;
        private readonly DynaBeeMethodDispatch _dispatch;

        public DynaBeeMethodInvoker(
            string assemblyName,
            string typeName,
            string methodName,
            Type targetType,
            Type returnType,
            IReadOnlyList<Type> parameterTypes,
            DynaBeeMethodDispatch dispatch)
        {
            _assemblyName = assemblyName;
            _typeName = typeName;
            _methodName = methodName;
            _targetType = targetType;
            ReturnType = returnType;
            ParameterTypes = parameterTypes;
            _dispatch = dispatch;
        }

        public Type ReturnType { get; }

        public IReadOnlyList<Type> ParameterTypes { get; }

        public object Invoke(object instance, IReadOnlyList<object> arguments)
        {
            ValidateTargetInstance(instance);
            var argumentArray = ValidateArguments(arguments);
            return _dispatch(instance, argumentArray);
        }

        internal void ValidateTargetInstance(object instance)
        {
            if (instance == null)
                throw CreateInvocationException("Target instance cannot be null.");

            if (!_targetType.IsInstanceOfType(instance))
                throw CreateInvocationException($"Target instance type '{instance.GetType().FullName}' is not assignable to generated type '{_targetType.FullName}'.");
        }

        private object[] ValidateArguments(IReadOnlyList<object> arguments)
        {
            if (arguments == null)
                throw new ArgumentNullException(nameof(arguments));

            if (arguments.Count != ParameterTypes.Count)
                throw CreateInvocationException($"Argument count mismatch. Expected {ParameterTypes.Count}, received {arguments.Count}.");

            var array = arguments as object[] ?? arguments.ToArray();

            for (var i = 0; i < ParameterTypes.Count; i++)
            {
                var expectedType = ParameterTypes[i];
                var value = array[i];

                if (!CanAssign(value, expectedType))
                {
                    var actualType = value?.GetType().FullName ?? "<null>";
                    throw CreateInvocationException($"Argument {i} cannot be assigned. Expected '{expectedType.FullName}', received '{actualType}'.");
                }
            }

            return array;
        }

        private static bool CanAssign(object value, Type expectedType)
        {
            if (value == null)
                return !expectedType.IsValueType || Nullable.GetUnderlyingType(expectedType) != null;

            if (expectedType.IsInstanceOfType(value))
                return true;

            var nullableUnderlying = Nullable.GetUnderlyingType(expectedType);
            return nullableUnderlying != null && nullableUnderlying.IsInstanceOfType(value);
        }

        private InvalidOperationException CreateInvocationException(string reason)
            => AssemblyContextInvokerExtensions.CreateResolutionException(_assemblyName, _typeName, _methodName, ParameterTypes, reason);
    }

    internal sealed class DynaBeeBoundMethodInvoker : IDynaBeeBoundMethodInvoker
    {
        private readonly IDynaBeeMethodInvoker _invoker;
        private readonly object _instance;

        public DynaBeeBoundMethodInvoker(string typeName, string methodName, IDynaBeeMethodInvoker invoker, object instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            _invoker = invoker ?? throw new ArgumentNullException(nameof(invoker));
            _instance = instance;

            if (_invoker is DynaBeeMethodInvoker dynaBeeInvoker)
                dynaBeeInvoker.ValidateTargetInstance(instance);
        }

        public Type ReturnType => _invoker.ReturnType;

        public IReadOnlyList<Type> ParameterTypes => _invoker.ParameterTypes;

        public object Invoke(IReadOnlyList<object> arguments)
            => _invoker.Invoke(_instance, arguments);
    }

    internal delegate object DynaBeeMethodDispatch(object instance, object[] arguments);

    internal static class DynaBeeMethodDispatchFactory
    {
        public static DynaBeeMethodDispatch Create(Type targetType, MethodInfo method)
        {
            var dynamicMethod = new DynamicMethod(
                $"DynaBee_Invoke_{targetType.Name}_{method.Name}",
                typeof(object),
                new[] { typeof(object), typeof(object[]) },
                targetType.Module,
                skipVisibility: true);

            var il = dynamicMethod.GetILGenerator();
            var parameters = method.GetParameters();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, targetType);

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameterType = parameters[i].ParameterType;
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldelem_Ref);
                EmitArgumentCast(il, parameterType);
            }

            il.Emit(method.IsVirtual && !method.IsFinal && !method.DeclaringType!.IsValueType ? OpCodes.Callvirt : OpCodes.Call, method);

            if (method.ReturnType == typeof(void))
            {
                il.Emit(OpCodes.Ldnull);
            }
            else if (method.ReturnType.IsValueType)
            {
                il.Emit(OpCodes.Box, method.ReturnType);
            }

            il.Emit(OpCodes.Ret);
            return (DynaBeeMethodDispatch)dynamicMethod.CreateDelegate(typeof(DynaBeeMethodDispatch));
        }

        private static void EmitArgumentCast(ILGenerator il, Type parameterType)
        {
            if (parameterType.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, parameterType);
                return;
            }

            il.Emit(OpCodes.Castclass, parameterType);
        }
    }

    internal readonly struct InvokerKey : IEquatable<InvokerKey>
    {
        private readonly string _typeName;
        private readonly string _methodName;
        private readonly string _parameterSignature;

        private InvokerKey(string typeName, string methodName, string parameterSignature)
        {
            _typeName = typeName;
            _methodName = methodName;
            _parameterSignature = parameterSignature;
        }

        public static InvokerKey Create(string typeName, string methodName, IReadOnlyList<Type> parameterTypes)
            => new(typeName, methodName, AssemblyContextInvokerExtensions.FormatTypes(parameterTypes));

        public bool Equals(InvokerKey other)
            => string.Equals(_typeName, other._typeName, StringComparison.Ordinal)
               && string.Equals(_methodName, other._methodName, StringComparison.Ordinal)
               && string.Equals(_parameterSignature, other._parameterSignature, StringComparison.Ordinal);

        public override bool Equals(object obj)
            => obj is InvokerKey other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(_typeName, _methodName, _parameterSignature);
    }
}
