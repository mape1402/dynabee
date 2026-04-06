namespace DynaBee
{
    /// <summary>
    /// Represents a typed metadata key for generated types and members.
    /// </summary>
    /// <typeparam name="T">Metadata value type.</typeparam>
    public readonly record struct BeeMetadataKey<T>
    {
        /// <summary>
        /// Creates a typed metadata key.
        /// </summary>
        public BeeMetadataKey(string name)
        {
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException(nameof(name)) : name;
        }

        /// <summary>
        /// Gets the metadata key name.
        /// </summary>
        public string Name { get; }
    }
}
