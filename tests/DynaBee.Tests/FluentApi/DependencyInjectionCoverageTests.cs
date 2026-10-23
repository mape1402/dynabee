namespace DynaBee.Tests.FluentApi;

using System.Reflection;
using DynaBee.FluentApi;
using DynaBee.FluentApi.DependencyInjection;
using DynaBee.Infrastructure.Contexts;
using Microsoft.Extensions.DependencyInjection;

public sealed class DependencyInjectionCoverageTests
{
    [Fact]
    public void AddDynaBee_Registers_Contexts_Interfaces_Rules_And_Projections()
    {
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("Service", type => type
                .Implements<ICoverageService>()
                .Implements<IHiddenService>(registerInDi: false)
                .RegisterAs<IAdditionalService>()
                .RegisterAsConcrete(false)
                .AddMethod(nameof(ICoverageService.GetValue), typeof(string), method => method
                    .EmitsExpression((System.Linq.Expressions.Expression<Func<string>>)(() => "service"))))
            .Build();
        var services = new ServiceCollection();

        services.AddDynaBee(context, options => options
            .Register(rule => rule
                .ForType("Service")
                .Where(type => type.ClrType.Name == "Service")
                .As<ICoverageService>()
                .WithLifetime(ServiceLifetime.Singleton)
                .SkipConcrete()
                .Project(type => new[] { new ServiceDescriptor(typeof(CoverageProjection), _ => new CoverageProjection(type.Name), ServiceLifetime.Singleton) })));

        var provider = services.BuildServiceProvider();

        Assert.Equal("service", provider.GetRequiredService<ICoverageService>().GetValue());
        Assert.IsType<CoverageProjection>(provider.GetRequiredService<CoverageProjection>());
        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<IHiddenService>());
        Assert.Same(context, provider.GetRequiredService<IAssemblyContext>());
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(ICoverageService) && descriptor.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddDynaBee_Overloads_Validate_Arguments_And_Register_Provider_Snapshots()
    {
        Assert.Throws<ArgumentNullException>(() => DynaBeeServiceCollectionExtensions.AddDynaBee(null!, "Assembly", _ => { }));
        Assert.Throws<ArgumentNullException>(() => new ServiceCollection().AddDynaBee("Assembly", null!));
        Assert.Throws<ArgumentNullException>(() => new ServiceCollection().AddDynaBee((IAssemblyContext)null!));
        Assert.Throws<ArgumentNullException>(() => new ServiceCollection().AddDynaBee((IAssemblyContext)null!, _ => { }));
        Assert.Throws<ArgumentNullException>(() => new ServiceCollection().AddDynaBee(new BeeAssemblyBuilder(NewAssemblyName()).Build(), null!));
        Assert.Throws<ArgumentNullException>(() => new ServiceCollection().AddDynaBee((IAssemblyContextProvider)null!));
        Assert.Throws<ArgumentNullException>(() => new ServiceCollection().AddDynaBee((IAssemblyContextProvider)null!, _ => { }));

        var services = new ServiceCollection();
        services.AddDynaBee("Configured", builder => builder
            .AddClass("ConfiguredService", type => type
                .Implements<ICoverageService>()
                .AddMethod(nameof(ICoverageService.GetValue), typeof(string), method => method
                    .EmitsExpression((System.Linq.Expressions.Expression<Func<string>>)(() => "configured")))));
        var configuredProvider = services.BuildServiceProvider();

        Assert.Equal("configured", configuredProvider.GetRequiredService<ICoverageService>().GetValue());

        var registry = new AssemblyContextRegistry(NewAssemblyName());
        registry.Configure(builder => builder.AddClass("ProviderService", type => type
            .Implements<ICoverageService>()
            .AddMethod(nameof(ICoverageService.GetValue), typeof(string), method => method
                .EmitsExpression((System.Linq.Expressions.Expression<Func<string>>)(() => "provider")))));
        var provider = new AssemblyContextProvider(registry);
        var providerServices = new ServiceCollection().AddDynaBee(provider).BuildServiceProvider();

        Assert.Equal("provider", providerServices.GetRequiredService<ICoverageService>().GetValue());
    }

    [Fact]
    public void Registries_Providers_Profiles_And_Catalogs_Expose_Mutable_Snapshots_Safely()
    {
        var assemblyName = NewAssemblyName();
        var registry = new AssemblyContextRegistry(assemblyName);
        var profile = new CoverageProfile(assemblyName);

        Assert.True(registry.AddProfile(profile));
        Assert.False(registry.AddProfile(profile));
        Assert.Throws<InvalidOperationException>(() => registry.AddProfile(new CoverageProfile(NewAssemblyName())));
        Assert.Throws<ArgumentNullException>(() => registry.Configure(null!));
        Assert.Throws<ArgumentNullException>(() => registry.AddProfile(null!));

        var provider = new AssemblyContextProvider(registry);
        var first = provider.Current;
        registry.Configure(builder => builder.AddClass("Second", type => type.AddAutoProperty<int>("Value")));
        var second = provider.Current;

        Assert.NotSame(first, second);
        Assert.Equal(1, provider.Generation);
        Assert.NotSame(second, provider.Rebuild());
        Assert.Equal(2, provider.Generation);

        var genericProfileRegistry = new AssemblyContextRegistry(CoverageProfile.DefaultAssemblyName);
        Assert.Same(genericProfileRegistry, genericProfileRegistry.AddProfile<CoverageProfile>());
        IAssemblyContextRegistry profileRegistry = new AssemblyContextRegistry(CoverageProfile.DefaultAssemblyName);
        Assert.Same(profileRegistry, AssemblyContextRegistryExtensions.AddProfile<CoverageProfile>(profileRegistry));
        var catalog = new DynaBeeAssemblyCatalog(
            new Dictionary<string, IAssemblyContextRegistry> { [assemblyName] = registry },
            new Dictionary<string, IAssemblyContextProvider> { [assemblyName] = provider });

        Assert.Equal(new[] { assemblyName }, catalog.AssemblyNames);
        Assert.Equal(new[] { registry }, catalog.Registries);
        Assert.Equal(new[] { provider }, catalog.Providers);
        Assert.Same(registry, catalog.GetRegistry(assemblyName));
        Assert.Same(provider, catalog.GetProvider(assemblyName));
        Assert.Same(provider.Current, catalog.GetContext(assemblyName));
        Assert.Throws<ArgumentException>(() => catalog.GetRegistry(""));
        Assert.Throws<KeyNotFoundException>(() => catalog.GetRegistry("missing"));
        Assert.Throws<ArgumentException>(() => catalog.GetProvider(""));
        Assert.Throws<KeyNotFoundException>(() => catalog.GetProvider("missing"));
        Assert.Throws<InvalidOperationException>(() => new DynaBeeAssemblyCatalog(
            new Dictionary<string, IAssemblyContextRegistry> { [assemblyName] = registry },
            new Dictionary<string, IAssemblyContextProvider>()));
    }

    [Fact]
    public void Profile_Discovery_Filters_Assemblies_And_Registers_Matching_Profiles()
    {
        var assembly = typeof(DependencyInjectionCoverageTests).Assembly;
        var assemblyName = CoverageProfile.DefaultAssemblyName;
        var discovered = DynaBeeProfileDiscoveryExtensions.DiscoverProfilesFromAssemblies(assembly, null!, assembly);
        var registry = new AssemblyContextRegistry(assemblyName);

        Assert.Empty(DynaBeeProfileDiscoveryExtensions.DiscoverProfilesFromAssemblies());
        Assert.Contains(
            DynaBeeProfileDiscoveryExtensions.DiscoverProfilesFromCurrentAppDomain(),
            profile => profile.GetType() == typeof(DiscoverableCoverageProfile));
        Assert.Contains(discovered, profile => profile.GetType() == typeof(DiscoverableCoverageProfile));
        Assert.Same(registry, registry.AddProfilesFromAssemblies(assembly));
        Assert.NotNull(registry.BuildSnapshot().Find("Discovered"));
        Assert.Same(registry, registry.AddProfilesFromCurrentAppDomain());

        var services = new ServiceCollection()
            .AddDynaBeeProfiles(assemblies: assembly)
            .BuildServiceProvider();
        var catalog = services.GetRequiredService<IDynaBeeAssemblyCatalog>();

        Assert.Contains(assemblyName, catalog.AssemblyNames);
        Assert.Equal("discovered", services.GetRequiredService<ICoverageService>().GetValue());
    }

    [Fact]
    public void Registration_Options_Validate_Rules_And_Compose_Predicates()
    {
        var options = new DynaBeeServiceRegistrationOptions();

        Assert.Throws<ArgumentNullException>(() => options.Register(null!));
        Assert.Throws<ArgumentException>(() => new DynaBeeGeneratedTypeRegistrationBuilder().ForType(""));
        Assert.Throws<ArgumentNullException>(() => new DynaBeeGeneratedTypeRegistrationBuilder().Where(null!));
        Assert.Throws<ArgumentNullException>(() => new DynaBeeGeneratedTypeRegistrationBuilder().As(null!));
        Assert.Throws<ArgumentNullException>(() => new DynaBeeGeneratedTypeRegistrationBuilder().Project(null!));

        var builder = new DynaBeeGeneratedTypeRegistrationBuilder()
            .ForType("Match")
            .Where(_ => true)
            .As<ICoverageService>()
            .As<ICoverageService>()
            .WithLifetime(ServiceLifetime.Scoped)
            .AsConcrete()
            .SkipConcrete()
            .Project(_ => null!);
        options.Register(_ => { });
        options.Register(rule => rule.Where(_ => true));

        Assert.Equal(2, options.Rules.Count);
        Assert.NotNull(builder.Build());
    }

    [Fact]
    public void Registration_Rules_Short_Circuit_Additional_Predicates_After_A_Non_Match()
    {
        var evaluatedSecondPredicate = false;
        var type = new TypeContext("Service", typeof(CoverageImplementation), Array.Empty<IElementContext>());
        var rule = new DynaBeeGeneratedTypeRegistrationBuilder()
            .Where(_ => false)
            .Where(_ =>
            {
                evaluatedSecondPredicate = true;
                return true;
            })
            .Build();

        Assert.False(rule.Predicate(type));
        Assert.False(evaluatedSecondPredicate);
    }

    [Fact]
    public void Di_Registration_Overloads_Validate_Service_Collections_And_Expose_Default_Concrete_Registrations()
    {
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("PlainService", type => type.AddAutoProperty<int>("Value"))
            .Build();
        var registry = new AssemblyContextRegistry(NewAssemblyName());
        var snapshotProvider = new AssemblyContextProvider(registry);
        var profile = new CoverageProfile(registry.AssemblyName);

        Assert.Throws<ArgumentNullException>(() => DynaBeeServiceCollectionExtensions.AddDynaBeeProfiles(null!));
        Assert.Throws<ArgumentNullException>(() => DynaBeeServiceCollectionExtensions.AddDynaBeeRegistry(null!, "Registry"));
        Assert.Throws<ArgumentNullException>(() => DynaBeeServiceCollectionExtensions.AddDynaBee(null!, context));
        Assert.Throws<ArgumentNullException>(() => DynaBeeServiceCollectionExtensions.AddDynaBee(null!, context, _ => { }));
        Assert.Throws<ArgumentNullException>(() => DynaBeeServiceCollectionExtensions.AddDynaBee(null!, snapshotProvider));
        Assert.Throws<ArgumentNullException>(() => DynaBeeServiceCollectionExtensions.AddDynaBee(null!, snapshotProvider, _ => { }));
        Assert.Throws<ArgumentNullException>(() => AssemblyContextRegistryExtensions.AddProfile(null!, profile));
        Assert.Throws<ArgumentNullException>(() => AssemblyContextRegistryExtensions.AddProfile<CoverageProfile>(null!));

        IAssemblyContextRegistry profileRegistry = new AssemblyContextRegistry(registry.AssemblyName);
        Assert.Same(profileRegistry, AssemblyContextRegistryExtensions.AddProfile(profileRegistry, profile));
        Assert.Throws<ArgumentNullException>(() => DynaBeeProfileDiscoveryExtensions.AddProfilesFromAssemblies(null!, typeof(DependencyInjectionCoverageTests).Assembly));

        var services = new ServiceCollection().AddDynaBee(context, ServiceLifetime.Singleton);
        var provider = services.BuildServiceProvider();
        var implementationType = context.GetClrType("PlainService");

        Assert.Same(provider.GetRequiredService(implementationType), provider.GetRequiredService(implementationType));
        Assert.Same(context, provider.GetRequiredService<IAssemblyContext>());
    }

    [Fact]
    public void Di_Registration_Only_Registers_Concrete_Non_Generic_Generated_Classes()
    {
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("ConcreteService", _ => { })
            .AddInterface("IGeneratedContract", _ => { })
            .AddStruct("GeneratedValue", _ => { })
            .AddGenericClass("GenericService", generic => generic
                .WithTypeParameter("T")
                .AddProperty("Value", generic.TypeParameter("T")))
            .Build();
        var concreteType = context.GetClrType("ConcreteService");
        var genericType = context.GetClrType("GenericService");
        var services = new ServiceCollection().AddDynaBee(context);
        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService(concreteType));
        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == genericType);
        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == context.GetClrType("IGeneratedContract"));
        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == context.GetClrType("GeneratedValue"));
    }

    [Fact]
    public void Registry_And_Provider_Overloads_Register_Current_Snapshots_And_Rule_Service_Types()
    {
        var registry = new AssemblyContextRegistry(NewAssemblyName());
        registry.Configure(builder => builder
            .AddClass("RegisteredService", type => type
                .Implements<ICoverageService>()
                .AddMethod(nameof(ICoverageService.GetValue), typeof(string), method => method
                    .EmitsExpression((System.Linq.Expressions.Expression<Func<string>>)(() => "registered")))));
        var snapshot = registry.BuildSnapshot();
        var snapshotProvider = new AssemblyContextProvider(registry, snapshot);

        var registryServices = new ServiceCollection()
            .AddDynaBeeRegistry(registry.AssemblyName, configureRegistry: null, ServiceLifetime.Scoped)
            .BuildServiceProvider();
        Assert.NotNull(registryServices.GetRequiredService<IAssemblyContextRegistry>());

        var services = new ServiceCollection()
            .AddDynaBee(snapshotProvider, options => options.Register(rule => rule
                .ForType("RegisteredService")
                .As<ICoverageService>()
                .WithLifetime(ServiceLifetime.Singleton)))
            .BuildServiceProvider();

        Assert.Equal("registered", services.GetRequiredService<ICoverageService>().GetValue());
        Assert.Same(services.GetRequiredService<ICoverageService>(), services.GetRequiredService<ICoverageService>());
    }

    [Fact]
    public void Di_Registration_Policy_Resolves_Metadata_Rules_And_Fallbacks_Deterministically()
    {
        var implementation = typeof(CoverageImplementation);
        var fallback = new TypeContext("Service", implementation, Array.Empty<IElementContext>());
        var metadata = new TypeContext(
            "Service",
            implementation,
            Array.Empty<IElementContext>(),
            new Dictionary<string, object>
            {
                [BeeDiMetadataKeys.RegisterAsConcrete] = false,
                [BeeDiMetadataKeys.InterfaceRegistrations] = new Dictionary<Type, bool>
                {
                    [typeof(ICoverageService)] = true,
                    [typeof(IHiddenService)] = false,
                },
                [BeeDiMetadataKeys.ServiceRegistrations] = new Dictionary<Type, bool>
                {
                    [typeof(IAdditionalService)] = true,
                    [typeof(IDisposable)] = false,
                },
            });
        var forceConcrete = new DynaBeeGeneratedTypeRegistrationRule(
            _ => true,
            Array.Empty<Type>(),
            null,
            true,
            Array.Empty<Func<ITypeContext, IEnumerable<ServiceDescriptor>>>());
        var skipConcrete = new DynaBeeGeneratedTypeRegistrationRule(
            _ => true,
            Array.Empty<Type>(),
            null,
            false,
            Array.Empty<Func<ITypeContext, IEnumerable<ServiceDescriptor>>>());

        Assert.True(InvokeDiPolicy<bool>("ResolveRegisterConcreteType", fallback, Array.Empty<DynaBeeGeneratedTypeRegistrationRule>()));
        Assert.False(InvokeDiPolicy<bool>("ResolveRegisterConcreteType", metadata, Array.Empty<DynaBeeGeneratedTypeRegistrationRule>()));
        Assert.True(InvokeDiPolicy<bool>("ResolveRegisterConcreteType", metadata, new[] { skipConcrete, forceConcrete }));
        Assert.Equal(new[] { typeof(ICoverageService) }, InvokeDiPolicy<IEnumerable<Type>>("ResolveInterfaceRegistrations", metadata, implementation));
        Assert.Contains(typeof(ICoverageService), InvokeDiPolicy<IEnumerable<Type>>("ResolveInterfaceRegistrations", fallback, implementation));
        Assert.Equal(new[] { typeof(IAdditionalService) }, InvokeDiPolicy<IEnumerable<Type>>("ResolveServiceRegistrations", metadata, implementation));
        Assert.Empty(InvokeDiPolicy<IEnumerable<Type>>("ResolveServiceRegistrations", fallback, implementation));
    }

    [Fact]
    public void Profile_And_Registration_Policies_Handle_Implicit_Discovery_And_NonBoolean_Metadata()
    {
        var assemblyName = CoverageProfile.DefaultAssemblyName;
        var profile = new CoverageProfile(assemblyName);
        var implementation = typeof(CoverageImplementation);
        var malformedMetadata = new TypeContext(
            "Service",
            implementation,
            Array.Empty<IElementContext>(),
            new Dictionary<string, object> { [BeeDiMetadataKeys.RegisterAsConcrete] = "true" });
        var rule = new DynaBeeGeneratedTypeRegistrationBuilder()
            .Where(_ => true)
            .Where(type => type.Name == "Service")
            .Build();
        var services = new ServiceCollection().AddDynaBeeProfiles();

        Assert.Throws<ArgumentException>(() => new CoverageProfile(" "));
        Assert.True(InvokeDiPolicy<bool>("ResolveRegisterConcreteType", malformedMetadata, Array.Empty<DynaBeeGeneratedTypeRegistrationRule>()));
        Assert.True(rule.Predicate(malformedMetadata));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IDynaBeeAssemblyCatalog));
        Assert.Equal(assemblyName, profile.AssemblyName);
    }

    [Fact]
    public void Profile_Discovery_Returns_An_Empty_Result_For_A_Null_Assembly_Collection()
    {
        Assert.Empty(DynaBeeProfileDiscoveryExtensions.DiscoverProfilesFromAssemblies(null!));
    }

    [Fact]
    public void Dependency_Infrastructure_Rejects_Invalid_Constructors_And_Keeps_Stable_Snapshots()
    {
        var registry = new AssemblyContextRegistry(NewAssemblyName());
        var snapshot = registry.BuildSnapshot();
        var provider = new AssemblyContextProvider(registry, snapshot);

        Assert.Throws<ArgumentNullException>(() => new AssemblyContextProvider(null!));
        Assert.Throws<ArgumentNullException>(() => new AssemblyContextProvider(null!, snapshot));
        Assert.Throws<ArgumentNullException>(() => new AssemblyContextProvider(registry, null!));
        Assert.Throws<ArgumentException>(() => new AssemblyContextRegistry(" "));
        Assert.Throws<ArgumentNullException>(() => new AssemblyContextRegistry("Registry", null!));
        Assert.Throws<ArgumentNullException>(() => new DynaBeeAssemblyCatalog(null!, new Dictionary<string, IAssemblyContextProvider>()));
        Assert.Throws<ArgumentNullException>(() => new DynaBeeAssemblyCatalog(new Dictionary<string, IAssemblyContextRegistry>(), null!));
        Assert.Throws<ArgumentNullException>(() => new DynaBeeGeneratedTypeRegistrationRule(null!, Array.Empty<Type>(), null, null, Array.Empty<Func<ITypeContext, IEnumerable<ServiceDescriptor>>>()));
        Assert.Throws<ArgumentNullException>(() => new DynaBeeGeneratedTypeRegistrationRule(_ => true, null!, null, null, Array.Empty<Func<ITypeContext, IEnumerable<ServiceDescriptor>>>()));
        Assert.Throws<ArgumentNullException>(() => new DynaBeeGeneratedTypeRegistrationRule(_ => true, Array.Empty<Type>(), null, null, null!));

        Assert.Same(snapshot, provider.Current);
        Assert.Equal(0, provider.Generation);
    }

    private static T InvokeDiPolicy<T>(string name, params object[] arguments)
        => (T)typeof(DynaBeeServiceCollectionExtensions)
            .GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, arguments)!;

    private static string NewAssemblyName()
        => $"DynaBee.Tests.DiCoverage.{Guid.NewGuid():N}";

    public interface ICoverageService
    {
        string GetValue();
    }

    public interface IHiddenService
    {
    }

    public interface IAdditionalService
    {
    }

    public sealed class CoverageImplementation : ICoverageService, IHiddenService, IAdditionalService, IDisposable
    {
        public string GetValue()
            => "implementation";

        public void Dispose()
        {
        }
    }

    public sealed record CoverageProjection(string TypeName);

    public sealed class CoverageProfile : DynaBeeProfile
    {
        public const string DefaultAssemblyName = "DynaBee.Tests.DiCoverage.Discovery";

        public CoverageProfile()
            : this(DefaultAssemblyName)
        {
        }

        public CoverageProfile(string assemblyName)
            : base(assemblyName)
        {
        }

        public override void Configure(IBeeAssemblyBuilder builder)
            => builder.AddClass("Profiled", type => type.AddAutoProperty<int>("Value"));
    }

    public sealed class DiscoverableCoverageProfile : DynaBeeProfile
    {
        public DiscoverableCoverageProfile()
            : base(CoverageProfile.DefaultAssemblyName)
        {
        }

        public override void Configure(IBeeAssemblyBuilder builder)
            => builder.AddClass("Discovered", type => type
                .Implements<ICoverageService>()
                .AddMethod(nameof(ICoverageService.GetValue), typeof(string), method => method
                    .EmitsExpression((System.Linq.Expressions.Expression<Func<string>>)(() => "discovered"))));
    }
}
