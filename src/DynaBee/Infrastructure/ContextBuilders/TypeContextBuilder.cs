namespace DynaBee.Infrastructure.ContextBuilders
{
    using DynaBee.Infrastructure.Contexts;
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    internal sealed class TypeContextBuilder : ITypeContextBuilder
    {
        private readonly Dictionary<string, IElementContextBuilder> _elementContextBuilders = new();
        private readonly Dictionary<string, object> _metadata = new();
        private readonly Dictionary<string, FieldInfo> _fields = new(StringComparer.Ordinal);
        private readonly Dictionary<string, DynamicPropertyAccessor> _properties = new(StringComparer.Ordinal);
        private readonly Dictionary<DynamicMethodKey, MethodInfo> _methods = new();

        public TypeContextBuilder(string name, TypeBuilder typeBuilder, IAssemblyContextBuilder assemblyBuilderContext)
        {
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentNullException(nameof(name)) : name;
            TypeBuilder = typeBuilder ?? throw new ArgumentNullException(nameof(typeBuilder));
            AssemblyBuilderContext = assemblyBuilderContext ?? throw new ArgumentNullException(nameof(assemblyBuilderContext));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IAssemblyContextBuilder AssemblyBuilderContext { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public TypeBuilder TypeBuilder {  get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IElementContextBuilder AddElement(
            string name,
            ElementType elementType,
            ElementBuilderAction buildAction,
            IReadOnlyDictionary<string, object> metadata = null)
        {
            if (_elementContextBuilders.ContainsKey(name))
                throw new InvalidOperationException($"Element with name '{name}' already exists in dynamic type '{TypeBuilder.Name}'.");

            var elementContextBuilder = new ElementContextBuilder(name, elementType, buildAction, this, metadata);
            _elementContextBuilders.Add(name, elementContextBuilder);

            return elementContextBuilder;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void SetMetadata(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));

            _metadata[key] = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ITypeContext Build()
        {
            var orderedBuilders = _elementContextBuilders.Values
                .OrderBy(GetBuildOrder)
                .ToArray();

            var elementContexts = orderedBuilders.Select(x => x.Build()).ToArray();
            var clrType = TypeBuilder.CreateTypeInfo()?.AsType() ?? (Type)TypeBuilder;
            return new TypeContext(Name, clrType, elementContexts, _metadata);
        }

        internal void RegisterField(string name, FieldInfo field)
        {
            _fields[name] = field ?? throw new ArgumentNullException(nameof(field));
        }

        internal bool TryGetField(string name, out FieldInfo field)
            => _fields.TryGetValue(name, out field);

        internal void RegisterProperty(string name, Type type, MethodInfo getter, MethodInfo setter)
        {
            _properties[name] = new DynamicPropertyAccessor(type, getter, setter);
        }

        internal bool TryGetProperty(string name, out DynamicPropertyAccessor property)
            => _properties.TryGetValue(name, out property);

        internal void RegisterMethod(string name, Type[] parameterTypes, MethodInfo method)
        {
            _methods[new DynamicMethodKey(name, parameterTypes)] = method ?? throw new ArgumentNullException(nameof(method));
        }

        internal bool TryGetMethod(string name, IReadOnlyList<Type> parameterTypes, out MethodInfo method)
            => _methods.TryGetValue(new DynamicMethodKey(name, parameterTypes), out method);

        private static int GetBuildOrder(IElementContextBuilder builder)
        {
            if (builder.Name.StartsWith(".ctor", StringComparison.Ordinal))
                return 3;

            return builder.ElementType switch
            {
                ElementType.Field => 0,
                ElementType.Property => 1,
                ElementType.Method => 2,
                _ => 2
            };
        }
    }

    internal readonly struct DynamicPropertyAccessor
    {
        public DynamicPropertyAccessor(Type type, MethodInfo getter, MethodInfo setter)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Getter = getter;
            Setter = setter;
        }

        public Type Type { get; }

        public MethodInfo Getter { get; }

        public MethodInfo Setter { get; }
    }

    internal readonly struct DynamicMethodKey : IEquatable<DynamicMethodKey>
    {
        private readonly Type[] _parameterTypes;

        public DynamicMethodKey(string name, IReadOnlyList<Type> parameterTypes)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _parameterTypes = parameterTypes?.ToArray() ?? throw new ArgumentNullException(nameof(parameterTypes));
        }

        public string Name { get; }

        public bool Equals(DynamicMethodKey other)
        {
            if (!string.Equals(Name, other.Name, StringComparison.Ordinal))
                return false;

            if (_parameterTypes.Length != other._parameterTypes.Length)
                return false;

            for (var i = 0; i < _parameterTypes.Length; i++)
            {
                if (_parameterTypes[i] != other._parameterTypes[i])
                    return false;
            }

            return true;
        }

        public override bool Equals(object obj)
            => obj is DynamicMethodKey other && Equals(other);

        public override int GetHashCode()
        {
            var hash = StringComparer.Ordinal.GetHashCode(Name);
            foreach (var parameterType in _parameterTypes)
                hash = HashCode.Combine(hash, parameterType);

            return hash;
        }
    }
}
