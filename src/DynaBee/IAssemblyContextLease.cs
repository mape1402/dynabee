namespace DynaBee
{
    /// <summary>
    /// Owns a generated assembly context that may be released when no longer needed.
    /// </summary>
    public interface IAssemblyContextLease : IDisposable
    {
        /// <summary>
        /// Gets the leased assembly context.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown after the lease is disposed.</exception>
        IAssemblyContext Context { get; }

        /// <summary>
        /// Gets whether the lease has released its context reference.
        /// </summary>
        bool IsDisposed { get; }
    }
}
