namespace DynaBee.FluentApi
{
    using DynaBee.Infrastructure;
    using DynaBee.Infrastructure.Configurators;

    public sealed class BeeInterfacePropertyBuilder
    {
        private readonly List<BeeAttribute> _attributes = new();

        internal BeeInterfacePropertyBuilder(string name, BeeType type)
        {
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException(nameof(name)) : name;
            Type = type;
        }

        public string Name { get; }

        public BeeType Type { get; }

        public bool HasGetter { get; private set; } = true;

        public bool HasSetter { get; private set; } = true;

        public MethodAccessModifier GetterAccessModifier { get; private set; } = MethodAccessModifier.Public;

        public MethodAccessModifier SetterAccessModifier { get; private set; } = MethodAccessModifier.Public;

        public BeeInterfacePropertyBuilder AsReadOnly()
        {
            HasGetter = true;
            HasSetter = false;
            return this;
        }

        public BeeInterfacePropertyBuilder AsWriteOnly()
        {
            HasGetter = false;
            HasSetter = true;
            return this;
        }

        public BeeInterfacePropertyBuilder WithGetter(bool enabled = true)
        {
            HasGetter = enabled;
            return this;
        }

        public BeeInterfacePropertyBuilder WithSetter(bool enabled = true)
        {
            HasSetter = enabled;
            return this;
        }

        public BeeInterfacePropertyBuilder WithGetterAccess(MethodAccessModifier accessModifier)
        {
            GetterAccessModifier = accessModifier;
            return this;
        }

        public BeeInterfacePropertyBuilder WithSetterAccess(MethodAccessModifier accessModifier)
        {
            SetterAccessModifier = accessModifier;
            return this;
        }

        public BeeInterfacePropertyBuilder AddAttribute(BeeAttribute attribute)
        {
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));

            _attributes.Add(attribute);
            return this;
        }

        public BeeInterfacePropertyBuilder AddAttribute<TAttribute>(Action<BeeAttributeBuilder> configure)
            where TAttribute : Attribute
        {
            var builder = new BeeAttributeBuilder(typeof(TAttribute));
            configure?.Invoke(builder);
            return AddAttribute(builder.Build());
        }

        internal InterfacePropertyConfigurator ToConfigurator()
            => new(Name, Type, HasGetter, HasSetter, GetterAccessModifier, SetterAccessModifier, _attributes);
    }
}