namespace DynaBee.FluentApi.Body
{
    /// <summary>
    /// Represents a method parameter.
    /// </summary>
    public interface IBeeParameter : IBeeValueExpression
    {
        /// <summary>
        /// Gets the parameter name.
        /// </summary>
        string Name { get; }
    }
}
