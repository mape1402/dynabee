namespace DynaBee.Tests.FluentApi;

using DynaBee.FluentApi;
using DynaBee.FluentApi.Extensions;
using DynaBee.FluentApi.Generation;
using Microsoft.Extensions.DependencyInjection;

public sealed class GenerationExtensionsCoverageTests
{
    [Fact]
    public void Metadata_Schemas_Distinguish_Missing_Null_Typed_And_Invalid_Values()
    {
        var key = new BeeMetadataKey<int>("priority");
        var schema = new DynaBeeMetadataSchema<int>(key, value => value > 0);
        var validation = new DynaBeeExtensionValidationContext();

        schema.Validate(new Dictionary<string, object>(), null, validation);
        schema.Validate(new Dictionary<string, object> { [key.Name] = null! }, "NullType", validation);
        schema.Validate(new Dictionary<string, object> { [key.Name] = 0 }, "InvalidType", validation);
        schema.Validate(new Dictionary<string, object> { [key.Name] = 1 }, "ValidType", validation);

        Assert.Equal(2, validation.Diagnostics.Count);
        Assert.Contains(validation.Diagnostics, diagnostic => diagnostic.Code == "DBX001" && diagnostic.TypeName == "NullType");
        Assert.Contains(validation.Diagnostics, diagnostic => diagnostic.Code == "DBX002" && diagnostic.TypeName == "InvalidType");
    }
    [Fact]
    public void GenerationServices_Register_Extensions_Schemas_And_Execute_Callbacks()
    {
        RecordingExtension.Reset();
        var rank = new BeeMetadataKey<int>("rank");
        var services = new ServiceCollection();
        services
            .AddDynaBeeGeneration()
            .AddDynaBeeExtension<RecordingExtension>()
            .AddDynaBeeMetadataSchema(new DynaBeeMetadataSchema<int>(rank, value => value > 0, "Rank must be positive."))
            .AddDynaBeeExtensionsFromAssemblies(typeof(GenerationExtensionsCoverageTests).Assembly, null!);

        var provider = services.BuildServiceProvider();
        var executor = provider.GetRequiredService<IDynaBeeGenerationPlanExecutor>();
        var extensions = provider.GetServices<IDynaBeeGenerationExtension>();
        var plan = new DynaBeeGenerationPlan(NewAssemblyName())
            .WithMetadata(rank, 1)
            .AddClass("Generated", type => type
                .WithMetadata("extension.enabled", true)
                .AddMethod("Value", typeof(int), method => method.EmitsBody(body => body.Return(body.Constant(4)))));

        var first = executor.Build(plan);
        var second = executor.Build(plan);
        var uncached = executor.Build(plan, useCache: false);

        Assert.NotEmpty(extensions);
        Assert.Same(first, second);
        Assert.NotSame(first, uncached);
        Assert.Equal("configured", first.GetMetadata("extension.mode"));
        Assert.True(RecordingExtension.ValidateCount >= 2);
        Assert.True(RecordingExtension.ConfigureCount >= 2);
        Assert.True(RecordingExtension.GeneratedCount >= 2);
    }

    [Fact]
    public void GenerationExtension_Support_Reports_Diagnostics_Validates_Schemas_And_Clears_Caches()
    {
        var rank = new BeeMetadataKey<int>("rank");
        var validation = new DynaBeeExtensionValidationContext();
        var schema = new DynaBeeMetadataSchema<int>(rank, value => value > 0, "Rank must be positive.");
        var baseExtension = new EmptyExtension();
        var plan = new DynaBeeGenerationPlan(NewAssemblyName()).WithMetadata("rank", "wrong");

        schema.Validate(plan.Metadata, null, validation);
        validation.Warning("DBX900", "warning", "Generated", "Value");
        baseExtension.Validate(plan, validation);
        baseExtension.Configure(plan, new BeeAssemblyBuilder(NewAssemblyName()));
        baseExtension.OnGenerated(plan, new BeeAssemblyBuilder(NewAssemblyName()).AddClass("Empty", _ => { }).Build());

        Assert.Equal(2, validation.Diagnostics.Count);
        Assert.Equal(DynaBeePlanDiagnosticSeverity.Error, validation.Diagnostics[0].Severity);
        Assert.Equal(DynaBeePlanDiagnosticSeverity.Warning, validation.Diagnostics[1].Severity);

        var cache = new DynaBeeGenerationPlanCache();
        var created = 0;
        var first = cache.GetOrAdd("key", () =>
        {
            created++;
            return new BeeAssemblyBuilder(NewAssemblyName()).AddClass("A", _ => { }).Build();
        });
        var second = cache.GetOrAdd("key", () => throw new InvalidOperationException("Factory should not run."));
        cache.Clear();
        var third = cache.GetOrAdd("key", () =>
        {
            created++;
            return new BeeAssemblyBuilder(NewAssemblyName()).AddClass("B", _ => { }).Build();
        });

        Assert.Same(first, second);
        Assert.NotSame(first, third);
        Assert.Equal(2, created);
        Assert.Throws<ArgumentException>(() => cache.GetOrAdd("", () => first));
        Assert.Throws<ArgumentNullException>(() => cache.GetOrAdd("missing", null!));
    }

    [Fact]
    public void GenerationPlans_Expose_Override_Registration_And_Source_Map_Metadata()
    {
        var property = typeof(VirtualBase).GetProperty(nameof(VirtualBase.Value))!;
        var plan = new DynaBeeGenerationPlan(NewAssemblyName())
            .AddClass("Generated", type => type
                .Inherits(typeof(VirtualBase))
                .RegisterAsConcrete(false)
                .OverrideProperty(property, overrideProperty => overrideProperty.Getter(getter => getter.ReturnsConstant(9))));

        var export = plan.ExportSource();
        var generated = plan.Classes.Single();

        Assert.False(generated.RegisterAsConcrete);
        Assert.Contains(generated.Members, member => member.Name == nameof(VirtualBase.Value));
        Assert.NotEmpty(export.Mappings);
        Assert.All(export.Mappings, entry => Assert.True(entry.Line > 0));
    }

    [Fact]
    public void GenerationServiceExtensions_Validate_Null_Arguments()
    {
        Assert.Throws<ArgumentNullException>(() => DynaBeeGenerationServiceCollectionExtensions.AddDynaBeeGeneration(null!));
        Assert.Throws<ArgumentNullException>(() => DynaBeeGenerationServiceCollectionExtensions.AddDynaBeeExtension<RecordingExtension>(null!));
        Assert.Throws<ArgumentNullException>(() => DynaBeeGenerationServiceCollectionExtensions.AddDynaBeeMetadataSchema<int>(null!, new DynaBeeMetadataSchema<int>(new BeeMetadataKey<int>("x"))));
        Assert.Throws<ArgumentNullException>(() => new ServiceCollection().AddDynaBeeMetadataSchema<int>(null!));
        Assert.Throws<ArgumentNullException>(() => DynaBeeGenerationServiceCollectionExtensions.AddDynaBeeExtensionsFromAssemblies(null!, typeof(GenerationExtensionsCoverageTests).Assembly));
    }

    private static string NewAssemblyName()
        => $"DynaBee.Tests.GenerationExtensions.{Guid.NewGuid():N}";

    public class VirtualBase
    {
        public virtual int Value { get; set; }
    }

    public sealed class RecordingExtension : DynaBeeGenerationExtension
    {
        public static int ValidateCount { get; private set; }

        public static int ConfigureCount { get; private set; }

        public static int GeneratedCount { get; private set; }

        public static void Reset()
        {
            ValidateCount = 0;
            ConfigureCount = 0;
            GeneratedCount = 0;
        }

        public override void Validate(DynaBeeGenerationPlan plan, DynaBeeExtensionValidationContext context)
            => ValidateCount++;

        public override void Configure(DynaBeeGenerationPlan plan, IBeeAssemblyBuilder builder)
        {
            ConfigureCount++;
            builder.WithMetadata("extension.mode", "configured");
        }

        public override void OnGenerated(DynaBeeGenerationPlan plan, IAssemblyContext context)
            => GeneratedCount++;
    }

    private sealed class EmptyExtension : DynaBeeGenerationExtension
    {
    }
}
