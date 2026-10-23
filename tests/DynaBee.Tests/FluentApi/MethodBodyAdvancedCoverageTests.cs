namespace DynaBee.Tests.FluentApi;

using System.Reflection;
using System.Reflection.Emit;
using DynaBee.FluentApi;
using DynaBee.FluentApi.Body;

public sealed class MethodBodyAdvancedCoverageTests
{
    [Fact]
    public void GeneratedBodies_Execute_Advanced_Construction_Control_Flow_And_Disposal()
    {
        var token = new Token("constant");
        ValueProbe.DisposeCount = 0;
        RefProbe.DisposeCount = 0;
        AdvancedRecorder.FinallyCount = 0;

        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("Advanced", type => type
                .AddMethod("Helper", typeof(int), method => method
                    .WithParameter<int>("value")
                    .EmitsBody(body => body.Return(body.Add(body.Parameter<int>("value"), body.Constant(1)))))
                .AddMethod("CallHelper", typeof(int), method => method
                    .WithParameter<int>("value")
                    .EmitsBody(body => body.Return(body.Call(body.Self(), "Helper", new[] { typeof(int) }, body.Parameter<int>("value")))))
                .AddMethod("Construct", typeof(int), method => method
                    .WithParameter<int>("value")
                    .EmitsBody(body => body.Return(body.Property(body.New(typeof(ConstructorTarget), body.Parameter<int>("value")), nameof(ConstructorTarget.Value)))))
                .AddMethod("Constant", typeof(Token), method => method
                    .EmitsBody(body => body.Return(body.Constant(token, typeof(Token)))))
                .AddMethod("CoalesceNullable", typeof(int), method => method
                    .WithParameter<int?>("value")
                    .EmitsBody(body => body.Return(body.Coalesce(body.Parameter<int?>("value"), body.Constant(9)))))
                .AddMethod("UseValue", typeof(int), method => method
                    .WithParameter<ValueProbe>("probe")
                    .EmitsBody(body => body.Using(body.Parameter<ValueProbe>("probe"), "resource", (_, scope) => scope.Return(scope.Constant(6)))))
                .AddMethod("UseReference", typeof(int), method => method
                    .WithParameter<RefProbe>("probe")
                    .EmitsBody(body => body.Using(body.Parameter<RefProbe>("probe"), "resource", (_, scope) => scope.Return(scope.Constant(7)))))
                .AddMethod("TryFinally", typeof(int), method => method
                    .EmitsBody(body => body.Try(
                        tryBody => tryBody.Return(tryBody.Constant(8)),
                        finallyBody: finallyBody => finallyBody.Evaluate(finallyBody.StaticCall(
                            typeof(AdvancedRecorder),
                            nameof(AdvancedRecorder.RecordFinally),
                            Type.EmptyTypes)))))
                .AddMethod("SwitchWithoutDefault", typeof(string), method => method
                    .WithParameter<int>("value")
                    .EmitsBody(body =>
                    {
                        var result = body.DeclareLocal<string>("result");
                        body.Assign(result, body.Default<string>());
                        body.Switch(body.Parameter<int>("value"), branches => branches.Case(1, branch => branch.Assign(result, branch.Constant("one"))));
                        body.Return(result);
                    }))
                .AddMethod("LongArray", typeof(long), method => method
                    .WithParameter<long>("value")
                    .EmitsBody(body => CopyArrayValue<long>(body, body.Parameter<long>("value"))))
                .AddMethod("FloatArray", typeof(float), method => method
                    .WithParameter<float>("value")
                    .EmitsBody(body => CopyArrayValue<float>(body, body.Parameter<float>("value"))))
                .AddMethod("DoubleArray", typeof(double), method => method
                    .WithParameter<double>("value")
                    .EmitsBody(body => CopyArrayValue<double>(body, body.Parameter<double>("value"))))
                .AddMethod("ShortArray", typeof(short), method => method
                    .WithParameter<short>("value")
                    .EmitsBody(body => CopyArrayValue<short>(body, body.Parameter<short>("value"))))
                .AddMethod("ByteArray", typeof(byte), method => method
                    .WithParameter<byte>("value")
                    .EmitsBody(body => CopyArrayValue<byte>(body, body.Parameter<byte>("value"))))
                .AddMethod("TokenArray", typeof(Token), method => method
                    .WithParameter<Token>("value")
                    .EmitsBody(body => CopyArrayValue<Token>(body, body.Parameter<Token>("value"))))
                .AddMethod("DateArray", typeof(DateTime), method => method
                    .WithParameter<DateTime>("value")
                    .EmitsBody(body => CopyArrayValue<DateTime>(body, body.Parameter<DateTime>("value")))))
            .Build();

        var instance = context.CreateInstance("Advanced");
        var type = instance.GetType();
        var referenceProbe = new RefProbe();
        var timestamp = new DateTime(2026, 10, 23);

        Assert.Equal(5, type.GetMethod("CallHelper")!.Invoke(instance, new object[] { 4 }));
        Assert.Equal(11, type.GetMethod("Construct")!.Invoke(instance, new object[] { 11 }));
        Assert.Same(token, type.GetMethod("Constant")!.Invoke(instance, null));
        Assert.Equal(9, type.GetMethod("CoalesceNullable")!.Invoke(instance, new object[] { null! }));
        Assert.Equal(3, type.GetMethod("CoalesceNullable")!.Invoke(instance, new object[] { 3 }));
        Assert.Equal(6, type.GetMethod("UseValue")!.Invoke(instance, new object[] { new ValueProbe() }));
        Assert.Equal(1, ValueProbe.DisposeCount);
        Assert.Equal(7, type.GetMethod("UseReference")!.Invoke(instance, new object[] { referenceProbe }));
        Assert.Equal(1, RefProbe.DisposeCount);
        Assert.Equal(7, type.GetMethod("UseReference")!.Invoke(instance, new object[] { null! }));
        Assert.Equal(1, RefProbe.DisposeCount);
        Assert.Equal(8, type.GetMethod("TryFinally")!.Invoke(instance, null));
        Assert.Equal(1, AdvancedRecorder.FinallyCount);
        Assert.Equal("one", type.GetMethod("SwitchWithoutDefault")!.Invoke(instance, new object[] { 1 }));
        Assert.Null(type.GetMethod("SwitchWithoutDefault")!.Invoke(instance, new object[] { 8 }));
        Assert.Equal(7L, type.GetMethod("LongArray")!.Invoke(instance, new object[] { 7L }));
        Assert.Equal(1.5f, type.GetMethod("FloatArray")!.Invoke(instance, new object[] { 1.5f }));
        Assert.Equal(2.5d, type.GetMethod("DoubleArray")!.Invoke(instance, new object[] { 2.5d }));
        Assert.Equal((short)3, type.GetMethod("ShortArray")!.Invoke(instance, new object[] { (short)3 }));
        Assert.Equal((byte)4, type.GetMethod("ByteArray")!.Invoke(instance, new object[] { (byte)4 }));
        Assert.Same(token, type.GetMethod("TokenArray")!.Invoke(instance, new object[] { token }));
        Assert.Equal(timestamp, type.GetMethod("DateArray")!.Invoke(instance, new object[] { timestamp }));
    }

    [Fact]
    public void MethodBodyBuilder_Validates_Advanced_Expression_And_Control_Flow_Contracts()
    {
        var body = CreateBody(("number", typeof(int), 0), ("text", typeof(string), 1), ("object", typeof(object), 2));
        var number = body.Parameter<int>("number");
        var text = body.Parameter<string>("text");
        var local = body.DeclareLocal<int>("local");

        Assert.Equal("local", local.Name);
        Assert.Equal(typeof(string), body.Constant("text").Type);
        Assert.Throws<MissingMethodException>(() => body.New(typeof(ConstructorTarget), body.Constant("wrong")));
        Assert.Throws<MissingMemberException>(() => body.Index(number, body.Constant(0)));
        Assert.Throws<InvalidOperationException>(() => body.Index(body.NewArray<int>(body.Constant(1)), body.Constant("wrong")));
        Assert.Throws<InvalidOperationException>(() => body.For(_ => { }, _ => body.Constant(1), _ => { }, _ => { }));
        Assert.Throws<InvalidOperationException>(() => body.While(_ => body.Constant(1), _ => { }));
        Assert.Throws<InvalidOperationException>(() => body.Throw(body.Constant("not-an-exception")));
        Assert.Throws<InvalidOperationException>(() => body.Using(number, "resource", (_, _) => { }));
        Assert.Throws<MissingMethodException>(() => body.StaticCall(typeof(string), "Missing", Type.EmptyTypes));
        Assert.Throws<InvalidOperationException>(() => body.StaticCall(typeof(DateTime).GetMethod(nameof(DateTime.IsLeapYear))!, text));
        Assert.Throws<InvalidOperationException>(() => body.Call(text, typeof(string).GetMethod(nameof(string.IsNullOrEmpty))!));
        Assert.Throws<InvalidOperationException>(() => body.Call(number, typeof(string).GetMethod(nameof(string.ToString), Type.EmptyTypes)!));
        Assert.Throws<InvalidOperationException>(() => body.StaticCall(typeof(Enumerable).GetMethod(nameof(Enumerable.Empty))!, Array.Empty<IBeeValueExpression>()));
        Assert.Throws<InvalidOperationException>(() => body.Call(text, nameof(string.Substring), new[] { typeof(int) }));
        Assert.Throws<InvalidOperationException>(() => body.AndAlso(number, body.Constant(true)));
        Assert.Throws<InvalidOperationException>(() => body.Not(number));
        Assert.Throws<InvalidOperationException>(() => body.Coalesce(number, body.Constant(1)));
        Assert.Throws<NotSupportedException>(() => body.Subtract(text, number));

        var catches = new BeeExceptionBuilder();
        Assert.Throws<ArgumentNullException>(() => catches.Catch(null!, "error", (_, _) => { }));
        Assert.Throws<ArgumentException>(() => catches.Catch(typeof(string), "error", (_, _) => { }));
        Assert.Throws<ArgumentException>(() => catches.Catch(typeof(Exception), "", (_, _) => { }));
        Assert.Throws<ArgumentNullException>(() => catches.Catch(typeof(Exception), "error", null!));
        catches.Catch<InvalidOperationException>("error", (_, _) => { });
        Assert.Throws<InvalidOperationException>(() => catches.Catch<InvalidOperationException>("again", (_, _) => { }));

        var branches = new BeeSwitchBuilder();
        Assert.Throws<ArgumentNullException>(() => branches.Case(1, null!));
        branches.Case(1, _ => { });
        Assert.Throws<InvalidOperationException>(() => branches.Case(1, _ => { }));
        Assert.Throws<ArgumentNullException>(() => branches.Default(null!));
        branches.Default(_ => { });
        Assert.Throws<InvalidOperationException>(() => branches.Default(_ => { }));
    }

    [Fact]
    public void MethodBodyBuilder_Rejects_Null_Signatures_Ambiguity_And_Invalid_Protected_Returns()
    {
        var body = CreateBody(("value", typeof(int), 0), ("item", typeof(AmbiguousIndexer), 1));
        var voidBody = new BeeMethodBodyBuilder(
            new DynamicMethod("Void", typeof(void), Type.EmptyTypes).GetILGenerator(),
            typeof(void),
            Array.Empty<(string Name, Type Type, int ArgumentIndex)>());
        var returnBody = new BeeMethodBodyBuilder(
            new DynamicMethod("Return", typeof(int), Type.EmptyTypes).GetILGenerator(),
            typeof(int),
            Array.Empty<(string Name, Type Type, int ArgumentIndex)>());
        var abstractBase = typeof(AbstractBase).GetMethod(nameof(AbstractBase.Value))!;
        var abstractBody = new BeeMethodBodyBuilder(
            new DynamicMethod("Abstract", typeof(int), Type.EmptyTypes).GetILGenerator(),
            typeof(int),
            Array.Empty<(string Name, Type Type, int ArgumentIndex)>(),
            typeof(AbstractBase),
            baseMethod: abstractBase);

        Assert.Throws<ArgumentNullException>(() => body.New(null!, body.Constant(1)));
        Assert.Throws<ArgumentNullException>(() => body.Index(body.NewArray<int>(body.Constant(1)), null!));
        Assert.Throws<ArgumentNullException>(() => body.While(null!, _ => { }));
        Assert.Throws<ArgumentNullException>(() => body.While(_ => body.Constant(true), null!));
        Assert.Throws<ArgumentNullException>(() => body.StaticCall((Type)null!, nameof(DateTime.IsLeapYear), new[] { typeof(int) }));
        Assert.Throws<ArgumentNullException>(() => body.Property(null!, "Value"));
        Assert.Throws<ArgumentException>(() => body.Property(body.Parameter<int>("value"), ""));
        Assert.Throws<ArgumentNullException>(() => body.Constant("value", null!));
        Assert.Throws<ArgumentNullException>(() => body.Assign(body.DeclareLocal<int>("target"), null!));
        Assert.Throws<ArgumentNullException>(() => body.Using(body.New<RefProbe>(), "resource", null!));
        Assert.Throws<InvalidOperationException>(() => returnBody.Return());
        Assert.Throws<ArgumentNullException>(() => voidBody.Return(null!));
        Assert.Throws<InvalidOperationException>(() => abstractBody.CallBase());
        Assert.Throws<AmbiguousMatchException>(() => body.Index(body.Parameter<AmbiguousIndexer>("item"), body.Constant((short)1)));
        Assert.Throws<InvalidOperationException>(() => body.StaticCall(typeof(DateTime).GetMethod(nameof(DateTime.IsLeapYear))!));

    }

    [Fact]
    public void MethodBodyBuilder_Infers_The_Runtime_Type_For_Object_Constants()
    {
        var body = CreateBody();

        Assert.Equal(typeof(string), body.Constant((object)"value").Type);
        Assert.Equal(typeof(object), body.Constant((object)null!).Type);
    }

    private static void CopyArrayValue<T>(IBeeMethodBodyBuilder body, IBeeValueExpression value)
    {
        var array = body.DeclareLocal<T[]>("array");
        body.Assign(array, body.NewArray<T>(body.Constant(1)));
        body.Assign(body.Index(array, body.Constant(0)), value);
        body.Return(body.Index(array, body.Constant(0)));
    }

    private static BeeMethodBodyBuilder CreateBody(params (string Name, Type Type, int ArgumentIndex)[] parameters)
        => new(new DynamicMethod("Coverage", typeof(void), parameters.Select(parameter => parameter.Type).ToArray()).GetILGenerator(), typeof(void), parameters);

    private static string NewAssemblyName()
        => $"DynaBee.Tests.AdvancedBody.{Guid.NewGuid():N}";

    public sealed class ConstructorTarget
    {
        public ConstructorTarget(int value)
        {
            Value = value;
        }

        public int Value { get; }
    }

    public sealed class Token
    {
        public Token(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }

    public struct ValueProbe : IDisposable
    {
        public static int DisposeCount;

        public void Dispose()
            => DisposeCount++;
    }

    public sealed class RefProbe : IDisposable
    {
        public static int DisposeCount;

        public void Dispose()
            => DisposeCount++;
    }

    public static class AdvancedRecorder
    {
        public static int FinallyCount;

        public static void RecordFinally()
            => FinallyCount++;
    }

    public abstract class AbstractBase
    {
        public abstract int Value();
    }

    public sealed class AmbiguousIndexer : AmbiguousIndexerBase
    {
        public int this[long index]
        {
            get => 0;
            set { }
        }
    }

    public class AmbiguousIndexerBase
    {
        public int this[int index]
        {
            get => 0;
            set { }
        }
    }
}
