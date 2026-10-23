namespace DynaBee.Tests.FluentApi;

using System.Reflection;
using DynaBee.FluentApi;
using DynaBee.FluentApi.Diagnostics;
using DynaBee.Infrastructure;
using DynaBee.Infrastructure.Configurators;

public sealed class FluentValidationCoverageTests
{
    [Fact]
    public void Assembly_Attribute_And_Metadata_Builders_Reject_Invalid_Arguments()
    {
        Assert.Throws<ArgumentException>(() => new BeeAssemblyBuilder(" "));
        Assert.Throws<ArgumentException>(() => new BeeAssemblyBuilder("Assembly").WithVersion(" "));
        Assert.Throws<ArgumentNullException>(() => new BeeAttributeBuilder(null!));
        Assert.Throws<ArgumentException>(() => new BeeAttributeBuilder(typeof(string)));

        var attribute = new BeeAttributeBuilder(typeof(ObsoleteAttribute));
        Assert.Throws<ArgumentNullException>(() => attribute.WithConstructorArguments(null!));
        Assert.Throws<ArgumentException>(() => attribute.WithProperty(" ", true));
        Assert.Throws<ArgumentException>(() => attribute.WithField(" ", true));
        Assert.Throws<ArgumentException>(() => new BeeMetadataKey<string>(" "));

        var assembly = new BeeAssemblyBuilder("Assembly");
        Assert.Throws<ArgumentException>(() => assembly.WithMetadata(" ", "value"));
        Assert.Throws<ArgumentNullException>(() => assembly.WithMetadata("value", null!));
    }

    [Fact]
    public void Method_Constructor_And_Property_Builders_Reject_Invalid_Arguments()
    {
        var method = new BeeMethodBuilder("Run", typeof(int));
        Assert.Throws<ArgumentException>(() => new BeeMethodBuilder(" ", typeof(void)));
        Assert.Throws<ArgumentException>(() => method.WithParameter(" ", typeof(int)));
        Assert.Throws<ArgumentException>(() => method.WithTypeParameter(" "));
        method.WithTypeParameter("T");
        Assert.Throws<InvalidOperationException>(() => method.WithTypeParameter("T"));
        Assert.Throws<ArgumentNullException>(() => method.WithParameterRange(null!));
        Assert.Throws<ArgumentNullException>(() => method.Overrides(null!));
        Assert.Throws<ArgumentNullException>(() => method.ImplementsInterfaceMember(null!));
        Assert.Throws<ArgumentNullException>(() => method.AddAttribute(null!));
        Assert.Throws<ArgumentNullException>(() => method.Emits(null!));
        Assert.Throws<ArgumentNullException>(() => method.EmitsBody(null!));
        Assert.Throws<ArgumentNullException>(() => method.EmitsLambda(null!));
        Assert.Throws<ArgumentNullException>(() => method.EmitsExpression(null!));
        Assert.Throws<ArgumentException>(() => method.EmitsInjectedLambda<string, int>(" ", value => value.Length));
        Assert.Throws<ArgumentNullException>(() => method.EmitsInjectedLambda<string, int>("Dependency", null!));
        Assert.Throws<ArgumentException>(() => method.EmitsInjectedLambda<string, int, int>(" ", (value, input) => value.Length + input));
        Assert.Throws<ArgumentNullException>(() => method.EmitsInjectedLambda<string, int, int>("Dependency", null!));
        Assert.Throws<ArgumentException>(() => method.WithMetadata(" ", "value"));
        Assert.Throws<ArgumentNullException>(() => method.WithMetadata("value", null!));

        var constructor = new BeeConstructorBuilder();
        Assert.Throws<ArgumentException>(() => constructor.WithParameter(" ", typeof(int)));
        Assert.Throws<ArgumentNullException>(() => constructor.Emits(null!));
        Assert.Throws<ArgumentNullException>(() => constructor.CallsBase(null!, _ => { }));
        Assert.Throws<ArgumentNullException>(() => constructor.WithMetadata("value", null!));
        Assert.Throws<ArgumentException>(() => constructor.WithMetadata(" ", "value"));
        Assert.Throws<ArgumentException>(() => new BeeConstructorBaseCallBuilder().Argument(" "));

        var property = new BeePropertyBuilder("Value", typeof(int));
        Assert.Throws<ArgumentException>(() => new BeePropertyBuilder(" ", typeof(int)));
        Assert.Throws<ArgumentNullException>(() => property.AddAttribute(null!));
        Assert.Throws<ArgumentException>(() => property.WithMetadata(" ", "value"));
        Assert.Throws<ArgumentNullException>(() => property.WithMetadata("value", null!));
    }

    [Fact]
    public void Generic_And_Class_Builders_Protect_Their_Contracts()
    {
        Assert.Throws<ArgumentException>(() => new BeeGenericTypeParameterBuilder(" "));
        var parameter = new BeeGenericTypeParameterBuilder("T");
        Assert.Throws<ArgumentNullException>(() => parameter.Inherits(null!));
        Assert.Throws<ArgumentException>(() => parameter.Inherits(typeof(IDisposable)));
        Assert.Throws<ArgumentException>(() => parameter.Inherits(typeof(int)));
        Assert.Throws<ArgumentNullException>(() => parameter.Implements(null!));
        Assert.Throws<ArgumentException>(() => parameter.Implements(typeof(string)));
        parameter.Inherits(typeof(Exception)).Implements<IDisposable>().Implements<IDisposable>();
        Assert.Contains(typeof(IDisposable), parameter.InterfaceConstraints);

        var configurator = new ClassConfigurator("Generated", ClassAccessModifier.Public);
        var type = new BeeClassBuilder(configurator);
        Assert.Throws<ArgumentNullException>(() => new BeeClassBuilder(null!));
        Assert.Throws<ArgumentNullException>(() => type.Implements(null!));
        Assert.Throws<ArgumentException>(() => type.Implements(typeof(string)));
        Assert.Throws<ArgumentNullException>(() => type.Intercept(null!, _ => { }));
        Assert.Throws<ArgumentNullException>(() => type.AddElementConfigurator(null!));
        Assert.Throws<ArgumentException>(() => type.Inject<string>(" "));
        type.Inject<string>("Dependency");
        Assert.Throws<InvalidOperationException>(() => type.Inject<string>("Dependency"));
        Assert.Throws<ArgumentOutOfRangeException>(() => type.AddOperator((BeeOperatorKind)999, typeof(int), _ => { }));
    }

    [Fact]
    public void Interface_Contract_Builder_Reports_Missing_Ambiguous_And_Duplicate_Members()
    {
        var owner = new BeeClassBuilder(new ClassConfigurator("Generated", ClassAccessModifier.Public));
        var contract = new BeeInterfaceContractBuilder(owner, typeof(IContractWithMembers), false);

        Assert.Throws<ArgumentException>(() => contract.Method(" ", _ => { }));
        Assert.Throws<MissingMethodException>(() => contract.Method("Missing", _ => { }));
        Assert.Throws<InvalidOperationException>(() => contract.Method(nameof(IContractWithMembers.Overloaded), _ => { }));
        Assert.Throws<ArgumentException>(() => contract.Method(" ", Array.Empty<Type>(), _ => { }));
        Assert.Throws<ArgumentNullException>(() => contract.Method(nameof(IContractWithMembers.Get), null!, _ => { }));
        Assert.Throws<MissingMethodException>(() => contract.Method(nameof(IContractWithMembers.Get), new[] { typeof(int) }, _ => { }));
        Assert.Throws<ArgumentException>(() => contract.Property(" ", _ => { }));
        Assert.Throws<MissingMemberException>(() => contract.Property("Missing", _ => { }));
        Assert.Throws<ArgumentNullException>(() => contract.Property(nameof(IContractWithMembers.Value), null!));
        contract.Property(nameof(IContractWithMembers.Value), _ => { });
        Assert.Throws<InvalidOperationException>(() => contract.Property(nameof(IContractWithMembers.Value), _ => { }));
        Assert.Throws<ArgumentException>(() => contract.Event(" ", _ => { }));
        Assert.Throws<MissingMemberException>(() => contract.Event("Missing", _ => { }));
        Assert.Throws<ArgumentNullException>(() => contract.Event(nameof(IContractWithMembers.Changed), null!));
        contract.Event(nameof(IContractWithMembers.Changed), _ => { });
        Assert.Throws<InvalidOperationException>(() => contract.Event(nameof(IContractWithMembers.Changed), _ => { }));

        contract.Method(nameof(IContractWithMembers.Get), _ => { });
        Assert.Throws<InvalidOperationException>(() => contract.Method(nameof(IContractWithMembers.Get), _ => { }));
        Assert.Throws<InvalidOperationException>(() => new BeeInterfaceContractBuilder(owner, typeof(IContractWithMembers), false)
            .RequireExplicitImplementations()
            .Apply());
    }

    [Fact]
    public void Diagnostics_Reports_All_Generated_Field_And_Method_Access_Modifiers()
    {
        var context = new BeeAssemblyBuilder($"DynaBee.Tests.Diagnostics.{Guid.NewGuid():N}")
            .AddClass("Visibility", type => type
                .AddField<int>("PublicField", FieldAccessModifier.Public)
                .AddField<int>("PrivateField", FieldAccessModifier.Private)
                .AddField<int>("InternalField", FieldAccessModifier.Internal)
                .AddField<int>("ProtectedField", FieldAccessModifier.Protected)
                .AddField<int>("ProtectedInternalField", FieldAccessModifier.ProtectedInternal)
                .AddField<int>("PrivateProtectedField", FieldAccessModifier.PrivateProtected)
                .AddMethod("PublicMethod", typeof(void), method => method.WithAccess(MethodAccessModifier.Public))
                .AddMethod("PrivateMethod", typeof(void), method => method.WithAccess(MethodAccessModifier.Private))
                .AddMethod("InternalMethod", typeof(void), method => method.WithAccess(MethodAccessModifier.Internal))
                .AddMethod("ProtectedMethod", typeof(void), method => method.WithAccess(MethodAccessModifier.Protected))
                .AddMethod("ProtectedInternalMethod", typeof(void), method => method.WithAccess(MethodAccessModifier.ProtectedInternal))
                .AddMethod("PrivateProtectedMethod", typeof(void), method => method.WithAccess(MethodAccessModifier.PrivateProtected))
                .AddProperty<int>("ReadOnly", property => property.AsReadOnly()))
            .Build();

        var members = context.GetDiagnostics().Types.Single().Members;

        Assert.Equal("Public", members.Single(x => x.Name == "PublicField").AccessModifier);
        Assert.Equal("Private", members.Single(x => x.Name == "PrivateField").AccessModifier);
        Assert.Equal("Internal", members.Single(x => x.Name == "InternalField").AccessModifier);
        Assert.Equal("Protected", members.Single(x => x.Name == "ProtectedField").AccessModifier);
        Assert.Equal("ProtectedInternal", members.Single(x => x.Name == "ProtectedInternalField").AccessModifier);
        Assert.Equal("PrivateProtected", members.Single(x => x.Name == "PrivateProtectedField").AccessModifier);
        Assert.Equal("Public", members.Single(x => x.Name == "PublicMethod").AccessModifier);
        Assert.Equal("Private", members.Single(x => x.Name == "PrivateMethod").AccessModifier);
        Assert.Equal("Internal", members.Single(x => x.Name == "InternalMethod").AccessModifier);
        Assert.Equal("Protected", members.Single(x => x.Name == "ProtectedMethod").AccessModifier);
        Assert.Equal("ProtectedInternal", members.Single(x => x.Name == "ProtectedInternalMethod").AccessModifier);
        Assert.Equal("PrivateProtected", members.Single(x => x.Name == "PrivateProtectedMethod").AccessModifier);
        Assert.Contains("get:Public", members.Single(x => x.Name == "ReadOnly").Signature);
        Assert.Contains("set:-", members.Single(x => x.Name == "ReadOnly").Signature);
    }

    public interface IContractWithMembers
    {
        int Value { get; set; }

        event EventHandler Changed;

        int Get();

        void Overloaded();

        void Overloaded(int value);
    }
}
