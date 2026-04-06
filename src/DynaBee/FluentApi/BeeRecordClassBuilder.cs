namespace DynaBee.FluentApi
{
    using DynaBee.Infrastructure;
    using DynaBee.Infrastructure.Configurators;

    /// <summary>
    /// Fluent builder for dynamic record classes.
    /// </summary>
    public sealed class BeeRecordClassBuilder
    {
        private readonly BeeClassBuilder _classBuilder;
        private readonly List<(string Name, BeeType Type)> _components = new();

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

            _components.Add((name, type));
            return this;
        }

        public BeeRecordClassBuilder AddComponent<T>(string name)
            => AddComponent(name, typeof(T));

        public BeeRecordClassBuilder AddMethod(string name, BeeType returnType, Action<BeeMethodBuilder> configure = null)
        {
            _classBuilder.AddMethod(name, returnType, configure);
            return this;
        }

        internal void FinalizeRecord()
        {
            if (_components.Count == 0)
                return;

            var componentNames = _components.Select(x => x.Name).ToArray();

            _classBuilder.AddMethod("Equals", typeof(bool), m => m
                .WithParameter<object>("other")
                .WithAccess(MethodAccessModifier.Public)
                .EmitsLambda((Func<object, object, bool>)((self, other) =>
                    RecordSemanticsHelper.EqualsByProperties(self, other, componentNames))));

            _classBuilder.AddMethod("GetHashCode", typeof(int), m => m
                .WithAccess(MethodAccessModifier.Public)
                .EmitsLambda((Func<object, int>)(self =>
                    RecordSemanticsHelper.ComputeHashCode(self, componentNames))));

            _classBuilder.AddMethod("ToString", typeof(string), m => m
                .WithAccess(MethodAccessModifier.Public)
                .EmitsLambda((Func<object, string>)(self =>
                    RecordSemanticsHelper.ToRecordString(self, componentNames))));

            _classBuilder.AddElementConfigurator(new RecordDeconstructConfigurator(_components));
        }
    }
}
