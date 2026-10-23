namespace DynaBee.Tests.FluentApi;

using System.Linq.Expressions;
using System.Reflection.Emit;
using DynaBee.FluentApi.Body;
using DynaBee.Infrastructure.Configurators;

public sealed class EmissionInfrastructureCoverageTests
{
    [Fact]
    public void IlConversions_Recognize_The_Supported_Runtime_Conversion_Matrix()
    {
        Assert.False(BeeIlConversions.CanConvert(typeof(void), typeof(int)));
        Assert.False(BeeIlConversions.CanConvert(typeof(int), typeof(void)));
        Assert.True(BeeIlConversions.CanConvert(typeof(int), typeof(int)));
        Assert.True(BeeIlConversions.CanConvert(typeof(string), typeof(object)));
        Assert.True(BeeIlConversions.CanConvert(typeof(int), typeof(long)));
        Assert.True(BeeIlConversions.CanConvert(typeof(int), typeof(decimal)));
        Assert.True(BeeIlConversions.CanConvert(typeof(int?), typeof(long?)));
        Assert.True(BeeIlConversions.CanConvert(typeof(int?), typeof(long)));
        Assert.True(BeeIlConversions.CanConvert(typeof(SampleState), typeof(int)));
        Assert.True(BeeIlConversions.CanConvert(typeof(int), typeof(SampleState)));
        Assert.True(BeeIlConversions.CanConvert(typeof(object), typeof(int)));
        Assert.True(BeeIlConversions.CanConvert(typeof(int), typeof(string)));
        Assert.True(BeeIlConversions.CanConvert(typeof(string), typeof(object)));
        Assert.False(BeeIlConversions.CanConvert(typeof(int), typeof(DateTime)));
        Assert.False(BeeIlConversions.CanConvert(typeof(int), typeof(List<int>)));
    }

    [Fact]
    public void IlConversions_Emit_Numeric_Nullable_Enum_Reference_And_String_Conversions()
    {
        Assert.Equal(7L, Convert<int, long>(7));
        Assert.Equal((short)7, Convert<int, short>(7));
        Assert.Equal((byte)7, Convert<int, byte>(7));
        Assert.Equal(7f, Convert<int, float>(7));
        Assert.Equal(7d, Convert<int, double>(7));
        Assert.Equal(7m, Convert<int, decimal>(7));
        Assert.True(Convert<int, bool>(1));
        Assert.False(Convert<int, bool>(0));
        Assert.Equal("7", Convert<int, string>(7));
        Assert.Equal("text", Convert<string, string>("text"));
        Assert.Equal(7, Convert<object, int>(7));
        Assert.Equal(7, (int)Convert<int, object>(7));
        Assert.Equal(SampleState.Ready, Convert<int, SampleState>(1));
        Assert.Equal(1, Convert<SampleState, int>(SampleState.Ready));
        Assert.Equal(7L, Convert<int?, long?>(7));
        Assert.Null(Convert<int?, long?>(null));
        Assert.Equal(7L, Convert<int?, long>(7));
        Assert.Equal(0L, Convert<int?, long>(null));
        Assert.Equal("text", Convert<object, string>("text"));
    }

    [Fact]
    public void IlConversions_Reject_Unsupported_Runtime_Conversions()
    {
        Assert.Throws<NotSupportedException>(() => Convert<char, decimal>('x'));
        Assert.Throws<NotSupportedException>(() => Convert<int, DateTime>(1));
    }

    [Fact]
    public void ExpressionEmitter_Compiles_All_Supported_Constant_And_Operator_Shapes()
    {
        Assert.Equal(5, Compile((Expression<Func<int>>)(() => 5))());
        Assert.Equal("bee", Compile((Expression<Func<string>>)(() => "bee"))());
        Assert.True(Compile((Expression<Func<bool>>)(() => true))());
        Assert.Equal(9L, Compile((Expression<Func<long>>)(() => 9L))());
        Assert.Equal(2.5f, Compile((Expression<Func<float>>)(() => 2.5f))());
        Assert.Equal(2.5d, Compile((Expression<Func<double>>)(() => 2.5d))());
        Assert.Null(Compile((Expression<Func<string>>)(() => null))());

        Assert.Equal(3, Compile((Expression<Func<int, int>>)(value => value + 1))(2));
        Assert.Equal(1, Compile((Expression<Func<int, int>>)(value => value - 1))(2));
        Assert.Equal(6, Compile((Expression<Func<int, int>>)(value => value * 3))(2));
        Assert.Equal(3, Compile((Expression<Func<int, int>>)(value => value / 2))(6));
        Assert.Equal(1, Compile((Expression<Func<int, int>>)(value => value % 2))(5));
        Assert.True(Compile((Expression<Func<int, bool>>)(value => value == 2))(2));
        Assert.True(Compile((Expression<Func<int, bool>>)(value => value != 2))(3));
        Assert.True(Compile((Expression<Func<int, bool>>)(value => value > 2))(3));
        Assert.True(Compile((Expression<Func<int, bool>>)(value => value < 4))(3));
        Assert.True(Compile((Expression<Func<int, bool>>)(value => value >= 3))(3));
        Assert.True(Compile((Expression<Func<int, bool>>)(value => value <= 3))(3));
        Assert.True(Compile((Expression<Func<bool, bool, bool>>)((left, right) => left && right))(true, true));
        Assert.False(Compile((Expression<Func<bool, bool, bool>>)((left, right) => left && right))(false, true));
        Assert.True(Compile((Expression<Func<bool, bool, bool>>)((left, right) => left || right))(false, true));
        Assert.False(Compile((Expression<Func<bool, bool, bool>>)((left, right) => left || right))(false, false));
    }

    [Fact]
    public void ExpressionEmitter_Compiles_Members_Calls_Construction_Conversions_And_Instance_Self()
    {
        ExpressionRuntime.StaticValue = 4;
        var runtime = new ExpressionRuntime(3) { Property = 5 };

        Assert.Equal(12, Compile((Expression<Func<ExpressionRuntime, int>>)(value => value.Field + value.Property + ExpressionRuntime.StaticValue))(runtime));
        Assert.Equal("BEE", Compile((Expression<Func<string, string>>)(value => value.ToUpperInvariant()))("bee"));
        Assert.Equal("value!", Compile((Expression<Func<string, string>>)(value => string.Concat(value, "!")))("value"));
        Assert.Equal(7, Compile((Expression<Func<int, ExpressionRuntime>>)(value => new ExpressionRuntime(value)))(7).Field);
        Assert.Equal("yes", Compile((Expression<Func<bool, string>>)(value => value ? "yes" : "no"))(true));
        Assert.Equal("no", Compile((Expression<Func<bool, string>>)(value => value ? "yes" : "no"))(false));
        Assert.Equal(7L, Compile((Expression<Func<int, long>>)(value => (long)value))(7));
        Assert.Equal(7, Compile((Expression<Func<object, int>>)(value => (int)value))(7));
        Assert.Equal(7, (int)Compile((Expression<Func<int, object>>)(value => (object)value))(7));

        var self = new SelfRuntime { Offset = 4 };
        Expression<Func<SelfRuntime, int, int>> expression = (instance, value) => instance.Offset + value;
        Assert.Equal(7, Compile(expression, new[] { typeof(int) }, false)(self, 3));
    }

    [Fact]
    public void ExpressionEmitter_Reports_Invalid_Signatures_And_Unsupported_Expressions()
    {
        var method = new DynamicMethod("InvalidExpression", typeof(int), new[] { typeof(int) });
        var il = method.GetILGenerator();
        var valid = (Expression<Func<int, int>>)(value => value);

        Assert.Throws<ArgumentNullException>(() => ExpressionIlEmitter.Emit(null!, il, new[] { typeof(int) }, typeof(int), true));
        Assert.Throws<ArgumentNullException>(() => ExpressionIlEmitter.Emit(valid, null!, new[] { typeof(int) }, typeof(int), true));
        Assert.Throws<InvalidOperationException>(() => ExpressionIlEmitter.Emit(valid, il, Type.EmptyTypes, typeof(int), true));
        Assert.Throws<InvalidOperationException>(() => ExpressionIlEmitter.Emit(valid, il, new[] { typeof(int), typeof(int) }, typeof(int), false));
        Assert.Throws<InvalidOperationException>(() => ExpressionIlEmitter.Emit(valid, il, new[] { typeof(int) }, typeof(string), true));
        Assert.Throws<NotSupportedException>(() => Compile((Expression<Func<int, int>>)(value => -value)));
        Assert.Throws<NotSupportedException>(() => Compile((Expression<Func<decimal>>)(() => 1m)));
        Assert.Throws<NotSupportedException>(() => Compile((Expression<Func<char, decimal>>)(value => (decimal)value)));
    }

    private static TTarget Convert<TSource, TTarget>(TSource value)
    {
        var method = new DynamicMethod("Convert", typeof(TTarget), new[] { typeof(TSource) });
        var il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        BeeIlConversions.EmitConvert(typeof(TSource), typeof(TTarget), il);
        il.Emit(OpCodes.Ret);
        return ((Func<TSource, TTarget>)method.CreateDelegate(typeof(Func<TSource, TTarget>)))(value);
    }

    private static TDelegate Compile<TDelegate>(Expression<TDelegate> expression)
        where TDelegate : Delegate
    {
        var invoke = typeof(TDelegate).GetMethod(nameof(Action.Invoke))!;
        var parameterTypes = invoke.GetParameters().Select(parameter => parameter.ParameterType).ToArray();
        var method = new DynamicMethod("Expression", invoke.ReturnType, parameterTypes);
        ExpressionIlEmitter.Emit(expression, method.GetILGenerator(), parameterTypes, invoke.ReturnType, true);
        return (TDelegate)method.CreateDelegate(typeof(TDelegate));
    }

    private static Func<SelfRuntime, int, int> Compile(
        Expression<Func<SelfRuntime, int, int>> expression,
        IReadOnlyList<Type> parameterTypes,
        bool isStatic)
    {
        var method = new DynamicMethod("ExpressionWithSelf", typeof(int), new[] { typeof(SelfRuntime), typeof(int) });
        ExpressionIlEmitter.Emit(expression, method.GetILGenerator(), parameterTypes, typeof(int), isStatic);
        return (Func<SelfRuntime, int, int>)method.CreateDelegate(typeof(Func<SelfRuntime, int, int>));
    }

    private enum SampleState
    {
        Unknown,
        Ready
    }

    public sealed class ExpressionRuntime
    {
        public ExpressionRuntime(int value)
        {
            Field = value;
        }

        public static int StaticValue;

        public int Field;

        public int Property { get; set; }
    }

    public sealed class SelfRuntime
    {
        public int Offset { get; set; }
    }
}
