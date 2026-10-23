namespace DynaBee.FluentApi
{
    using System.Reflection;

    /// <summary>
    /// Configures how a generated class fulfills a reflected interface contract.
    /// </summary>
    public sealed class BeeInterfaceContractBuilder
    {
        private readonly BeeClassBuilder _owner;
        private readonly Type _interfaceType;
        private readonly bool _registerInDi;
        private readonly Dictionary<MethodInfo, Action<BeeMethodBuilder>> _methods = new();
        private readonly Dictionary<PropertyInfo, Action<BeePropertyBuilder>> _properties = new();
        private bool _requireExplicitImplementations;

        internal BeeInterfaceContractBuilder(BeeClassBuilder owner, Type interfaceType, bool registerInDi)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _interfaceType = interfaceType ?? throw new ArgumentNullException(nameof(interfaceType));
            _registerInDi = registerInDi;
        }

        /// <summary>
        /// Requires every method and property in the contract to be configured explicitly.
        /// </summary>
        /// <returns>The same contract builder.</returns>
        public BeeInterfaceContractBuilder RequireExplicitImplementations()
        {
            _requireExplicitImplementations = true;
            return this;
        }

        /// <summary>
        /// Configures a contract method that has a unique name.
        /// </summary>
        /// <param name="name">Method name.</param>
        /// <param name="configure">Method configuration.</param>
        /// <returns>The same contract builder.</returns>
        public BeeInterfaceContractBuilder Method(string name, Action<BeeMethodBuilder> configure)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));

            var methods = GetMethods().Where(x => string.Equals(x.Name, name, StringComparison.Ordinal)).ToArray();
            if (methods.Length == 0)
                throw new MissingMethodException(_interfaceType.FullName, name);

            if (methods.Length > 1)
            {
                throw new InvalidOperationException(
                    $"Interface method '{_interfaceType.FullName}.{name}' is overloaded. Use the overload that specifies parameter types.");
            }

            return Method(methods[0], configure);
        }

        /// <summary>
        /// Configures a contract method identified by its exact parameter types.
        /// </summary>
        /// <param name="name">Method name.</param>
        /// <param name="parameterTypes">Exact parameter types.</param>
        /// <param name="configure">Method configuration.</param>
        /// <returns>The same contract builder.</returns>
        public BeeInterfaceContractBuilder Method(string name, IReadOnlyList<Type> parameterTypes, Action<BeeMethodBuilder> configure)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));

            if (parameterTypes == null)
                throw new ArgumentNullException(nameof(parameterTypes));

            var method = GetMethods().SingleOrDefault(x =>
                string.Equals(x.Name, name, StringComparison.Ordinal) &&
                x.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes));

            if (method == null)
            {
                throw new MissingMethodException(
                    $"Interface method '{_interfaceType.FullName}.{name}({string.Join(", ", parameterTypes.Select(x => x.Name))})' was not found.");
            }

            return Method(method, configure);
        }

        /// <summary>
        /// Configures a contract property that has a unique name.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="configure">Property configuration.</param>
        /// <returns>The same contract builder.</returns>
        public BeeInterfaceContractBuilder Property(string name, Action<BeePropertyBuilder> configure)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));

            var property = GetProperties().SingleOrDefault(x => string.Equals(x.Name, name, StringComparison.Ordinal));
            if (property == null)
                throw new MissingMemberException(_interfaceType.FullName, name);

            if (_properties.ContainsKey(property))
                throw new InvalidOperationException($"Interface property '{_interfaceType.FullName}.{name}' is already configured.");

            _properties.Add(property, configure ?? throw new ArgumentNullException(nameof(configure)));
            return this;
        }

        internal void Apply()
        {
            var events = _interfaceType.GetEvents();
            if (events.Length != 0)
            {
                throw new NotSupportedException(
                    $"Interface '{_interfaceType.FullName}' contains events. Event implementation is not available yet.");
            }

            var methods = GetMethods();
            var properties = GetProperties();
            if (_requireExplicitImplementations)
            {
                var missingMethods = methods.Where(x => !_methods.ContainsKey(x)).Select(FormatMethod);
                var missingProperties = properties.Where(x => !_properties.ContainsKey(x)).Select(x => x.Name);
                var missing = missingMethods.Concat(missingProperties).ToArray();
                if (missing.Length != 0)
                {
                    throw new InvalidOperationException(
                        $"Interface '{_interfaceType.FullName}' requires explicit implementations for: {string.Join(", ", missing)}.");
                }
            }

            _owner.Implements(_interfaceType, _registerInDi);
            foreach (var property in properties)
            {
                _properties.TryGetValue(property, out var configure);
                _owner.AddProperty(property.Name, property.PropertyType, configure);
            }

            foreach (var method in methods)
            {
                _methods.TryGetValue(method, out var configure);
                _owner.AddInterfaceMethod(method, configure);
            }
        }

        private BeeInterfaceContractBuilder Method(MethodInfo method, Action<BeeMethodBuilder> configure)
        {
            if (_methods.ContainsKey(method))
                throw new InvalidOperationException($"Interface method '{_interfaceType.FullName}.{FormatMethod(method)}' is already configured.");

            _methods.Add(method, configure ?? throw new ArgumentNullException(nameof(configure)));
            return this;
        }

        private MethodInfo[] GetMethods()
            => _interfaceType.GetMethods().Where(x => !x.IsSpecialName).ToArray();

        private PropertyInfo[] GetProperties()
            => _interfaceType.GetProperties();

        private static string FormatMethod(MethodInfo method)
            => $"{method.Name}({string.Join(", ", method.GetParameters().Select(x => x.ParameterType.Name))})";
    }
}
