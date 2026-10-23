namespace DynaBee.Tests.FluentApi;

using System.Reflection;
using DynaBee.FluentApi;
using DynaBee.Infrastructure;

public sealed class FluentSurfaceCoverageTests
{
    [Fact]
    public void Attributes_Apply_Constructor_Properties_And_Fields_To_Generated_Members()
    {
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddInterface("IAnnotated", type => type
                .Inherits<IDisposable>()
                .AddAttribute<CoverageAttribute>(attribute => attribute
                    .WithConstructorArguments("interface")
                    .WithProperty(nameof(CoverageAttribute.Level), 1)
                    .WithField(nameof(CoverageAttribute.Marker), "I"))
                .AddMethod<string>("Format", method => method
                    .WithParameter<string>("value")
                    .WithAccess(MethodAccessModifier.Public)
                    .AddAttribute<CoverageAttribute>(attribute => attribute
                        .WithConstructorArgument("method")
                        .WithProperty(nameof(CoverageAttribute.Level), 2)))
                .AddProperty<string>("Name", property => property
                    .AsReadOnly()
                    .WithGetter(true)
                    .WithGetterAccess(MethodAccessModifier.Public)
                    .AddAttribute<CoverageAttribute>(attribute => attribute
                        .WithConstructorArgument("property")
                        .WithField(nameof(CoverageAttribute.Marker), "P"))))
            .Build();

        var type = context.GetClrType("IAnnotated");
        var typeAttribute = type.GetCustomAttribute<CoverageAttribute>()!;
        var methodAttribute = type.GetMethod("Format")!.GetCustomAttribute<CoverageAttribute>()!;
        var property = type.GetProperty("Name")!;
        var propertyAttribute = property.GetCustomAttribute<CoverageAttribute>()!;

        Assert.True(type.IsAssignableTo(typeof(IDisposable)));
        Assert.Equal("interface", typeAttribute.Name);
        Assert.Equal(1, typeAttribute.Level);
        Assert.Equal("I", typeAttribute.Marker);
        Assert.Equal("method", methodAttribute.Name);
        Assert.Equal(2, methodAttribute.Level);
        Assert.Equal("property", propertyAttribute.Name);
        Assert.Equal("P", propertyAttribute.Marker);
        Assert.NotNull(property.GetMethod);
        Assert.Null(property.SetMethod);
    }

    [Fact]
    public void Attribute_Building_Validates_Constructors_Properties_And_Fields()
    {
        Assert.Throws<ArgumentException>(() => BeeAttribute.Of(typeof(string)));
        Assert.Throws<ArgumentException>(() => BeeAttribute.Of<CoverageAttribute>().WithProperty("", 1));
        Assert.Throws<ArgumentException>(() => BeeAttribute.Of<CoverageAttribute>().WithField("", 1));

        var badConstructor = BeeAttribute.Of<CoverageAttribute>(1);
        var missingProperty = BeeAttribute.Of<CoverageAttribute>("valid").WithProperty("Missing", 1);
        var missingField = BeeAttribute.Of<CoverageAttribute>("valid").WithField("Missing", 1);

        Assert.Throws<InvalidOperationException>(() => badConstructor.Build());
        Assert.Throws<InvalidOperationException>(() => missingProperty.Build());
        Assert.Throws<InvalidOperationException>(() => missingField.Build());
    }

    [Fact]
    public void Indexers_Events_And_Metadata_Are_Generated_With_Configured_Accessors()
    {
        var key = new BeeMetadataKey<string>("coverage:key");
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("ObservableStore", type => type
                .AddIndexer("Item", typeof(int), typeof(string), indexer => indexer
                    .WithGetter(true)
                    .WithSetter(true)
                    .WithGetterAccess(MethodAccessModifier.Public)
                    .WithSetterAccess(MethodAccessModifier.Public)
                    .WithMetadata(key.Name, "items")
                    .AddAttribute(BeeAttribute.Of<CoverageAttribute>("indexer")))
                .AddEvent<EventHandler>("Changed", @event => @event
                    .WithAccess(MethodAccessModifier.Public)
                    .WithMetadata(key.Name, "changed")
                    .AddAttribute<CoverageAttribute>("event")))
            .Build();

        var type = context.GetClrType("ObservableStore");
        var instance = Activator.CreateInstance(type)!;
        var indexer = type.GetProperty("Item")!;
        var @event = type.GetEvent("Changed")!;

        indexer.SetValue(instance, "one", new object[] { 1 });

        Assert.Equal("one", indexer.GetValue(instance, new object[] { 1 }));
        Assert.True(indexer.GetMethod!.IsPublic);
        Assert.True(indexer.SetMethod!.IsPublic);
        Assert.Equal("indexer", indexer.GetCustomAttribute<CoverageAttribute>()!.Name);
        Assert.Equal("event", @event.GetCustomAttribute<CoverageAttribute>()!.Name);
        Assert.True(context.Find("ObservableStore").FindOne("Item").TryGetMetadata(key, out string indexMetadata));
        Assert.True(context.Find("ObservableStore").FindOne("Changed").TryGetMetadata(key, out string eventMetadata));
        Assert.Equal("items", indexMetadata);
        Assert.Equal("changed", eventMetadata);
    }

    [Fact]
    public void Indexer_And_Event_Builders_Reject_Invalid_Configuration()
    {
        var indexer = Assert.Throws<InvalidOperationException>(() =>
            new BeeAssemblyBuilder(NewAssemblyName())
                .AddClass("Broken", type => type
                    .AddIndexer<int, string>(configuration => configuration
                        .WithGetter(false)
                        .WithSetter(false))));

        Assert.Contains("at least a getter or a setter", indexer.Message);
        Assert.Throws<ArgumentException>(() => new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("Broken", type => type.AddEvent<EventHandler>("Changed", @event => @event.WithMetadata("", "value"))));
    }

    [Fact]
    public void Generic_Parameters_Apply_Constraints_And_Reject_Invalid_Constraints()
    {
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddGenericClass("Constrained", generic => generic
                .WithTypeParameter("T", parameter => parameter
                    .ReferenceType()
                    .HasDefaultConstructor()
                    .Inherits(typeof(CoverageBase))
                    .Implements<IDisposable>()
                    .Implements(typeof(ICloneable)))
                .AddField("Value", generic.TypeParameter("T"), FieldAccessModifier.Private)
                .AddConstructor(_ => { })
                .Configure(type => type.AddAutoProperty<string>("Name")))
            .Build();

        var parameter = context.GetClrType("Constrained").GetGenericArguments().Single();
        var attributes = parameter.GenericParameterAttributes;

        Assert.True(attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint));
        Assert.True(attributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint));
        Assert.Contains(typeof(CoverageBase), parameter.GetGenericParameterConstraints());
        Assert.Contains(typeof(IDisposable), parameter.GetGenericParameterConstraints());
        Assert.Contains(typeof(ICloneable), parameter.GetGenericParameterConstraints());

        Assert.Throws<ArgumentException>(() => new BeeAssemblyBuilder(NewAssemblyName())
            .AddGenericClass("Broken", generic => generic.WithTypeParameter("T", parameter => parameter.Inherits(typeof(IDisposable)))));
        Assert.Throws<ArgumentException>(() => new BeeAssemblyBuilder(NewAssemblyName())
            .AddGenericClass("Broken", generic => generic.WithTypeParameter("T", parameter => parameter.Implements(typeof(string)))));
        Assert.Throws<InvalidOperationException>(() => new BeeAssemblyBuilder(NewAssemblyName())
            .AddGenericClass("Broken", generic => generic.WithTypeParameter("T").WithTypeParameter("T")));
        Assert.Throws<KeyNotFoundException>(() => new BeeAssemblyBuilder(NewAssemblyName())
            .AddGenericClass("Broken", generic => generic.TypeParameter("Missing")));
    }

    private static string NewAssemblyName()
        => $"DynaBee.Tests.Coverage.{Guid.NewGuid():N}";

    [AttributeUsage(AttributeTargets.All)]
    public sealed class CoverageAttribute : Attribute
    {
        public CoverageAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public int Level { get; set; }

        public string Marker;
    }

    public class CoverageBase
    {
        public CoverageBase()
        {
        }
    }
}
