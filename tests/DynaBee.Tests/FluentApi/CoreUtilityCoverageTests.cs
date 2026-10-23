namespace DynaBee.Tests.FluentApi;

using DynaBee;
using DynaBee.FluentApi;
using DynaBee.Infrastructure;
using DynaBee.Infrastructure.Configurators;

public sealed class CoreUtilityCoverageTests
{
    [Fact]
    public void DynamicAccess_Reads_Writes_And_Explains_Invalid_Property_Requests()
    {
        var target = new UtilityModel { Value = 2 };

        Assert.Equal(2, DynamicAccess.GetProperty<int>(target, nameof(UtilityModel.Value)));
        DynamicAccess.SetProperty(target, nameof(UtilityModel.Value), 5);
        Assert.Equal(5, target.Value);
        Assert.Throws<ArgumentNullException>(() => DynamicAccess.GetProperty<int>(null!, "Value"));
        Assert.Throws<ArgumentException>(() => DynamicAccess.GetProperty<int>(target, ""));
        Assert.Throws<InvalidOperationException>(() => DynamicAccess.GetProperty<int>(target, "Missing"));
        Assert.Throws<ArgumentNullException>(() => DynamicAccess.SetProperty(null!, "Value", 1));
        Assert.Throws<ArgumentException>(() => DynamicAccess.SetProperty(target, "", 1));
        Assert.Throws<InvalidOperationException>(() => DynamicAccess.SetProperty(target, "Missing", 1));
    }

    [Fact]
    public void Attributes_Resolve_Constructors_Named_Members_And_Invalid_Shapes()
    {
        var attribute = BeeAttribute.Of<UtilityAttribute>("message")
            .WithProperty(nameof(UtilityAttribute.Enabled), true)
            .WithField(nameof(UtilityAttribute.Code), 7);
        var builder = new BeeAttributeBuilder(typeof(UtilityAttribute))
            .WithConstructorArgument("message")
            .WithConstructorArguments()
            .WithProperty(nameof(UtilityAttribute.Enabled), true)
            .WithField(nameof(UtilityAttribute.Code), 7);

        Assert.NotNull(attribute.Build());
        Assert.NotNull(builder.Build());
        Assert.Throws<InvalidOperationException>(() => BeeAttribute.Of<UtilityAttribute>((object[])null!).Build());
        Assert.Throws<ArgumentNullException>(() => BeeAttribute.Of(null!));
        Assert.Throws<ArgumentException>(() => BeeAttribute.Of(typeof(string)));
        Assert.Throws<ArgumentException>(() => attribute.WithProperty("", true));
        Assert.Throws<ArgumentException>(() => attribute.WithField("", true));
        Assert.Throws<ArgumentNullException>(() => builder.WithConstructorArguments(null!));
        Assert.Throws<ArgumentException>(() => builder.WithProperty("", true));
        Assert.Throws<ArgumentException>(() => builder.WithField("", true));
        Assert.Throws<InvalidOperationException>(() => BeeAttribute.Of<UtilityAttribute>(1).Build());
        Assert.Throws<InvalidOperationException>(() => BeeAttribute.Of<UtilityAttribute>("x").WithProperty("Missing", true).Build());
        Assert.Throws<InvalidOperationException>(() => BeeAttribute.Of<UtilityAttribute>("x").WithField("Missing", true).Build());
    }

    [Fact]
    public void Record_Semantics_Handle_Identity_Nulls_Differences_And_Missing_Properties()
    {
        var first = new UtilityModel { Value = 3 };
        var same = new UtilityModel { Value = 3 };
        var different = new UtilityModel { Value = 4 };

        Assert.True(RecordSemanticsHelper.EqualsByProperties(first, first, new[] { nameof(UtilityModel.Value) }));
        Assert.False(RecordSemanticsHelper.EqualsByProperties(null!, first, new[] { nameof(UtilityModel.Value) }));
        Assert.False(RecordSemanticsHelper.EqualsByProperties(first, null!, new[] { nameof(UtilityModel.Value) }));
        Assert.False(RecordSemanticsHelper.EqualsByProperties(first, "other", new[] { nameof(UtilityModel.Value) }));
        Assert.True(RecordSemanticsHelper.EqualsByProperties(first, same, new[] { nameof(UtilityModel.Value) }));
        Assert.False(RecordSemanticsHelper.EqualsByProperties(first, different, new[] { nameof(UtilityModel.Value) }));
        Assert.Throws<InvalidOperationException>(() => RecordSemanticsHelper.EqualsByProperties(first, same, new[] { "Missing" }));
        Assert.Equal(0, RecordSemanticsHelper.ComputeHashCode(null!, Array.Empty<string>()));
        Assert.Equal(RecordSemanticsHelper.ComputeHashCode(first, new[] { nameof(UtilityModel.Value) }), RecordSemanticsHelper.ComputeHashCode(same, new[] { nameof(UtilityModel.Value) }));
        Assert.Throws<InvalidOperationException>(() => RecordSemanticsHelper.ComputeHashCode(first, new[] { "Missing" }));
        Assert.Equal(string.Empty, RecordSemanticsHelper.ToRecordString(null!, Array.Empty<string>()));
        Assert.Equal("UtilityModel { Value = 3 }", RecordSemanticsHelper.ToRecordString(first, new[] { nameof(UtilityModel.Value) }));
        Assert.Throws<InvalidOperationException>(() => RecordSemanticsHelper.ToRecordString(first, new[] { "Missing" }));
    }

    [Fact]
    public void Core_Type_And_Access_Value_Objects_Reject_Invalid_Inputs_And_Compare_Objects()
    {
        Type nullType = null!;

        Assert.Throws<ArgumentNullException>(() => { BeeType ignored = nullType; });
        Assert.False(ClassAccessModifier.Public.Equals("public"));
        Assert.False(FieldAccessModifier.Public.Equals("public"));
        Assert.False(MethodAccessModifier.Public.Equals("public"));
    }

    [Fact]
    public void Assembly_Context_Helpers_Validate_Null_Contexts_And_Leases_Dispose_Deterministically()
    {
        var context = new BeeAssemblyBuilder($"DynaBee.Tests.Lease.{Guid.NewGuid():N}").Build();
        var lease = new AssemblyContextLease(context);

        Assert.Throws<ArgumentNullException>(() => AssemblyContextExtensions.GetClrType(null!, "Generated"));
        Assert.Throws<ArgumentNullException>(() => new AssemblyContextLease(null!));
        Assert.Same(context, lease.Context);
        lease.Dispose();
        Assert.True(lease.IsDisposed);
        Assert.Throws<ObjectDisposedException>(() => _ = lease.Context);
    }

    [Fact]
    public void Fluent_Wrapper_Constructors_And_Accessor_Bodies_Reject_Invalid_Null_Contracts()
    {
        Assert.Throws<ArgumentNullException>(() => new BeeEnumBuilder(null!));
        Assert.Throws<ArgumentNullException>(() => new BeeInterfaceBuilder(null!));
        Assert.Throws<ArgumentNullException>(() => new BeeStructBuilder(null!));
        Assert.Throws<ArgumentNullException>(() => new BeeRecordClassBuilder(null!));
        Assert.Throws<ArgumentNullException>(() => new BeeRecordStructBuilder(null!));
        Assert.Throws<ArgumentNullException>(() => new BeePropertyOverrideBuilder(null!));
        Assert.Throws<ArgumentNullException>(() => new BeePropertyAccessorBuilder().Emits(null!));
        Assert.Throws<ArgumentNullException>(() => new BeePropertyAccessorBuilder().EmitsBody(null!));
        Assert.Throws<ArgumentNullException>(() => new BeeInterfaceContractBuilder(null!, null!, false));
        Assert.Throws<ArgumentNullException>(() => new BeeConstructorBuilder().CallsBase(
            (System.Reflection.ConstructorInfo)null!,
            (Action<BeeConstructorBaseCallBuilder>)null!));
        Assert.Throws<ArgumentNullException>(() => new BeeConstructorBuilder().CallsBase(
            (System.Reflection.ConstructorInfo)null!,
            Array.Empty<string>()));

        new BeeAssemblyBuilder($"DynaBee.Tests.Contract.{Guid.NewGuid():N}")
            .AddClass("Generated", type => Assert.Throws<ArgumentNullException>(() =>
                new BeeInterfaceContractBuilder(type, null!, false)));
    }

    public sealed class UtilityModel
    {
        public int Value { get; set; }
    }

    [AttributeUsage(AttributeTargets.All)]
    public sealed class UtilityAttribute : Attribute
    {
        public UtilityAttribute(string message)
        {
            Message = message;
        }

        public string Message { get; }

        public bool Enabled { get; set; }

        public int Code;
    }
}
