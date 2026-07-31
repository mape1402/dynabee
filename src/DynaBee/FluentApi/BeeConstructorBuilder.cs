namespace DynaBee.FluentApi
{
    using DynaBee.Infrastructure;
    using System.Reflection.Emit;
    using DynaBee.Infrastructure.Configurators;
    using System.Reflection;

    /// <summary>
    /// Fluent builder for a dynamic constructor.
    /// </summary>
    public sealed class BeeConstructorBuilder
    {
        private readonly List<(string Name, BeeType Type)> _parameters = new();
        private readonly Dictionary<string, object> _metadata = new();
        private Action<ILGenerator> _body;
        private ConstructorInfo _baseConstructor;
        private IReadOnlyList<string> _baseConstructorArgumentNames;

        /// <summary>
        /// Adds one constructor parameter.
        /// </summary>
        public BeeConstructorBuilder WithParameter(string name, BeeType parameterType)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));

            _parameters.Add((name, parameterType));
            return this;
        }

        /// <summary>
        /// Adds one constructor parameter.
        /// </summary>
        public BeeConstructorBuilder WithParameter<TParameter>(string name)
            => WithParameter(name, typeof(TParameter));

        /// <summary>
        /// Defines custom constructor body IL. The callback must emit a ret opcode.
        /// </summary>
        public BeeConstructorBuilder Emits(Action<ILGenerator> body)
        {
            _body = body ?? throw new ArgumentNullException(nameof(body));
            _baseConstructor = null;
            _baseConstructorArgumentNames = null;
            return this;
        }

        /// <summary>
        /// Emits a constructor body that forwards selected generated constructor parameters to a base constructor.
        /// </summary>
        /// <param name="baseConstructor">Base constructor to call.</param>
        /// <param name="configureArguments">Argument selector using generated constructor parameter names.</param>
        /// <returns>The same constructor builder.</returns>
        public BeeConstructorBuilder CallsBase(ConstructorInfo baseConstructor, Action<BeeConstructorBaseCallBuilder> configureArguments)
        {
            if (baseConstructor == null)
                throw new ArgumentNullException(nameof(baseConstructor));

            var argumentsBuilder = new BeeConstructorBaseCallBuilder();
            configureArguments?.Invoke(argumentsBuilder);
            return CallsBase(baseConstructor, argumentsBuilder.ArgumentNames);
        }

        /// <summary>
        /// Emits a constructor body that forwards selected generated constructor parameters to a base constructor.
        /// </summary>
        /// <param name="baseConstructor">Base constructor to call.</param>
        /// <param name="argumentNames">Generated constructor parameter names to pass to the base constructor.</param>
        /// <returns>The same constructor builder.</returns>
        public BeeConstructorBuilder CallsBase(ConstructorInfo baseConstructor, params string[] argumentNames)
            => CallsBase(baseConstructor, (IReadOnlyList<string>)argumentNames);

        private BeeConstructorBuilder CallsBase(ConstructorInfo baseConstructor, IReadOnlyList<string> argumentNames)
        {
            if (baseConstructor == null)
                throw new ArgumentNullException(nameof(baseConstructor));

            if (argumentNames == null)
                throw new ArgumentNullException(nameof(argumentNames));

            _baseConstructor = baseConstructor;
            _baseConstructorArgumentNames = argumentNames.ToArray();
            _body = null;
            return this;
        }

        /// <summary>
        /// Stores metadata for this generated constructor.
        /// </summary>
        public BeeConstructorBuilder WithMetadata(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));

            _metadata[key] = value ?? throw new ArgumentNullException(nameof(value));
            return this;
        }

        /// <summary>
        /// Stores strongly typed metadata for this generated constructor.
        /// </summary>
        public BeeConstructorBuilder WithMetadata<T>(BeeMetadataKey<T> key, T value)
            => WithMetadata(key.Name, value);

        internal ConstructorConfigurator ToConfigurator()
            => new ConstructorConfigurator(_parameters, _body, _metadata, _baseConstructor, _baseConstructorArgumentNames);
    }
}
