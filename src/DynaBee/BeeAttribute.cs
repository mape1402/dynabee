namespace DynaBee
{
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>
    /// Represents a custom attribute declaration for dynamic members.
    /// </summary>
    public sealed class BeeAttribute
    {
        private readonly Type _attributeType;
        private readonly object[] _constructorArguments;
        private readonly Dictionary<string, object> _namedProperties = new(StringComparer.Ordinal);
        private readonly Dictionary<string, object> _namedFields = new(StringComparer.Ordinal);

        private BeeAttribute(Type attributeType, object[] constructorArguments)
        {
            _attributeType = attributeType ?? throw new ArgumentNullException(nameof(attributeType));
            if (!typeof(Attribute).IsAssignableFrom(_attributeType))
                throw new ArgumentException("Attribute type must inherit from System.Attribute.", nameof(attributeType));

            _constructorArguments = constructorArguments ?? Array.Empty<object>();
        }

        /// <summary>
        /// Creates a custom attribute declaration.
        /// </summary>
        public static BeeAttribute Of<TAttribute>(params object[] constructorArguments)
            where TAttribute : Attribute
            => new(typeof(TAttribute), constructorArguments);

        /// <summary>
        /// Creates a custom attribute declaration.
        /// </summary>
        public static BeeAttribute Of(Type attributeType, params object[] constructorArguments)
            => new(attributeType, constructorArguments);

        /// <summary>
        /// Adds a named property assignment for the attribute.
        /// </summary>
        public BeeAttribute WithProperty(string propertyName, object value)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException(nameof(propertyName));

            _namedProperties[propertyName] = value;
            return this;
        }

        /// <summary>
        /// Adds a named field assignment for the attribute.
        /// </summary>
        public BeeAttribute WithField(string fieldName, object value)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentException(nameof(fieldName));

            _namedFields[fieldName] = value;
            return this;
        }

        internal CustomAttributeBuilder Build()
        {
            var constructor = ResolveConstructor();

            var propertyInfos = new List<PropertyInfo>();
            var propertyValues = new List<object>();
            foreach (var pair in _namedProperties)
            {
                var property = _attributeType.GetProperty(pair.Key, BindingFlags.Instance | BindingFlags.Public)
                    ?? throw new InvalidOperationException($"Property '{pair.Key}' was not found in attribute '{_attributeType.FullName}'.");

                propertyInfos.Add(property);
                propertyValues.Add(pair.Value);
            }

            var fieldInfos = new List<FieldInfo>();
            var fieldValues = new List<object>();
            foreach (var pair in _namedFields)
            {
                var field = _attributeType.GetField(pair.Key, BindingFlags.Instance | BindingFlags.Public)
                    ?? throw new InvalidOperationException($"Field '{pair.Key}' was not found in attribute '{_attributeType.FullName}'.");

                fieldInfos.Add(field);
                fieldValues.Add(pair.Value);
            }

            return new CustomAttributeBuilder(
                constructor,
                _constructorArguments,
                propertyInfos.ToArray(),
                propertyValues.ToArray(),
                fieldInfos.ToArray(),
                fieldValues.ToArray());
        }

        private ConstructorInfo ResolveConstructor()
        {
            var constructors = _attributeType
                .GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                .Where(c => c.GetParameters().Length == _constructorArguments.Length)
                .ToArray();

            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();
                var isMatch = true;

                for (var i = 0; i < parameters.Length; i++)
                {
                    var argument = _constructorArguments[i];
                    var parameterType = parameters[i].ParameterType;

                    if (argument == null)
                    {
                        if (parameterType.IsValueType && Nullable.GetUnderlyingType(parameterType) == null)
                        {
                            isMatch = false;
                            break;
                        }

                        continue;
                    }

                    if (!parameterType.IsAssignableFrom(argument.GetType()))
                    {
                        isMatch = false;
                        break;
                    }
                }

                if (isMatch)
                    return constructor;
            }

            throw new InvalidOperationException(
                $"No matching constructor was found for attribute '{_attributeType.FullName}' with {_constructorArguments.Length} argument(s).");
        }
    }
}