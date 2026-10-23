namespace DynaBee.FluentApi.Generation
{
    using DynaBee.FluentApi.DependencyInjection;
    using DynaBee.FluentApi.Extensions;

    /// <summary>
    /// Default DI-first generation-plan executor.
    /// </summary>
    public sealed class DynaBeeGenerationPlanExecutor : IDynaBeeGenerationPlanExecutor
    {
        private readonly IDynaBeeAssemblyBuilderFactory _builderFactory;
        private readonly IDynaBeeGenerationPlanCache _cache;
        private readonly IReadOnlyList<IDynaBeeGenerationExtension> _extensions;
        private readonly IReadOnlyList<IDynaBeeMetadataSchema> _metadataSchemas;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynaBeeGenerationPlanExecutor"/> class.
        /// </summary>
        public DynaBeeGenerationPlanExecutor(
            IDynaBeeAssemblyBuilderFactory builderFactory,
            IDynaBeeGenerationPlanCache cache,
            IEnumerable<IDynaBeeGenerationExtension> extensions,
            IEnumerable<IDynaBeeMetadataSchema> metadataSchemas)
        {
            _builderFactory = builderFactory ?? throw new ArgumentNullException(nameof(builderFactory));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _extensions = (extensions ?? throw new ArgumentNullException(nameof(extensions))).ToArray();
            _metadataSchemas = (metadataSchemas ?? throw new ArgumentNullException(nameof(metadataSchemas))).ToArray();
        }

        /// <inheritdoc/>
        public IAssemblyContext Build(DynaBeeGenerationPlan plan, bool useCache = true)
        {
            if (plan == null)
                throw new ArgumentNullException(nameof(plan));

            IAssemblyContext BuildCore()
            {
                var validation = new DynaBeeExtensionValidationContext();
                foreach (var schema in _metadataSchemas)
                {
                    schema.Validate(plan.Metadata, null, validation);
                    foreach (var classPlan in plan.Classes)
                        schema.Validate(classPlan.Metadata, classPlan.Name, validation);
                }

                foreach (var extension in _extensions)
                    extension.Validate(plan, validation);
                validation.ThrowIfErrors();

                var builder = _builderFactory.Create(plan.AssemblyName).DisableCache();
                plan.ApplyTo(builder);
                foreach (var extension in _extensions)
                    extension.Configure(plan, builder);

                var context = builder.Build();
                foreach (var extension in _extensions)
                    extension.OnGenerated(plan, context);
                return context;
            }

            var extensionKey = string.Join("|", _extensions.Select(x => x.GetType().AssemblyQualifiedName));
            var cacheKey = $"{plan.GetFingerprint()}::{extensionKey}";
            return useCache ? _cache.GetOrAdd(cacheKey, BuildCore) : BuildCore();
        }
    }
}
