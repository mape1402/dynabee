namespace DynaBee.FluentApi
{
    using DynaBee.Infrastructure;
    using System.Linq.Expressions;
    using System.Reflection.Emit;
    using DynaBee.Infrastructure.Configurators;

    /// <summary>
    /// Fluent builder for a dynamic method.
    /// </summary>
    public sealed class BeeMethodBuilder
    {
        private readonly List<(string Name, BeeType Type)> _parameters = new();
        private readonly List<BeeAttribute> _attributes = new();
        private readonly Dictionary<string, object> _metadata = new();
        private Action<ILGenerator> _body;
        private Delegate _lambdaBody;
        private LambdaExpression _expressionBody;
        private bool _isStatic;
        private MethodAccessModifier _accessModifier;

        internal BeeMethodBuilder(string name, BeeType returnType)
        {
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException(nameof(name)) : name;
            ReturnType = returnType;
        }

        /// <summary>
        /// Method name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Return type.
        /// </summary>
        public BeeType ReturnType { get; }

        /// <summary>
        /// Adds one parameter to the method.
        /// </summary>
        public BeeMethodBuilder WithParameter(string name, BeeType parameterType)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));

            _parameters.Add((name, parameterType));
            return this;
        }

        /// <summary>
        /// Adds one parameter to the method.
        /// </summary>
        public BeeMethodBuilder WithParameter<TParameter>(string name)
            => WithParameter(name, typeof(TParameter));

        /// <summary>
        /// Marks this method as static.
        /// </summary>
        public BeeMethodBuilder AsStatic()
        {
            _isStatic = true;
            return this;
        }

        /// <summary>
        /// Sets the method access modifier.
        /// </summary>
        public BeeMethodBuilder WithAccess(MethodAccessModifier accessModifier)
        {
            _accessModifier = accessModifier;
            return this;
        }

        /// <summary>
        /// Adds a custom attribute to the generated method.
        /// </summary>
        public BeeMethodBuilder AddAttribute(BeeAttribute attribute)
        {
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));

            _attributes.Add(attribute);
            return this;
        }

        /// <summary>
        /// Adds a custom attribute to the generated method.
        /// </summary>
        public BeeMethodBuilder AddAttribute<TAttribute>(params object[] constructorArguments)
            where TAttribute : Attribute
            => AddAttribute(BeeAttribute.Of<TAttribute>(constructorArguments));

        /// <summary>
        /// Adds a custom attribute to the generated method using fluent configuration.
        /// </summary>
        public BeeMethodBuilder AddAttribute<TAttribute>(Action<BeeAttributeBuilder> configure)
            where TAttribute : Attribute
        {
            var builder = new BeeAttributeBuilder(typeof(TAttribute));
            configure?.Invoke(builder);
            return AddAttribute(builder.Build());
        }

        /// <summary>
        /// Defines custom method body IL. The callback must emit a ret opcode.
        /// </summary>
        public BeeMethodBuilder Emits(Action<ILGenerator> body)
        {
            _body = body ?? throw new ArgumentNullException(nameof(body));
            _lambdaBody = null;
            _expressionBody = null;
            return this;
        }

        /// <summary>
        /// Defines method logic using a .NET delegate. The delegate parameters
        /// must match method parameters (or include target instance as first argument).
        /// </summary>
        public BeeMethodBuilder EmitsLambda(Delegate lambdaBody)
        {
            _lambdaBody = lambdaBody ?? throw new ArgumentNullException(nameof(lambdaBody));
            _body = null;
            _expressionBody = null;
            return this;
        }

        /// <summary>
        /// Defines method logic using a lambda with access to the generated instance as first parameter.
        /// </summary>
        public BeeMethodBuilder EmitsLambdaWithSelf<TSelf, TResult>(Func<TSelf, TResult> lambdaBody)
            => EmitsLambda(lambdaBody);

        /// <summary>
        /// Defines method logic using a lambda with access to the generated instance and one method argument.
        /// </summary>
        public BeeMethodBuilder EmitsLambdaWithSelf<TSelf, T1, TResult>(Func<TSelf, T1, TResult> lambdaBody)
            => EmitsLambda(lambdaBody);

        /// <summary>
        /// Defines method logic using a lambda with access to the generated instance and two method arguments.
        /// </summary>
        public BeeMethodBuilder EmitsLambdaWithSelf<TSelf, T1, T2, TResult>(Func<TSelf, T1, T2, TResult> lambdaBody)
            => EmitsLambda(lambdaBody);

        /// <summary>
        /// Defines method logic from an expression tree that is translated to IL.
        /// </summary>
        public BeeMethodBuilder EmitsExpression(LambdaExpression expressionBody)
        {
            _expressionBody = expressionBody ?? throw new ArgumentNullException(nameof(expressionBody));
            _body = null;
            _lambdaBody = null;
            return this;
        }

        /// <summary>
        /// Defines method logic from a strongly typed expression tree.
        /// </summary>
        public BeeMethodBuilder EmitsExpression<TDelegate>(Expression<TDelegate> expressionBody)
            where TDelegate : Delegate
            => EmitsExpression((LambdaExpression)expressionBody);

        /// <summary>
        /// Defines method logic as a function that receives an injected dependency by property name.
        /// </summary>
        public BeeMethodBuilder EmitsInjectedLambda<TDependency, TResult>(string dependencyProperty, Func<TDependency, TResult> lambdaBody)
        {
            if (string.IsNullOrWhiteSpace(dependencyProperty))
                throw new ArgumentException(nameof(dependencyProperty));

            if (lambdaBody == null)
                throw new ArgumentNullException(nameof(lambdaBody));

            return EmitsLambda((Func<object, TResult>)(self =>
                lambdaBody(DynamicAccess.GetProperty<TDependency>(self, dependencyProperty))));
        }

        /// <summary>
        /// Defines method logic as a function that receives an injected dependency and one method argument.
        /// </summary>
        public BeeMethodBuilder EmitsInjectedLambda<TDependency, T1, TResult>(
            string dependencyProperty,
            Func<TDependency, T1, TResult> lambdaBody)
        {
            if (string.IsNullOrWhiteSpace(dependencyProperty))
                throw new ArgumentException(nameof(dependencyProperty));

            if (lambdaBody == null)
                throw new ArgumentNullException(nameof(lambdaBody));

            return EmitsLambda((Func<object, T1, TResult>)((self, arg1) =>
                lambdaBody(DynamicAccess.GetProperty<TDependency>(self, dependencyProperty), arg1)));
        }

        /// <summary>
        /// Stores metadata for this generated method.
        /// </summary>
        public BeeMethodBuilder WithMetadata(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));

            _metadata[key] = value ?? throw new ArgumentNullException(nameof(value));
            return this;
        }

        /// <summary>
        /// Stores strongly typed metadata for this generated method.
        /// </summary>
        public BeeMethodBuilder WithMetadata<T>(BeeMetadataKey<T> key, T value)
            => WithMetadata(key.Name, value);

        internal MethodConfigurator ToConfigurator()
            => new MethodConfigurator(Name, ReturnType, _parameters, _body, _lambdaBody, _expressionBody, _isStatic, _accessModifier, _attributes, _metadata);
    }
}
