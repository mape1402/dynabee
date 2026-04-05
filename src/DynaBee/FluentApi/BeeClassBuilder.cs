namespace DynaBee.FluentApi
{
    using DynaBee.Infrastructure;
    using DynaBee.Infrastructure.Configurators;

    /// <summary>
    /// Fluent builder for a dynamic class.
    /// </summary>
    public sealed class BeeClassBuilder
    {
        private readonly ClassConfigurator _classConfigurator;
        private readonly List<(string PropertyName, Type PropertyType, string ParameterName)> _injectedDependencies = new();
        private bool _hasSynthesizedDependencyConstructor;

        internal BeeClassBuilder(ClassConfigurator classConfigurator)
        {
            _classConfigurator = classConfigurator ?? throw new ArgumentNullException(nameof(classConfigurator));
        }

        /// <summary>
        /// Sets the base class for the generated type.
        /// </summary>
        public BeeClassBuilder Inherits(Type parentType)
        {
            _classConfigurator.WithParentType(parentType);
            return this;
        }

        /// <summary>
        /// Sets the base class for the generated type.
        /// </summary>
        public BeeClassBuilder Inherits<TParent>()
            => Inherits(typeof(TParent));

        /// <summary>
        /// Adds an interface implementation.
        /// </summary>
        public BeeClassBuilder Implements(Type interfaceType)
        {
            _classConfigurator.Implements(interfaceType);
            return this;
        }

        /// <summary>
        /// Adds an interface implementation.
        /// </summary>
        public BeeClassBuilder Implements<TInterface>()
            => Implements(typeof(TInterface));

        /// <summary>
        /// Adds a custom attribute to the generated class.
        /// </summary>
        public BeeClassBuilder AddAttribute(BeeAttribute attribute)
        {
            _classConfigurator.AddAttribute(attribute);
            return this;
        }

        /// <summary>
        /// Adds a custom attribute to the generated class.
        /// </summary>
        public BeeClassBuilder AddAttribute<TAttribute>(params object[] constructorArguments)
            where TAttribute : Attribute
            => AddAttribute(BeeAttribute.Of<TAttribute>(constructorArguments));

        /// <summary>
        /// Adds a custom attribute to the generated class using fluent configuration.
        /// </summary>
        public BeeClassBuilder AddAttribute<TAttribute>(Action<BeeAttributeBuilder> configure)
            where TAttribute : Attribute
        {
            var builder = new BeeAttributeBuilder(typeof(TAttribute));
            configure?.Invoke(builder);
            return AddAttribute(builder.Build());
        }

        /// <summary>
        /// Adds a property using fluent configuration.
        /// </summary>
        public BeeClassBuilder AddProperty(string name, BeeType type, Action<BeePropertyBuilder> configure = null)
        {
            var propertyBuilder = new BeePropertyBuilder(name, type);
            configure?.Invoke(propertyBuilder);
            _classConfigurator.AddElementBuilder(propertyBuilder.ToConfigurator());
            return this;
        }

        /// <summary>
        /// Adds a property using fluent configuration.
        /// </summary>
        public BeeClassBuilder AddProperty<TProperty>(string name, Action<BeePropertyBuilder> configure = null)
            => AddProperty(name, typeof(TProperty), configure);

        /// <summary>
        /// Adds an auto-property with private backing field and public getter/setter.
        /// </summary>
        public BeeClassBuilder AddAutoProperty(string name, BeeType type)
            => AddProperty(name, type);

        /// <summary>
        /// Adds an auto-property with custom attributes.
        /// </summary>
        public BeeClassBuilder AddAutoProperty(string name, BeeType type, params BeeAttribute[] attributes)
            => AddProperty(name, type, p =>
            {
                if (attributes == null)
                    return;

                foreach (var attribute in attributes)
                    p.AddAttribute(attribute);
            });

        /// <summary>
        /// Adds an auto-property with configurable getter/setter visibility.
        /// </summary>
        public BeeClassBuilder AddAutoProperty(
            string name,
            BeeType type,
            bool hasGetter,
            bool hasSetter,
            FieldAccessModifier fieldAccessModifier = default,
            MethodAccessModifier getterAccessModifier = default,
            MethodAccessModifier setterAccessModifier = default,
            IReadOnlyCollection<BeeAttribute> attributes = null)
        {
            return AddProperty(name, type, p =>
            {
                p.WithGetter(hasGetter)
                 .WithSetter(hasSetter)
                 .WithBackingFieldAccess(fieldAccessModifier)
                 .WithGetterAccess(getterAccessModifier)
                 .WithSetterAccess(setterAccessModifier);

                if (attributes == null)
                    return;

                foreach (var attribute in attributes)
                    p.AddAttribute(attribute);
            });
        }

        /// <summary>
        /// Adds an auto-property with private backing field and public getter/setter.
        /// </summary>
        public BeeClassBuilder AddAutoProperty<TProperty>(string name)
            => AddAutoProperty(name, typeof(TProperty));

        /// <summary>
        /// Adds an auto-property with custom attributes.
        /// </summary>
        public BeeClassBuilder AddAutoProperty<TProperty>(string name, params BeeAttribute[] attributes)
            => AddAutoProperty(name, typeof(TProperty), attributes);

        /// <summary>
        /// Adds an auto-property with configurable getter/setter visibility.
        /// </summary>
        public BeeClassBuilder AddAutoProperty<TProperty>(string name, bool hasGetter, bool hasSetter)
            => AddAutoProperty(name, typeof(TProperty), hasGetter, hasSetter);

        /// <summary>
        /// Adds an auto-property with configurable access modifiers.
        /// </summary>
        public BeeClassBuilder AddAutoProperty<TProperty>(
            string name,
            bool hasGetter,
            bool hasSetter,
            FieldAccessModifier fieldAccessModifier,
            MethodAccessModifier getterAccessModifier,
            MethodAccessModifier setterAccessModifier,
            IReadOnlyCollection<BeeAttribute> attributes = null)
            => AddAutoProperty(
                name,
                typeof(TProperty),
                hasGetter,
                hasSetter,
                fieldAccessModifier,
                getterAccessModifier,
                setterAccessModifier,
                attributes);

        /// <summary>
        /// Adds a read-only auto-property (getter only).
        /// </summary>
        public BeeClassBuilder AddReadOnlyProperty(string name, BeeType type)
            => AddProperty(name, type, p => p.AsReadOnly());

        /// <summary>
        /// Adds a read-only auto-property (getter only).
        /// </summary>
        public BeeClassBuilder AddReadOnlyProperty<TProperty>(string name)
            => AddReadOnlyProperty(name, typeof(TProperty));

        /// <summary>
        /// Adds a write-only auto-property (setter only).
        /// </summary>
        public BeeClassBuilder AddWriteOnlyProperty(string name, BeeType type)
            => AddProperty(name, type, p => p.AsWriteOnly());

        /// <summary>
        /// Adds a write-only auto-property (setter only).
        /// </summary>
        public BeeClassBuilder AddWriteOnlyProperty<TProperty>(string name)
            => AddWriteOnlyProperty(name, typeof(TProperty));

        /// <summary>
        /// Adds a method.
        /// </summary>
        public BeeClassBuilder AddMethod(string name, BeeType returnType, Action<BeeMethodBuilder> configure = null)
        {
            var methodBuilder = new BeeMethodBuilder(name, returnType);
            configure?.Invoke(methodBuilder);
            _classConfigurator.AddElementBuilder(methodBuilder.ToConfigurator());
            return this;
        }

        /// <summary>
        /// Adds a field.
        /// </summary>
        public BeeClassBuilder AddField(string name, BeeType type, FieldAccessModifier accessModifier = default)
        {
            _classConfigurator.AddElementBuilder(new FieldConfigurator(name, type, accessModifier));
            return this;
        }

        /// <summary>
        /// Adds a field.
        /// </summary>
        public BeeClassBuilder AddField<TField>(string name, FieldAccessModifier accessModifier = default)
            => AddField(name, typeof(TField), accessModifier);

        /// <summary>
        /// Adds a void method.
        /// </summary>
        public BeeClassBuilder AddVoidMethod(string name, Action<BeeMethodBuilder> configure = null)
            => AddMethod(name, typeof(void), configure);

        /// <summary>
        /// Adds a public constructor.
        /// </summary>
        public BeeClassBuilder AddConstructor(Action<BeeConstructorBuilder> configure = null)
        {
            var constructorBuilder = new BeeConstructorBuilder();
            configure?.Invoke(constructorBuilder);
            _classConfigurator.AddElementBuilder(constructorBuilder.ToConfigurator());
            return this;
        }

        /// <summary>
        /// Adds an auto-property and includes it in a synthesized constructor for dependency injection.
        /// </summary>
        public BeeClassBuilder Inject<TDependency>(string propertyName, string parameterName = null)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException(nameof(propertyName));

            if (_injectedDependencies.Any(x => x.PropertyName == propertyName))
                throw new InvalidOperationException($"Dependency property '{propertyName}' is already registered.");

            AddAutoProperty(propertyName, typeof(TDependency));
            _injectedDependencies.Add((propertyName, typeof(TDependency), parameterName ?? ToParameterName(propertyName)));

            if (!_hasSynthesizedDependencyConstructor)
            {
                _classConfigurator.AddElementBuilder(new DependencyConstructorConfigurator(_injectedDependencies));
                _hasSynthesizedDependencyConstructor = true;
            }

            return this;
        }

        private static string ToParameterName(string propertyName)
        {
            if (propertyName.Length == 1)
                return propertyName.ToLowerInvariant();

            return char.ToLowerInvariant(propertyName[0]) + propertyName.Substring(1);
        }
    }
}
