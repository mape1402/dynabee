namespace DynaBee.FluentApi
{
    using DynaBee.Infrastructure;
    using DynaBee.Infrastructure.Configurators;

    public sealed class BeeInterfaceMethodBuilder
    {
        private readonly List<(string Name, BeeType Type)> _parameters = new();
        private readonly List<BeeAttribute> _attributes = new();
        private MethodAccessModifier _accessModifier;

        internal BeeInterfaceMethodBuilder(string name, BeeType returnType)
        {
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException(nameof(name)) : name;
            ReturnType = returnType;
        }

        public string Name { get; }

        public BeeType ReturnType { get; }

        public BeeInterfaceMethodBuilder WithParameter(string name, BeeType type)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));

            _parameters.Add((name, type));
            return this;
        }

        public BeeInterfaceMethodBuilder WithParameter<T>(string name)
            => WithParameter(name, typeof(T));

        public BeeInterfaceMethodBuilder WithAccess(MethodAccessModifier accessModifier)
        {
            _accessModifier = accessModifier;
            return this;
        }

        public BeeInterfaceMethodBuilder AddAttribute(BeeAttribute attribute)
        {
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));

            _attributes.Add(attribute);
            return this;
        }

        public BeeInterfaceMethodBuilder AddAttribute<TAttribute>(Action<BeeAttributeBuilder> configure)
            where TAttribute : Attribute
        {
            var builder = new BeeAttributeBuilder(typeof(TAttribute));
            configure?.Invoke(builder);
            return AddAttribute(builder.Build());
        }

        internal InterfaceMethodConfigurator ToConfigurator()
            => new(Name, ReturnType, _parameters, _accessModifier, _attributes);
    }
}