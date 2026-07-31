namespace DynaBee.FluentApi.Body
{
    using DynaBee.Infrastructure.ContextBuilders;
    using System.Collections.Concurrent;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class BeeMethodBodyBuilder : IBeeMethodBodyBuilder
    {
        private readonly ILGenerator _il;
        private readonly Type _returnType;
        private readonly Type _selfType;
        private readonly TypeContextBuilder _typeContextBuilder;
        private readonly Dictionary<string, BeeParameterExpression> _parameters;
        private readonly Dictionary<string, BeeLocalExpression> _locals = new(StringComparer.Ordinal);

        public BeeMethodBodyBuilder(
            ILGenerator il,
            Type returnType,
            IReadOnlyList<(string Name, Type Type, int ArgumentIndex)> parameters,
            Type selfType = null,
            TypeContextBuilder typeContextBuilder = null)
        {
            _il = il ?? throw new ArgumentNullException(nameof(il));
            _returnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
            _selfType = selfType;
            _typeContextBuilder = typeContextBuilder;
            _parameters = parameters.ToDictionary(
                x => x.Name,
                x => new BeeParameterExpression(x.Name, x.Type, x.ArgumentIndex),
                StringComparer.Ordinal);
        }

        public bool HasReturn { get; private set; }

        public IBeeValueExpression Self()
        {
            if (_selfType == null)
                throw new InvalidOperationException("Self is not available in static methods.");

            return new BeeSelfExpression(_selfType, _typeContextBuilder);
        }

        public IBeeParameter Parameter(string name)
        {
            if (!_parameters.TryGetValue(name, out var parameter))
                throw new KeyNotFoundException($"Parameter '{name}' was not found.");

            return parameter;
        }

        public IBeeParameter Parameter<T>(string name)
        {
            var parameter = Parameter(name);
            if (parameter.Type != typeof(T))
                throw new InvalidOperationException($"Parameter '{name}' is '{parameter.Type}', not '{typeof(T)}'.");

            return parameter;
        }

        public IBeeLocal DeclareLocal(string name, Type type)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (_locals.ContainsKey(name))
                throw new InvalidOperationException($"Local '{name}' has already been declared.");

            var local = new BeeLocalExpression(name, type, _il.DeclareLocal(type));
            _locals.Add(name, local);
            return local;
        }

        public IBeeLocal DeclareLocal<T>(string name)
            => DeclareLocal(name, typeof(T));

        public IBeeValueExpression New(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return new BeeNewExpression(type, Array.Empty<BeeValueExpression>());
        }

        public IBeeValueExpression New(Type type, params IBeeValueExpression[] arguments)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var argumentExpressions = (arguments ?? Array.Empty<IBeeValueExpression>())
                .Select(RequireExpression)
                .ToArray();

            return new BeeNewExpression(type, argumentExpressions);
        }

        public IBeeValueExpression New<T>()
            => New(typeof(T));

        public IBeeValueExpression NewArray(Type elementType, IBeeValueExpression length)
        {
            if (elementType == null)
                throw new ArgumentNullException(nameof(elementType));

            var lengthExpression = RequireExpression(length);
            if (!BeeIlConversions.CanConvert(lengthExpression.Type, typeof(int)))
                throw new InvalidOperationException($"Array length type '{lengthExpression.Type}' cannot be converted to '{typeof(int)}'.");

            return new BeeNewArrayExpression(elementType, lengthExpression);
        }

        public IBeeValueExpression NewArray<TElement>(IBeeValueExpression length)
            => NewArray(typeof(TElement), length);

        public IBeeAssignableExpression Index(IBeeValueExpression instance, IBeeValueExpression index)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            if (index == null)
                throw new ArgumentNullException(nameof(index));

            var instanceExpression = RequireExpression(instance);
            var indexExpression = RequireExpression(index);

            if (instanceExpression.Type.IsArray)
            {
                if (instanceExpression.Type.GetArrayRank() != 1)
                    throw new NotSupportedException("Only one-dimensional arrays are supported.");

                if (!BeeIlConversions.CanConvert(indexExpression.Type, typeof(int)))
                    throw new InvalidOperationException($"Array index type '{indexExpression.Type}' cannot be converted to '{typeof(int)}'.");

                return new BeeArrayIndexExpression(instanceExpression, indexExpression);
            }

            var indexer = ResolveIndexer(instanceExpression.Type, indexExpression.Type);
            return new BeeIndexerExpression(instanceExpression, indexExpression, indexer);
        }

        public IBeeValueExpression LessThan(IBeeValueExpression left, IBeeValueExpression right)
            => new BeeOrderedComparisonExpression(RequireExpression(left), RequireExpression(right), BeeOrderedComparisonKind.LessThan);

        public IBeeValueExpression LessThanOrEqual(IBeeValueExpression left, IBeeValueExpression right)
            => new BeeOrderedComparisonExpression(RequireExpression(left), RequireExpression(right), BeeOrderedComparisonKind.LessThanOrEqual);

        public IBeeValueExpression GreaterThan(IBeeValueExpression left, IBeeValueExpression right)
            => new BeeOrderedComparisonExpression(RequireExpression(left), RequireExpression(right), BeeOrderedComparisonKind.GreaterThan);

        public IBeeValueExpression GreaterThanOrEqual(IBeeValueExpression left, IBeeValueExpression right)
            => new BeeOrderedComparisonExpression(RequireExpression(left), RequireExpression(right), BeeOrderedComparisonKind.GreaterThanOrEqual);

        public IBeeMethodBodyBuilder For(
            Action<IBeeMethodBodyBuilder> initialize,
            Func<IBeeMethodBodyBuilder, IBeeValueExpression> condition,
            Action<IBeeMethodBodyBuilder> increment,
            Action<IBeeMethodBodyBuilder> body)
        {
            if (initialize == null)
                throw new ArgumentNullException(nameof(initialize));

            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            if (increment == null)
                throw new ArgumentNullException(nameof(increment));

            if (body == null)
                throw new ArgumentNullException(nameof(body));

            var startLabel = _il.DefineLabel();
            var endLabel = _il.DefineLabel();

            initialize(this);
            _il.MarkLabel(startLabel);

            var conditionExpression = RequireExpression(condition(this));
            if (conditionExpression.Type != typeof(bool))
                throw new InvalidOperationException($"Loop condition must be '{typeof(bool)}', not '{conditionExpression.Type}'.");

            conditionExpression.EmitLoad(_il);
            _il.Emit(OpCodes.Brfalse, endLabel);
            body(this);
            increment(this);
            _il.Emit(OpCodes.Br, startLabel);
            _il.MarkLabel(endLabel);

            return this;
        }

        public IBeeMethodBodyBuilder ForEach(
            IBeeValueExpression source,
            string itemName,
            Action<IBeeLocal, IBeeMethodBodyBuilder> body)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (string.IsNullOrWhiteSpace(itemName))
                throw new ArgumentException(nameof(itemName));

            if (body == null)
                throw new ArgumentNullException(nameof(body));

            var sourceExpression = RequireExpression(source);
            var itemType = ResolveEnumerableItemType(sourceExpression.Type);
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(itemType);
            var enumeratorType = typeof(IEnumerator<>).MakeGenericType(itemType);
            var getEnumerator = enumerableType.GetMethod(nameof(IEnumerable<object>.GetEnumerator))!;
            var moveNext = typeof(System.Collections.IEnumerator).GetMethod(nameof(System.Collections.IEnumerator.MoveNext))!;
            var current = enumeratorType.GetProperty(nameof(IEnumerator<object>.Current))!.GetGetMethod()!;
            var dispose = typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose))!;
            var enumerator = _il.DeclareLocal(enumeratorType);
            var item = new BeeLocalExpression(itemName, itemType, _il.DeclareLocal(itemType));
            var loopLabel = _il.DefineLabel();
            var conditionLabel = _il.DefineLabel();
            var finallyEndLabel = _il.DefineLabel();

            sourceExpression.EmitLoad(_il);
            BeeIlConversions.EmitConvert(sourceExpression.Type, enumerableType, _il);
            _il.Emit(OpCodes.Callvirt, getEnumerator);
            _il.Emit(OpCodes.Stloc, enumerator);

            _il.BeginExceptionBlock();
            _il.Emit(OpCodes.Br, conditionLabel);
            _il.MarkLabel(loopLabel);
            _il.Emit(OpCodes.Ldloc, enumerator);
            _il.Emit(OpCodes.Callvirt, current);
            _il.Emit(OpCodes.Stloc, item.LocalBuilder);
            body(item, this);
            _il.MarkLabel(conditionLabel);
            _il.Emit(OpCodes.Ldloc, enumerator);
            _il.Emit(OpCodes.Callvirt, moveNext);
            _il.Emit(OpCodes.Brtrue, loopLabel);
            _il.BeginFinallyBlock();
            _il.Emit(OpCodes.Ldloc, enumerator);
            _il.Emit(OpCodes.Brfalse, finallyEndLabel);
            _il.Emit(OpCodes.Ldloc, enumerator);
            _il.Emit(OpCodes.Callvirt, dispose);
            _il.MarkLabel(finallyEndLabel);
            _il.EndExceptionBlock();

            return this;
        }

        public IBeeValueExpression Call(IBeeValueExpression instance, MethodInfo method, params IBeeValueExpression[] arguments)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            if (method == null)
                throw new ArgumentNullException(nameof(method));

            return CreateCallExpression(RequireExpression(instance), method, isStaticCall: false, arguments);
        }

        public IBeeValueExpression Call(
            IBeeValueExpression instance,
            string methodName,
            IReadOnlyList<Type> parameterTypes,
            params IBeeValueExpression[] arguments)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            if (string.IsNullOrWhiteSpace(methodName))
                throw new ArgumentException(nameof(methodName));

            if (parameterTypes == null)
                throw new ArgumentNullException(nameof(parameterTypes));

            var expression = RequireExpression(instance);
            var method = ResolveInstanceMethod(expression, methodName, parameterTypes);
            return CreateCallExpression(expression, method, isStaticCall: false, arguments);
        }

        public IBeeValueExpression StaticCall(MethodInfo method, params IBeeValueExpression[] arguments)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            return CreateCallExpression(null, method, isStaticCall: true, arguments);
        }

        public IBeeValueExpression StaticCall(
            Type declaringType,
            string methodName,
            IReadOnlyList<Type> parameterTypes,
            params IBeeValueExpression[] arguments)
        {
            if (declaringType == null)
                throw new ArgumentNullException(nameof(declaringType));

            if (string.IsNullOrWhiteSpace(methodName))
                throw new ArgumentException(nameof(methodName));

            if (parameterTypes == null)
                throw new ArgumentNullException(nameof(parameterTypes));

            var method = ResolveStaticMethod(declaringType, methodName, parameterTypes);
            return CreateCallExpression(null, method, isStaticCall: true, arguments);
        }

        public IBeeMethodBodyBuilder Evaluate(IBeeValueExpression expression)
        {
            var value = RequireExpression(expression);
            value.EmitLoad(_il);

            if (value.Type != typeof(void))
                _il.Emit(OpCodes.Pop);

            return this;
        }

        public IBeeAssignableExpression Property(IBeeValueExpression instance, string name)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));

            if (instance is BeeSelfExpression selfExpression &&
                selfExpression.TypeContextBuilder?.TryGetProperty(name, out var dynamicProperty) == true)
            {
                return new BeeDynamicPropertyExpression(RequireExpression(instance), name, dynamicProperty);
            }

            var property = ResolveInstanceProperty(instance.Type, name)
                ?? throw new MissingMemberException(instance.Type.FullName, name);

            return new BeePropertyExpression(RequireExpression(instance), property);
        }

        public IBeeAssignableExpression StaticProperty(Type declaringType, string name)
        {
            if (declaringType == null)
                throw new ArgumentNullException(nameof(declaringType));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));

            var property = declaringType.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new MissingMemberException(declaringType.FullName, name);

            return new BeeStaticPropertyExpression(property);
        }

        public IBeeAssignableExpression Field(IBeeValueExpression instance, string name)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));

            if (instance is BeeSelfExpression selfExpression &&
                selfExpression.TypeContextBuilder?.TryGetField(name, out var dynamicField) == true)
            {
                return new BeeFieldExpression(RequireExpression(instance), dynamicField);
            }

            var field = ResolveInstanceField(instance.Type, name)
                ?? throw new MissingFieldException(instance.Type.FullName, name);

            return new BeeFieldExpression(RequireExpression(instance), field);
        }

        private static PropertyInfo ResolveInstanceProperty(Type type, string name)
        {
            try
            {
                return type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }
            catch (NotSupportedException) when (type is TypeBuilder typeBuilder)
            {
                return typeBuilder.BaseType?.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }

        private static FieldInfo ResolveInstanceField(Type type, string name)
        {
            try
            {
                return type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }
            catch (NotSupportedException) when (type is TypeBuilder typeBuilder)
            {
                return typeBuilder.BaseType?.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }

        public IBeeAssignableExpression StaticField(Type declaringType, string name)
        {
            if (declaringType == null)
                throw new ArgumentNullException(nameof(declaringType));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));

            var field = declaringType.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new MissingFieldException(declaringType.FullName, name);

            return new BeeStaticFieldExpression(field);
        }

        public IBeeValueExpression Constant(object value)
            => Constant(value, value?.GetType() ?? typeof(object));

        public IBeeValueExpression Constant(object value, Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return new BeeConstantExpression(value, type);
        }

        public IBeeValueExpression Constant<T>(T value)
            => Constant(value, typeof(T));

        public IBeeValueExpression Default(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return new BeeDefaultExpression(type);
        }

        public IBeeValueExpression Default<T>()
            => Default(typeof(T));

        public IBeeValueExpression Convert(IBeeValueExpression value, Type type)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return new BeeConvertExpression(RequireExpression(value), type);
        }

        public IBeeValueExpression Convert<T>(IBeeValueExpression value)
            => Convert(value, typeof(T));

        public IBeeValueExpression Equal(IBeeValueExpression left, IBeeValueExpression right)
            => new BeeComparisonExpression(RequireExpression(left), RequireExpression(right), equal: true);

        public IBeeValueExpression NotEqual(IBeeValueExpression left, IBeeValueExpression right)
            => new BeeComparisonExpression(RequireExpression(left), RequireExpression(right), equal: false);

        public IBeeValueExpression IsNull(IBeeValueExpression value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (Nullable.GetUnderlyingType(value.Type) != null)
                return new BeeNullableIsNullExpression(RequireExpression(value));

            if (value.Type.IsValueType)
                return Constant(false, typeof(bool));

            return Equal(value, Constant(null, value.Type));
        }

        public IBeeValueExpression Add(IBeeValueExpression left, IBeeValueExpression right)
            => new BeeAddExpression(RequireExpression(left), RequireExpression(right));

        public IBeeValueExpression Subtract(IBeeValueExpression left, IBeeValueExpression right)
            => new BeeArithmeticExpression(RequireExpression(left), RequireExpression(right), BeeArithmeticKind.Subtract);

        public IBeeValueExpression Multiply(IBeeValueExpression left, IBeeValueExpression right)
            => new BeeArithmeticExpression(RequireExpression(left), RequireExpression(right), BeeArithmeticKind.Multiply);

        public IBeeValueExpression Divide(IBeeValueExpression left, IBeeValueExpression right)
            => new BeeArithmeticExpression(RequireExpression(left), RequireExpression(right), BeeArithmeticKind.Divide);

        public IBeeValueExpression Modulo(IBeeValueExpression left, IBeeValueExpression right)
            => new BeeArithmeticExpression(RequireExpression(left), RequireExpression(right), BeeArithmeticKind.Modulo);

        public IBeeValueExpression AndAlso(IBeeValueExpression left, IBeeValueExpression right)
            => new BeeBooleanBinaryExpression(RequireExpression(left), RequireExpression(right), andAlso: true);

        public IBeeValueExpression OrElse(IBeeValueExpression left, IBeeValueExpression right)
            => new BeeBooleanBinaryExpression(RequireExpression(left), RequireExpression(right), andAlso: false);

        public IBeeValueExpression Not(IBeeValueExpression value)
            => new BeeNotExpression(RequireExpression(value));

        public IBeeValueExpression Coalesce(IBeeValueExpression value, IBeeValueExpression fallback)
            => new BeeCoalesceExpression(RequireExpression(value), RequireExpression(fallback));

        public IBeeValueExpression Concat(params IBeeValueExpression[] values)
        {
            if (values == null || values.Length == 0)
                return Constant(string.Empty, typeof(string));

            return values
                .Select(RequireExpression)
                .Aggregate((BeeValueExpression)new BeeConstantExpression(string.Empty, typeof(string)), (left, right) => new BeeConcatExpression(left, right));
        }

        public IBeeValueExpression If(IBeeValueExpression condition, IBeeValueExpression whenTrue, IBeeValueExpression whenFalse)
            => new BeeConditionalExpression(RequireExpression(condition), RequireExpression(whenTrue), RequireExpression(whenFalse));

        public IBeeMethodBodyBuilder Assign(IBeeAssignableExpression target, IBeeValueExpression value)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            RequireAssignable(target).EmitAssign(_il, RequireExpression(value));
            return this;
        }

        public IBeeMethodBodyBuilder If(
            IBeeValueExpression condition,
            Action<IBeeMethodBodyBuilder> whenTrue,
            Action<IBeeMethodBodyBuilder> whenFalse = null)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            if (whenTrue == null)
                throw new ArgumentNullException(nameof(whenTrue));

            var falseLabel = _il.DefineLabel();
            var endLabel = _il.DefineLabel();

            RequireExpression(condition).EmitLoad(_il);
            _il.Emit(OpCodes.Brfalse, falseLabel);
            whenTrue(this);
            _il.Emit(OpCodes.Br, endLabel);
            _il.MarkLabel(falseLabel);
            whenFalse?.Invoke(this);
            _il.MarkLabel(endLabel);

            return this;
        }

        public IBeeMethodBodyBuilder Return()
        {
            if (_returnType != typeof(void))
                throw new InvalidOperationException("A return value is required for non-void methods.");

            _il.Emit(OpCodes.Ret);
            HasReturn = true;
            return this;
        }

        public IBeeMethodBodyBuilder Return(IBeeValueExpression value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (_returnType == typeof(void))
                throw new InvalidOperationException("Void methods cannot return a value.");

            EmitLoadWithConversion(_il, RequireExpression(value), _returnType);
            _il.Emit(OpCodes.Ret);
            HasReturn = true;
            return this;
        }

        internal static void EmitLoadWithConversion(ILGenerator il, BeeValueExpression value, Type targetType)
        {
            value.EmitLoad(il);
            BeeIlConversions.EmitConvert(value.Type, targetType, il);
        }

        private IBeeValueExpression CreateCallExpression(
            BeeValueExpression instance,
            MethodInfo method,
            bool isStaticCall,
            IBeeValueExpression[] arguments)
        {
            ValidateMethod(method, isStaticCall);

            var argumentExpressions = (arguments ?? Array.Empty<IBeeValueExpression>())
                .Select(RequireExpression)
                .ToArray();

            ValidateCallTarget(instance, method, isStaticCall);
            ValidateCallArguments(method, argumentExpressions);

            return new BeeMethodCallExpression(instance, method, argumentExpressions);
        }

        private MethodInfo ResolveInstanceMethod(BeeValueExpression instance, string methodName, IReadOnlyList<Type> parameterTypes)
        {
            if (instance is BeeSelfExpression selfExpression &&
                selfExpression.TypeContextBuilder?.TryGetMethod(methodName, parameterTypes, out var dynamicMethod) == true)
            {
                return dynamicMethod;
            }

            return ResolveMethod(instance.Type, methodName, parameterTypes, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private static MethodInfo ResolveStaticMethod(Type declaringType, string methodName, IReadOnlyList<Type> parameterTypes)
            => ResolveMethod(declaringType, methodName, parameterTypes, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        private static PropertyInfo ResolveIndexer(Type declaringType, Type indexType)
        {
            var matches = declaringType
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.GetIndexParameters().Length == 1)
                .Where(x => BeeIlConversions.CanConvert(indexType, x.GetIndexParameters()[0].ParameterType))
                .ToArray();

            var publicMatches = matches
                .Where(x => x.GetGetMethod(false) != null || x.GetSetMethod(false) != null)
                .ToArray();

            if (publicMatches.Length > 0)
                matches = publicMatches;

            var exactMatches = matches
                .Where(x => x.GetIndexParameters()[0].ParameterType == indexType)
                .ToArray();

            if (exactMatches.Length > 0)
                matches = exactMatches;

            var defaultItemMatches = matches
                .Where(x => string.Equals(x.Name, "Item", StringComparison.Ordinal))
                .ToArray();

            if (defaultItemMatches.Length > 0)
                matches = defaultItemMatches;

            if (matches.Length == 0)
                throw new MissingMemberException(declaringType.FullName, $"indexer[{indexType}]");

            if (matches.Length > 1)
                throw new AmbiguousMatchException($"More than one indexer on '{declaringType}' accepts index type '{indexType}'.");

            return matches[0];
        }

        private static Type ResolveEnumerableItemType(Type sourceType)
        {
            if (sourceType.IsArray)
                return sourceType.GetElementType()!;

            if (sourceType.IsGenericType && sourceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return sourceType.GetGenericArguments()[0];

            var enumerable = sourceType
                .GetInterfaces()
                .Concat(new[] { sourceType })
                .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (enumerable == null)
                throw new InvalidOperationException($"Type '{sourceType}' does not implement IEnumerable<T>.");

            return enumerable.GetGenericArguments()[0];
        }

        private static MethodInfo ResolveMethod(Type declaringType, string methodName, IReadOnlyList<Type> parameterTypes, BindingFlags bindingFlags)
        {
            var matches = declaringType
                .GetMethods(bindingFlags)
                .Where(x => string.Equals(x.Name, methodName, StringComparison.Ordinal))
                .Where(x => ParametersMatch(x.GetParameters(), parameterTypes))
                .ToArray();

            if (matches.Length == 0)
                throw new MissingMethodException(declaringType.FullName, FormatMethodSignature(methodName, parameterTypes));

            if (matches.Length > 1)
                throw new AmbiguousMatchException($"More than one method matches '{FormatMethodSignature(methodName, parameterTypes)}' on '{declaringType}'.");

            return matches[0];
        }

        private static bool ParametersMatch(ParameterInfo[] parameters, IReadOnlyList<Type> parameterTypes)
        {
            if (parameters.Length != parameterTypes.Count)
                return false;

            for (var i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType != parameterTypes[i])
                    return false;
            }

            return true;
        }

        private static void ValidateMethod(MethodInfo method, bool isStaticCall)
        {
            if (method.ContainsGenericParameters)
                throw new InvalidOperationException($"Method '{method}' contains open generic parameters. Supply a closed MethodInfo.");

            if (method.IsStatic != isStaticCall)
            {
                var expected = isStaticCall ? "static" : "instance";
                throw new InvalidOperationException($"Method '{method}' is not a {expected} method.");
            }
        }

        private static void ValidateCallTarget(BeeValueExpression instance, MethodInfo method, bool isStaticCall)
        {
            if (isStaticCall)
                return;

            if (instance == null)
                throw new InvalidOperationException($"Instance method '{method}' requires a target instance.");

            var declaringType = method.DeclaringType
                ?? throw new InvalidOperationException($"Method '{method}' has no declaring type.");

            if (!declaringType.IsAssignableFrom(instance.Type) && instance is not BeeSelfExpression)
                throw new InvalidOperationException($"Method '{method}' cannot be called on instance type '{instance.Type}'.");
        }

        private static void ValidateCallArguments(MethodInfo method, IReadOnlyList<BeeValueExpression> arguments)
        {
            var parameters = method.GetParameters();
            if (parameters.Length != arguments.Count)
            {
                throw new InvalidOperationException(
                    $"Method '{method}' expects {parameters.Length} argument(s), but {arguments.Count} were provided.");
            }

            for (var i = 0; i < parameters.Length; i++)
            {
                if (!BeeIlConversions.CanConvert(arguments[i].Type, parameters[i].ParameterType))
                {
                    throw new InvalidOperationException(
                        $"Argument {i} for method '{method}' has type '{arguments[i].Type}', which cannot be assigned or converted to '{parameters[i].ParameterType}'.");
                }
            }
        }

        private static string FormatMethodSignature(string methodName, IReadOnlyList<Type> parameterTypes)
            => $"{methodName}({string.Join(", ", parameterTypes.Select(x => x.FullName ?? x.Name))})";

        private static BeeValueExpression RequireExpression(IBeeValueExpression expression)
            => expression as BeeValueExpression
               ?? throw new NotSupportedException("Custom value expression implementations are not supported.");

        private static BeeAssignableExpression RequireAssignable(IBeeAssignableExpression expression)
            => expression as BeeAssignableExpression
               ?? throw new NotSupportedException("Custom assignable expression implementations are not supported.");
    }

    internal abstract class BeeValueExpression : IBeeValueExpression
    {
        protected BeeValueExpression(Type type)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public Type Type { get; }

        public abstract void EmitLoad(ILGenerator il);
    }

    internal abstract class BeeAssignableExpression : BeeValueExpression, IBeeAssignableExpression
    {
        protected BeeAssignableExpression(Type type) : base(type)
        {
        }

        public abstract void EmitAssign(ILGenerator il, BeeValueExpression value);
    }

    internal sealed class BeeParameterExpression : BeeValueExpression, IBeeParameter
    {
        public BeeParameterExpression(string name, Type type, int argumentIndex) : base(type)
        {
            Name = name;
            ArgumentIndex = argumentIndex;
        }

        public string Name { get; }

        public int ArgumentIndex { get; }

        public override void EmitLoad(ILGenerator il)
            => il.Emit(OpCodes.Ldarg, ArgumentIndex);
    }

    internal sealed class BeeSelfExpression : BeeValueExpression
    {
        public BeeSelfExpression(Type type, TypeContextBuilder typeContextBuilder) : base(type)
        {
            TypeContextBuilder = typeContextBuilder;
        }

        public TypeContextBuilder TypeContextBuilder { get; }

        public override void EmitLoad(ILGenerator il)
            => il.Emit(OpCodes.Ldarg_0);
    }

    internal sealed class BeeLocalExpression : BeeAssignableExpression, IBeeLocal
    {
        private readonly LocalBuilder _local;

        public BeeLocalExpression(string name, Type type, LocalBuilder local) : base(type)
        {
            Name = name;
            _local = local;
        }

        public string Name { get; }

        internal LocalBuilder LocalBuilder => _local;

        public override void EmitLoad(ILGenerator il)
            => il.Emit(OpCodes.Ldloc, _local);

        public override void EmitAssign(ILGenerator il, BeeValueExpression value)
        {
            BeeMethodBodyBuilder.EmitLoadWithConversion(il, value, Type);
            il.Emit(OpCodes.Stloc, _local);
        }
    }

    internal sealed class BeeNewExpression : BeeValueExpression
    {
        private readonly ConstructorInfo _constructor;
        private readonly BeeValueExpression[] _arguments;

        public BeeNewExpression(Type type, BeeValueExpression[] arguments) : base(type)
        {
            _arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
            _constructor = ResolveConstructor(type, _arguments);
        }

        public override void EmitLoad(ILGenerator il)
        {
            var parameters = _constructor.GetParameters();
            for (var i = 0; i < _arguments.Length; i++)
                BeeMethodBodyBuilder.EmitLoadWithConversion(il, _arguments[i], parameters[i].ParameterType);

            il.Emit(OpCodes.Newobj, _constructor);
        }

        private static ConstructorInfo ResolveConstructor(Type type, BeeValueExpression[] arguments)
        {
            var matches = type
                .GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => ConstructorMatches(x.GetParameters(), arguments))
                .ToArray();

            if (matches.Length == 0)
                throw new MissingMethodException(type.FullName, FormatConstructor(arguments));

            if (matches.Length > 1)
                throw new AmbiguousMatchException($"More than one constructor on '{type}' accepts ({string.Join(", ", arguments.Select(x => x.Type))}).");

            return matches[0];
        }

        private static bool ConstructorMatches(ParameterInfo[] parameters, BeeValueExpression[] arguments)
        {
            if (parameters.Length != arguments.Length)
                return false;

            for (var i = 0; i < parameters.Length; i++)
            {
                if (!BeeIlConversions.CanConvert(arguments[i].Type, parameters[i].ParameterType))
                    return false;
            }

            return true;
        }

        private static string FormatConstructor(BeeValueExpression[] arguments)
            => $".ctor({string.Join(", ", arguments.Select(x => x.Type.FullName ?? x.Type.Name))})";
    }

    internal sealed class BeeNewArrayExpression : BeeValueExpression
    {
        private readonly Type _elementType;
        private readonly BeeValueExpression _length;

        public BeeNewArrayExpression(Type elementType, BeeValueExpression length) : base(elementType.MakeArrayType())
        {
            _elementType = elementType;
            _length = length;
        }

        public override void EmitLoad(ILGenerator il)
        {
            BeeMethodBodyBuilder.EmitLoadWithConversion(il, _length, typeof(int));
            il.Emit(OpCodes.Newarr, _elementType);
        }
    }

    internal sealed class BeePropertyExpression : BeeAssignableExpression
    {
        private readonly BeeValueExpression _instance;
        private readonly PropertyInfo _property;

        public BeePropertyExpression(BeeValueExpression instance, PropertyInfo property) : base(property.PropertyType)
        {
            _instance = instance;
            _property = property;
        }

        public override void EmitLoad(ILGenerator il)
        {
            var getter = _property.GetGetMethod(true)
                ?? throw new InvalidOperationException($"Property '{_property.Name}' has no getter.");

            _instance.EmitLoad(il);
            il.Emit(ShouldCallVirtual(getter) ? OpCodes.Callvirt : OpCodes.Call, getter);
        }

        public override void EmitAssign(ILGenerator il, BeeValueExpression value)
        {
            var setter = _property.GetSetMethod(true)
                ?? throw new InvalidOperationException($"Property '{_property.Name}' has no setter.");

            _instance.EmitLoad(il);
            BeeMethodBodyBuilder.EmitLoadWithConversion(il, value, Type);
            il.Emit(ShouldCallVirtual(setter) ? OpCodes.Callvirt : OpCodes.Call, setter);
        }

        private static bool ShouldCallVirtual(MethodInfo method)
            => method.IsVirtual && !method.IsFinal && !method.DeclaringType!.IsValueType;
    }

    internal sealed class BeeDynamicPropertyExpression : BeeAssignableExpression
    {
        private readonly BeeValueExpression _instance;
        private readonly string _name;
        private readonly DynamicPropertyAccessor _property;

        public BeeDynamicPropertyExpression(BeeValueExpression instance, string name, DynamicPropertyAccessor property) : base(property.Type)
        {
            _instance = instance;
            _name = name;
            _property = property;
        }

        public override void EmitLoad(ILGenerator il)
        {
            var getter = _property.Getter
                ?? throw new InvalidOperationException($"Property '{_name}' has no getter.");

            _instance.EmitLoad(il);
            il.Emit(ShouldCallVirtual(getter) ? OpCodes.Callvirt : OpCodes.Call, getter);
        }

        public override void EmitAssign(ILGenerator il, BeeValueExpression value)
        {
            var setter = _property.Setter
                ?? throw new InvalidOperationException($"Property '{_name}' has no setter.");

            _instance.EmitLoad(il);
            BeeMethodBodyBuilder.EmitLoadWithConversion(il, value, Type);
            il.Emit(ShouldCallVirtual(setter) ? OpCodes.Callvirt : OpCodes.Call, setter);
        }

        private static bool ShouldCallVirtual(MethodInfo method)
            => method.IsVirtual && !method.IsFinal && !method.DeclaringType!.IsValueType;
    }

    internal sealed class BeeFieldExpression : BeeAssignableExpression
    {
        private readonly BeeValueExpression _instance;
        private readonly FieldInfo _field;

        public BeeFieldExpression(BeeValueExpression instance, FieldInfo field) : base(field.FieldType)
        {
            _instance = instance;
            _field = field;
        }

        public override void EmitLoad(ILGenerator il)
        {
            _instance.EmitLoad(il);
            il.Emit(OpCodes.Ldfld, _field);
        }

        public override void EmitAssign(ILGenerator il, BeeValueExpression value)
        {
            _instance.EmitLoad(il);
            BeeMethodBodyBuilder.EmitLoadWithConversion(il, value, Type);
            il.Emit(OpCodes.Stfld, _field);
        }
    }

    internal sealed class BeeArrayIndexExpression : BeeAssignableExpression
    {
        private readonly BeeValueExpression _array;
        private readonly BeeValueExpression _index;
        private readonly Type _elementType;

        public BeeArrayIndexExpression(BeeValueExpression array, BeeValueExpression index)
            : base(array.Type.GetElementType()!)
        {
            _array = array;
            _index = index;
            _elementType = Type;
        }

        public override void EmitLoad(ILGenerator il)
        {
            _array.EmitLoad(il);
            BeeMethodBodyBuilder.EmitLoadWithConversion(il, _index, typeof(int));
            EmitLoadElement(il, _elementType);
        }

        public override void EmitAssign(ILGenerator il, BeeValueExpression value)
        {
            _array.EmitLoad(il);
            BeeMethodBodyBuilder.EmitLoadWithConversion(il, _index, typeof(int));
            BeeMethodBodyBuilder.EmitLoadWithConversion(il, value, _elementType);
            EmitStoreElement(il, _elementType);
        }

        private static void EmitLoadElement(ILGenerator il, Type elementType)
        {
            if (!elementType.IsValueType)
                il.Emit(OpCodes.Ldelem_Ref);
            else if (elementType == typeof(int)) il.Emit(OpCodes.Ldelem_I4);
            else if (elementType == typeof(long)) il.Emit(OpCodes.Ldelem_I8);
            else if (elementType == typeof(float)) il.Emit(OpCodes.Ldelem_R4);
            else if (elementType == typeof(double)) il.Emit(OpCodes.Ldelem_R8);
            else if (elementType == typeof(short)) il.Emit(OpCodes.Ldelem_I2);
            else if (elementType == typeof(byte)) il.Emit(OpCodes.Ldelem_U1);
            else il.Emit(OpCodes.Ldelem, elementType);
        }

        private static void EmitStoreElement(ILGenerator il, Type elementType)
        {
            if (!elementType.IsValueType)
                il.Emit(OpCodes.Stelem_Ref);
            else if (elementType == typeof(int)) il.Emit(OpCodes.Stelem_I4);
            else if (elementType == typeof(long)) il.Emit(OpCodes.Stelem_I8);
            else if (elementType == typeof(float)) il.Emit(OpCodes.Stelem_R4);
            else if (elementType == typeof(double)) il.Emit(OpCodes.Stelem_R8);
            else if (elementType == typeof(short)) il.Emit(OpCodes.Stelem_I2);
            else if (elementType == typeof(byte)) il.Emit(OpCodes.Stelem_I1);
            else il.Emit(OpCodes.Stelem, elementType);
        }
    }

    internal sealed class BeeIndexerExpression : BeeAssignableExpression
    {
        private readonly BeeValueExpression _instance;
        private readonly BeeValueExpression _index;
        private readonly PropertyInfo _property;
        private readonly Type _indexType;

        public BeeIndexerExpression(BeeValueExpression instance, BeeValueExpression index, PropertyInfo property)
            : base(property.PropertyType)
        {
            _instance = instance;
            _index = index;
            _property = property;
            _indexType = property.GetIndexParameters()[0].ParameterType;
        }

        public override void EmitLoad(ILGenerator il)
        {
            var getter = _property.GetGetMethod(true)
                ?? throw new InvalidOperationException($"Indexer '{_property.Name}' has no getter.");

            _instance.EmitLoad(il);
            BeeMethodBodyBuilder.EmitLoadWithConversion(il, _index, _indexType);
            il.Emit(ShouldCallVirtual(getter) ? OpCodes.Callvirt : OpCodes.Call, getter);
        }

        public override void EmitAssign(ILGenerator il, BeeValueExpression value)
        {
            var setter = _property.GetSetMethod(true)
                ?? throw new InvalidOperationException($"Indexer '{_property.Name}' has no setter.");

            _instance.EmitLoad(il);
            BeeMethodBodyBuilder.EmitLoadWithConversion(il, _index, _indexType);
            BeeMethodBodyBuilder.EmitLoadWithConversion(il, value, Type);
            il.Emit(ShouldCallVirtual(setter) ? OpCodes.Callvirt : OpCodes.Call, setter);
        }

        private static bool ShouldCallVirtual(MethodInfo method)
            => method.IsVirtual && !method.IsFinal && !method.DeclaringType!.IsValueType;
    }

    internal sealed class BeeStaticPropertyExpression : BeeAssignableExpression
    {
        private readonly PropertyInfo _property;

        public BeeStaticPropertyExpression(PropertyInfo property) : base(property.PropertyType)
        {
            _property = property;
        }

        public override void EmitLoad(ILGenerator il)
        {
            var getter = _property.GetGetMethod(true)
                ?? throw new InvalidOperationException($"Property '{_property.Name}' has no getter.");

            il.Emit(OpCodes.Call, getter);
        }

        public override void EmitAssign(ILGenerator il, BeeValueExpression value)
        {
            var setter = _property.GetSetMethod(true)
                ?? throw new InvalidOperationException($"Property '{_property.Name}' has no setter.");

            BeeMethodBodyBuilder.EmitLoadWithConversion(il, value, Type);
            il.Emit(OpCodes.Call, setter);
        }
    }

    internal sealed class BeeStaticFieldExpression : BeeAssignableExpression
    {
        private readonly FieldInfo _field;

        public BeeStaticFieldExpression(FieldInfo field) : base(field.FieldType)
        {
            _field = field;
        }

        public override void EmitLoad(ILGenerator il)
            => il.Emit(OpCodes.Ldsfld, _field);

        public override void EmitAssign(ILGenerator il, BeeValueExpression value)
        {
            BeeMethodBodyBuilder.EmitLoadWithConversion(il, value, Type);
            il.Emit(OpCodes.Stsfld, _field);
        }
    }

    internal sealed class BeeConstantExpression : BeeValueExpression
    {
        private readonly object _value;

        public BeeConstantExpression(object value, Type type) : base(type)
        {
            _value = value;
        }

        public override void EmitLoad(ILGenerator il)
        {
            if (_value == null)
            {
                if (Type.IsValueType)
                {
                    BeeDefaultExpression.EmitDefault(il, Type);
                    return;
                }

                il.Emit(OpCodes.Ldnull);
                return;
            }

            if (Type == typeof(int)) il.Emit(OpCodes.Ldc_I4, (int)_value);
            else if (Type == typeof(string)) il.Emit(OpCodes.Ldstr, (string)_value);
            else if (Type == typeof(bool)) il.Emit((bool)_value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            else if (Type == typeof(long)) il.Emit(OpCodes.Ldc_I8, (long)_value);
            else if (Type == typeof(float)) il.Emit(OpCodes.Ldc_R4, (float)_value);
            else if (Type == typeof(double)) il.Emit(OpCodes.Ldc_R8, (double)_value);
            else
            {
                var id = BeeMethodBodyConstantTable.Register(_value);
                il.Emit(OpCodes.Ldc_I4, id);
                il.Emit(OpCodes.Call, typeof(BeeMethodBodyConstantTable).GetMethod(nameof(BeeMethodBodyConstantTable.Get))!);
                BeeIlConversions.EmitConvert(typeof(object), Type, il);
            }
        }
    }

    internal sealed class BeeDefaultExpression : BeeValueExpression
    {
        public BeeDefaultExpression(Type type) : base(type)
        {
        }

        public override void EmitLoad(ILGenerator il)
            => EmitDefault(il, Type);

        public static void EmitDefault(ILGenerator il, Type type)
        {
            if (!type.IsValueType)
            {
                il.Emit(OpCodes.Ldnull);
                return;
            }

            var local = il.DeclareLocal(type);
            il.Emit(OpCodes.Ldloca_S, local);
            il.Emit(OpCodes.Initobj, type);
            il.Emit(OpCodes.Ldloc, local);
        }
    }

    internal sealed class BeeConvertExpression : BeeValueExpression
    {
        private readonly BeeValueExpression _value;

        public BeeConvertExpression(BeeValueExpression value, Type type) : base(type)
        {
            _value = value;
        }

        public override void EmitLoad(ILGenerator il)
        {
            _value.EmitLoad(il);
            BeeIlConversions.EmitConvert(_value.Type, Type, il);
        }
    }

    internal sealed class BeeMethodCallExpression : BeeValueExpression
    {
        private readonly BeeValueExpression _instance;
        private readonly MethodInfo _method;
        private readonly BeeValueExpression[] _arguments;

        public BeeMethodCallExpression(BeeValueExpression instance, MethodInfo method, BeeValueExpression[] arguments)
            : base(method.ReturnType)
        {
            _instance = instance;
            _method = method;
            _arguments = arguments;
        }

        public override void EmitLoad(ILGenerator il)
        {
            if (!_method.IsStatic)
                _instance.EmitLoad(il);

            var parameters = _method.GetParameters();
            for (var i = 0; i < _arguments.Length; i++)
                BeeMethodBodyBuilder.EmitLoadWithConversion(il, _arguments[i], parameters[i].ParameterType);

            il.Emit(ShouldCallVirtual(_method) ? OpCodes.Callvirt : OpCodes.Call, _method);
        }

        private static bool ShouldCallVirtual(MethodInfo method)
            => !method.IsStatic && method.IsVirtual && !method.IsFinal && !method.DeclaringType!.IsValueType;
    }

    internal enum BeeOrderedComparisonKind
    {
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual
    }

    internal sealed class BeeOrderedComparisonExpression : BeeValueExpression
    {
        private readonly BeeValueExpression _left;
        private readonly BeeValueExpression _right;
        private readonly BeeOrderedComparisonKind _kind;
        private readonly Type _comparisonType;

        public BeeOrderedComparisonExpression(BeeValueExpression left, BeeValueExpression right, BeeOrderedComparisonKind kind)
            : base(typeof(bool))
        {
            _left = left;
            _right = right;
            _kind = kind;
            _comparisonType = ResolveComparisonType(left.Type, right.Type);
        }

        public override void EmitLoad(ILGenerator il)
        {
            _left.EmitLoad(il);
            BeeIlConversions.EmitConvert(_left.Type, _comparisonType, il);
            _right.EmitLoad(il);
            BeeIlConversions.EmitConvert(_right.Type, _comparisonType, il);

            switch (_kind)
            {
                case BeeOrderedComparisonKind.LessThan:
                    il.Emit(OpCodes.Clt);
                    break;
                case BeeOrderedComparisonKind.GreaterThan:
                    il.Emit(OpCodes.Cgt);
                    break;
                case BeeOrderedComparisonKind.LessThanOrEqual:
                    il.Emit(OpCodes.Cgt);
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Ceq);
                    break;
                case BeeOrderedComparisonKind.GreaterThanOrEqual:
                    il.Emit(OpCodes.Clt);
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Ceq);
                    break;
                default:
                    throw new NotSupportedException($"Comparison '{_kind}' is not supported.");
            }
        }

        private static Type ResolveComparisonType(Type leftType, Type rightType)
        {
            if (!IsSupportedNumeric(leftType) || !IsSupportedNumeric(rightType))
                throw new NotSupportedException($"Ordered comparison between '{leftType}' and '{rightType}' is not supported.");

            if (leftType == typeof(double) || rightType == typeof(double))
                return typeof(double);

            if (leftType == typeof(float) || rightType == typeof(float))
                return typeof(float);

            if (leftType == typeof(long) || rightType == typeof(long))
                return typeof(long);

            return typeof(int);
        }

        private static bool IsSupportedNumeric(Type type)
            => type == typeof(byte)
               || type == typeof(short)
               || type == typeof(int)
               || type == typeof(long)
               || type == typeof(float)
               || type == typeof(double);
    }

    internal sealed class BeeComparisonExpression : BeeValueExpression
    {
        private readonly BeeValueExpression _left;
        private readonly BeeValueExpression _right;
        private readonly bool _equal;

        public BeeComparisonExpression(BeeValueExpression left, BeeValueExpression right, bool equal) : base(typeof(bool))
        {
            _left = left;
            _right = right;
            _equal = equal;
        }

        public override void EmitLoad(ILGenerator il)
        {
            _left.EmitLoad(il);
            BeeIlConversions.EmitConvert(_left.Type, _right.Type, il);
            _right.EmitLoad(il);
            il.Emit(OpCodes.Ceq);

            if (!_equal)
            {
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ceq);
            }
        }
    }

    internal sealed class BeeNullableIsNullExpression : BeeValueExpression
    {
        private readonly BeeValueExpression _value;

        public BeeNullableIsNullExpression(BeeValueExpression value) : base(typeof(bool))
        {
            _value = value;
        }

        public override void EmitLoad(ILGenerator il)
        {
            var local = il.DeclareLocal(_value.Type);
            var hasValue = _value.Type.GetProperty(nameof(Nullable<int>.HasValue))!.GetGetMethod()!;

            _value.EmitLoad(il);
            il.Emit(OpCodes.Stloc, local);
            il.Emit(OpCodes.Ldloca_S, local);
            il.Emit(OpCodes.Call, hasValue);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ceq);
        }
    }

    internal sealed class BeeAddExpression : BeeValueExpression
    {
        private readonly BeeValueExpression _left;
        private readonly BeeValueExpression _right;

        public BeeAddExpression(BeeValueExpression left, BeeValueExpression right)
            : base(left.Type == typeof(string) || right.Type == typeof(string) ? typeof(string) : left.Type)
        {
            _left = left;
            _right = right;
        }

        public override void EmitLoad(ILGenerator il)
        {
            if (Type == typeof(string))
            {
                new BeeConcatExpression(_left, _right).EmitLoad(il);
                return;
            }

            _left.EmitLoad(il);
            BeeIlConversions.EmitConvert(_left.Type, Type, il);
            _right.EmitLoad(il);
            BeeIlConversions.EmitConvert(_right.Type, Type, il);
            il.Emit(OpCodes.Add);
        }
    }

    internal enum BeeArithmeticKind
    {
        Subtract,
        Multiply,
        Divide,
        Modulo
    }

    internal sealed class BeeArithmeticExpression : BeeValueExpression
    {
        private readonly BeeValueExpression _left;
        private readonly BeeValueExpression _right;
        private readonly BeeArithmeticKind _kind;

        public BeeArithmeticExpression(BeeValueExpression left, BeeValueExpression right, BeeArithmeticKind kind)
            : base(ResolveResultType(left.Type, right.Type))
        {
            _left = left;
            _right = right;
            _kind = kind;
        }

        public override void EmitLoad(ILGenerator il)
        {
            _left.EmitLoad(il);
            BeeIlConversions.EmitConvert(_left.Type, Type, il);
            _right.EmitLoad(il);
            BeeIlConversions.EmitConvert(_right.Type, Type, il);

            switch (_kind)
            {
                case BeeArithmeticKind.Subtract:
                    il.Emit(OpCodes.Sub);
                    break;
                case BeeArithmeticKind.Multiply:
                    il.Emit(OpCodes.Mul);
                    break;
                case BeeArithmeticKind.Divide:
                    il.Emit(OpCodes.Div);
                    break;
                case BeeArithmeticKind.Modulo:
                    il.Emit(OpCodes.Rem);
                    break;
                default:
                    throw new NotSupportedException($"Arithmetic operation '{_kind}' is not supported.");
            }
        }

        private static Type ResolveResultType(Type leftType, Type rightType)
        {
            if (!IsSupportedNumeric(leftType) || !IsSupportedNumeric(rightType))
                throw new NotSupportedException($"Arithmetic operation between '{leftType}' and '{rightType}' is not supported.");

            if (leftType == typeof(double) || rightType == typeof(double))
                return typeof(double);

            if (leftType == typeof(float) || rightType == typeof(float))
                return typeof(float);

            if (leftType == typeof(long) || rightType == typeof(long))
                return typeof(long);

            return typeof(int);
        }

        private static bool IsSupportedNumeric(Type type)
            => type == typeof(byte)
               || type == typeof(short)
               || type == typeof(int)
               || type == typeof(long)
               || type == typeof(float)
               || type == typeof(double);
    }

    internal sealed class BeeBooleanBinaryExpression : BeeValueExpression
    {
        private readonly BeeValueExpression _left;
        private readonly BeeValueExpression _right;
        private readonly bool _andAlso;

        public BeeBooleanBinaryExpression(BeeValueExpression left, BeeValueExpression right, bool andAlso) : base(typeof(bool))
        {
            if (left.Type != typeof(bool) || right.Type != typeof(bool))
                throw new InvalidOperationException($"Boolean expressions require bool operands, not '{left.Type}' and '{right.Type}'.");

            _left = left;
            _right = right;
            _andAlso = andAlso;
        }

        public override void EmitLoad(ILGenerator il)
        {
            var shortCircuitLabel = il.DefineLabel();
            var endLabel = il.DefineLabel();

            _left.EmitLoad(il);
            il.Emit(_andAlso ? OpCodes.Brfalse : OpCodes.Brtrue, shortCircuitLabel);
            _right.EmitLoad(il);
            il.Emit(OpCodes.Br, endLabel);
            il.MarkLabel(shortCircuitLabel);
            il.Emit(_andAlso ? OpCodes.Ldc_I4_0 : OpCodes.Ldc_I4_1);
            il.MarkLabel(endLabel);
        }
    }

    internal sealed class BeeNotExpression : BeeValueExpression
    {
        private readonly BeeValueExpression _value;

        public BeeNotExpression(BeeValueExpression value) : base(typeof(bool))
        {
            if (value.Type != typeof(bool))
                throw new InvalidOperationException($"Boolean negation requires a bool operand, not '{value.Type}'.");

            _value = value;
        }

        public override void EmitLoad(ILGenerator il)
        {
            _value.EmitLoad(il);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ceq);
        }
    }

    internal sealed class BeeCoalesceExpression : BeeValueExpression
    {
        private readonly BeeValueExpression _value;
        private readonly BeeValueExpression _fallback;

        public BeeCoalesceExpression(BeeValueExpression value, BeeValueExpression fallback)
            : base(ResolveResultType(value.Type, fallback.Type))
        {
            _value = value;
            _fallback = fallback;
        }

        public override void EmitLoad(ILGenerator il)
        {
            var fallbackLabel = il.DefineLabel();
            var endLabel = il.DefineLabel();
            var local = il.DeclareLocal(_value.Type);
            var nullableUnderlyingType = Nullable.GetUnderlyingType(_value.Type);

            _value.EmitLoad(il);
            il.Emit(OpCodes.Stloc, local);

            if (nullableUnderlyingType != null)
            {
                var hasValue = _value.Type.GetProperty(nameof(Nullable<int>.HasValue))!.GetGetMethod()!;
                var getValueOrDefault = _value.Type.GetMethod(nameof(Nullable<int>.GetValueOrDefault), Type.EmptyTypes)!;

                il.Emit(OpCodes.Ldloca_S, local);
                il.Emit(OpCodes.Call, hasValue);
                il.Emit(OpCodes.Brfalse, fallbackLabel);
                il.Emit(OpCodes.Ldloca_S, local);
                il.Emit(OpCodes.Call, getValueOrDefault);
                BeeIlConversions.EmitConvert(nullableUnderlyingType, Type, il);
            }
            else
            {
                il.Emit(OpCodes.Ldloc, local);
                il.Emit(OpCodes.Brfalse, fallbackLabel);
                il.Emit(OpCodes.Ldloc, local);
                BeeIlConversions.EmitConvert(_value.Type, Type, il);
            }

            il.Emit(OpCodes.Br, endLabel);
            il.MarkLabel(fallbackLabel);
            BeeMethodBodyBuilder.EmitLoadWithConversion(il, _fallback, Type);
            il.MarkLabel(endLabel);
        }

        private static Type ResolveResultType(Type valueType, Type fallbackType)
        {
            var nullableUnderlyingType = Nullable.GetUnderlyingType(valueType);

            if (valueType.IsValueType && nullableUnderlyingType == null)
                throw new InvalidOperationException($"Coalesce requires a reference or nullable value, not '{valueType}'.");

            if (nullableUnderlyingType != null && BeeIlConversions.CanConvert(fallbackType, nullableUnderlyingType))
                return nullableUnderlyingType;

            if (valueType == fallbackType || valueType.IsAssignableFrom(fallbackType))
                return valueType;

            if (fallbackType.IsAssignableFrom(valueType))
                return fallbackType;

            if (BeeIlConversions.CanConvert(fallbackType, valueType))
                return valueType;

            throw new InvalidOperationException($"Coalesce fallback type '{fallbackType}' cannot be assigned or converted to '{valueType}'.");
        }
    }

    internal sealed class BeeConcatExpression : BeeValueExpression
    {
        private static readonly MethodInfo ConcatMethod = typeof(string).GetMethod(
            nameof(string.Concat),
            new[] { typeof(string), typeof(string) })!;

        private readonly BeeValueExpression _left;
        private readonly BeeValueExpression _right;

        public BeeConcatExpression(BeeValueExpression left, BeeValueExpression right) : base(typeof(string))
        {
            _left = left;
            _right = right;
        }

        public override void EmitLoad(ILGenerator il)
        {
            _left.EmitLoad(il);
            BeeIlConversions.EmitConvert(_left.Type, typeof(string), il);
            _right.EmitLoad(il);
            BeeIlConversions.EmitConvert(_right.Type, typeof(string), il);
            il.Emit(OpCodes.Call, ConcatMethod);
        }
    }

    internal sealed class BeeConditionalExpression : BeeValueExpression
    {
        private readonly BeeValueExpression _condition;
        private readonly BeeValueExpression _whenTrue;
        private readonly BeeValueExpression _whenFalse;

        public BeeConditionalExpression(BeeValueExpression condition, BeeValueExpression whenTrue, BeeValueExpression whenFalse)
            : base(whenTrue.Type)
        {
            _condition = condition;
            _whenTrue = whenTrue;
            _whenFalse = whenFalse;
        }

        public override void EmitLoad(ILGenerator il)
        {
            var falseLabel = il.DefineLabel();
            var endLabel = il.DefineLabel();

            _condition.EmitLoad(il);
            il.Emit(OpCodes.Brfalse, falseLabel);
            _whenTrue.EmitLoad(il);
            il.Emit(OpCodes.Br, endLabel);
            il.MarkLabel(falseLabel);
            BeeMethodBodyBuilder.EmitLoadWithConversion(il, _whenFalse, Type);
            il.MarkLabel(endLabel);
        }
    }

    internal static class BeeIlConversions
    {
        public static bool CanConvert(Type fromType, Type toType)
        {
            if (fromType == typeof(void) || toType == typeof(void))
                return false;

            if (fromType == toType || toType.IsAssignableFrom(fromType))
                return true;

            var fromNullable = Nullable.GetUnderlyingType(fromType);
            var toNullable = Nullable.GetUnderlyingType(toType);

            if (toNullable != null)
                return CanConvert(fromNullable ?? fromType, toNullable);

            if (fromNullable != null)
                return CanConvert(fromNullable, toType);

            if (fromType.IsEnum)
                return CanConvert(Enum.GetUnderlyingType(fromType), toType);

            if (toType.IsEnum)
                return CanConvert(fromType, Enum.GetUnderlyingType(toType));

            if (fromType == typeof(object) && toType.IsValueType)
                return true;

            if (toType == typeof(string))
                return true;

            if (!toType.IsValueType)
                return !fromType.IsValueType;

            return IsNumericConversion(fromType, toType);
        }

        public static void EmitConvert(Type fromType, Type toType, ILGenerator il)
        {
            if (fromType == toType)
                return;

            var fromNullable = Nullable.GetUnderlyingType(fromType);
            var toNullable = Nullable.GetUnderlyingType(toType);

            if (toNullable != null)
            {
                EmitToNullable(fromType, toType, toNullable, il);
                return;
            }

            if (fromNullable != null)
            {
                var getValueOrDefault = fromType.GetMethod(nameof(Nullable<int>.GetValueOrDefault), Type.EmptyTypes)
                    ?? throw new MissingMethodException(fromType.FullName, nameof(Nullable<int>.GetValueOrDefault));

                var local = il.DeclareLocal(fromType);
                il.Emit(OpCodes.Stloc, local);
                il.Emit(OpCodes.Ldloca_S, local);
                il.Emit(OpCodes.Call, getValueOrDefault);
                EmitConvert(fromNullable, toType, il);
                return;
            }

            if (fromType.IsEnum)
            {
                EmitConvert(Enum.GetUnderlyingType(fromType), toType, il);
                return;
            }

            if (toType.IsEnum)
            {
                EmitConvert(fromType, Enum.GetUnderlyingType(toType), il);
                return;
            }

            if (toType.IsAssignableFrom(fromType))
            {
                if (toType == typeof(object) && fromType.IsValueType)
                    il.Emit(OpCodes.Box, fromType);

                return;
            }

            if (fromType == typeof(object) && toType.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, toType);
                return;
            }

            if (toType == typeof(string))
            {
                if (fromType.IsValueType)
                    il.Emit(OpCodes.Box, fromType);

                il.Emit(OpCodes.Call, typeof(Convert).GetMethod(nameof(Convert.ToString), new[] { typeof(object) })!);
                return;
            }

            if (!toType.IsValueType)
            {
                il.Emit(OpCodes.Castclass, toType);
                return;
            }

            if (toType == typeof(decimal))
            {
                EmitToDecimal(fromType, il);
                return;
            }

            if (toType == typeof(int)) il.Emit(OpCodes.Conv_I4);
            else if (toType == typeof(long)) il.Emit(OpCodes.Conv_I8);
            else if (toType == typeof(float)) il.Emit(OpCodes.Conv_R4);
            else if (toType == typeof(double)) il.Emit(OpCodes.Conv_R8);
            else if (toType == typeof(short)) il.Emit(OpCodes.Conv_I2);
            else if (toType == typeof(byte)) il.Emit(OpCodes.Conv_U1);
            else if (toType == typeof(bool))
            {
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ceq);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ceq);
            }
            else
            {
                throw new NotSupportedException($"Conversion from '{fromType}' to '{toType}' is not supported.");
            }
        }

        private static bool IsNumericConversion(Type fromType, Type toType)
        {
            if (!IsNumericLike(fromType))
                return false;

            if (toType == typeof(decimal))
            {
                return fromType == typeof(byte)
                    || fromType == typeof(short)
                    || fromType == typeof(int)
                    || fromType == typeof(long)
                    || fromType == typeof(float)
                    || fromType == typeof(double);
            }

            return toType == typeof(int)
                || toType == typeof(long)
                || toType == typeof(float)
                || toType == typeof(double)
                || toType == typeof(short)
                || toType == typeof(byte)
                || toType == typeof(bool);
        }

        private static bool IsNumericLike(Type type)
            => type == typeof(byte)
               || type == typeof(short)
               || type == typeof(int)
               || type == typeof(long)
               || type == typeof(float)
               || type == typeof(double)
               || type == typeof(bool);

        private static void EmitToNullable(Type fromType, Type nullableType, Type nullableUnderlyingType, ILGenerator il)
        {
            var fromNullable = Nullable.GetUnderlyingType(fromType);

            if (fromNullable != null)
            {
                var hasValue = fromType.GetProperty(nameof(Nullable<int>.HasValue))!.GetGetMethod()!;
                var getValueOrDefault = fromType.GetMethod(nameof(Nullable<int>.GetValueOrDefault), Type.EmptyTypes)!;
                var sourceLocal = il.DeclareLocal(fromType);
                var resultLocal = il.DeclareLocal(nullableType);
                var falseLabel = il.DefineLabel();
                var endLabel = il.DefineLabel();

                il.Emit(OpCodes.Stloc, sourceLocal);
                il.Emit(OpCodes.Ldloca_S, sourceLocal);
                il.Emit(OpCodes.Call, hasValue);
                il.Emit(OpCodes.Brfalse, falseLabel);
                il.Emit(OpCodes.Ldloca_S, sourceLocal);
                il.Emit(OpCodes.Call, getValueOrDefault);
                EmitConvert(fromNullable, nullableUnderlyingType, il);
                il.Emit(OpCodes.Newobj, GetNullableConstructor(nullableType, nullableUnderlyingType));
                il.Emit(OpCodes.Stloc, resultLocal);
                il.Emit(OpCodes.Br, endLabel);
                il.MarkLabel(falseLabel);
                il.Emit(OpCodes.Ldloca_S, resultLocal);
                il.Emit(OpCodes.Initobj, nullableType);
                il.MarkLabel(endLabel);
                il.Emit(OpCodes.Ldloc, resultLocal);
                return;
            }

            EmitConvert(fromType, nullableUnderlyingType, il);
            il.Emit(OpCodes.Newobj, GetNullableConstructor(nullableType, nullableUnderlyingType));
        }

        private static ConstructorInfo GetNullableConstructor(Type nullableType, Type nullableUnderlyingType)
            => nullableType.GetConstructor(new[] { nullableUnderlyingType })
               ?? throw new MissingMethodException(nullableType.FullName, ".ctor");

        private static void EmitToDecimal(Type fromType, ILGenerator il)
        {
            Type constructorType;
            if (fromType == typeof(byte) || fromType == typeof(short) || fromType == typeof(int))
            {
                il.Emit(OpCodes.Conv_I4);
                constructorType = typeof(int);
            }
            else if (fromType == typeof(long))
            {
                constructorType = typeof(long);
            }
            else if (fromType == typeof(float))
            {
                constructorType = typeof(float);
            }
            else if (fromType == typeof(double))
            {
                constructorType = typeof(double);
            }
            else
            {
                throw new NotSupportedException($"Conversion from '{fromType}' to '{typeof(decimal)}' is not supported.");
            }

            var constructor = typeof(decimal).GetConstructor(new[] { constructorType })
                ?? throw new MissingMethodException(typeof(decimal).FullName, ".ctor");

            il.Emit(OpCodes.Newobj, constructor);
        }
    }

    internal static class BeeMethodBodyConstantTable
    {
        private static readonly ConcurrentDictionary<int, object> Constants = new();
        private static int _nextId;

        public static int Register(object value)
        {
            var id = Interlocked.Increment(ref _nextId);
            Constants[id] = value;
            return id;
        }

        public static object Get(int id)
            => Constants[id];
    }
}
