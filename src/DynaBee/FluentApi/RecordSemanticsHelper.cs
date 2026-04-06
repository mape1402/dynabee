namespace DynaBee.FluentApi
{
    internal static class RecordSemanticsHelper
    {
        public static bool EqualsByProperties(object self, object other, string[] propertyNames)
        {
            if (ReferenceEquals(self, other))
                return true;

            if (self == null || other == null)
                return false;

            var selfType = self.GetType();
            if (selfType != other.GetType())
                return false;

            foreach (var name in propertyNames)
            {
                var property = selfType.GetProperty(name)
                    ?? throw new InvalidOperationException($"Property '{name}' was not found in type '{selfType.FullName}'.");

                var left = property.GetValue(self);
                var right = property.GetValue(other);
                if (!Equals(left, right))
                    return false;
            }

            return true;
        }

        public static int ComputeHashCode(object self, string[] propertyNames)
        {
            if (self == null)
                return 0;

            var hash = new HashCode();
            var selfType = self.GetType();
            foreach (var name in propertyNames)
            {
                var property = selfType.GetProperty(name)
                    ?? throw new InvalidOperationException($"Property '{name}' was not found in type '{selfType.FullName}'.");

                hash.Add(property.GetValue(self));
            }

            return hash.ToHashCode();
        }

        public static string ToRecordString(object self, string[] propertyNames)
        {
            if (self == null)
                return string.Empty;

            var selfType = self.GetType();
            var values = new List<string>(propertyNames.Length);

            foreach (var name in propertyNames)
            {
                var property = selfType.GetProperty(name)
                    ?? throw new InvalidOperationException($"Property '{name}' was not found in type '{selfType.FullName}'.");

                var value = property.GetValue(self);
                values.Add($"{name} = {value}");
            }

            return $"{selfType.Name} {{ {string.Join(", ", values)} }}";
        }
    }
}