namespace DynaBee.Infrastructure
{
    using DynaBee.Tools;

    /// <summary>
    /// Represents a collection of strongly typed arguments used to define metadata for a dynamically generated class.
    /// </summary>
    internal class ClassArguments : BaseArguments
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassArguments"/> class
        /// with default validation rules for its arguments.
        /// </summary>
        public ClassArguments()
        {
            Arguments.Add(nameof(Name), new Immutable<string>(v => !string.IsNullOrWhiteSpace(v)));
            Arguments.Add(nameof(AccessModifier), new Immutable<ClassAccessModifier>());
        }

        /// <summary>
        /// Gets or sets the name of the class.
        /// </summary>
        public string Name
        {
            get => Get<string>();
            set => Set(value);
        }

        /// <summary>
        /// Gets or sets the access modifier for the class definition.
        /// </summary>
        public ClassAccessModifier AccessModifier
        {
            get => Get<ClassAccessModifier>();
            set => Set(value);
        }

    }

}
