namespace DynaBee
{
    using System.Reflection.Emit;

    /// <summary>
    /// Represents a delegate that applies custom configuration logic to a given <see cref="ITypeContextBuilder"/>.
    /// </summary>
    /// <param name="typeContextBuilder">
    /// The <see cref="TypeBuilder"/> instance to configure.
    /// </param>
    public delegate void ElementBuilderAction(ITypeContextBuilder typeContextBuilder);
}
