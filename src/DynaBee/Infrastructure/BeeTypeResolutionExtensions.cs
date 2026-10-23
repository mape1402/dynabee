namespace DynaBee.Infrastructure
{
    using DynaBee.Infrastructure.ContextBuilders;

    /// <summary>
    /// Resolves fluent type references while a generated type is being emitted.
    /// </summary>
    internal static class BeeTypeResolutionExtensions
    {
        public static Type Resolve(this BeeType beeType, ITypeContextBuilder typeContextBuilder)
        {
            if (typeContextBuilder == null)
                throw new ArgumentNullException(nameof(typeContextBuilder));

            if (beeType.IsReference)
                return typeContextBuilder.AssemblyBuilderContext.GetTypeBuilder((string)beeType).TypeBuilder;

            if (beeType.IsGenericParameter)
            {
                if (typeContextBuilder is not TypeContextBuilder concreteBuilder ||
                    !concreteBuilder.TryGetGenericParameter(beeType.GenericParameterName, out var parameter))
                {
                    throw new InvalidOperationException(
                        $"Generic parameter '{beeType.GenericParameterName}' is not declared by generated type '{typeContextBuilder.Name}'.");
                }

                return parameter;
            }

            return beeType;
        }
    }
}
