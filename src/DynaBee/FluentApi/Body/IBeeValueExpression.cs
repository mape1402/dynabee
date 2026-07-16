namespace DynaBee.FluentApi.Body
{
    /// <summary>
    /// Represents a value expression that can be loaded by a generated method body.
    /// </summary>
    public interface IBeeValueExpression
    {
        /// <summary>
        /// Gets the expression value type.
        /// </summary>
        Type Type { get; }
    }
}
