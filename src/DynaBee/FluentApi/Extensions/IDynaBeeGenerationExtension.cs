namespace DynaBee.FluentApi.Extensions
{
    using DynaBee.FluentApi.Generation;

    /// <summary>
    /// Extends DynaBee generation through dependency injection without coupling behavior to the core library.
    /// </summary>
    public interface IDynaBeeGenerationExtension
    {
        /// <summary>
        /// Validates extension-specific plan metadata before an assembly is emitted.
        /// </summary>
        /// <param name="plan">Plan being built.</param>
        /// <param name="context">Diagnostic reporting context.</param>
        void Validate(DynaBeeGenerationPlan plan, DynaBeeExtensionValidationContext context);

        /// <summary>
        /// Applies extension configuration after the plan has populated an assembly builder and before it is built.
        /// </summary>
        /// <param name="plan">Plan being built.</param>
        /// <param name="builder">Assembly builder to extend.</param>
        void Configure(DynaBeeGenerationPlan plan, IBeeAssemblyBuilder builder);

        /// <summary>
        /// Observes the immutable generated context after emission.
        /// </summary>
        /// <param name="plan">Plan that was built.</param>
        /// <param name="context">Generated immutable context.</param>
        void OnGenerated(DynaBeeGenerationPlan plan, IAssemblyContext context);
    }
}
