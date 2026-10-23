namespace DynaBee.FluentApi.Generation
{
    using System.Collections.Concurrent;

    /// <summary>
    /// Thread-safe in-memory plan cache intended for dependency injection registration.
    /// </summary>
    public sealed class DynaBeeGenerationPlanCache : IDynaBeeGenerationPlanCache
    {
        private readonly ConcurrentDictionary<string, IAssemblyContext> _contexts = new(StringComparer.Ordinal);

        /// <inheritdoc/>
        public IAssemblyContext GetOrAdd(string fingerprint, Func<IAssemblyContext> factory)
        {
            if (string.IsNullOrWhiteSpace(fingerprint))
                throw new ArgumentException(nameof(fingerprint));

            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            return _contexts.GetOrAdd(fingerprint, _ => factory());
        }

        /// <inheritdoc/>
        public void Clear()
            => _contexts.Clear();
    }
}
