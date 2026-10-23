namespace DynaBee.Tests.FluentApi;

using DynaBee.FluentApi;
using DynaBee.Infrastructure;

public sealed class GenericTypeTests
{
    [Fact]
    public void AddGenericClass_Creates_Constrained_Generic_Type_That_Uses_Its_Parameter()
    {
        var context = new BeeAssemblyBuilder("DynaBee.Tests.GenericTypes")
            .AddGenericClass("Box", generic => generic
                .WithTypeParameter("T", parameter => parameter.ReferenceType().HasDefaultConstructor())
                .AddProperty("Value", generic.TypeParameter("T"))
                .AddMethod("GetValue", generic.TypeParameter("T"), method => method
                    .EmitsBody(body => body.Return(body.Property(body.Self(), "Value")))))
            .Build();

        var genericBox = context.GetClrType("Box");
        var boxType = genericBox.MakeGenericType(typeof(GenericValue));
        var box = Activator.CreateInstance(boxType)!;
        boxType.GetProperty("Value")!.SetValue(box, new GenericValue { Name = "bee" });

        var value = (GenericValue)boxType.GetMethod("GetValue")!.Invoke(box, null)!;
        var parameter = genericBox.GetGenericArguments().Single();

        Assert.Equal("bee", value.Name);
        Assert.True(parameter.GenericParameterAttributes.HasFlag(System.Reflection.GenericParameterAttributes.ReferenceTypeConstraint));
        Assert.True(parameter.GenericParameterAttributes.HasFlag(System.Reflection.GenericParameterAttributes.DefaultConstructorConstraint));
    }

    [Fact]
    public void AddMethod_Can_Declare_And_Return_Its_Own_Generic_Parameter()
    {
        var context = new BeeAssemblyBuilder("DynaBee.Tests.GenericMethods")
            .AddClass("GenericUtility", type => type
                .AddMethod("Identity", BeeType.GenericParameter("T"), method => method
                    .WithTypeParameter("T")
                    .WithParameter("value", BeeType.GenericParameter("T"))
                    .EmitsBody(body => body.Return(body.Parameter("value")))))
            .Build();

        var utility = context.CreateInstance("GenericUtility");
        var identity = utility.GetType().GetMethod("Identity")!.MakeGenericMethod(typeof(string));

        Assert.Equal("bee", identity.Invoke(utility, new object[] { "bee" }));
    }

    public sealed class GenericValue
    {
        public string Name { get; set; }
    }
}
