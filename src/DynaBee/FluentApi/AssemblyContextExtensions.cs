namespace DynaBee.FluentApi
{
    /// <summary>
    /// Helper methods for consuming generated types from an assembly context.
    /// </summary>
    public static class AssemblyContextExtensions
    {
        /// <summary>
        /// Gets the generated CLR type by logical type name.
        /// </summary>
        public static Type GetClrType(this IAssemblyContext assemblyContext, string typeName)
        {
            if (assemblyContext == null)
                throw new ArgumentNullException(nameof(assemblyContext));

            return assemblyContext.Find(typeName).ClrType;
        }

        /// <summary>
        /// Creates a new instance for a generated type by logical type name.
        /// </summary>
        public static object CreateInstance(this IAssemblyContext assemblyContext, string typeName, params object[] args)
        {
            var clrType = assemblyContext.GetClrType(typeName);
            return Activator.CreateInstance(clrType, args);
        }

        /// <summary>
        /// Creates a new instance and casts it to T.
        /// </summary>
        public static T CreateInstance<T>(this IAssemblyContext assemblyContext, string typeName, params object[] args)
            => (T)assemblyContext.CreateInstance(typeName, args);
    }
}