namespace DynaBee.FluentApi
{
    using DynaBee;
    using DynaBee.Infrastructure;
    using DynaBee.Infrastructure.Configurators;

    /// <summary>
    /// Fluent builder for dynamic structs.
    /// </summary>
    public sealed class BeeStructBuilder
    {
        private readonly StructConfigurator _configurator;

        internal BeeStructBuilder(StructConfigurator configurator)
        {
            _configurator = configurator ?? throw new ArgumentNullException(nameof(configurator));
        }

        public BeeStructBuilder Implements(Type interfaceType)
        {
            _configurator.Implements(interfaceType);
            return this;
        }

        public BeeStructBuilder Implements<TInterface>()
            => Implements(typeof(TInterface));

        public BeeStructBuilder AddAttribute(BeeAttribute attribute)
        {
            _configurator.AddAttribute(attribute);
            return this;
        }

        public BeeStructBuilder AddAttribute<TAttribute>(Action<BeeAttributeBuilder> configure)
            where TAttribute : Attribute
        {
            var builder = new BeeAttributeBuilder(typeof(TAttribute));
            configure?.Invoke(builder);
            return AddAttribute(builder.Build());
        }

        public BeeStructBuilder AddProperty(string name, BeeType type, Action<BeePropertyBuilder> configure = null)
        {
            var propertyBuilder = new BeePropertyBuilder(name, type);
            configure?.Invoke(propertyBuilder);
            _configurator.AddElementBuilder(propertyBuilder.ToConfigurator());
            return this;
        }

        public BeeStructBuilder AddProperty<TProperty>(string name, Action<BeePropertyBuilder> configure = null)
            => AddProperty(name, typeof(TProperty), configure);

        public BeeStructBuilder AddField(string name, BeeType type, FieldAccessModifier accessModifier = default)
        {
            _configurator.AddElementBuilder(new FieldConfigurator(name, type, accessModifier));
            return this;
        }

        public BeeStructBuilder AddField<TField>(string name, FieldAccessModifier accessModifier = default)
            => AddField(name, typeof(TField), accessModifier);

        public BeeStructBuilder AddMethod(string name, BeeType returnType, Action<BeeMethodBuilder> configure = null)
        {
            var methodBuilder = new BeeMethodBuilder(name, returnType);
            configure?.Invoke(methodBuilder);
            _configurator.AddElementBuilder(methodBuilder.ToConfigurator());
            return this;
        }

        public BeeStructBuilder AddVoidMethod(string name, Action<BeeMethodBuilder> configure = null)
            => AddMethod(name, typeof(void), configure);

        internal BeeStructBuilder AddElementConfigurator(IElementConfigurator configurator)
        {
            _configurator.AddElementBuilder(configurator);
            return this;
        }
    }
}
