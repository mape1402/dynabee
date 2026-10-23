namespace DynaBee.Tests.FluentApi;

using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using DynaBee.FluentApi;
using DynaBee.FluentApi.Body;
using DynaBee.Infrastructure;

public sealed class BodyAndExpressionCoverageTests
{
    [Fact]
    public void MethodBodyBuilder_Assigns_Instance_Static_And_Dynamic_Members()
    {
        BodyStatics.Count = 0;
        BodyStatics.Value = 0;
        var context = BuildClass("BodyFeatures", type => type
            .AddAutoProperty<int>("DynamicValue")
            .AddField<int>("DynamicField", FieldAccessModifier.Public)
            .AddMethod("AssignAndCall", typeof(int), method => method
                .WithParameter<BodyRuntime>("target")
                .WithParameter<int>("value")
                .EmitsBody(body =>
                {
                    var target = body.Parameter<BodyRuntime>("target");
                    var value = body.Parameter<int>("value");
                    body.Assign(body.Field(target, nameof(BodyRuntime.Field)), value);
                    body.Assign(body.Property(target, nameof(BodyRuntime.Value)), body.Add(value, body.Constant(1)));
                    body.Assign(body.StaticField(typeof(BodyStatics), nameof(BodyStatics.Count)), value);
                    body.Assign(body.StaticProperty(typeof(BodyStatics), nameof(BodyStatics.Value)), body.Add(value, body.Constant(2)));
                    body.Assign(body.Property(body.Self(), "DynamicValue"), value);
                    body.Assign(body.Field(body.Self(), "DynamicField"), body.Add(value, body.Constant(3)));
                    body.Evaluate(body.Call(target, nameof(BodyRuntime.Touch), Type.EmptyTypes));
                    body.Return(body.Add(
                        body.Call(target, typeof(BodyRuntime).GetMethod(nameof(BodyRuntime.Total))!),
                        body.Add(body.Property(body.Self(), "DynamicValue"), body.Field(body.Self(), "DynamicField"))));
                })));

        var type = context.GetClrType("BodyFeatures");
        var instance = Activator.CreateInstance(type)!;
        var target = new BodyRuntime();

        Assert.Equal(21, Invoke<int>(type, instance, "AssignAndCall", target, 4));
        Assert.Equal(4, target.Field);
        Assert.Equal(5, target.Value);
        Assert.Equal(1, target.TouchCount);
        Assert.Equal(4, BodyStatics.Count);
        Assert.Equal(6, BodyStatics.Value);
        Assert.Equal(7, type.GetField("DynamicField")!.GetValue(instance));
        Assert.Equal(4, type.GetProperty("DynamicValue")!.GetValue(instance));
    }

    [Fact]
    public void MethodBodyBuilder_Calls_Static_Methods_By_Name_And_Signature()
    {
        var context = BuildClass("StaticCall", type => type
            .AddMethod("Add", typeof(int), method => method
                .WithParameter<int>("left")
                .WithParameter<int>("right")
                .EmitsBody(body => body.Return(body.StaticCall(
                    typeof(BodyRuntime),
                    nameof(BodyRuntime.StaticAdd),
                    new[] { typeof(int), typeof(int) },
                    body.Parameter<int>("left"),
                    body.Parameter<int>("right"))))));

        var type = context.GetClrType("StaticCall");
        Assert.Equal(9, Invoke<int>(type, Activator.CreateInstance(type)!, "Add", 4, 5));
    }

    [Fact]
    public void MethodBodyBuilder_Converts_Int32_To_Decimal()
    {
        var context = BuildConversionClass<int, decimal>("Decimal");
        var type = context.GetClrType("Conversion");

        Assert.Equal(12m, Invoke<decimal>(type, Activator.CreateInstance(type)!, "Decimal", 12));
    }

    [Fact]
    public void MethodBodyBuilder_Converts_Nullable_Int32_To_Nullable_Int64()
    {
        var context = BuildConversionClass<int?, long?>("NullableLong");
        var type = context.GetClrType("Conversion");
        var instance = Activator.CreateInstance(type)!;

        Assert.Equal(9L, Invoke<long?>(type, instance, "NullableLong", 9));
        Assert.Null(Invoke<long?>(type, instance, "NullableLong", new object[] { null! }));
    }

    [Fact]
    public void MethodBodyBuilder_Converts_Int32_To_Nullable_Int32()
    {
        var context = BuildConversionClass<int, int?>("NullableInt");
        var type = context.GetClrType("Conversion");

        Assert.Equal(8, Invoke<int?>(type, Activator.CreateInstance(type)!, "NullableInt", 8));
    }

    [Fact]
    public void MethodBodyBuilder_Returns_Default_Values_In_Concatenation()
    {
        var context = BuildClass("Defaults", type => type
            .AddMethod("Value", typeof(string), method => method
                .EmitsBody(body => body.Return(body.Concat(body.Default<string>(), body.Constant("value")))))
            .AddMethod("Empty", typeof(string), method => method
                .EmitsBody(body => body.Return(body.Concat()))));
        var type = context.GetClrType("Defaults");
        var instance = Activator.CreateInstance(type)!;

        Assert.Equal("value", Invoke<string>(type, instance, "Value"));
        Assert.Equal(string.Empty, Invoke<string>(type, instance, "Empty"));
    }

    [Fact]
    public void MethodBodyBuilder_Emits_Void_Returns()
    {
        var context = BuildClass("VoidReturn", type => type
            .AddVoidMethod("Return", method => method.EmitsBody(body => body.Return())));
        var type = context.GetClrType("VoidReturn");

        type.GetMethod("Return")!.Invoke(Activator.CreateInstance(type), null);
    }

    [Fact]
    public void MethodBodyBuilder_Creates_And_Assigns_Array_Elements()
    {
        var context = BuildClass("Arrays", type => type
            .AddMethod("ValueAtZero", typeof(int), method => method
                .WithParameter<int>("length")
                .WithParameter<int>("value")
                .EmitsBody(body =>
                {
                    var array = body.DeclareLocal<int[]>("array");
                    body.Assign(array, body.NewArray<int>(body.Parameter<int>("length")));
                    body.Assign(body.Index(array, body.Constant(0)), body.Parameter<int>("value"));
                    body.Return(body.Index(array, body.Constant(0)));
                })));
        var type = context.GetClrType("Arrays");

        Assert.Equal(17, Invoke<int>(type, Activator.CreateInstance(type)!, "ValueAtZero", 2, 17));
    }

    [Fact]
    public void MethodBodyBuilder_Assigns_List_Indexers()
    {
        var context = BuildClass("Lists", type => type
            .AddMethod("UpdateFirst", typeof(string), method => method
                .WithParameter<List<string>>("items")
                .EmitsBody(body =>
                {
                    var items = body.Parameter<List<string>>("items");
                    body.Assign(body.Index(items, body.Constant(0)), body.Constant("updated"));
                    body.Return(body.Index(items, body.Constant(0)));
                })));
        var type = context.GetClrType("Lists");
        var items = new List<string> { "original" };

        Assert.Equal("updated", Invoke<string>(type, Activator.CreateInstance(type)!, "UpdateFirst", items));
        Assert.Equal("updated", items[0]);
    }

    [Fact]
    public void MethodBodyBuilder_Enumerates_IEnumerable_Values()
        => Assert.Equal(6, InvokeSum<IEnumerable<int>>(new[] { 1, 2, 3 }.Where(value => value > 0)));

    [Fact]
    public void MethodBodyBuilder_Enumerates_Array_Values()
        => Assert.Equal(6, InvokeSum(new[] { 1, 2, 3 }));

    [Fact]
    public void MethodBodyBuilder_Enumerates_ReadOnlyList_Values()
        => Assert.Equal(6, InvokeSum<IReadOnlyList<int>>(new List<int> { 1, 2, 3 }));

    [Fact]
    public void MethodBodyBuilder_Enumerates_Collection_Values()
        => Assert.Equal(6, InvokeSum<ICollection<int>>(new List<int> { 1, 2, 3 }));

    [Fact]
    public void MethodBodyBuilder_Evaluates_Comparison_Operators()
    {
        var context = BuildClass("Comparison", type => type
            .AddMethod("Ordered", typeof(bool), method => method
                .WithParameter<int>("left")
                .WithParameter<int>("right")
                .EmitsBody(body => body.Return(body.AndAlso(
                    body.LessThanOrEqual(body.Parameter<int>("left"), body.Parameter<int>("right")),
                    body.GreaterThanOrEqual(body.Parameter<int>("right"), body.Parameter<int>("left"))))))
            .AddMethod("Different", typeof(bool), method => method
                .WithParameter<int>("left")
                .WithParameter<int>("right")
                .EmitsBody(body => body.Return(body.NotEqual(body.Parameter<int>("left"), body.Parameter<int>("right"))))));
        var type = context.GetClrType("Comparison");
        var instance = Activator.CreateInstance(type)!;

        Assert.True(Invoke<bool>(type, instance, "Ordered", 2, 3));
        Assert.False(Invoke<bool>(type, instance, "Ordered", 4, 3));
        Assert.True(Invoke<bool>(type, instance, "Different", 2, 3));
        Assert.False(Invoke<bool>(type, instance, "Different", 3, 3));
    }

    [Fact]
    public void MethodBodyBuilder_Detects_Null_For_Reference_Nullable_And_Value_Types()
    {
        var context = BuildClass("NullChecks", type => type
            .AddMethod("Reference", typeof(bool), method => method
                .WithParameter<string>("value")
                .EmitsBody(body => body.Return(body.IsNull(body.Parameter<string>("value")))))
            .AddMethod("Nullable", typeof(bool), method => method
                .WithParameter<int?>("value")
                .EmitsBody(body => body.Return(body.IsNull(body.Parameter<int?>("value")))))
            .AddMethod("Value", typeof(bool), method => method
                .WithParameter<int>("value")
                .EmitsBody(body => body.Return(body.IsNull(body.Parameter<int>("value"))))));
        var type = context.GetClrType("NullChecks");
        var instance = Activator.CreateInstance(type)!;

        Assert.True(Invoke<bool>(type, instance, "Reference", new object[] { null! }));
        Assert.False(Invoke<bool>(type, instance, "Reference", "value"));
        Assert.True(Invoke<bool>(type, instance, "Nullable", new object[] { null! }));
        Assert.False(Invoke<bool>(type, instance, "Nullable", 1));
        Assert.False(Invoke<bool>(type, instance, "Value", 1));
    }

    [Fact]
    public void MethodBodyBuilder_Evaluates_Conditional_And_Coalesce_Expressions()
    {
        var context = BuildClass("Conditionals", type => type
            .AddMethod("Choice", typeof(string), method => method
                .WithParameter<bool>("condition")
                .EmitsBody(body => body.Return(body.If(body.Parameter<bool>("condition"), body.Constant("yes"), body.Constant("no")))))
            .AddMethod("Coalesce", typeof(string), method => method
                .WithParameter<string>("value")
                .EmitsBody(body => body.Return(body.Coalesce(body.Parameter<string>("value"), body.Constant("fallback"))))));
        var type = context.GetClrType("Conditionals");
        var instance = Activator.CreateInstance(type)!;

        Assert.Equal("yes", Invoke<string>(type, instance, "Choice", true));
        Assert.Equal("no", Invoke<string>(type, instance, "Choice", false));
        Assert.Equal("fallback", Invoke<string>(type, instance, "Coalesce", new object[] { null! }));
        Assert.Equal("value", Invoke<string>(type, instance, "Coalesce", "value"));
    }

    [Fact]
    public void MethodBodyBuilder_Evaluates_Boolean_Negation_And_Arithmetic()
    {
        var context = BuildClass("Operators", type => type
            .AddMethod("Negate", typeof(bool), method => method
                .WithParameter<bool>("value")
                .EmitsBody(body => body.Return(body.Not(body.Parameter<bool>("value")))))
            .AddMethod("Arithmetic", typeof(int), method => method
                .WithParameter<int>("value")
                .EmitsBody(body => body.Return(body.Modulo(
                    body.Divide(body.Multiply(body.Subtract(body.Parameter<int>("value"), body.Constant(1)), body.Constant(3)), body.Constant(2)),
                    body.Constant(4))))));
        var type = context.GetClrType("Operators");
        var instance = Activator.CreateInstance(type)!;

        Assert.True(Invoke<bool>(type, instance, "Negate", false));
        Assert.Equal(2, Invoke<int>(type, instance, "Arithmetic", 5));
    }

    [Fact]
    public void MethodBodyBuilder_Accesses_Inherited_Fields_And_Properties()
    {
        var context = BuildClass("DerivedMembers", type => type
            .Inherits<InheritedMembers>()
            .AddMethod("StoreAndRead", typeof(int), method => method
                .WithParameter<int>("value")
                .EmitsBody(body =>
                {
                    var value = body.Parameter<int>("value");
                    body.Assign(body.Property(body.Self(), nameof(InheritedMembers.Value)), value);
                    body.Assign(body.Field(body.Self(), nameof(InheritedMembers.Field)), body.Add(value, body.Constant(1)));
                    body.Return(body.Add(body.Property(body.Self(), nameof(InheritedMembers.Value)), body.Field(body.Self(), nameof(InheritedMembers.Field))));
                })));
        var type = context.GetClrType("DerivedMembers");
        var instance = (InheritedMembers)Activator.CreateInstance(type)!;

        Assert.Equal(13, Invoke<int>(type, instance, "StoreAndRead", 6));
        Assert.Equal(6, instance.Value);
        Assert.Equal(7, instance.Field);
    }

    [Fact]
    public void MethodBodyBuilder_Applies_Numeric_Promotions()
    {
        var context = BuildClass("Numeric", type => type
            .AddMethod("Double", typeof(bool), method => method
                .WithParameter<double>("left")
                .WithParameter<int>("right")
                .EmitsBody(body => body.Return(body.GreaterThan(body.Parameter<double>("left"), body.Parameter<int>("right")))))
            .AddMethod("Float", typeof(float), method => method
                .WithParameter<float>("left")
                .WithParameter<int>("right")
                .EmitsBody(body => body.Return(body.Multiply(body.Parameter<float>("left"), body.Parameter<int>("right")))))
            .AddMethod("Long", typeof(long), method => method
                .WithParameter<long>("left")
                .WithParameter<int>("right")
                .EmitsBody(body => body.Return(body.Subtract(body.Parameter<long>("left"), body.Parameter<int>("right"))))));
        var type = context.GetClrType("Numeric");
        var instance = Activator.CreateInstance(type)!;

        Assert.True(Invoke<bool>(type, instance, "Double", 2.5d, 2));
        Assert.Equal(7.5f, Invoke<float>(type, instance, "Float", 2.5f, 3));
        Assert.Equal(7L, Invoke<long>(type, instance, "Long", 9L, 2));
    }

    [Fact]
    public void MethodBodyBuilder_Coalesces_Nullable_And_Object_Values()
    {
        var context = BuildClass("Coalesce", type => type
            .AddMethod("Nullable", typeof(int), method => method
                .WithParameter<int?>("value")
                .EmitsBody(body => body.Return(body.Coalesce(body.Parameter<int?>("value"), body.Constant(9)))))
            .AddMethod("Object", typeof(object), method => method
                .WithParameter<object>("value")
                .EmitsBody(body => body.Return(body.Coalesce(body.Parameter<object>("value"), body.Constant("fallback"))))));
        var type = context.GetClrType("Coalesce");
        var instance = Activator.CreateInstance(type)!;

        Assert.Equal(9, Invoke<int>(type, instance, "Nullable", new object[] { null! }));
        Assert.Equal(4, Invoke<int>(type, instance, "Nullable", 4));
        Assert.Equal("fallback", Invoke<object>(type, instance, "Object", new object[] { null! }));
        Assert.Equal("value", Invoke<object>(type, instance, "Object", "value"));
    }

    [Fact]
    public void ExpressionEmitter_Evaluates_Arithmetic_And_Boolean_Operators()
    {
        var context = BuildClass("Expressions", type => type
            .AddMethod("Math", typeof(int), method => method
                .WithParameter<int>("left")
                .WithParameter<int>("right")
                .EmitsExpression((Expression<Func<int, int, int>>)((left, right) => ((left - right) * 3 / 2) % 5)))
            .AddMethod("Compare", typeof(bool), method => method
                .WithParameter<int>("left")
                .WithParameter<int>("right")
                .EmitsExpression((Expression<Func<int, int, bool>>)((left, right) => left >= right && (left != right || left <= right))))
            .AddMethod("Or", typeof(bool), method => method
                .WithParameter<bool>("left")
                .WithParameter<bool>("right")
                .EmitsExpression((Expression<Func<bool, bool, bool>>)((left, right) => left || right))));
        var type = context.GetClrType("Expressions");
        var instance = Activator.CreateInstance(type)!;

        Assert.Equal(3, Invoke<int>(type, instance, "Math", 7, 5));
        Assert.True(Invoke<bool>(type, instance, "Compare", 4, 3));
        Assert.False(Invoke<bool>(type, instance, "Compare", 2, 3));
        Assert.True(Invoke<bool>(type, instance, "Or", false, true));
        Assert.False(Invoke<bool>(type, instance, "Or", false, false));
    }

    [Fact]
    public void ExpressionEmitter_Evaluates_Conditional_Expressions()
    {
        var context = BuildClass("ExpressionChoice", type => type
            .AddMethod("Choice", typeof(string), method => method
                .WithParameter<bool>("condition")
                .EmitsExpression((Expression<Func<bool, string>>)(condition => condition ? "yes" : "no"))));
        var type = context.GetClrType("ExpressionChoice");
        var instance = Activator.CreateInstance(type)!;

        Assert.Equal("yes", Invoke<string>(type, instance, "Choice", true));
        Assert.Equal("no", Invoke<string>(type, instance, "Choice", false));
    }

    [Fact]
    public void ExpressionEmitter_Constructs_Objects_And_Reads_Members()
    {
        ExpressionRuntime.StaticNumber = 4;
        var context = BuildClass("ExpressionMembers", type => type
            .AddMethod("Create", typeof(ExpressionRuntime), method => method
                .WithParameter<int>("value")
                .EmitsExpression((Expression<Func<int, ExpressionRuntime>>)(value => new ExpressionRuntime(value))))
            .AddMethod("Members", typeof(int), method => method
                .WithParameter<ExpressionRuntime>("value")
                .EmitsExpression((Expression<Func<ExpressionRuntime, int>>)(value => value.Number + ExpressionRuntime.StaticNumber + value.Property))));
        var type = context.GetClrType("ExpressionMembers");
        var instance = Activator.CreateInstance(type)!;

        Assert.Equal(8, Invoke<ExpressionRuntime>(type, instance, "Create", 8).Number);
        Assert.Equal(11, Invoke<int>(type, instance, "Members", new ExpressionRuntime(3) { Property = 4 }));
    }

    [Fact]
    public void ExpressionEmitter_Calls_Static_Methods_And_Converts_Boxed_Values()
    {
        var context = BuildClass("ExpressionCalls", type => type
            .AddMethod("Text", typeof(string), method => method
                .WithParameter<string>("value")
                .EmitsExpression((Expression<Func<string, string>>)(value => string.Concat(value, "!"))))
            .AddMethod("Box", typeof(object), method => method
                .WithParameter<int>("value")
                .EmitsExpression((Expression<Func<int, object>>)(value => (object)value)))
            .AddMethod("Unbox", typeof(int), method => method
                .WithParameter<object>("value")
                .EmitsExpression((Expression<Func<object, int>>)(value => (int)value))));
        var type = context.GetClrType("ExpressionCalls");
        var instance = Activator.CreateInstance(type)!;

        Assert.Equal("value!", Invoke<string>(type, instance, "Text", "value"));
        Assert.Equal(9, Invoke<object>(type, instance, "Box", 9));
        Assert.Equal(10, Invoke<int>(type, instance, "Unbox", (object)10));
    }

    [Fact]
    public void ExpressionEmitter_Emits_Constants()
    {
        var context = BuildClass("ExpressionConstants", type => type
            .AddMethod("Long", typeof(long), method => method.EmitsExpression((Expression<Func<long>>)(() => 12L)))
            .AddMethod("Floating", typeof(double), method => method.EmitsExpression((Expression<Func<double>>)(() => 1.5d)))
            .AddMethod("Flag", typeof(bool), method => method.EmitsExpression((Expression<Func<bool>>)(() => true))));
        var type = context.GetClrType("ExpressionConstants");
        var instance = Activator.CreateInstance(type)!;

        Assert.Equal(12L, Invoke<long>(type, instance, "Long"));
        Assert.Equal(1.5d, Invoke<double>(type, instance, "Floating"));
        Assert.True(Invoke<bool>(type, instance, "Flag"));
    }

    [Fact]
    public void MethodBodyBuilder_Rejects_Invalid_Context_And_Local_Usage()
    {
        var body = CreateBody(("number", typeof(int), 0));

        Assert.Throws<InvalidOperationException>(() => body.Self());
        Assert.Throws<KeyNotFoundException>(() => body.Parameter("missing"));
        Assert.Throws<InvalidOperationException>(() => body.Parameter<string>("number"));
        Assert.Throws<ArgumentException>(() => body.DeclareLocal("", typeof(int)));
        Assert.Throws<ArgumentNullException>(() => body.DeclareLocal("value", null!));
        body.DeclareLocal<int>("value");
        Assert.Throws<InvalidOperationException>(() => body.DeclareLocal<int>("value"));
        Assert.Throws<ArgumentNullException>(() => body.New(null!));
        Assert.Throws<ArgumentNullException>(() => body.NewArray(null!, body.Constant(1)));
        Assert.Throws<InvalidOperationException>(() => body.NewArray<int>(body.Constant("bad")));
        Assert.Throws<ArgumentNullException>(() => body.Index(null!, body.Constant(0)));
        Assert.Throws<InvalidOperationException>(() => body.Break());
        Assert.Throws<InvalidOperationException>(() => body.Continue());
        Assert.Throws<InvalidOperationException>(() => body.CallBase());
    }

    [Fact]
    public void MethodBodyBuilder_Rejects_Invalid_Static_Member_Usage()
    {
        var body = CreateBody();

        Assert.Throws<ArgumentNullException>(() => body.StaticCall(null!, Array.Empty<IBeeValueExpression>()));
        Assert.Throws<ArgumentException>(() => body.StaticCall(typeof(BodyRuntime), "", Type.EmptyTypes));
        Assert.Throws<ArgumentNullException>(() => body.StaticCall(typeof(BodyRuntime), nameof(BodyRuntime.StaticAdd), null!));
        Assert.Throws<ArgumentNullException>(() => body.StaticField(null!, nameof(BodyStatics.Count)));
        Assert.Throws<ArgumentException>(() => body.StaticProperty(typeof(BodyStatics), ""));
        Assert.Throws<ArgumentNullException>(() => body.Default(null!));
        Assert.Throws<ArgumentNullException>(() => body.Convert(null!, typeof(int)));
        Assert.Throws<ArgumentNullException>(() => body.Assign(null!, body.Constant(1)));
        Assert.Throws<ArgumentNullException>(() => body.Throw(null!));
    }

    [Fact]
    public void MethodBodyBuilder_Rejects_Invalid_Loop_And_Indexer_Inputs()
    {
        var body = CreateBody(("number", typeof(int), 0), ("matrix", typeof(int[,]), 1));
        var number = body.Parameter<int>("number");

        Assert.Throws<ArgumentNullException>(() => body.For(null!, _ => body.Constant(true), _ => { }, _ => { }));
        Assert.Throws<ArgumentNullException>(() => body.For(_ => { }, null!, _ => { }, _ => { }));
        Assert.Throws<ArgumentNullException>(() => body.For(_ => { }, _ => body.Constant(true), null!, _ => { }));
        Assert.Throws<ArgumentNullException>(() => body.For(_ => { }, _ => body.Constant(true), _ => { }, null!));
        Assert.Throws<ArgumentNullException>(() => body.ForEach(null!, "item", (_, _) => { }));
        Assert.Throws<ArgumentException>(() => body.ForEach(number, "", (_, _) => { }));
        Assert.Throws<ArgumentNullException>(() => body.ForEach(number, "item", null!));
        Assert.Throws<InvalidOperationException>(() => body.ForEach(number, "item", (_, _) => { }));
        Assert.Throws<NotSupportedException>(() => body.Index(body.Parameter("matrix"), body.Constant(0)));
    }

    [Fact]
    public void MethodBodyBuilder_Rejects_Invalid_Member_And_Control_Flow_Inputs()
    {
        var body = CreateBody(("target", typeof(BodyRuntime), 0));
        var target = body.Parameter<BodyRuntime>("target");

        Assert.Throws<ArgumentNullException>(() => body.Call(null!, typeof(BodyRuntime).GetMethod(nameof(BodyRuntime.Total))!));
        Assert.Throws<ArgumentNullException>(() => body.Call(target, (MethodInfo)null!));
        Assert.Throws<ArgumentNullException>(() => body.Call(null!, nameof(BodyRuntime.Total), Type.EmptyTypes));
        Assert.Throws<ArgumentException>(() => body.Call(target, "", Type.EmptyTypes));
        Assert.Throws<ArgumentNullException>(() => body.Call(target, nameof(BodyRuntime.Total), null!));
        Assert.Throws<ArgumentNullException>(() => body.StaticProperty(null!, nameof(BodyStatics.Value)));
        Assert.Throws<MissingMemberException>(() => body.StaticProperty(typeof(BodyStatics), "Missing"));
        Assert.Throws<ArgumentNullException>(() => body.Field(null!, nameof(BodyRuntime.Field)));
        Assert.Throws<ArgumentException>(() => body.Field(target, ""));
        Assert.Throws<MissingFieldException>(() => body.Field(target, "Missing"));
        Assert.Throws<ArgumentException>(() => body.StaticField(typeof(BodyStatics), ""));
        Assert.Throws<MissingFieldException>(() => body.StaticField(typeof(BodyStatics), "Missing"));
        Assert.Throws<ArgumentNullException>(() => body.IsNull(null!));
        Assert.Throws<ArgumentNullException>(() => body.If(null!, _ => { }));
        Assert.Throws<ArgumentNullException>(() => body.If(body.Constant(true), null!));
        Assert.Throws<ArgumentNullException>(() => body.Switch(null!, _ => { }));
        Assert.Throws<ArgumentNullException>(() => body.Switch(body.Constant(1), null!));
        Assert.Throws<ArgumentNullException>(() => body.Try(null!));
        Assert.Throws<InvalidOperationException>(() => body.Try(_ => { }));
        Assert.Throws<ArgumentNullException>(() => body.Using(null!, "resource", (_, _) => { }));
        Assert.Throws<ArgumentException>(() => body.Using(target, "", (_, _) => { }));
        Assert.Throws<InvalidOperationException>(() => body.Using(target, "resource", (_, _) => { }));
        Assert.Throws<InvalidOperationException>(() => body.Return(body.Constant(1)));
    }

    [Fact]
    public void IlConversions_Recognize_Nullable_Reference_And_Enum_Compatibility()
    {
        Assert.False(BeeIlConversions.CanConvert(typeof(void), typeof(int)));
        Assert.True(BeeIlConversions.CanConvert(typeof(int), typeof(int)));
        Assert.True(BeeIlConversions.CanConvert(typeof(string), typeof(object)));
        Assert.True(BeeIlConversions.CanConvert(typeof(int), typeof(int?)));
        Assert.True(BeeIlConversions.CanConvert(typeof(int?), typeof(long)));
        Assert.True(BeeIlConversions.CanConvert(typeof(int?), typeof(long?)));
        Assert.True(BeeIlConversions.CanConvert(typeof(ConversionState), typeof(int)));
        Assert.True(BeeIlConversions.CanConvert(typeof(int), typeof(ConversionState)));
        Assert.True(BeeIlConversions.CanConvert(typeof(object), typeof(int)));
        Assert.True(BeeIlConversions.CanConvert(typeof(int), typeof(string)));
        Assert.True(BeeIlConversions.CanConvert(typeof(string), typeof(Stream)));
        Assert.False(BeeIlConversions.CanConvert(typeof(int), typeof(Stream)));
        Assert.False(BeeIlConversions.CanConvert(typeof(DateTime), typeof(int)));
    }

    [Fact]
    public void IlConversions_Emit_Numeric_And_Boolean_Conversions()
    {
        Assert.Equal(5L, CreateConversion<int, long>()(5));
        Assert.Equal(5, CreateConversion<long, int>()(5L));
        Assert.Equal(5f, CreateConversion<int, float>()(5));
        Assert.Equal(5d, CreateConversion<int, double>()(5));
        Assert.Equal((short)5, CreateConversion<int, short>()(5));
        Assert.Equal((byte)5, CreateConversion<int, byte>()(5));
        Assert.False(CreateConversion<int, bool>()(0));
        Assert.True(CreateConversion<int, bool>()(3));
        Assert.Equal(3m, CreateConversion<short, decimal>()(3));
    }

    [Fact]
    public void IlConversions_Emit_Boxing_Text_Enum_And_Nullable_Conversions()
    {
        Assert.Equal(7, CreateConversion<object, int>()(7));
        Assert.Equal("8", CreateConversion<int, string>()(8));
        Assert.Equal(2, CreateConversion<ConversionState, int>()(ConversionState.Ready));
        Assert.Equal(ConversionState.Ready, CreateConversion<int, ConversionState>()(2));

        var nullableToNullable = CreateConversion<int?, long?>();
        Assert.Null(nullableToNullable(null));
        Assert.Equal(9L, nullableToNullable(9));
    }

    private static IAssemblyContext BuildClass(string name, Action<BeeClassBuilder> configure)
        => new BeeAssemblyBuilder(NewAssemblyName()).AddClass(name, configure).Build();

    private static IAssemblyContext BuildConversionClass<TSource, TTarget>(string methodName)
        => BuildClass("Conversion", type => type
            .AddMethod(methodName, typeof(TTarget), method => method
                .WithParameter<TSource>("value")
                .EmitsBody(body => body.Return(body.Convert<TTarget>(body.Parameter<TSource>("value"))))));

    private static int InvokeSum<TCollection>(TCollection values)
    {
        var context = BuildClass("Sum", type => type
            .AddMethod("Values", typeof(int), method => method
                .WithParameter<TCollection>("items")
                .EmitsBody(body => SumWithForEach(body, body.Parameter<TCollection>("items")))));
        var type = context.GetClrType("Sum");

        return Invoke<int>(type, Activator.CreateInstance(type)!, "Values", values!);
    }

    private static void SumWithForEach(IBeeMethodBodyBuilder body, IBeeValueExpression items)
    {
        var sum = body.DeclareLocal<int>("sum");
        body.Assign(sum, body.Constant(0));
        body.ForEach(items, "item", (item, loop) => loop.Assign(sum, loop.Add(sum, item)));
        body.Return(sum);
    }

    private static T Invoke<T>(Type type, object instance, string methodName, params object[] arguments)
        => (T)type.GetMethod(methodName)!.Invoke(instance, arguments)!;

    private static BeeMethodBodyBuilder CreateBody(params (string Name, Type Type, int ArgumentIndex)[] parameters)
        => new(new DynamicMethod("Coverage", typeof(void), parameters.Select(x => x.Type).ToArray()).GetILGenerator(), typeof(void), parameters);

    private static Func<TSource, TTarget> CreateConversion<TSource, TTarget>()
    {
        var method = new DynamicMethod(
            "Convert",
            typeof(TTarget),
            new[] { typeof(TSource) },
            typeof(BodyAndExpressionCoverageTests).Module,
            skipVisibility: true);
        var il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        BeeIlConversions.EmitConvert(typeof(TSource), typeof(TTarget), il);
        il.Emit(OpCodes.Ret);
        return (Func<TSource, TTarget>)method.CreateDelegate(typeof(Func<TSource, TTarget>));
    }

    private static string NewAssemblyName()
        => $"DynaBee.Tests.BodyCoverage.{Guid.NewGuid():N}";

    public sealed class BodyRuntime
    {
        public int Field;

        public int Value { get; set; }

        public int TouchCount { get; private set; }

        public void Touch()
            => TouchCount++;

        public int Total()
            => Field + Value + TouchCount;

        public static int StaticAdd(int left, int right)
            => left + right;
    }

    public static class BodyStatics
    {
        public static int Count;

        public static int Value { get; set; }
    }

    public sealed class ExpressionRuntime
    {
        public ExpressionRuntime(int number)
        {
            Number = number;
        }

        public static int StaticNumber;

        public int Number;

        public int Property { get; set; }
    }

    public class InheritedMembers
    {
        public int Field;

        public int Value { get; set; }
    }

    private enum ConversionState
    {
        Unknown = 0,
        Ready = 2,
    }
}
