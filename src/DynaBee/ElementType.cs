namespace DynaBee
{
    /// <summary>
    /// Specifies the kind of element that can be dynamically defined within a type.
    /// </summary>
    public enum ElementType
    {
        /// <summary>
        /// Indicates that the element is a property.
        /// </summary>
        Property,

        /// <summary>
        /// Indicates that the element is a method.
        /// </summary>
        Method,

        /// <summary>
        /// Indicates that the element is a field.
        /// </summary>
        Field,

        /// <summary>
        /// Indicates that the element is a constant value.
        /// </summary>
        Constant
    }
}
