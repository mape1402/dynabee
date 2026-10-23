namespace DynaBee.FluentApi.Extensions
{
    using DynaBee.FluentApi.Generation;

    /// <summary>
    /// Validates metadata attached to a generation plan scope.
    /// </summary>
    public interface IDynaBeeMetadataSchema
    {
        /// <summary>Validates metadata for an assembly or generated type.</summary>
        /// <param name="metadata">Metadata values to validate.</param>
        /// <param name="typeName">Generated type name, or null for assembly metadata.</param>
        /// <param name="context">Diagnostic reporting context.</param>
        void Validate(IReadOnlyDictionary<string, object> metadata, string typeName, DynaBeeExtensionValidationContext context);
    }
}
