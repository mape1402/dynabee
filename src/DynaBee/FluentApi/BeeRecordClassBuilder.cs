namespace DynaBee.FluentApi
{
    using DynaBee.Infrastructure;

    /// <summary>
    /// Fluent builder for dynamic record classes.
    /// </summary>
    public sealed class BeeRecordClassBuilder
    {
        private readonly BeeClassBuilder _classBuilder;

        internal BeeRecordClassBuilder(BeeClassBuilder classBuilder)
        {
            _classBuilder = classBuilder ?? throw new ArgumentNullException(nameof(classBuilder));
        }

        public BeeRecordClassBuilder Implements<TInterface>()
        {
            _classBuilder.Implements<TInterface>();
            return this;
        }

        public BeeRecordClassBuilder AddAttribute<TAttribute>(Action<BeeAttributeBuilder> configure)
            where TAttribute : Attribute
        {
            _classBuilder.AddAttribute<TAttribute>(configure);
            return this;
        }

        public BeeRecordClassBuilder AddComponent(string name, BeeType type)
        {
            _classBuilder.AddProperty(name, type, p => p
                .WithGetter(true)
                .WithSetter(true)
                .WithGetterAccess(MethodAccessModifier.Public)
                .WithSetterAccess(MethodAccessModifier.Public));

            return this;
        }

        public BeeRecordClassBuilder AddComponent<T>(string name)
            => AddComponent(name, typeof(T));

        public BeeRecordClassBuilder AddMethod(string name, BeeType returnType, Action<BeeMethodBuilder> configure = null)
        {
            _classBuilder.AddMethod(name, returnType, configure);
            return this;
        }
    }
}