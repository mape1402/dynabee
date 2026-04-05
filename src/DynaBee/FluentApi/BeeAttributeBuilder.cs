namespace DynaBee.FluentApi
{
    /// <summary>
    /// Fluent builder for custom attributes.
    /// </summary>
    public sealed class BeeAttributeBuilder
    {
        private readonly Type _attributeType;
        private readonly List<object> _constructorArguments = new();
        private readonly Dictionary<string, object> _properties = new(StringComparer.Ordinal);
        private readonly Dictionary<string, object> _fields = new(StringComparer.Ordinal);

        internal BeeAttributeBuilder(Type attributeType)
        {
            _attributeType = attributeType ?? throw new ArgumentNullException(nameof(attributeType));
            if (!typeof(Attribute).IsAssignableFrom(_attributeType))
                throw new ArgumentException("Attribute type must inherit from System.Attribute.", nameof(attributeType));
        }

        /// <summary>
        /// Adds one constructor argument.
        /// </summary>
        public BeeAttributeBuilder WithConstructorArgument(object argument)
        {
            _constructorArguments.Add(argument);
            return this;
        }

        /// <summary>
        /// Adds constructor arguments.
        /// </summary>
        public BeeAttributeBuilder WithConstructorArguments(params object[] arguments)
        {
            if (arguments == null)
                throw new ArgumentNullException(nameof(arguments));

            _constructorArguments.AddRange(arguments);
            return this;
        }

        /// <summary>
        /// Sets a named property value.
        /// </summary>
        public BeeAttributeBuilder WithProperty(string propertyName, object value)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException(nameof(propertyName));

            _properties[propertyName] = value;
            return this;
        }

        /// <summary>
        /// Sets a named field value.
        /// </summary>
        public BeeAttributeBuilder WithField(string fieldName, object value)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentException(nameof(fieldName));

            _fields[fieldName] = value;
            return this;
        }

        internal BeeAttribute Build()
        {
            var attribute = BeeAttribute.Of(_attributeType, _constructorArguments.ToArray());

            foreach (var property in _properties)
                attribute.WithProperty(property.Key, property.Value);

            foreach (var field in _fields)
                attribute.WithField(field.Key, field.Value);

            return attribute;
        }
    }
}