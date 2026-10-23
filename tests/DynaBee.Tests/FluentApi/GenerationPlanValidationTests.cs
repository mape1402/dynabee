namespace DynaBee.Tests.FluentApi;

using System.Reflection;
using System.Reflection.Emit;
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

    [Fact]
    public void Validate_Reports_Duplicate_Method_Signatures()
    {
        var plan = new DynaBeeGenerationPlan("DynaBee.Tests.Invalid.Methods")
            .AddClass("Formatter", type => type
                .AddMethod("Format", typeof(string), method => method.WithParameter<int>("value"))
                .AddMethod("Format", typeof(string), method => method.WithParameter<int>("value")));

        var result = plan.Validate();

        Assert.Contains(result.Diagnostics, x => x.Code == "DBP032");
    }

    [Fact]
    public void SourceExporter_Rejects_A_Null_Plan()
    {
        Assert.Throws<ArgumentNullException>(() => new DynaBeeSourceExporter().Export(null!));
    }

    [Fact]
    public void Declarative_Overrides_Support_Reflected_Parameters_Without_Names()
    {
        var baseType = CreateBaseTypeWithUnnamedParameter();
        var baseMethod = baseType.GetMethod("Identity")!;
        var plan = new DynaBeeGenerationPlan("DynaBee.Tests.PlanUnnamedOverride")
            .AddClass("Derived", type => type
                .Inherits(baseType)
                .OverrideMethod(baseMethod, method => method
                    .EmitsBody(body => body.Return(body.Parameter<int>("arg0")))));

        var context = plan.ApplyTo(new BeeAssemblyBuilder(plan.AssemblyName)).Build();

        Assert.Null(baseMethod.GetParameters()[0].Name);
        Assert.Equal(9, baseMethod.Invoke(context.CreateInstance("Derived"), new object[] { 9 }));
    }

    private static Type CreateBaseTypeWithUnnamedParameter()
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName($"DynaBee.Tests.UnnamedPlanBase.{Guid.NewGuid():N}"),
            AssemblyBuilderAccess.Run);
        var type = assembly.DefineDynamicModule("Contracts").DefineType(
            "UnnamedPlanBase",
            TypeAttributes.Public | TypeAttributes.Class);
        var constructor = type.DefineConstructor(
            MethodAttributes.Public,
            CallingConventions.Standard,
            Type.EmptyTypes);
        var constructorIl = constructor.GetILGenerator();
        constructorIl.Emit(OpCodes.Ldarg_0);
        constructorIl.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes)!);
        constructorIl.Emit(OpCodes.Ret);

        var method = type.DefineMethod(
            "Identity",
            MethodAttributes.Public | MethodAttributes.Virtual,
            typeof(int),
            new[] { typeof(int) });
        method.GetILGenerator().Emit(OpCodes.Ldarg_1);
        method.GetILGenerator().Emit(OpCodes.Ret);
        return type.CreateTypeInfo()!.AsType();
    }
}
