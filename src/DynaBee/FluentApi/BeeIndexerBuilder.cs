namespace DynaBee.FluentApi
{
    using DynaBee.Infrastructure;
    using DynaBee.Infrastructure.Configurators;

    /// <summary>
    /// Fluent builder for a generated dictionary-backed indexer.
    /// </summary>
    public sealed class BeeIndexerBuilder
    {
        private readonly List<BeeAttribute> _attributes = new();
        private readonly Dictionary<string, object> _metadata = new();
        private bool _hasGetter = true;
        private bool _hasSetter = true;
        private MethodAccessModifier _getterAccessModifier;
        private MethodAccessModifier _setterAccessModifier;

        internal BeeIndexerBuilder(string name, BeeType indexType, BeeType valueType)
        {
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException(nameof(name)) : name;
            IndexType = indexType;
            ValueType = valueType;
        }

        /// <summary>Gets the CLR property name for this indexer.</summary>
        public string Name { get; }

        /// <summary>Gets the index parameter type.</summary>
        public BeeType IndexType { get; }

        /// <summary>Gets the indexed value type.</summary>
        public BeeType ValueType { get; }

        /// <summary>Enables or disables the getter.</summary>
        public BeeIndexerBuilder WithGetter(bool enabled = true)
        {
            _hasGetter = enabled;
            ValidateAccessors();
            return this;
        }

        /// <summary>Enables or disables the setter.</summary>
        public BeeIndexerBuilder WithSetter(bool enabled = true)
        {
            _hasSetter = enabled;
            ValidateAccessors();
            return this;
        }

        /// <summary>Sets getter visibility.</summary>
        public BeeIndexerBuilder WithGetterAccess(MethodAccessModifier accessModifier)
        {
            _getterAccessModifier = accessModifier;
            return this;
        }

        /// <summary>Sets setter visibility.</summary>
        public BeeIndexerBuilder WithSetterAccess(MethodAccessModifier accessModifier)
        {
            _setterAccessModifier = accessModifier;
            return this;
        }

        /// <summary>Adds an indexer attribute.</summary>
        public BeeIndexerBuilder AddAttribute(BeeAttribute attribute)
        {
            _attributes.Add(attribute ?? throw new ArgumentNullException(nameof(attribute)));
            return this;
        }

        /// <summary>Stores indexer metadata.</summary>
        public BeeIndexerBuilder WithMetadata(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));

            _metadata[key] = value ?? throw new ArgumentNullException(nameof(value));
            return this;
        }

        internal IndexerConfigurator ToConfigurator()
            => new(Name, IndexType, ValueType, _hasGetter, _hasSetter, _getterAccessModifier, _setterAccessModifier, _attributes, _metadata);

        private void ValidateAccessors()
        {
            if (!_hasGetter && !_hasSetter)
                throw new InvalidOperationException("An indexer must define at least a getter or a setter.");
        }
    }
}
