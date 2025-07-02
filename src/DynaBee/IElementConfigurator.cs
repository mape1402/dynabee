namespace DynaBee
{
    /// <summary>
    /// Defines a contract for configuring a specific element
    /// (such as a property, method, field, or constant)
    /// within a dynamic type.
    /// </summary>
    public interface IElementConfigurator
    {
        /// <summary>
        /// Applies the element configuration to the specified <see cref="ITypeContextBuilder"/>.
        /// This method defines how the element should be added to the dynamic type.
        /// </summary>
        /// <param name="typeContextBuilder">
        /// The type context builder to which the element configuration will be applied.
        /// </param>
        void Configure(ITypeContextBuilder typeContextBuilder);
    }
}
