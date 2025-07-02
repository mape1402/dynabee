namespace DynaBee.Tools
{
    /// <summary>
    /// Represents an immutable value container that can be set only once 
    /// if the value passes a specified validation function.
    /// </summary>
    /// <typeparam name="T">The type of the contained value.</typeparam>
    public class Immutable<T> : IValidableArgument
    {
        private readonly Func<T, bool> _isValid;

        /// <summary>
        /// Initializes a new instance of the <see cref="Immutable{T}"/> class 
        /// using a default validation rule that considers the default value of T as invalid.
        /// </summary>
        public Immutable() : this(v => !v.Equals(default(T))) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Immutable{T}"/> class 
        /// with a custom validation function.
        /// </summary>
        /// <param name="isValid">A function to validate whether a value is acceptable.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="isValid"/> is null.</exception>
        public Immutable(Func<T, bool> isValid)
        {
            _isValid = isValid ?? throw new ArgumentNullException(nameof(isValid));
        }

        /// <summary>
        /// Gets the stored value. The value is only available after being successfully set.
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool IsValid()
            => _isValid(Value);

        /// <summary>
        /// Sets the value if it passes validation and has not been set before.
        /// </summary>
        /// <param name="value">The value to assign.</param>
        /// <exception cref="ArgumentException">Thrown if the value is invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the value has already been set.</exception>
        public void Set(T value)
        {
            if (!_isValid(value))
                throw new ArgumentException($"Invalid value for +_beeType '{typeof(T).Name}'.");

            if (IsValid())
                throw new InvalidOperationException($"The value of +_beeType '{typeof(T).Name}' has already been set and is immutable.");

            Value = value;
        }

        /// <summary>
        /// Implicitly converts an <see cref="Immutable{T}"/> instance to its underlying value of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="v">The <see cref="Immutable{T}"/> instance to convert.</param>
        public static implicit operator T(Immutable<T> v) => v.Value;
    }
}