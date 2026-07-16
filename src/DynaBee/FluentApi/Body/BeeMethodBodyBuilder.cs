namespace DynaBee.FluentApi.Body
{
    using System.Collections.Concurrent;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class BeeMethodBodyBuilder : IBeeMethodBodyBuilder
    {
        private readonly ILGenerator _il;
        private readonly Type _returnType;
        private readonly Dictionary<string, BeeParameterExpression> _parameters;
        private readonly Dictionary<string, BeeLocalExpression> _locals = new(StringComparer.Ordinal);

        public BeeMethodBodyBuilder(
            ILGenerator il,
            Type returnType,
            IReadOnlyList<(string Name, Type Type, int ArgumentIndex)> parameters)
        {
            _il = il ?? throw new ArgumentNullException(nameof(il));
            _returnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
            _parameters = parameters.ToDictionary(
                x => x.Name,
                x => new BeeParameterExpression(x.Name, x.Type, x.ArgumentIndex),
                StringComparer.Ordinal);
        }

        public bool HasReturn { get; private set; }

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

            return new BeeNewExpression(type);
        }

        public IBeeValueExpression New<T>()
            => New(typeof(T));

        public IBeeAssignableExpression Property(IBeeValueExpression instance, string name)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));

            var property = instance.Type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
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

            var field = instance.Type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new MissingFieldException(instance.Type.FullName, name);

            return new BeeFieldExpression(RequireExpression(instance), field);
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

    internal sealed class BeeLocalExpression : BeeAssignableExpression, IBeeLocal
    {
        private readonly LocalBuilder _local;

        public BeeLocalExpression(string name, Type type, LocalBuilder local) : base(type)
        {
            Name = name;
            _local = local;
        }

        public string Name { get; }

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

        public BeeNewExpression(Type type) : base(type)
        {
            _constructor = type.GetConstructor(Type.EmptyTypes)
                ?? throw new MissingMethodException(type.FullName, ".ctor()");
        }

        public override void EmitLoad(ILGenerator il)
            => il.Emit(OpCodes.Newobj, _constructor);
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
