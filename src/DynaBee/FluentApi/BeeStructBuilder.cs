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

        /// <summary>
        /// Adds an interface implementation to the generated struct.
        /// </summary>
        /// <param name="interfaceType">Interface type.</param>
        /// <returns>The current builder instance.</returns>
        public BeeStructBuilder Implements(Type interfaceType)
        {
            _configurator.Implements(interfaceType);
            return this;
        }

        /// <summary>
        /// Adds an interface implementation to the generated struct.
        /// </summary>
        /// <typeparam name="TInterface">Interface type.</typeparam>
        /// <returns>The current builder instance.</returns>
        public BeeStructBuilder Implements<TInterface>()
            => Implements(typeof(TInterface));

        /// <summary>
        /// Adds a custom attribute to the generated struct.
        /// </summary>
        /// <param name="attribute">Attribute descriptor.</param>
        /// <returns>The current builder instance.</returns>
        public BeeStructBuilder AddAttribute(BeeAttribute attribute)
        {
            _configurator.AddAttribute(attribute);
            return this;
        }

        /// <summary>
        /// Adds a custom attribute to the generated struct using fluent configuration.
        /// </summary>
        /// <typeparam name="TAttribute">Attribute type.</typeparam>
        /// <param name="configure">Attribute configuration callback.</param>
        /// <returns>The current builder instance.</returns>
        public BeeStructBuilder AddAttribute<TAttribute>(Action<BeeAttributeBuilder> configure)
            where TAttribute : Attribute
        {
            var builder = new BeeAttributeBuilder(typeof(TAttribute));
            configure?.Invoke(builder);
            return AddAttribute(builder.Build());
        }

        /// <summary>
        /// Adds a property to the generated struct.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="type">Property type.</param>
        /// <param name="configure">Optional property configuration callback.</param>
        /// <returns>The current builder instance.</returns>
        public BeeStructBuilder AddProperty(string name, BeeType type, Action<BeePropertyBuilder> configure = null)
        {
            var propertyBuilder = new BeePropertyBuilder(name, type);
            configure?.Invoke(propertyBuilder);
            _configurator.AddElementBuilder(propertyBuilder.ToConfigurator());
            return this;
        }

        /// <summary>
        /// Adds a property to the generated struct.
        /// </summary>
        /// <typeparam name="TProperty">Property type.</typeparam>
        /// <param name="name">Property name.</param>
        /// <param name="configure">Optional property configuration callback.</param>
        /// <returns>The current builder instance.</returns>
        public BeeStructBuilder AddProperty<TProperty>(string name, Action<BeePropertyBuilder> configure = null)
            => AddProperty(name, typeof(TProperty), configure);

        /// <summary>
        /// Adds a field to the generated struct.
        /// </summary>
        /// <param name="name">Field name.</param>
        /// <param name="type">Field type.</param>
        /// <param name="accessModifier">Field access modifier.</param>
        /// <returns>The current builder instance.</returns>
        public BeeStructBuilder AddField(string name, BeeType type, FieldAccessModifier accessModifier = default)
        {
            _configurator.AddElementBuilder(new FieldConfigurator(name, type, accessModifier));
            return this;
        }

        /// <summary>
        /// Adds a field to the generated struct.
        /// </summary>
        /// <typeparam name="TField">Field type.</typeparam>
        /// <param name="name">Field name.</param>
        /// <param name="accessModifier">Field access modifier.</param>
        /// <returns>The current builder instance.</returns>
        public BeeStructBuilder AddField<TField>(string name, FieldAccessModifier accessModifier = default)
            => AddField(name, typeof(TField), accessModifier);

        /// <summary>
        /// Adds a method to the generated struct.
        /// </summary>
        /// <param name="name">Method name.</param>
        /// <param name="returnType">Method return type.</param>
        /// <param name="configure">Optional method configuration callback.</param>
        /// <returns>The current builder instance.</returns>
        public BeeStructBuilder AddMethod(string name, BeeType returnType, Action<BeeMethodBuilder> configure = null)
        {
            var methodBuilder = new BeeMethodBuilder(name, returnType);
            configure?.Invoke(methodBuilder);
            _configurator.AddElementBuilder(methodBuilder.ToConfigurator());
            return this;
        }

        /// <summary>
        /// Adds a void method to the generated struct.
        /// </summary>
        /// <param name="name">Method name.</param>
        /// <param name="configure">Optional method configuration callback.</param>
        /// <returns>The current builder instance.</returns>
        public BeeStructBuilder AddVoidMethod(string name, Action<BeeMethodBuilder> configure = null)
            => AddMethod(name, typeof(void), configure);

        internal BeeStructBuilder AddElementConfigurator(IElementConfigurator configurator)
        {
            _configurator.AddElementBuilder(configurator);
            return this;
        }
    }
}
