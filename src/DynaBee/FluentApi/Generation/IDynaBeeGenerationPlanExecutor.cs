namespace DynaBee.FluentApi.Generation
{
    /// <summary>
    /// Builds generation plans through dependency-injected DynaBee services.
    /// </summary>
    public interface IDynaBeeGenerationPlanExecutor
    {
        /// <summary>
        /// Builds a plan, optionally reusing the context cached for its fingerprint.
        /// </summary>
        /// <param name="plan">Generation plan to build.</param>
        /// <param name="useCache">True to reuse a cached immutable context.</param>
        /// <returns>The generated assembly context.</returns>
        IAssemblyContext Build(DynaBeeGenerationPlan plan, bool useCache = true);
    }
}
