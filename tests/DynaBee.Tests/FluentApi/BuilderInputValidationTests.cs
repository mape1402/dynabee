namespace DynaBee.Tests.FluentApi;

using System.Linq.Expressions;
using System.Reflection;
using DynaBee.FluentApi;
using DynaBee.Infrastructure;
using DynaBee.Infrastructure.Configurators;

public sealed class BuilderInputValidationTests
{
    [Fact]
    public void MethodBuilder_Rejects_Invalid_Input_And_Preserves_The_Latest_Body_Strategy()
    {
        Assert.Throws<ArgumentException>(() => new BeeMethodBuilder("", typeof(int)));

        var builder = new BeeMethodBuilder("Value", typeof(int));
        Assert.Throws<ArgumentException>(() => builder.WithParameter("", typeof(int)));
        Assert.Throws<ArgumentException>(() => builder.WithTypeParameter(""));
        builder.WithTypeParameter("T", parameter => parameter.ReferenceType());
        Assert.Throws<InvalidOperationException>(() => builder.WithTypeParameter("T"));
        Assert.Throws<ArgumentNullException>(() => builder.AddAttribute(null!));
        Assert.Throws<ArgumentNullException>(() => builder.Emits(null!));
        Assert.Throws<ArgumentNullException>(() => builder.EmitsBody(null!));
        Assert.Throws<ArgumentNullException>(() => builder.EmitsLambda(null!));
        Assert.Throws<ArgumentNullException>(() => builder.EmitsExpression((LambdaExpression)null!));
        Assert.Throws<ArgumentException>(() => builder.EmitsInjectedLambda<IDisposable, int>("", _ => 1));
        Assert.Throws<ArgumentNullException>(() => builder.EmitsInjectedLambda<IDisposable, int>("Dependency", null!));
        Assert.Throws<ArgumentException>(() => builder.EmitsInjectedLambda<IDisposable, int, int>("", (_, value) => value));
        Assert.Throws<ArgumentNullException>(() => builder.EmitsInjectedLambda<IDisposable, int, int>("Dependency", null!));
        Assert.Throws<ArgumentException>(() => builder.WithMetadata("", "value"));
        Assert.Throws<ArgumentNullException>(() => builder.WithMetadata("key", null!));

        builder
            .WithParameter<int>("value")
            .AddAttribute<ObsoleteAttribute>("method")
            .AddAttribute<CLSCompliantAttribute>(attribute => attribute.WithConstructorArgument(false))
            .WithMetadata("source", "test")
            .Emits(il => il.Emit(System.Reflection.Emit.OpCodes.Ret))
            .EmitsBody(body => body.Return(body.Constant(1)))
            .EmitsLambda((Func<int, int>)(value => value + 1))
            .EmitsExpression((Expression<Func<int, int>>)(value => value + 2));

        Assert.Equal("Value", builder.Name);
        Assert.Equal(typeof(int), (Type)builder.ReturnType);
        Assert.Single(builder.Parameters);
    }

    [Fact]
    public void Class_Property_And_Constructor_Builders_Reject_Invalid_Contracts()
    {
        var classBuilder = new BeeClassBuilder(new ClassConfigurator("Generated", ClassAccessModifier.Public));
        Assert.Throws<ArgumentNullException>(() => classBuilder.Inherits(null!));
        Assert.Throws<ArgumentNullException>(() => classBuilder.Implements(null!));
        Assert.Throws<ArgumentException>(() => classBuilder.Implements(typeof(string)));
        Assert.Throws<ArgumentNullException>(() => classBuilder.RegisterAs(null!));
        Assert.Throws<ArgumentNullException>(() => classBuilder.AddAttribute(null!));
        Assert.Throws<ArgumentException>(() => classBuilder.AddProperty("", typeof(int)));
        Assert.Throws<ArgumentOutOfRangeException>(() => classBuilder.AddOperator((BeeOperatorKind)99, typeof(int), _ => { }));
        Assert.Throws<ArgumentNullException>(() => classBuilder.Intercept(null!, _ => { }));
        Assert.Throws<ArgumentNullException>(() => classBuilder.OverrideMethod(null!));
        Assert.Throws<ArgumentNullException>(() => classBuilder.OverrideProperty(null!, _ => { }));
        Assert.Throws<ArgumentException>(() => classBuilder.Inject<IDisposable>(""));

        classBuilder.Inject<IDisposable>("X");
        Assert.Throws<InvalidOperationException>(() => classBuilder.Inject<IDisposable>("X"));

        var property = new BeePropertyBuilder("Value", typeof(int));
        Assert.Throws<ArgumentNullException>(() => property.AddAttribute(null!));
        Assert.Throws<ArgumentException>(() => property.WithMetadata("", "value"));
        Assert.Throws<ArgumentNullException>(() => property.WithMetadata("key", null!));
        Assert.Same(property, property
            .AsReadOnly()
            .AsWriteOnly()
            .WithGetter()
            .WithSetter()
            .WithBackingFieldAccess(FieldAccessModifier.Internal)
            .WithGetterAccess(MethodAccessModifier.Protected)
            .WithSetterAccess(MethodAccessModifier.Private)
            .AddAttribute<ObsoleteAttribute>("property")
            .AddAttribute<CLSCompliantAttribute>(attribute => attribute.WithConstructorArgument(false))
            .WithMetadata("key", "value"));

        var baseProperty = typeof(OverrideContract).GetProperty(nameof(OverrideContract.Value))!;
        var overrideBuilder = new BeePropertyOverrideBuilder(baseProperty);
        Assert.Throws<ArgumentNullException>(() => overrideBuilder.AddAttribute(null!));
        Assert.Throws<ArgumentException>(() => overrideBuilder.WithMetadata("", "value"));
        Assert.Throws<ArgumentNullException>(() => overrideBuilder.WithMetadata("key", null!));
        Assert.Same(overrideBuilder, overrideBuilder
            .AddAttribute(BeeAttribute.Of<ObsoleteAttribute>("override"))
            .WithMetadata("key", "value")
            .Getter(_ => { })
            .Setter());

        var constructor = new BeeConstructorBuilder();
        Assert.Throws<ArgumentException>(() => constructor.WithParameter("", typeof(int)));
        Assert.Throws<ArgumentNullException>(() => constructor.Emits(null!));
        Assert.Throws<ArgumentNullException>(() => constructor.CallsBase(null!, _ => { }));
        Assert.Throws<ArgumentNullException>(() => constructor.CallsBase(typeof(ConstructorContract).GetConstructor(Type.EmptyTypes)!, (string[])null!));
        Assert.Throws<ArgumentException>(() => constructor.WithMetadata("", "value"));
        Assert.Throws<ArgumentNullException>(() => constructor.WithMetadata("key", null!));
    }

    [Fact]
    public void Indexer_And_Event_Builders_Validate_Accessors_Metadata_And_Handler_Shapes()
    {
        Assert.Throws<ArgumentException>(() => new BeeIndexerBuilder("", typeof(int), typeof(string)));
        var indexer = new BeeIndexerBuilder("Item", typeof(int), typeof(string));
        Assert.Throws<ArgumentNullException>(() => indexer.AddAttribute(null!));
        Assert.Throws<ArgumentException>(() => indexer.WithMetadata("", "value"));
        Assert.Throws<ArgumentNullException>(() => indexer.WithMetadata("key", null!));
        indexer.WithGetter(false);
        Assert.Throws<InvalidOperationException>(() => indexer.WithSetter(false));

        Assert.Throws<ArgumentException>(() => new BeeEventBuilder("", typeof(Action)));
        var @event = new BeeEventBuilder("Changed", typeof(EventHandler));
        Assert.Throws<ArgumentNullException>(() => @event.AddAttribute(null!));
        Assert.Throws<ArgumentException>(() => @event.WithMetadata("", "value"));
        Assert.Throws<ArgumentNullException>(() => @event.WithMetadata("key", null!));
        Assert.Same(@event, @event
            .WithAccess(MethodAccessModifier.Private)
            .AddAttribute<ObsoleteAttribute>("event")
            .WithMetadata("key", "value"));

        Assert.Throws<ArgumentException>(() => new BeeAssemblyBuilder("DynaBee.Tests.InvalidEvent")
            .AddClass("Invalid", type => type.AddEvent("Changed", typeof(string)))
            .Build());

        var context = new BeeAssemblyBuilder("DynaBee.Tests.PrivateEvent")
            .AddClass("Events", type => type.AddEvent<EventHandler>("Changed", builder => builder
                .WithAccess(MethodAccessModifier.Private)
                .AddAttribute<ObsoleteAttribute>("event")))
            .Build();
        var generatedEvent = context.GetClrType("Events").GetEvent("Changed", BindingFlags.Instance | BindingFlags.NonPublic)!;

        Assert.Equal("event", generatedEvent.GetCustomAttribute<ObsoleteAttribute>()!.Message);
        Assert.True(generatedEvent.GetAddMethod(true)!.IsPrivate);
        Assert.True(generatedEvent.GetRemoveMethod(true)!.IsPrivate);
    }

    public class OverrideContract
    {
        public virtual int Value { get; set; }
    }

    public class ConstructorContract
    {
        public ConstructorContract()
        {
        }
    }
}
