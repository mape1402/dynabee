namespace DynaBee.Infrastructure.Configurators
{
    using System.Collections.Concurrent;

    /// <summary>
    /// Registry for runtime delegates used by generated methods implemented through lambdas.
    /// </summary>
    public static class LambdaMethodRegistry
    {
        private static readonly ConcurrentDictionary<int, Delegate> Delegates = new();
        private static int _nextId;

        /// <summary>
        /// Registers a delegate and returns a unique identifier.
        /// </summary>
        public static int Register(Delegate methodDelegate)
        {
            if (methodDelegate == null)
                throw new ArgumentNullException(nameof(methodDelegate));

            var id = Interlocked.Increment(ref _nextId);
            Delegates.TryAdd(id, methodDelegate);
            return id;
        }

        /// <summary>
        /// Invokes a previously registered delegate.
        /// </summary>
        /// <param name="id">Delegate identifier returned by <see cref="Register"/>.</param>
        /// <param name="instance">Target instance of the generated type, if needed.</param>
        /// <param name="args">Method arguments.</param>
        /// <returns>The delegate result.</returns>
        public static object Invoke(int id, object instance, object[] args)
        {
            if (!Delegates.TryGetValue(id, out var methodDelegate))
                throw new InvalidOperationException($"Lambda delegate with id '{id}' was not found.");

            args ??= Array.Empty<object>();
            var parameters = methodDelegate.Method.GetParameters();

            object[] invocationArgs;
            if (parameters.Length == args.Length)
            {
                invocationArgs = args;
            }
            else if (parameters.Length == args.Length + 1)
            {
                invocationArgs = new object[args.Length + 1];
                invocationArgs[0] = instance;
                Array.Copy(args, 0, invocationArgs, 1, args.Length);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Delegate signature does not match generated method. Delegate parameters: {parameters.Length}, method parameters: {args.Length}.");
            }

            return methodDelegate.DynamicInvoke(invocationArgs);
        }
    }
}
