namespace DynaBee.Infrastructure
{
    using System.Reflection;

    /// <summary>
    /// Represents valid access modifiers for fields.
    /// </summary>
    public readonly struct FieldAccessModifier
    {
        private readonly FieldAttributes _attributes;

        private FieldAccessModifier(FieldAttributes attributes)
        {
            _attributes = attributes & FieldAttributes.FieldAccessMask;
        }

        /// <summary>
        /// Gets whether this instance is uninitialized.
        /// </summary>
        public bool IsDefault => _attributes == 0;

        /// <summary>
        /// Gets the raw member access field attributes.
        /// </summary>
        internal FieldAttributes Attributes => _attributes;

        /// <summary>
        /// Public field.
        /// </summary>
        public static FieldAccessModifier Public => new(FieldAttributes.Public);

        /// <summary>
        /// Private field.
        /// </summary>
        public static FieldAccessModifier Private => new(FieldAttributes.Private);

        /// <summary>
        /// Internal field.
        /// </summary>
        public static FieldAccessModifier Internal => new(FieldAttributes.Assembly);

        /// <summary>
        /// Protected field.
        /// </summary>
        public static FieldAccessModifier Protected => new(FieldAttributes.Family);

        /// <summary>
        /// Protected internal field.
        /// </summary>
        public static FieldAccessModifier ProtectedInternal => new(FieldAttributes.FamORAssem);

        /// <summary>
        /// Private protected field.
        /// </summary>
        public static FieldAccessModifier PrivateProtected => new(FieldAttributes.FamANDAssem);

        /// <summary>
        /// Implicit conversion to field attributes.
        /// </summary>
        public static implicit operator FieldAttributes(FieldAccessModifier modifier)
            => modifier._attributes;

        /// <inheritdoc/>
        public override bool Equals(object obj)
            => obj is FieldAccessModifier other && _attributes == other._attributes;

        /// <inheritdoc/>
        public override int GetHashCode()
            => _attributes.GetHashCode();

        /// <inheritdoc/>
        public override string ToString()
            => _attributes.ToString();
    }
}