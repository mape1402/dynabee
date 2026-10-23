namespace DynaBee.FluentApi
{
    using DynaBee.Infrastructure;
    using DynaBee.Infrastructure.Configurators;

    /// <summary>
    /// Fluent builder for a generic dynamic class.
    /// </summary>
    public sealed class BeeGenericClassBuilder
    {
        private readonly BeeClassBuilder _classBuilder;
        private readonly ClassConfigurator _configurator;
        private readonly Dictionary<string, BeeGenericTypeParameterBuilder> _parameters = new(StringComparer.Ordinal);

        internal BeeGenericClassBuilder(BeeClassBuilder classBuilder, ClassConfigurator configurator)
        {
            _classBuilder = classBuilder ?? throw new ArgumentNullException(nameof(classBuilder));
            _configurator = configurator ?? throw new ArgumentNullException(nameof(configurator));
        }

        /// <summary>
        /// Declares a generic type parameter.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <param name="configure">Optional constraint configuration.</param>
        /// <returns>The same generic class builder.</returns>
        public BeeGenericClassBuilder WithTypeParameter(string name, Action<BeeGenericTypeParameterBuilder> configure = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));

            if (_parameters.ContainsKey(name))
                throw new InvalidOperationException($"Generic parameter '{name}' is already declared.");

            var parameter = new BeeGenericTypeParameterBuilder(name);
            configure?.Invoke(parameter);
            _parameters.Add(name, parameter);
            _configurator.AddGenericParameter(parameter);
            return this;
        }

        /// <summary>
        /// Gets a fluent type reference for a declared generic parameter.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <returns>A type reference that can be used for generated members.</returns>
        public BeeType TypeParameter(string name)
        {
            if (!_parameters.ContainsKey(name))
                throw new KeyNotFoundException($"Generic parameter '{name}' is not declared.");

            return BeeType.GenericParameter(name);
        }

        /// <summary>
        /// Configures regular class features using the underlying class builder.
        /// </summary>
        /// <param name="configure">Class configuration.</param>
        /// <returns>The same generic class builder.</returns>
        public BeeGenericClassBuilder Configure(Action<BeeClassBuilder> configure)
        {
            configure?.Invoke(_classBuilder);
            return this;
        }

        /// <summary>
        /// Adds a property to the generated generic class.
        /// </summary>
        public BeeGenericClassBuilder AddProperty(string name, BeeType type, Action<BeePropertyBuilder> configure = null)
        {
            _classBuilder.AddProperty(name, type, configure);
            return this;
        }

        /// <summary>
        /// Adds a field to the generated generic class.
        /// </summary>
        public BeeGenericClassBuilder AddField(string name, BeeType type, FieldAccessModifier accessModifier = default)
        {
            _classBuilder.AddField(name, type, accessModifier);
            return this;
        }

        /// <summary>
        /// Adds a method to the generated generic class.
        /// </summary>
        public BeeGenericClassBuilder AddMethod(string name, BeeType returnType, Action<BeeMethodBuilder> configure = null)
        {
            _classBuilder.AddMethod(name, returnType, configure);
            return this;
        }

        /// <summary>
        /// Adds a constructor to the generated generic class.
        /// </summary>
        public BeeGenericClassBuilder AddConstructor(Action<BeeConstructorBuilder> configure = null)
        {
            _classBuilder.AddConstructor(configure);
            return this;
        }
    }
}
