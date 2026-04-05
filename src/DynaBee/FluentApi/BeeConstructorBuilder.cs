namespace DynaBee.FluentApi
{
    using DynaBee.Infrastructure;
    using System.Reflection.Emit;
    using DynaBee.Infrastructure.Configurators;

    /// <summary>
    /// Fluent builder for a dynamic constructor.
    /// </summary>
    public sealed class BeeConstructorBuilder
    {
        private readonly List<(string Name, BeeType Type)> _parameters = new();
        private Action<ILGenerator> _body;

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
            return this;
        }

        internal ConstructorConfigurator ToConfigurator()
            => new ConstructorConfigurator(_parameters, _body);
    }
}
