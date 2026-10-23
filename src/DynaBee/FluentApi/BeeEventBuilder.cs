namespace DynaBee.FluentApi
{
    using DynaBee.Infrastructure;
    using DynaBee.Infrastructure.Configurators;

    /// <summary>
    /// Fluent builder for a generated event.
    /// </summary>
    public sealed class BeeEventBuilder
    {
        private readonly List<BeeAttribute> _attributes = new();
        private readonly Dictionary<string, object> _metadata = new();
        private MethodAccessModifier _accessModifier;

        internal BeeEventBuilder(string name, BeeType handlerType)
        {
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException(nameof(name)) : name;
            HandlerType = handlerType;
        }

        /// <summary>
        /// Gets the event name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the event handler type.
        /// </summary>
        public BeeType HandlerType { get; }

        /// <summary>
        /// Sets the add and remove accessor visibility.
        /// </summary>
        /// <param name="accessModifier">Accessor visibility.</param>
        /// <returns>The same event builder.</returns>
        public BeeEventBuilder WithAccess(MethodAccessModifier accessModifier)
        {
            _accessModifier = accessModifier;
            return this;
        }

        /// <summary>
        /// Adds an event attribute.
        /// </summary>
        /// <param name="attribute">Attribute descriptor.</param>
        /// <returns>The same event builder.</returns>
        public BeeEventBuilder AddAttribute(BeeAttribute attribute)
        {
            _attributes.Add(attribute ?? throw new ArgumentNullException(nameof(attribute)));
            return this;
        }

        /// <summary>
        /// Adds an event attribute.
        /// </summary>
        /// <typeparam name="TAttribute">Attribute type.</typeparam>
        /// <param name="constructorArguments">Attribute constructor arguments.</param>
        /// <returns>The same event builder.</returns>
        public BeeEventBuilder AddAttribute<TAttribute>(params object[] constructorArguments)
            where TAttribute : Attribute
            => AddAttribute(BeeAttribute.Of<TAttribute>(constructorArguments));

        /// <summary>
        /// Stores event metadata.
        /// </summary>
        /// <param name="key">Metadata key.</param>
        /// <param name="value">Metadata value.</param>
        /// <returns>The same event builder.</returns>
        public BeeEventBuilder WithMetadata(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));

            _metadata[key] = value ?? throw new ArgumentNullException(nameof(value));
            return this;
        }

        internal EventConfigurator ToConfigurator()
            => new(Name, HandlerType, _accessModifier, _attributes, _metadata);
    }
}
