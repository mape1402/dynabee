namespace DynaBee.Tests.FluentApi;

using DynaBee.FluentApi;
using DynaBee.Infrastructure;
using System.Reflection.Emit;

public sealed class RuntimeSurfaceTests
{
    [Fact]
    public void Generated_Class_Supports_Events_And_Dictionary_Backed_Indexers()
    {
        var context = new BeeAssemblyBuilder("DynaBee.Tests.RuntimeSurface")
            .AddClass("GeneratedStore", type => type
                .AddEvent<EventHandler>("Changed")
                .AddIndexer<int, string>())
            .Build();

        var type = context.GetClrType("GeneratedStore");
        var instance = Activator.CreateInstance(type)!;
        var indexer = type.GetProperty("Item")!;
        var changed = type.GetEvent("Changed")!;
        EventHandler handler = (_, _) => { };

        indexer.SetValue(instance, "bee", new object[] { 4 });
        changed.AddEventHandler(instance, handler);

        Assert.Equal("bee", indexer.GetValue(instance, new object[] { 4 }));
        Assert.NotNull(type.GetField("_Changed", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(instance));

        changed.RemoveEventHandler(instance, handler);
        Assert.Null(type.GetField("_Changed", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(instance));
    }

    [Fact]
    public void Generated_Class_Exposes_Clr_Operators()
    {
        var context = new BeeAssemblyBuilder("DynaBee.Tests.RuntimeSurface.Operators")
            .AddClass("GeneratedComparable", type => type
                .AddOperator(BeeOperatorKind.Equality, typeof(bool), method => method
                    .WithParameter("left", BeeType.Parse("GeneratedComparable"))
                    .WithParameter("right", BeeType.Parse("GeneratedComparable"))
                    .Emits(il =>
                    {
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Ceq);
                        il.Emit(OpCodes.Ret);
                    })))
            .Build();

        var type = context.GetClrType("GeneratedComparable");
        var left = Activator.CreateInstance(type)!;
        var right = Activator.CreateInstance(type)!;
        var equality = type.GetMethod("op_Equality")!;

        Assert.True((bool)equality.Invoke(null, new[] { left, left })!);
        Assert.False((bool)equality.Invoke(null, new[] { left, right })!);
        Assert.True(equality.IsSpecialName);
    }
}
