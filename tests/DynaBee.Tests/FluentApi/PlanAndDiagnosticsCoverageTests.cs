namespace DynaBee.Tests.FluentApi;

using System.Reflection;
using DynaBee.FluentApi;
using DynaBee.FluentApi.Diagnostics;
using DynaBee.FluentApi.Generation;
using DynaBee.Infrastructure;

public sealed class PlanAndDiagnosticsCoverageTests
{
    [Fact]
    public void Generation_Plans_Apply_Validate_Fingerprint_And_Export_Each_Member_Shape()
    {
        var key = new BeeMetadataKey<string>("coverage:plan");
        var plan = new DynaBeeGenerationPlan("DynaBee.Tests.PlanCoverage")
            .WithMetadata(key, "assembly")
            .AddClass("Models.Generated", type => type
                .Inherits(typeof(PlanBase))
                .Implements<IPlanContract>(registerInDi: false)
                .RegisterAs(typeof(IPlanContract), registerInDi: true)
                .RegisterAsConcrete(false)
                .WithMetadata(key.Name, "class")
                .AddProperty("Name", typeof(string))
                .AddConstructor(_ => { })
                .AddMethod("Format", typeof(string), method => method
                    .WithParameter<int>("value")
                    .EmitsBody(body => body.Return(body.Constant("formatted")))));

        var validation = plan.Validate();
        var source = plan.ExportSource();
        var builder = new BeeAssemblyBuilder(plan.AssemblyName);
        var context = plan.ApplyTo(builder).Build();

        Assert.False(validation.HasErrors);
        Assert.False(validation.HasWarnings);
        validation.ThrowIfErrors();
        Assert.Equal(typeof(DynaBeePlanValidationResult), validation.GetType());
        Assert.Equal(64, plan.GetFingerprint().Length);
        Assert.Contains("namespace Models;", source.Source);
        Assert.Contains("public string Name { get; set; }", source.Source);
        Assert.Contains("public string Format(int arg0)", source.Source);
        Assert.Contains("Constructor body", source.Source);
        Assert.Equal(4, source.Mappings.Count);
        Assert.NotNull(context.GetClrType("Models.Generated"));
        Assert.Equal("assembly", context.GetMetadata(key.Name));
    }

    [Fact]
    public void Generation_Plan_Validation_Reports_Each_Declarative_Conflict()
    {
        var plan = new DynaBeeGenerationPlan("DynaBee.Tests.PlanValidationCoverage")
            .AddClass("Duplicate", _ => { })
            .AddClass("Duplicate", _ => { })
            .AddClass("InvalidBase", type => type.Inherits(typeof(IDisposable)))
            .AddClass("SealedBase", type => type.Inherits(typeof(string)))
            .AddClass("Interfaces", type => type
                .Implements(typeof(string))
                .Implements(typeof(IDisposable))
                .Implements(typeof(IDisposable)))
            .AddClass("Members", type => type
                .AddProperty("Name", typeof(string))
                .AddProperty("Name", typeof(string))
                .AddMethod("Format", typeof(string), method => method.WithParameter<int>("value"))
                .AddMethod("Format", typeof(string), method => method.WithParameter<int>("value")));

        var result = plan.Validate();
        var exception = Assert.Throws<DynaBeePlanValidationException>(() => result.ThrowIfErrors());

        Assert.True(result.HasErrors);
        Assert.True(result.HasWarnings);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "DBP001");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "DBP010");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "DBP011");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "DBP020");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "DBP021");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "DBP031");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "DBP032");
        Assert.Contains("DBP001", result.Diagnostics.First(x => x.Code == "DBP001").ToString());
        Assert.Same(result, exception.Validation);
    }

    [Fact]
    public void Generation_Plan_Builders_Validate_Inputs_And_Record_Contracts()
    {
        Assert.Throws<ArgumentException>(() => new DynaBeeGenerationPlan(""));
        Assert.Throws<ArgumentException>(() => new DynaBeeGenerationPlan("Plan").WithMetadata("", "value"));
        Assert.Throws<ArgumentNullException>(() => new DynaBeeGenerationPlan("Plan").WithMetadata("key", null!));
        Assert.Throws<ArgumentNullException>(() => new DynaBeeGenerationPlan("Plan").ApplyTo(null!));
        Assert.Throws<ArgumentException>(() => new DynaBeeGeneratedClassPlanBuilder(""));

        var builder = new DynaBeeGeneratedClassPlanBuilder("Generated");
        Assert.Throws<ArgumentNullException>(() => builder.Inherits(null!));
        Assert.Throws<ArgumentNullException>(() => builder.Implements(null!));
        Assert.Throws<ArgumentException>(() => builder.ImplementInterface(typeof(string)));
        Assert.Throws<ArgumentNullException>(() => builder.ImplementInterface(null!));
        Assert.Throws<ArgumentNullException>(() => builder.RegisterAs(null!));
        Assert.Throws<ArgumentException>(() => builder.WithMetadata("", "value"));
        Assert.Throws<ArgumentNullException>(() => builder.WithMetadata("key", null!));

        var plan = new DynaBeeGenerationPlan("DynaBee.Tests.ContractPlan")
            .AddClass("Contract", type => type.ImplementInterface<IPlanContract>(contract => contract
                .RequireExplicitImplementations()
                .Property(nameof(IPlanContract.Name), property => property.AsReadOnly())
                .Method(nameof(IPlanContract.Format), method => method.EmitsBody(body => body.Return(body.Constant("contract"))))));

        var classPlan = Assert.Single(plan.Classes);

        Assert.Contains(typeof(IPlanContract), classPlan.Interfaces);
        Assert.Contains(classPlan.Members, member => member.Name == nameof(IPlanContract.Name));
        Assert.Contains(classPlan.Members, member => member.Name == nameof(IPlanContract.Format) && member.ParameterTypes.SequenceEqual(new[] { typeof(int) }));
    }

    [Fact]
    public void Diagnostics_Describe_Fields_Properties_Methods_And_Access_Modifiers()
    {
        var typeKey = new BeeMetadataKey<string>("coverage:type");
        var memberKey = new BeeMetadataKey<string>("coverage:member");
        var context = new BeeAssemblyBuilder($"DynaBee.Tests.DiagnosticsCoverage.{Guid.NewGuid():N}")
            .WithMetadata(typeKey, "assembly")
            .AddClass("DiagnosticType", ClassAccessModifier.Internal, type => type
                .WithMetadata(typeKey, "type")
                .AddField<int>("PublicField", FieldAccessModifier.Public)
                .AddField<int>("PrivateField", FieldAccessModifier.Private)
                .AddField<int>("InternalField", FieldAccessModifier.Internal)
                .AddField<int>("ProtectedField", FieldAccessModifier.Protected)
                .AddProperty<int>("ProtectedProperty", property => property
                    .WithGetterAccess(MethodAccessModifier.Protected)
                    .WithSetterAccess(MethodAccessModifier.Private)
                    .WithMetadata(memberKey, "property"))
                .AddMethod("PublicMethod", typeof(int), method => method
                    .WithMetadata(memberKey, "method")
                    .EmitsBody(body => body.Return(body.Constant(1))))
                .AddMethod("PrivateMethod", typeof(void), method => method
                    .WithAccess(MethodAccessModifier.Private)
                    .EmitsBody(body => body.Return()))
                .AddMethod("InternalMethod", typeof(void), method => method
                    .WithAccess(MethodAccessModifier.Internal)
                    .EmitsBody(body => body.Return()))
                .AddMethod("ProtectedMethod", typeof(void), method => method
                    .WithAccess(MethodAccessModifier.Protected)
                    .EmitsBody(body => body.Return())))
            .Build();

        var diagnostics = context.GetDiagnostics();
        var type = Assert.Single(diagnostics.Types);
        var json = context.ToDiagnosticsJson(indented: false);

        Assert.Equal("Internal", type.AccessModifier);
        Assert.Equal("Class", type.Kind);
        Assert.Contains("coverage:type", type.MetadataKeys);
        Assert.Contains(type.Members, member => member.Name == "PublicField" && member.AccessModifier == "Public");
        Assert.Contains(type.Members, member => member.Name == "PrivateField" && member.AccessModifier == "Private");
        Assert.Contains(type.Members, member => member.Name == "InternalField" && member.AccessModifier == "Internal");
        Assert.Contains(type.Members, member => member.Name == "ProtectedField" && member.AccessModifier == "Protected");
        Assert.Contains(type.Members, member => member.Name == "ProtectedProperty" && member.AccessModifier.Contains("Protected"));
        Assert.Contains(type.Members, member => member.Name == "PublicMethod" && member.MetadataKeys.Contains("coverage:member"));
        Assert.Contains("\"DiagnosticType\"", json);
        Assert.DoesNotContain(Environment.NewLine + "  ", json);
        Assert.Throws<ArgumentNullException>(() => DynaBeeDiagnosticsExtensions.GetDiagnostics(null!));
    }

    public interface IPlanContract
    {
        string Name { get; }

        string Format(int value);
    }

    public abstract class PlanBase
    {
        public PlanBase()
        {
        }
    }
}
