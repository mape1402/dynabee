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

        public BeeEnumBuilder AddValue(string name, object value)
        {
            _configurator.AddValue(name, value);
            return this;
        }

        public BeeEnumBuilder AddAttribute(BeeAttribute attribute)
        {
            _configurator.AddAttribute(attribute);
            return this;
        }

        public BeeEnumBuilder AddAttribute<TAttribute>(Action<BeeAttributeBuilder> configure)
            where TAttribute : Attribute
        {
            var builder = new BeeAttributeBuilder(typeof(TAttribute));
            configure?.Invoke(builder);
            return AddAttribute(builder.Build());
        }
    }
}