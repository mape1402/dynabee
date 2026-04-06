namespace DynaBee.FluentApi
{
    using DynaBee.Infrastructure;
    using DynaBee.Infrastructure.Configurators;

    /// <summary>
    /// Fluent builder for dynamic interface property signatures.
    /// </summary>
    public sealed class BeeInterfacePropertyBuilder
    {
        private readonly List<BeeAttribute> _attributes = new();

        internal BeeInterfacePropertyBuilder(string name, BeeType type)
        {
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException(nameof(name)) : name;
            Type = type;
        }

        /// <summary>
        /// Gets the property name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the property type.
        /// </summary>
        public BeeType Type { get; }

        /// <summary>
        /// Gets whether the property defines a getter.
        /// </summary>
        public bool HasGetter { get; private set; } = true;

        /// <summary>
        /// Gets whether the property defines a setter.
        /// </summary>
        public bool HasSetter { get; private set; } = true;

        /// <summary>
        /// Gets the getter access modifier.
        /// </summary>
        public MethodAccessModifier GetterAccessModifier { get; private set; } = MethodAccessModifier.Public;

        /// <summary>
        /// Gets the setter access modifier.
        /// </summary>
        public MethodAccessModifier SetterAccessModifier { get; private set; } = MethodAccessModifier.Public;

        /// <summary>
        /// Sets the property to read-only (getter only).
        /// </summary>
        /// <returns>The current builder instance.</returns>
        public BeeInterfacePropertyBuilder AsReadOnly()
        {
            HasGetter = true;
            HasSetter = false;
            return this;
        }

        /// <summary>
        /// Sets the property to write-only (setter only).
        /// </summary>
        /// <returns>The current builder instance.</returns>
        public BeeInterfacePropertyBuilder AsWriteOnly()
        {
            HasGetter = false;
            HasSetter = true;
            return this;
        }

        /// <summary>
        /// Enables or disables the getter.
        /// </summary>
        /// <param name="enabled">Whether getter should be enabled.</param>
        /// <returns>The current builder instance.</returns>
        public BeeInterfacePropertyBuilder WithGetter(bool enabled = true)
        {
            HasGetter = enabled;
            return this;
        }

        /// <summary>
        /// Enables or disables the setter.
        /// </summary>
        /// <param name="enabled">Whether setter should be enabled.</param>
        /// <returns>The current builder instance.</returns>
        public BeeInterfacePropertyBuilder WithSetter(bool enabled = true)
        {
            HasSetter = enabled;
            return this;
        }

        /// <summary>
        /// Sets the getter access modifier.
        /// </summary>
        /// <param name="accessModifier">Getter access modifier.</param>
        /// <returns>The current builder instance.</returns>
        public BeeInterfacePropertyBuilder WithGetterAccess(MethodAccessModifier accessModifier)
        {
            GetterAccessModifier = accessModifier;
            return this;
        }

        /// <summary>
        /// Sets the setter access modifier.
        /// </summary>
        /// <param name="accessModifier">Setter access modifier.</param>
        /// <returns>The current builder instance.</returns>
        public BeeInterfacePropertyBuilder WithSetterAccess(MethodAccessModifier accessModifier)
        {
            SetterAccessModifier = accessModifier;
            return this;
        }

        /// <summary>
        /// Adds a custom attribute to the generated property signature.
        /// </summary>
        /// <param name="attribute">Attribute descriptor.</param>
        /// <returns>The current builder instance.</returns>
        public BeeInterfacePropertyBuilder AddAttribute(BeeAttribute attribute)
        {
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));

            _attributes.Add(attribute);
            return this;
        }

        /// <summary>
        /// Adds a custom attribute to the generated property signature using fluent configuration.
        /// </summary>
        /// <typeparam name="TAttribute">Attribute type.</typeparam>
        /// <param name="configure">Attribute configuration callback.</param>
        /// <returns>The current builder instance.</returns>
        public BeeInterfacePropertyBuilder AddAttribute<TAttribute>(Action<BeeAttributeBuilder> configure)
            where TAttribute : Attribute
        {
            var builder = new BeeAttributeBuilder(typeof(TAttribute));
            configure?.Invoke(builder);
            return AddAttribute(builder.Build());
        }

        internal InterfacePropertyConfigurator ToConfigurator()
            => new(Name, Type, HasGetter, HasSetter, GetterAccessModifier, SetterAccessModifier, _attributes);
    }
}
