namespace DynaBee.Tests.FluentApi;

using System.Reflection;
using System.Text;
using DynaBee.FluentApi;
using DynaBee.FluentApi.Generation;
using DynaBee.Infrastructure;

public sealed class GenerationInternalCoverageTests
{
    [Fact]
    public void SourceExporter_Renders_All_Supported_Model_Member_And_Type_Shapes()
    {
        var plan = CreatePlan(
            "Model",
            typeof(object),
            new[] { typeof(IDisposable) },
            new[]
            {
                new DynaBeeGeneratedMemberPlan("Count", ElementType.Field, typeof(int)),
                new DynaBeeGeneratedMemberPlan("Name", ElementType.Property, typeof(string)),
                new DynaBeeGeneratedMemberPlan(".ctor", ElementType.Method, null),
                new DynaBeeGeneratedMemberPlan("Convert", ElementType.Method, typeof(List<int[]>), new[] { typeof(int), typeof(Dictionary<string, decimal>) }),
                new DynaBeeGeneratedMemberPlan("Changed", ElementType.Event, typeof(EventHandler))
            });
        var source = new StringBuilder();
        var mappings = new List<DynaBeeSourceMapEntry>();

        InvokePrivate("WriteClass", source, mappings, plan);

        var text = source.ToString();
        Assert.Contains("public partial class Model : global::System.IDisposable", text);
        Assert.Contains("private int Count;", text);
        Assert.Contains("public string Name { get; set; }", text);
        Assert.Contains("Constructor body", text);
        Assert.Contains("global::System.Collections.Generic.List<int[]> Convert(int arg0, global::System.Collections.Generic.Dictionary<string, decimal> arg1)", text);
        Assert.Contains("Unsupported planned element: Changed", text);
        Assert.Equal(6, mappings.Count);

        Assert.Equal("void", InvokePrivate("FormatType", (object)null!));
        Assert.Equal("void", InvokePrivate("FormatType", typeof(void)));
        Assert.Equal("string", InvokePrivate("FormatType", typeof(string)));
        Assert.Equal("object", InvokePrivate("FormatType", typeof(object)));
        Assert.Equal("bool", InvokePrivate("FormatType", typeof(bool)));
        Assert.Equal("int", InvokePrivate("FormatType", typeof(int)));
        Assert.Equal("decimal", InvokePrivate("FormatType", typeof(decimal)));
        Assert.Equal("int[]", InvokePrivate("FormatType", typeof(int[])));
        Assert.Equal("global::System.Collections.Generic.List<string>", InvokePrivate("FormatType", typeof(List<string>)));
        Assert.Equal("global::DynaBee.Tests.FluentApi.GenerationInternalCoverageTests.NestedType", InvokePrivate("FormatType", typeof(NestedType)));
    }

    [Fact]
    public void GenerationPlan_Validates_Internal_Model_Errors_And_Fingerprints_Nullable_Shape_Parts()
    {
        var plan = CreatePlan(
            "Broken",
            typeof(IDisposable),
            new Type[] { null!, typeof(string), typeof(string) },
            new[] { new DynaBeeGeneratedMemberPlan("Missing", ElementType.Field, null) },
            new Type[] { null! });
        var diagnostics = new List<DynaBeePlanDiagnostic>();

        InvokePlanPrivate("ValidateClass", plan, diagnostics);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Code == "DBP010");
        Assert.Contains(diagnostics, diagnostic => diagnostic.Code == "DBP020");
        Assert.Contains(diagnostics, diagnostic => diagnostic.Code == "DBP021");
        Assert.Contains(diagnostics, diagnostic => diagnostic.Code == "DBP030");
        Assert.Contains(diagnostics, diagnostic => diagnostic.Code == "DBP040");
        Assert.Equal("<null>", InvokePlanPrivate("FormatValue", (object)null!));
        Assert.Contains("System.Int32", (string)InvokePlanPrivate("FormatValue", 5));
    }

    [Fact]
    public void GenerationPlan_Fingerprint_Handles_Nullable_Declarative_Model_Parts_Deterministically()
    {
        var plan = new DynaBeeGenerationPlan("DynaBee.Tests.NullableFingerprint");
        var classPlan = CreatePlan(
            "NullableShape",
            null!,
            new Type[] { null!, typeof(IDisposable) },
            new[] { new DynaBeeGeneratedMemberPlan("Map", ElementType.Method, typeof(string), new Type[] { null!, typeof(int) }) },
            new Type[] { null! });
        var classes = (IList<DynaBeeGeneratedClassPlan>)typeof(DynaBeeGenerationPlan)
            .GetField("_classes", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(plan)!;

        classes.Add(classPlan);

        var first = plan.GetFingerprint();
        var second = plan.GetFingerprint();

        Assert.Equal(64, first.Length);
        Assert.Equal(first, second);
    }

    private static DynaBeeGeneratedClassPlan CreatePlan(
        string name,
        Type baseType,
        IReadOnlyList<Type> interfaces,
        IReadOnlyList<DynaBeeGeneratedMemberPlan> members,
        IReadOnlyList<Type> serviceTypes = null)
        => new(
            name,
            baseType,
            interfaces,
            serviceTypes ?? Array.Empty<Type>(),
            null,
            new Dictionary<string, object>(),
            members,
            Array.Empty<Action<BeeClassBuilder>>());

    private static object InvokePrivate(string name, params object[] arguments)
        => typeof(DynaBeeSourceExporter)
            .GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, arguments)!;

    private static object InvokePlanPrivate(string name, params object[] arguments)
        => typeof(DynaBeeGenerationPlan)
            .GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, arguments)!;

    public sealed class NestedType
    {
    }
}
