namespace DynaBee.FluentApi
{
    /// <summary>
    /// Specifies a CLR operator method exposed by a generated type.
    /// </summary>
    public enum BeeOperatorKind
    {
        /// <summary>Addition operator.</summary>
        Addition,
        /// <summary>Subtraction operator.</summary>
        Subtraction,
        /// <summary>Multiplication operator.</summary>
        Multiplication,
        /// <summary>Division operator.</summary>
        Division,
        /// <summary>Equality operator.</summary>
        Equality,
        /// <summary>Inequality operator.</summary>
        Inequality,
        /// <summary>Implicit conversion operator.</summary>
        ImplicitConversion,
        /// <summary>Explicit conversion operator.</summary>
        ExplicitConversion
    }
}
