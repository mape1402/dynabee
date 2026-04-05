namespace DynaBee.Infrastructure
{
    using System.Reflection;

    /// <summary>
    /// Represents valid access modifiers for methods.
    /// </summary>
    public readonly struct MethodAccessModifier
    {
        private readonly MethodAttributes _attributes;

        private MethodAccessModifier(MethodAttributes attributes)
        {
            _attributes = attributes & MethodAttributes.MemberAccessMask;
        }

        /// <summary>
        /// Gets whether this instance is uninitialized.
        /// </summary>
        public bool IsDefault => _attributes == 0;

        /// <summary>
        /// Gets the raw member access method attributes.
        /// </summary>
        internal MethodAttributes Attributes => _attributes;

        /// <summary>
        /// Public method.
        /// </summary>
        public static MethodAccessModifier Public => new(MethodAttributes.Public);

        /// <summary>
        /// Private method.
        /// </summary>
        public static MethodAccessModifier Private => new(MethodAttributes.Private);

        /// <summary>
        /// Internal method.
        /// </summary>
        public static MethodAccessModifier Internal => new(MethodAttributes.Assembly);

        /// <summary>
        /// Protected method.
        /// </summary>
        public static MethodAccessModifier Protected => new(MethodAttributes.Family);

        /// <summary>
        /// Protected internal method.
        /// </summary>
        public static MethodAccessModifier ProtectedInternal => new(MethodAttributes.FamORAssem);

        /// <summary>
        /// Private protected method.
        /// </summary>
        public static MethodAccessModifier PrivateProtected => new(MethodAttributes.FamANDAssem);

        /// <summary>
        /// Implicit conversion to method attributes.
        /// </summary>
        public static implicit operator MethodAttributes(MethodAccessModifier modifier)
            => modifier._attributes;

        /// <inheritdoc/>
        public override bool Equals(object obj)
            => obj is MethodAccessModifier other && _attributes == other._attributes;

        /// <inheritdoc/>
        public override int GetHashCode()
            => _attributes.GetHashCode();

        /// <inheritdoc/>
        public override string ToString()
            => _attributes.ToString();
    }
}