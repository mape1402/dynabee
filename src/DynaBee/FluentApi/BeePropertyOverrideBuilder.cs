namespace DynaBee.FluentApi
{
    using DynaBee.Infrastructure.Configurators;
    using System.Reflection;

    /// <summary>
    /// Fluent builder for overriding a virtual or abstract property from a base class.
    /// </summary>
    public sealed class BeePropertyOverrideBuilder
    {
        private readonly PropertyInfo _baseProperty;
        private readonly Dictionary<string, object> _metadata = new();
        private readonly List<BeeAttribute> _attributes = new();
        private BeePropertyAccessorBuilder _getter;
        private BeePropertyAccessorBuilder _setter;

        internal BeePropertyOverrideBuilder(PropertyInfo baseProperty)
        {
            _baseProperty = baseProperty ?? throw new ArgumentNullException(nameof(baseProperty));
        }

        /// <summary>
        /// Configures the overriding getter.
        /// </summary>
        /// <param name="configure">Getter configuration callback.</param>
        /// <returns>The same property override builder.</returns>
        public BeePropertyOverrideBuilder Getter(Action<BeePropertyAccessorBuilder> configure)
        {
            var builder = new BeePropertyAccessorBuilder();
            configure?.Invoke(builder);
            _getter = builder;
            return this;
        }

        /// <summary>
        /// Configures the overriding setter.
        /// </summary>
        /// <param name="configure">Setter configuration callback.</param>
        /// <returns>The same property override builder.</returns>
        public BeePropertyOverrideBuilder Setter(Action<BeePropertyAccessorBuilder> configure = null)
        {
            var builder = new BeePropertyAccessorBuilder();
            configure?.Invoke(builder);
            _setter = builder;
            return this;
        }

        /// <summary>
        /// Adds a custom attribute to the generated overriding property.
        /// </summary>
        /// <param name="attribute">Attribute descriptor.</param>
        /// <returns>The same property override builder.</returns>
        public BeePropertyOverrideBuilder AddAttribute(BeeAttribute attribute)
        {
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));

            _attributes.Add(attribute);
            return this;
        }

        /// <summary>
        /// Stores metadata for this generated overriding property.
        /// </summary>
        /// <param name="key">Metadata key.</param>
        /// <param name="value">Metadata value.</param>
        /// <returns>The same property override builder.</returns>
        public BeePropertyOverrideBuilder WithMetadata(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));

            _metadata[key] = value ?? throw new ArgumentNullException(nameof(value));
            return this;
        }

        /// <summary>
        /// Stores strongly typed metadata for this generated overriding property.
        /// </summary>
        /// <typeparam name="T">Metadata value type.</typeparam>
        /// <param name="key">Typed metadata key.</param>
        /// <param name="value">Metadata value.</param>
        /// <returns>The same property override builder.</returns>
        public BeePropertyOverrideBuilder WithMetadata<T>(BeeMetadataKey<T> key, T value)
            => WithMetadata(key.Name, value);

        internal PropertyOverrideConfigurator ToConfigurator()
            => new(_baseProperty, _getter, _setter, _attributes, _metadata);
    }
}
