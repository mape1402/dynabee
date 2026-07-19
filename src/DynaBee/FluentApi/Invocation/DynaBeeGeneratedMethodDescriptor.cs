namespace DynaBee.FluentApi.Invocation
{
    /// <summary>
    /// Describes a generated method using stable metadata suitable for diagnostics and cache keys.
    /// </summary>
    public sealed class DynaBeeGeneratedMethodDescriptor
    {
        /// <summary>
        /// Initializes a new generated method descriptor.
        /// </summary>
        /// <param name="declaringType">Runtime type that declares the generated method.</param>
        /// <param name="name">Generated method name.</param>
        /// <param name="parameterTypes">Generated method parameter types.</param>
        /// <param name="returnType">Generated method return type.</param>
        public DynaBeeGeneratedMethodDescriptor(
            Type declaringType,
            string name,
            IReadOnlyList<Type> parameterTypes,
            Type returnType)
        {
            DeclaringType = declaringType ?? throw new ArgumentNullException(nameof(declaringType));
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException(nameof(name)) : name;
            ParameterTypes = parameterTypes?.ToArray() ?? throw new ArgumentNullException(nameof(parameterTypes));
            ReturnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
        }

        /// <summary>
        /// Gets the runtime type that declares the generated method.
        /// </summary>
        public Type DeclaringType { get; }

        /// <summary>
        /// Gets the generated method name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the generated method parameter types in declaration order.
        /// </summary>
        public IReadOnlyList<Type> ParameterTypes { get; }

        /// <summary>
        /// Gets the generated method return type.
        /// </summary>
        public Type ReturnType { get; }
    }
}
