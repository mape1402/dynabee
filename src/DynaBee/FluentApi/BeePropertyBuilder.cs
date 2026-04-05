namespace DynaBee.FluentApi
{
    using DynaBee.Infrastructure;
    using DynaBee.Infrastructure.Configurators;

    /// <summary>
    /// Fluent builder for a dynamic property.
    /// </summary>
    public sealed class BeePropertyBuilder
    {
        private readonly List<BeeAttribute> _attributes = new();

        internal BeePropertyBuilder(string name, BeeType type)
        {
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException(nameof(name)) : name;
            Type = type;
        }

        /// <summary>
        /// Property name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Property type.
        /// </summary>
        public BeeType Type { get; }

        /// <summary>
        /// Indicates whether property has getter.
        /// </summary>
        public bool HasGetter { get; private set; } = true;

        /// <summary>
        /// Indicates whether property has setter.
        /// </summary>
        public bool HasSetter { get; private set; } = true;

        /// <summary>
        /// Backing field access modifier.
        /// </summary>
        public FieldAccessModifier BackingFieldAccessModifier { get; private set; } = FieldAccessModifier.Private;

        /// <summary>
        /// Getter access modifier.
        /// </summary>
        public MethodAccessModifier GetterAccessModifier { get; private set; } = MethodAccessModifier.Public;

        /// <summary>
        /// Setter access modifier.
        /// </summary>
        public MethodAccessModifier SetterAccessModifier { get; private set; } = MethodAccessModifier.Public;

        /// <summary>
        /// Sets the property as read-only.
        /// </summary>
        public BeePropertyBuilder AsReadOnly()
        {
            HasGetter = true;
            HasSetter = false;
            return this;
        }

        /// <summary>
        /// Sets the property as write-only.
        /// </summary>
        public BeePropertyBuilder AsWriteOnly()
        {
            HasGetter = false;
            HasSetter = true;
            return this;
        }

        /// <summary>
        /// Enables/disables getter.
        /// </summary>
        public BeePropertyBuilder WithGetter(bool enabled = true)
        {
            HasGetter = enabled;
            return this;
        }

        /// <summary>
        /// Enables/disables setter.
        /// </summary>
        public BeePropertyBuilder WithSetter(bool enabled = true)
        {
            HasSetter = enabled;
            return this;
        }

        /// <summary>
        /// Sets backing field access.
        /// </summary>
        public BeePropertyBuilder WithBackingFieldAccess(FieldAccessModifier accessModifier)
        {
            BackingFieldAccessModifier = accessModifier;
            return this;
        }

        /// <summary>
        /// Sets getter access.
        /// </summary>
        public BeePropertyBuilder WithGetterAccess(MethodAccessModifier accessModifier)
        {
            GetterAccessModifier = accessModifier;
            return this;
        }

        /// <summary>
        /// Sets setter access.
        /// </summary>
        public BeePropertyBuilder WithSetterAccess(MethodAccessModifier accessModifier)
        {
            SetterAccessModifier = accessModifier;
            return this;
        }

        /// <summary>
        /// Adds a custom attribute to the generated property.
        /// </summary>
        public BeePropertyBuilder AddAttribute(BeeAttribute attribute)
        {
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));

            _attributes.Add(attribute);
            return this;
        }

        /// <summary>
        /// Adds a custom attribute to the generated property.
        /// </summary>
        public BeePropertyBuilder AddAttribute<TAttribute>(params object[] constructorArguments)
            where TAttribute : Attribute
            => AddAttribute(BeeAttribute.Of<TAttribute>(constructorArguments));

        /// <summary>
        /// Adds a custom attribute using fluent configuration.
        /// </summary>
        public BeePropertyBuilder AddAttribute<TAttribute>(Action<BeeAttributeBuilder> configure)
            where TAttribute : Attribute
        {
            var builder = new BeeAttributeBuilder(typeof(TAttribute));
            configure?.Invoke(builder);
            return AddAttribute(builder.Build());
        }

        internal PropertyConfigurator ToConfigurator()
            => new(
                Name,
                Type,
                HasGetter,
                HasSetter,
                BackingFieldAccessModifier,
                GetterAccessModifier,
                SetterAccessModifier,
                _attributes);
    }
}