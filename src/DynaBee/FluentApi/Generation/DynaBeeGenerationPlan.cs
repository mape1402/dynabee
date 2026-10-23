namespace DynaBee.FluentApi.Generation
{
    using DynaBee.Infrastructure;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;

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

            Validate().ThrowIfErrors();

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

        /// <summary>
        /// Validates the declarative portion of this generation plan without emitting an assembly.
        /// </summary>
        /// <returns>The collected diagnostics.</returns>
        public DynaBeePlanValidationResult Validate()
        {
            var diagnostics = new List<DynaBeePlanDiagnostic>();
            var classesByName = _classes
                .GroupBy(x => x.Name, StringComparer.Ordinal)
                .Where(x => x.Count() > 1);

            foreach (var duplicate in classesByName)
            {
                diagnostics.Add(new DynaBeePlanDiagnostic(
                    DynaBeePlanDiagnosticSeverity.Error,
                    "DBP001",
                    $"The generated type name '{duplicate.Key}' is declared more than once.",
                    duplicate.Key));
            }

            foreach (var classPlan in _classes)
                ValidateClass(classPlan, diagnostics);

            return new DynaBeePlanValidationResult(diagnostics);
        }

        /// <summary>
        /// Creates a stable SHA-256 fingerprint for the declarative assembly and type shape in this plan.
        /// Add a metadata value when distinct method-body closures share the same declared shape and need separate cache entries.
        /// </summary>
        /// <returns>A lowercase hexadecimal SHA-256 fingerprint.</returns>
        public string GetFingerprint()
        {
            var builder = new StringBuilder(AssemblyName);
            foreach (var metadata in _metadata.OrderBy(x => x.Key, StringComparer.Ordinal))
                Append(builder, "assembly-metadata", metadata.Key, FormatValue(metadata.Value));

            foreach (var classPlan in _classes.OrderBy(x => x.Name, StringComparer.Ordinal))
            {
                Append(builder, "class", classPlan.Name, classPlan.BaseType?.AssemblyQualifiedName ?? string.Empty);
                foreach (var interfaceType in classPlan.Interfaces.OrderBy(x => x?.AssemblyQualifiedName, StringComparer.Ordinal))
                    Append(builder, "interface", interfaceType?.AssemblyQualifiedName ?? string.Empty);
                foreach (var serviceType in classPlan.ServiceTypes.OrderBy(x => x?.AssemblyQualifiedName, StringComparer.Ordinal))
                    Append(builder, "service", serviceType?.AssemblyQualifiedName ?? string.Empty);
                foreach (var metadata in classPlan.Metadata.OrderBy(x => x.Key, StringComparer.Ordinal))
                    Append(builder, "class-metadata", metadata.Key, FormatValue(metadata.Value));
                foreach (var member in classPlan.Members.OrderBy(x => x.ElementType).ThenBy(x => x.Name, StringComparer.Ordinal))
                    Append(builder, "member", member.ElementType.ToString(), member.Name, member.ValueType?.AssemblyQualifiedName ?? string.Empty);
            }

            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString()))).ToLowerInvariant();
        }

        /// <summary>
        /// Exports the declarative type shape as deterministic C# source.
        /// </summary>
        /// <returns>Generated source and source-map entries.</returns>
        public DynaBeeSourceExportResult ExportSource()
            => new DynaBeeSourceExporter().Export(this);

        private static void ValidateClass(DynaBeeGeneratedClassPlan classPlan, ICollection<DynaBeePlanDiagnostic> diagnostics)
        {
            if (classPlan.BaseType != null)
            {
                if (classPlan.BaseType.IsInterface || classPlan.BaseType.IsValueType)
                {
                    diagnostics.Add(new DynaBeePlanDiagnostic(
                        DynaBeePlanDiagnosticSeverity.Error,
                        "DBP010",
                        $"Base type '{classPlan.BaseType}' must be a non-value class.",
                        classPlan.Name));
                }
                else if (classPlan.BaseType.IsSealed)
                {
                    diagnostics.Add(new DynaBeePlanDiagnostic(
                        DynaBeePlanDiagnosticSeverity.Error,
                        "DBP011",
                        $"Base type '{classPlan.BaseType}' is sealed and cannot be inherited.",
                        classPlan.Name));
                }
            }

            foreach (var interfaceType in classPlan.Interfaces.Where(x => x == null || !x.IsInterface))
            {
                diagnostics.Add(new DynaBeePlanDiagnostic(
                    DynaBeePlanDiagnosticSeverity.Error,
                    "DBP020",
                    $"'{interfaceType}' is not an interface and cannot be implemented.",
                    classPlan.Name));
            }

            foreach (var duplicate in classPlan.Interfaces
                .Where(x => x != null)
                .GroupBy(x => x)
                .Where(x => x.Count() > 1))
            {
                diagnostics.Add(new DynaBeePlanDiagnostic(
                    DynaBeePlanDiagnosticSeverity.Warning,
                    "DBP021",
                    $"Interface '{duplicate.Key}' is declared more than once.",
                    classPlan.Name));
            }

            foreach (var member in classPlan.Members.Where(x => x.ValueType == null && x.Name != ".ctor"))
            {
                diagnostics.Add(new DynaBeePlanDiagnostic(
                    DynaBeePlanDiagnosticSeverity.Error,
                    "DBP030",
                    $"Member '{member.Name}' does not define a value or return type.",
                    classPlan.Name,
                    member.Name));
            }

            foreach (var duplicate in classPlan.Members
                .Where(x => x.ElementType != ElementType.Method)
                .GroupBy(x => (x.ElementType, x.Name))
                .Where(x => x.Count() > 1))
            {
                diagnostics.Add(new DynaBeePlanDiagnostic(
                    DynaBeePlanDiagnosticSeverity.Error,
                    "DBP031",
                    $"{duplicate.Key.ElementType} '{duplicate.Key.Name}' is declared more than once.",
                    classPlan.Name,
                    duplicate.Key.Name));
            }

            foreach (var serviceType in classPlan.ServiceTypes.Where(x => x == null))
            {
                diagnostics.Add(new DynaBeePlanDiagnostic(
                    DynaBeePlanDiagnosticSeverity.Error,
                    "DBP040",
                    "A service registration type cannot be null.",
                    classPlan.Name));
            }
        }

        private static void Append(StringBuilder builder, params string[] values)
        {
            foreach (var value in values)
            {
                builder.Append(value?.Length ?? 0).Append(':').Append(value).Append('|');
            }
        }

        private static string FormatValue(object value)
            => value == null
                ? "<null>"
                : $"{value.GetType().AssemblyQualifiedName}:{Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)}";
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
        internal DynaBeeGeneratedMemberPlan(string name, ElementType elementType, Type valueType, IReadOnlyList<Type> parameterTypes = null)
        {
            Name = name;
            ElementType = elementType;
            ValueType = valueType;
            ParameterTypes = parameterTypes ?? Array.Empty<Type>();
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

        /// <summary>
        /// Gets the planned method or constructor parameter types.
        /// </summary>
        public IReadOnlyList<Type> ParameterTypes { get; }
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
        /// Adds a reflected interface contract whose methods, properties, events, and supported indexers are generated automatically.
        /// </summary>
        /// <param name="interfaceType">Interface contract to implement.</param>
        /// <param name="configure">Optional contract member configuration.</param>
        /// <param name="registerInDi">True to register the interface in DI.</param>
        /// <returns>The same class plan builder.</returns>
        public DynaBeeGeneratedClassPlanBuilder ImplementInterface(
            Type interfaceType,
            Action<BeeInterfaceContractBuilder> configure = null,
            bool registerInDi = true)
        {
            if (interfaceType == null)
                throw new ArgumentNullException(nameof(interfaceType));

            if (!interfaceType.IsInterface)
                throw new ArgumentException("The provided type must be an interface.", nameof(interfaceType));

            _interfaces.Add(interfaceType);
            foreach (var property in interfaceType.GetProperties())
                _members.Add(new DynaBeeGeneratedMemberPlan(property.Name, ElementType.Property, property.PropertyType, property.GetIndexParameters().Select(x => x.ParameterType).ToArray()));
            foreach (var method in interfaceType.GetMethods().Where(x => !x.IsSpecialName))
                _members.Add(new DynaBeeGeneratedMemberPlan(method.Name, ElementType.Method, method.ReturnType, method.GetParameters().Select(x => x.ParameterType).ToArray()));

            _operations.Add(type => type.ImplementInterface(interfaceType, configure, registerInDi));
            return this;
        }

        /// <summary>
        /// Adds a reflected generic interface contract whose members are generated automatically.
        /// </summary>
        /// <typeparam name="TInterface">Interface contract to implement.</typeparam>
        /// <param name="configure">Optional contract member configuration.</param>
        /// <param name="registerInDi">True to register the interface in DI.</param>
        /// <returns>The same class plan builder.</returns>
        public DynaBeeGeneratedClassPlanBuilder ImplementInterface<TInterface>(
            Action<BeeInterfaceContractBuilder> configure = null,
            bool registerInDi = true)
            => ImplementInterface(typeof(TInterface), configure, registerInDi);

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
            var methodBuilder = new BeeMethodBuilder(name, returnType);
            configure?.Invoke(methodBuilder);
            _members.Add(new DynaBeeGeneratedMemberPlan(
                name,
                ElementType.Method,
                returnType,
                methodBuilder.Parameters.Select(x => (Type)x.Type).ToArray()));
            _operations.Add(type => type.AddConfiguredMethod(methodBuilder));
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

            var methodBuilder = new BeeMethodBuilder(baseMethod.Name, baseMethod.ReturnType)
                .WithParameterRange(baseMethod.GetParameters().Select(x => (x.Name ?? $"arg{x.Position}", (BeeType)x.ParameterType)))
                .Overrides(baseMethod);
            configure?.Invoke(methodBuilder);
            _members.Add(new DynaBeeGeneratedMemberPlan(
                baseMethod.Name,
                ElementType.Method,
                baseMethod.ReturnType,
                baseMethod.GetParameters().Select(x => x.ParameterType).ToArray()));
            _operations.Add(type => type.AddConfiguredMethod(methodBuilder));
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
