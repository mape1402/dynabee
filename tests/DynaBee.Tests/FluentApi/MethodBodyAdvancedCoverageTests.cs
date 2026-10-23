namespace DynaBee.Tests.FluentApi;

using System.Reflection;
using System.Reflection.Emit;
using DynaBee.FluentApi;
using DynaBee.FluentApi.Body;
using DynaBee.Infrastructure.ContextBuilders;

public sealed class MethodBodyAdvancedCoverageTests
{
    [Fact]
    public void Body_Builder_Supports_Parameterless_Construction_Calls_And_One_Sided_Conditionals()
    {
        var toString = typeof(System.Text.StringBuilder).GetMethod(nameof(object.ToString), Type.EmptyTypes)!;
        var context = new BeeAssemblyBuilder($"DynaBee.Tests.BodyEdges.{Guid.NewGuid():N}")
            .AddClass("Edges", type => type
                .AddMethod("Text", typeof(string), method => method.EmitsBody(body =>
                    body.Return(body.Call(body.New<System.Text.StringBuilder>(), toString))))
                .AddVoidMethod("OnlyTrue", method => method
                    .WithParameter<bool>("enabled")
                    .EmitsBody(body => body.If(body.Parameter<bool>("enabled"), _ => { }))))
            .Build();
        var instance = context.CreateInstance("Edges");
        var type = instance.GetType();

        Assert.Equal(string.Empty, type.GetMethod("Text")!.Invoke(instance, null));
        type.GetMethod("OnlyTrue")!.Invoke(instance, new object[] { true });
        type.GetMethod("OnlyTrue")!.Invoke(instance, new object[] { false });
    }
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
        Assert.Throws<InvalidOperationException>(() => voidBody.Try(
            tryBody => tryBody.Evaluate(tryBody.Constant(1)),
            finallyBody: finallyBody => finallyBody.Return()));

    }

    [Fact]
    public void MethodBodyBuilder_Infers_The_Runtime_Type_For_Object_Constants()
    {
        var body = CreateBody();

        Assert.Equal(typeof(string), body.Constant((object)"value").Type);
        Assert.Equal(typeof(object), body.Constant((object)null!).Type);
    }

    [Fact]
    public void GeneratedBodies_Execute_DeferredVoidReturns_DefaultValues_And_AllDecimalConversions()
    {
        AdvancedRecorder.FinallyCount = 0;

        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("Completion", type => type
                .AddVoidMethod("ReturnFromTry", method => method
                    .EmitsBody(body => body.Try(
                        tryBody => tryBody.Return(),
                        finallyBody: finallyBody => finallyBody.Evaluate(finallyBody.StaticCall(
                            typeof(AdvancedRecorder),
                            nameof(AdvancedRecorder.RecordFinally),
                            Type.EmptyTypes)))))
                .AddMethod("DefaultDate", typeof(DateTime), method => method
                    .EmitsBody(body => body.Return(body.Default<DateTime>())))
                .AddMethod("DefaultFromNull", typeof(DateTime), method => method
                    .EmitsBody(body => body.Return(body.Constant(null!, typeof(DateTime)))))
                .AddMethod("LongToDecimal", typeof(decimal), method => method
                    .WithParameter<long>("value")
                    .EmitsBody(body => body.Return(body.Convert<decimal>(body.Parameter<long>("value")))))
                .AddMethod("FloatToDecimal", typeof(decimal), method => method
                    .WithParameter<float>("value")
                    .EmitsBody(body => body.Return(body.Convert<decimal>(body.Parameter<float>("value")))))
                .AddMethod("DoubleToDecimal", typeof(decimal), method => method
                    .WithParameter<double>("value")
                    .EmitsBody(body => body.Return(body.Convert<decimal>(body.Parameter<double>("value")))))
                .AddMethod("NoElse", typeof(int), method => method
                    .WithParameter<bool>("condition")
                    .EmitsBody(body =>
                    {
                        var result = body.DeclareLocal<int>("result");
                        body.Assign(result, body.Constant(0));
                        body.If(body.Parameter<bool>("condition"), whenTrue => whenTrue.Assign(result, whenTrue.Constant(1)));
                        body.Return(result);
                    })))
            .Build();

        var instance = context.CreateInstance("Completion");
        var type = instance.GetType();

        type.GetMethod("ReturnFromTry")!.Invoke(instance, null);
        Assert.Equal(1, AdvancedRecorder.FinallyCount);
        Assert.Equal(default(DateTime), type.GetMethod("DefaultDate")!.Invoke(instance, null));
        Assert.Equal(default(DateTime), type.GetMethod("DefaultFromNull")!.Invoke(instance, null));
        Assert.Equal(123m, type.GetMethod("LongToDecimal")!.Invoke(instance, new object[] { 123L }));
        Assert.Equal(1.25m, type.GetMethod("FloatToDecimal")!.Invoke(instance, new object[] { 1.25f }));
        Assert.Equal(2.5m, type.GetMethod("DoubleToDecimal")!.Invoke(instance, new object[] { 2.5d }));
        Assert.Equal(1, type.GetMethod("NoElse")!.Invoke(instance, new object[] { true }));
        Assert.Equal(0, type.GetMethod("NoElse")!.Invoke(instance, new object[] { false }));
    }

    [Fact]
    public void MethodBodyBuilder_Rejects_Unsupported_Extensibility_And_Invalid_Call_Shapes()
    {
        var body = CreateBody(("value", typeof(int), 0), ("text", typeof(string), 1));
        var genericDefinition = typeof(Enumerable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(method => method.Name == nameof(Enumerable.Empty) && method.IsGenericMethodDefinition);
        var instanceMethod = typeof(string).GetMethod(nameof(string.ToString), Type.EmptyTypes)!;
        var staticMethod = typeof(DateTime).GetMethod(nameof(DateTime.IsLeapYear))!;

        Assert.Throws<ArgumentNullException>(() => body.Convert(body.Constant(1), null!));
        Assert.Throws<NotSupportedException>(() => body.Evaluate(new ExternalValueExpression()));
        Assert.Throws<NotSupportedException>(() => body.Assign(new ExternalAssignableExpression(), body.Constant(1)));
        Assert.Throws<NotSupportedException>(() => body.Concat(body.Constant("value"), new ExternalValueExpression()));
        Assert.Throws<InvalidOperationException>(() => body.StaticCall(genericDefinition));
        Assert.Throws<InvalidOperationException>(() => body.StaticCall(instanceMethod));
        Assert.Throws<InvalidOperationException>(() => body.Call(body.Parameter<string>("text"), staticMethod, body.Constant(2024)));
        Assert.Throws<InvalidOperationException>(() => body.Call(body.Parameter<int>("value"), instanceMethod));
        Assert.Equal(typeof(string), body.Call(body.Parameter<int>("value"), nameof(int.ToString), Type.EmptyTypes).Type);
        Assert.Throws<NotSupportedException>(() => body.LessThan(body.Parameter<string>("text"), body.Constant("other")));
        Assert.Equal(typeof(string), body.Add(body.Parameter<string>("text"), body.Constant(new object())).Type);
        Assert.Throws<InvalidOperationException>(() => body.Coalesce(
            body.Constant(null!, typeof(Stream)),
            body.Constant(10)));
    }

    [Fact]
    public void MethodBodyBuilder_Resolves_Coalesce_Result_Types_And_Null_Argument_Collections()
    {
        var body = CreateBody(("value", typeof(string), 0));
        var genericParameter = typeof(List<>).GetGenericArguments()[0];

        Assert.Equal(typeof(RefProbe), body.New(typeof(RefProbe), null!).Type);
        Assert.Equal(typeof(string), body.Concat(null!).Type);
        Assert.Equal(typeof(object), body.Coalesce(body.Parameter<string>("value"), body.Constant(null!, typeof(object))).Type);
        Assert.Equal(typeof(string), body.Coalesce(body.Parameter<string>("value"), body.Constant(10)).Type);
        Assert.Throws<MissingMethodException>(() => body.StaticCall(typeof(Convert), "Missing", new[] { genericParameter }));
    }

    [Fact]
    public void MethodBodyBuilder_Handles_Null_Params_And_Rejects_Value_Returns_From_Finally()
    {
        var body = CreateBody(("text", typeof(string), 0));
        var emptyArguments = Array.Empty<IBeeValueExpression>();
        var valueReturn = new BeeMethodBodyBuilder(
            new DynamicMethod("FinallyReturn", typeof(int), Type.EmptyTypes).GetILGenerator(),
            typeof(int),
            Array.Empty<(string Name, Type Type, int ArgumentIndex)>());

        Assert.Equal(typeof(RefProbe), body.New(typeof(RefProbe), (IBeeValueExpression[])null!).Type);
        Assert.Equal(typeof(RefProbe), body.New(typeof(RefProbe), emptyArguments).Type);
        Assert.Equal(typeof(string), body.Call(
            body.Parameter<string>("text"),
            typeof(string).GetMethod(nameof(string.ToString), Type.EmptyTypes)!,
            (IBeeValueExpression[])null!).Type);
        Assert.Equal(typeof(string), body.Call(
            body.Parameter<string>("text"),
            typeof(string).GetMethod(nameof(string.ToString), Type.EmptyTypes)!,
            emptyArguments).Type);
        Assert.Equal(typeof(Guid), body.StaticCall(
            typeof(Guid).GetMethod(nameof(Guid.NewGuid), Type.EmptyTypes)!,
            (IBeeValueExpression[])null!).Type);
        Assert.Equal(typeof(Guid), body.StaticCall(
            typeof(Guid).GetMethod(nameof(Guid.NewGuid), Type.EmptyTypes)!,
            emptyArguments).Type);
        Assert.Equal(typeof(string), body.Concat((IBeeValueExpression[])null!).Type);
        Assert.Equal(typeof(string), body.Concat(body.Constant("only")).Type);
        Assert.Throws<NotSupportedException>(() => body.New(typeof(RefProbe), new ExternalValueExpression()));
        Assert.Throws<NotSupportedException>(() => body.Call(
            body.Parameter<string>("text"),
            typeof(string).GetMethod(nameof(string.ToString), Type.EmptyTypes)!,
            new ExternalValueExpression()));
        Assert.Throws<NotSupportedException>(() => body.StaticCall(
            typeof(Guid).GetMethod(nameof(Guid.NewGuid), Type.EmptyTypes)!,
            new ExternalValueExpression()));

        Assert.Throws<InvalidOperationException>(() => valueReturn.Try(
            tryBody => tryBody.Evaluate(tryBody.Constant(1)),
            finallyBody: finallyBody => finallyBody.Return(finallyBody.Constant(2))));
    }

    [Fact]
    public void MethodBodyBuilder_Reports_Invalid_Internal_Call_Targets_And_Nullable_Metadata()
    {
        var validateCallTarget = typeof(BeeMethodBodyBuilder).GetMethod(
            "ValidateCallTarget",
            BindingFlags.NonPublic | BindingFlags.Static)!;
        var nullableConstructor = typeof(BeeIlConversions).GetMethod(
            "GetNullableConstructor",
            BindingFlags.NonPublic | BindingFlags.Static)!;
        var instanceMethod = typeof(string).GetMethod(nameof(string.ToString), Type.EmptyTypes)!;
        var unboundMethod = new DynamicMethod("Unbound", typeof(void), Type.EmptyTypes);
        var instance = new BeeParameterExpression("instance", typeof(object), 0);

        var missingInstance = Assert.Throws<TargetInvocationException>(() =>
            validateCallTarget.Invoke(null, new object[] { null!, instanceMethod, false }));
        var missingDeclaringType = Assert.Throws<TargetInvocationException>(() =>
            validateCallTarget.Invoke(null, new object[] { instance, unboundMethod, false }));
        var missingConstructor = Assert.Throws<TargetInvocationException>(() =>
            nullableConstructor.Invoke(null, new object[] { typeof(Guid), typeof(int) }));

        Assert.IsType<InvalidOperationException>(missingInstance.InnerException);
        Assert.IsType<InvalidOperationException>(missingDeclaringType.InnerException);
        Assert.IsType<MissingMethodException>(missingConstructor.InnerException);
    }

    [Fact]
    public void GeneratedBodies_Use_Foreach_Break_And_Null_Base_Call_Arguments()
    {
        var baseMethod = typeof(BaseCallTarget).GetMethod(nameof(BaseCallTarget.GetValue))!;
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("ControlFlow", type => type
                .AddMethod("FirstOrZero", typeof(int), method => method
                    .WithParameter<IEnumerable<int>>("values")
                    .EmitsBody(body =>
                    {
                        var result = body.DeclareLocal<int>("result");
                        body.Assign(result, body.Constant(0));
                        body.ForEach(body.Parameter<IEnumerable<int>>("values"), "item", (item, loop) =>
                        {
                            loop.Assign(result, item);
                            loop.Break();
                        });
                        body.Return(result);
                    })))
            .AddClass("Derived", type => type
                .Inherits<BaseCallTarget>()
                .OverrideMethod(baseMethod, method => method.EmitsBody(body => body.Return(body.CallBase(null!)))))
            .Build();

        var controlFlow = context.CreateInstance("ControlFlow");

        Assert.Equal(0, controlFlow.GetType().GetMethod("FirstOrZero")!.Invoke(controlFlow, new object[] { Array.Empty<int>() }));
        Assert.Equal(4, controlFlow.GetType().GetMethod("FirstOrZero")!.Invoke(controlFlow, new object[] { new[] { 4, 5 } }));
        Assert.Equal(6, ((BaseCallTarget)context.CreateInstance("Derived")).GetValue());
    }

    [Fact]
    public void MethodBodyBuilder_Requires_An_Il_Generator_And_Return_Type()
    {
        var il = new DynamicMethod("Coverage", typeof(void), Type.EmptyTypes).GetILGenerator();
        var parameters = Array.Empty<(string Name, Type Type, int ArgumentIndex)>();

        Assert.Throws<ArgumentNullException>(() => new BeeMethodBodyBuilder(null!, typeof(void), parameters));
        Assert.Throws<ArgumentNullException>(() => new BeeMethodBodyBuilder(il, null!, parameters));
    }

    [Fact]
    public void Body_Expressions_Reject_Missing_Property_And_Indexer_Accessors_Before_Emission()
    {
        var il = new DynamicMethod("Accessors", typeof(void), Type.EmptyTypes).GetILGenerator();
        var instance = new BeeParameterExpression("value", typeof(AccessorTarget), 0);
        var getterOnly = new BeePropertyExpression(instance, typeof(AccessorTarget).GetProperty(nameof(AccessorTarget.GetterOnly))!);
        var setterOnly = new BeePropertyExpression(instance, typeof(AccessorTarget).GetProperty(nameof(AccessorTarget.SetterOnly))!);
        var staticGetterOnly = new BeeStaticPropertyExpression(typeof(AccessorTarget).GetProperty(nameof(AccessorTarget.StaticGetterOnly))!);
        var staticSetterOnly = new BeeStaticPropertyExpression(typeof(AccessorTarget).GetProperty(nameof(AccessorTarget.StaticSetterOnly))!);
        var indexerGetterOnly = new BeeIndexerExpression(instance, new BeeConstantExpression(0, typeof(int)), typeof(AccessorTarget).GetProperty("Item", new[] { typeof(int) })!);
        var indexerSetterOnly = new BeeIndexerExpression(instance, new BeeConstantExpression("key", typeof(string)), typeof(AccessorTarget).GetProperty("Item", new[] { typeof(string) })!);
        var dynamicGetterOnly = new BeeDynamicPropertyExpression(instance, "Dynamic", new DynamicPropertyAccessor(typeof(int), null!, null!));

        Assert.Throws<InvalidOperationException>(() => getterOnly.EmitAssign(il, new BeeConstantExpression(1, typeof(int))));
        Assert.Throws<InvalidOperationException>(() => setterOnly.EmitLoad(il));
        Assert.Throws<InvalidOperationException>(() => staticGetterOnly.EmitAssign(il, new BeeConstantExpression(1, typeof(int))));
        Assert.Throws<InvalidOperationException>(() => staticSetterOnly.EmitLoad(il));
        Assert.Throws<InvalidOperationException>(() => indexerGetterOnly.EmitAssign(il, new BeeConstantExpression(1, typeof(int))));
        Assert.Throws<InvalidOperationException>(() => indexerSetterOnly.EmitLoad(il));
        Assert.Throws<InvalidOperationException>(() => dynamicGetterOnly.EmitLoad(il));
        Assert.Throws<InvalidOperationException>(() => dynamicGetterOnly.EmitAssign(il, new BeeConstantExpression(1, typeof(int))));
    }

    [Fact]
    public void Il_Conversion_Compatibility_Covers_Nullables_Enums_References_And_Numerics()
    {
        Assert.False(BeeIlConversions.CanConvert(typeof(void), typeof(int)));
        Assert.True(BeeIlConversions.CanConvert(typeof(int), typeof(int)));
        Assert.True(BeeIlConversions.CanConvert(typeof(string), typeof(object)));
        Assert.True(BeeIlConversions.CanConvert(typeof(int), typeof(int?)));
        Assert.True(BeeIlConversions.CanConvert(typeof(int?), typeof(long)));
        Assert.True(BeeIlConversions.CanConvert(typeof(ConversionKind), typeof(int)));
        Assert.True(BeeIlConversions.CanConvert(typeof(int), typeof(ConversionKind)));
        Assert.True(BeeIlConversions.CanConvert(typeof(object), typeof(Guid)));
        Assert.True(BeeIlConversions.CanConvert(typeof(Guid), typeof(string)));
        Assert.True(BeeIlConversions.CanConvert(typeof(string), typeof(IDisposable)));
        Assert.False(BeeIlConversions.CanConvert(typeof(int), typeof(Guid)));
        Assert.False(BeeIlConversions.CanConvert(typeof(DateTime), typeof(decimal)));
    }

    [Fact]
    public void Value_Expressions_Emit_Native_Table_And_NonVirtual_Member_Loads()
    {
        var il = new DynamicMethod("Expressions", typeof(void), new[] { typeof(ExpressionEmissionTarget) }).GetILGenerator();
        var instance = new BeeParameterExpression("target", typeof(ExpressionEmissionTarget), 0);
        var property = typeof(ExpressionEmissionTarget).GetProperty(nameof(ExpressionEmissionTarget.NonVirtual))!;
        var indexer = typeof(ExpressionEmissionTarget).GetProperty("Item")!;

        new BeeConstantExpression(null!, typeof(int)).EmitLoad(il);
        new BeeConstantExpression(null!, typeof(string)).EmitLoad(il);
        new BeeConstantExpression(1, typeof(int)).EmitLoad(il);
        new BeeConstantExpression("text", typeof(string)).EmitLoad(il);
        new BeeConstantExpression(true, typeof(bool)).EmitLoad(il);
        new BeeConstantExpression(false, typeof(bool)).EmitLoad(il);
        new BeeConstantExpression(1L, typeof(long)).EmitLoad(il);
        new BeeConstantExpression(1f, typeof(float)).EmitLoad(il);
        new BeeConstantExpression(1d, typeof(double)).EmitLoad(il);
        new BeeConstantExpression(Guid.Empty, typeof(Guid)).EmitLoad(il);
        new BeeNewExpression(typeof(ExpressionEmissionTarget), Array.Empty<BeeValueExpression>()).EmitLoad(il);
        new BeePropertyExpression(instance, property).EmitLoad(il);
        new BeeIndexerExpression(instance, new BeeConstantExpression(0, typeof(int)), indexer).EmitLoad(il);
    }

    [Fact]
    public void GeneratedBodies_Dispatch_Virtual_Properties_Indexers_And_Overrides()
    {
        var baseProperty = typeof(VirtualPropertyBase).GetProperty(nameof(VirtualPropertyBase.Value))!;
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("VirtualMembers", type => type
                .AddMethod("Update", typeof(int), method => method
                    .WithParameter<VirtualMemberTarget>("target")
                    .EmitsBody(body =>
                    {
                        var target = body.Parameter<VirtualMemberTarget>("target");
                        body.Assign(body.Property(target, nameof(VirtualMemberTarget.Value)), body.Constant(4));
                        body.Assign(body.Index(target, body.Constant(0)), body.Constant(5));
                        body.Return(body.Add(
                            body.Property(target, nameof(VirtualMemberTarget.Value)),
                            body.Index(target, body.Constant(0))));
                    })))
            .AddClass("OverriddenProperty", type => type
                .Inherits<VirtualPropertyBase>()
                .OverrideProperty(baseProperty, property => property
                    .Getter(getter => getter.EmitsBody(body => body.Return(body.Constant(0))))
                    .Setter(setter => setter.EmitsBody(body => body.Return())))
                .AddMethod("Store", typeof(int), method => method
                    .WithParameter<int>("value")
                    .EmitsBody(body =>
                    {
                        body.Assign(body.Property(body.Self(), nameof(VirtualPropertyBase.Value)), body.Parameter<int>("value"));
                        body.Return(body.Property(body.Self(), nameof(VirtualPropertyBase.Value)));
                    })))
            .Build();

        var target = new VirtualMemberTarget();
        var members = context.CreateInstance("VirtualMembers");
        var overridden = context.CreateInstance("OverriddenProperty");

        Assert.Equal(9, members.GetType().GetMethod("Update")!.Invoke(members, new object[] { target }));
        Assert.Equal(4, target.Value);
        Assert.Equal(5, target[0]);
        Assert.Equal(0, overridden.GetType().GetMethod("Store")!.Invoke(overridden, new object[] { 9 }));
    }

    [Fact]
    public void GeneratedBodies_Apply_Numeric_Promotions_And_Nullable_Conversions()
    {
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("NumericPromotions", type => type
                .AddMethod("FloatLess", typeof(bool), method => method
                    .WithParameter<float>("left")
                    .WithParameter<int>("right")
                    .EmitsBody(body => body.Return(body.LessThan(body.Parameter<float>("left"), body.Parameter<int>("right")))))
                .AddMethod("LongLess", typeof(bool), method => method
                    .WithParameter<long>("left")
                    .WithParameter<int>("right")
                    .EmitsBody(body => body.Return(body.LessThan(body.Parameter<long>("left"), body.Parameter<int>("right")))))
                .AddMethod("DoubleDivide", typeof(double), method => method
                    .WithParameter<double>("left")
                    .WithParameter<int>("right")
                    .EmitsBody(body => body.Return(body.Divide(body.Parameter<double>("left"), body.Parameter<int>("right")))))
                .AddMethod("NullableToNullable", typeof(long?), method => method
                    .WithParameter<int?>("value")
                    .EmitsBody(body => body.Return(body.Convert<long?>(body.Parameter<int?>("value")))))
                .AddMethod("ByteDecimal", typeof(decimal), method => method
                    .WithParameter<byte>("value")
                    .EmitsBody(body => body.Return(body.Convert<decimal>(body.Parameter<byte>("value")))))
                .AddMethod("ShortDecimal", typeof(decimal), method => method
                    .WithParameter<short>("value")
                    .EmitsBody(body => body.Return(body.Convert<decimal>(body.Parameter<short>("value")))))
                .AddMethod("IntDecimal", typeof(decimal), method => method
                    .WithParameter<int>("value")
                    .EmitsBody(body => body.Return(body.Convert<decimal>(body.Parameter<int>("value")))))
                .AddMethod("LongDecimal", typeof(decimal), method => method
                    .WithParameter<long>("value")
                    .EmitsBody(body => body.Return(body.Convert<decimal>(body.Parameter<long>("value")))))
                .AddMethod("FloatDecimal", typeof(decimal), method => method
                    .WithParameter<float>("value")
                    .EmitsBody(body => body.Return(body.Convert<decimal>(body.Parameter<float>("value")))))
                .AddMethod("DoubleDecimal", typeof(decimal), method => method
                    .WithParameter<double>("value")
                    .EmitsBody(body => body.Return(body.Convert<decimal>(body.Parameter<double>("value"))))))
            .Build();
        var instance = context.CreateInstance("NumericPromotions");
        var type = instance.GetType();

        Assert.True((bool)type.GetMethod("FloatLess")!.Invoke(instance, new object[] { 1.5f, 2 })!);
        Assert.True((bool)type.GetMethod("LongLess")!.Invoke(instance, new object[] { 1L, 2 })!);
        Assert.Equal(2.5d, type.GetMethod("DoubleDivide")!.Invoke(instance, new object[] { 5d, 2 }));
        Assert.Equal(4L, type.GetMethod("NullableToNullable")!.Invoke(instance, new object[] { 4 }));
        Assert.Null(type.GetMethod("NullableToNullable")!.Invoke(instance, new object[] { null! }));
        Assert.Equal(1m, type.GetMethod("ByteDecimal")!.Invoke(instance, new object[] { (byte)1 }));
        Assert.Equal(2m, type.GetMethod("ShortDecimal")!.Invoke(instance, new object[] { (short)2 }));
        Assert.Equal(3m, type.GetMethod("IntDecimal")!.Invoke(instance, new object[] { 3 }));
        Assert.Equal(4m, type.GetMethod("LongDecimal")!.Invoke(instance, new object[] { 4L }));
        Assert.Equal(5m, type.GetMethod("FloatDecimal")!.Invoke(instance, new object[] { 5f }));
        Assert.Equal(6m, type.GetMethod("DoubleDecimal")!.Invoke(instance, new object[] { 6d }));

        Assert.True(BeeIlConversions.CanConvert(typeof(int?), typeof(long?)));
        Assert.True(BeeIlConversions.CanConvert(typeof(byte), typeof(decimal)));
        Assert.True(BeeIlConversions.CanConvert(typeof(short), typeof(decimal)));
        Assert.True(BeeIlConversions.CanConvert(typeof(int), typeof(decimal)));
        Assert.True(BeeIlConversions.CanConvert(typeof(long), typeof(decimal)));
        Assert.True(BeeIlConversions.CanConvert(typeof(float), typeof(decimal)));
        Assert.True(BeeIlConversions.CanConvert(typeof(double), typeof(decimal)));
    }

    [Fact]
    public void MethodBodyBuilder_Handles_Optional_Arguments_Ambiguous_Members_And_Inherited_TypeBuilder_Members()
    {
        var builder = CreateBody(("item", typeof(HiddenMethodDerived), 0));
        var noArgumentInstanceMethod = typeof(System.Text.StringBuilder).GetMethod(nameof(System.Text.StringBuilder.ToString), Type.EmptyTypes)!;
        var noArgumentStaticMethod = typeof(Guid).GetMethod(nameof(Guid.NewGuid), Type.EmptyTypes)!;
        var switchCases = new BeeSwitchBuilder();

        Assert.Equal(typeof(System.Text.StringBuilder), builder.New(typeof(System.Text.StringBuilder), null!).Type);
        Assert.Equal(typeof(string), builder.Call(builder.New<System.Text.StringBuilder>(), noArgumentInstanceMethod, null!).Type);
        Assert.Equal(typeof(Guid), builder.StaticCall(noArgumentStaticMethod, null!).Type);
        builder.If(builder.Constant(true), _ => { }, _ => { });
        switchCases.Case(null, _ => { });
        Assert.Throws<InvalidOperationException>(() => switchCases.Case(null, _ => { }));
        Assert.Throws<ArgumentNullException>(() => new BeeConstantExpression(1, null!));
        Assert.Throws<ArgumentNullException>(() => new BeeNewExpression(typeof(System.Text.StringBuilder), null!));
        Assert.Throws<AmbiguousMatchException>(() => builder.Call(
            builder.Parameter<HiddenMethodDerived>("item"),
            nameof(HiddenMethodDerived.Format),
            new[] { typeof(int) },
            builder.Constant(1)));

        var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName($"DynaBee.Tests.InheritedMembers.{Guid.NewGuid():N}"),
            AssemblyBuilderAccess.Run);
        var module = dynamicAssembly.DefineDynamicModule("Inherited");
        var assemblyContext = new AssemblyContextBuilder("Inherited", dynamicAssembly, module);
        var typeBuilder = module.DefineType("Generated", TypeAttributes.Public, typeof(InheritedTypeBuilderBase));
        var typeContext = (TypeContextBuilder)assemblyContext.AddTypeBuilder("Generated", typeBuilder);
        var inheritedBody = new BeeMethodBodyBuilder(
            typeBuilder.DefineMethod("UseInherited", MethodAttributes.Public, typeof(void), Type.EmptyTypes).GetILGenerator(),
            typeof(void),
            Array.Empty<(string Name, Type Type, int ArgumentIndex)>(),
            typeBuilder,
            typeContext);

        Assert.Equal(typeof(int), inheritedBody.Property(inheritedBody.Self(), nameof(InheritedTypeBuilderBase.Value)).Type);
        Assert.Equal(typeof(int), inheritedBody.Field(inheritedBody.Self(), nameof(InheritedTypeBuilderBase.Counter)).Type);
    }

    [Fact]
    public void MethodBodyBuilder_Uses_Self_Fallbacks_And_Rejects_Invalid_Expression_Arguments()
    {
        var body = new BeeMethodBodyBuilder(
            new DynamicMethod("Fallbacks", typeof(void), Type.EmptyTypes).GetILGenerator(),
            typeof(void),
            Array.Empty<(string Name, Type Type, int ArgumentIndex)>(),
            typeof(SelfFallbackProbe));
        var baseMethod = typeof(BaseCallTarget).GetMethod(nameof(BaseCallTarget.GetValue))!;
        var baseBody = new BeeMethodBodyBuilder(
            new DynamicMethod("Base", typeof(int), Type.EmptyTypes).GetILGenerator(),
            typeof(int),
            Array.Empty<(string Name, Type Type, int ArgumentIndex)>(),
            typeof(BaseCallTarget),
            baseMethod: baseMethod);
        var genericParameter = typeof(List<>).GetGenericArguments()[0];

        Assert.Equal(typeof(int), body.Property(body.Self(), nameof(SelfFallbackProbe.Value)).Type);
        Assert.Equal(typeof(int), body.Field(body.Self(), nameof(SelfFallbackProbe.Field)).Type);
        Assert.Equal(typeof(int), body.Call(body.Self(), nameof(SelfFallbackProbe.VirtualValue), Type.EmptyTypes).Type);
        Assert.Throws<MissingMemberException>(() => body.Property(body.Self(), "Missing"));
        Assert.Throws<MissingFieldException>(() => body.Field(body.Self(), "Missing"));
        Assert.Throws<MissingMethodException>(() => body.Call(body.Self(), "Missing", Type.EmptyTypes));
        Assert.Throws<NotSupportedException>(() => body.New(typeof(ConstructorTarget), new IBeeValueExpression[] { null! }));
        Assert.Throws<NotSupportedException>(() => baseBody.CallBase(new IBeeValueExpression[] { null! }));
        Assert.Throws<NotSupportedException>(() => body.StaticCall(
            typeof(Guid).GetMethod(nameof(Guid.NewGuid), Type.EmptyTypes)!,
            new IBeeValueExpression[] { null! }));
        Assert.Throws<AmbiguousMatchException>(() => body.New(
            typeof(AmbiguousConstructorTarget),
            body.Constant(1)));
        Assert.Throws<MissingMethodException>(() => body.New(
            typeof(ConstructorTarget),
            body.Constant(null!, genericParameter)));
        Assert.True(BeeIlConversions.CanConvert(typeof(int), typeof(long?)));
    }

    [Fact]
    public void MethodBodyBuilder_Handles_Empty_Argument_Collections_And_Incomplete_Interface_TypeBuilders()
    {
        var body = CreateBody();
        var baseMethod = typeof(ArgumentBase).GetMethod(nameof(ArgumentBase.Add))!;
        var baseBody = new BeeMethodBodyBuilder(
            new DynamicMethod("BaseArguments", typeof(int), Type.EmptyTypes).GetILGenerator(),
            typeof(int),
            Array.Empty<(string Name, Type Type, int ArgumentIndex)>(),
            typeof(ArgumentBase),
            baseMethod: baseMethod);
        var assembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName($"DynaBee.Tests.IncompleteInterface.{Guid.NewGuid():N}"),
            AssemblyBuilderAccess.Run);
        var interfaceBuilder = assembly
            .DefineDynamicModule("Incomplete")
            .DefineType("IIncomplete", TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);
        var interfaceBody = new BeeMethodBodyBuilder(
            interfaceBuilder.DefineMethod("Use", MethodAttributes.Public | MethodAttributes.Abstract | MethodAttributes.Virtual, typeof(void), Type.EmptyTypes).GetILGenerator(),
            typeof(void),
            Array.Empty<(string Name, Type Type, int ArgumentIndex)>(),
            interfaceBuilder);

        Assert.Equal(typeof(string), body.Concat(Array.Empty<IBeeValueExpression>()).Type);
        Assert.Equal(typeof(ConstructorTarget), body.New(typeof(ConstructorTarget), new[] { body.Constant(1) }).Type);
        Assert.Equal(typeof(int), baseBody.CallBase(new[] { baseBody.Constant(1), baseBody.Constant(2) }).Type);
        Assert.Throws<MissingMemberException>(() => interfaceBody.Property(interfaceBody.Self(), "Missing"));
        Assert.Throws<MissingFieldException>(() => interfaceBody.Field(interfaceBody.Self(), "Missing"));
    }

    [Fact]
    public void Body_Expression_Guards_Reject_Unknown_Internal_Operator_Kinds()
    {
        var il = new DynamicMethod("InvalidOperators", typeof(void), Type.EmptyTypes).GetILGenerator();
        var left = new BeeConstantExpression(1, typeof(int));
        var right = new BeeConstantExpression(2, typeof(int));

        Assert.Throws<NotSupportedException>(() => new BeeOrderedComparisonExpression(
            left,
            right,
            (BeeOrderedComparisonKind)999).EmitLoad(il));
        Assert.Throws<NotSupportedException>(() => new BeeArithmeticExpression(
            left,
            right,
            (BeeArithmeticKind)999).EmitLoad(il));
    }

    [Fact]
    public void Generated_Bodies_Execute_String_Addition_And_Protected_Value_Returns()
    {
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("BodyBranches", type => type
                .AddMethod("Append", typeof(string), method => method
                    .WithParameter<string>("value")
                    .EmitsBody(body => body.Return(body.Add(body.Parameter<string>("value"), body.Constant("!")))))
                .AddMethod("ReturnFromCatch", typeof(int), method => method
                    .EmitsBody(body => body.Try(
                        tryBody => tryBody.Return(tryBody.Constant(7)),
                        catches => catches.Catch<InvalidOperationException>("error", (_, catchBody) => catchBody.Return(catchBody.Constant(-1)))))))
            .Build();
        var instance = context.CreateInstance("BodyBranches");
        var type = instance.GetType();

        Assert.Equal("value!", type.GetMethod("Append")!.Invoke(instance, new object[] { "value" }));
        Assert.Equal(7, type.GetMethod("ReturnFromCatch")!.Invoke(instance, null));
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

    public sealed class AmbiguousConstructorTarget
    {
        public AmbiguousConstructorTarget(IComparable value)
        {
        }

        public AmbiguousConstructorTarget(IFormattable value)
        {
        }
    }

    public class SelfFallbackProbe
    {
        public virtual int Value { get; set; }

        public int Field;

        public virtual int VirtualValue()
            => Value + Field;
    }

    public class ArgumentBase
    {
        public virtual int Add(int left, int right)
            => left + right;
    }

    private enum ConversionKind
    {
        None,
        One,
    }

    public sealed class Token
    {
        public Token(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }

    public sealed class ExpressionEmissionTarget
    {
        public int NonVirtual => 1;

        public int this[int index] => index;
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

    public class BaseCallTarget
    {
        public virtual int GetValue()
            => 6;
    }

    public sealed class AccessorTarget
    {
        public int GetterOnly => 1;

        public int SetterOnly
        {
            set { }
        }

        public static int StaticGetterOnly => 1;

        public static int StaticSetterOnly
        {
            set { }
        }

        public int this[int index] => index;

        public int this[string key]
        {
            set { }
        }
    }

    public class VirtualMemberTarget
    {
        private readonly int[] _values = new int[1];

        public virtual int Value { get; set; }

        public virtual int this[int index]
        {
            get => _values[index];
            set => _values[index] = value;
        }
    }

    public class VirtualPropertyBase
    {
        public virtual int Value { get; set; }
    }

    public class HiddenMethodBase
    {
        public string Format(int value)
            => value.ToString();
    }

    public class HiddenMethodDerived : HiddenMethodBase
    {
        public new string Format(int value)
            => $"{value}";
    }

    public class InheritedTypeBuilderBase
    {
        public int Counter;

        public int Value { get; set; }
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

    private sealed class ExternalValueExpression : IBeeValueExpression
    {
        public Type Type => typeof(int);
    }

    private sealed class ExternalAssignableExpression : IBeeAssignableExpression
    {
        public Type Type => typeof(int);
    }
}
