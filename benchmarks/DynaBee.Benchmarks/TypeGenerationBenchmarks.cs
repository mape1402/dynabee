namespace DynaBee.Benchmarks;

using BenchmarkDotNet.Attributes;
using DynaBee;
using DynaBee.FluentApi;
using System.Threading;

[MemoryDiagnoser]
public class TypeGenerationBenchmarks
{
    private BeeAssemblyBuilder _cachedBuilder;
    private static int _sequence;

    [GlobalSetup]
    public void Setup()
    {
        _cachedBuilder = DynaBeeBuilder
            .CreateAssembly("DynaBee.Bench.Cached")
            .WithVersion("1.0.0")
            .AddClass("Customer", c => c
                .AddAutoProperty<int>("Id")
                .AddMethod("GetFixedValue", typeof(int), m => m.EmitsExpression((System.Linq.Expressions.Expression<Func<int>>)(() => 42))));

        _ = _cachedBuilder.Build();
    }

    [Benchmark]
    public IAssemblyContext BuildClass_NoCache()
    {
        var next = Interlocked.Increment(ref _sequence);
        return DynaBeeBuilder
            .CreateAssembly($"DynaBee.Bench.NoCache.{next}")
            .DisableCache()
            .AddClass("Customer", c => c
                .AddAutoProperty<int>("Id")
                .AddMethod("GetFixedValue", typeof(int), m => m.EmitsExpression((System.Linq.Expressions.Expression<Func<int>>)(() => 42))))
            .Build();
    }

    [Benchmark]
    public IAssemblyContext BuildClass_FromCache()
        => _cachedBuilder.Build();
}
