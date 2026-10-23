namespace DynaBee.Tests.FluentApi;

using DynaBee.FluentApi;
using DynaBee.FluentApi.Generation;

public sealed class GenerationPlanValidationTests
{
    [Fact]
    public void Validate_Reports_Declarative_Errors_Without_Emitting_An_Assembly()
    {
        var plan = new DynaBeeGenerationPlan("DynaBee.Tests.Invalid.Plan")
            .AddClass("Duplicate", type => type.AddProperty("Name", typeof(string)))
            .AddClass("Duplicate", type => type.AddProperty("Name", typeof(string)))
            .AddClass("InvalidBase", type => type.Inherits(typeof(string)))
            .AddClass("DuplicateMembers", type => type
                .AddProperty("Name", typeof(string))
                .AddProperty("Name", typeof(string)));

        var result = plan.Validate();

        Assert.True(result.HasErrors);
        Assert.Contains(result.Diagnostics, x => x.Code == "DBP001");
        Assert.Contains(result.Diagnostics, x => x.Code == "DBP011");
        Assert.Contains(result.Diagnostics, x => x.Code == "DBP031");
        Assert.Throws<DynaBeePlanValidationException>(() => result.ThrowIfErrors());
    }

    [Fact]
    public void ApplyTo_Stops_Before_Emission_When_Plan_Is_Invalid()
    {
        var plan = new DynaBeeGenerationPlan("DynaBee.Tests.Invalid.Apply")
            .AddClass("Broken", type => type.Inherits(typeof(string)));
        var builder = new BeeAssemblyBuilder(plan.AssemblyName);

        var exception = Assert.Throws<DynaBeePlanValidationException>(() => plan.ApplyTo(builder));

        Assert.Contains("DBP011", exception.Message);
    }

    [Fact]
    public void Validate_Allows_Overloaded_Method_Names()
    {
        var plan = new DynaBeeGenerationPlan("DynaBee.Tests.Valid.Overloads")
            .AddClass("Formatter", type => type
                .AddMethod("Format", typeof(string), method => method.WithParameter<int>("value"))
                .AddMethod("Format", typeof(string), method => method.WithParameter<decimal>("value")));

        var result = plan.Validate();

        Assert.False(result.HasErrors);
    }
}
