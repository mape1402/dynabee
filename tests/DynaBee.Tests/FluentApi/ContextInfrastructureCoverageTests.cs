namespace DynaBee.Tests.FluentApi;

using System.Reflection;
using System.Reflection.Emit;
using DynaBee.Infrastructure;
using DynaBee.Infrastructure.ContextBuilders;
using DynaBee.Infrastructure.Contexts;

public sealed class ContextInfrastructureCoverageTests
{
    [Fact]
    public void Immutable_Contexts_Validate_Lookups_Metadata_And_Typed_Metadata()
    {
        var key = new BeeMetadataKey<int>("rank");
        var element = new ElementContext("Value", ElementType.Property, new Dictionary<string, object> { [key.Name] = 4 });
        var type = new TypeContext("Type", typeof(Contract), new[] { element }, new Dictionary<string, object> { [key.Name] = 5 });
        var assembly = new AssemblyContext("Assembly", typeof(ContextInfrastructureCoverageTests).Assembly, new[] { type }, new Dictionary<string, object> { [key.Name] = 6 });

        Assert.Equal(4, element.GetMetadata(key.Name));
        Assert.True(element.TryGetMetadata(key, out int elementRank));
        Assert.Equal(4, elementRank);
        Assert.False(element.TryGetMetadata(new BeeMetadataKey<int>("missing"), out _));
        Assert.False(element.TryGetMetadata(new BeeMetadataKey<string>(key.Name), out _));
        Assert.Throws<KeyNotFoundException>(() => element.GetMetadata("missing"));

        Assert.Equal(5, type.GetMetadata(key.Name));
        Assert.Same(element, type.FindOne("Value"));
        Assert.Empty(type.Find(_ => false));
        Assert.True(type.TryGetMetadata(key, out int typeRank));
        Assert.Equal(5, typeRank);
        Assert.False(type.TryGetMetadata(new BeeMetadataKey<int>("missing"), out _));
        Assert.Throws<KeyNotFoundException>(() => type.FindOne("missing"));

        Assert.Equal(6, assembly.GetMetadata(key.Name));
        Assert.Same(type, assembly.Find("Type"));
        Assert.True(assembly.TryGetMetadata(key, out int assemblyRank));
        Assert.Equal(6, assemblyRank);
        Assert.False(assembly.TryGetMetadata(new BeeMetadataKey<int>("missing"), out _));
        Assert.Throws<KeyNotFoundException>(() => assembly.Find("missing"));
        Assert.Throws<KeyNotFoundException>(() => assembly.GetMetadata("missing"));

        Assert.Throws<ArgumentException>(() => new ElementContext("", ElementType.Field));
        Assert.Throws<ArgumentException>(() => new TypeContext("", typeof(Contract), Array.Empty<IElementContext>()));
        Assert.Throws<ArgumentNullException>(() => new TypeContext("Type", null!, Array.Empty<IElementContext>()));
        Assert.Throws<ArgumentNullException>(() => new TypeContext("Type", typeof(Contract), null!));
        Assert.Throws<ArgumentException>(() => new AssemblyContext("", typeof(Contract).Assembly, Array.Empty<ITypeContext>()));
        Assert.Throws<ArgumentNullException>(() => new AssemblyContext("Assembly", null!, Array.Empty<ITypeContext>()));
        Assert.Throws<ArgumentNullException>(() => new AssemblyContext("Assembly", typeof(Contract).Assembly, null!));
    }

    [Fact]
    public void Context_Builders_Order_Elements_And_Enforce_Registration_Contracts()
    {
        var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName($"DynaBee.Tests.Contexts.{Guid.NewGuid():N}"), AssemblyBuilderAccess.Run);
        var module = dynamicAssembly.DefineDynamicModule("Contexts");
        var assembly = new AssemblyContextBuilder("Contexts", dynamicAssembly, module);
        var typeBuilder = module.DefineType("Ordered", TypeAttributes.Public);
        var type = (TypeContextBuilder)assembly.AddTypeBuilder("Ordered", typeBuilder);
        var events = new List<string>();

        type.AddElement("Method", ElementType.Method, _ => events.Add("method"));
        type.AddElement("Field", ElementType.Field, _ => events.Add("field"));
        type.AddElement("Property", ElementType.Property, _ => events.Add("property"));
        type.AddElement(".ctor:0", ElementType.Method, _ => events.Add("constructor"));
        type.AddElement("Method", ElementType.Method, _ => events.Add("method-overload"));
        type.SetMetadata("tag", "value");

        Assert.Throws<InvalidOperationException>(() => type.AddElement("Field", ElementType.Field, _ => { }));
        Assert.Throws<ArgumentException>(() => type.SetMetadata("", "value"));
        Assert.Throws<ArgumentNullException>(() => type.SetMetadata("value", null!));
        Assert.Throws<ArgumentNullException>(() => type.RegisterField("field", null!));
        Assert.Throws<ArgumentNullException>(() => type.RegisterMethod("method", Type.EmptyTypes, null!));
        Assert.Throws<ArgumentNullException>(() => type.RegisterGenericParameter("T", null!));

        var completed = type.Build();
        Assert.Equal(new[] { "field", "property", "method", "method-overload", "constructor" }, events);
        Assert.Equal("value", completed.GetMetadata("tag"));
        Assert.False(type.TryGetField("missing", out _));
        Assert.False(type.TryGetProperty("missing", out _));
        Assert.False(type.TryGetMethod("missing", Type.EmptyTypes, out _));
        Assert.False(type.TryGetGenericParameter("missing", out _));

        Assert.Throws<InvalidOperationException>(() => assembly.AddTypeBuilder("Ordered", module.DefineType("Other", TypeAttributes.Public)));
        Assert.Throws<InvalidOperationException>(() => assembly.GetTypeBuilder("missing"));
        Assert.Throws<ArgumentException>(() => assembly.SetMetadata("", "value"));
        Assert.Throws<ArgumentNullException>(() => assembly.SetMetadata("value", null!));
    }

    [Fact]
    public void Element_Builder_Executes_Once_And_Validates_Metadata()
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName($"DynaBee.Tests.Elements.{Guid.NewGuid():N}"), AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("Elements");
        var assemblyBuilder = new AssemblyContextBuilder("Elements", assembly, module);
        var type = (TypeContextBuilder)assemblyBuilder.AddTypeBuilder("ElementType", module.DefineType("ElementType", TypeAttributes.Public));
        var calls = 0;
        var builder = new ElementContextBuilder("Element", ElementType.Field, _ => calls++, type);

        builder.SetMetadata("tag", "value");
        var context = builder.Build();

        Assert.Equal(1, calls);
        Assert.Equal("value", context.GetMetadata("tag"));
        Assert.Throws<ArgumentException>(() => builder.SetMetadata("", "value"));
        Assert.Throws<ArgumentNullException>(() => builder.SetMetadata("tag", null!));
        Assert.Throws<ArgumentException>(() => new ElementContextBuilder("", ElementType.Field, _ => { }, type));
        Assert.Throws<ArgumentNullException>(() => new ElementContextBuilder("Element", ElementType.Field, null!, type));
        Assert.Throws<ArgumentNullException>(() => new ElementContextBuilder("Element", ElementType.Field, _ => { }, null!));
    }

    [Fact]
    public void Contexts_Handle_Default_Metadata_Typed_Mismatches_And_Ambiguous_Elements()
    {
        var first = new ElementContext("Same", ElementType.Field);
        var second = new ElementContext("Same", ElementType.Property);
        var type = new TypeContext("Type", typeof(Contract), new IElementContext[] { first, second });
        var assembly = new AssemblyContext("Assembly", typeof(Contract).Assembly, new[] { type });

        Assert.Empty(first.Metadata);
        Assert.Empty(type.Metadata);
        Assert.Empty(assembly.Metadata);
        Assert.False(type.TryGetMetadata(new BeeMetadataKey<int>("rank"), out _));
        Assert.False(assembly.TryGetMetadata(new BeeMetadataKey<string>("rank"), out _));
        Assert.Throws<InvalidOperationException>(() => type.FindOne("Same"));
        Assert.Single(assembly.Find(context => context.Name == "Type"));
    }

    public interface Contract
    {
    }
}
