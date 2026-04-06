namespace DynaBee.FluentApi
{
    using DynaBee.Infrastructure;
    using DynaBee.Infrastructure.Configurators;

    /// <summary>
    /// Fluent builder for dynamic interfaces.
    /// </summary>
    public sealed class BeeInterfaceBuilder
    {
        private readonly InterfaceConfigurator _configurator;

        internal BeeInterfaceBuilder(InterfaceConfigurator configurator)
        {
            _configurator = configurator ?? throw new ArgumentNullException(nameof(configurator));
        }

        /// <summary>
        /// Adds a base interface to the generated interface.
        /// </summary>
        /// <param name="interfaceType">Base interface type.</param>
        /// <returns>The current builder instance.</returns>
        public BeeInterfaceBuilder Inherits(Type interfaceType)
        {
            _configurator.Inherits(interfaceType);
            return this;
        }

        /// <summary>
        /// Adds a base interface to the generated interface.
        /// </summary>
        /// <typeparam name="TInterface">Base interface type.</typeparam>
        /// <returns>The current builder instance.</returns>
        public BeeInterfaceBuilder Inherits<TInterface>()
            => Inherits(typeof(TInterface));

        /// <summary>
        /// Adds a custom attribute to the generated interface.
        /// </summary>
        /// <param name="attribute">Attribute descriptor.</param>
        /// <returns>The current builder instance.</returns>
        public BeeInterfaceBuilder AddAttribute(BeeAttribute attribute)
        {
            _configurator.AddAttribute(attribute);
            return this;
        }

        /// <summary>
        /// Adds a custom attribute to the generated interface using fluent configuration.
        /// </summary>
        /// <typeparam name="TAttribute">Attribute type.</typeparam>
        /// <param name="configure">Attribute configuration callback.</param>
        /// <returns>The current builder instance.</returns>
        public BeeInterfaceBuilder AddAttribute<TAttribute>(Action<BeeAttributeBuilder> configure)
            where TAttribute : Attribute
        {
            var builder = new BeeAttributeBuilder(typeof(TAttribute));
            configure?.Invoke(builder);
            return AddAttribute(builder.Build());
        }

        /// <summary>
        /// Adds a method signature to the generated interface.
        /// </summary>
        /// <param name="name">Method name.</param>
        /// <param name="returnType">Method return type.</param>
        /// <param name="configure">Optional method configuration callback.</param>
        /// <returns>The current builder instance.</returns>
        public BeeInterfaceBuilder AddMethod(string name, BeeType returnType, Action<BeeInterfaceMethodBuilder> configure = null)
        {
            var methodBuilder = new BeeInterfaceMethodBuilder(name, returnType);
            configure?.Invoke(methodBuilder);
            _configurator.AddElementBuilder(methodBuilder.ToConfigurator());
            return this;
        }

        /// <summary>
        /// Adds a property signature to the generated interface.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="type">Property type.</param>
        /// <param name="configure">Optional property configuration callback.</param>
        /// <returns>The current builder instance.</returns>
        public BeeInterfaceBuilder AddProperty(string name, BeeType type, Action<BeeInterfacePropertyBuilder> configure = null)
        {
            var propertyBuilder = new BeeInterfacePropertyBuilder(name, type);
            configure?.Invoke(propertyBuilder);
            _configurator.AddElementBuilder(propertyBuilder.ToConfigurator());
            return this;
        }

        /// <summary>
        /// Adds a method signature to the generated interface.
        /// </summary>
        /// <typeparam name="T">Method return type.</typeparam>
        /// <param name="name">Method name.</param>
        /// <param name="configure">Optional method configuration callback.</param>
        /// <returns>The current builder instance.</returns>
        public BeeInterfaceBuilder AddMethod<T>(string name, Action<BeeInterfaceMethodBuilder> configure = null)
            => AddMethod(name, typeof(T), configure);

        /// <summary>
        /// Adds a property signature to the generated interface.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="name">Property name.</param>
        /// <param name="configure">Optional property configuration callback.</param>
        /// <returns>The current builder instance.</returns>
        public BeeInterfaceBuilder AddProperty<T>(string name, Action<BeeInterfacePropertyBuilder> configure = null)
            => AddProperty(name, typeof(T), configure);
    }
}
