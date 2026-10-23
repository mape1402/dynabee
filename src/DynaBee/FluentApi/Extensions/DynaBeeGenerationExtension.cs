namespace DynaBee.FluentApi.Extensions
{
    using DynaBee.FluentApi.Generation;

    /// <summary>
    /// Convenience base class for optional DynaBee generation extension callbacks.
    /// </summary>
    public abstract class DynaBeeGenerationExtension : IDynaBeeGenerationExtension
    {
        /// <inheritdoc/>
        public virtual void Validate(DynaBeeGenerationPlan plan, DynaBeeExtensionValidationContext context)
        {
        }

        /// <inheritdoc/>
        public virtual void Configure(DynaBeeGenerationPlan plan, IBeeAssemblyBuilder builder)
        {
        }

        /// <inheritdoc/>
        public virtual void OnGenerated(DynaBeeGenerationPlan plan, IAssemblyContext context)
        {
        }
    }
}
