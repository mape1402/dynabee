namespace DynaBee.FluentApi
{
    using DynaBee.Infrastructure;
    using DynaBee.Infrastructure.Configurators;

    /// <summary>
    /// Fluent builder for dynamic interface method signatures.
    /// </summary>
    public sealed class BeeInterfaceMethodBuilder
    {
        private readonly List<(string Name, BeeType Type)> _parameters = new();
        private readonly List<BeeAttribute> _attributes = new();
        private MethodAccessModifier _accessModifier;

        internal BeeInterfaceMethodBuilder(string name, BeeType returnType)
        {
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException(nameof(name)) : name;
            ReturnType = returnType;
        }

        /// <summary>
        /// Gets the method name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the method return type.
        /// </summary>
        public BeeType ReturnType { get; }

        /// <summary>
        /// Adds a parameter to the interface method signature.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <param name="type">Parameter type.</param>
        /// <returns>The current builder instance.</returns>
        public BeeInterfaceMethodBuilder WithParameter(string name, BeeType type)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));

            _parameters.Add((name, type));
            return this;
        }

        /// <summary>
        /// Adds a parameter to the interface method signature.
        /// </summary>
        /// <typeparam name="T">Parameter type.</typeparam>
        /// <param name="name">Parameter name.</param>
        /// <returns>The current builder instance.</returns>
        public BeeInterfaceMethodBuilder WithParameter<T>(string name)
            => WithParameter(name, typeof(T));

        /// <summary>
        /// Sets the access modifier for the generated method signature.
        /// </summary>
        /// <param name="accessModifier">Method access modifier.</param>
        /// <returns>The current builder instance.</returns>
        public BeeInterfaceMethodBuilder WithAccess(MethodAccessModifier accessModifier)
        {
            _accessModifier = accessModifier;
            return this;
        }

        /// <summary>
        /// Adds a custom attribute to the generated method signature.
        /// </summary>
        /// <param name="attribute">Attribute descriptor.</param>
        /// <returns>The current builder instance.</returns>
        public BeeInterfaceMethodBuilder AddAttribute(BeeAttribute attribute)
        {
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));

            _attributes.Add(attribute);
            return this;
        }

        /// <summary>
        /// Adds a custom attribute to the generated method signature using fluent configuration.
        /// </summary>
        /// <typeparam name="TAttribute">Attribute type.</typeparam>
        /// <param name="configure">Attribute configuration callback.</param>
        /// <returns>The current builder instance.</returns>
        public BeeInterfaceMethodBuilder AddAttribute<TAttribute>(Action<BeeAttributeBuilder> configure)
            where TAttribute : Attribute
        {
            var builder = new BeeAttributeBuilder(typeof(TAttribute));
            configure?.Invoke(builder);
            return AddAttribute(builder.Build());
        }

        internal InterfaceMethodConfigurator ToConfigurator()
            => new(Name, ReturnType, _parameters, _accessModifier, _attributes);
    }
}
