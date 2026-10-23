namespace DynaBee.FluentApi.Generation
{
    /// <summary>
    /// Maps a generated C# declaration to a DynaBee plan element.
    /// </summary>
    public sealed class DynaBeeSourceMapEntry
    {
        internal DynaBeeSourceMapEntry(string typeName, string memberName, int line)
        {
            TypeName = typeName;
            MemberName = memberName;
            Line = line;
        }

        /// <summary>Gets the logical generated type name.</summary>
        public string TypeName { get; }

        /// <summary>Gets the member name, or null for a type declaration.</summary>
        public string MemberName { get; }

        /// <summary>Gets the one-based generated source line.</summary>
        public int Line { get; }
    }
}
