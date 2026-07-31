namespace DynaBee.FluentApi.DependencyInjection
{
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Options for registering generated DynaBee types in dependency injection.
    /// </summary>
    public sealed class DynaBeeServiceRegistrationOptions
    {
        private readonly List<DynaBeeGeneratedTypeRegistrationRule> _rules = new();

        /// <summary>
        /// Adds a registration rule for generated types.
        /// </summary>
        /// <param name="configure">Registration rule configuration callback.</param>
        /// <returns>The same options instance.</returns>
        public DynaBeeServiceRegistrationOptions Register(Action<DynaBeeGeneratedTypeRegistrationBuilder> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var builder = new DynaBeeGeneratedTypeRegistrationBuilder();
            configure(builder);
            _rules.Add(builder.Build());
            return this;
        }

        internal IReadOnlyList<DynaBeeGeneratedTypeRegistrationRule> Rules => _rules;
    }

    /// <summary>
    /// Fluent builder for one generated type registration rule.
    /// </summary>
    public sealed class DynaBeeGeneratedTypeRegistrationBuilder
    {
        private readonly List<Type> _serviceTypes = new();
        private readonly List<Func<ITypeContext, IEnumerable<ServiceDescriptor>>> _projections = new();
        private Func<ITypeContext, bool> _predicate = _ => true;
        private ServiceLifetime? _lifetime;
        private bool? _registerConcrete;

        /// <summary>
        /// Restricts the rule to a generated logical type name.
        /// </summary>
        /// <param name="typeName">Generated logical type name.</param>
        /// <returns>The same rule builder.</returns>
        public DynaBeeGeneratedTypeRegistrationBuilder ForType(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentException(nameof(typeName));

            return Where(type => string.Equals(type.Name, typeName, StringComparison.Ordinal));
        }

        /// <summary>
        /// Restricts the rule to generated types matching a predicate.
        /// </summary>
        /// <param name="predicate">Generated type predicate.</param>
        /// <returns>The same rule builder.</returns>
        public DynaBeeGeneratedTypeRegistrationBuilder Where(Func<ITypeContext, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            var previous = _predicate;
            _predicate = type => previous(type) && predicate(type);
            return this;
        }

        /// <summary>
        /// Registers matching generated types as the specified service type when assignable.
        /// </summary>
        /// <param name="serviceType">Service type to register.</param>
        /// <returns>The same rule builder.</returns>
        public DynaBeeGeneratedTypeRegistrationBuilder As(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            _serviceTypes.Add(serviceType);
            return this;
        }

        /// <summary>
        /// Registers matching generated types as the specified service type when assignable.
        /// </summary>
        /// <typeparam name="TService">Service type to register.</typeparam>
        /// <returns>The same rule builder.</returns>
        public DynaBeeGeneratedTypeRegistrationBuilder As<TService>()
            => As(typeof(TService));

        /// <summary>
        /// Sets the service lifetime for descriptors produced by this rule.
        /// </summary>
        /// <param name="lifetime">Service lifetime.</param>
        /// <returns>The same rule builder.</returns>
        public DynaBeeGeneratedTypeRegistrationBuilder WithLifetime(ServiceLifetime lifetime)
        {
            _lifetime = lifetime;
            return this;
        }

        /// <summary>
        /// Skips registering matching generated types as their concrete CLR type.
        /// </summary>
        /// <returns>The same rule builder.</returns>
        public DynaBeeGeneratedTypeRegistrationBuilder SkipConcrete()
        {
            _registerConcrete = false;
            return this;
        }

        /// <summary>
        /// Registers matching generated types as their concrete CLR type.
        /// </summary>
        /// <returns>The same rule builder.</returns>
        public DynaBeeGeneratedTypeRegistrationBuilder AsConcrete()
        {
            _registerConcrete = true;
            return this;
        }

        /// <summary>
        /// Adds caller-provided service descriptor projection for matching generated types.
        /// </summary>
        /// <param name="project">Projection callback.</param>
        /// <returns>The same rule builder.</returns>
        public DynaBeeGeneratedTypeRegistrationBuilder Project(Func<ITypeContext, IEnumerable<ServiceDescriptor>> project)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            _projections.Add(project);
            return this;
        }

        internal DynaBeeGeneratedTypeRegistrationRule Build()
            => new(_predicate, _serviceTypes.ToArray(), _lifetime, _registerConcrete, _projections.ToArray());
    }

    internal sealed class DynaBeeGeneratedTypeRegistrationRule
    {
        public DynaBeeGeneratedTypeRegistrationRule(
            Func<ITypeContext, bool> predicate,
            IReadOnlyList<Type> serviceTypes,
            ServiceLifetime? lifetime,
            bool? registerConcrete,
            IReadOnlyList<Func<ITypeContext, IEnumerable<ServiceDescriptor>>> projections)
        {
            Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
            ServiceTypes = serviceTypes ?? throw new ArgumentNullException(nameof(serviceTypes));
            Lifetime = lifetime;
            RegisterConcrete = registerConcrete;
            Projections = projections ?? throw new ArgumentNullException(nameof(projections));
        }

        public Func<ITypeContext, bool> Predicate { get; }

        public IReadOnlyList<Type> ServiceTypes { get; }

        public ServiceLifetime? Lifetime { get; }

        public bool? RegisterConcrete { get; }

        public IReadOnlyList<Func<ITypeContext, IEnumerable<ServiceDescriptor>>> Projections { get; }
    }
}
