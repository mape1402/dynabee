namespace DynaBee.FluentApi
{
    using DynaBee.Infrastructure;

    /// <summary>
    /// Fluent builder for dynamic record structs.
    /// </summary>
    public sealed class BeeRecordStructBuilder
    {
        private readonly BeeStructBuilder _structBuilder;

        internal BeeRecordStructBuilder(BeeStructBuilder structBuilder)
        {
            _structBuilder = structBuilder ?? throw new ArgumentNullException(nameof(structBuilder));
        }

        public BeeRecordStructBuilder Implements<TInterface>()
        {
            _structBuilder.Implements<TInterface>();
            return this;
        }

        public BeeRecordStructBuilder AddAttribute<TAttribute>(Action<BeeAttributeBuilder> configure)
            where TAttribute : Attribute
        {
            _structBuilder.AddAttribute<TAttribute>(configure);
            return this;
        }

        public BeeRecordStructBuilder AddComponent(string name, BeeType type)
        {
            _structBuilder.AddProperty(name, type, p => p
                .WithGetter(true)
                .WithSetter(true)
                .WithGetterAccess(MethodAccessModifier.Public)
                .WithSetterAccess(MethodAccessModifier.Public));

            return this;
        }

        public BeeRecordStructBuilder AddComponent<T>(string name)
            => AddComponent(name, typeof(T));

        public BeeRecordStructBuilder AddMethod(string name, BeeType returnType, Action<BeeMethodBuilder> configure = null)
        {
            _structBuilder.AddMethod(name, returnType, configure);
            return this;
        }
    }
}