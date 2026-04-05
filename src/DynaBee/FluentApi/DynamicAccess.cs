namespace DynaBee.FluentApi
{
    /// <summary>
    /// Runtime helpers to access generated type members without declaring host interfaces.
    /// </summary>
    public static class DynamicAccess
    {
        /// <summary>
        /// Reads a property value from an object instance and casts it to <typeparamref name="T"/>.
        /// </summary>
        public static T GetProperty<T>(object instance, string propertyName)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException(nameof(propertyName));

            var property = instance.GetType().GetProperty(propertyName)
                ?? throw new InvalidOperationException($"Property '{propertyName}' was not found in type '{instance.GetType().FullName}'.");

            var value = property.GetValue(instance);
            return (T)value;
        }

        /// <summary>
        /// Sets a property value in an object instance.
        /// </summary>
        public static void SetProperty(object instance, string propertyName, object value)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException(nameof(propertyName));

            var property = instance.GetType().GetProperty(propertyName)
                ?? throw new InvalidOperationException($"Property '{propertyName}' was not found in type '{instance.GetType().FullName}'.");

            property.SetValue(instance, value);
        }
    }
}