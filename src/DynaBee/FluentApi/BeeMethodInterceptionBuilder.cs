namespace DynaBee.FluentApi
{
    using DynaBee.FluentApi.Body;
    using System.Reflection;

    /// <summary>
    /// Configures an override that surrounds a base method invocation with interceptor bodies.
    /// </summary>
    public sealed class BeeMethodInterceptionBuilder
    {
        private readonly MethodInfo _baseMethod;
        private Action<IBeeMethodBodyBuilder> _before;
        private Action<IBeeLocal, IBeeMethodBodyBuilder> _after;
        private Action<IBeeMethodBodyBuilder> _afterVoid;
        private Action<IBeeMethodBodyBuilder> _finally;

        internal BeeMethodInterceptionBuilder(MethodInfo baseMethod)
        {
            _baseMethod = baseMethod ?? throw new ArgumentNullException(nameof(baseMethod));
        }

        /// <summary>
        /// Adds work that runs before the base method.
        /// </summary>
        /// <param name="body">Interceptor body.</param>
        /// <returns>The same interception builder.</returns>
        public BeeMethodInterceptionBuilder Before(Action<IBeeMethodBodyBuilder> body)
        {
            _before = body ?? throw new ArgumentNullException(nameof(body));
            return this;
        }

        /// <summary>
        /// Adds work that runs after a non-void base method and receives its result local.
        /// </summary>
        /// <param name="body">Interceptor body.</param>
        /// <returns>The same interception builder.</returns>
        public BeeMethodInterceptionBuilder After(Action<IBeeLocal, IBeeMethodBodyBuilder> body)
        {
            _after = body ?? throw new ArgumentNullException(nameof(body));
            return this;
        }

        /// <summary>
        /// Adds work that runs after a void base method.
        /// </summary>
        /// <param name="body">Interceptor body.</param>
        /// <returns>The same interception builder.</returns>
        public BeeMethodInterceptionBuilder After(Action<IBeeMethodBodyBuilder> body)
        {
            _afterVoid = body ?? throw new ArgumentNullException(nameof(body));
            return this;
        }

        /// <summary>
        /// Adds work that runs whether the base method completes or throws.
        /// </summary>
        /// <param name="body">Interceptor body.</param>
        /// <returns>The same interception builder.</returns>
        public BeeMethodInterceptionBuilder Finally(Action<IBeeMethodBodyBuilder> body)
        {
            _finally = body ?? throw new ArgumentNullException(nameof(body));
            return this;
        }

        internal void Emit(IBeeMethodBodyBuilder body)
        {
            if (_baseMethod.ReturnType == typeof(void))
            {
                Action<IBeeMethodBodyBuilder> invoke = scope =>
                {
                    _before?.Invoke(scope);
                    scope.Evaluate(scope.CallBase());
                    _afterVoid?.Invoke(scope);
                };

                if (_finally != null)
                    body.Try(invoke, finallyBody: _finally);
                else
                    invoke(body);

                body.Return();
                return;
            }

            var result = body.DeclareLocal("__dynabee_interception_result", _baseMethod.ReturnType);
            Action<IBeeMethodBodyBuilder> invokeWithResult = scope =>
            {
                _before?.Invoke(scope);
                scope.Assign(result, scope.CallBase());
                _after?.Invoke(result, scope);
            };

            if (_finally != null)
                body.Try(invokeWithResult, finallyBody: _finally);
            else
                invokeWithResult(body);

            body.Return(result);
        }
    }
}
