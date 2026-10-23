namespace DynaBee.FluentApi
{
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>
    /// Configures constraints for a generic parameter declared by a generated type.
    /// </summary>
    public sealed class BeeGenericTypeParameterBuilder
    {
        private readonly List<Type> _interfaceConstraints = new();

        internal BeeGenericTypeParameterBuilder(string name)
        {
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException(nameof(name)) : name;
        }

        /// <summary>
        /// Gets the generic parameter name.
        /// </summary>
        public string Name { get; }

        internal GenericParameterAttributes Attributes { get; private set; }

        internal Type BaseTypeConstraint { get; private set; }

        internal IReadOnlyList<Type> InterfaceConstraints => _interfaceConstraints;

        /// <summary>
        /// Requires a reference type argument.
        /// </summary>
        /// <returns>The same parameter builder.</returns>
        public BeeGenericTypeParameterBuilder ReferenceType()
        {
            Attributes |= GenericParameterAttributes.ReferenceTypeConstraint;
            return this;
        }

        /// <summary>
        /// Requires a non-nullable value type argument.
        /// </summary>
        /// <returns>The same parameter builder.</returns>
        public BeeGenericTypeParameterBuilder ValueType()
        {
            Attributes |= GenericParameterAttributes.NotNullableValueTypeConstraint;
            return this;
        }

        /// <summary>
        /// Requires a public parameterless constructor.
        /// </summary>
        /// <returns>The same parameter builder.</returns>
        public BeeGenericTypeParameterBuilder HasDefaultConstructor()
        {
            Attributes |= GenericParameterAttributes.DefaultConstructorConstraint;
            return this;
        }

        /// <summary>
        /// Adds a class constraint.
        /// </summary>
        /// <param name="baseType">Required base class.</param>
        /// <returns>The same parameter builder.</returns>
        public BeeGenericTypeParameterBuilder Inherits(Type baseType)
        {
            if (baseType == null)
                throw new ArgumentNullException(nameof(baseType));

            if (baseType.IsInterface || baseType.IsValueType)
                throw new ArgumentException("A generic base-type constraint must be a reference class.", nameof(baseType));

            BaseTypeConstraint = baseType;
            return this;
        }

        /// <summary>
        /// Adds an interface constraint.
        /// </summary>
        /// <param name="interfaceType">Required interface.</param>
        /// <returns>The same parameter builder.</returns>
        public BeeGenericTypeParameterBuilder Implements(Type interfaceType)
        {
            if (interfaceType == null)
                throw new ArgumentNullException(nameof(interfaceType));

            if (!interfaceType.IsInterface)
                throw new ArgumentException("A generic interface constraint must be an interface.", nameof(interfaceType));

            if (!_interfaceConstraints.Contains(interfaceType))
                _interfaceConstraints.Add(interfaceType);

            return this;
        }

        /// <summary>
        /// Adds a typed interface constraint.
        /// </summary>
        /// <typeparam name="TInterface">Required interface.</typeparam>
        /// <returns>The same parameter builder.</returns>
        public BeeGenericTypeParameterBuilder Implements<TInterface>()
            => Implements(typeof(TInterface));

        internal void Apply(GenericTypeParameterBuilder parameterBuilder)
        {
            parameterBuilder.SetGenericParameterAttributes(Attributes);
            if (BaseTypeConstraint != null)
                parameterBuilder.SetBaseTypeConstraint(BaseTypeConstraint);
            if (_interfaceConstraints.Count != 0)
                parameterBuilder.SetInterfaceConstraints(_interfaceConstraints.ToArray());
        }
    }
}
