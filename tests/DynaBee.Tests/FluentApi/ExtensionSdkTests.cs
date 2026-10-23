namespace DynaBee.Tests.FluentApi;

using DynaBee;
using DynaBee.FluentApi;
using DynaBee.FluentApi.Extensions;
using DynaBee.FluentApi.Generation;
using Microsoft.Extensions.DependencyInjection;

public sealed class ExtensionSdkTests
{
    [Fact]
    public void Executor_Runs_DI_Extension_Lifecycle_And_Exposes_Metadata()
    {
        var services = new ServiceCollection()
            .AddDynaBeeGeneration()
            .AddDynaBeeExtension<TrackingExtension>();
        var provider = services.BuildServiceProvider();
        var executor = provider.GetRequiredService<IDynaBeeGenerationPlanExecutor>();
        var extension = provider.GetRequiredService<IEnumerable<IDynaBeeGenerationExtension>>().OfType<TrackingExtension>().Single();
        var plan = new DynaBeeGenerationPlan("DynaBee.Tests.Extensions")
            .AddClass("Generated", type => type.WithMetadata("extension.enabled", true));

        var context = executor.Build(plan, useCache: false);

        Assert.True(context.TryGetMetadata("extension.configured", out var configured));
        Assert.Equal(true, configured);
        Assert.Equal(1, extension.ValidationCalls);
        Assert.Equal(1, extension.GeneratedCalls);
        Assert.Equal("Generated", extension.LastTypeName);
    }

    [Fact]
    public void MetadataSchemas_Reject_Invalid_Extension_Metadata_Before_Emission()
    {
        var key = new BeeMetadataKey<int>("sample.priority");
        var services = new ServiceCollection()
            .AddDynaBeeGeneration()
            .AddDynaBeeMetadataSchema(new DynaBeeMetadataSchema<int>(key, value => value > 0, "Priority must be positive."));
        var executor = services.BuildServiceProvider().GetRequiredService<IDynaBeeGenerationPlanExecutor>();
        var plan = new DynaBeeGenerationPlan("DynaBee.Tests.Extensions.Invalid")
            .WithMetadata(key.Name, -1)
            .AddClass("Generated", _ => { });

        var exception = Assert.Throws<DynaBeePlanValidationException>(() => executor.Build(plan, useCache: false));

        Assert.Contains("DBX002", exception.Message);
    }

    public sealed class TrackingExtension : DynaBeeGenerationExtension
    {
        public int ValidationCalls { get; private set; }

        public int GeneratedCalls { get; private set; }

        public string LastTypeName { get; private set; }

        public override void Validate(DynaBeeGenerationPlan plan, DynaBeeExtensionValidationContext context)
        {
            ValidationCalls++;
            if (!plan.Classes.All(x => x.Metadata.ContainsKey("extension.enabled")))
                context.Error("TEST001", "The extension requires enabled metadata on every type.");
        }

        public override void Configure(DynaBeeGenerationPlan plan, IBeeAssemblyBuilder builder)
            => builder.WithMetadata("extension.configured", true);

        public override void OnGenerated(DynaBeeGenerationPlan plan, IAssemblyContext context)
        {
            GeneratedCalls++;
            LastTypeName = context.Find(_ => true).Single().Name;
        }
    }
}
