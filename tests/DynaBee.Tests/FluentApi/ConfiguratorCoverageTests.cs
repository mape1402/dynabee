namespace DynaBee.Tests.FluentApi;

using System.Reflection;
using System.Reflection.Emit;
using DynaBee.FluentApi;
using DynaBee.Infrastructure;
using DynaBee.Infrastructure.Configurators;

public sealed class ConfiguratorCoverageTests
{
    [Fact]
    public void MethodConfiguration_Emits_Default_Il_Lambda_And_Static_Method_Bodies()
    {
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("Methods", type => type
                .AddMethod("DefaultValue", typeof(int))
                .AddMethod("DefaultReference", typeof(string))
                .AddVoidMethod("DefaultVoid")
                .AddMethod("Raw", typeof(int), method => method
                    .WithParameter<int>("value")
                    .Emits(il =>
                    {
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Ldc_I4_1);
                        il.Emit(OpCodes.Add);
                        il.Emit(OpCodes.Ret);
                    }))
                .AddMethod("Lambda", typeof(int), method => method
                    .WithParameter<int>("value")
                    .EmitsLambda((Func<int, int>)(value => value * 2)))
                .AddVoidMethod("LambdaVoid", method => method
                    .WithParameter<Action>("callback")
                    .EmitsLambda((Action<Action>)(callback => callback())))
                .AddMethod("StaticLambda", typeof(int), method => method
                    .AsStatic()
                    .WithParameter<int>("value")
                    .EmitsLambda((Func<int, int>)(value => value + 3)))
                .AddMethod("StaticExpression", typeof(int), method => method
                    .AsStatic()
                    .WithParameter<int>("value")
                    .EmitsExpression((System.Linq.Expressions.Expression<Func<int, int>>)(value => value + 4))))
            .Build();

        var type = context.GetClrType("Methods");
        var instance = Activator.CreateInstance(type)!;
        var called = false;

        Assert.Equal(0, type.GetMethod("DefaultValue")!.Invoke(instance, null));
        Assert.Null(type.GetMethod("DefaultReference")!.Invoke(instance, null));
        type.GetMethod("DefaultVoid")!.Invoke(instance, null);
        Assert.Equal(8, type.GetMethod("Raw")!.Invoke(instance, new object[] { 7 }));
        Assert.Equal(14, type.GetMethod("Lambda")!.Invoke(instance, new object[] { 7 }));
        type.GetMethod("LambdaVoid")!.Invoke(instance, new object[] { (Action)(() => called = true) });
        Assert.True(called);
        Assert.Equal(10, type.GetMethod("StaticLambda")!.Invoke(null, new object[] { 7 }));
        Assert.Equal(11, type.GetMethod("StaticExpression")!.Invoke(null, new object[] { 7 }));
    }

    [Fact]
    public void PropertyOverrides_Emit_Body_Il_And_Default_Accessors()
    {
        var number = typeof(OverrideBase).GetProperty(nameof(OverrideBase.Number))!;
        var text = typeof(OverrideBase).GetProperty(nameof(OverrideBase.Text))!;
        var defaultNumber = typeof(OverrideBase).GetProperty(nameof(OverrideBase.DefaultNumber))!;
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("Overrides", type => type
                .Inherits<OverrideBase>()
                .AddAutoProperty<int>("Stored")
                .OverrideProperty(number, property => property
                    .Getter(getter => getter.EmitsBody(body => body.Return(body.Property(body.Self(), "Stored"))))
                    .Setter(setter => setter.EmitsBody(body =>
                    {
                        body.Assign(body.Property(body.Self(), "Stored"), body.Parameter<int>("value"));
                        body.Return();
                    })))
                .OverrideProperty(text, property => property
                    .Getter(getter => getter.Emits(il =>
                    {
                        il.Emit(OpCodes.Ldstr, "from-il");
                        il.Emit(OpCodes.Ret);
                    }))
                    .Setter(setter => setter.Emits(il => il.Emit(OpCodes.Ret))))
                .OverrideProperty(defaultNumber, property => property.Getter(_ => { })))
            .Build();

        var instance = (OverrideBase)context.CreateInstance("Overrides");
        instance.Number = 19;
        instance.Text = "ignored";

        Assert.Equal(19, instance.Number);
        Assert.Equal("from-il", instance.Text);
        Assert.Equal(0, instance.DefaultNumber);
    }

    [Fact]
    public void OverrideConfiguration_Rejects_Invalid_Property_And_Method_Shapes()
    {
        var writable = typeof(OverrideBase).GetProperty(nameof(OverrideBase.Number))!;
        var readOnly = typeof(OverrideBase).GetProperty(nameof(OverrideBase.DefaultNumber))!;
        var nonVirtual = typeof(OverrideBase).GetProperty(nameof(OverrideBase.NonVirtual))!;
        var virtualMethod = typeof(OverrideBase).GetMethod(nameof(OverrideBase.VirtualMethod))!;
        var regularMethod = typeof(OverrideBase).GetMethod(nameof(OverrideBase.RegularMethod))!;

        Assert.Throws<InvalidOperationException>(() => BuildInvalid(type => type.OverrideProperty(writable, _ => { })));
        Assert.Throws<InvalidOperationException>(() => BuildInvalid(type => type.OverrideProperty(readOnly, property => property.Setter(_ => { }))));
        Assert.Throws<InvalidOperationException>(() => BuildInvalid(type => type.OverrideProperty(nonVirtual, property => property.Getter(_ => { }))));
        Assert.Throws<InvalidOperationException>(() => BuildInvalid(type => type.OverrideProperty(writable, property => property.Setter(setter => setter.ReturnsConstant(1)))));
        Assert.Throws<InvalidOperationException>(() => BuildInvalid(type => type.OverrideMethod(regularMethod, method => method.EmitsBody(body => body.Return(body.Constant(1))))));
        Assert.Throws<InvalidOperationException>(() => BuildInvalid(type => type.OverrideMethod(virtualMethod, method => method.AsStatic().EmitsBody(body => body.Return(body.Constant(1))))));
        Assert.Throws<InvalidOperationException>(() => BuildInvalid(type => type.OverrideMethod(virtualMethod, method => method.WithAccess(MethodAccessModifier.Protected).EmitsBody(body => body.Return(body.Constant(1))))));
    }

    [Fact]
    public void MethodConfigurator_Resolves_All_Runtime_Access_Modifiers()
    {
        Assert.Equal(MethodAccessModifier.Public, MethodConfigurator.GetAccessModifier(GetMethod(nameof(AccessBase.Public))));
        Assert.Equal(MethodAccessModifier.Protected, MethodConfigurator.GetAccessModifier(GetMethod("Protected")));
        Assert.Equal(MethodAccessModifier.Internal, MethodConfigurator.GetAccessModifier(GetMethod("Internal")));
        Assert.Equal(MethodAccessModifier.ProtectedInternal, MethodConfigurator.GetAccessModifier(GetMethod("ProtectedInternal")));
        Assert.Equal(MethodAccessModifier.PrivateProtected, MethodConfigurator.GetAccessModifier(GetMethod("PrivateProtected")));
        Assert.Equal(MethodAccessModifier.Private, MethodConfigurator.GetAccessModifier(GetMethod("Private")));
    }

    private static void BuildInvalid(Action<BeeClassBuilder> configure)
        => new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("Invalid", type =>
            {
                type.Inherits<OverrideBase>();
                configure(type);
            })
            .Build();

    private static MethodInfo GetMethod(string name)
        => typeof(AccessBase).GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

    private static string NewAssemblyName()
        => $"DynaBee.Tests.Configurators.{Guid.NewGuid():N}";

    public class OverrideBase
    {
        public virtual int Number { get; set; }

        public virtual string Text { get; set; }

        public virtual int DefaultNumber { get; }

        public int NonVirtual { get; set; }

        public virtual int VirtualMethod()
            => 1;

        public int RegularMethod()
            => 1;
    }

    private class AccessBase
    {
        public void Public() { }

        protected void Protected() { }

        internal void Internal() { }

        protected internal void ProtectedInternal() { }

        private protected void PrivateProtected() { }

        private void Private() { }
    }
}
