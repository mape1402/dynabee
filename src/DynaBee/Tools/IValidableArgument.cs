namespace DynaBee.Tools
{
    /// <summary>
    /// Defines a contract for an argument that can be validated.
    /// </summary>
    public interface IValidableArgument
    {
        /// <summary>
        /// Determines whether the current value of the argument is valid.
        /// </summary>
        /// <returns>True if the argument is valid; otherwise, false.</returns>
        bool IsValid();
    }
}
