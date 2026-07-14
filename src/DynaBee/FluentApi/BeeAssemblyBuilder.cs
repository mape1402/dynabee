namespace DynaBee.FluentApi
{
    using DynaBee.Infrastructure;
    using DynaBee.Infrastructure.Configurators;
    using System.Collections.Concurrent;

    /// <summary>
    /// Fluent builder for a dynamic assembly.
    /// </summary>
    public sealed class BeeAssemblyBuilder : IBeeAssemblyBuilder
    {
        private static readonly ConcurrentDictionary<string, IAssemblyContext> Cache = new(StringComparer.Ordinal);

        private readonly AssemblyConfigurator _assemblyConfigurator;
        private readonly string _assemblyName;
        private string _version = "latest";
        private bool _cacheEnabled = true;

        internal BeeAssemblyBuilder(string assemblyName)
        {
            _assemblyName = string.IsNullOrWhiteSpace(assemblyName)
                ? throw new ArgumentException(nameof(assemblyName))
                : assemblyName;

            _assemblyConfigurator = new AssemblyConfigurator(_assemblyName);
        }

        /// <summary>
        /// Sets a semantic version token for the generated assembly cache key.
        /// </summary>
        public BeeAssemblyBuilder WithVersion(string version)
        {
            _version = string.IsNullOrWhiteSpace(version)
                ? throw new ArgumentException(nameof(version))
                : version;

            return this;
        }

        /// <summary>
        /// Disables the in-memory cache for this build operation.
        /// </summary>
        public BeeAssemblyBuilder DisableCache()
        {
            _cacheEnabled = false;
            return this;
        }

        /// <summary>
        /// Enables the in-memory cache for this build operation.
        /// </summary>
        public BeeAssemblyBuilder EnableCache()
        {
            _cacheEnabled = true;
            return this;
        }

        /// <summary>
        /// Adds a dynamic class to the assembly using public visibility.
        /// </summary>
        /// <param name="name">Class name.</param>
        /// <param name="configure">Class configuration callback.</param>
        /// <returns>The same assembly builder.</returns>
        public BeeAssemblyBuilder AddClass(string name, Action<BeeClassBuilder> configure)
            => AddClass(name, ClassAccessModifier.Public, configure);

        /// <summary>
        /// Adds a dynamic class to the assembly.
        /// </summary>
        /// <param name="name">Class name.</param>
        /// <param name="accessModifier">Class visibility.</param>
        /// <param name="configure">Class configuration callback.</param>
        /// <returns>The same assembly builder.</returns>
        public BeeAssemblyBuilder AddClass(string name, ClassAccessModifier accessModifier, Action<BeeClassBuilder> configure = null)
        {
            var classConfigurator = new ClassConfigurator(name, accessModifier);
            var classBuilder = new BeeClassBuilder(classConfigurator);
            configure?.Invoke(classBuilder);
            _assemblyConfigurator.AddTypeBuilder(classConfigurator);
            return this;
        }

        /// <summary>
        /// Adds a dynamic interface.
        /// </summary>
        public BeeAssemblyBuilder AddInterface(string name, Action<BeeInterfaceBuilder> configure)
            => AddInterface(name, ClassAccessModifier.Public, configure);

        /// <summary>
        /// Adds a dynamic interface.
        /// </summary>
        public BeeAssemblyBuilder AddInterface(string name, ClassAccessModifier accessModifier, Action<BeeInterfaceBuilder> configure = null)
        {
            var configurator = new InterfaceConfigurator(name, accessModifier);
            var builder = new BeeInterfaceBuilder(configurator);
            configure?.Invoke(builder);
            _assemblyConfigurator.AddTypeBuilder(configurator);
            return this;
        }

        /// <summary>
        /// Adds a dynamic struct.
        /// </summary>
        public BeeAssemblyBuilder AddStruct(string name, Action<BeeStructBuilder> configure)
            => AddStruct(name, ClassAccessModifier.Public, configure);

        /// <summary>
        /// Adds a dynamic struct.
        /// </summary>
        public BeeAssemblyBuilder AddStruct(string name, ClassAccessModifier accessModifier, Action<BeeStructBuilder> configure = null)
        {
            var configurator = new StructConfigurator(name, accessModifier);
            var builder = new BeeStructBuilder(configurator);
            configure?.Invoke(builder);
            _assemblyConfigurator.AddTypeBuilder(configurator);
            return this;
        }

        /// <summary>
        /// Adds a dynamic enum.
        /// </summary>
        public BeeAssemblyBuilder AddEnum<TUnderlying>(string name, Action<BeeEnumBuilder> configure)
            where TUnderlying : struct
            => AddEnum(name, typeof(TUnderlying), ClassAccessModifier.Public, configure);

        /// <summary>
        /// Adds a dynamic enum.
        /// </summary>
        public BeeAssemblyBuilder AddEnum<TUnderlying>(string name, ClassAccessModifier accessModifier, Action<BeeEnumBuilder> configure)
            where TUnderlying : struct
            => AddEnum(name, typeof(TUnderlying), accessModifier, configure);

        /// <summary>
        /// Adds a dynamic enum.
        /// </summary>
        public BeeAssemblyBuilder AddEnum(string name, Type underlyingType, ClassAccessModifier accessModifier, Action<BeeEnumBuilder> configure = null)
        {
            var configurator = new EnumConfigurator(name, underlyingType, accessModifier);
            var builder = new BeeEnumBuilder(configurator);
            configure?.Invoke(builder);
            _assemblyConfigurator.AddTypeBuilder(configurator);
            return this;
        }

        /// <summary>
        /// Adds a dynamic record class.
        /// </summary>
        public BeeAssemblyBuilder AddRecordClass(string name, Action<BeeRecordClassBuilder> configure)
            => AddRecordClass(name, ClassAccessModifier.Public, configure);

        /// <summary>
        /// Adds a dynamic record class.
        /// </summary>
        public BeeAssemblyBuilder AddRecordClass(string name, ClassAccessModifier accessModifier, Action<BeeRecordClassBuilder> configure = null)
        {
            var classConfigurator = new ClassConfigurator(name, accessModifier);
            classConfigurator.AddAttribute(BeeAttribute.Of<RecordLikeAttribute>());
            var classBuilder = new BeeClassBuilder(classConfigurator);
            var recordBuilder = new BeeRecordClassBuilder(classBuilder);
            configure?.Invoke(recordBuilder);
            recordBuilder.FinalizeRecord();
            _assemblyConfigurator.AddTypeBuilder(classConfigurator);
            return this;
        }

        /// <summary>
        /// Adds a dynamic record struct.
        /// </summary>
        public BeeAssemblyBuilder AddRecordStruct(string name, Action<BeeRecordStructBuilder> configure)
            => AddRecordStruct(name, ClassAccessModifier.Public, configure);

        /// <summary>
        /// Adds a dynamic record struct.
        /// </summary>
        public BeeAssemblyBuilder AddRecordStruct(string name, ClassAccessModifier accessModifier, Action<BeeRecordStructBuilder> configure = null)
        {
            var structConfigurator = new StructConfigurator(name, accessModifier);
            structConfigurator.AddAttribute(BeeAttribute.Of<RecordLikeAttribute>());
            var structBuilder = new BeeStructBuilder(structConfigurator);
            var recordBuilder = new BeeRecordStructBuilder(structBuilder);
            configure?.Invoke(recordBuilder);
            recordBuilder.FinalizeRecord();
            _assemblyConfigurator.AddTypeBuilder(structConfigurator);
            return this;
        }

        /// <summary>
        /// Builds all configured types and returns the assembly context.
        /// </summary>
        /// <returns>Built assembly context and generated types.</returns>
        public IAssemblyContext Build()
        {
            if (!_cacheEnabled)
                return _assemblyConfigurator.Configure().Build();

            var key = $"{_assemblyName}::{_version}";
            return Cache.GetOrAdd(key, _ => _assemblyConfigurator.Configure().Build());
        }
    }
}
