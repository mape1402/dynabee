namespace DynaBee.FluentApi
{
    /// <summary>
    /// Default context lease implementation.
    /// </summary>
    public sealed class AssemblyContextLease : IAssemblyContextLease
    {
        private IAssemblyContext _context;

        internal AssemblyContextLease(IAssemblyContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc/>
        public IAssemblyContext Context
            => _context ?? throw new ObjectDisposedException(nameof(AssemblyContextLease));

        /// <inheritdoc/>
        public bool IsDisposed => _context == null;

        /// <inheritdoc/>
        public void Dispose()
            => _context = null;
    }
}
