namespace DynaBee.FluentApi.Extensions
{
    /// <summary>
    /// Defines a typed, optional metadata key and validation rule for extension packages.
    /// </summary>
    /// <typeparam name="T">Expected metadata value type.</typeparam>
    public sealed class DynaBeeMetadataSchema<T> : IDynaBeeMetadataSchema
    {
        private readonly Func<T, bool> _validator;

        /// <summary>
        /// Initializes a new schema.
        /// </summary>
        /// <param name="key">Metadata key.</param>
        /// <param name="validator">Optional validity predicate.</param>
        /// <param name="description">Human-readable rule description.</param>
        public DynaBeeMetadataSchema(BeeMetadataKey<T> key, Func<T, bool> validator = null, string description = null)
        {
            Key = key;
            _validator = validator;
            Description = description;
        }

        /// <summary>Gets the typed metadata key.</summary>
        public BeeMetadataKey<T> Key { get; }

        /// <summary>Gets the optional human-readable rule description.</summary>
        public string Description { get; }

        /// <inheritdoc/>
        public void Validate(IReadOnlyDictionary<string, object> metadata, string typeName, DynaBeeExtensionValidationContext context)
        {
            if (!metadata.TryGetValue(Key.Name, out var value))
                return;

            if (value is not T typedValue)
            {
                context.Error(
                    "DBX001",
                    $"Metadata '{Key.Name}' must be '{typeof(T).FullName}', not '{value?.GetType().FullName ?? "null"}'.",
                    typeName);
                return;
            }

            if (_validator != null && !_validator(typedValue))
            {
                context.Error(
                    "DBX002",
                    Description ?? $"Metadata '{Key.Name}' has an invalid value.",
                    typeName);
            }
        }
    }
}
