namespace DynaBee.Tests.Testing;

using DynaBee.FluentApi.Generation;
using DynaBee.FluentApi.DependencyInjection;
using DynaBee.Testing;

public sealed class DynaBeeTestSpecificationTests
{
    [Fact]
    public void Constructor_Requires_An_Assembly_Name()
    {
        Assert.Throws<ArgumentException>(() => new DynaBeeTestSpecification(null!));
        Assert.Throws<ArgumentException>(() => new DynaBeeTestSpecification(" "));
    }

    [Fact]
    public void Configure_Stores_Callback_And_Rejects_Null()
    {
        var specification = new DynaBeeTestSpecification("Tests.Specification");

        Assert.Same(specification, specification.Configure(_ => { }));
        Assert.Single(specification.Configurations);
        Assert.Throws<ArgumentNullException>(() => specification.Configure(null!));
    }

    [Fact]
    public void UsePlan_Requires_A_Matching_Assembly()
    {
        var specification = new DynaBeeTestSpecification("Tests.Specification");

        Assert.Throws<ArgumentNullException>(() => specification.UsePlan(null!));
        Assert.Throws<InvalidOperationException>(() => specification.UsePlan(new DynaBeeGenerationPlan("Tests.Other")));
    }

    [Fact]
    public async Task UsePlan_Stores_The_Plan_And_Exports_Its_Source()
    {
        var plan = new DynaBeeGenerationPlan("Tests.Plan")
            .AddClass("Generated", type => type.AddProperty("Name", typeof(string)));
        var specification = new DynaBeeTestSpecification("Tests.Plan");

        Assert.Same(specification, specification.UsePlan(plan));
        Assert.Single(specification.Configurations);

        var result = await new DynaBeeTestGenerator(new DynaBeeAssemblyBuilderFactory())
            .GenerateAssemblyAsync(specification);

        Assert.Single(result.GeneratedSources);
        Assert.Contains("Generated", result.GeneratedSources["DynaBee.Plan.g.cs"]);
    }

    [Fact]
    public void AddGeneratedSource_Validates_Inputs_And_Replaces_Same_Path()
    {
        var specification = new DynaBeeTestSpecification("Tests.Sources");

        Assert.Throws<ArgumentException>(() => specification.AddGeneratedSource(null!, "source"));
        Assert.Throws<ArgumentException>(() => specification.AddGeneratedSource(" ", "source"));
        Assert.Throws<ArgumentNullException>(() => specification.AddGeneratedSource("snapshot.cs", null!));

        specification.AddGeneratedSource("snapshot.cs", "first");
        Assert.Same(specification, specification.AddGeneratedSource("snapshot.cs", "second"));

        Assert.Equal("second", specification.GeneratedSources["snapshot.cs"]);
    }

    [Fact]
    public void Factory_Methods_Validate_Arguments()
    {
        Assert.Throws<ArgumentNullException>(() => DynaBeeTestSpecification.Create("Tests.Create", null!));
        Assert.Throws<ArgumentNullException>(() => DynaBeeTestSpecification.FromPlan(null!));

        var specification = DynaBeeTestSpecification.Create("Tests.Create", _ => { });

        Assert.Equal("Tests.Create", specification.AssemblyName);
    }
}
