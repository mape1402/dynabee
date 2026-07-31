namespace DynaBee.FluentApi
{
    using DynaBee.FluentApi.Body;
    using System.Reflection.Emit;

    /// <summary>
    /// Fluent builder for an overriding property accessor body.
    /// </summary>
    public sealed class BeePropertyAccessorBuilder
    {
        private Action<ILGenerator> _ilBody;
        private Action<IBeeMethodBodyBuilder> _methodBody;
        private object _constantValue;
        private bool _hasConstantValue;

        /// <summary>
        /// Defines the accessor body using raw IL.
        /// </summary>
        /// <param name="body">IL body callback. The callback must emit a ret opcode.</param>
        /// <returns>The same accessor builder.</returns>
        public BeePropertyAccessorBuilder Emits(Action<ILGenerator> body)
        {
            _ilBody = body ?? throw new ArgumentNullException(nameof(body));
            _methodBody = null;
            _hasConstantValue = false;
            return this;
        }

        /// <summary>
        /// Defines the accessor body using high-level DynaBee body builder operations.
        /// </summary>
        /// <param name="body">Accessor body callback.</param>
        /// <returns>The same accessor builder.</returns>
        public BeePropertyAccessorBuilder EmitsBody(Action<IBeeMethodBodyBuilder> body)
        {
            _methodBody = body ?? throw new ArgumentNullException(nameof(body));
            _ilBody = null;
            _hasConstantValue = false;
            return this;
        }

        /// <summary>
        /// Defines a getter body that returns a constant value.
        /// </summary>
        /// <param name="value">Constant value to return.</param>
        /// <returns>The same accessor builder.</returns>
        public BeePropertyAccessorBuilder ReturnsConstant(object value)
        {
            _constantValue = value;
            _hasConstantValue = true;
            _ilBody = null;
            _methodBody = null;
            return this;
        }

        internal Action<ILGenerator> IlBody => _ilBody;

        internal Action<IBeeMethodBodyBuilder> MethodBody => _methodBody;

        internal object ConstantValue => _constantValue;

        internal bool HasConstantValue => _hasConstantValue;
    }
}
