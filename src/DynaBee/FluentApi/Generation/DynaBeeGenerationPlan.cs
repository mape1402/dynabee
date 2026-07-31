namespace DynaBee.FluentApi.Generation
{
    using DynaBee.Infrastructure;
    using System.Reflection;

    /// <summary>
    /// Describes dynamic types before they are emitted into an assembly builder.
    /// </summary>
    public sealed class DynaBeeGenerationPlan
    {
        private readonly Dictionary<string, object> _metadata = new();
        private readonly List<DynaBeeGeneratedClassPlan> _classes = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="DynaBeeGenerationPlan"/> class.
        /// </summary>
        /// <param name="assemblyName">Logical dynamic assembly name.</param>
        public DynaBeeGenerationPlan(string assemblyName)
        {
            AssemblyName = string.IsNullOrWhiteSpace(assemblyName)
                ? throw new ArgumentException(nameof(assemblyName))
                : assemblyName;
        }

        /// <summary>
        /// Gets the logical dynamic assembly name.
        /// </summary>
        public string AssemblyName { get; }

        /// <summary>
        /// Gets metadata attached to the planned assembly.
        /// </summary>
        public IReadOnlyDictionary<string, object> Metadata => _metadata;

        /// <summary>
        /// Gets planned generated classes.
        /// </summary>
        public IReadOnlyList<DynaBeeGeneratedClassPlan> Classes => _classes;

        /// <summary>
        /// Stores metadata for the planned generated assembly.
        /// </summary>
        /// <param name="key">Metadata key.</param>
        /// <param name="value">Metadata value.</param>
        /// <returns>The same generation plan.</returns>
        public DynaBeeGenerationPlan WithMetadata(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));

            _metadata[key] = value ?? throw new ArgumentNullException(nameof(value));
            return this;
        }

        /// <summary>
        /// Stores strongly typed metadata for the planned generated assembly.
        /// </summary>
        /// <typeparam name="T">Metadata value type.</typeparam>
        /// <param name="key">Typed metadata key.</param>
        /// <param name="value">Metadata value.</param>
        /// <returns>The same generation plan.</returns>
        public DynaBeeGenerationPlan WithMetadata<T>(BeeMetadataKey<T> key, T value)
            => WithMetadata(key.Name, value);

        /// <summary>
        /// Adds a generated class plan.
        /// </summary>
        /// <param name="name">Logical generated type name.</param>
        /// <param name="configure">Class plan configuration callback.</param>
        /// <returns>The same generation plan.</returns>
        public DynaBeeGenerationPlan AddClass(string name, Action<DynaBeeGeneratedClassPlanBuilder> configure)
        {
            var builder = new DynaBeeGeneratedClassPlanBuilder(name);
            configure?.Invoke(builder);
            _classes.Add(builder.Build());
            return this;
        }

        /// <summary>
        /// Applies the plan to an assembly builder.
        /// </summary>
        /// <param name="builder">Target assembly builder.</param>
        /// <returns>The same assembly builder.</returns>
        public IBeeAssemblyBuilder ApplyTo(IBeeAssemblyBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            foreach (var metadata in _metadata)
                builder.WithMetadata(metadata.Key, metadata.Value);

            foreach (var classPlan in _classes)
            {
                builder.AddClass(classPlan.Name, classBuilder =>
                {
                    foreach (var operation in classPlan.Operations)
                        operation(classBuilder);
                });
            }

            return builder;
        }
    }

    /// <summary>
    /// Describes a generated class before emission.
    /// </summary>
    public sealed class DynaBeeGeneratedClassPlan
    {
        internal DynaBeeGeneratedClassPlan(
            string name,
            Type baseType,
            IReadOnlyList<Type> interfaces,
            IReadOnlyList<Type> serviceTypes,
            bool? registerAsConcrete,
            IReadOnlyDictionary<string, object> metadata,
            IReadOnlyList<DynaBeeGeneratedMemberPlan> members,
            IReadOnlyList<Action<BeeClassBuilder>> operations)
        {
            Name = name;
            BaseType = baseType;
            Interfaces = interfaces;
            ServiceTypes = serviceTypes;
            RegisterAsConcrete = registerAsConcrete;
            Metadata = metadata;
            Members = members;
            Operations = operations;
        }

        /// <summary>
        /// Gets the logical generated type name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the planned base type, or <see langword="null"/> when the default base type is used.
        /// </summary>
        public Type BaseType { get; }

        /// <summary>
        /// Gets the planned interfaces.
        /// </summary>
        public IReadOnlyList<Type> Interfaces { get; }

        /// <summary>
        /// Gets planned service registration types.
        /// </summary>
        public IReadOnlyList<Type> ServiceTypes { get; }

        /// <summary>
        /// Gets the planned concrete DI registration setting, or <see langword="null"/> when unspecified.
        /// </summary>
        public bool? RegisterAsConcrete { get; }

        /// <summary>
        /// Gets metadata attached to the planned generated type.
        /// </summary>
        public IReadOnlyDictionary<string, object> Metadata { get; }

        /// <summary>
        /// Gets planned generated members.
        /// </summary>
        public IReadOnlyList<DynaBeeGeneratedMemberPlan> Members { get; }

        internal IReadOnlyList<Action<BeeClassBuilder>> Operations { get; }
    }

    /// <summary>
    /// Describes a generated member before emission.
    /// </summary>
    public sealed class DynaBeeGeneratedMemberPlan
    {
        internal DynaBeeGeneratedMemberPlan(string name, ElementType elementType, Type valueType)
        {
            Name = name;
            ElementType = elementType;
            ValueType = valueType;
        }

        /// <summary>
        /// Gets the member name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the member element type.
        /// </summary>
        public ElementType ElementType { get; }

        /// <summary>
        /// Gets the member value type, return type, or <see langword="null"/> when not applicable.
        /// </summary>
        public Type ValueType { get; }
    }

    /// <summary>
    /// Fluent builder for a generated class plan.
    /// </summary>
    public sealed class DynaBeeGeneratedClassPlanBuilder
    {
        private readonly List<Type> _interfaces = new();
        private readonly List<Type> _serviceTypes = new();
        private readonly Dictionary<string, object> _metadata = new();
        private readonly List<DynaBeeGeneratedMemberPlan> _members = new();
        private readonly List<Action<BeeClassBuilder>> _operations = new();
        private Type _baseType;
        private bool? _registerAsConcrete;

        internal DynaBeeGeneratedClassPlanBuilder(string name)
        {
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException(nameof(name)) : name;
        }

        /// <summary>
        /// Gets the logical generated type name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Sets the base class for the planned generated type.
        /// </summary>
        /// <param name="baseType">Base class.</param>
        /// <returns>The same class plan builder.</returns>
        public DynaBeeGeneratedClassPlanBuilder Inherits(Type baseType)
        {
            _baseType = baseType ?? throw new ArgumentNullException(nameof(baseType));
            _operations.Add(type => type.Inherits(baseType));
            return this;
        }

        /// <summary>
        /// Adds an interface implementation to the planned generated type.
        /// </summary>
        /// <param name="interfaceType">Interface type.</param>
        /// <param name="registerInDi">True to register the interface in DI.</param>
        /// <returns>The same class plan builder.</returns>
        public DynaBeeGeneratedClassPlanBuilder Implements(Type interfaceType, bool registerInDi = true)
        {
            if (interfaceType == null)
                throw new ArgumentNullException(nameof(interfaceType));

            _interfaces.Add(interfaceType);
            _operations.Add(type => type.Implements(interfaceType, registerInDi));
            return this;
        }

        /// <summary>
        /// Adds an interface implementation to the planned generated type.
        /// </summary>
        /// <typeparam name="TInterface">Interface type.</typeparam>
        /// <param name="registerInDi">True to register the interface in DI.</param>
        /// <returns>The same class plan builder.</returns>
        public DynaBeeGeneratedClassPlanBuilder Implements<TInterface>(bool registerInDi = true)
            => Implements(typeof(TInterface), registerInDi);

        /// <summary>
        /// Adds a service registration type to the planned generated type.
        /// </summary>
        /// <param name="serviceType">Service type.</param>
        /// <param name="registerInDi">True to register the service type in DI.</param>
        /// <returns>The same class plan builder.</returns>
        public DynaBeeGeneratedClassPlanBuilder RegisterAs(Type serviceType, bool registerInDi = true)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            _serviceTypes.Add(serviceType);
            _operations.Add(type => type.RegisterAs(serviceType, registerInDi));
            return this;
        }

        /// <summary>
        /// Sets whether the generated concrete type should be registered in DI.
        /// </summary>
        /// <param name="register">True to register the concrete type.</param>
        /// <returns>The same class plan builder.</returns>
        public DynaBeeGeneratedClassPlanBuilder RegisterAsConcrete(bool register = true)
        {
            _registerAsConcrete = register;
            _operations.Add(type => type.RegisterAsConcrete(register));
            return this;
        }

        /// <summary>
        /// Stores metadata for the planned generated type.
        /// </summary>
        /// <param name="key">Metadata key.</param>
        /// <param name="value">Metadata value.</param>
        /// <returns>The same class plan builder.</returns>
        public DynaBeeGeneratedClassPlanBuilder WithMetadata(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));

            _metadata[key] = value ?? throw new ArgumentNullException(nameof(value));
            _operations.Add(type => type.WithMetadata(key, value));
            return this;
        }

        /// <summary>
        /// Adds a constructor definition to the planned generated type.
        /// </summary>
        /// <param name="configure">Constructor configuration callback.</param>
        /// <returns>The same class plan builder.</returns>
        public DynaBeeGeneratedClassPlanBuilder AddConstructor(Action<BeeConstructorBuilder> configure)
        {
            _members.Add(new DynaBeeGeneratedMemberPlan(".ctor", ElementType.Method, null));
            _operations.Add(type => type.AddConstructor(configure));
            return this;
        }

        /// <summary>
        /// Adds a method definition to the planned generated type.
        /// </summary>
        /// <param name="name">Method name.</param>
        /// <param name="returnType">Return type.</param>
        /// <param name="configure">Method configuration callback.</param>
        /// <returns>The same class plan builder.</returns>
        public DynaBeeGeneratedClassPlanBuilder AddMethod(string name, BeeType returnType, Action<BeeMethodBuilder> configure = null)
        {
            _members.Add(new DynaBeeGeneratedMemberPlan(name, ElementType.Method, returnType));
            _operations.Add(type => type.AddMethod(name, returnType, configure));
            return this;
        }

        /// <summary>
        /// Adds a property definition to the planned generated type.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="propertyType">Property type.</param>
        /// <param name="configure">Property configuration callback.</param>
        /// <returns>The same class plan builder.</returns>
        public DynaBeeGeneratedClassPlanBuilder AddProperty(string name, BeeType propertyType, Action<BeePropertyBuilder> configure = null)
        {
            _members.Add(new DynaBeeGeneratedMemberPlan(name, ElementType.Property, propertyType));
            _operations.Add(type => type.AddProperty(name, propertyType, configure));
            return this;
        }

        /// <summary>
        /// Adds an overriding method definition to the planned generated type.
        /// </summary>
        /// <param name="baseMethod">Base method to override.</param>
        /// <param name="configure">Method configuration callback.</param>
        /// <returns>The same class plan builder.</returns>
        public DynaBeeGeneratedClassPlanBuilder OverrideMethod(MethodInfo baseMethod, Action<BeeMethodBuilder> configure = null)
        {
            if (baseMethod == null)
                throw new ArgumentNullException(nameof(baseMethod));

            _members.Add(new DynaBeeGeneratedMemberPlan(baseMethod.Name, ElementType.Method, baseMethod.ReturnType));
            _operations.Add(type => type.OverrideMethod(baseMethod, configure));
            return this;
        }

        /// <summary>
        /// Adds an overriding property definition to the planned generated type.
        /// </summary>
        /// <param name="baseProperty">Base property to override.</param>
        /// <param name="configure">Property override configuration callback.</param>
        /// <returns>The same class plan builder.</returns>
        public DynaBeeGeneratedClassPlanBuilder OverrideProperty(PropertyInfo baseProperty, Action<BeePropertyOverrideBuilder> configure)
        {
            if (baseProperty == null)
                throw new ArgumentNullException(nameof(baseProperty));

            _members.Add(new DynaBeeGeneratedMemberPlan(baseProperty.Name, ElementType.Property, baseProperty.PropertyType));
            _operations.Add(type => type.OverrideProperty(baseProperty, configure));
            return this;
        }

        internal DynaBeeGeneratedClassPlan Build()
            => new(
                Name,
                _baseType,
                _interfaces.ToArray(),
                _serviceTypes.ToArray(),
                _registerAsConcrete,
                new Dictionary<string, object>(_metadata),
                _members.ToArray(),
                _operations.ToArray());
    }
}
