namespace DynaBee.Benchmarks;

using BenchmarkDotNet.Attributes;
using DynaBee;
using DynaBee.FluentApi;
using System.Reflection;

[MemoryDiagnoser]
public class InvocationBenchmarks
{
    private IAssemblyContext _context;
    private IAdder _typedInstance;
    private MethodInfo _addMethod;
    private object[] _addArguments;

    [GlobalSetup]
    public void Setup()
    {
        _context = DynaBeeBuilder
            .CreateAssembly("DynaBee.Bench.Invocation")
            .WithVersion("1.0.0")
            .AddClass("Adder", c => c
                .Implements<IAdder>()
                .AddMethod("Add", typeof(int), m => m
                    .WithParameter<int>("x")
                    .WithParameter<int>("y")
                    .EmitsExpression((System.Linq.Expressions.Expression<Func<int, int, int>>)((x, y) => x + y))))
            .Build();

        _typedInstance = _context.CreateInstance<IAdder>("Adder");
        _addMethod = _typedInstance.GetType().GetMethod("Add")!;
        _addArguments = [3, 4];
    }

    [Benchmark]
    public object CreateInstance()
        => _context.CreateInstance("Adder");

    [Benchmark]
    public int CallViaInterface()
        => _typedInstance.Add(3, 4);

    [Benchmark]
    public int CallViaReflection()
        => (int)_addMethod.Invoke(_typedInstance, _addArguments)!;

    public interface IAdder
    {
        int Add(int x, int y);
    }
}
