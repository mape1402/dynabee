namespace DynaBee.Tests.FluentApi;

using DynaBee.FluentApi;

public sealed class MethodBodyControlFlowTests
{
    [Fact]
    public void EmitsBody_Supports_While_Break_And_Continue()
    {
        var context = new BeeAssemblyBuilder("DynaBee.Tests.ControlFlow.Loops")
            .AddClass("LoopImplementation", type => type
                .Implements<ILoopContract>()
                .AddMethod(nameof(ILoopContract.SumOddUntilThree), typeof(int), method => method
                    .WithParameter<int>("limit")
                    .EmitsBody(body =>
                    {
                        var current = body.DeclareLocal<int>("current");
                        var total = body.DeclareLocal<int>("total");
                        body.Assign(current, body.Constant(0));
                        body.Assign(total, body.Constant(0));
                        body.While(
                            loop => loop.LessThan(current, loop.Parameter<int>("limit")),
                            loop =>
                            {
                                loop.Assign(current, loop.Add(current, loop.Constant(1)));
                                loop.If(loop.Equal(current, loop.Constant(2)), nested => nested.Continue());
                                loop.Assign(total, loop.Add(total, current));
                                loop.If(loop.Equal(current, loop.Constant(3)), nested => nested.Break());
                            });
                        body.Return(total);
                    })))
            .Build();

        var value = context.CreateInstance<ILoopContract>("LoopImplementation");

        Assert.Equal(4, value.SumOddUntilThree(10));
    }

    [Fact]
    public void EmitsBody_Supports_Try_Catch_Finally_And_Returns_From_Protected_Regions()
    {
        var context = new BeeAssemblyBuilder("DynaBee.Tests.ControlFlow.Exceptions")
            .AddClass("ExceptionImplementation", type => type
                .Implements<IExceptionContract>()
                .AddMethod(nameof(IExceptionContract.Recover), typeof(int), method => method
                    .EmitsBody(body =>
                    {
                        body.Try(
                            tryBody => tryBody.Throw(tryBody.New<InvalidOperationException>()),
                            catches => catches.Catch<InvalidOperationException>("error", (error, catchBody) =>
                                catchBody.Return(catchBody.Constant(42))),
                            finallyBody => finallyBody.Evaluate(finallyBody.StaticCall(
                                typeof(MethodBodyTestRecorder),
                                nameof(MethodBodyTestRecorder.RecordFinally),
                                Array.Empty<Type>())));
                        body.Return(body.Constant(-1));
                    })))
            .Build();

        MethodBodyTestRecorder.FinallyCount = 0;
        var value = context.CreateInstance<IExceptionContract>("ExceptionImplementation");

        Assert.Equal(42, value.Recover());
        Assert.Equal(1, MethodBodyTestRecorder.FinallyCount);
    }

    [Fact]
    public void EmitsBody_Supports_Switch_And_Using()
    {
        var context = new BeeAssemblyBuilder("DynaBee.Tests.ControlFlow.SwitchUsing")
            .AddClass("ResourceImplementation", type => type
                .Implements<IResourceContract>()
                .AddMethod(nameof(IResourceContract.Classify), typeof(string), method => method
                    .WithParameter<int>("value")
                    .EmitsBody(body =>
                    {
                        var result = body.DeclareLocal<string>("result");
                        body.Switch(body.Parameter<int>("value"), branches => branches
                            .Case(1, branch => branch.Assign(result, branch.Constant("one")))
                            .Case(2, branch => branch.Assign(result, branch.Constant("two")))
                            .Default(branch => branch.Assign(result, branch.Constant("other"))));
                        body.Return(result);
                    }))
                .AddMethod(nameof(IResourceContract.Use), typeof(int), method => method
                    .WithParameter<Probe>("probe")
                    .EmitsBody(body => body.Using(
                        body.Parameter<Probe>("probe"),
                        "resource",
                        (resource, scope) => scope.Return(scope.Constant(7))))))
            .Build();

        var value = context.CreateInstance<IResourceContract>("ResourceImplementation");
        var probe = new Probe();

        Assert.Equal("one", value.Classify(1));
        Assert.Equal("two", value.Classify(2));
        Assert.Equal("other", value.Classify(9));
        Assert.Equal(7, value.Use(probe));
        Assert.Equal(1, probe.DisposeCount);
    }

    public interface ILoopContract
    {
        int SumOddUntilThree(int limit);
    }

    public interface IExceptionContract
    {
        int Recover();
    }

    public interface IResourceContract
    {
        string Classify(int value);

        int Use(Probe probe);
    }

    public sealed class Probe : IDisposable
    {
        public int DisposeCount { get; private set; }

        public void Dispose()
            => DisposeCount++;
    }
}

public static class MethodBodyTestRecorder
{
    public static int FinallyCount { get; set; }

    public static void RecordFinally()
        => FinallyCount++;
}
