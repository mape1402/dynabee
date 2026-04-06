namespace DynaBee.FluentApi.Diagnostics
{
    using System.Reflection;
    using System.Text.Json;

    /// <summary>
    /// Diagnostic helpers for generated assemblies and types.
    /// </summary>
    public static class DynaBeeDiagnosticsExtensions
    {
        /// <summary>
        /// Creates a rich diagnostic snapshot for a generated assembly context.
        /// </summary>
        public static AssemblyDiagnostic GetDiagnostics(this IAssemblyContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var types = context
                .Find(_ => true)
                .Select(x => BuildTypeDiagnostic(x.ClrType))
                .OrderBy(x => x.Name, StringComparer.Ordinal)
                .ToArray();

            return new AssemblyDiagnostic
            {
                Name = context.Assembly.GetName().Name ?? context.Name,
                Version = context.Assembly.GetName().Version?.ToString() ?? "0.0.0.0",
                Types = types
            };
        }

        /// <summary>
        /// Serializes diagnostics to JSON.
        /// </summary>
        public static string ToDiagnosticsJson(this IAssemblyContext context, bool indented = true)
        {
            var model = context.GetDiagnostics();
            var options = new JsonSerializerOptions { WriteIndented = indented };
            return JsonSerializer.Serialize(model, options);
        }

        private static TypeDiagnostic BuildTypeDiagnostic(Type type)
        {
            var members = new List<MemberDiagnostic>();

            var bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            foreach (var field in type.GetFields(bindingFlags))
            {
                members.Add(new MemberDiagnostic
                {
                    Name = field.Name,
                    Kind = "Field",
                    Signature = $"{field.FieldType.Name} {field.Name}",
                    AccessModifier = GetFieldAccess(field),
                    Attributes = field.GetCustomAttributesData().Select(a => a.AttributeType.Name).ToArray()
                });
            }

            foreach (var property in type.GetProperties(bindingFlags))
            {
                var getter = property.GetGetMethod(true);
                var setter = property.GetSetMethod(true);
                var accessorSignature = $"get:{(getter != null ? GetMethodAccess(getter) : "-")} set:{(setter != null ? GetMethodAccess(setter) : "-")}";

                members.Add(new MemberDiagnostic
                {
                    Name = property.Name,
                    Kind = "Property",
                    Signature = $"{property.PropertyType.Name} {property.Name} ({accessorSignature})",
                    AccessModifier = accessorSignature,
                    Attributes = property.GetCustomAttributesData().Select(a => a.AttributeType.Name).ToArray()
                });
            }

            foreach (var method in type.GetMethods(bindingFlags).Where(m => !m.IsSpecialName))
            {
                var parameters = string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                members.Add(new MemberDiagnostic
                {
                    Name = method.Name,
                    Kind = "Method",
                    Signature = $"{method.ReturnType.Name} {method.Name}({parameters})",
                    AccessModifier = GetMethodAccess(method),
                    Attributes = method.GetCustomAttributesData().Select(a => a.AttributeType.Name).ToArray()
                });
            }

            return new TypeDiagnostic
            {
                Name = type.Name,
                FullName = type.FullName ?? type.Name,
                Kind = GetTypeKind(type),
                AccessModifier = GetTypeAccess(type),
                Attributes = type.GetCustomAttributesData().Select(a => a.AttributeType.Name).ToArray(),
                Members = members.OrderBy(x => x.Name, StringComparer.Ordinal).ToArray()
            };
        }

        private static string GetTypeKind(Type type)
        {
            if (type.IsInterface) return "Interface";
            if (type.IsEnum) return "Enum";
            if (type.IsValueType) return "Struct";
            return "Class";
        }

        private static string GetTypeAccess(Type type)
        {
            if (type.IsPublic || type.IsNestedPublic) return "Public";
            if (type.IsNotPublic || type.IsNestedAssembly) return "Internal";
            if (type.IsNestedPrivate) return "Private";
            if (type.IsNestedFamily) return "Protected";
            if (type.IsNestedFamORAssem) return "ProtectedInternal";
            if (type.IsNestedFamANDAssem) return "PrivateProtected";
            return "Unknown";
        }

        private static string GetMethodAccess(MethodInfo method)
        {
            if (method.IsPublic) return "Public";
            if (method.IsPrivate) return "Private";
            if (method.IsAssembly) return "Internal";
            if (method.IsFamily) return "Protected";
            if (method.IsFamilyOrAssembly) return "ProtectedInternal";
            if (method.IsFamilyAndAssembly) return "PrivateProtected";
            return "Unknown";
        }

        private static string GetFieldAccess(FieldInfo field)
        {
            if (field.IsPublic) return "Public";
            if (field.IsPrivate) return "Private";
            if (field.IsAssembly) return "Internal";
            if (field.IsFamily) return "Protected";
            if (field.IsFamilyOrAssembly) return "ProtectedInternal";
            if (field.IsFamilyAndAssembly) return "PrivateProtected";
            return "Unknown";
        }
    }
}