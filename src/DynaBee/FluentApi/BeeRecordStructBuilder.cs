namespace DynaBee.FluentApi
{
    using DynaBee.Infrastructure;
    using DynaBee.Infrastructure.Configurators;

    /// <summary>
    /// Fluent builder for dynamic record structs.
    /// </summary>
    public sealed class BeeRecordStructBuilder
    {
        private readonly BeeStructBuilder _structBuilder;
        private readonly List<(string Name, BeeType Type)> _components = new();

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

            _components.Add((name, type));
            return this;
        }

        public BeeRecordStructBuilder AddComponent<T>(string name)
            => AddComponent(name, typeof(T));

        public BeeRecordStructBuilder AddMethod(string name, BeeType returnType, Action<BeeMethodBuilder> configure = null)
        {
            _structBuilder.AddMethod(name, returnType, configure);
            return this;
        }

        internal void FinalizeRecord()
        {
            if (_components.Count == 0)
                return;

            var componentNames = _components.Select(x => x.Name).ToArray();

            _structBuilder.AddMethod("Equals", typeof(bool), m => m
                .WithParameter<object>("other")
                .WithAccess(MethodAccessModifier.Public)
                .EmitsLambda((Func<object, object, bool>)((self, other) =>
                    RecordSemanticsHelper.EqualsByProperties(self, other, componentNames))));

            _structBuilder.AddMethod("GetHashCode", typeof(int), m => m
                .WithAccess(MethodAccessModifier.Public)
                .EmitsLambda((Func<object, int>)(self =>
                    RecordSemanticsHelper.ComputeHashCode(self, componentNames))));

            _structBuilder.AddMethod("ToString", typeof(string), m => m
                .WithAccess(MethodAccessModifier.Public)
                .EmitsLambda((Func<object, string>)(self =>
                    RecordSemanticsHelper.ToRecordString(self, componentNames))));

            _structBuilder.AddElementConfigurator(new RecordDeconstructConfigurator(_components));
        }
    }
}
