namespace DynaBee.FluentApi.Invocation
{
    using System.Collections.Concurrent;
    using System.Linq.Expressions;
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
        /// Creates a typed delegate bound to a generated method target instance.
        /// </summary>
        /// <typeparam name="TDelegate">Delegate type that matches the generated method signature.</typeparam>
        /// <param name="context">Generated assembly context.</param>
        /// <param name="typeName">Generated type name.</param>
        /// <param name="instance">Target object instance.</param>
        /// <param name="methodName">Method name.</param>
        /// <param name="parameterTypes">Requested method parameter types.</param>
        /// <returns>A typed delegate bound to the target instance.</returns>
        public static TDelegate CreateBoundDelegate<TDelegate>(
            this IAssemblyContext context,
            string typeName,
            object instance,
            string methodName,
            IReadOnlyList<Type> parameterTypes)
            where TDelegate : Delegate
            => (TDelegate)context.CreateBoundDelegate(typeof(TDelegate), typeName, instance, methodName, parameterTypes);

        /// <summary>
        /// Creates a typed delegate, provided at runtime, bound to a generated method target instance.
        /// </summary>
        /// <param name="context">Generated assembly context.</param>
        /// <param name="delegateType">Delegate type that matches the generated method signature.</param>
        /// <param name="typeName">Generated type name.</param>
        /// <param name="instance">Target object instance.</param>
        /// <param name="methodName">Method name.</param>
        /// <param name="parameterTypes">Requested method parameter types.</param>
        /// <returns>A delegate instance assignable to <paramref name="delegateType"/>.</returns>
        public static object CreateBoundDelegate(
            this IAssemblyContext context,
            Type delegateType,
            string typeName,
            object instance,
            string methodName,
            IReadOnlyList<Type> parameterTypes)
        {
            var resolved = ResolveGeneratedMethod(context, typeName, methodName, parameterTypes);
            ValidateTargetInstance(context.Name, typeName, methodName, parameterTypes, resolved.TypeContext.ClrType, instance);
            return CompileBoundDelegate(context.Name, typeName, methodName, parameterTypes, resolved.TypeContext.ClrType, resolved.Method, delegateType, instance);
        }

        /// <summary>
        /// Creates a typed open-instance delegate for a generated method.
        /// </summary>
        /// <typeparam name="TDelegate">Delegate type. The first parameter must be the generated instance type or a compatible base/interface type.</typeparam>
        /// <param name="context">Generated assembly context.</param>
        /// <param name="typeName">Generated type name.</param>
        /// <param name="methodName">Method name.</param>
        /// <param name="parameterTypes">Requested method parameter types.</param>
        /// <returns>A typed open-instance delegate.</returns>
        public static TDelegate CreateOpenDelegate<TDelegate>(
            this IAssemblyContext context,
            string typeName,
            string methodName,
            IReadOnlyList<Type> parameterTypes)
            where TDelegate : Delegate
        {
            var resolved = ResolveGeneratedMethod(context, typeName, methodName, parameterTypes);
            return CompileOpenDelegate<TDelegate>(context.Name, typeName, methodName, parameterTypes, resolved.TypeContext.ClrType, resolved.Method);
        }

        /// <summary>
        /// Creates a typed factory delegate for a generated type constructor.
        /// </summary>
        /// <typeparam name="TDelegate">Delegate type whose parameters match the constructor and whose return type is assignable from the generated type.</typeparam>
        /// <param name="context">Generated assembly context.</param>
        /// <param name="typeName">Generated type name.</param>
        /// <param name="parameterTypes">Requested constructor parameter types.</param>
        /// <returns>A compiled factory delegate.</returns>
        public static TDelegate CreateFactoryDelegate<TDelegate>(
            this IAssemblyContext context,
            string typeName,
            IReadOnlyList<Type> parameterTypes)
            where TDelegate : Delegate
            => (TDelegate)context.CreateFactoryDelegate(typeof(TDelegate), typeName, parameterTypes);

        /// <summary>
        /// Creates a factory delegate, provided at runtime, for a generated type constructor.
        /// </summary>
        /// <param name="context">Generated assembly context.</param>
        /// <param name="delegateType">Delegate type whose parameters match the constructor and whose return type is assignable from the generated type.</param>
        /// <param name="typeName">Generated type name.</param>
        /// <param name="parameterTypes">Requested constructor parameter types.</param>
        /// <returns>A delegate instance assignable to <paramref name="delegateType"/>.</returns>
        public static object CreateFactoryDelegate(
            this IAssemblyContext context,
            Type delegateType,
            string typeName,
            IReadOnlyList<Type> parameterTypes)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (parameterTypes == null)
                throw new ArgumentNullException(nameof(parameterTypes));

            var typeContext = ResolveTypeContext(context, typeName, ".ctor", parameterTypes);
            var constructor = ResolveConstructor(context, typeContext.ClrType, typeName, parameterTypes);
            return CompileFactory(context.Name, typeName, parameterTypes, typeContext.ClrType, constructor, delegateType);
        }

        /// <summary>
        /// Creates a fast object-to-object adapter around a single-argument generated method.
        /// </summary>
        /// <param name="context">Generated assembly context.</param>
        /// <param name="typeName">Generated type name.</param>
        /// <param name="instance">Target object instance.</param>
        /// <param name="methodName">Method name.</param>
        /// <param name="parameterTypes">Requested method parameter types. Exactly one parameter is required.</param>
        /// <returns>An object adapter that casts its input, calls the generated method, and boxes the result when needed.</returns>
        public static Func<object, object> CreateObjectAdapter(
            this IAssemblyContext context,
            string typeName,
            object instance,
            string methodName,
            IReadOnlyList<Type> parameterTypes)
        {
            if (parameterTypes == null)
                throw new ArgumentNullException(nameof(parameterTypes));

            if (parameterTypes.Count != 1)
            {
                throw CreateResolutionException(
                    context?.Name,
                    typeName,
                    methodName,
                    parameterTypes,
                    $"Object adapter requires exactly one method parameter, but {parameterTypes.Count} were provided.");
            }

            var resolved = ResolveGeneratedMethod(context, typeName, methodName, parameterTypes);
            ValidateTargetInstance(context.Name, typeName, methodName, parameterTypes, resolved.TypeContext.ClrType, instance);

            return CompileObjectAdapter<Func<object, object>>(
                context.Name,
                typeName,
                methodName,
                parameterTypes,
                resolved.TypeContext.ClrType,
                resolved.Method,
                instance,
                Expression.Parameter(typeof(object), "argument"));
        }

        /// <summary>
        /// Creates a fast object-to-object adapter around a two-argument generated method.
        /// </summary>
        /// <param name="context">Generated assembly context.</param>
        /// <param name="typeName">Generated type name.</param>
        /// <param name="instance">Target object instance.</param>
        /// <param name="methodName">Method name.</param>
        /// <param name="parameterTypes">Requested method parameter types. Exactly two parameters are required.</param>
        /// <returns>An object adapter that casts its inputs, calls the generated method, and boxes the result when needed.</returns>
        public static Func<object, object, object> CreateObjectAdapter2(
            this IAssemblyContext context,
            string typeName,
            object instance,
            string methodName,
            IReadOnlyList<Type> parameterTypes)
        {
            ValidateObjectAdapterArity(context, typeName, methodName, parameterTypes, 2);
            var resolved = ResolveGeneratedMethod(context, typeName, methodName, parameterTypes);
            ValidateTargetInstance(context.Name, typeName, methodName, parameterTypes, resolved.TypeContext.ClrType, instance);

            return CompileObjectAdapter<Func<object, object, object>>(
                context.Name,
                typeName,
                methodName,
                parameterTypes,
                resolved.TypeContext.ClrType,
                resolved.Method,
                instance,
                Expression.Parameter(typeof(object), "argument1"),
                Expression.Parameter(typeof(object), "argument2"));
        }

        /// <summary>
        /// Creates a fast object-to-object adapter around a three-argument generated method.
        /// </summary>
        /// <param name="context">Generated assembly context.</param>
        /// <param name="typeName">Generated type name.</param>
        /// <param name="instance">Target object instance.</param>
        /// <param name="methodName">Method name.</param>
        /// <param name="parameterTypes">Requested method parameter types. Exactly three parameters are required.</param>
        /// <returns>An object adapter that casts its inputs, calls the generated method, and boxes the result when needed.</returns>
        public static Func<object, object, object, object> CreateObjectAdapter3(
            this IAssemblyContext context,
            string typeName,
            object instance,
            string methodName,
            IReadOnlyList<Type> parameterTypes)
        {
            ValidateObjectAdapterArity(context, typeName, methodName, parameterTypes, 3);
            var resolved = ResolveGeneratedMethod(context, typeName, methodName, parameterTypes);
            ValidateTargetInstance(context.Name, typeName, methodName, parameterTypes, resolved.TypeContext.ClrType, instance);

            return CompileObjectAdapter<Func<object, object, object, object>>(
                context.Name,
                typeName,
                methodName,
                parameterTypes,
                resolved.TypeContext.ClrType,
                resolved.Method,
                instance,
                Expression.Parameter(typeof(object), "argument1"),
                Expression.Parameter(typeof(object), "argument2"),
                Expression.Parameter(typeof(object), "argument3"));
        }

        /// <summary>
        /// Creates an object-based adapter for a generated method with any supported arity.
        /// </summary>
        /// <param name="context">Generated assembly context.</param>
        /// <param name="typeName">Generated type name.</param>
        /// <param name="instance">Target object instance.</param>
        /// <param name="methodName">Method name.</param>
        /// <param name="parameterTypes">Requested method parameter types.</param>
        /// <returns>An object-based method adapter.</returns>
        public static IDynaBeeObjectMethodAdapter CreateArgumentListAdapter(
            this IAssemblyContext context,
            string typeName,
            object instance,
            string methodName,
            IReadOnlyList<Type> parameterTypes)
        {
            var resolved = ResolveGeneratedMethod(context, typeName, methodName, parameterTypes);
            ValidateTargetInstance(context.Name, typeName, methodName, parameterTypes, resolved.TypeContext.ClrType, instance);
            var dispatch = CompileArgumentListAdapter(context.Name, typeName, methodName, parameterTypes, resolved.TypeContext.ClrType, resolved.Method, instance);
            return new DynaBeeObjectMethodAdapter(context.Name, typeName, methodName, parameterTypes.ToArray(), resolved.Method.ReturnType, dispatch);
        }

        /// <summary>
        /// Gets a descriptor for one generated method overload.
        /// </summary>
        /// <param name="context">Generated assembly context.</param>
        /// <param name="typeName">Generated type name.</param>
        /// <param name="methodName">Method name.</param>
        /// <param name="parameterTypes">Requested method parameter types.</param>
        /// <returns>A generated method descriptor.</returns>
        public static DynaBeeGeneratedMethodDescriptor GetGeneratedMethodDescriptor(
            this IAssemblyContext context,
            string typeName,
            string methodName,
            IReadOnlyList<Type> parameterTypes)
        {
            var resolved = ResolveGeneratedMethod(context, typeName, methodName, parameterTypes);
            return CreateDescriptor(resolved.TypeContext.ClrType, resolved.Method);
        }

        /// <summary>
        /// Gets descriptors for all public generated instance methods on a generated type.
        /// </summary>
        /// <param name="context">Generated assembly context.</param>
        /// <param name="typeName">Generated type name.</param>
        /// <returns>Generated method descriptors.</returns>
        public static IReadOnlyList<DynaBeeGeneratedMethodDescriptor> GetGeneratedMethodDescriptors(
            this IAssemblyContext context,
            string typeName)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var typeContext = ResolveTypeContext(context, typeName, "*", Array.Empty<Type>());
            return typeContext.ClrType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(x => !x.IsSpecialName)
                .Select(x => CreateDescriptor(typeContext.ClrType, x))
                .ToArray();
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

            var resolved = ResolveGeneratedMethod(context, typeName, methodName, parameterTypes);
            var dispatch = DynaBeeMethodDispatchFactory.Create(resolved.TypeContext.ClrType, resolved.Method);

            return new DynaBeeMethodInvoker(context.Name, typeName, methodName, resolved.TypeContext.ClrType, resolved.Method.ReturnType, parameterTypes.ToArray(), dispatch);
        }

        private static GeneratedMethodResolution ResolveGeneratedMethod(
            IAssemblyContext context,
            string typeName,
            string methodName,
            IReadOnlyList<Type> parameterTypes)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentException(nameof(typeName));

            if (string.IsNullOrWhiteSpace(methodName))
                throw new ArgumentException(nameof(methodName));

            if (parameterTypes == null)
                throw new ArgumentNullException(nameof(parameterTypes));

            var typeContext = ResolveTypeContext(context, typeName, methodName, parameterTypes);
            var method = ResolveMethod(context, typeContext.ClrType, typeName, methodName, parameterTypes);
            return new GeneratedMethodResolution(typeContext, method);
        }

        private static DynaBeeGeneratedMethodDescriptor CreateDescriptor(Type declaringType, MethodInfo method)
        {
            var parameterTypes = method.GetParameters().Select(x => x.ParameterType).ToArray();
            return new DynaBeeGeneratedMethodDescriptor(declaringType, method.Name, parameterTypes, method.ReturnType);
        }

        private static ConstructorInfo ResolveConstructor(
            IAssemblyContext context,
            Type generatedType,
            string typeName,
            IReadOnlyList<Type> parameterTypes)
        {
            var matches = generatedType
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .Where(constructor => ParametersMatch(constructor.GetParameters(), parameterTypes))
                .ToArray();

            if (matches.Length == 0)
                throw CreateResolutionException(context.Name, typeName, ".ctor", parameterTypes, "No constructor matches the requested parameter types.");

            if (matches.Length > 1)
                throw CreateResolutionException(context.Name, typeName, ".ctor", parameterTypes, "More than one constructor matches the requested parameter types.");

            return matches[0];
        }

        private static Delegate CompileFactory(
            string assemblyName,
            string typeName,
            IReadOnlyList<Type> parameterTypes,
            Type generatedType,
            ConstructorInfo constructor,
            Type delegateType)
        {
            var invoke = GetDelegateInvokeMethod(assemblyName, typeName, ".ctor", parameterTypes, delegateType);
            var delegateParameters = invoke.GetParameters();

            if (!ParametersMatch(delegateParameters, parameterTypes))
            {
                throw CreateResolutionException(
                    assemblyName,
                    typeName,
                    ".ctor",
                    parameterTypes,
                    $"Delegate '{delegateType}' parameters do not match the requested constructor parameter types.");
            }

            if (!invoke.ReturnType.IsAssignableFrom(generatedType))
            {
                throw CreateResolutionException(
                    assemblyName,
                    typeName,
                    ".ctor",
                    parameterTypes,
                    $"Delegate return type '{invoke.ReturnType.FullName}' is not assignable from generated type '{generatedType.FullName}'.");
            }

            var parameters = delegateParameters
                .Select(x => Expression.Parameter(x.ParameterType, x.Name ?? "arg"))
                .ToArray();
            var @new = Expression.New(constructor, parameters);
            Expression body = invoke.ReturnType == generatedType ? @new : Expression.Convert(@new, invoke.ReturnType);
            return Expression.Lambda(delegateType, body, parameters).Compile();
        }

        private static Delegate CompileBoundDelegate(
            string assemblyName,
            string typeName,
            string methodName,
            IReadOnlyList<Type> parameterTypes,
            Type generatedType,
            MethodInfo method,
            Type delegateType,
            object instance)
        {
            var invoke = GetDelegateInvokeMethod(assemblyName, typeName, methodName, parameterTypes, delegateType);
            var delegateParameters = invoke.GetParameters();

            if (!ParametersMatch(delegateParameters, parameterTypes))
            {
                throw CreateResolutionException(
                    assemblyName,
                    typeName,
                    methodName,
                    parameterTypes,
                    $"Delegate '{delegateType}' parameters do not match the requested method parameter types.");
            }

            ValidateReturnType(assemblyName, typeName, methodName, parameterTypes, $"Delegate '{delegateType}'", invoke.ReturnType, method.ReturnType);

            var parameters = delegateParameters
                .Select(x => Expression.Parameter(x.ParameterType, x.Name ?? "arg"))
                .ToArray();
            var call = Expression.Call(Expression.Constant(instance, generatedType), method, parameters);
            var body = BuildReturnBody(call, invoke.ReturnType, method.ReturnType);
            return Expression.Lambda(delegateType, body, parameters).Compile();
        }

        private static TDelegate CompileOpenDelegate<TDelegate>(
            string assemblyName,
            string typeName,
            string methodName,
            IReadOnlyList<Type> parameterTypes,
            Type generatedType,
            MethodInfo method)
            where TDelegate : Delegate
        {
            var invoke = typeof(TDelegate).GetMethod(nameof(Action.Invoke))
                ?? throw CreateResolutionException(assemblyName, typeName, methodName, parameterTypes, $"Delegate '{typeof(TDelegate)}' has no Invoke method.");
            var delegateParameters = invoke.GetParameters();

            if (delegateParameters.Length != parameterTypes.Count + 1)
            {
                throw CreateResolutionException(
                    assemblyName,
                    typeName,
                    methodName,
                    parameterTypes,
                    $"Open delegate '{typeof(TDelegate)}' must declare one target parameter plus {parameterTypes.Count} method parameter(s).");
            }

            if (!delegateParameters[0].ParameterType.IsAssignableFrom(generatedType) && delegateParameters[0].ParameterType != typeof(object))
            {
                throw CreateResolutionException(
                    assemblyName,
                    typeName,
                    methodName,
                    parameterTypes,
                    $"Open delegate target parameter '{delegateParameters[0].ParameterType.FullName}' is not compatible with generated type '{generatedType.FullName}'.");
            }

            for (var i = 0; i < parameterTypes.Count; i++)
            {
                if (delegateParameters[i + 1].ParameterType != parameterTypes[i])
                {
                    throw CreateResolutionException(
                        assemblyName,
                        typeName,
                        methodName,
                        parameterTypes,
                        $"Open delegate parameter {i + 1} must be '{parameterTypes[i].FullName}', not '{delegateParameters[i + 1].ParameterType.FullName}'.");
                }
            }

            if (invoke.ReturnType != typeof(void) && !invoke.ReturnType.IsAssignableFrom(method.ReturnType))
            {
                throw CreateResolutionException(
                    assemblyName,
                    typeName,
                    methodName,
                    parameterTypes,
                    $"Open delegate return type '{invoke.ReturnType.FullName}' is not assignable from method return type '{method.ReturnType.FullName}'.");
            }

            var parameters = delegateParameters
                .Select(x => Expression.Parameter(x.ParameterType, x.Name ?? "arg"))
                .ToArray();
            var instance = Expression.Convert(parameters[0], generatedType);
            var arguments = parameters.Skip(1).Select((parameter, index) => Expression.Convert(parameter, parameterTypes[index]));
            var call = Expression.Call(instance, method, arguments);
            Expression body = invoke.ReturnType == typeof(void)
                ? method.ReturnType == typeof(void) ? call : Expression.Block(call, Expression.Empty())
                : method.ReturnType == invoke.ReturnType ? call : Expression.Convert(call, invoke.ReturnType);

            return Expression.Lambda<TDelegate>(body, parameters).Compile();
        }

        private static TDelegate CompileObjectAdapter<TDelegate>(
            string assemblyName,
            string typeName,
            string methodName,
            IReadOnlyList<Type> parameterTypes,
            Type generatedType,
            MethodInfo method,
            object instance,
            params ParameterExpression[] parameters)
            where TDelegate : Delegate
        {
            if (parameters.Length != parameterTypes.Count)
            {
                throw CreateResolutionException(
                    assemblyName,
                    typeName,
                    methodName,
                    parameterTypes,
                    $"Object adapter requires exactly {parameters.Length} method parameter(s), but {parameterTypes.Count} were provided.");
            }

            var arguments = parameters.Select((parameter, index) => CastAdapterArgumentExpression(
                parameter,
                parameterTypes[index],
                index,
                assemblyName,
                typeName,
                methodName,
                parameterTypes));
            var call = Expression.Call(Expression.Constant(instance, generatedType), method, arguments);
            var body = BuildObjectReturnBody(call, method.ReturnType);
            return Expression.Lambda<TDelegate>(body, parameters).Compile();
        }

        private static Func<IReadOnlyList<object>, object> CompileArgumentListAdapter(
            string assemblyName,
            string typeName,
            string methodName,
            IReadOnlyList<Type> parameterTypes,
            Type generatedType,
            MethodInfo method,
            object instance)
        {
            var argumentsParameter = Expression.Parameter(typeof(IReadOnlyList<object>), "arguments");
            var indexer = typeof(IReadOnlyList<object>).GetProperty("Item")
                ?? throw CreateResolutionException(assemblyName, typeName, methodName, parameterTypes, "IReadOnlyList<object> indexer was not found.");
            var arguments = parameterTypes
                .Select((parameterType, index) => CastAdapterArgumentExpression(
                    Expression.Property(argumentsParameter, indexer, Expression.Constant(index)),
                    parameterType,
                    index,
                    assemblyName,
                    typeName,
                    methodName,
                    parameterTypes))
                .ToArray();
            var count = Expression.Property(argumentsParameter, typeof(IReadOnlyCollection<object>).GetProperty(nameof(IReadOnlyCollection<object>.Count))!);
            var countMismatch = Expression.NotEqual(count, Expression.Constant(parameterTypes.Count));
            var throwMismatch = Expression.Throw(
                Expression.New(
                    typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) })!,
                    Expression.Constant(CreateResolutionException(assemblyName, typeName, methodName, parameterTypes, $"Argument count mismatch. Expected {parameterTypes.Count}.").Message)),
                typeof(object));
            var call = Expression.Call(Expression.Constant(instance, generatedType), method, arguments);
            var body = Expression.Condition(countMismatch, throwMismatch, BuildObjectReturnBody(call, method.ReturnType));

            return Expression.Lambda<Func<IReadOnlyList<object>, object>>(body, argumentsParameter).Compile();
        }

        private static void ValidateObjectAdapterArity(
            IAssemblyContext context,
            string typeName,
            string methodName,
            IReadOnlyList<Type> parameterTypes,
            int arity)
        {
            if (parameterTypes == null)
                throw new ArgumentNullException(nameof(parameterTypes));

            if (parameterTypes.Count != arity)
            {
                throw CreateResolutionException(
                    context?.Name,
                    typeName,
                    methodName,
                    parameterTypes,
                    $"Object adapter requires exactly {arity} method parameter(s), but {parameterTypes.Count} were provided.");
            }
        }

        private static MethodInfo GetDelegateInvokeMethod(
            string assemblyName,
            string typeName,
            string methodName,
            IReadOnlyList<Type> parameterTypes,
            Type delegateType)
        {
            if (delegateType == null)
                throw new ArgumentNullException(nameof(delegateType));

            if (!typeof(Delegate).IsAssignableFrom(delegateType))
            {
                throw CreateResolutionException(
                    assemblyName,
                    typeName,
                    methodName,
                    parameterTypes,
                    $"Type '{delegateType.FullName}' is not a delegate type.");
            }

            return delegateType.GetMethod(nameof(Action.Invoke))
                ?? throw CreateResolutionException(assemblyName, typeName, methodName, parameterTypes, $"Delegate '{delegateType}' has no Invoke method.");
        }

        private static void ValidateReturnType(
            string assemblyName,
            string typeName,
            string methodName,
            IReadOnlyList<Type> parameterTypes,
            string delegateDescription,
            Type delegateReturnType,
            Type methodReturnType)
        {
            if (delegateReturnType == typeof(void))
                return;

            if (methodReturnType == typeof(void))
            {
                throw CreateResolutionException(
                    assemblyName,
                    typeName,
                    methodName,
                    parameterTypes,
                    $"{delegateDescription} expects return type '{delegateReturnType.FullName}', but method returns void.");
            }

            if (!delegateReturnType.IsAssignableFrom(methodReturnType))
            {
                throw CreateResolutionException(
                    assemblyName,
                    typeName,
                    methodName,
                    parameterTypes,
                    $"{delegateDescription} return type '{delegateReturnType.FullName}' is not assignable from method return type '{methodReturnType.FullName}'.");
            }
        }

        private static Expression BuildReturnBody(Expression call, Type delegateReturnType, Type methodReturnType)
        {
            if (delegateReturnType == typeof(void))
                return methodReturnType == typeof(void) ? call : Expression.Block(call, Expression.Empty());

            return methodReturnType == delegateReturnType ? call : Expression.Convert(call, delegateReturnType);
        }

        private static Expression BuildObjectReturnBody(Expression call, Type methodReturnType)
            => methodReturnType == typeof(void)
                ? Expression.Block(call, Expression.Constant(null, typeof(object)))
                : Expression.Convert(call, typeof(object));

        private static Expression CastAdapterArgumentExpression(
            Expression value,
            Type expectedType,
            int index,
            string assemblyName,
            string typeName,
            string methodName,
            IReadOnlyList<Type> parameterTypes)
            => Expression.Convert(
                Expression.Call(
                    typeof(AssemblyContextInvokerExtensions).GetMethod(nameof(CastAdapterArgument), BindingFlags.NonPublic | BindingFlags.Static)!,
                    value,
                    Expression.Constant(expectedType, typeof(Type)),
                    Expression.Constant(index),
                    Expression.Constant(assemblyName),
                    Expression.Constant(typeName),
                    Expression.Constant(methodName),
                    Expression.Constant(parameterTypes.ToArray(), typeof(IReadOnlyList<Type>))),
                expectedType);

        private static object CastAdapterArgument(
            object value,
            Type expectedType,
            int index,
            string assemblyName,
            string typeName,
            string methodName,
            IReadOnlyList<Type> parameterTypes)
        {
            if (CanAssign(value, expectedType))
                return value;

            var actualType = value?.GetType().FullName ?? "<null>";
            throw CreateResolutionException(
                assemblyName,
                typeName,
                methodName,
                parameterTypes,
                $"Argument {index} cannot be assigned. Expected '{expectedType.FullName}', received '{actualType}'.");
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

        private static void ValidateTargetInstance(
            string assemblyName,
            string typeName,
            string methodName,
            IReadOnlyList<Type> parameterTypes,
            Type targetType,
            object instance)
        {
            if (instance == null)
                throw CreateResolutionException(assemblyName, typeName, methodName, parameterTypes, "Target instance cannot be null.");

            if (!targetType.IsInstanceOfType(instance))
            {
                throw CreateResolutionException(
                    assemblyName,
                    typeName,
                    methodName,
                    parameterTypes,
                    $"Target instance type '{instance.GetType().FullName}' is not assignable to generated type '{targetType.FullName}'.");
            }
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

    internal sealed class DynaBeeObjectMethodAdapter : IDynaBeeObjectMethodAdapter
    {
        private readonly string _assemblyName;
        private readonly string _typeName;
        private readonly string _methodName;
        private readonly Func<IReadOnlyList<object>, object> _dispatch;

        public DynaBeeObjectMethodAdapter(
            string assemblyName,
            string typeName,
            string methodName,
            IReadOnlyList<Type> parameterTypes,
            Type returnType,
            Func<IReadOnlyList<object>, object> dispatch)
        {
            _assemblyName = assemblyName;
            _typeName = typeName;
            _methodName = methodName;
            ParameterTypes = parameterTypes ?? throw new ArgumentNullException(nameof(parameterTypes));
            ReturnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
            _dispatch = dispatch ?? throw new ArgumentNullException(nameof(dispatch));
        }

        public IReadOnlyList<Type> ParameterTypes { get; }

        public Type ReturnType { get; }

        public object Invoke(IReadOnlyList<object> arguments)
        {
            if (arguments == null)
                throw new ArgumentNullException(nameof(arguments));

            if (arguments.Count != ParameterTypes.Count)
            {
                throw AssemblyContextInvokerExtensions.CreateResolutionException(
                    _assemblyName,
                    _typeName,
                    _methodName,
                    ParameterTypes,
                    $"Argument count mismatch. Expected {ParameterTypes.Count}, received {arguments.Count}.");
            }

            try
            {
                return _dispatch(arguments);
            }
            catch (InvalidCastException ex)
            {
                throw CreateArgumentException(arguments, ex);
            }
            catch (NullReferenceException ex)
            {
                throw CreateArgumentException(arguments, ex);
            }
        }

        private InvalidOperationException CreateArgumentException(IReadOnlyList<object> arguments, Exception innerException)
        {
            for (var i = 0; i < ParameterTypes.Count; i++)
            {
                if (!CanAssign(arguments[i], ParameterTypes[i]))
                {
                    var actualType = arguments[i]?.GetType().FullName ?? "<null>";
                    return AssemblyContextInvokerExtensions.CreateResolutionException(
                        _assemblyName,
                        _typeName,
                        _methodName,
                        ParameterTypes,
                        $"Argument {i} cannot be assigned. Expected '{ParameterTypes[i].FullName}', received '{actualType}'.",
                        innerException);
                }
            }

            return AssemblyContextInvokerExtensions.CreateResolutionException(_assemblyName, _typeName, _methodName, ParameterTypes, "The adapter invocation failed.", innerException);
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
    }

    internal delegate object DynaBeeMethodDispatch(object instance, object[] arguments);

    internal readonly struct GeneratedMethodResolution
    {
        public GeneratedMethodResolution(ITypeContext typeContext, MethodInfo method)
        {
            TypeContext = typeContext ?? throw new ArgumentNullException(nameof(typeContext));
            Method = method ?? throw new ArgumentNullException(nameof(method));
        }

        public ITypeContext TypeContext { get; }

        public MethodInfo Method { get; }
    }

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
