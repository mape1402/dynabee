namespace DynaBee.FluentApi.Generation
{
    /// <summary>
    /// Caches immutable generated assembly contexts by a plan fingerprint.
    /// </summary>
    public interface IDynaBeeGenerationPlanCache
    {
        /// <summary>
        /// Gets or creates a context for a fingerprint.
        /// </summary>
        /// <param name="fingerprint">Stable plan fingerprint.</param>
        /// <param name="factory">Context factory used on a cache miss.</param>
        /// <returns>The cached or newly created context.</returns>
        IAssemblyContext GetOrAdd(string fingerprint, Func<IAssemblyContext> factory);

        /// <summary>
        /// Removes all cached contexts.
        /// </summary>
        void Clear();
    }
}
