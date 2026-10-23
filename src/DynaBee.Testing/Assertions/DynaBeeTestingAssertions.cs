namespace DynaBee.Testing.Assertions;

using System.Reflection;

/// <summary>
/// Assertion extensions for DynaBee test diagnostics.
/// </summary>
public static class DynaBeeDiagnosticAssertions
{
    /// <summary>
    /// Asserts that no diagnostics were produced.
    /// </summary>
    /// <param name="diagnostics">Diagnostics to inspect.</param>
    public static void ShouldBeEmpty(this IReadOnlyCollection<DynaBeeTestDiagnostic> diagnostics)
    {
        if (diagnostics == null)
            throw new ArgumentNullException(nameof(diagnostics));

        if (diagnostics.Count == 0)
            return;

        throw new InvalidOperationException("Expected no DynaBee diagnostics, but found: " + string.Join(Environment.NewLine, diagnostics));
    }

    /// <summary>
    /// Asserts that diagnostics contain no error-severity entries.
    /// </summary>
    /// <param name="diagnostics">Diagnostics to inspect.</param>
    public static void ShouldHaveNoErrors(this IReadOnlyCollection<DynaBeeTestDiagnostic> diagnostics)
    {
        if (diagnostics == null)
            throw new ArgumentNullException(nameof(diagnostics));

        var errors = diagnostics.Where(x => x.Severity == DynaBeeTestDiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException("Expected no DynaBee errors, but found: " + string.Join(Environment.NewLine, errors));
    }

    /// <summary>
    /// Asserts that diagnostics contain no warning-severity entries.
    /// </summary>
    /// <param name="diagnostics">Diagnostics to inspect.</param>
    public static void ShouldHaveNoWarnings(this IReadOnlyCollection<DynaBeeTestDiagnostic> diagnostics)
    {
        if (diagnostics == null)
            throw new ArgumentNullException(nameof(diagnostics));

        var warnings = diagnostics.Where(x => x.Severity == DynaBeeTestDiagnosticSeverity.Warning).ToArray();
        if (warnings.Length != 0)
            throw new InvalidOperationException("Expected no DynaBee warnings, but found: " + string.Join(Environment.NewLine, warnings));
    }
}

/// <summary>
/// Assertion extensions for generated assemblies.
/// </summary>
public static class DynaBeeGeneratedAssemblyAssertions
{
    /// <summary>
    /// Asserts that a generated assembly context exists.
    /// </summary>
    /// <param name="assembly">Generated assembly context.</param>
    /// <returns>The same assembly context.</returns>
    public static IAssemblyContext ShouldExist(this IAssemblyContext assembly)
        => assembly ?? throw new InvalidOperationException("Expected generated assembly to exist, but it was null.");

    /// <summary>
    /// Asserts that a generated assembly contains a type by logical DynaBee name.
    /// </summary>
    /// <param name="assembly">Generated assembly context.</param>
    /// <param name="typeName">Logical generated type name.</param>
    /// <returns>The matching generated type context.</returns>
    public static ITypeContext ShouldContainType(this IAssemblyContext assembly, string typeName)
    {
        assembly.ShouldExist();

        try
        {
            return assembly.Find(typeName);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Expected generated assembly '{assembly.Name}' to contain type '{typeName}'.", ex);
        }
    }
}

/// <summary>
/// Assertion extensions for generated types.
/// </summary>
public static class DynaBeeGeneratedTypeAssertions
{
    /// <summary>
    /// Asserts that a generated type implements an interface.
    /// </summary>
    /// <param name="typeContext">Generated type context.</param>
    /// <param name="interfaceType">Expected interface type or generic interface definition.</param>
    /// <returns>The same generated type context.</returns>
    public static ITypeContext ShouldImplement(this ITypeContext typeContext, Type interfaceType)
    {
        if (typeContext == null)
            throw new ArgumentNullException(nameof(typeContext));

        typeContext.ClrType.ShouldImplement(interfaceType);
        return typeContext;
    }

    /// <summary>
    /// Asserts that a CLR type implements an interface.
    /// </summary>
    /// <param name="type">Generated CLR type.</param>
    /// <param name="interfaceType">Expected interface type or generic interface definition.</param>
    /// <returns>The same CLR type.</returns>
    public static Type ShouldImplement(this Type type, Type interfaceType)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        if (interfaceType == null)
            throw new ArgumentNullException(nameof(interfaceType));

        var implemented = type.GetInterfaces().Any(x => MatchesType(x, interfaceType));
        if (!implemented)
            throw new InvalidOperationException($"Expected generated type '{type.FullName}' to implement '{interfaceType.FullName}'.");

        return type;
    }

    /// <summary>
    /// Asserts that a generated type inherits from a base type.
    /// </summary>
    /// <param name="typeContext">Generated type context.</param>
    /// <param name="baseType">Expected base type or generic base type definition.</param>
    /// <returns>The same generated type context.</returns>
    public static ITypeContext ShouldInheritFrom(this ITypeContext typeContext, Type baseType)
    {
        if (typeContext == null)
            throw new ArgumentNullException(nameof(typeContext));

        typeContext.ClrType.ShouldInheritFrom(baseType);
        return typeContext;
    }

    /// <summary>
    /// Asserts that a CLR type inherits from a base type.
    /// </summary>
    /// <param name="type">Generated CLR type.</param>
    /// <param name="baseType">Expected base type or generic base type definition.</param>
    /// <returns>The same CLR type.</returns>
    public static Type ShouldInheritFrom(this Type type, Type baseType)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        if (baseType == null)
            throw new ArgumentNullException(nameof(baseType));

        for (var current = type.BaseType; current != null; current = current.BaseType)
        {
            if (MatchesType(current, baseType))
                return type;
        }

        throw new InvalidOperationException($"Expected generated type '{type.FullName}' to inherit from '{baseType.FullName}'.");
    }

    /// <summary>
    /// Asserts that a generated type has a public constructor matching the requested parameter types.
    /// </summary>
    /// <param name="typeContext">Generated type context.</param>
    /// <param name="parameterTypes">Expected constructor parameter types.</param>
    /// <returns>The matching constructor.</returns>
    public static ConstructorInfo ShouldHaveConstructor(this ITypeContext typeContext, params Type[] parameterTypes)
    {
        if (typeContext == null)
            throw new ArgumentNullException(nameof(typeContext));

        return typeContext.ClrType.ShouldHaveConstructor(parameterTypes);
    }

    /// <summary>
    /// Asserts that a CLR type has a public constructor matching the requested parameter types.
    /// </summary>
    /// <param name="type">Generated CLR type.</param>
    /// <param name="parameterTypes">Expected constructor parameter types.</param>
    /// <returns>The matching constructor.</returns>
    public static ConstructorInfo ShouldHaveConstructor(this Type type, params Type[] parameterTypes)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        parameterTypes ??= Type.EmptyTypes;
        var constructor = type.GetConstructor(parameterTypes);
        if (constructor == null)
        {
            throw new InvalidOperationException(
                $"Expected generated type '{type.FullName}' to have constructor ({string.Join(", ", parameterTypes.Select(x => x.Name))}).");
        }

        return constructor;
    }

    /// <summary>
    /// Asserts that a generated type exposes a method with an exact signature.
    /// </summary>
    /// <param name="typeContext">Generated type context.</param>
    /// <param name="name">Method name.</param>
    /// <param name="parameterTypes">Expected parameter types.</param>
    /// <returns>The matching method.</returns>
    public static MethodInfo ShouldHaveMethod(this ITypeContext typeContext, string name, params Type[] parameterTypes)
        => (typeContext ?? throw new ArgumentNullException(nameof(typeContext))).ClrType.ShouldHaveMethod(name, parameterTypes);

    /// <summary>
    /// Asserts that a CLR type exposes a method with an exact signature.
    /// </summary>
    /// <param name="type">Generated CLR type.</param>
    /// <param name="name">Method name.</param>
    /// <param name="parameterTypes">Expected parameter types.</param>
    /// <returns>The matching method.</returns>
    public static MethodInfo ShouldHaveMethod(this Type type, string name, params Type[] parameterTypes)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        var method = type.GetMethod(name, parameterTypes ?? Type.EmptyTypes);
        return method ?? throw new InvalidOperationException(
            $"Expected generated type '{type.FullName}' to have method '{name}({string.Join(", ", (parameterTypes ?? Type.EmptyTypes).Select(x => x.Name))})'.");
    }

    /// <summary>
    /// Asserts that a generated type exposes a property with the requested type.
    /// </summary>
    /// <param name="typeContext">Generated type context.</param>
    /// <param name="name">Property name.</param>
    /// <param name="propertyType">Expected property type.</param>
    /// <returns>The matching property.</returns>
    public static PropertyInfo ShouldHaveProperty(this ITypeContext typeContext, string name, Type propertyType)
        => (typeContext ?? throw new ArgumentNullException(nameof(typeContext))).ClrType.ShouldHaveProperty(name, propertyType);

    /// <summary>
    /// Asserts that a CLR type exposes a property with the requested type.
    /// </summary>
    /// <param name="type">Generated CLR type.</param>
    /// <param name="name">Property name.</param>
    /// <param name="propertyType">Expected property type.</param>
    /// <returns>The matching property.</returns>
    public static PropertyInfo ShouldHaveProperty(this Type type, string name, Type propertyType)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        if (propertyType == null)
            throw new ArgumentNullException(nameof(propertyType));

        var property = type.GetProperty(name);
        if (property == null || property.PropertyType != propertyType)
            throw new InvalidOperationException($"Expected generated type '{type.FullName}' to have property '{name}' of type '{propertyType.FullName}'.");

        return property;
    }

    /// <summary>
    /// Asserts that a type has the requested generic arguments.
    /// </summary>
    /// <param name="type">Type to inspect.</param>
    /// <param name="genericArguments">Expected generic arguments.</param>
    /// <returns>The same type.</returns>
    public static Type ShouldHaveGenericArguments(this Type type, params Type[] genericArguments)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        genericArguments ??= Type.EmptyTypes;
        var actual = type.IsGenericType ? type.GetGenericArguments() : Type.EmptyTypes;
        if (!actual.SequenceEqual(genericArguments))
        {
            throw new InvalidOperationException(
                $"Expected type '{type.FullName}' to have generic arguments [{string.Join(", ", genericArguments.Select(x => x.Name))}], but found [{string.Join(", ", actual.Select(x => x.Name))}].");
        }

        return type;
    }

    private static bool MatchesType(Type actual, Type expected)
    {
        if (actual == expected)
            return true;

        return expected.IsGenericTypeDefinition && actual.IsGenericType && actual.GetGenericTypeDefinition() == expected;
    }
}

/// <summary>
/// Assertion extensions for generated source snapshots.
/// </summary>
public static class DynaBeeGeneratedSourceAssertions
{
    /// <summary>
    /// Asserts that a named generated source exactly matches expected text.
    /// </summary>
    /// <param name="result">Generation result.</param>
    /// <param name="path">Generated source path.</param>
    /// <param name="expected">Expected source text.</param>
    public static void ShouldMatchSource(this DynaBeeTestGenerationResult result, string path, string expected)
    {
        if (result == null)
            throw new ArgumentNullException(nameof(result));

        if (!result.GeneratedSources.TryGetValue(path, out var actual))
            throw new InvalidOperationException($"Generated source '{path}' was not found.");

        if (!string.Equals(expected, actual, StringComparison.Ordinal))
            throw new InvalidOperationException($"Generated source '{path}' did not match the expected snapshot.");
    }
}
