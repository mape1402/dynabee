namespace DynaBee.FluentApi
{
    /// <summary>
    /// Entry point for building dynamic assemblies with DynaBee fluent API.
    /// </summary>
    public static class DynaBeeBuilder
    {
        /// <summary>
        /// Creates a new dynamic assembly builder.
        /// </summary>
        /// <param name="assemblyName">Logical name of the dynamic assembly.</param>
        /// <returns>A fluent assembly builder.</returns>
        public static BeeAssemblyBuilder CreateAssembly(string assemblyName)
            => new BeeAssemblyBuilder(assemblyName);
    }
}