namespace DynaBee.Tests.FluentApi;

using DynaBee.FluentApi;
using System.Reflection;

public sealed class MethodInterceptionTests
{
    [Fact]
    public void Intercept_Calls_Base_And_Executes_Before_After_And_Finally()
    {
        var add = typeof(InterceptedCalculator).GetMethod(nameof(InterceptedCalculator.Add))!;
        var context = new BeeAssemblyBuilder("DynaBee.Tests.Interception")
            .AddClass("GeneratedCalculator", type => type
                .Inherits<InterceptedCalculator>()
                .Intercept(add, interception => interception
                    .Before(body => body.Evaluate(body.StaticCall(
                        typeof(InterceptionRecorder),
                        nameof(InterceptionRecorder.Before),
                        Array.Empty<Type>())))
                    .After((result, body) => body.Evaluate(body.StaticCall(
                        typeof(InterceptionRecorder),
                        nameof(InterceptionRecorder.After),
                        new[] { typeof(int) },
                        result)))
                    .Finally(body => body.Evaluate(body.StaticCall(
                        typeof(InterceptionRecorder),
                        nameof(InterceptionRecorder.Finally),
                        Array.Empty<Type>())))))
            .Build();

        InterceptionRecorder.Reset();
        var calculator = context.CreateInstance<InterceptedCalculator>("GeneratedCalculator");

        Assert.Equal(7, calculator.Add(3, 4));
        Assert.Equal(new[] { "before", "after:7", "finally" }, InterceptionRecorder.Events);
    }

    [Fact]
    public void CallBase_Can_Be_Used_Directly_Inside_An_Override()
    {
        var add = typeof(InterceptedCalculator).GetMethod(nameof(InterceptedCalculator.Add))!;
        var context = new BeeAssemblyBuilder("DynaBee.Tests.Interception.Direct")
            .AddClass("GeneratedCalculator", type => type
                .Inherits<InterceptedCalculator>()
                .OverrideMethod(add, method => method.EmitsBody(body => body.Return(
                    body.Add(body.CallBase(), body.Constant(1))))))
            .Build();

        var calculator = context.CreateInstance<InterceptedCalculator>("GeneratedCalculator");

        Assert.Equal(8, calculator.Add(3, 4));
    }

    public class InterceptedCalculator
    {
        public virtual int Add(int left, int right)
            => left + right;
    }
}

public static class InterceptionRecorder
{
    private static readonly List<string> Entries = new();

    public static IReadOnlyList<string> Events => Entries;

    public static void Reset()
        => Entries.Clear();

    public static void Before()
        => Entries.Add("before");

    public static void After(int value)
        => Entries.Add($"after:{value}");

    public static void Finally()
        => Entries.Add("finally");
}
