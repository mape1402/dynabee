namespace DynaBee.Tests.FluentApi;

using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using DynaBee.FluentApi;
using DynaBee.FluentApi.Body;
using DynaBee.Infrastructure;
using DynaBee.Infrastructure.ContextBuilders;

public sealed class BodyAndExpressionCoverageTests
{
    [Fact]
    public void MethodBodyBuilder_Emits_Assignment_Calls_Defaults_And_Conversions()
    {
        BodyStatics.Count = 0;
        BodyStatics.Value = 0;

        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("BodyFeatures", type => type
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
                            body.Add(
                                body.Property(body.Self(), "DynamicValue"),
                                body.Field(body.Self(), "DynamicField"))));
                    }))
                .AddMethod("NamedStaticCall", typeof(int), method => method
                    .WithParameter<int>("left")
                    .WithParameter<int>("right")
                    .EmitsBody(body => body.Return(body.StaticCall(
                        typeof(BodyRuntime),
                        nameof(BodyRuntime.StaticAdd),
                        new[] { typeof(int), typeof(int) },
                        body.Parameter<int>("left"),
                        body.Parameter<int>("right")))))
                .AddMethod("Decimal", typeof(decimal), method => method
                    .WithParameter<int>("value")
                    .EmitsBody(body => body.Return(body.Convert<decimal>(body.Parameter<int>("value")))))
                .AddMethod("NullableLong", typeof(long?), method => method
                    .WithParameter<int?>("value")
                    .EmitsBody(body => body.Return(body.Convert<long?>(body.Parameter<int?>("value")))))
                .AddMethod("NullableInt", typeof(int?), method => method
                    .WithParameter<int>("value")
                    .EmitsBody(body => body.Return(body.Convert<int?>(body.Parameter<int>("value")))))
                .AddMethod("Defaults", typeof(string), method => method
                    .EmitsBody(body => body.Return(body.Concat(
                        body.Default<string>(),
                        body.Constant("value")))))
                .AddMethod("Empty", typeof(string), method => method
                    .EmitsBody(body => body.Return(body.Concat())))
                .AddVoidMethod("ReturnVoid", method => method.EmitsBody(body => body.Return())))
            .Build();

        var type = context.GetClrType("BodyFeatures");
        var instance = Activator.CreateInstance(type)!;
        var target = new BodyRuntime();

        Assert.Equal(21, type.GetMethod("AssignAndCall")!.Invoke(instance, new object[] { target, 4 }));
        Assert.Equal(4, target.Field);
        Assert.Equal(5, target.Value);
        Assert.Equal(1, target.TouchCount);
        Assert.Equal(4, BodyStatics.Count);
        Assert.Equal(6, BodyStatics.Value);
        Assert.Equal(7, type.GetField("DynamicField")!.GetValue(instance));
        Assert.Equal(4, type.GetProperty("DynamicValue")!.GetValue(instance));
        Assert.Equal(9, type.GetMethod("NamedStaticCall")!.Invoke(instance, new object[] { 4, 5 }));
        Assert.Equal(12m, type.GetMethod("Decimal")!.Invoke(instance, new object[] { 12 }));
        Assert.Equal(9L, type.GetMethod("NullableLong")!.Invoke(instance, new object[] { 9 }));
        Assert.Null(type.GetMethod("NullableLong")!.Invoke(instance, new object[] { null! }));
        Assert.Equal(8, type.GetMethod("NullableInt")!.Invoke(instance, new object[] { 8 }));
        Assert.Equal("value", type.GetMethod("Defaults")!.Invoke(instance, null));
        Assert.Equal(string.Empty, type.GetMethod("Empty")!.Invoke(instance, null));
        type.GetMethod("ReturnVoid")!.Invoke(instance, null);
    }

    [Fact]
    public void MethodBodyBuilder_Emits_Array_Indexer_And_Enumerable_Bodies()
    {
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("CollectionFeatures", type => type
                .AddMethod("Array", typeof(int), method => method
                    .WithParameter<int>("length")
                    .WithParameter<int>("value")
                    .EmitsBody(body =>
                    {
                        var array = body.DeclareLocal<int[]>("array");
                        body.Assign(array, body.NewArray<int>(body.Parameter<int>("length")));
                        body.Assign(body.Index(array, body.Constant(0)), body.Parameter<int>("value"));
                        body.Return(body.Index(array, body.Constant(0)));
                    }))
                .AddMethod("List", typeof(string), method => method
                    .WithParameter<List<string>>("items")
                    .EmitsBody(body =>
                    {
                        var items = body.Parameter<List<string>>("items");
                        body.Assign(body.Index(items, body.Constant(0)), body.Constant("updated"));
                        body.Return(body.Index(items, body.Constant(0)));
                    }))
                .AddMethod("SumEnumerable", typeof(int), method => method
                    .WithParameter<IEnumerable<int>>("items")
                    .EmitsBody(body =>
                    {
                        var sum = body.DeclareLocal<int>("sum");
                        body.Assign(sum, body.Constant(0));
                        body.ForEach(body.Parameter<IEnumerable<int>>("items"), "item", (item, loop) =>
                            loop.Assign(sum, loop.Add(sum, item)));
                        body.Return(sum);
                    }))
                .AddMethod("SumArray", typeof(int), method => method
                    .WithParameter<int[]>("items")
                    .EmitsBody(body => SumWithForEach(body, body.Parameter<int[]>("items"))))
                .AddMethod("SumReadOnly", typeof(int), method => method
                    .WithParameter<IReadOnlyList<int>>("items")
                    .EmitsBody(body => SumWithForEach(body, body.Parameter<IReadOnlyList<int>>("items"))))
                .AddMethod("SumCollection", typeof(int), method => method
                    .WithParameter<ICollection<int>>("items")
                    .EmitsBody(body => SumWithForEach(body, body.Parameter<ICollection<int>>("items")))))
            .Build();

        var type = context.GetClrType("CollectionFeatures");
        var instance = Activator.CreateInstance(type)!;
        var values = new List<string> { "original" };

        Assert.Equal(17, type.GetMethod("Array")!.Invoke(instance, new object[] { 2, 17 }));
        Assert.Equal("updated", type.GetMethod("List")!.Invoke(instance, new object[] { values }));
        Assert.Equal("updated", values[0]);
        Assert.Equal(6, type.GetMethod("SumEnumerable")!.Invoke(instance, new object[] { new[] { 1, 2, 3 }.Where(x => x > 0) }));
        Assert.Equal(6, type.GetMethod("SumArray")!.Invoke(instance, new object[] { new[] { 1, 2, 3 } }));
        Assert.Equal(6, type.GetMethod("SumReadOnly")!.Invoke(instance, new object[] { (IReadOnlyList<int>)new List<int> { 1, 2, 3 } }));
        Assert.Equal(6, type.GetMethod("SumCollection")!.Invoke(instance, new object[] { (ICollection<int>)new List<int> { 1, 2, 3 } }));
    }

    [Fact]
    public void MethodBodyBuilder_Emits_Comparisons_Conditions_And_Null_Operations()
    {
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("ConditionalFeatures", type => type
                .AddMethod("Compare", typeof(bool), method => method
                    .WithParameter<int>("left")
                    .WithParameter<int>("right")
                    .EmitsBody(body => body.Return(body.AndAlso(
                        body.LessThanOrEqual(body.Parameter<int>("left"), body.Parameter<int>("right")),
                        body.GreaterThanOrEqual(body.Parameter<int>("right"), body.Parameter<int>("left"))))))
                .AddMethod("Different", typeof(bool), method => method
                    .WithParameter<int>("left")
                    .WithParameter<int>("right")
                    .EmitsBody(body => body.Return(body.NotEqual(body.Parameter<int>("left"), body.Parameter<int>("right")))))
                .AddMethod("Null", typeof(bool), method => method
                    .WithParameter<string>("value")
                    .EmitsBody(body => body.Return(body.IsNull(body.Parameter<string>("value")))))
                .AddMethod("NullableNull", typeof(bool), method => method
                    .WithParameter<int?>("value")
                    .EmitsBody(body => body.Return(body.IsNull(body.Parameter<int?>("value")))))
                .AddMethod("ValueNull", typeof(bool), method => method
                    .WithParameter<int>("value")
                    .EmitsBody(body => body.Return(body.IsNull(body.Parameter<int>("value")))))
                .AddMethod("Conditional", typeof(string), method => method
                    .WithParameter<bool>("condition")
                    .EmitsBody(body => body.Return(body.If(
                        body.Parameter<bool>("condition"),
                        body.Constant("yes"),
                        body.Constant("no")))))
                .AddMethod("Coalesce", typeof(string), method => method
                    .WithParameter<string>("value")
                    .EmitsBody(body => body.Return(body.Coalesce(body.Parameter<string>("value"), body.Constant("fallback")))))
                .AddMethod("Negate", typeof(bool), method => method
                    .WithParameter<bool>("value")
                    .EmitsBody(body => body.Return(body.Not(body.Parameter<bool>("value")))))
                .AddMethod("Arithmetic", typeof(int), method => method
                    .WithParameter<int>("value")
                    .EmitsBody(body => body.Return(body.Modulo(
                        body.Divide(
                            body.Multiply(
                                body.Subtract(body.Parameter<int>("value"), body.Constant(1)),
                                body.Constant(3)),
                            body.Constant(2)),
                        body.Constant(4))))))
            .Build();

        var type = context.GetClrType("ConditionalFeatures");
        var instance = Activator.CreateInstance(type)!;

        Assert.True((bool)type.GetMethod("Compare")!.Invoke(instance, new object[] { 2, 3 })!);
        Assert.False((bool)type.GetMethod("Compare")!.Invoke(instance, new object[] { 4, 3 })!);
        Assert.True((bool)type.GetMethod("Different")!.Invoke(instance, new object[] { 2, 3 })!);
        Assert.False((bool)type.GetMethod("Different")!.Invoke(instance, new object[] { 3, 3 })!);
        Assert.True((bool)type.GetMethod("Null")!.Invoke(instance, new object[] { null! })!);
        Assert.False((bool)type.GetMethod("Null")!.Invoke(instance, new object[] { "value" })!);
        Assert.True((bool)type.GetMethod("NullableNull")!.Invoke(instance, new object[] { null! })!);
        Assert.False((bool)type.GetMethod("NullableNull")!.Invoke(instance, new object[] { 1 })!);
        Assert.False((bool)type.GetMethod("ValueNull")!.Invoke(instance, new object[] { 1 })!);
        Assert.Equal("yes", type.GetMethod("Conditional")!.Invoke(instance, new object[] { true }));
        Assert.Equal("no", type.GetMethod("Conditional")!.Invoke(instance, new object[] { false }));
        Assert.Equal("fallback", type.GetMethod("Coalesce")!.Invoke(instance, new object[] { null! }));
        Assert.Equal("value", type.GetMethod("Coalesce")!.Invoke(instance, new object[] { "value" }));
        Assert.True((bool)type.GetMethod("Negate")!.Invoke(instance, new object[] { false })!);
        Assert.Equal(2, type.GetMethod("Arithmetic")!.Invoke(instance, new object[] { 5 }));
    }

    [Fact]
    public void MethodBodyBuilder_Can_Access_Inherited_Fields_And_Properties_While_The_Type_Is_Being_Generated()
    {
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("DerivedMembers", type => type
                .Inherits<InheritedMembers>()
                .AddMethod("StoreAndRead", typeof(int), method => method
                    .WithParameter<int>("value")
                    .EmitsBody(body =>
                    {
                        var value = body.Parameter<int>("value");
                        body.Assign(body.Property(body.Self(), nameof(InheritedMembers.Value)), value);
                        body.Assign(body.Field(body.Self(), nameof(InheritedMembers.Field)), body.Add(value, body.Constant(1)));
                        body.Return(body.Add(
                            body.Property(body.Self(), nameof(InheritedMembers.Value)),
                            body.Field(body.Self(), nameof(InheritedMembers.Field))));
                    })))
            .Build();

        var instance = (InheritedMembers)context.CreateInstance("DerivedMembers");

        Assert.Equal(13, context.GetClrType("DerivedMembers").GetMethod("StoreAndRead")!.Invoke(instance, new object[] { 6 }));
        Assert.Equal(6, instance.Value);
        Assert.Equal(7, instance.Field);
    }

    [Fact]
    public void MethodBodyBuilder_Executes_Numeric_Promotions_And_Nullable_Coalesce_Fallbacks()
    {
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("Numeric", type => type
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
                    .EmitsBody(body => body.Return(body.Subtract(body.Parameter<long>("left"), body.Parameter<int>("right")))))
                .AddMethod("Nullable", typeof(int), method => method
                    .WithParameter<int?>("value")
                    .EmitsBody(body => body.Return(body.Coalesce(body.Parameter<int?>("value"), body.Constant(9)))))
                .AddMethod("Object", typeof(object), method => method
                    .WithParameter<object>("value")
                    .EmitsBody(body => body.Return(body.Coalesce(body.Parameter<object>("value"), body.Constant("fallback"))))))
            .Build();

        var type = context.GetClrType("Numeric");
        var instance = Activator.CreateInstance(type)!;

        Assert.True((bool)type.GetMethod("Double")!.Invoke(instance, new object[] { 2.5d, 2 })!);
        Assert.Equal(7.5f, type.GetMethod("Float")!.Invoke(instance, new object[] { 2.5f, 3 }));
        Assert.Equal(7L, type.GetMethod("Long")!.Invoke(instance, new object[] { 9L, 2 }));
        Assert.Equal(9, type.GetMethod("Nullable")!.Invoke(instance, new object[] { null! }));
        Assert.Equal(4, type.GetMethod("Nullable")!.Invoke(instance, new object[] { 4 }));
        Assert.Equal("fallback", type.GetMethod("Object")!.Invoke(instance, new object[] { null! }));
        Assert.Equal("value", type.GetMethod("Object")!.Invoke(instance, new object[] { "value" }));
    }

    [Fact]
    public void Expression_Emitter_Supports_Operators_Members_Construction_And_Control_Flow()
    {
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("ExpressionFeatures", type => type
                .AddMethod("Math", typeof(int), method => method
                    .WithParameter<int>("left")
                    .WithParameter<int>("right")
                    .EmitsExpression((Expression<Func<int, int, int>>)((left, right) => ((left - right) * 3 / 2) % 5)))
                .AddMethod("Compare", typeof(bool), method => method
                    .WithParameter<int>("left")
                    .WithParameter<int>("right")
                    .EmitsExpression((Expression<Func<int, int, bool>>)((left, right) => left >= right && (left != right || left <= right))))
                .AddMethod("Choice", typeof(string), method => method
                    .WithParameter<bool>("condition")
                    .EmitsExpression((Expression<Func<bool, string>>)(condition => condition ? "yes" : "no")))
                .AddMethod("Create", typeof(ExpressionRuntime), method => method
                    .WithParameter<int>("value")
                    .EmitsExpression((Expression<Func<int, ExpressionRuntime>>)(value => new ExpressionRuntime(value))))
                .AddMethod("Members", typeof(int), method => method
                    .WithParameter<ExpressionRuntime>("value")
                    .EmitsExpression((Expression<Func<ExpressionRuntime, int>>)(value => value.Number + ExpressionRuntime.StaticNumber + value.Property)))
                .AddMethod("Text", typeof(string), method => method
                    .WithParameter<string>("value")
                    .EmitsExpression((Expression<Func<string, string>>)(value => string.Concat(value, "!"))))
                .AddMethod("Box", typeof(object), method => method
                    .WithParameter<int>("value")
                    .EmitsExpression((Expression<Func<int, object>>)(value => (object)value)))
                .AddMethod("Unbox", typeof(int), method => method
                    .WithParameter<object>("value")
                    .EmitsExpression((Expression<Func<object, int>>)(value => (int)value)))
                .AddMethod("Constants", typeof(long), method => method
                    .EmitsExpression((Expression<Func<long>>)(() => 12L)))
                .AddMethod("Floating", typeof(double), method => method
                    .EmitsExpression((Expression<Func<double>>)(() => 1.5d)))
                .AddMethod("Flag", typeof(bool), method => method
                    .EmitsExpression((Expression<Func<bool>>)(() => true)))
                .AddMethod("Or", typeof(bool), method => method
                    .WithParameter<bool>("left")
                    .WithParameter<bool>("right")
                    .EmitsExpression((Expression<Func<bool, bool, bool>>)((left, right) => left || right))))
            .Build();

        var type = context.GetClrType("ExpressionFeatures");
        var instance = Activator.CreateInstance(type)!;
        ExpressionRuntime.StaticNumber = 4;

        Assert.Equal(3, type.GetMethod("Math")!.Invoke(instance, new object[] { 7, 5 }));
        Assert.True((bool)type.GetMethod("Compare")!.Invoke(instance, new object[] { 4, 3 })!);
        Assert.False((bool)type.GetMethod("Compare")!.Invoke(instance, new object[] { 2, 3 })!);
        Assert.Equal("yes", type.GetMethod("Choice")!.Invoke(instance, new object[] { true }));
        Assert.Equal("no", type.GetMethod("Choice")!.Invoke(instance, new object[] { false }));
        Assert.Equal(8, ((ExpressionRuntime)type.GetMethod("Create")!.Invoke(instance, new object[] { 8 })!).Number);
        Assert.Equal(11, type.GetMethod("Members")!.Invoke(instance, new object[] { new ExpressionRuntime(3) { Property = 4 } }));
        Assert.Equal("value!", type.GetMethod("Text")!.Invoke(instance, new object[] { "value" }));
        Assert.Equal(9, type.GetMethod("Box")!.Invoke(instance, new object[] { 9 }));
        Assert.Equal(10, type.GetMethod("Unbox")!.Invoke(instance, new object[] { (object)10 }));
        Assert.Equal(12L, type.GetMethod("Constants")!.Invoke(instance, null));
        Assert.Equal(1.5d, type.GetMethod("Floating")!.Invoke(instance, null));
        Assert.True((bool)type.GetMethod("Flag")!.Invoke(instance, null)!);
        Assert.True((bool)type.GetMethod("Or")!.Invoke(instance, new object[] { false, true })!);
        Assert.False((bool)type.GetMethod("Or")!.Invoke(instance, new object[] { false, false })!);
    }

    [Fact]
    public void MethodBodyBuilder_Reports_Invalid_Usage_Before_Emission()
    {
        var body = new BeeMethodBodyBuilder(
            new DynamicMethod("Coverage", typeof(void), Type.EmptyTypes).GetILGenerator(),
            typeof(void),
            new[] { ("number", typeof(int), 0) });

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
    public void MethodBodyBuilder_Validates_All_Public_Statement_And_Member_Inputs()
    {
        var body = CreateBody(
            ("number", typeof(int), 0),
            ("matrix", typeof(int[,]), 1),
            ("target", typeof(BodyRuntime), 2));
        var number = body.Parameter<int>("number");
        var target = body.Parameter<BodyRuntime>("target");

        Assert.Throws<ArgumentNullException>(() => body.For(null!, _ => body.Constant(true), _ => { }, _ => { }));
        Assert.Throws<ArgumentNullException>(() => body.For(_ => { }, null!, _ => { }, _ => { }));
        Assert.Throws<ArgumentNullException>(() => body.For(_ => { }, _ => body.Constant(true), null!, _ => { }));
        Assert.Throws<ArgumentNullException>(() => body.For(_ => { }, _ => body.Constant(true), _ => { }, null!));
        Assert.Throws<ArgumentNullException>(() => body.ForEach(null!, "item", (_, _) => { }));
        Assert.Throws<ArgumentException>(() => body.ForEach(number, "", (_, _) => { }));
        Assert.Throws<ArgumentNullException>(() => body.ForEach(number, "item", null!));
        Assert.Throws<InvalidOperationException>(() => body.ForEach(number, "item", (_, _) => { }));
        Assert.Throws<NotSupportedException>(() => body.Index(body.Parameter("matrix"), body.Constant(0)));
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
    public void Il_Conversions_Support_Nullables_Enums_Boxing_Text_And_Numeric_Targets()
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

        Assert.Equal(5L, CreateConversion<int, long>()(5));
        Assert.Equal(5, CreateConversion<long, int>()(5L));
        Assert.Equal(5f, CreateConversion<int, float>()(5));
        Assert.Equal(5d, CreateConversion<int, double>()(5));
        Assert.Equal((short)5, CreateConversion<int, short>()(5));
        Assert.Equal((byte)5, CreateConversion<int, byte>()(5));
        Assert.False(CreateConversion<int, bool>()(0));
        Assert.True(CreateConversion<int, bool>()(3));
        Assert.Equal(7, CreateConversion<object, int>()(7));
        Assert.Equal("8", CreateConversion<int, string>()(8));
        Assert.Equal(2, CreateConversion<ConversionState, int>()(ConversionState.Ready));
        Assert.Equal(ConversionState.Ready, CreateConversion<int, ConversionState>()(2));
        Assert.Equal(3m, CreateConversion<short, decimal>()(3));

        var nullableToNullable = CreateConversion<int?, long?>();
        Assert.Null(nullableToNullable(null));
        Assert.Equal(9L, nullableToNullable(9));
    }

    private static void SumWithForEach(IBeeMethodBodyBuilder body, IBeeValueExpression items)
    {
        var sum = body.DeclareLocal<int>("sum");
        body.Assign(sum, body.Constant(0));
        body.ForEach(items, "item", (item, loop) => loop.Assign(sum, loop.Add(sum, item)));
        body.Return(sum);
    }

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
