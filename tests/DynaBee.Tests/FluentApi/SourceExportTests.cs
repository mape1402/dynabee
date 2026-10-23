namespace DynaBee.Tests.FluentApi;

using DynaBee.FluentApi.Generation;

public sealed class SourceExportTests
{
    [Fact]
    public void ExportSource_Produces_Deterministic_Mapped_CSharp_Shape()
    {
        var plan = new DynaBeeGenerationPlan("DynaBee.Tests.SourceExport")
            .AddClass("Demo.Generated.Customer", type => type
                .AddProperty("Name", typeof(string))
                .AddMethod("Format", typeof(string), method => method
                    .WithParameter<int>("id")
                    .EmitsBody(body => body.Return(body.Constant("customer")))));

        var export = plan.ExportSource();

        Assert.Contains("namespace Demo.Generated;", export.Source);
        Assert.Contains("public partial class Customer", export.Source);
        Assert.Contains("public string Name { get; set; }", export.Source);
        Assert.Contains("public string Format(int arg0)", export.Source);
        Assert.Contains(export.Mappings, x => x.TypeName == "Demo.Generated.Customer" && x.MemberName == "Format");
        Assert.Contains(plan.GetFingerprint(), export.Source);
    }

    [Fact]
    public void SourceExportResult_Writes_A_Stable_Utf8_File()
    {
        var plan = new DynaBeeGenerationPlan("DynaBee.Tests.SourceExport.File")
            .AddClass("Generated", type => type.AddProperty("Value", typeof(int)));
        var path = Path.Combine(Path.GetTempPath(), "dynabee-source-" + Guid.NewGuid().ToString("N"), "Generated.g.cs");

        plan.ExportSource().WriteTo(path);

        Assert.True(File.Exists(path));
        Assert.Contains("Generated", File.ReadAllText(path));
        Directory.Delete(Path.GetDirectoryName(path)!, recursive: true);
    }
}
