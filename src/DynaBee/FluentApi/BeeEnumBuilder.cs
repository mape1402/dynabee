namespace DynaBee.FluentApi
{
    using DynaBee.Infrastructure.Configurators;

    /// <summary>
    /// Fluent builder for dynamic enums.
    /// </summary>
    public sealed class BeeEnumBuilder
    {
        private readonly EnumConfigurator _configurator;

        internal BeeEnumBuilder(EnumConfigurator configurator)
        {
            _configurator = configurator ?? throw new ArgumentNullException(nameof(configurator));
        }

        /// <summary>
        /// Adds a named enum literal value.
        /// </summary>
        /// <param name="name">Enum literal name.</param>
        /// <param name="value">Enum literal value.</param>
        /// <returns>The current builder instance.</returns>
        public BeeEnumBuilder AddValue(string name, object value)
        {
            _configurator.AddValue(name, value);
            return this;
        }

        /// <summary>
        /// Adds a custom attribute to the generated enum.
        /// </summary>
        /// <param name="attribute">Attribute descriptor.</param>
        /// <returns>The current builder instance.</returns>
        public BeeEnumBuilder AddAttribute(BeeAttribute attribute)
        {
            _configurator.AddAttribute(attribute);
            return this;
        }

        /// <summary>
        /// Adds a custom attribute to the generated enum using fluent configuration.
        /// </summary>
        /// <typeparam name="TAttribute">Attribute type.</typeparam>
        /// <param name="configure">Attribute configuration callback.</param>
        /// <returns>The current builder instance.</returns>
        public BeeEnumBuilder AddAttribute<TAttribute>(Action<BeeAttributeBuilder> configure)
            where TAttribute : Attribute
        {
            var builder = new BeeAttributeBuilder(typeof(TAttribute));
            configure?.Invoke(builder);
            return AddAttribute(builder.Build());
        }
    }
}
