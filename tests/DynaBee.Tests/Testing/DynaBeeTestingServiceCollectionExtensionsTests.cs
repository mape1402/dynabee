namespace DynaBee.Tests.Testing;

using DynaBee.FluentApi;
using DynaBee.Testing;
using DynaBee.Testing.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

public sealed class DynaBeeTestingServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDynaBeeTesting_Registers_The_Generator()
    {
        var services = new ServiceCollection();

        Assert.Same(services, services.AddDynaBeeTesting());

        using var provider = services.BuildServiceProvider();
        Assert.IsType<DynaBeeTestGenerator>(provider.GetRequiredService<IDynaBeeTestGenerator>());
    }

    [Fact]
    public void AddDynaBeeTesting_Requires_A_Service_Collection()
    {
        Assert.Throws<ArgumentNullException>(() => DynaBeeTestingServiceCollectionExtensions.AddDynaBeeTesting(null!));
    }

    [Fact]
    public void AddGeneratedAssembly_Requires_A_Service_Collection_And_Assembly()
    {
        var assembly = DynaBeeBuilder.CreateAssembly("Tests.DI").Build();

        Assert.Throws<ArgumentNullException>(() => DynaBeeTestingServiceCollectionExtensions.AddGeneratedAssembly(null!, assembly));
        Assert.Throws<ArgumentNullException>(() => new ServiceCollection().AddGeneratedAssembly(null!));
    }

    [Fact]
    public void AddGeneratedAssembly_Returns_The_Service_Collection()
    {
        var assembly = DynaBeeBuilder.CreateAssembly("Tests.DI.Return").Build();
        var services = new ServiceCollection();

        Assert.Same(services, services.AddGeneratedAssembly(assembly, ServiceLifetime.Singleton));
    }
}
