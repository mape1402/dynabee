namespace DynaBee.Tests.FluentApi;

using System.Reflection;
using DynaBee;
using DynaBee.FluentApi;
using DynaBee.Infrastructure;

public sealed class CoreValueAndTypeFormCoverageTests
{
    [Fact]
    public void BeeType_Parses_Converts_And_Rejects_Incompatible_Representations()
    {
        BeeType reference = "Sibling";
        BeeType parsed = BeeType.Parse("Parsed");
        BeeType clr = typeof(int);
        var generic = BeeType.GenericParameter("T");

        Assert.True(BeeType.TryParse("Resolved", out var resolved));
        Assert.False(BeeType.TryParse("", out var empty));
        Assert.Equal(default, empty);
        Assert.Equal("Sibling", (string)reference);
        Assert.Equal("Parsed", (string)parsed);
        Assert.Equal("Resolved", (string)resolved);
        Assert.Equal(typeof(int), (Type)clr);
        Assert.Throws<InvalidOperationException>(() => _ = (Type)reference);
        Assert.Throws<InvalidOperationException>(() => _ = (string)clr);
        Assert.Throws<InvalidOperationException>(() => _ = (Type)generic);
        Assert.Throws<ArgumentNullException>(() => BeeType.Parse(null!));
        Assert.Throws<ArgumentNullException>(() => BeeType.GenericParameter(""));
    }

    [Fact]
    public void AccessModifiers_Expose_Stable_Equality_Hashes_Strings_And_Attributes()
    {
        Assert.True(ClassAccessModifier.Public == ClassAccessModifier.Public);
        Assert.True(ClassAccessModifier.Public != ClassAccessModifier.Internal);
        Assert.Equal(ClassAccessModifier.Public.GetHashCode(), ClassAccessModifier.Public.GetHashCode());
        Assert.Equal(TypeAttributes.Public, (TypeAttributes)ClassAccessModifier.Public);
        Assert.Equal(TypeAttributes.NestedPublic, (TypeAttributes)ClassAccessModifier.NestedPublic);
        Assert.Equal(TypeAttributes.NestedPrivate, (TypeAttributes)ClassAccessModifier.NestedPrivate);
        Assert.Equal(TypeAttributes.NestedFamily, (TypeAttributes)ClassAccessModifier.NestedProtected);
        Assert.Equal(TypeAttributes.NestedAssembly, (TypeAttributes)ClassAccessModifier.NestedInternal);
        Assert.Equal(TypeAttributes.NestedFamORAssem, (TypeAttributes)ClassAccessModifier.NestedProtectedInternal);
        Assert.Equal(TypeAttributes.NestedFamANDAssem, (TypeAttributes)ClassAccessModifier.NestedPrivateProtected);
        Assert.Contains("Public", ClassAccessModifier.Public.ToString());

        Assert.True(FieldAccessModifier.Public.Equals(FieldAccessModifier.Public));
        Assert.Equal(FieldAccessModifier.Public.GetHashCode(), FieldAccessModifier.Public.GetHashCode());
        Assert.Equal(FieldAttributes.FamORAssem, (FieldAttributes)FieldAccessModifier.ProtectedInternal);
        Assert.Equal(FieldAttributes.FamANDAssem, (FieldAttributes)FieldAccessModifier.PrivateProtected);
        Assert.Contains("Public", FieldAccessModifier.Public.ToString());
        Assert.Equal(MethodAttributes.Public, (MethodAttributes)MethodAccessModifier.Public);
        Assert.Equal(MethodAccessModifier.Public.GetHashCode(), MethodAccessModifier.Public.GetHashCode());
    }

    [Fact]
    public void TypeForm_Builders_Create_Attributed_Structs()
    {
        var context = CreateTypeFormsContext();
        var structType = context.GetClrType("GeneratedStruct");
        var value = Activator.CreateInstance(structType)!;
        structType.GetProperty("Value")!.SetValue(value, 7);
        structType.GetField("Counter")!.SetValue(value, 9);

        Assert.True(structType.IsValueType);
        Assert.Contains(typeof(IMarker), structType.GetInterfaces());
        Assert.Equal("struct", structType.GetCustomAttribute<ObsoleteAttribute>()!.Message);
        Assert.Equal(7, structType.GetProperty("Value")!.GetValue(value));
        Assert.Equal(9, structType.GetField("Counter")!.GetValue(value));
        Assert.Equal(42, structType.GetMethod("Answer")!.Invoke(value, null));
        structType.GetMethod("Noop")!.Invoke(value, null);
    }

    [Fact]
    public void TypeForm_Builders_Create_Attributed_Enums()
    {
        var context = CreateTypeFormsContext();
        var enumType = context.GetClrType("GeneratedEnum");

        Assert.Equal(typeof(byte), Enum.GetUnderlyingType(enumType));
        Assert.Equal(new[] { "None", "Ready" }, Enum.GetNames(enumType));
        Assert.NotNull(enumType.GetCustomAttribute<SerializableAttribute>());
        Assert.Equal("enum", enumType.GetCustomAttribute<ObsoleteAttribute>()!.Message);
        Assert.Equal(typeof(long), Enum.GetUnderlyingType(context.GetClrType("RawEnum")));
    }

    [Fact]
    public void TypeForm_Builders_Create_Records_With_Components_And_Attributes()
    {
        var context = CreateTypeFormsContext();
        var emptyRecord = context.GetClrType("EmptyRecord");
        var record = context.CreateInstance("GeneratedRecord");
        var recordStruct = Activator.CreateInstance(context.GetClrType("GeneratedRecordStruct"))!;

        Assert.NotNull(emptyRecord.GetCustomAttribute<RecordLikeAttribute>());
        Assert.Contains(typeof(IMarker), record.GetType().GetInterfaces());
        Assert.Equal("record", record.GetType().GetMethod("Label")!.Invoke(record, null));
        Assert.Equal("record", record.GetType().GetCustomAttribute<ObsoleteAttribute>()!.Message);
        Assert.Equal("struct", recordStruct.GetType().GetMethod("Label")!.Invoke(recordStruct, null));
        Assert.Contains(typeof(IMarker), recordStruct.GetType().GetInterfaces());
    }
    [Fact]
    public void TypeForm_Builders_Reject_Invalid_Interface_Enum_And_Attribute_Configuration()
    {
        Assert.Throws<ArgumentException>(() => new BeeAssemblyBuilder(NewAssemblyName())
            .AddStruct("Invalid", builder => builder.Implements(typeof(string))));
        Assert.Throws<ArgumentNullException>(() => new BeeAssemblyBuilder(NewAssemblyName())
            .AddStruct("Invalid", builder => builder.Implements(null!)));
        Assert.Throws<ArgumentNullException>(() => new BeeAssemblyBuilder(NewAssemblyName())
            .AddStruct("Invalid", builder => builder.AddAttribute(null!)));
        Assert.Throws<ArgumentException>(() => new BeeAssemblyBuilder(NewAssemblyName())
            .AddEnum("Invalid", typeof(decimal), ClassAccessModifier.Public));
        Assert.Throws<ArgumentException>(() => new BeeAssemblyBuilder(NewAssemblyName())
            .AddEnum<int>("Invalid", enumBuilder => enumBuilder.AddValue("", 1)));
        Assert.Throws<ArgumentNullException>(() => new BeeAssemblyBuilder(NewAssemblyName())
            .AddEnum<int>("Invalid", enumBuilder => enumBuilder.AddValue("Value", null!)));
        Assert.Throws<ArgumentNullException>(() => new BeeAssemblyBuilder(NewAssemblyName())
            .AddEnum<int>("Invalid", enumBuilder => enumBuilder.AddAttribute(null!)));
    }

    private static IAssemblyContext CreateTypeFormsContext()
    {
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .EnableCache()
            .AddStruct("GeneratedStruct", structBuilder => structBuilder
                .Implements<IMarker>()
                .AddAttribute<ObsoleteAttribute>(attribute => attribute.WithConstructorArgument("struct"))
                .AddField<int>("Counter", FieldAccessModifier.Public)
                .AddProperty<int>("Value")
                .AddMethod("Answer", typeof(int), method => method.EmitsBody(body => body.Return(body.Constant(42))))
                .AddVoidMethod("Noop"))
            .AddEnum<byte>("GeneratedEnum", enumBuilder => enumBuilder
                .AddAttribute(BeeAttribute.Of<SerializableAttribute>())
                .AddAttribute<ObsoleteAttribute>(attribute => attribute.WithConstructorArgument("enum"))
                .AddValue("None", 0)
                .AddValue("Ready", 1))
            .AddEnum("RawEnum", typeof(long), ClassAccessModifier.Internal, enumBuilder => enumBuilder.AddValue("One", 1L))
            .AddRecordClass("EmptyRecord", _ => { })
            .AddRecordClass("GeneratedRecord", record => record
                .Implements<IMarker>()
                .AddAttribute<ObsoleteAttribute>(attribute => attribute.WithConstructorArgument("record"))
                .AddComponent<int>("Id")
                .AddMethod("Label", typeof(string), method => method.EmitsBody(body => body.Return(body.Constant("record")))))
            .AddRecordStruct("GeneratedRecordStruct", record => record
                .Implements<IMarker>()
                .AddAttribute<ObsoleteAttribute>(attribute => attribute.WithConstructorArgument("record-struct"))
                .AddComponent<int>("Id")
                .AddMethod("Label", typeof(string), method => method.EmitsBody(body => body.Return(body.Constant("struct")))))
            .Build();

        return context;
    }

    private static string NewAssemblyName()
        => $"DynaBee.Tests.TypeFormCoverage.{Guid.NewGuid():N}";

    public interface IMarker
    {
    }
}
