namespace DynaBee.Infrastructure
{
    /// <summary>
    /// Represents a dynamic type abstraction that can be either a direct <see cref="Type"/> reference 
    /// or a string-based type name to be resolved later.
    /// </summary>
    public struct BeeType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BeeType"/> struct using a reference type name.
        /// </summary>
        /// <param name="referenceType">The string name of the reference type.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="referenceType"/> is null or whitespace.</exception>
        private BeeType(string referenceType)
        {
            ReferenceType = string.IsNullOrWhiteSpace(referenceType) ? throw new ArgumentNullException(nameof(referenceType)) : referenceType;
            IsReference = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BeeType"/> struct using a direct <see cref="Type"/> reference.
        /// </summary>
        /// <param name="clrType">The <see cref="Type"/> to represent.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="clrType"/> is null.</exception>
        private BeeType(Type clrType)
        {
            ClrType = clrType ?? throw new ArgumentNullException(nameof(clrType));
            IsReference = false;
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="BeeType"/> represents a reference by name rather than a concrete <see cref="Type"/>.
        /// </summary>
        internal bool IsReference { get; }

        /// <summary>
        /// Gets the string name of the reference type, if <see cref="IsReference"/> is true.
        /// </summary>
        internal string ReferenceType { get; }

        /// <summary>
        /// Gets the concrete <see cref="Type"/> represented by this instance, if <see cref="IsReference"/> is false.
        /// </summary>
        internal Type ClrType { get; }

        /// <summary>
        /// Implicitly converts a string type name to a <see cref="BeeType"/>.
        /// </summary>
        /// <param name="referenceType">The string reference type name.</param>
        public static implicit operator BeeType(string referenceType)
            => new BeeType(referenceType);

        /// <summary>
        /// Implicitly converts a <see cref="Type"/> to a <see cref="BeeType"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to represent.</param>
        public static implicit operator BeeType(Type type)
            => new BeeType(type);

        /// <summary>
        /// Implicitly converts a <see cref="BeeType"/> to its reference type name if it represents a reference.
        /// </summary>
        /// <param name="beeType">The <see cref="BeeType"/> instance to convert.</param>
        /// <returns>The reference type name as a string.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <paramref name="beeType"/> is not a reference-based type.
        /// </exception>
        public static implicit operator string(BeeType beeType)
            => beeType.IsReference ? beeType.ReferenceType : throw new InvalidOperationException("BeeType isn't a ReferenceType");

        /// <summary>
        /// Implicitly converts a <see cref="BeeType"/> to its underlying <see cref="Type"/> if it represents a CLR type.
        /// </summary>
        /// <param name="beeType">The <see cref="BeeType"/> instance to convert.</param>
        /// <returns>The CLR <see cref="Type"/>.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <paramref name="beeType"/> is not a CLR type.
        /// </exception>
        public static implicit operator Type(BeeType beeType)
            => !beeType.IsReference ? beeType.ClrType : throw new InvalidOperationException("BeeType isn't a ClrType");

    }
}
