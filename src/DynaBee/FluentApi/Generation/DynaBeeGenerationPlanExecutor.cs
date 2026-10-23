namespace DynaBee.FluentApi.Generation
{
    using DynaBee.FluentApi.DependencyInjection;

    /// <summary>
    /// Default DI-first generation-plan executor.
    /// </summary>
    public sealed class DynaBeeGenerationPlanExecutor : IDynaBeeGenerationPlanExecutor
    {
        private readonly IDynaBeeAssemblyBuilderFactory _builderFactory;
        private readonly IDynaBeeGenerationPlanCache _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynaBeeGenerationPlanExecutor"/> class.
        /// </summary>
        public DynaBeeGenerationPlanExecutor(
            IDynaBeeAssemblyBuilderFactory builderFactory,
            IDynaBeeGenerationPlanCache cache)
        {
            _builderFactory = builderFactory ?? throw new ArgumentNullException(nameof(builderFactory));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <inheritdoc/>
        public IAssemblyContext Build(DynaBeeGenerationPlan plan, bool useCache = true)
        {
            if (plan == null)
                throw new ArgumentNullException(nameof(plan));

            IAssemblyContext BuildCore()
            {
                var builder = _builderFactory.Create(plan.AssemblyName).DisableCache();
                plan.ApplyTo(builder);
                return builder.Build();
            }

            return useCache ? _cache.GetOrAdd(plan.GetFingerprint(), BuildCore) : BuildCore();
        }
    }
}
