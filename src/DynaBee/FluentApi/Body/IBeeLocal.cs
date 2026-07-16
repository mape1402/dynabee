namespace DynaBee.FluentApi.Body
{
    /// <summary>
    /// Represents a declared local variable.
    /// </summary>
    public interface IBeeLocal : IBeeAssignableExpression
    {
        /// <summary>
        /// Gets the local name.
        /// </summary>
        string Name { get; }
    }
}
