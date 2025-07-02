namespace DynaBee.Infrastructure
{
    using System.Reflection;

    /// <summary>
    /// Represents valid access modifiers for classes when using Reflection.Emit.
    /// Restricts usage to combinations allowed by C# and the .NET runtime.
    /// </summary>
    public readonly struct ClassAccessModifier
    {
        private readonly TypeAttributes _attributes;

        private ClassAccessModifier(TypeAttributes attributes)
        {
            _attributes = attributes;
        }

        /// <summary>
        /// Gets whether this instance is uninitialized (equal to 'default').
        /// </summary>
        public bool IsDefault => _attributes == 0;

        /// <summary>
        /// Implicitly converts a <see cref="ClassAccessModifier"/> to <see cref="TypeAttributes"/>.
        /// </summary>
        /// <param name="modifier">The custom access modifier instance.</param>
        public static implicit operator TypeAttributes(ClassAccessModifier modifier) => modifier._attributes;

        /// <summary>
        /// Indicates whether two <see cref="ClassAccessModifier"/> instances are equal.
        /// </summary>
        /// <param name="left">The first modifier.</param>
        /// <param name="right">The second modifier.</param>
        /// <returns><c>true</c> if they represent the same access level; otherwise, <c>false</c>.</returns>
        public static bool operator ==(ClassAccessModifier left, ClassAccessModifier right)
            => left._attributes == right._attributes;

        /// <summary>
        /// Indicates whether two <see cref="ClassAccessModifier"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first modifier.</param>
        /// <param name="right">The second modifier.</param>
        /// <returns><c>true</c> if they represent different access levels; otherwise, <c>false</c>.</returns>
        public static bool operator !=(ClassAccessModifier left, ClassAccessModifier right)
            => !(left == right);

        /// <summary>
        /// Determines whether this instance is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns><c>true</c> if the object is a <see cref="ClassAccessModifier"/> with the same access level.</returns>
        public override bool Equals(object obj)
            => obj is ClassAccessModifier other && this == other;

        /// <summary>
        /// Returns a hash code for the current modifier.
        /// </summary>
        public override int GetHashCode()
            => _attributes.GetHashCode();

        // ----------- Top-level class modifiers -----------

        /// <summary>
        /// Public class (visible from other assemblies).
        /// </summary>
        public static ClassAccessModifier Public => new(TypeAttributes.Public);

        /// <summary>
        /// Internal class (visible only within the same assembly).
        /// Equivalent to C#'s 'internal'.
        /// </summary>
        public static ClassAccessModifier Internal => new(TypeAttributes.NotPublic);

        // ----------- Nested class modifiers -----------

        /// <summary>
        /// Nested public class (visible anywhere if the containing class is accessible).
        /// </summary>
        public static ClassAccessModifier NestedPublic => new(TypeAttributes.NestedPublic);

        /// <summary>
        /// Nested private class (visible only within the containing class).
        /// </summary>
        public static ClassAccessModifier NestedPrivate => new(TypeAttributes.NestedPrivate);

        /// <summary>
        /// Nested protected class (visible only to derived classes).
        /// </summary>
        public static ClassAccessModifier NestedProtected => new(TypeAttributes.NestedFamily);

        /// <summary>
        /// Nested internal class (visible only within the same assembly).
        /// </summary>
        public static ClassAccessModifier NestedInternal => new(TypeAttributes.NestedAssembly);

        /// <summary>
        /// Nested protected internal class (visible to derived classes or within the same assembly).
        /// Equivalent to C#'s 'protected internal'.
        /// </summary>
        public static ClassAccessModifier NestedProtectedInternal => new(TypeAttributes.NestedFamORAssem);

        /// <summary>
        /// Nested private protected class (visible to derived classes within the same assembly).
        /// Equivalent to C#'s 'private protected'.
        /// </summary>
        public static ClassAccessModifier NestedPrivateProtected => new(TypeAttributes.NestedFamANDAssem);

        /// <summary>
        /// Returns the underlying <see cref="TypeAttributes"/> as a string for debugging purposes.
        /// </summary>
        public override string ToString() => _attributes.ToString();
    }
}
