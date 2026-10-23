namespace DynaBee.Tests.Testing;

using DynaBee.Testing;
using NSubstitute;

public sealed class DynaBeeGeneratedSourceWriterTests
{
    [Fact]
    public void WriteType_Requires_A_Context_With_A_Clr_Type()
    {
        Assert.Throws<ArgumentNullException>(() => DynaBeeGeneratedSourceWriter.WriteType(null!));

        var context = Substitute.For<ITypeContext>();
        context.ClrType.Returns((Type)null!);

        Assert.Throws<ArgumentNullException>(() => DynaBeeGeneratedSourceWriter.WriteType(context));
    }

    [Fact]
    public void WriteType_Describes_Inheritance_Generic_Members_And_Access_Modifiers()
    {
        var source = DynaBeeGeneratedSourceWriter.WriteType(ContextFor(
            typeof(SourceShape),
            new Dictionary<string, object>
            {
                ["zeta"] = 2,
                ["alpha"] = 1
            }));

        Assert.Contains("// metadata: alpha = 1", source);
        Assert.Contains("public class SourceShape : SourceBase<Int32>, ISourceContract<String>", source);
        Assert.Contains("public SourceShape();", source);
        Assert.Contains("protected SourceShape(Boolean value);", source);
        Assert.Contains("internal SourceShape(Byte value);", source);
        Assert.Contains("protected internal SourceShape(Int16 value);", source);
        Assert.Contains("private protected SourceShape(Char value);", source);
        Assert.Contains("private SourceShape(Int32 value);", source);
        Assert.Contains("List<Int32> PublicCollection { get; set; }", source);
        Assert.Contains("SetterOnly { set; }", source);
        Assert.Contains("GetterOnly { get; }", source);
        Assert.Contains("protected Void ProtectedMethod();", source);
        Assert.Contains("internal Void InternalMethod();", source);
        Assert.Contains("protected internal Void ProtectedInternalMethod();", source);
        Assert.Contains("private protected Void PrivateProtectedMethod();", source);
        Assert.Contains("private Void PrivateMethod();", source);
    }

    [Fact]
    public void WriteType_Describes_Interface_Enum_And_Struct_Shapes()
    {
        Assert.Contains("public interface ISourceContract", DynaBeeGeneratedSourceWriter.WriteType(ContextFor(typeof(ISourceContract<string>))));
        Assert.Contains("public enum SourceEnum", DynaBeeGeneratedSourceWriter.WriteType(ContextFor(typeof(SourceEnum))));
        Assert.Contains("public struct SourceStruct", DynaBeeGeneratedSourceWriter.WriteType(ContextFor(typeof(SourceStruct))));
    }

    private static ITypeContext ContextFor(Type type, IReadOnlyDictionary<string, object> metadata = null)
    {
        var context = Substitute.For<ITypeContext>();
        context.Name.Returns(type.Name);
        context.ClrType.Returns(type);
        context.Metadata.Returns(metadata ?? new Dictionary<string, object>());
        return context;
    }

    private interface ISourceContract<T>
    {
    }

    private class SourceBase<T>
    {
    }

    private class SourceShape : SourceBase<int>, ISourceContract<string>
    {
        public SourceShape()
        {
        }

        protected SourceShape(bool value)
        {
        }

        internal SourceShape(byte value)
        {
        }

        protected internal SourceShape(short value)
        {
        }

        private protected SourceShape(char value)
        {
        }

        private SourceShape(int value)
        {
        }

        public List<int> PublicCollection { get; set; }

        public string SetterOnly
        {
            set { }
        }

        public string GetterOnly => string.Empty;

        public void PublicMethod(List<int> values)
        {
        }

        protected void ProtectedMethod()
        {
        }

        internal void InternalMethod()
        {
        }

        protected internal void ProtectedInternalMethod()
        {
        }

        private protected void PrivateProtectedMethod()
        {
        }

        private void PrivateMethod()
        {
        }
    }

    private enum SourceEnum
    {
        First
    }

    private struct SourceStruct
    {
        public int Value { get; set; }
    }
}
