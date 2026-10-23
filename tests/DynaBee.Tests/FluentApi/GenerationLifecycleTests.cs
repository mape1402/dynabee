namespace DynaBee.Tests.FluentApi;

using DynaBee.FluentApi;
using DynaBee.FluentApi.Generation;
using Microsoft.Extensions.DependencyInjection;

public sealed class GenerationLifecycleTests
{
    [Fact]
    public void GetFingerprint_Is_Stable_For_Equivalent_Declarative_Plans()
    {
        var first = CreatePlan("DynaBee.Tests.Fingerprint");
        var second = CreatePlan("DynaBee.Tests.Fingerprint");

        Assert.Equal(first.GetFingerprint(), second.GetFingerprint());
    }

    [Fact]
    public void PlanExecutor_Uses_A_DI_Managed_Cache()
    {
        var services = new ServiceCollection().AddDynaBeeGeneration();
        var executor = services.BuildServiceProvider().GetRequiredService<IDynaBeeGenerationPlanExecutor>();
        var plan = CreatePlan("DynaBee.Tests.PlanExecutor");

        var first = executor.Build(plan);
        var second = executor.Build(plan);
        var uncached = executor.Build(plan, useCache: false);

        Assert.Same(first, second);
        Assert.NotSame(first, uncached);
    }

    [Fact]
    public void Collectible_BuildLease_Releases_Its_Context_Reference()
    {
        var lease = new BeeAssemblyBuilder("DynaBee.Tests.Collectible")
            .AsCollectible()
            .AddClass("Transient", _ => { })
            .BuildLease();

        Assert.True(lease.Context.Assembly.IsCollectible);
        lease.Dispose();

        Assert.True(lease.IsDisposed);
        Assert.Throws<ObjectDisposedException>(() => _ = lease.Context);
    }

    private static DynaBeeGenerationPlan CreatePlan(string assemblyName)
        => new DynaBeeGenerationPlan(assemblyName)
            .WithMetadata("scenario", "stable")
            .AddClass("GeneratedValue", type => type
                .AddProperty("Name", typeof(string)));
}
