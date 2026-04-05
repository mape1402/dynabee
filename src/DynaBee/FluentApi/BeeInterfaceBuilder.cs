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

        public BeeInterfaceBuilder Inherits(Type interfaceType)
        {
            _configurator.Inherits(interfaceType);
            return this;
        }

        public BeeInterfaceBuilder Inherits<TInterface>()
            => Inherits(typeof(TInterface));

        public BeeInterfaceBuilder AddAttribute(BeeAttribute attribute)
        {
            _configurator.AddAttribute(attribute);
            return this;
        }

        public BeeInterfaceBuilder AddAttribute<TAttribute>(Action<BeeAttributeBuilder> configure)
            where TAttribute : Attribute
        {
            var builder = new BeeAttributeBuilder(typeof(TAttribute));
            configure?.Invoke(builder);
            return AddAttribute(builder.Build());
        }

        public BeeInterfaceBuilder AddMethod(string name, BeeType returnType, Action<BeeInterfaceMethodBuilder> configure = null)
        {
            var methodBuilder = new BeeInterfaceMethodBuilder(name, returnType);
            configure?.Invoke(methodBuilder);
            _configurator.AddElementBuilder(methodBuilder.ToConfigurator());
            return this;
        }

        public BeeInterfaceBuilder AddProperty(string name, BeeType type, Action<BeeInterfacePropertyBuilder> configure = null)
        {
            var propertyBuilder = new BeeInterfacePropertyBuilder(name, type);
            configure?.Invoke(propertyBuilder);
            _configurator.AddElementBuilder(propertyBuilder.ToConfigurator());
            return this;
        }

        public BeeInterfaceBuilder AddMethod<T>(string name, Action<BeeInterfaceMethodBuilder> configure = null)
            => AddMethod(name, typeof(T), configure);

        public BeeInterfaceBuilder AddProperty<T>(string name, Action<BeeInterfacePropertyBuilder> configure = null)
            => AddProperty(name, typeof(T), configure);
    }
}