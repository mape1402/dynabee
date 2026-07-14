namespace DynaBee.FluentApi.DependencyInjection
{
    /// <summary>
    /// Provides access to the current immutable assembly context and allows controlled rebuilds.
    /// </summary>
    public interface IAssemblyContextProvider
    {
        /// <summary>
        /// Gets the latest immutable assembly context snapshot.
        /// </summary>
        IAssemblyContext Current { get; }

        /// <summary>
        /// Gets the current internal generation number.
        /// </summary>
        long Generation { get; }

        /// <summary>
        /// Rebuilds and replaces the current snapshot using the latest registry definitions.
        /// </summary>
        /// <returns>The newly built assembly context snapshot.</returns>
        IAssemblyContext Rebuild();
    }
}
